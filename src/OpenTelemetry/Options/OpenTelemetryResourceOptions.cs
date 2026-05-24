// <copyright file="OpenTelemetryResourceOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Options for configuring OpenTelemetry resource metadata attributes.
/// </summary>
public sealed class OpenTelemetryResourceOptions
{
    /// <summary>
    /// Gets or sets the optional service namespace (e.g. "orders", "payments").
    /// </summary>
    public string? ServiceNamespace { get; set; }

    /// <summary>
    /// Gets or sets a unique identifier for this service instance (e.g. pod name, container id).
    /// </summary>
    public string? ServiceInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the deployment environment name (e.g. "production", "staging", "development").
    /// </summary>
    public string? DeploymentEnvironment { get; set; }

    /// <summary>
    /// Gets additional resource attributes to include in the OpenTelemetry resource.
    /// </summary>
    public IDictionary<string, object> Attributes { get; } = new Dictionary<string, object>();
}
