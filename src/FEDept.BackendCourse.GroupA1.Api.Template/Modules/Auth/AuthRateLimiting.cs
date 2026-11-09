using System.Threading.RateLimiting;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;

public static class AuthRateLimiting
{
    public const string RegisterPolicyName = "register";

    public static IServiceCollection AddAuthRateLimiting(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var registerPerMinute = config.GetValue<int>("Registration:PerMinute");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("register", http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = registerPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retry))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retry.TotalSeconds)).ToString();
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { error = "too many registration attempts" }, ct);
            };
        });

        return services;
    }
}
