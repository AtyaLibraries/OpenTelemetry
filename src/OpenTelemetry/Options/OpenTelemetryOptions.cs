// <copyright file="OpenTelemetryOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Root options for configuring the Atya telemetry pipeline,
/// including OpenTelemetry SDK setup, instrumentations, and exporters.
/// </summary>
public sealed class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets the logical service name used for resource metadata, tracing, and metrics.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service version used for resource metadata and instrument versioning.
    /// </summary>
    public string? ServiceVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the OpenTelemetry tracing pipeline is enabled. Default is <c>true</c>.
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the OpenTelemetry metrics pipeline is enabled. Default is <c>true</c>.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the OpenTelemetry logging pipeline is enabled. Default is <c>false</c>.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Atya Observation logging layer is enabled. Default is <c>false</c>.
    /// </summary>
    public bool EnableObservationLogging { get; set; }

    /// <summary>
    /// Gets or sets the ActivitySource name override. When null, defaults to <see cref="ServiceName"/>.
    /// </summary>
    public string? ActivitySourceName { get; set; }

    /// <summary>
    /// Gets or sets the Meter name override. When null, defaults to <see cref="ServiceName"/>.
    /// </summary>
    public string? MeterName { get; set; }

    /// <summary>
    /// Gets additional application <see cref="System.Diagnostics.ActivitySource"/> names to subscribe to.
    /// </summary>
    public IList<string> ActivitySources { get; } = [];

    /// <summary>
    /// Gets additional application <see cref="System.Diagnostics.Metrics.Meter"/> names to subscribe to.
    /// </summary>
    public IList<string> Meters { get; } = [];

    /// <summary>
    /// Gets resource metadata options for the OpenTelemetry resource builder.
    /// </summary>
    public OpenTelemetryResourceOptions Resource { get; } = new();

    /// <summary>
    /// Gets instrumentation toggle options.
    /// </summary>
    public OpenTelemetryInstrumentationOptions Instrumentations { get; } = new();

    /// <summary>
    /// Gets OpenTelemetry logging options.
    /// </summary>
    public OpenTelemetryLoggingOptions Logging { get; } = new();

    /// <summary>
    /// Gets exporter configuration options.
    /// </summary>
    public OpenTelemetryExporterOptions Exporters { get; } = new();
}
