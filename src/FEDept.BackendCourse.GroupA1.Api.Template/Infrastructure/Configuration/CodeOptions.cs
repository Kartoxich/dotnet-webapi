using System.Text;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public sealed class CodeOptions
{
    public required int MaxAttempts { get; init; }
    public required int PerMinute { get; init; }
    public required int CodeTtlMinutes { get; init; }
    public required string CodeKey { get; init; }

    public byte[] CodeKeyBytes => Encoding.UTF8.GetBytes(CodeKey);

    public static CodeOptions From(IConfiguration config)
    {
        var o = new CodeOptions
        {
            MaxAttempts = ReadInt(config, "Confirmation:MaxAttempts"),
            PerMinute = ReadInt(config, "Confirmation:PerMinute"),
            CodeTtlMinutes = ReadInt(config, "Confirmation:CodeTtlMinutes"),
            CodeKey = ReadString(config, "Confirmation:CodeKey"),
        };

        if (o.MaxAttempts >= o.PerMinute)
            throw new InvalidOperationException(
                $"Confirmation:MaxAttempts ({o.MaxAttempts}) must be below PerMinute ({o.PerMinute}) " +
                "or the attempt lockout is unreachable behind the rate limiter.");

        if (o.CodeTtlMinutes <= 0)
            throw new InvalidOperationException("Confirmation:CodeTtlMinutes must be positive.");

        return o;
    }

    private static int ReadInt(IConfiguration c, string key) =>
        c.GetValue<int?>(key) ?? throw new InvalidOperationException($"Missing {key}");

    private static string ReadString(IConfiguration c, string key) =>
        c[key] is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException($"Missing {key}");
}
