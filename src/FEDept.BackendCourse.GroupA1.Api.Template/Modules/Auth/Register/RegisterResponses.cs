namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public sealed record RegisterResponse(
    Guid Id,
    string Email,
    string Status);

public static class RegisterResponses
{
    public static IResult From(RegisterResult r) => r.Outcome switch
    {
        RegisterOutcome.Created
            when r.Id is { } id && r.Email is { } email && r.Status is { } status
            => Created(id, email, status),

        RegisterOutcome.Pending when r.Ticket is { } ticket
            => Results.Accepted(value: ConfirmationTicketResponses.From(ticket)),

        RegisterOutcome.MailFailed => MailFailure.Response(),

        RegisterOutcome.InvalidRequest when r.Errors is { } errors
            => TypedResults.ValidationProblem(errors),

        _ => throw new ArgumentOutOfRangeException(nameof(r), r.Outcome, "Unmapped register outcome"),
    };

    private static IResult Created(Guid id, string email, UserStatus status) => Results.Created(
        uri: $"/auth/users/{id}",
        value: new RegisterResponse(id, email, status.ToColumn()));
}
