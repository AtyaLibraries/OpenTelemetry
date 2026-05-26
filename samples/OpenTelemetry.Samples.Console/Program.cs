using Atya.Diagnostics.Metrics.Abstractions;
using Atya.Diagnostics.Metrics.Tags;
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ------------------------------------------------------------------------
// Atya.Diagnostics.OpenTelemetry Sample
//
// Demonstrates the full OpenTelemetry telemetry pipeline:
//   - Resource metadata configuration
//   - Logging + Tracing + Metrics pipelines with console and OTLP export
//   - ASP.NET Core / HttpClient / Runtime instrumentations
//   - Using generic Logging + Tracing + Metrics building blocks inside the pipeline
// ------------------------------------------------------------------------

var builder = Host.CreateApplicationBuilder(args);

// Register console logging so we can see log output.
_ = builder.Logging.ClearProviders();
_ = builder.Logging.AddConsole();

// Register the full telemetry pipeline.
_ = builder.Services.AddAtyaOpenTelemetry(options =>
{
    // Service identity (required).
    options.Observation.ServiceName = "Samples.OrderProcessor";
    options.Observation.ServiceVersion = "1.0.0";

    // Pipeline toggles.
    options.EnableLogging = true;
    options.EnableTracing = true;
    options.EnableMetrics = true;
    options.EnableObservationLogging = true;

    // Resource metadata.
    options.Resource.ServiceNamespace = "samples";
    options.Resource.DeploymentEnvironment = "development";
    options.Resource.Attributes["team"] = "platform";

    // Instrumentations (toggle what you need).
    options.Instrumentations.HttpClient.Enabled = true;
    options.Instrumentations.AspNetCore.Enabled = true;
    options.Instrumentations.Runtime.Enabled = true;

    // Console exporter is handy while developing locally.
    options.Exporters.Console.Enabled = true;

    // OTLP exporter (enable when a collector is available).
    options.Exporters.Otlp.Enabled = false;
    options.Exporters.Otlp.Endpoint = "http://localhost:4317";
});

using var host = builder.Build();
await host.StartAsync();

try
{
    // Resolve the generic building blocks that OpenTelemetry registered for us.
    var logger = host.Services.GetRequiredService<ILogger<OrderProcessor>>();
    var activitySourceAccessor = host.Services.GetRequiredService<IActivitySourceAccessor>();
    var meterAccessor = host.Services.GetRequiredService<IMeterAccessor>();

    var processor = new OrderProcessor(logger, activitySourceAccessor, meterAccessor);

    // Simulate processing some orders.
    processor.ProcessOrder("ORD-001", "tenant-acme");
    processor.ProcessOrder("ORD-002", "tenant-globex");
}
finally
{
    await host.StopAsync();
}

Console.WriteLine();
Console.WriteLine("Sample completed. Console exporter output is written while the host stops.");
Console.WriteLine("logs, traces, and metrics would be exported to your collector.");

// ------------------------------------------------------------------------
// Sample service that uses all three diagnostics building blocks.
// ------------------------------------------------------------------------

internal sealed class OrderProcessor(
    ILogger<OrderProcessor> logger,
    IActivitySourceAccessor activitySource,
    IMeterAccessor meter)
{
    private readonly ILogger<OrderProcessor> _logger = logger;
    private readonly IActivitySourceAccessor _activitySource = activitySource;
    private readonly IMeterAccessor _meter = meter;

    public void ProcessOrder(string orderId, string tenantId)
    {
        // Start a traced activity for this operation.
        using var activity = _activitySource.StartInternalActivity("ProcessOrder");
        _ = (activity?
            .SetOperationName("ProcessOrder")
            .SetCorrelationId(Guid.NewGuid().ToString("N"))
            .SetTenantId(tenantId)
            .SetEntity("Order", orderId));

        _logger.LogInformation("Processing order {OrderId} for tenant {TenantId}", orderId, tenantId);

        // Simulate work.
        Thread.Sleep(50);

        // Record a metric.
        var counter = _meter.CreateCounter<long>("orders.processed", description: "Total orders processed");
        counter.Add(1,
            MetricTags.Operation("ProcessOrder"),
            MetricTags.TenantId(tenantId),
            MetricTags.Outcome("success"));

        var histogram = _meter.CreateHistogram<double>("orders.duration_ms", "ms", "Order processing duration");
        histogram.Record(50.0,
            MetricTags.Operation("ProcessOrder"),
            MetricTags.TenantId(tenantId));

        _ = (activity?.MarkSuccess());

        _logger.LogInformation("Order {OrderId} processed successfully", orderId);
    }
}
