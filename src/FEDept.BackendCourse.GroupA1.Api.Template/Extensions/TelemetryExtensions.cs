using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Extensions;

public static class TelemetryExtensions
{
    public const string ServiceName = "orders-api";

    public static IServiceCollection AddAppTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: ServiceName,
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString()))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddSource(OrdersTelemetry.ActivitySource.Name)
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(OrdersTelemetry.Meter.Name)
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
