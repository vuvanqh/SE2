using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Authentication;

namespace StudentPlanner.UI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Invalid credentials")]
    public async Task<IActionResult> LogIn(LoginRequestDto loginRequest)
    {
        //_logger.LogInformation("/account/login");
        //_logger.LogDebug($"{loginRequest.Email}");      
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return Ok(new { Message = "User is already signed in.", User = User.Identity.Name });


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


    [AllowAnonymous]
    [HttpPost("refreshToken")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token expired")]
    public async Task<IActionResult> RefreshToken()
    {
        string? token = Request.Cookies["refreshToken"];
        //_logger.LogInformation(token);
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
                Console.WriteLine(ex.Message);
                return Unauthorized(ex.Message);
            }
        }
        return Unauthorized("Session Expired");
    }
}
