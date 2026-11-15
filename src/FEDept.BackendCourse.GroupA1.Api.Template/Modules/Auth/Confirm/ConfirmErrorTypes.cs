namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public static class ConfirmErrorTypes
{
    private const string Prefix = "urn:fedept:confirm:";

    public const string CodeInvalid = Prefix + "code-invalid";
    public const string RateLimit = Prefix + "confirm-rate-limit";
}
