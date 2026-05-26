// <copyright file="ConsoleOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Options for configuring the console exporter.
/// </summary>
public sealed class ConsoleOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the console exporter is enabled. Default is <c>false</c>.
    /// </summary>
    public bool Enabled { get; set; }
}
