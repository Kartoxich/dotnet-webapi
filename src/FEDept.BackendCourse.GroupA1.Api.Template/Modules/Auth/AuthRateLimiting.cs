using System.Threading.RateLimiting;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;

public static class AuthRateLimiting
{
    public const string RegisterPolicyName = "register";
    public const string ConfirmPolicyName = "confirm";
    public const string ResendPolicyName = "resend";

    public static IServiceCollection AddAuthRateLimiting(
        this IServiceCollection services,
        CodeOptions codeOptions,
        IConfiguration config
    )
    {
        var registerPerMinute = config.GetValue<int?>("Registration:PerMinute")
            ?? throw new InvalidOperationException("Missing Registration:PerMinute");
        var confirmPerMinute = config.GetValue<int?>("Confirmation:PerMinute")
            ?? throw new InvalidOperationException("Missing Confirmation:PerMinute");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RegisterPolicyName, http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = registerPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy(ConfirmPolicyName, http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = confirmPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy(ResendPolicyName, http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = codeOptions.ResendPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            options.OnRejected = async (context, ct) =>
            {
                var (type, title) = context.HttpContext.Request.Path.Value switch
                {
                    string p when p.StartsWith("/auth/register/confirm") =>
                        (ConfirmErrorTypes.RateLimit, "Too many confirmation attempts"),
                    string p when p.StartsWith("/auth/register/resend") =>
                        (ConfirmErrorTypes.RateLimit, "Too many resend attempts"),
                    _ => (RegisterErrorTypes.RateLimit, "Too many registration attempts")
                };

                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retry))
                    response.Headers.RetryAfter = ((int)Math.Ceiling(retry.TotalSeconds)).ToString();

                await response.WriteAsJsonAsync(new ProblemDetails
                {
                    Type = type, Title = title, Status = StatusCodes.Status429TooManyRequests
                }, ct);
            };
        });

        return services;
    }
}
