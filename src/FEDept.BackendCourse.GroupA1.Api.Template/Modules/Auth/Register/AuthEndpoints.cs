namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", RegisterHandler.HandleAsync)
            .RequireRateLimiting(AuthRateLimiting.RegisterPolicyName);
    }
}
