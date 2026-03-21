using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Core.Application.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenService _refreshTokenService;
    public AuthenticationService(IIdentityService identityService, IEmailService emailService, IJwtService jwtService,
        IUserRepository userRepo, IRefreshTokenService refreshTokenService)
    {
        _identityService = identityService;
        _emailService = emailService;
        _jwtService = jwtService;
        _userRepo = userRepo;
        _refreshTokenService = refreshTokenService;
    }


    public async Task<(LoginResponseDto, RefreshTokenResult)> LoginAsync(LoginRequestDto request)
    {
        var user = await _identityService.SignInAsync(request.Email, request.Password);
        RefreshTokenResult refreshTokenResult = await _refreshTokenService.IssueOnLogin(user);
        // Placeholder token
        return (new LoginResponseDto
        {
            Token = _jwtService.CreateToken(user),
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        },refreshTokenResult);
    }

    public async Task RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        //TO KATARZYNA: _userManager.AddToRole(user, userroles.user or sth like that)
        await _identityService.RegisterUser(user, request.Password);    
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        User user = (await _userRepo.GetUserByEmailAsync(request.Email)) ?? throw new ApplicationException("Invalid operation.");
      
        var token = await _identityService.GeneratePasswordResetTokenAsync(user.Email);
        await _emailService.SendPasswordResetEmailAsync(request.Email, token);
        
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        User user = (await _userRepo.GetUserByEmailAsync(request.Email))?? throw new ApplicationException("Invalid operation.");
 
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
