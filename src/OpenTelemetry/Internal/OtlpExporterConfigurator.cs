// <copyright file="OtlpExporterConfigurator.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using OpenTelemetry.Exporter;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

/// <summary>
/// Applies <see cref="OtlpOptions"/> configuration to the underlying OpenTelemetry <see cref="OtlpExporterOptions"/>.
/// Shared between tracing and metrics pipelines.
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

        if (!string.IsNullOrWhiteSpace(options.Protocol))
        {
            otlp.Protocol = ParseProtocol(options.Protocol);
        }

        if (options.Headers.Count > 0)
        {
            otlp.Headers = string.Join(",", options.Headers.Select(h => $"{h.Key}={h.Value}"));
        }
    }

    public static bool IsSupportedProtocol(string? protocol)
    {
        if (string.IsNullOrWhiteSpace(protocol))
        {
            return true;
        }

        return protocol.Equals("grpc", StringComparison.OrdinalIgnoreCase) ||
            protocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase);
    }

    private static OtlpExportProtocol ParseProtocol(string protocol)
    {
        var normalizedProtocol = Guard.AgainstNullOrWhiteSpace(protocol).Trim();

        if (normalizedProtocol.Equals("grpc", StringComparison.OrdinalIgnoreCase))
        {
            return OtlpExportProtocol.Grpc;
        }

        if (normalizedProtocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase))
        {
            return OtlpExportProtocol.HttpProtobuf;
        }

        throw new ArgumentException(
            "OTLP protocol must be either 'grpc' or 'http/protobuf'.",
            nameof(protocol));
    }
}
