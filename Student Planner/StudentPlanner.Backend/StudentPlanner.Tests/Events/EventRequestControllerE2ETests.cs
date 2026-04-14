using Microsoft.AspNetCore.Mvc.Testing;
using StudentPlanner.Backend;
using StudentPlanner.Core.Application.Authentication;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using StudentPlanner.Infrastructure.IdentityEntities;
using StudentPlanner.Infrastructure;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core;
using Microsoft.EntityFrameworkCore;

namespace StudentPlanner.Tests.Events;

public class EventRequestControllerE2ETests : IntegrationTestBase
{
    public EventRequestControllerE2ETests(StudentPlannerWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<string> RegisterAndLoginUserAsync(string email, string password, string role = "Student")
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);

        registerResponse.EnsureSuccessStatusCode();

        if (role != "Student")
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }

            var identityUser = await userManager.FindByEmailAsync(email);
            if (identityUser != null)
            {
                var existingRoles = await userManager.GetRolesAsync(identityUser);
                await userManager.RemoveFromRolesAsync(identityUser, existingRoles);
                await userManager.AddToRoleAsync(identityUser, role);
            }
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);

        loginResponse.EnsureSuccessStatusCode();
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        return loginData!.Token;
    }

    private async Task<AppFaculty> EnsureFacultyExistsAsync(string internalId, string name = "Test Faculty", string code = "TF")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var faculty = await db.Faculties.FirstOrDefaultAsync(f => f.FacultyId == internalId);
        if (faculty == null)
        {
            faculty = new AppFaculty
            {
                Id = Guid.NewGuid(),
                FacultyId = internalId,
                FacultyName = name,
                FacultyCode = code
            };
            db.Faculties.Add(faculty);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return faculty;
    }

    private async Task SeedAcademicEventAsync(Guid id, Guid facultyId, string title = "Existing Event")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.AcademicEvents.Add(new StudentPlanner.Core.Domain.AcademicEvent
        {
            Id = id,
            FacultyId = facultyId,
            EventDetails = new EventDetails
            {
                Title = title,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                Location = "Location"
            }
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<Guid> SeedEventRequestAsync(Guid managerId, Guid facultyId, RequestType type = RequestType.Create, Guid? eventId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var request = new StudentPlanner.Core.Domain.EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = facultyId,
            ManagerId = managerId,
            RequestType = type,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EventId = eventId,
            EventDetails = new EventDetails
            {
                Title = type == RequestType.Create ? "New E2E Event" : "Modified E2E Event",
                Description = "Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Room 101"
            }
        };

        db.EventRequests.Add(request);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return request.Id;
    }

    private async Task<Guid> GetCurrentUserIdAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user!.Id;
    }

    [Fact]
    public async Task ApproveRequest_Create_CreatesAcademicEvent()
    {
        var managerEmail = "manager_create@pw.edu.pl";
        await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var faculty = await EnsureFacultyExistsAsync("FAC_CREATE");
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id, RequestType.Create);

        var adminToken = await RegisterAndLoginUserAsync("admin_create@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/approve/{requestId}", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedRequest = await db.EventRequests.FindAsync(new object[] { requestId }, TestContext.Current.CancellationToken);
        updatedRequest!.Status.Should().Be(RequestStatus.Approved);

        var createdEvent = await db.AcademicEvents.FirstOrDefaultAsync(e => e.EventDetails.Title == "New E2E Event", TestContext.Current.CancellationToken);
        createdEvent.Should().NotBeNull();
        createdEvent!.FacultyId.Should().Be(faculty.Id);
    }

    [Fact]
    public async Task ApproveRequest_Update_UpdatesAcademicEvent()
    {
        var faculty = await EnsureFacultyExistsAsync("FAC_UPDATE");
        var eventId = Guid.NewGuid();
        await SeedAcademicEventAsync(eventId, faculty.Id, "Old Title");

        var managerEmail = "manager_update@pw.edu.pl";
        await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id, RequestType.Update, eventId);

        var adminToken = await RegisterAndLoginUserAsync("admin_update@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/approve/{requestId}", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedEvent = await db.AcademicEvents.FindAsync(new object[] { eventId }, TestContext.Current.CancellationToken);
        updatedEvent!.EventDetails.Title.Should().Be("Modified E2E Event");
    }

    [Fact]
    public async Task ApproveRequest_Delete_DeletesAcademicEvent()
    {
        var faculty = await EnsureFacultyExistsAsync("FAC_DELETE");
        var eventId = Guid.NewGuid();
        await SeedAcademicEventAsync(eventId, faculty.Id);

        var managerEmail = "manager_delete@pw.edu.pl";
        await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id, RequestType.Delete, eventId);

        var adminToken = await RegisterAndLoginUserAsync("admin_delete@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/approve/{requestId}", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var deletedEvent = await db.AcademicEvents.FindAsync(new object[] { eventId }, TestContext.Current.CancellationToken);
        deletedEvent.Should().BeNull();
    }

    [Fact]
    public async Task ApproveRequest_Update_NonExistentEvent_Returns404()
    {
        var faculty = await EnsureFacultyExistsAsync("FAC_UPDATE_404");
        var eventId = Guid.NewGuid(); // Not seeded

        var managerEmail = "manager_update_404@pw.edu.pl";
        await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id, RequestType.Update, eventId);

        var adminToken = await RegisterAndLoginUserAsync("admin_update_404@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/approve/{requestId}", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(TestContext.Current.CancellationToken);
        result.Should().ContainKey("message");
        result!["message"].ToString().Should().Contain("exist");
    }

    [Fact]
    public async Task ApproveRequest_Delete_NonExistentEvent_Returns200()
    {
        var faculty = await EnsureFacultyExistsAsync("FAC_DELETE_404");
        var eventId = Guid.NewGuid(); // not seeded

        var managerEmail = "manager_delete_404@pw.edu.pl";
        await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id, RequestType.Delete, eventId);

        var adminToken = await RegisterAndLoginUserAsync("admin_delete_404@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/approve/{requestId}", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApproveRequest_Unauthorized_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PatchAsync($"/api/event-requests/approve/{Guid.NewGuid()}", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveRequest_Manager_Returns403()
    {
        var managerToken = await RegisterAndLoginUserAsync("manager_no_access@pw.edu.pl", "Password123!", "Manager");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

        var response = await _client.PatchAsync($"/api/event-requests/approve/{Guid.NewGuid()}", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveRequest_NonExistent_Returns404()
    {
        var adminToken = await RegisterAndLoginUserAsync("admin_not_found_approve@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/approve/{Guid.NewGuid()}", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectRequest_Admin_Returns200_AndUpdatesStatus()
    {
        // create a Manager and an Event Request
        var managerEmail = "manager_reject@pw.edu.pl";
        await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var faculty = await EnsureFacultyExistsAsync("FAC_REJECT");
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id);

        // login as Admin
        var adminToken = await RegisterAndLoginUserAsync("admin_reject@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/reject/{requestId}", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedRequest = await db.EventRequests.FindAsync(new object[] { requestId }, TestContext.Current.CancellationToken);
        updatedRequest!.Status.Should().Be(RequestStatus.Rejected);
        updatedRequest.ReviewedByAdminId.Should().NotBeNull();
    }

    [Fact]
    public async Task RejectRequest_Unauthorized_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PatchAsync($"/api/event-requests/reject/{Guid.NewGuid()}", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectRequest_Manager_Returns403()
    {
        var managerToken = await RegisterAndLoginUserAsync("reject_manager_403@pw.edu.pl", "Password123!", "Manager");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

        var response = await _client.PatchAsync($"/api/event-requests/reject/{Guid.NewGuid()}", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectRequest_NonExistent_Returns404()
    {
        var adminToken = await RegisterAndLoginUserAsync("reject_admin_404@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PatchAsync($"/api/event-requests/reject/{Guid.NewGuid()}", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateRequest_Manager_Returns200_AndPersistsPendingRequest()
    {
        var managerToken = await RegisterAndLoginUserAsync("manager_create_req@pw.edu.pl", "Password123!", "Manager");
        var faculty = await EnsureFacultyExistsAsync("FAC_CREATE_REQ");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

        var payload = new
        {
            facultyId = faculty.Id,
            eventId = (Guid?)null,
            requestType = "Create",
            eventDetails = new
            {
                title = "Created From API",
                startTime = DateTime.UtcNow.AddDays(1),
                endTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                location = "Room 101",
                description = "Description"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/event-requests/create", payload, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var saved = await db.EventRequests.FirstOrDefaultAsync(r => r.EventDetails.Title == "Created From API", TestContext.Current.CancellationToken);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(RequestStatus.Pending);
        saved.RequestType.Should().Be(RequestType.Create);
    }

    [Fact]
    public async Task CreateRequest_Student_Returns403()
    {
        var studentToken = await RegisterAndLoginUserAsync("student_create_req@pw.edu.pl", "Password123!", "Student");
        var faculty = await EnsureFacultyExistsAsync("FAC_CREATE_STUDENT");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);

        var payload = new
        {
            facultyId = faculty.Id,
            eventId = (Guid?)null,
            requestType = "Create",
            eventDetails = new
            {
                title = "Student Event",
                startTime = DateTime.UtcNow.AddDays(1),
                endTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                location = "Room 101",
                description = "Description"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/event-requests/create", payload, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyRequests_Manager_ReturnsOnlyOwnRequests()
    {
        var faculty = await EnsureFacultyExistsAsync("FAC_GET_MY");

        var manager1Email = "manager_get_my_1@pw.edu.pl";
        await RegisterAndLoginUserAsync(manager1Email, "Password123!", "Manager");
        var manager1Id = await GetCurrentUserIdAsync(manager1Email);

        var manager2Email = "manager_get_my_2@pw.edu.pl";
        await RegisterAndLoginUserAsync(manager2Email, "Password123!", "Manager");
        var manager2Id = await GetCurrentUserIdAsync(manager2Email);

        await SeedEventRequestAsync(manager1Id, faculty.Id, RequestType.Create);
        await SeedEventRequestAsync(manager2Id, faculty.Id, RequestType.Create);

        var manager1Token = await RegisterAndLoginUserAsync("manager_get_my_login@pw.edu.pl", "Password123!", "Manager");
        var managerLoginId = await GetCurrentUserIdAsync("manager_get_my_login@pw.edu.pl");
        await SeedEventRequestAsync(managerLoginId, faculty.Id, RequestType.Create);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", manager1Token);

        var response = await _client.GetAsync("/api/event-requests", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("New E2E Event");
    }

    [Fact]
    public async Task GetAllRequests_Admin_Returns200()
    {
        var adminToken = await RegisterAndLoginUserAsync("admin_get_all@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.GetAsync("/api/event-requests/all", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllRequests_Manager_Returns403()
    {
        var managerToken = await RegisterAndLoginUserAsync("manager_get_all_403@pw.edu.pl", "Password123!", "Manager");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

        var response = await _client.GetAsync("/api/event-requests/all", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteRequest_Manager_DeletesPendingRequest()
    {
        var managerEmail = "manager_delete_req@pw.edu.pl";
        var managerToken = await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var faculty = await EnsureFacultyExistsAsync("FAC_DELETE_REQ");
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id, RequestType.Create);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

        var response = await _client.DeleteAsync($"/api/event-requests/delete/{requestId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deleted = await db.EventRequests.FindAsync(new object[] { requestId }, TestContext.Current.CancellationToken);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRequest_Manager_ApprovedRequest_Returns400()
    {
        var managerEmail = "manager_delete_approved@pw.edu.pl";
        var managerToken = await RegisterAndLoginUserAsync(managerEmail, "Password123!", "Manager");
        var managerId = await GetCurrentUserIdAsync(managerEmail);
        var faculty = await EnsureFacultyExistsAsync("FAC_DELETE_APPROVED");
        var requestId = await SeedEventRequestAsync(managerId, faculty.Id, RequestType.Create);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = await db.EventRequests.FindAsync(new object[] { requestId }, TestContext.Current.CancellationToken);
            request!.Status = RequestStatus.Approved;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

        var response = await _client.DeleteAsync($"/api/event-requests/delete/{requestId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
