// <copyright file="DatabaseInstrumentationOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Options for database client instrumentation.
/// </summary>
public sealed class DatabaseInstrumentationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether this database instrumentation is enabled. Default is <c>false</c>.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SQL command text is captured on spans. Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// SQL text can contain sensitive data. Enable only after reviewing query contents and telemetry access controls.
    /// </remarks>
    public bool CaptureSqlText { get; set; }
}
