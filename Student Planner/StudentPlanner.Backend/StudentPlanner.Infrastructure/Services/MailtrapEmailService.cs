using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure.Services.Settings;
using System;
using System.Threading.Tasks;

namespace StudentPlanner.Infrastructure.Services;

public class MailtrapEmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ISmtpClient _smtpClient;
    private readonly ILogger<MailtrapEmailService> _logger;

    public MailtrapEmailService(IOptions<EmailSettings> emailSettings, ISmtpClient smtpClient, ILogger<MailtrapEmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _smtpClient = smtpClient;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Reset your password - Student Planner";

        message.Body = new TextPart("plain")
        {
            Text = $"Password reset code: {token}"
        };

        try
        {
            _logger.LogInformation("Attempting to send password reset email to {Email}", email);

            _smtpClient.Timeout = 10000; // 10 seconds

            await _smtpClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await _smtpClient.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await _smtpClient.SendAsync(message);
            await _smtpClient.DisconnectAsync(true);

            _logger.LogInformation("Password reset email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            //rethrow InvalidOperationException
            throw new InvalidOperationException("Could not send password reset email. Please try again later.", ex);
        }
    }
}
