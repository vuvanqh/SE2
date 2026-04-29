using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.Events.EventPreveiws;
using StudentPlanner.Core;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Entities;
using StudentPlanner.Infrastructure;
using StudentPlanner.Infrastructure.IdentityEntities;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace StudentPlanner.Tests.Events;

public class EventPreviewControllerE2ETests : IntegrationTestBase
{
    public EventPreviewControllerE2ETests(StudentPlannerWebApplicationFactory factory) : base(factory)
    {
    }

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private async Task<string> RegisterAndLoginUserAsync(string email, string password, string role = "Student", Guid? facultyId = null)
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User",
            FacultyId = facultyId
        }, TestContext.Current.CancellationToken);

        if (registerResponse.StatusCode != HttpStatusCode.BadRequest && 
            registerResponse.StatusCode != HttpStatusCode.Conflict && 
            registerResponse.StatusCode != HttpStatusCode.OK && 
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            registerResponse.EnsureSuccessStatusCode();
        }

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<ApplicationRole>>();

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

            if (facultyId != null)
            {
                identityUser.FacultyId = facultyId;
                await userManager.UpdateAsync(identityUser);
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

    [Fact]
    public async Task GetPreviews_Student_Returns200AndData()
    {
        var facultyId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Faculties.Add(new AppFaculty { Id = facultyId, FacultyId = "PREV_FAC", FacultyName = "Preview Faculty", FacultyCode = "PF" });
            
            var eventId = Guid.NewGuid();
            db.AcademicEvents.Add(new FacultyEvent
            {
                Id = eventId,
                FacultyId = facultyId,
                EventDetails = new EventDetails
                {
                    Title = "Preview Academic Event",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                    Location = "Campus"
                }
            });

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Need to subscribe the user to the event for it to show up in previews
        }

        var token = await RegisterAndLoginUserAsync("preview_student@pw.edu.pl", "Password123!", "Student", facultyId);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var identityUser = await userManager.FindByEmailAsync("preview_student@pw.edu.pl");
            var eventId = db.AcademicEvents.First(e => e.EventDetails.Title == "Preview Academic Event").Id;
            
            db.AcademicEventSubscribers.Add(new AcademicEventSubscriber
            {
                AcademicEventId = eventId,
                UserId = identityUser!.Id
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/event-preview?days=7", TestContext.Current.CancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with status {response.StatusCode}. Error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<List<EventPreveiwDto>>(_jsonOptions, TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.Should().Contain(e => e.Title == "Preview Academic Event");
    }

    [Fact]
    public async Task GetPreviews_Admin_ReturnsAllEvents()
    {
        var facultyId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Faculties.Add(new AppFaculty { Id = facultyId, FacultyId = "ADMIN_PREV_FAC", FacultyName = "Admin Preview Faculty", FacultyCode = "APF" });
            
            db.AcademicEvents.Add(new FacultyEvent
            {
                Id = Guid.NewGuid(),
                FacultyId = facultyId,
                EventDetails = new EventDetails
                {
                    Title = "Admin Visible Event",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                    Location = "Campus"
                }
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var token = await RegisterAndLoginUserAsync("preview_admin@pw.edu.pl", "Password123!", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/event-preview?days=7", TestContext.Current.CancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with status {response.StatusCode}. Error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<List<EventPreveiwDto>>(_jsonOptions, TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.Should().Contain(e => e.Title == "Admin Visible Event");
    }

    [Fact]
    public async Task GetPreviews_Unauthorized_Returns401()
    {
        var response = await _client.GetAsync("/api/event-preview", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
