namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;

public static class ResendResponses
{
    public static IResult From(ResendResult result) => result.Outcome switch
    {
        ResendOutcome.Accepted when result.Ticket is { } ticket
            => Results.Accepted(value: ConfirmationTicketResponses.From(ticket)),

        ResendOutcome.Accepted
            => Results.Accepted(value: ConfirmationTicketResponses.Neutral()),

        ResendOutcome.MailFailed => MailFailure.Response(),

        ResendOutcome.InvalidRequest when result.Errors is { } errors
            => TypedResults.ValidationProblem(errors),

        _ => throw new ArgumentOutOfRangeException(
            nameof(result), result.Outcome, "Unmapped resend outcome"),
    };
}
