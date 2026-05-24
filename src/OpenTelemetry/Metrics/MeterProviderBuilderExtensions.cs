// <copyright file="MeterProviderBuilderExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Internal;
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Atya.Diagnostics.OpenTelemetry.Metrics;

/// <summary>
/// Provides fluent extension methods for configuring the Atya
/// OpenTelemetry metrics pipeline on a <see cref="MeterProviderBuilder"/>.
/// </summary>
internal static class MeterProviderBuilderExtensions
{
    /// <summary>
    /// Configures the metrics pipeline with the Atya telemetry
    /// resource, meter, instrumentations, and exporters.
    /// </summary>
    /// <returns>The same <see cref="MeterProviderBuilder"/> instance.</returns>
    public static MeterProviderBuilder ConfigureAtyaMetrics(
        this MeterProviderBuilder builder,
        OpenTelemetryOptions options,
        ResourceBuilder resourceBuilder,
        string meterName)
    {
        _ = Guard.AgainstNull(builder);
        _ = Guard.AgainstNull(options);
        _ = Guard.AgainstNull(resourceBuilder);
        _ = Guard.AgainstNullOrWhiteSpace(meterName);

        _ = builder.SetResourceBuilder(resourceBuilder);
        foreach (var configuredMeterName in TelemetryNameNormalizer.Normalize(meterName, options.Meters))
        {
            _ = builder.AddMeter(configuredMeterName);
        }

        _ = builder.AddConfiguredInstrumentations(options.Instrumentations);
        _ = builder.AddConfiguredExporters(options.Exporters);

        return builder;
    }

    private static MeterProviderBuilder AddConfiguredInstrumentations(
        this MeterProviderBuilder builder,
        OpenTelemetryInstrumentationOptions instrumentations)
    {
        if (instrumentations.AspNetCore.Enabled)
        {
            _ = builder.AddAspNetCoreInstrumentation();
        }

        if (instrumentations.HttpClient.Enabled)
        {
            _ = builder.AddHttpClientInstrumentation();
        }

        if (instrumentations.SqlClient.Enabled)
        {
            _ = builder.AddSqlClientInstrumentation();
        }

        if (instrumentations.Runtime.Enabled)
        {
            _ = builder.AddRuntimeInstrumentation();
        }

        return builder;
    }

    private static MeterProviderBuilder AddConfiguredExporters(
        this MeterProviderBuilder builder,
        OpenTelemetryExporterOptions exporters)
    {
        if (exporters.Otlp.Enabled)
        {
            _ = builder.AddOtlpExporter(otlp => OtlpExporterConfigurator.Apply(otlp, exporters.Otlp));
        }

        return builder;
    }
}
