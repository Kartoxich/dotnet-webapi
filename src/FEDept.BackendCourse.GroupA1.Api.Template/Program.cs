using System.Diagnostics;
using System.Diagnostics.Metrics;
using FEDept.BackendCourse.GroupA1.Api.Template.Extensions;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAppTelemetry();
builder.Logging.AddAppTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var connectionString = builder.Configuration
    .GetConnectionString("Default") ??
    throw new InvalidOperationException("Missing ConnectionStrings:Default");

app.MapGet("/health", async () =>
{
    try
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        return Results.Ok(new { status = "ok", db = "up" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"db unreachable: {ex.Message}");
    }
});


app.MapGet("/db-info", async () =>
{
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand("SELECT version();", conn);
    return Results.Ok(await cmd.ExecuteScalarAsync());
});

app.Run();

public partial class Program;

public static class OrdersTelemetry
{
    public static readonly ActivitySource ActivitySource = new("orders-api");
    public static readonly Meter Meter = new("orders-api");
    public static readonly Counter<long> OrdersCreated =
        Meter.CreateCounter<long>("orders.created");
}
