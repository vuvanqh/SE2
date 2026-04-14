using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.Exceptions;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Infrastructure.Clients;
using StudentPlanner.Infrastructure.Clients.DTO;
using StudentPlanner.Infrastructure.IdentityEntities;
using StudentPlanner.Infrastructure.Services.Settings;
using System.Net.Http.Json;
namespace StudentPlanner.Infrastructure.Services;

public class UsosClient : IUsosClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsosClient> _logger;

    public UsosClient(HttpClient httpClient, ILogger<UsosClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UsosLoginResponse> LoginAsync(string email, string password)
    {
        var request = new
        {
            student_email = email,
            password = password
        };


        var response = await _httpClient.PostAsJsonAsync("/services/login", request);

        if (!response.IsSuccessStatusCode)
        {

            _logger.LogWarning("USOS login failed for email {Email}. Status code: {StatusCode}",
                email,
                (int)response.StatusCode);

            throw new UsosException($"USOS login failed for email {email}. Status code: {response.StatusCode}");
        }

        UsosLoginResponseDto? resp = await response.Content.ReadFromJsonAsync<UsosLoginResponseDto>();
        if (resp == null)
        {
            _logger.LogCritical("USOS  returned an empty login response for email: {Email}", email);
            throw new InvalidResponseException("USOS returned an empty login response.");
        }

        return resp.ToUsosLoginResponse();
    }

    public async Task<List<Faculty>> GetFacultiesAsync()
    {
        var resp = await _httpClient.GetAsync("/services/faculties");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogCritical("Failed to fetch faculties from UsosAPI with status code {StatusCode}.", resp.StatusCode);
            throw new UsosException($"Failed to fetch faculties from UsosAPI with status code {resp.StatusCode}.");
        }

        List<FacultyResponse>? responses = await resp.Content.ReadFromJsonAsync<List<FacultyResponse>>();

        if (responses == null)
        {
            _logger.LogCritical("Usos returned an empty list of faculties.");
            throw new InvalidResponseException("Usos returned an empty list of faculties.");
        }

        return responses.Select(r => new Faculty()
        {
            FacultyCode = r.faculty_code,
            FacultyId = r.faculty_id,
            FacultyName = r.faculty_name,
            Id = Guid.NewGuid()
        }).ToList();
    }
    public async Task<List<UsosEventResponseDto>> GetTimetableAsync(string usosToken, DateOnly start, int days)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/services/tt/user?start={start:yyyy-MM-dd}&days={days}");

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", usosToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Fetching USOS timetable failed. Status: {StatusCode}", response.StatusCode);
            throw new UsosException($"Fetching timetable failed with status {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<List<UsosEventResponseDto>>();

        if (result == null)
            throw new InvalidResponseException("USOS returned empty timetable response.");

        return result;
    }

}