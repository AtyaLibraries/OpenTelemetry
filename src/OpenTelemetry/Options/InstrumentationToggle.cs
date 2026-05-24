// <copyright file="InstrumentationToggle.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// A simple toggle for enabling or disabling an instrumentation.
/// </summary>
public sealed class InstrumentationToggle
{
    /// <summary>
    /// Gets or sets a value indicating whether this instrumentation is enabled. Default is <c>false</c>.
    /// </summary>
    public bool Enabled { get; set; }
}
