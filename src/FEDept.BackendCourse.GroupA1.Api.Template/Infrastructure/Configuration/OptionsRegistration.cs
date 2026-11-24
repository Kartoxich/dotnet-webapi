namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public static class OptionsRegistration
{
    private const string CodeSection = "Code";
    private const string ResendSection = "Resend";
    private const string SmtpSection = "Smtp";
    private const string SecuritySection = "Security";

    /// <summary>
    /// Binds every strongly typed options object exactly once and fails the
    /// application start — not the first request — when configuration is wrong.
    /// </summary>
    public static IServiceCollection AddAppOptions(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddOptions<CodeOptions>()
            .Bind(config.GetSection(CodeSection))
            .Validate(o => o.MaxAttempts > 0, $"{CodeSection}:MaxAttempts must be positive.")
            .Validate(o => o.PerMinute > 0, $"{CodeSection}:PerMinute must be positive.")
            .Validate(o => o.TtlMinutes > 0, $"{CodeSection}:TtlMinutes must be positive.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), $"{CodeSection}:Key is required.")
            .Validate(o => o.MaxAttempts < o.PerMinute,
                $"{CodeSection}:MaxAttempts must be below {CodeSection}:PerMinute, " +
                "or the attempt lockout is unreachable behind the rate limiter.")
            .ValidateOnStart();

        services.AddOptions<ResendOptions>()
            .Bind(config.GetSection(ResendSection))
            .Validate(o => o.CooldownSeconds >= 1, $"{ResendSection}:CooldownSeconds must be positive.")
            .Validate(o => o.PerMinute > 0, $"{ResendSection}:PerMinute must be positive.")
            .ValidateOnStart();

        services.AddOptions<SmtpOptions>()
            .Bind(config.GetSection(SmtpSection))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host), $"{SmtpSection}:Host is required.")
            .Validate(o => o.Port is > 0 and <= 65535, $"{SmtpSection}:Port must be a valid port.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.From), $"{SmtpSection}:From is required.")
            .Validate(o => Enum.IsDefined(o.Security),
                $"{SmtpSection}:Security must be one of: {string.Join(", ", Enum.GetNames<MailKit.Security.SecureSocketOptions>())}.")
            .ValidateOnStart();

        services.AddOptions<SecurityOptions>()
            .Bind(config.GetSection(SecuritySection))
            .Validate(o => o.WorkFactor is >= 4 and <= 31,
                $"{SecuritySection}:WorkFactor must be between 4 and 31.")
            .ValidateOnStart();

        return services;
    }

    /// <summary>Reads a required scalar option, failing fast when it is missing.</summary>
    public static int RequiredInt(this IConfiguration config, string key) =>
        config.GetValue<int?>(key) ?? throw new InvalidOperationException($"Missing {key}");
}
