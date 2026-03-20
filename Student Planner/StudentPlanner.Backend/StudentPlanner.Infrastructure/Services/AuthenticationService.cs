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
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new LoginResponseDto { Success = false, Message = "Invalid Credentials" };
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return new LoginResponseDto { Success = false, Message = "Invalid Credentials" };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleString = roles.Any() ? string.Join(", ", roles) : "User"; //todo

        // Placeholder token
        return new LoginResponseDto
        {
            Success = true,
            Message = $"Login Successful. Role: {roleString}",
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new RegisterResponseDto { Success = false, Message = "User is already registered." };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new RegisterResponseDto { Success = false, Message = $"Registration failed: {errors}" };
        }

        return new RegisterResponseDto { Success = true, Message = "Registration Successful" };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(request.Email, token);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordDto request)
    {
        throw new NotImplementedException();
    }
}
