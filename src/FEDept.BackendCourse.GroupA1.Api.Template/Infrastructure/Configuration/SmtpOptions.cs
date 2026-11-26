using MailKit.Security;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public sealed class SmtpOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string From { get; init; }

    public required string LinkBaseUrl { get; init; }

    public SecureSocketOptions Security { get; init; } = SecureSocketOptions.None;
}
