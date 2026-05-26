// <copyright file="OpenTelemetryLoggingOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Options for configuring OpenTelemetry log records.
/// </summary>
public sealed class OpenTelemetryLoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether formatted log messages are included on exported log records.
    /// Default is <c>true</c>.
    /// </summary>
    public bool IncludeFormattedMessage { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether logging scopes are included on exported log records.
    /// Default is <c>true</c>.
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether structured log state values are parsed into log record attributes.
    /// Default is <c>true</c>.
    /// </summary>
    public bool ParseStateValues { get; set; } = true;
}
