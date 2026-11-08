using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Health;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app, string connectionString)
    {
        app.MapGet("/health", async () =>
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(timeout.Token);

                await using var cmd = new NpgsqlCommand("SELECT 1", conn) { CommandTimeout = 2 };
                await cmd.ExecuteScalarAsync(timeout.Token);

                return Results.Ok(new { status = "ok", db = "up" });
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning("health: db unreachable: {Error}", ex.Message);
                return Results.Problem("database is unavailable",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });

        app.MapGet("/db-info", async () =>
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT version();", conn);
            return Results.Ok(await cmd.ExecuteScalarAsync());
        });
    }
}
