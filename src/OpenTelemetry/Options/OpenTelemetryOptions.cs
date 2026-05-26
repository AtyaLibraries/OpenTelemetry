// <copyright file="OpenTelemetryOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Observation.Options;

namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Root options for configuring the Atya telemetry pipeline,
/// including OpenTelemetry SDK setup, instrumentations, and exporters.
/// </summary>
public sealed class OpenTelemetryOptions
{
    /// <summary>
    /// Gets the shared observation identity (service name, version,
    /// activity source name, meter name) used across the Atya
    /// diagnostics layers. Single source of truth for identity.
    /// </summary>
    /// <remarks>
    /// Setting Observation.ConfigureLogging / ConfigureTracing /
    /// ConfigureMetrics directly is IGNORED. The OpenTelemetry-layer
    /// toggles EnableObservationLogging / EnableTracing / EnableMetrics
    /// take precedence and overwrite them at registration time.
    /// </remarks>
    public ObservationOptions Observation { get; } = new();

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
    /// <remarks>
    /// Independent of <see cref="EnableObservationLogging"/>, which only
    /// registers the in-process Atya.Diagnostics.Logging helpers (scope
    /// factories, structured property helpers) and does NOT export logs
    /// anywhere. EnableLogging controls the OpenTelemetry SDK LoggerProvider
    /// pipeline (OTLP / Console export).
    /// </remarks>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Atya Observation logging layer is enabled. Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// Independent of <see cref="EnableLogging"/>, which controls the
    /// OpenTelemetry SDK LoggerProvider pipeline (OTLP / Console export).
    /// EnableObservationLogging only registers the in-process
    /// Atya.Diagnostics.Logging helpers (scope factories, structured property
    /// helpers) and does NOT export logs anywhere.
    /// </remarks>
    public bool EnableObservationLogging { get; set; }

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
