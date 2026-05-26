// <copyright file="OpenTelemetryServiceCollectionExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Internal;
using Atya.Diagnostics.OpenTelemetry.Logging;
using Atya.Diagnostics.OpenTelemetry.Metrics;
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Diagnostics.OpenTelemetry.Tracing;
using Atya.Diagnostics.Observation.Models;
using Atya.Foundation.Guards;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides dependency injection extensions for registering the OpenTelemetry-based telemetry pipeline.
/// </summary>
public static class OpenTelemetryServiceCollectionExtensions
{
    private const string DefaultConfigurationSectionName = "OpenTelemetry";

    /// <summary>
    /// Registers the Atya telemetry pipeline including OpenTelemetry SDK,
    /// resource metadata, instrumentations, and exporters.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate used to configure <see cref="OpenTelemetryOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAtyaOpenTelemetry(
        this IServiceCollection services,
        Action<OpenTelemetryOptions>? configure = null)
    {
        _ = Guard.AgainstNull(services);
        var configureOptions = configure ?? (static (OpenTelemetryOptions _) => { });

        // Bootstrap options eagerly because OpenTelemetry providers are configured during registration.
        var bootstrapOptions = new OpenTelemetryOptions();
        configureOptions(bootstrapOptions);
        ValidateBootstrapOptions(bootstrapOptions);

        // Register and validate options.
        _ = services.AddOptions<OpenTelemetryOptions>()
            .Configure(configureOptions)
            .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<OpenTelemetryOptions>, OpenTelemetryOptionsValidator>());

        var identity = ObservationIdentityResolver.Resolve(bootstrapOptions.Observation);

        // Compose the generic Observation layer (Logging + Tracing + Metrics registration).
        _ = services.AddAtyaObservation(observationOptions =>
        {
            observationOptions.ServiceName = bootstrapOptions.Observation.ServiceName;
            observationOptions.ServiceVersion = bootstrapOptions.Observation.ServiceVersion;
            observationOptions.ActivitySourceName = bootstrapOptions.Observation.ActivitySourceName;
            observationOptions.MeterName = bootstrapOptions.Observation.MeterName;
            observationOptions.ConfigureLogging = bootstrapOptions.EnableObservationLogging;
            observationOptions.ConfigureTracing = bootstrapOptions.EnableTracing;
            observationOptions.ConfigureMetrics = bootstrapOptions.EnableMetrics;
        });

        // Build the OpenTelemetry resource.
        var resourceBuilder = ResourceBuilderFactory.Create(identity, bootstrapOptions.Resource);

        // Delegate pipeline configuration to folder-based configurators.
        var otelBuilder = services.AddOpenTelemetry();

        if (bootstrapOptions.EnableLogging)
        {
            _ = otelBuilder.WithLogging(
                logging => logging.ConfigureAtyaLogging(bootstrapOptions, resourceBuilder),
                loggingOptions => loggingOptions.ConfigureAtyaLogging(bootstrapOptions.Logging));
        }

        if (bootstrapOptions.EnableTracing)
        {
            _ = otelBuilder.WithTracing(tracing =>
                tracing.ConfigureAtyaTracing(bootstrapOptions, resourceBuilder, identity.ActivitySourceName));
        }

        if (bootstrapOptions.EnableMetrics)
        {
            _ = otelBuilder.WithMetrics(metrics =>
                metrics.ConfigureAtyaMetrics(bootstrapOptions, resourceBuilder, identity.MeterName));
        }

        return services;
    }

    /// <summary>
    /// Registers the Atya telemetry pipeline using an application configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <param name="sectionName">The configuration section name. Defaults to <c>OpenTelemetry</c>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAtyaOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = DefaultConfigurationSectionName)
    {
        _ = Guard.AgainstNull(services);
        _ = Guard.AgainstNull(configuration);

        var normalizedSectionName = Guard.AgainstNullOrWhiteSpace(sectionName).Trim();
        var section = configuration.GetSection(normalizedSectionName);

        return services.AddAtyaOpenTelemetry(options => section.Bind(options));
    }

    private static void ValidateBootstrapOptions(OpenTelemetryOptions options)
    {
        var validationResult = new OpenTelemetryOptionsValidator().Validate(string.Empty, options);
        if (validationResult.Succeeded)
        {
            return;
        }

        throw new OptionsValidationException(
            string.Empty,
            typeof(OpenTelemetryOptions),
            validationResult.Failures);
    }
}
