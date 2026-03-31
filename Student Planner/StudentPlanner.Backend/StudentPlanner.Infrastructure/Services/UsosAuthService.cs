using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure.Services.Settings;
namespace StudentPlanner.Infrastructure.Services;
public class UsosAuthService : IUsosAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsosAuthService> _logger;

    public UsosAuthService(
        HttpClient httpClient,
        IOptions<UsosApiSettings> usosApiSettings,
        ILogger<UsosAuthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var settings = usosApiSettings.Value;

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            throw new InvalidOperationException("UsosApi:BaseUrl configuration is missing.");
        }

        _httpClient.BaseAddress = new Uri(settings.BaseUrl);
    } 
     public async Task<bool> LoginAsync(string email, string password)
    {
        var request = new
        {
            student_email = email,
            password = password
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/services/login", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "USOS login failed for email {Email}. Status code: {StatusCode}",
                    email,
                    (int)response.StatusCode);

                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calling USOS login for email {Email}", email);
            return false;
        }
    }
    // private sealed class UsosLoginRequest
    // {
    //     public string student_email { get; set; } = string.Empty;
    //     public string password { get; set; } = string.Empty;

    //     public string StudentEmail
    //     {
    //         get => student_email;
    //         set => student_email = value;
    //     }

    //     public string Password
    //     {
    //         get => password;
    //         set => password = value;
    //     }
    // }

    // private sealed class UsosLoginResponse
    // {
    //     public string Token { get; set; } = string.Empty;
    // }
}