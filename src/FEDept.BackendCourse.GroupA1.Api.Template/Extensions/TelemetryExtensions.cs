using System.Reflection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Extensions;

public static class TelemetryExtensions
{
    private static readonly string ServiceName =
        Assembly.GetEntryAssembly()?.GetName().Name ?? "dotnet-webapi";

    public static IServiceCollection AddAppTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: ServiceName,
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString()))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        return services;
    }

    public static ILoggingBuilder AddAppTelemetry(this ILoggingBuilder logging)
    {
        logging.AddOpenTelemetry(loggerOptions =>
        {
            loggerOptions.IncludeFormattedMessage = true;
            loggerOptions.IncludeScopes = true;
            loggerOptions.ParseStateValues = true;
            loggerOptions.AddOtlpExporter();
        });

        return logging;
    }
}
