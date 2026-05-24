// <copyright file="OpenTelemetryOptionsValidator.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using Microsoft.Extensions.Options;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

internal sealed class OpenTelemetryOptionsValidator : IValidateOptions<OpenTelemetryOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenTelemetryOptions options)
    {
        _ = name;
        _ = Guard.AgainstNull(options);

        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            return ValidateOptionsResult.Fail("OpenTelemetryOptions.ServiceName cannot be null or whitespace.");
        }

        var activitySourceValidation = ValidateTelemetryNames(options.ActivitySources, nameof(options.ActivitySources));
        if (activitySourceValidation is not null)
        {
            return activitySourceValidation;
        }

        var meterValidation = ValidateTelemetryNames(options.Meters, nameof(options.Meters));
        if (meterValidation is not null)
        {
            return meterValidation;
        }

        if (!options.Exporters.Otlp.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (options.Exporters.Otlp.Endpoint is not null &&
            !Uri.TryCreate(options.Exporters.Otlp.Endpoint, UriKind.Absolute, out _))
        {
            return ValidateOptionsResult.Fail(
                $"OpenTelemetryOptions.Exporters.Otlp.Endpoint '{options.Exporters.Otlp.Endpoint}' is not a valid absolute URI.");
        }

        if (!OtlpExporterConfigurator.IsSupportedProtocol(options.Exporters.Otlp.Protocol))
        {
            return ValidateOptionsResult.Fail(
                "OpenTelemetryOptions.Exporters.Otlp.Protocol must be either 'grpc' or 'http/protobuf'.");
        }

        foreach (var header in options.Exporters.Otlp.Headers)
        {
            if (string.IsNullOrWhiteSpace(header.Key))
            {
                return ValidateOptionsResult.Fail("OpenTelemetryOptions.Exporters.Otlp.Headers cannot contain a null or whitespace header name.");
            }

            if (header.Key.Contains(',', StringComparison.Ordinal) ||
                header.Key.Contains('=', StringComparison.Ordinal))
            {
                return ValidateOptionsResult.Fail("OpenTelemetryOptions.Exporters.Otlp.Headers header names cannot contain ',' or '='.");
            }

            if (header.Value is null)
            {
                return ValidateOptionsResult.Fail("OpenTelemetryOptions.Exporters.Otlp.Headers cannot contain a null header value.");
            }

            if (header.Value.Contains(',', StringComparison.Ordinal))
            {
                return ValidateOptionsResult.Fail("OpenTelemetryOptions.Exporters.Otlp.Headers header values cannot contain ','.");
            }
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult? ValidateTelemetryNames(IEnumerable<string?> names, string optionName)
    {
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ValidateOptionsResult.Fail(
                    $"OpenTelemetryOptions.{optionName} cannot contain a null or whitespace name.");
            }
        }

        return null;
    }
}
