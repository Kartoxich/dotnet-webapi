namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public static class RegisterErrorTypes
{
    private const string Prefix = "urn:fedept:register:";

    public const string EmailTaken = Prefix + "email-taken";
    public const string RegisterRateLimit = Prefix + "register-rate-limit";
}
