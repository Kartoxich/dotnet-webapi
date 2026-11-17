namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;

public enum UserStatus
{
    Pending,
    Active
}

public static class UserStatusExtensions
{
    public static string ToColumn(this UserStatus status) => status switch
    {
        UserStatus.Pending => "pending",
        UserStatus.Active => "active",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static UserStatus FromColumn(string s) => s switch
    {
        "pending" => UserStatus.Pending,
        "active" => UserStatus.Active,
        _ => throw new InvalidOperationException($"Unknown user status: {s}"),
    };
}
