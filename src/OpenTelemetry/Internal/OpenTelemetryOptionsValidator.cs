// <copyright file="OpenTelemetryOptionsValidator.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

internal sealed class OpenTelemetryOptionsValidator : IValidateOptions<OpenTelemetryOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenTelemetryOptions options)
    {
        _ = name;
        _ = Guard.AgainstNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Observation.ServiceName))
        {
            failures.Add("OpenTelemetryOptions.Observation.ServiceName cannot be null or whitespace.");
        }

        AddTelemetryNameFailures(options.ActivitySources, nameof(options.ActivitySources), failures);
        AddTelemetryNameFailures(options.Meters, nameof(options.Meters), failures);

        if (options.Exporters.Otlp.Enabled)
        {
            if (options.Exporters.Otlp.Endpoint is not null &&
                !Uri.TryCreate(options.Exporters.Otlp.Endpoint, UriKind.Absolute, out _))
            {
                failures.Add(
                    $"OpenTelemetryOptions.Exporters.Otlp.Endpoint '{options.Exporters.Otlp.Endpoint}' is not a valid absolute URI.");
            }

            if (options.Exporters.Otlp.Protocol is { } protocol &&
                !Enum.IsDefined(typeof(OtlpExportProtocol), protocol))
            {
                failures.Add("OpenTelemetryOptions.Exporters.Otlp.Protocol must be a defined OtlpExportProtocol value.");
            }

            foreach (var header in options.Exporters.Otlp.Headers)
            {
                if (string.IsNullOrWhiteSpace(header.Key))
                {
                    failures.Add("OpenTelemetryOptions.Exporters.Otlp.Headers cannot contain a null or whitespace header name.");
                }

                if (header.Key.Contains(',', StringComparison.Ordinal) ||
                    header.Key.Contains('=', StringComparison.Ordinal))
                {
                    failures.Add("OpenTelemetryOptions.Exporters.Otlp.Headers header names cannot contain ',' or '='.");
                }

                if (header.Value is null)
                {
                    failures.Add("OpenTelemetryOptions.Exporters.Otlp.Headers cannot contain a null header value.");
                }
                else if (header.Value.Contains(',', StringComparison.Ordinal))
                {
                    failures.Add("OpenTelemetryOptions.Exporters.Otlp.Headers header values cannot contain ','.");
                }
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void AddTelemetryNameFailures(
        IEnumerable<string?> names,
        string optionName,
        List<string> failures)
    {
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                failures.Add($"OpenTelemetryOptions.{optionName} cannot contain a null or whitespace name.");
            }
        }
    }
}
