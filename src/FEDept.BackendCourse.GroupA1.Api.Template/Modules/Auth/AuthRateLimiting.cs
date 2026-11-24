using System.Threading.RateLimiting;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;

public static class AuthRateLimiting
{
    public const string RegisterPolicyName = "register";
    public const string ConfirmPolicyName = "confirm";
    public const string ResendPolicyName = "resend";

    public static IServiceCollection AddAuthRateLimiting(
        this IServiceCollection services,
        IConfiguration config)
    {
        var registerPerMinute = config.RequiredInt("Registration:PerMinute");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RegisterPolicyName, http =>
                PerIp(http, registerPerMinute));

            options.AddPolicy(ConfirmPolicyName, http =>
                PerIp(http, http.RequestServices.GetRequiredService<IOptions<CodeOptions>>().Value.PerMinute));

            options.AddPolicy(ResendPolicyName, http =>
                PerIp(http, http.RequestServices.GetRequiredService<IOptions<ResendOptions>>().Value.PerMinute));

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
                {
                    response.Headers.RetryAfter = ((int)Math.Ceiling(retry.TotalSeconds)).ToString();
                }

                await response.WriteAsJsonAsync(new ProblemDetails
                {
                    Type = type, Title = title, Status = StatusCodes.Status429TooManyRequests
                }, ct);
            };
        });

        return services;
    }

    private static RateLimitPartition<string> PerIp(HttpContext http, int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
}
