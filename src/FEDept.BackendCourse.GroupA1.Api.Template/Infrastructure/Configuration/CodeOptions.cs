using System.Text;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public sealed class CodeOptions
{
    public required int MaxAttempts { get; init; }
    public required int PerMinute { get; init; }
    public required int TtlMinutes { get; init; }
    public required string Key { get; init; }

    public byte[] KeyBytes => Encoding.UTF8.GetBytes(Key);
}
