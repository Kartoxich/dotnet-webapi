using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;

public static class MailFailure
{
    public static IResult Response() => Results.Problem(
        title: "Confirmation email could not be sent",
        detail: "The account is waiting for confirmation. Use the resend endpoint to try the email again.",
        type: AuthErrorTypes.MailNotSent,
        statusCode: StatusCodes.Status503ServiceUnavailable);
}
