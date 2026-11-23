using System.Text;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public sealed class CodeOptions
{
    public required int ResendCooldownSeconds { get; init; }
    public required int ResendPerMinute { get; init; }
    public required int MaxAttempts { get; init; }
    public required int PerMinute { get; init; }
    public required int CodeTtlMinutes { get; init; }
    public required string CodeKey { get; init; }

    public byte[] CodeKeyBytes => Encoding.UTF8.GetBytes(CodeKey);

    public static CodeOptions From(IConfiguration config)
    {
        var o = new CodeOptions
        {
            MaxAttempts = ReadInt(config, "Code:MaxAttempts"),
            PerMinute = ReadInt(config, "Code:PerMinute"),
            CodeTtlMinutes = ReadInt(config, "Code:CodeTtlMinutes"),
            CodeKey = ReadString(config, "Code:CodeKey"),
            ResendCooldownSeconds = ReadInt(config, "Code:ResendCooldownSeconds"),
            ResendPerMinute = ReadInt(config, "Code:ResendPerMinute"),
        };

        if (o.MaxAttempts >= o.PerMinute)
        {
            throw new InvalidOperationException(
                $"Confirmation:MaxAttempts ({o.MaxAttempts}) must be below PerMinute ({o.PerMinute}) " +
                "or the attempt lockout is unreachable behind the rate limiter.");
        }

        if (o.CodeTtlMinutes <= 0)
        {
            throw new InvalidOperationException("Confirmation:CodeTtlMinutes must be positive.");
        }

        if (o.ResendCooldownSeconds < 1)
        {
            throw new InvalidOperationException("Confirmation:ResendCooldownSeconds must be positive.");
        }

        return o;
    }

    private static int ReadInt(IConfiguration c, string key) =>
        c.GetValue<int?>(key) ?? throw new InvalidOperationException($"Missing {key}");

    private static string ReadString(IConfiguration c, string key) =>
        c[key] is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException($"Missing {key}");
}
