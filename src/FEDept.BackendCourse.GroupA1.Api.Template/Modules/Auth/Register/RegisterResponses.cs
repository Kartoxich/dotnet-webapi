using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public sealed record RegisterPendingResponse(
    bool ConfirmationRequired,
    string Method,
    Guid AttemptId,
    int ExpiresInSeconds,
    int ResendAfterSeconds);

public static class RegisterResponses
{
    public static IResult From(RegisterResult r) => r.Outcome switch
    {
        RegisterOutcome.Success when r.Ticket is { } ticket => Accepted(ticket),
        RegisterOutcome.MailFailed => MailFailure.Response(),
        _ => TypedResults.ValidationProblem(r.Errors!),
    };

    private static IResult Accepted(ConfirmationTicket ticket) => Results.Accepted(
        value: new RegisterPendingResponse(
            ConfirmationRequired: true,
            Method: ticket.Method.ToColumn(),
            AttemptId: ticket.AttemptId,
            ExpiresInSeconds: (int)ticket.Ttl.TotalSeconds,
            ResendAfterSeconds: (int)ticket.Ttl.TotalSeconds));
}
