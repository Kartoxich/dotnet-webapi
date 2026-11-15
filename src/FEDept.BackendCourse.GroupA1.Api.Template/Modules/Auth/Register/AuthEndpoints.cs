using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", RegisterHandler.HandleAsync)
            .RequireRateLimiting(AuthRateLimiting.RegisterPolicyName);

        app.MapPost("/auth/register/confirm", ConfirmHandler.HandleAsync)
            .RequireRateLimiting(AuthRateLimiting.ConfirmPolicyName);
    }
}
