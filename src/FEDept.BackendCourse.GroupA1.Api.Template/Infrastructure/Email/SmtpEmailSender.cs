using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using MailKit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Email;

public sealed class SmtpEmailSender(IConfiguration config) : IEmailSender
{
    private readonly string _host = config["Smtp:Host"]
        ?? throw new InvalidOperationException("Missing Smtp:Host");
    private readonly int _port = config.GetValue<int?>("Smtp:Port")
        ?? throw new InvalidOperationException("Missing Smtp:Port");
    private readonly string _from = config["Smtp:From"]
        ?? throw new InvalidOperationException("Missing Smtp:From");

    public async Task SendConfirmationAsync(string to, string code, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_from));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = "Your confirmation code";

        var body = new BodyBuilder
        {
            TextBody = $"Your confirmation code is {code}.\r\n\r\n" +
                       "If you did not request this, ignore this email.",
        };
        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.None, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
