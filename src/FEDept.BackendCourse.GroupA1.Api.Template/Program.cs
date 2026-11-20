using System.Text.Json;
using System.Text.Json.Serialization;
using FEDept.BackendCourse.GroupA1.Api.Template.Extensions;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Email;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Errors;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Security;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Health;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration
    .GetConnectionString("Default") ??
    throw new InvalidOperationException("Missing ConnectionStrings:Default");

builder.Services.AddOpenApi()
    .AddAppTelemetry()
    .AddProblemDetails()
    .AddExceptionHandler<ConflictExceptionHandler>()
    .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IChallengeRepository, ChallengeRepository>()
    .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>())
    .AddScoped<RegisterHandler>()
    .AddScoped<ConfirmHandler>()
    .AddSingleton<IPasswordHasher, BCryptPasswordHasher>()
    .AddSingleton<IEmailSender, SmtpEmailSender>()
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
