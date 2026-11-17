namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
