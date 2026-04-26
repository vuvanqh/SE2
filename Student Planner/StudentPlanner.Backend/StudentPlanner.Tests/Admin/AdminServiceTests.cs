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
            .Setup(x => x.GetFacultyByIdAsync(request.FacultyId))
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
            .Setup(x => x.GetFacultyByIdAsync(request.FacultyId))
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

        var student = new User
        {
            Id = Guid.NewGuid(),
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
    public async Task GetManagersAsync_ShouldReturnOnlyManagers()
    {
        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            FacultyId = "FacultyId",
            FacultyName = "Electronics",
            FacultyCode = "EL"
        };

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Anna",
                LastName = "Nowak",
                Email = "anna.nowak@pw.edu.pl",
                Role = UserRoleOptions.Manager.ToString(),
                Faculty = faculty
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Piotr",
                LastName = "Kowal",
                Email = "piotr.kowal@pw.edu.pl",
                Role = UserRoleOptions.Manager.ToString(),
                Faculty = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Jan",
                LastName = "Student",
                Email = "jan.student@pw.edu.pl",
                Role = UserRoleOptions.Student.ToString(),
                Faculty = faculty
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Adam",
                LastName = "Admin",
                Email = "adam.admin@pw.edu.pl",
                Role = UserRoleOptions.Admin.ToString(),
                Faculty = faculty
            }
        };

        _identityServiceMock
            .Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);

        var result = await _adminService.GetManagersAsync();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x =>
            x.Email == "anna.nowak@pw.edu.pl" ||
            x.Email == "piotr.kowal@pw.edu.pl");

        result.Should().ContainEquivalentOf(new UsersResultDto
        {
            Id = users[0].Id,
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna.nowak@pw.edu.pl",
            UserRole = UserRoleOptions.Manager.ToString(),
            FacultyCode = "EL"
        });

        result.Should().ContainEquivalentOf(new UsersResultDto
        {
            Id = users[1].Id,
            FirstName = "Piotr",
            LastName = "Kowal",
            Email = "piotr.kowal@pw.edu.pl",
            UserRole = UserRoleOptions.Manager.ToString(),
            FacultyCode = null
        });
    }
    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsersMappedToDto()
    {
        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            FacultyId = "FacultyId",
            FacultyName = "Electronics",
            FacultyCode = "EL"
        };
        Guid AnnaId = Guid.NewGuid();
        Guid JanId = Guid.NewGuid();
        var users = new List<User>
        {
            new()
            {
                Id = AnnaId,
                FirstName = "Anna",
                LastName = "Nowak",
                Email = "anna.nowak@pw.edu.pl",
                Role = UserRoleOptions.Manager.ToString(),
                Faculty = faculty
            },
            new()
            {
                Id = JanId,
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan.kowalski@pw.edu.pl",
                Role = UserRoleOptions.Student.ToString(),
                Faculty = null
            }
        };

        _identityServiceMock
            .Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);

        var result = await _adminService.GetAllUsersAsync();

        result.Should().HaveCount(2);

        result.Should().ContainEquivalentOf(new UsersResultDto
        {
            Id = AnnaId,
            FirstName = "Anna",
            LastName = "Nowak",
            UserRole = UserRoleOptions.Manager.ToString(),
            Email = "anna.nowak@pw.edu.pl",
            FacultyCode = "EL"
        });

        result.Should().ContainEquivalentOf(new UsersResultDto
        {
            Id = JanId,
            FirstName = "Jan",
            LastName = "Kowalski",
            UserRole = UserRoleOptions.Student.ToString(),
            Email = "jan.kowalski@pw.edu.pl",
            FacultyCode = null
        });
    }
}