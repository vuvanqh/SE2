using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using StudentPlanner.Infrastructure.Services.Settings;
using StudentPlanner.Infrastructure.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StudentPlanner.Tests.Authentication;

public class MailtrapEmailServiceTests
{
    private readonly Mock<ISmtpClient> _smtpClientMock;
    private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
    private readonly Mock<ILogger<MailtrapEmailService>> _loggerMock;
    private readonly MailtrapEmailService _emailService;
    private readonly EmailSettings _settings;

    public MailtrapEmailServiceTests()
    {
        _smtpClientMock = new Mock<ISmtpClient>();
        _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
        _loggerMock = new Mock<ILogger<MailtrapEmailService>>();

        _settings = new EmailSettings
        {
            SmtpServer = "smtp.mailtrap.io",
            SmtpPort = 587,
            SmtpUsername = "user",
            SmtpPassword = "pass",
            SenderEmail = "test@studentplanner.com",
            SenderName = "Test Sender"
        };

        _emailSettingsMock.Setup(s => s.Value).Returns(_settings);

        _emailService = new MailtrapEmailService(_emailSettingsMock.Object, _smtpClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_ShouldConnectAuthenticateSendAndDisconnect()
    {
        var email = "recipient@example.com";
        var token = "secret-token";

        await _emailService.SendPasswordResetEmailAsync(email, token);

        _smtpClientMock.Verify(c => c.ConnectAsync(
            _settings.SmtpServer,
            _settings.SmtpPort,
            SecureSocketOptions.StartTls,
            It.IsAny<CancellationToken>()), Times.Once);

        _smtpClientMock.Verify(c => c.AuthenticateAsync(
            _settings.SmtpUsername,
            _settings.SmtpPassword,
            It.IsAny<CancellationToken>()), Times.Once);

        _smtpClientMock.Verify(c => c.SendAsync(
            It.IsAny<MimeMessage>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<ITransferProgress>()), Times.Once);

        _smtpClientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_ShouldThrowInvalidOperationException_WhenSmtpFails()
    {
        var email = "recipient@example.com";
        var token = "secret-token";

        _smtpClientMock.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP connection error"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _emailService.SendPasswordResetEmailAsync(email, token));
        Assert.Contains("Could not send password reset email", exception.Message);
    }
}
