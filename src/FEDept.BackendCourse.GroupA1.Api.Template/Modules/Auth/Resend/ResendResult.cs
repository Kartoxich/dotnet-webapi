using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;

public enum ResendOutcome { Accepted, InvalidRequest, MailFailed }

public sealed record ResendResult(
    ResendOutcome Outcome,
    ConfirmationTicket? Ticket = null,
    IDictionary<string, string[]>? Errors = null)
{
    public static readonly ResendResult Accepted = new(ResendOutcome.Accepted);

    public static ResendResult Issued(ConfirmationTicket ticket)
        => new(ResendOutcome.Accepted, ticket);

    public static readonly ResendResult MailFailed = new(ResendOutcome.MailFailed);
    public static ResendResult InvalidRequest(IDictionary<string, string[]> e)
        => new(ResendOutcome.InvalidRequest, Errors: e);
}
