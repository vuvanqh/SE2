using FluentAssertions;
using Moq;
using StudentPlanner.Core;
using StudentPlanner.Core.Application.Admin.DTO;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Application.ClientContracts;
using Xunit;

namespace StudentPlanner.Tests.Admin;

public class AdminServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IUsosClient> _usosClientMock;
    private readonly Mock<IFacultyRepository> _facultyRepositoryMock;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _usosClientMock = new Mock<IUsosClient>();
        _facultyRepositoryMock = new Mock<IFacultyRepository>();

        _adminService = new AdminService(
            _identityServiceMock.Object,
            _usosClientMock.Object,
            _facultyRepositoryMock.Object);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@pw.edu.pl",
            FirstName = "Jan",
            LastName = "Kowalski",
            Role = UserRoleOptions.Student.ToString()
        };

        _identityServiceMock
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.DeleteUserAsync(userId))
            .Returns(Task.CompletedTask);

        await _adminService.DeleteUserAsync(userId);

        _identityServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _identityServiceMock.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var act = async () => await _adminService.DeleteUserAsync(userId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User is not found");

        _identityServiceMock.Verify(x => x.DeleteUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateManagerAsync_ShouldCreateManager_WhenRequestIsValid()
    {
        Guid FacultyId = Guid.NewGuid();
        var request = new CreateManagerRequestDto
        {
            FirstName = "Anna",
            LastName = "Nowak",
            FacultyId = FacultyId,
            Email = "anna.nowak@pw.edu.pl"
        };

        var faculty = new Faculty
        {
            Id = FacultyId,
            FacultyId = "somestring",
            FacultyName = "Electronics",
            FacultyCode = "EL"
        };

        _facultyRepositoryMock
            .Setup(x => x.GetFacultyByIdAsync(request.FacultyId!.Value))
            .ReturnsAsync(faculty);

        var result = await _adminService.CreateManagerAsync(request);

        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.Role.Should().Be(UserRoleOptions.Manager.ToString());
        result.TemporaryPassword.Should().NotBeNullOrWhiteSpace();
        result.TemporaryPassword.Length.Should().Be(12);
        result.TemporaryPassword.Should().MatchRegex(@"[A-Z]");
        result.TemporaryPassword.Should().MatchRegex(@"[a-z]");
        result.TemporaryPassword.Should().MatchRegex(@"\d");
        result.TemporaryPassword.Should().MatchRegex(@"[!@#$%^&*]");

        _identityServiceMock.Verify(x => x.RegisterUser(
            It.Is<User>(u =>
                u.FirstName == request.FirstName &&
                u.LastName == request.LastName &&
                u.Email == request.Email &&
                u.Role == UserRoleOptions.Manager.ToString() &&
                u.Faculty == faculty &&
                u.UsosToken == null),
            It.Is<string>(pwd =>
                !string.IsNullOrWhiteSpace(pwd) &&
                pwd.Length == 12),
            faculty.Id,
            UserRoleOptions.Manager.ToString()), Times.Once);
    }

    [Fact]
    public async Task CreateManagerAsync_ShouldThrow_WhenFacultyDoesNotExist()
    {
        var request = new CreateManagerRequestDto
        {
            FirstName = "Anna",
            LastName = "Nowak",
            FacultyId = Guid.NewGuid(),
            Email = "anna.nowak@pw.edu.pl"
        };

        _facultyRepositoryMock
            .Setup(x => x.GetFacultyByIdAsync(request.FacultyId!.Value))
            .ReturnsAsync((Faculty?)null);

        var act = async () => await _adminService.CreateManagerAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Faculty not found.");

        _identityServiceMock.Verify(x => x.RegisterUser(
            It.IsAny<User>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<string>()), Times.Never);
    }


    [Fact]
    public async Task SyncUsersWithUsosAsync_ShouldTreatAdminAndManagerAsValidWithoutUsosChecks()
    {

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@pw.edu.pl",
            FirstName = "Admin",
            LastName = "One",
            Role = UserRoleOptions.Admin.ToString()
        };

        var managerUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "manager@pw.edu.pl",
            FirstName = "Manager",
            LastName = "One",
            Role = UserRoleOptions.Manager.ToString()
        };

        _identityServiceMock
            .Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(new List<User> { adminUser, managerUser });

        var result = await _adminService.SyncUsersWithUsosAsync();

        result.CheckedUsers.Should().Be(2);
        result.ValidUsers.Should().Be(2);
        result.DisabledUsers.Should().Be(0);
        result.FailedChecks.Should().Be(0);

        _usosClientMock.Verify(x => x.GetStudentsByFacultyAsync(
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SyncUsersWithUsosAsync_ShouldIncreaseFailedChecks_WhenUsosThrows()
    {
        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            FacultyId = "FacultyId",
            FacultyName = "Electronics",
            FacultyCode = "EL"
        };

        var userId = Guid.NewGuid();

        var student = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "Jan",
            LastName = "Kowalski",
            Role = UserRoleOptions.Student.ToString(),
            UsosToken = "usos-token",
            Faculty = faculty
        };

        _identityServiceMock
            .Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(new List<User> { student });

        _usosClientMock
            .Setup(x => x.GetStudentsByFacultyAsync(student.UsosToken!, faculty.FacultyId.ToString()))
            .ThrowsAsync(new Exception("USOS error"));

        var result = await _adminService.SyncUsersWithUsosAsync();

        result.CheckedUsers.Should().Be(1);
        result.ValidUsers.Should().Be(0);
        result.DisabledUsers.Should().Be(0);
        result.FailedChecks.Should().Be(1);
    }

    [Fact]
    public async Task CreateManagerAsync_ShouldCreateUniversityManager_WhenFacultyIdIsNull()
    {
        var request = new CreateManagerRequestDto
        {
            FirstName = "Piotr",
            LastName = "Kowal",
            FacultyId = null,
            Email = "piotr.kowal@pw.edu.pl"
        };

        var result = await _adminService.CreateManagerAsync(request);

        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.Role.Should().Be(UserRoleOptions.Manager.ToString());

        _identityServiceMock.Verify(x => x.RegisterUser(
            It.Is<User>(u =>
                u.FirstName == request.FirstName &&
                u.LastName == request.LastName &&
                u.Email == request.Email &&
                u.Role == UserRoleOptions.Manager.ToString() &&
                u.Faculty == null &&
                u.UsosToken == null),
            It.IsAny<string>(),
            null,
            UserRoleOptions.Manager.ToString()), Times.Once);
    }
}
