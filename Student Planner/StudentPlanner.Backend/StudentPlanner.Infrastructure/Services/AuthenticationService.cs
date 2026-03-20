using System;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;


using StudentPlanner.Infrastructure.IdentityEntities;
using StudentPlanner.Core.Application.Authentication;

namespace StudentPlanner.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public AuthenticationService(UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }


    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        throw new NotImplementedException();
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        throw new NotImplementedException();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto request)
    {
        throw new NotImplementedException();
    }

    public async Task ResetPasswordAsync(ResetPasswordDto request)
    {
        throw new NotImplementedException();
    }
}
