// <copyright file="TracerProviderBuilderExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Internal;
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Atya.Diagnostics.OpenTelemetry.Tracing;

/// <summary>
/// Provides fluent extension methods for configuring the Atya
/// OpenTelemetry tracing pipeline on a <see cref="TracerProviderBuilder"/>.
/// </summary>
internal static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Configures the tracing pipeline with the Atya telemetry
    /// resource, activity source, instrumentations, and exporters.
    /// </summary>
    /// <returns>The same <see cref="TracerProviderBuilder"/> instance.</returns>
    public static TracerProviderBuilder ConfigureAtyaTracing(
        this TracerProviderBuilder builder,
        OpenTelemetryOptions options,
        ResourceBuilder resourceBuilder,
        string activitySourceName)
    {
        _ = Guard.AgainstNull(builder);
        _ = Guard.AgainstNull(options);
        _ = Guard.AgainstNull(resourceBuilder);
        _ = Guard.AgainstNullOrWhiteSpace(activitySourceName);

        _ = builder.SetResourceBuilder(resourceBuilder);
        foreach (var sourceName in TelemetryNameNormalizer.Normalize(activitySourceName, options.ActivitySources))
        {
            _ = builder.AddSource(sourceName);
        }

        _ = builder.AddConfiguredInstrumentations(options.Instrumentations);
        _ = builder.AddConfiguredExporters(options.Exporters);

        return builder;
    }

    private static TracerProviderBuilder AddConfiguredInstrumentations(
        this TracerProviderBuilder builder,
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
            _ = builder.AddSqlClientInstrumentation(sqlClient =>
            {
                if (instrumentations.SqlClient.CaptureSqlText)
                {
                    sqlClient.EnrichWithSqlCommand = DatabaseInstrumentationEnricher.EnrichWithSqlCommandText;
                }
            });
        }

        if (instrumentations.EntityFrameworkCore.Enabled)
        {
            _ = builder.AddEntityFrameworkCoreInstrumentation(entityFrameworkCore =>
            {
                if (instrumentations.EntityFrameworkCore.CaptureSqlText)
                {
                    entityFrameworkCore.EnrichWithIDbCommand = (activity, command) =>
                        DatabaseInstrumentationEnricher.EnrichWithSqlText(activity, command.CommandText);
                }
            });
        }

        if (instrumentations.GrpcClient.Enabled)
        {
            _ = builder.AddGrpcClientInstrumentation();
        }

        return builder;
    }

    private static TracerProviderBuilder AddConfiguredExporters(
        this TracerProviderBuilder builder,
        OpenTelemetryExporterOptions exporters)
    {
        if (exporters.Otlp.Enabled)
        {
            _ = builder.AddOtlpExporter(otlp => OtlpExporterConfigurator.Apply(otlp, exporters.Otlp));
        }

        return builder;
    }
}
