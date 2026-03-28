namespace StudentPlanner.Core.Application.Authentication;

public interface IAuthenticationService
{
    Task RegisterAsync(RegisterRequestDto request);
    Task<(LoginResponseDto, RefreshTokenResult)> LoginAsync(LoginRequestDto request);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<RefreshTokenResponse> RotateRefreshToken(string refreshToken);
}
