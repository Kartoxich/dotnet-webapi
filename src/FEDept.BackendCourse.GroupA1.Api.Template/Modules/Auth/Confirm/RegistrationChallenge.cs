namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class RegistrationChallenge
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public byte[] SecretHash { get; set; } = null;
    public string Method { get; set; } = "otp";
    public int Attempts { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset LastSentAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
}
