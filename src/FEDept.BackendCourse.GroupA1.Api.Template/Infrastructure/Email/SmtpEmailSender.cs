using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Email;

public sealed class SmtpEmailSender(
    IOptions<SmtpOptions> options,
    IOptions<EmailOptions> emailOptions) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task SendConfirmationAsync(ConfirmationMessage message, CancellationToken ct)
    {
        var mail = new MimeMessage();
        mail.From.Add(MailboxAddress.Parse(_options.From));
        mail.To.Add(new MailboxAddress("", message.Email));

        (mail.Subject, var body) = Render(message);

        mail.Body = new BodyBuilder { TextBody = body }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, _options.Security, ct);
        await client.SendAsync(mail, ct);
        await client.DisconnectAsync(true, ct);
    }

    private (string Subject, string Body) Render(ConfirmationMessage message) =>
        message.Method switch
        {
            ChallengeMethod.Otp => (
                "Your confirmation code",
                $"Your confirmation code is {message.Secret}.\r\n\r\n" +
                $"It expires in {message.Ttl.TotalMinutes:0} minutes.\r\n\r\n" +
                "If you did not request this, ignore this email."),

            ChallengeMethod.MagicLink => (
                "Confirm your email",
                $"Open this page and confirm your account:\r\n" +
                $"{_emailOptions.ConfirmationLinkBaseUrl}?token={Uri.EscapeDataString(message.Secret)}\r\n\r\n" +
                $"It expires in {message.Ttl.TotalMinutes:0} minutes.\r\n\r\n" +
                "If you did not request this, ignore this email."),

            _ => throw new ArgumentOutOfRangeException(
                nameof(message), message.Method, "Unknown confirmation method"),
        };
}
