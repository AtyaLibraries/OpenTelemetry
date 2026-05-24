// <copyright file="OpenTelemetryExporterOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Options for configuring OpenTelemetry exporters.
/// </summary>
public sealed class OpenTelemetryExporterOptions
{
    /// <summary>
    /// Gets OTLP exporter configuration.
    /// </summary>
    public OtlpOptions Otlp { get; } = new();
}
