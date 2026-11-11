using System.Threading.RateLimiting;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.AspNetCore.Mvc;

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
                var response = context.HttpContext.Response;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retry))
                {
                    response.Headers.RetryAfter = ((int)Math.Ceiling(retry.TotalSeconds)).ToString();
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Too many registration attemts",
                    Type = RegisterErrorTypes.RegisterRateLimit,
                    Status = StatusCodes.Status429TooManyRequests
                }, ct);
            };
        });

        return services;
    }
}
