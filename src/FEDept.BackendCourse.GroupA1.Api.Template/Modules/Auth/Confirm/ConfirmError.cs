using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public static class ConfirmError
{
    public static IResult InvalidCode() => Results.Problem(
        title: "Code is not valid",
        type: AuthErrorTypes.CodeInvalid,
        statusCode: StatusCodes.Status400BadRequest);

    public static IResult AttemptsExhausted() => Results.Problem(
        title: "Too many attempts",
        type: AuthErrorTypes.AttemptsExhausted,
        statusCode: StatusCodes.Status429TooManyRequests);
}
