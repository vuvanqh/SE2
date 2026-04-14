using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Domain.Entities;

namespace StudentPlanner.Core.Application.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUsosClient _usosAuthService;
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IFacultyRepository _facultyRepo;
    public AuthenticationService(IIdentityService identityService, IEmailService emailService, IJwtService jwtService,
        IUserRepository userRepo, IRefreshTokenService refreshTokenService, IUsosClient usosAuthService, IFacultyRepository facultyRepo)
    {
        _identityService = identityService;
        _emailService = emailService;
        _jwtService = jwtService;
        _userRepo = userRepo;
        _refreshTokenService = refreshTokenService;
        _usosAuthService = usosAuthService;
        _facultyRepo = facultyRepo;
    }


    public async Task<(LoginResponseDto, RefreshTokenResult)> LoginAsync(LoginRequestDto request)
    {
        var user = await _identityService.SignInAsync(request.Email, request.Password);
        var roles = await _identityService.GetUserRolesAsync(user);
        var role = roles.FirstOrDefault() ?? UserRoleOptions.Student.ToString();

        RefreshTokenResult refreshTokenResult = await _refreshTokenService.IssueOnLogin(user);
        UsosLoginResponse response = await _usosAuthService.LoginAsync(request.Email, request.Password);
        await _identityService.UpdateUsosToken(response.UsosToken, user);
        return (new LoginResponseDto
        {
            Token = _jwtService.CreateToken(user),
            UserRole = role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            FacultyCode = user.Faculty?.FacultyCode,
            FacultyId = user.Faculty?.Id
        }, refreshTokenResult);
    }

    public async Task RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }
        // add USOS
        UsosLoginResponse response = await _usosAuthService.LoginAsync(request.Email, request.Password);
        Faculty? faculty = await _facultyRepo.GetFacultyByUsosIdAsync(response.FacultyId);

        if (faculty == null)
            throw new InvalidOperationException("Student must belog to a certain faculty.");


        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = response.FirstName,
            LastName = response.LastName,
            UsosToken = response.Token,
            Role = UserRoleOptions.Student.ToString()
        };
        await _identityService.RegisterUser(user, request.Password, faculty.Id, UserRoleOptions.Student.ToString());
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        User? user = (await _userRepo.GetUserByEmailAsync(request.Email));

        if (user == null)
        {
            return;
        }

        var token = await _identityService.GeneratePasswordResetTokenAsync(user.Email);
        await _emailService.SendPasswordResetEmailAsync(request.Email, token);

    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        User user = (await _userRepo.GetUserByEmailAsync(request.Email)) ?? throw new InvalidOperationException("User not found.");

        await _identityService.ResetPasswordAsync(user.Email, request.Token, request.NewPassword);
    }

    public async Task<RefreshTokenResponse> RotateRefreshToken(string refreshToken)
    {
        (User user, RefreshTokenResult refreshTokenResult) = await _refreshTokenService.RotateTokenAsync(refreshToken);

        return new RefreshTokenResponse()
        {
            AccessToken = _jwtService.CreateToken(user),
            RefreshToken = refreshTokenResult.RefreshToken,
            ExpirationDate = refreshTokenResult.ExpirationDate
        };
    }
}
