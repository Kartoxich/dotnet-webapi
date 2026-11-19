using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", async (RegisterRequest request, RegisterHandler handler, CancellationToken ct) =>
                RegisterResponses.From(await handler.RegisterAsync(request.Email, request.Password, ct)))
            .RequireRateLimiting(AuthRateLimiting.RegisterPolicyName);

        app.MapPost("/auth/register/confirm", async (ConfirmRequest request, ConfirmHandler handler, CancellationToken ct) =>
                ConfirmResponses.From(await handler.ConfirmAsync(request.Email, request.Code, ct)))
            .RequireRateLimiting(AuthRateLimiting.ConfirmPolicyName);
    }
}
