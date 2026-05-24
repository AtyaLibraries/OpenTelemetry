// <copyright file="OpenTelemetryInstrumentationOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.OpenTelemetry.Options;

/// <summary>
/// Options for toggling OpenTelemetry instrumentations.
/// </summary>
public sealed class OpenTelemetryInstrumentationOptions
{
    /// <summary>
    /// Gets ASP.NET Core request/response instrumentation.
    /// </summary>
    public InstrumentationToggle AspNetCore { get; } = new();

    /// <summary>
    /// Gets HttpClient outgoing request instrumentation.
    /// </summary>
    public InstrumentationToggle HttpClient { get; } = new();

    /// <summary>
    /// Gets SQL Server client instrumentation for Microsoft.Data.SqlClient and System.Data.SqlClient.
    /// </summary>
    public DatabaseInstrumentationOptions SqlClient { get; } = new();

    /// <summary>
    /// Gets Entity Framework Core database command instrumentation.
    /// </summary>
    public DatabaseInstrumentationOptions EntityFrameworkCore { get; } = new();

    /// <summary>
    /// Gets gRPC client instrumentation for Grpc.Net.Client.
    /// </summary>
    public InstrumentationToggle GrpcClient { get; } = new();

    /// <summary>
    /// Gets .NET runtime instrumentation (GC, thread pool, JIT).
    /// </summary>
    public InstrumentationToggle Runtime { get; } = new();
}
