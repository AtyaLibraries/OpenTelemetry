// <copyright file="OtlpOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Options for configuring the OTLP exporter.
/// </summary>
public sealed class OtlpOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the OTLP exporter is enabled. Default is <c>false</c>.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the OTLP collector endpoint (e.g. "http://localhost:4317").
    /// When null, the OpenTelemetry SDK default is used.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the OTLP transport protocol. Supported values: "grpc", "http/protobuf".
    /// When null, the OpenTelemetry SDK default is used.
    /// </summary>
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets additional headers to include in OTLP export requests.
    /// </summary>
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
}
