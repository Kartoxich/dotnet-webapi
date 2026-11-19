namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public sealed record ConfirmResponse(string Status);

public static class ConfirmResponses
{
    public static IResult From(ConfirmResult r) => r.Outcome switch
    {
        ConfirmOutcome.Success => TypedResults.Ok(new ConfirmResponse("active")),
        ConfirmOutcome.AttemptsExhausted => ConfirmError.AttemptsExhausted(),
        ConfirmOutcome.InvalidRequest => TypedResults.ValidationProblem(r.Errors!),
        _ => ConfirmError.InvalidCode(),
    };
}
