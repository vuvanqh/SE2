using System.Threading.Tasks;

namespace StudentPlanner.Core.Application.Authentication;

public interface IAuthenticationService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
}
