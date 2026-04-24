using Microsoft.Extensions.Options;
using StudentPlanner.Infrastructure.Services.Settings;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentPlanner.Infrastructure.Testing;

public class MailtrapMessage
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("text_body")]
    public string TextBody { get; set; } = string.Empty;

    [JsonPropertyName("html_body")]
    public string HtmlBody { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("txt_path")]
    public string TxtPath { get; set; } = string.Empty;
}

public class MailtrapTestingClient
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;
    private readonly string _baseUrl;

    public MailtrapTestingClient(HttpClient httpClient, IOptions<EmailSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        if (string.IsNullOrEmpty(_settings.ApiToken))
        {
            throw new InvalidOperationException("Mailtrap ApiToken is missing. Check your .env file.");
        }

        var apiBaseUrl = (_httpClient.BaseAddress?.ToString() ?? _settings.ApiBaseUrl).TrimEnd('/');

        if (string.IsNullOrEmpty(_settings.AccountId) || string.IsNullOrEmpty(_settings.InboxId))
        {
            _baseUrl = $"{apiBaseUrl}/api/accounts/0/inboxes/0";
        }
        else
        {
            _baseUrl = $"{apiBaseUrl}/api/accounts/{_settings.AccountId}/inboxes/{_settings.InboxId}";
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("Api-Token"))
        {
            _httpClient.DefaultRequestHeaders.Add("Api-Token", _settings.ApiToken);
        }
    }

    public async Task<List<MailtrapMessage>> GetMessagesAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_settings.InboxId) || _settings.InboxId == "0")
            throw new InvalidOperationException("Mailtrap InboxId is not configured correctly.");

        var url = $"{_baseUrl}/messages";

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Mailtrap API Error: {response.StatusCode} - {error} (URL: {url})");
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<MailtrapMessage>>(content) ?? new List<MailtrapMessage>();
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Mailtrap API Connection Error: {ex.Message} (URL: {url})", ex);
        }
    }

    public async Task<MailtrapMessage?> GetMessageAsync(long messageId, CancellationToken ct = default)
    {
        var url = $"{_baseUrl}/messages/{messageId}";

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Mailtrap API Error fetching message: {response.StatusCode} - {error} (URL: {url})");
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var message = JsonSerializer.Deserialize<MailtrapMessage>(content);

            if (message != null && !string.IsNullOrEmpty(message.TxtPath))
            {
                var bodyResponse = await _httpClient.GetAsync(message.TxtPath, ct);
                if (bodyResponse.IsSuccessStatusCode)
                {
                    message.TextBody = await bodyResponse.Content.ReadAsStringAsync(ct);
                }
            }

            return message;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Mailtrap API Connection Error fetching message {messageId}: {ex.Message} (URL: {url})", ex);
        }
    }
}
