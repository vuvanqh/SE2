using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Authentication;

namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Unified controller for account management and authentication.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user and returns 201 Created on success.
    /// Restricted to @pw.edu.pl email addresses.
    /// </summary>
    /// <param name="registerRequest">The registration data.</param>
    /// <returns>A status code indicating the result of the registration.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Description = "Account with this email already exists")]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Description = "Invalid data format or other error")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
    {
        _logger.LogInformation("/authentication/register");
        _logger.LogDebug($"{registerRequest.Email}");
        try
        {
            await _authenticationService.RegisterAsync(registerRequest);

            // todo: logic for USOS 
            _logger.LogInformation("User {Email} registered successfully. USOS OAuth redirection pending implementation.", registerRequest.Email);

            return StatusCode(StatusCodes.Status201Created);
        }
        catch (ApplicationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
             _logger.LogError(ex, "Error during registration for user {Email} - user already exists", registerRequest.Email);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", registerRequest.Email);
            
            if (ex is ApplicationException)
            {
                return BadRequest(ex.Message);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error during user creation");
        }
    }

    /// <summary>
    /// Authenticates a user and issues a JWT.
    /// </summary>
    /// <param name="loginRequest">The login credentials.</param>
    /// <returns>User data and JWT token on success.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Invalid credentials")] 
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {

        _logger.LogInformation("/authentication/login");
        _logger.LogDebug($"{loginRequest.Email}");

        if (User.Identity != null && User.Identity.IsAuthenticated)
            return Ok("User is already signed in.");

        try
        {
            (LoginResponseDto response, RefreshTokenResult refreshTokenResult) = await _authenticationService.LoginAsync(loginRequest);
            Response.Cookies.Append("refreshToken", refreshTokenResult.RefreshToken, new CookieOptions()
            {
                HttpOnly = true, //prevents js/ts from reading the cookie & protects against xss attacks
                Secure = true, //only sent over https
                SameSite = SameSiteMode.Lax, //blocks csrf attacks
                Expires = refreshTokenResult.ExpirationDate,
                Path = "/api/auth"
            });
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
    

    /// <summary>
    /// Refreshes the access token using the refresh token stored in cookies.
    /// </summary>
    /// <returns>A new access token.</returns>
    [AllowAnonymous]
    [HttpPost("refreshToken")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token expired")]
    public async Task<IActionResult> RefreshToken()
    {
        string? token = Request.Cookies["refreshToken"];
        _logger.LogInformation(token);
        if (token != null)
        {
            try
            {
                RefreshTokenResponse resp = await _authenticationService.RotateRefreshToken(token);

                Response.Cookies.Append("refreshToken", resp.RefreshToken, new CookieOptions()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = resp.ExpirationDate,
                    Path = "/api/auth"
                });

                return Ok(resp.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token rotation.");
                return Unauthorized(ex.Message);
            }
        }
        return Unauthorized("Session Expired");
    }

    /// <summary>
    /// Generates a secure password reset token and sends it via email.
    /// </summary>
    /// <param name="request">The forgot password request.</param>
    /// <returns>200 OK.</returns>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        _logger.LogInformation("/authentication/reset-password");
        _logger.LogDebug($"{request.Email}");
        try
        {
        await _authenticationService.ForgotPasswordAsync(request);
        return Ok();
    }
        catch (ApplicationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Validates the reset token and updates the user's password.
    /// </summary>
    /// <param name="request">The reset password data.</param>
    /// <returns>200 OK or 400 Bad Request.</returns>
    [AllowAnonymous]
    [HttpPost("verify-reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Description = "The reset token is either invalid or expired")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Description = "User not found")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        _logger.LogInformation("/authentication/verify-reset");
        _logger.LogDebug($"{request.Email}");
        try
        {
            await _authenticationService.ResetPasswordAsync(request);
            return Ok();
        }
        catch (ApplicationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
             _logger.LogWarning("Password reset attempt for non-existent email: {Email}", request.Email);
             return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for user {Email}", request.Email);
            return BadRequest("Invalid or expired token.");
        }
    }
}
