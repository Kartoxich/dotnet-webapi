namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;

public sealed record ResendResponse(string Message);

public static class ResendResponses
{
    public static IResult From(ResendResult result) => result.Outcome switch
    {
        ResendOutcome.InvalidRequest => TypedResults.ValidationProblem(result.Errors!),
        ResendOutcome.MailFailed => MailFailure.Response(),
        _ => TypedResults.Accepted((string?)null, new ResendResponse(
            "If your account needs a confirmation code, one may have been sent. " +
            "If you don't see it, try again in a minute."))
    };
}
