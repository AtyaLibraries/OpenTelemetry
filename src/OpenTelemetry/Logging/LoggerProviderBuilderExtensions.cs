// <copyright file="LoggerProviderBuilderExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Internal;
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Atya.Diagnostics.OpenTelemetry.Logging;

/// <summary>
/// Provides fluent extension methods for configuring the Atya
/// OpenTelemetry logging pipeline on a <see cref="LoggerProviderBuilder"/>.
/// </summary>
internal static class LoggerProviderBuilderExtensions
{
    /// <summary>
    /// Configures the logging pipeline with the Atya telemetry
    /// resource and exporters.
    /// </summary>
    /// <returns>The same <see cref="LoggerProviderBuilder"/> instance.</returns>
    public static LoggerProviderBuilder ConfigureAtyaLogging(
        this LoggerProviderBuilder builder,
        OpenTelemetryOptions options,
        ResourceBuilder resourceBuilder)
    {
        _ = Guard.AgainstNull(builder);
        _ = Guard.AgainstNull(options);
        _ = Guard.AgainstNull(resourceBuilder);

        _ = builder.SetResourceBuilder(resourceBuilder);
        _ = builder.AddConfiguredExporters(options.Exporters);

        return builder;
    }

    private static LoggerProviderBuilder AddConfiguredExporters(
        this LoggerProviderBuilder builder,
        OpenTelemetryExporterOptions exporters)
    {
        if (exporters.Console.Enabled)
        {
            _ = builder.AddConsoleExporter();
        }

        if (exporters.Otlp.Enabled)
        {
            _ = builder.AddOtlpExporter(otlp => OtlpExporterConfigurator.Apply(otlp, exporters.Otlp));
        }

        return builder;
    }
}
