// <copyright file="OtlpExporterConfigurator.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using OpenTelemetry.Exporter;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

/// <summary>
/// Applies <see cref="OtlpOptions"/> configuration to the underlying OpenTelemetry <see cref="OtlpExporterOptions"/>.
/// Shared between logging, tracing, and metrics pipelines.
/// </summary>
internal static class OtlpExporterConfigurator
{
    public static void Apply(OtlpExporterOptions otlp, OtlpOptions options)
    {
        _ = Guard.AgainstNull(otlp);
        _ = Guard.AgainstNull(options);

        if (!string.IsNullOrWhiteSpace(options.Endpoint))
        {
            otlp.Endpoint = new Uri(options.Endpoint);
        }

        if (options.Protocol is { } protocol)
        {
            if (!Enum.IsDefined(typeof(OtlpExportProtocol), protocol))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options),
                    protocol,
                    "OTLP protocol must be a defined OtlpExportProtocol value.");
            }

            otlp.Protocol = protocol;
        }

        if (options.Headers.Count > 0)
        {
            otlp.Headers = string.Join(",", options.Headers.Select(h => $"{h.Key}={h.Value}"));
        }
    }

}
