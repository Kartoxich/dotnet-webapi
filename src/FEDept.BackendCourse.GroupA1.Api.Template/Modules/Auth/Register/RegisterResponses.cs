namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public sealed record RegisterResponse(Guid Id, string Email, UserStatus Status);

public static class RegisterResponses
{
    public static IResult From(RegisterResult r) => r.Outcome switch
    {
        RegisterOutcome.Success => TypedResults.Created(
            (Uri?)null,
            new RegisterResponse(r.Id!.Value, r.Email!, r.Status!.Value)),
        _ => TypedResults.ValidationProblem(r.Errors!),
    };
}
