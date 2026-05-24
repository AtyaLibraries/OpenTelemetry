# Atya.Diagnostics.OpenTelemetry

`Atya.Diagnostics.OpenTelemetry` is the host-facing OpenTelemetry integration package for Atya diagnostics libraries. It wires OpenTelemetry tracing and metrics, Atya service identity, resource metadata, optional instrumentations, and OTLP export through one dependency-injection entry point.

## Supported Framework

This package intentionally targets `net10.0` only. Consumers must run on .NET 10 or a compatible later runtime selected by the .NET host.

## Installation

```shell
dotnet add package Atya.Diagnostics.OpenTelemetry
```

## Quick Start

```csharp
using Microsoft.Extensions.DependencyInjection;

services.AddAtyaOpenTelemetry(options =>
{
    options.ServiceName = "Orders.Service";
    options.ServiceVersion = "1.0.0";
    options.ActivitySources.Add("Orders.Workflows");
    options.Meters.Add("Orders.Business");

    options.Resource.ServiceNamespace = "orders";
    options.Resource.DeploymentEnvironment = "production";

    options.Instrumentations.AspNetCore.Enabled = true;
    options.Instrumentations.HttpClient.Enabled = true;
    options.Instrumentations.SqlClient.Enabled = true;
    options.Instrumentations.SqlClient.CaptureSqlText = false;
    options.Instrumentations.EntityFrameworkCore.Enabled = true;
    options.Instrumentations.EntityFrameworkCore.CaptureSqlText = false;
    options.Instrumentations.GrpcClient.Enabled = true;
    options.Instrumentations.Runtime.Enabled = true;

    options.Exporters.Otlp.Enabled = true;
    options.Exporters.Otlp.Endpoint = "http://otel-collector:4317";
    options.Exporters.Otlp.Protocol = "grpc";
});
```

## Configuration Binding

Bind from the default `OpenTelemetry` configuration section:

```json
{
  "OpenTelemetry": {
    "ServiceName": "Orders.Service",
    "ServiceVersion": "1.0.0",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableObservationLogging": false,
    "ActivitySources": [ "Orders.Workflows" ],
    "Meters": [ "Orders.Business" ],
    "Resource": {
      "ServiceNamespace": "orders",
      "DeploymentEnvironment": "production",
      "Attributes": {
        "team": "platform"
      }
    },
    "Instrumentations": {
      "AspNetCore": { "Enabled": true },
      "HttpClient": { "Enabled": true },
      "SqlClient": {
        "Enabled": true,
        "CaptureSqlText": false
      },
      "EntityFrameworkCore": {
        "Enabled": true,
        "CaptureSqlText": false
      },
      "GrpcClient": { "Enabled": true },
      "Runtime": { "Enabled": true }
    },
    "Exporters": {
      "Otlp": {
        "Enabled": true,
        "Endpoint": "http://otel-collector:4317",
        "Protocol": "grpc",
        "Headers": {
          "x-service": "orders"
        }
      }
    }
  }
}
```

```csharp
services.AddAtyaOpenTelemetry(configuration);
```

Use a custom section when needed:

```csharp
services.AddAtyaOpenTelemetry(configuration, "Diagnostics:OpenTelemetry");
```

## Behavior

- `ServiceName` is required and is trimmed before registration.
- `ActivitySourceName` defaults to `ServiceName` when omitted.
- `MeterName` defaults to `ServiceName` when omitted.
- `ActivitySources` adds extra application `ActivitySource` names beyond the package default.
- `Meters` adds extra application `Meter` names beyond the package default.
- Options passed to `AddAtyaOpenTelemetry` are validated immediately because the OpenTelemetry providers are configured during service registration.
- Configure the package through the delegate or configuration section passed to `AddAtyaOpenTelemetry`; later `services.Configure<OpenTelemetryOptions>(...)` calls do not rebuild the OpenTelemetry tracing or metrics providers.
- Tracing and metrics are enabled by default.
- Observation-layer logging is disabled by default.
- ASP.NET Core, HttpClient, Runtime, and OTLP exporter registrations are opt-in.
- SqlClient, Entity Framework Core, and gRPC client instrumentations are opt-in.
- SQL command text capture is disabled by default because command text can contain sensitive data.
- The package composes `Atya.Diagnostics.Observation`; it does not define business metrics, activity names, or log catalogs.

## Validation and Errors

Options are validated through `Microsoft.Extensions.Options`. Invalid options fail when options are resolved or when host startup validation runs.

- `ServiceName` cannot be null, empty, or whitespace.
- `ActivitySources` and `Meters` cannot contain null, empty, or whitespace names.
- OTLP `Endpoint`, when set, must be an absolute URI.
- OTLP `Protocol`, when set, must be `grpc` or `http/protobuf`.
- OTLP header names cannot be empty and cannot contain `,` or `=`.
- OTLP header values cannot be null and cannot contain `,`.

## Supported Instrumentations

| Instrumentation | Pipeline | Toggle |
| --------------- | -------- | ------ |
| ASP.NET Core | Tracing and metrics | `Instrumentations.AspNetCore.Enabled` |
| HttpClient | Tracing and metrics | `Instrumentations.HttpClient.Enabled` |
| SqlClient | Tracing and metrics | `Instrumentations.SqlClient.Enabled` |
| Entity Framework Core | Tracing | `Instrumentations.EntityFrameworkCore.Enabled` |
| gRPC client | Tracing | `Instrumentations.GrpcClient.Enabled` |
| .NET Runtime | Metrics | `Instrumentations.Runtime.Enabled` |

SQL command text capture is controlled separately:

| Setting | Effect |
| ------- | ------ |
| `Instrumentations.SqlClient.CaptureSqlText` | Adds SQL command text to database spans for SqlClient as `db.query.text` and `db.statement`. |
| `Instrumentations.EntityFrameworkCore.CaptureSqlText` | Adds EF Core database command text to spans as `db.query.text` and `db.statement`. |

Leave SQL text capture disabled unless queries are known not to contain secrets or regulated data and telemetry access is appropriately restricted.

## Supported Exporters

| Exporter | Toggle | Configuration |
| -------- | ------ | ------------- |
| OTLP | `Exporters.Otlp.Enabled` | `Endpoint`, `Protocol`, `Headers` |

## Package Boundaries

| Package | Responsibility |
| ------- | -------------- |
| `Atya.Diagnostics.Logging` | Generic structured logging conventions and helpers |
| `Atya.Diagnostics.Tracing` | Generic ActivitySource, activity helpers, and trace context |
| `Atya.Diagnostics.Metrics` | Generic Meter, instruments, and metric tags |
| `Atya.Diagnostics.Observation` | Thin composition over Logging, Tracing, and Metrics |
| `Atya.Diagnostics.OpenTelemetry` | OpenTelemetry SDK setup, instrumentations, and exporters |

## Compatibility and Versioning

The package follows semantic versioning. Breaking public API or behavior changes require a major version. Release versions use stable SemVer such as `1.0.0`.

Runtime dependencies are centrally managed by the repository. Consumers should keep their own OpenTelemetry and Microsoft.Extensions package set coherent, especially in applications that already reference OpenTelemetry packages directly.

Entity Framework Core and gRPC client instrumentation currently depend on upstream OpenTelemetry prerelease instrumentation packages. They are included intentionally in the stable `1.0.0` package line and should be reviewed during dependency updates.

## Support and Security

Use the repository issue templates for bug reports and feature requests. Report security issues privately according to the repository `SECURITY.md`; do not disclose suspected vulnerabilities in public issues.

## Migration Notes

This is the initial 1.x package line. Future migrations will be documented in the repository `CHANGELOG.md` with any required code or configuration changes.
