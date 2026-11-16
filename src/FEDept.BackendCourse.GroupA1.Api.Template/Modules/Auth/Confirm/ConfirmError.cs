namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public static class ConfirmError
{
    public const string InvalidCodeType = "urn:fedept:confirm:code-invalid";
    public const string AttemptsExhastedType = "urn:fedept:confirm:attempts-exhausted";

    public static IResult InvalidCode() => Results.Problem(
        title: "Code is not valid",
        type: InvalidCodeType,
        statusCode: StatusCodes.Status400BadRequest);

    public static IResult AttemptsExhausted(TimeSpan retryAfter) => Results.Problem(
        title: "Too many attempts",
        type: AttemptsExhastedType,
        statusCode: StatusCodes.Status429TooManyRequests);
}
