namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public sealed class ResendOptions
{
    public required int CooldownSeconds { get; init; }
    public required int PerMinute { get; init; }
}
