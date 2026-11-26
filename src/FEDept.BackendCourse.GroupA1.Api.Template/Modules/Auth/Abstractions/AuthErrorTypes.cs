namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public static class AuthErrorTypes
{
    private const string Prefix = "urn:fedept:auth:";

    public const string EmailTaken = Prefix + "email-taken";
    public const string RegisterRateLimit = Prefix + "register-rate-limit";
    public const string MailNotSent = Prefix + "mail-not-sent";

    public const string CodeInvalid = Prefix + "code-invalid";
    public const string AttemptsExhausted = Prefix + "attempts-exhausted";
    public const string ConfirmRateLimit = Prefix + "confirm-rate-limit";
    public const string ResendRateLimit = Prefix + "resend-rate-limit";
}
