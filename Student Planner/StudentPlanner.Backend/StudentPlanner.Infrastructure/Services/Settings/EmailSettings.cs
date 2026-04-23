namespace StudentPlanner.Infrastructure.Services.Settings;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string InboxId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
}
