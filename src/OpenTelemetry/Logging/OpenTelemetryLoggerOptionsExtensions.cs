// <copyright file="OpenTelemetryLoggerOptionsExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using OpenTelemetry.Logs;

namespace Atya.Diagnostics.OpenTelemetry.Logging;

/// <summary>
/// Provides fluent extension methods for configuring OpenTelemetry logger options.
/// </summary>
internal static class OpenTelemetryLoggerOptionsExtensions
{
    /// <summary>
    /// Applies Atya log record options to the OpenTelemetry logging provider.
    /// </summary>
    /// <returns>The same <see cref="OpenTelemetryLoggerOptions"/> instance.</returns>
    public static OpenTelemetryLoggerOptions ConfigureAtyaLogging(
        this OpenTelemetryLoggerOptions loggerOptions,
        OpenTelemetryLoggingOptions options)
    {
        _ = Guard.AgainstNull(loggerOptions);
        _ = Guard.AgainstNull(options);

        loggerOptions.IncludeFormattedMessage = options.IncludeFormattedMessage;
        loggerOptions.IncludeScopes = options.IncludeScopes;
        loggerOptions.ParseStateValues = options.ParseStateValues;

        return loggerOptions;
    }
}
