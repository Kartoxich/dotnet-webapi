namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public sealed class RegistrationOptions
{
    public required int PerMinute { get; init; }
    public required bool RequireConfirmation { get; init; }
}
