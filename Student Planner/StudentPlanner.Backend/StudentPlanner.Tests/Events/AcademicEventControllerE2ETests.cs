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
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Application.AcademicEvents.DTOs;
using StudentPlanner.Core;
using Microsoft.EntityFrameworkCore;

namespace StudentPlanner.Tests.Events;

public class AcademicEventControllerE2ETests : IntegrationTestBase
{
    public AcademicEventControllerE2ETests(StudentPlannerWebApplicationFactory factory) : base(factory)
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
            // assign custom role directly through Identity
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
                // remove default roles and add target role
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

    private async Task SeedEventAsync(Guid id, Guid facultyId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.AcademicEvents.Add(new AcademicEvent
        {
            Id = id,
            FacultyId = facultyId,
            EventDetails = new EventDetails
            {
                Title = "E2E Test Event",
                Description = "A description",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                Location = "Online"
            }
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAccessibleEvents_StudentWithoutFilters_Returns200()
    {
        var token = await RegisterAndLoginUserAsync(
            "student_ok@test.com",
            "Password123!"
        );

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response =
            await _client.GetAsync("/api/academic-events", TestContext.Current.CancellationToken);

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAccessibleEvents_StudentWithFacultyFilter_Returns403()
    {
        var token = await RegisterAndLoginUserAsync(
            "student_filter@test.com",
            "Password123!"
        );

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var facultyId = Guid.NewGuid();

        var response =
            await _client.GetAsync(
                $"/api/academic-events?facultyIds={facultyId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAccessibleEvents_AdminWithFacultyFilter_ReturnsFilteredEvents()
    {
        var faculty1 = Guid.NewGuid();
        var faculty2 = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            db.Faculties.AddRange(
                new AppFaculty
                {
                    Id = faculty1,
                    FacultyId = "F1",
                    FacultyName = "Faculty1",
                    FacultyCode = "F1"
                },
                new AppFaculty
                {
                    Id = faculty2,
                    FacultyId = "F2",
                    FacultyName = "Faculty2",
                    FacultyCode = "F2"
                });

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var event1 = Guid.NewGuid();
        var event2 = Guid.NewGuid();

        await SeedEventAsync(event1, faculty1);
        await SeedEventAsync(event2, faculty2);

        var token = await RegisterAndLoginUserAsync(
            "admin_filter@test.com",
            "Password123!",
            "Admin"
        );

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response =
            await _client.GetAsync(
                $"/api/academic-events?facultyIds={faculty1}", TestContext.Current.CancellationToken);

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var events =
            await response.Content.ReadFromJsonAsync<
                List<AcademicEventResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        events.Should().Contain(e => e.Id == event1);
        events.Should().NotContain(e => e.Id == event2);
    }

    [Fact]
    public async Task GetAccessibleEvents_Admin_Returns200_AndContainsData()
    {
        var eventId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // add a mock faculty so foreign keys don't crash
            db.Faculties.Add(new AppFaculty { Id = facultyId, FacultyId = "E2E_FAC", FacultyName = "Test Faculty", FacultyCode = "TF" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        await SeedEventAsync(eventId, facultyId);

        // get admin token
        var token = await RegisterAndLoginUserAsync("admin_fetch@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/academic-events", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedEvents = await response.Content.ReadFromJsonAsync<List<AcademicEventResponse>>(TestContext.Current.CancellationToken);

        returnedEvents.Should().NotBeNull();
        returnedEvents.Should().NotBeEmpty();
        returnedEvents.Should().Contain(e => e.Id == eventId && e.Title == "E2E Test Event");
    }

    [Fact]
    public async Task GetAccessibleEvents_Student_Returns403()
    {
        var token = await RegisterAndLoginUserAsync("student_no_access@pw.edu.pl", "Password123!", "Student");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/academic-events", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAccessibleEvents_Unauthorized_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/academic-events", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEventDetails_NonExistent_Returns404()
    {
        var token = await RegisterAndLoginUserAsync("non_existent_student@pw.edu.pl", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/academic-events/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEventDetails_Authorized_Returns200_AndContainsData()
    {
        var token = await RegisterAndLoginUserAsync("details_student@pw.edu.pl", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var eventId = Guid.NewGuid();

        var faculty = await EnsureFacultyExistsAsync("test-faculty");
        await SeedEventAsync(eventId, faculty.Id);

        var response200 = await _client.GetAsync($"/api/academic-events/{eventId}", TestContext.Current.CancellationToken);

        response200.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedEvent = await response200.Content.ReadFromJsonAsync<AcademicEventResponse>(TestContext.Current.CancellationToken);
        returnedEvent.Should().NotBeNull();
        returnedEvent!.Id.Should().Be(eventId);
        returnedEvent.Title.Should().Be("E2E Test Event");
    }

    [Fact]
    public async Task GetEventDetails_Unauthorized_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/academic-events/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEventDetails_CrossFacultyPeeping_Returns404()
    {
        var token = await RegisterAndLoginUserAsync("snooper@pw.edu.pl", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var foreignEventId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // add a different foreign faculty
            var foreignFaculty = new AppFaculty { Id = Guid.NewGuid(), FacultyId = "FOREIGN_FAC2", FacultyName = "Other", FacultyCode = "OF2" };
            db.Faculties.Add(foreignFaculty);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            // seed an event in the foreign faculty
            await SeedEventAsync(foreignEventId, foreignFaculty.Id);
        }
        var response = await _client.GetAsync($"/api/academic-events/{foreignEventId}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Subscribe_AuthorizedUser_Returns204_AndCreatesSubscription()
    {
        var token = await RegisterAndLoginUserAsync("subscribe_student@pw.edu.pl", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventId = Guid.NewGuid();
        var faculty = await EnsureFacultyExistsAsync("test-faculty");
        await SeedEventAsync(eventId, faculty.Id);

        var response = await _client.PutAsync($"/api/academic-events/{eventId}/subscribe", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync("subscribe_student@pw.edu.pl");

        var subscriptionExists = await db.AcademicEventSubscribers
            .AnyAsync(s => s.AcademicEventId == eventId && s.UserId == user!.Id, TestContext.Current.CancellationToken);

        subscriptionExists.Should().BeTrue();
    }

    [Fact]
    public async Task Subscribe_Unauthorized_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PutAsync($"/api/academic-events/{Guid.NewGuid()}/subscribe", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Subscribe_NonExistentEvent_Returns404()
    {
        var token = await RegisterAndLoginUserAsync("subscribe_missing_event@pw.edu.pl", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsync($"/api/academic-events/{Guid.NewGuid()}/subscribe", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unsubscribe_AuthorizedUser_Returns204_AndRemovesSubscription()
    {
        var token = await RegisterAndLoginUserAsync("unsubscribe_student@pw.edu.pl", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventId = Guid.NewGuid();
        var faculty = await EnsureFacultyExistsAsync("test-faculty");
        await SeedEventAsync(eventId, faculty.Id);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("unsubscribe_student@pw.edu.pl");

            db.AcademicEventSubscribers.Add(new AcademicEventSubscriber
            {
                AcademicEventId = eventId,
                UserId = user!.Id
            });

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var response = await _client.PutAsync($"/api/academic-events/{eventId}/unsubscribe", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var subscriptionExists = await verifyDb.AcademicEventSubscribers
            .AnyAsync(s => s.AcademicEventId == eventId, TestContext.Current.CancellationToken);

        subscriptionExists.Should().BeFalse();
    }

    [Fact]
    public async Task Unsubscribe_WhenSubscriptionDoesNotExist_Returns404()
    {
        var token = await RegisterAndLoginUserAsync("unsubscribe_missing_subscription@pw.edu.pl", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var eventId = Guid.NewGuid();
        var faculty = await EnsureFacultyExistsAsync("test-faculty");
        await SeedEventAsync(eventId, faculty.Id);

        var response = await _client.PutAsync($"/api/academic-events/{eventId}/unsubscribe", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
