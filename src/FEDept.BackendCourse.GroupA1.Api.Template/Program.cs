using System.Text.Json;
using System.Text.Json.Serialization;
using FEDept.BackendCourse.GroupA1.Api.Template.Extensions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Health;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration
    .GetConnectionString("Default") ??
    throw new InvalidOperationException("Missing ConnectionStrings:Default");

builder.Services.AddOpenApi()
    .AddAppTelemetry()
    .AddProblemDetails()
    .AddSingleton(new UserRepository(connectionString))
    .AddAuthRateLimiting(builder.Configuration)
    .ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
builder.Logging.AddAppTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRateLimiter();

app.MapHealthEndpoints(connectionString);
app.MapAuthEndpoints();

app.Run();

public partial class Program;
