namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public sealed record ConfirmResponse(string Status);

public static class ConfirmResponses
{
    public static IResult From(ConfirmResult r) => r.Outcome switch
    {
        ConfirmOutcome.Success => TypedResults.Ok(new ConfirmResponse("active")),
        ConfirmOutcome.AttemptsExhausted => ConfirmError.AttemptsExhausted(),
        ConfirmOutcome.InvalidCode => ConfirmError.InvalidCode(),

        ConfirmOutcome.InvalidRequest when r.Errors is { } errors
            => TypedResults.ValidationProblem(errors),

        _ => throw new ArgumentOutOfRangeException(nameof(r), r.Outcome, "Unmapped confirm outcome"),
    };
}
