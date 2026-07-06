# Atya.Diagnostics.OpenTelemetry

`Atya.Diagnostics.OpenTelemetry` is the host-facing OpenTelemetry integration package for Atya diagnostics libraries. It wires OpenTelemetry logging, tracing, and metrics, Atya service identity, resource metadata, optional instrumentations, and OTLP export through one dependency-injection entry point.

## Supported Framework

This package intentionally targets `net10.0` only. Consumers must run on .NET 10 or a compatible later runtime selected by the .NET host.

## Installation

```shell
dotnet add package Atya.Diagnostics.OpenTelemetry
```

## Quick Start

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;

services.AddAtyaOpenTelemetry(options =>
{
    options.Observation.ServiceName = "Orders.Service";
    options.Observation.ServiceVersion = "1.0.0";
    options.EnableLogging = true;
    options.ActivitySources.Add("Orders.Workflows");
    options.Meters.Add("Orders.Business");

    options.Resource.ServiceNamespace = "orders";
    options.Resource.DeploymentEnvironment = "production";

    options.Instrumentations.AspNetCore.Enabled = true;
    options.Instrumentations.HttpClient.Enabled = true;
    options.Instrumentations.SqlClient.Enabled = true;
    options.Instrumentations.SqlClient.CaptureSqlText = false;
    options.Instrumentations.Runtime.Enabled = true;

    options.Exporters.Console.Enabled = true;
    options.Exporters.Otlp.Enabled = true;
    options.Exporters.Otlp.Endpoint = "http://otel-collector:4317";
    options.Exporters.Otlp.Protocol = OtlpExportProtocol.Grpc;
});
```

## Configuration Binding

Bind from the default `OpenTelemetry` configuration section:

```json
{
  "OpenTelemetry": {
    "Observation": {
      "ServiceName": "Orders.Service",
      "ServiceVersion": "1.0.0"
    },
    "EnableLogging": true,
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
    "Logging": {
      "IncludeFormattedMessage": true,
      "IncludeScopes": true,
      "ParseStateValues": true
    },
    "Instrumentations": {
      "AspNetCore": { "Enabled": true },
      "HttpClient": { "Enabled": true },
      "SqlClient": {
        "Enabled": true,
        "CaptureSqlText": false
      },
      "Runtime": { "Enabled": true }
    },
    "Exporters": {
      "Console": {
        "Enabled": true
      },
      "Otlp": {
        "Enabled": true,
        "Endpoint": "http://otel-collector:4317",
        "Protocol": "Grpc",
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

- `Observation.ServiceName` is required and is trimmed before registration.
- `Observation.ActivitySourceName` defaults to `Observation.ServiceName` when omitted.
- `Observation.MeterName` defaults to `Observation.ServiceName` when omitted.
- `ActivitySources` adds extra application `ActivitySource` names beyond the package default.
- `Meters` adds extra application `Meter` names beyond the package default.
- Options passed to `AddAtyaOpenTelemetry` are validated immediately because the OpenTelemetry providers are configured during service registration.
- Configure the package through the delegate or configuration section passed to `AddAtyaOpenTelemetry`; later `services.Configure<OpenTelemetryOptions>(...)` calls do not rebuild the OpenTelemetry logging, tracing, or metrics providers.
- Tracing and metrics are enabled by default.
- OpenTelemetry logging is disabled by default. Set `EnableLogging` to `true` to register the OpenTelemetry logging provider and export logs through configured exporters.
- Observation-layer logging is disabled by default.
- `EnableObservationLogging` registers Atya Observation logging services; it does not by itself register the OpenTelemetry logging provider.
- ASP.NET Core, HttpClient, Runtime, console exporter, and OTLP exporter registrations are opt-in.
- SqlClient instrumentation is opt-in.
- Entity Framework Core and gRPC client instrumentation packages are not referenced by this package because the upstream packages are prerelease-only. Applications that need them should reference and register those instrumentation packages directly.
- SQL command text capture is disabled by default because command text can contain sensitive data.
- The package composes `Atya.Diagnostics.Observation`; it does not define business metrics, activity names, or log catalogs.

## Validation and Errors

Options are validated through `Microsoft.Extensions.Options`. Invalid options fail when options are resolved or when host startup validation runs.

- `Observation.ServiceName` cannot be null, empty, or whitespace.
- `ActivitySources` and `Meters` cannot contain null, empty, or whitespace names.
- OTLP `Endpoint`, when set, must be an absolute URI.
- OTLP `Protocol`, when set, must be a defined `OtlpExportProtocol` value such as `Grpc` or `HttpProtobuf`.
- OTLP header names cannot be empty and cannot contain `,` or `=`.
- OTLP header values cannot be null and cannot contain `,`.

## Supported Instrumentations

| Instrumentation | Pipeline | Toggle |
| --------------- | -------- | ------ |
| ASP.NET Core | Tracing and metrics | `Instrumentations.AspNetCore.Enabled` |
| HttpClient | Tracing and metrics | `Instrumentations.HttpClient.Enabled` |
| SqlClient | Tracing and metrics | `Instrumentations.SqlClient.Enabled` |
| .NET Runtime | Metrics | `Instrumentations.Runtime.Enabled` |

SQL command text capture is controlled separately:

| Setting | Effect |
| ------- | ------ |
| `Instrumentations.SqlClient.CaptureSqlText` | Adds SQL command text to database spans for SqlClient as `db.query.text` and `db.statement`. |

Leave SQL text capture disabled unless queries are known not to contain secrets or regulated data and telemetry access is appropriately restricted.

### Optional prerelease instrumentations

Entity Framework Core and gRPC client instrumentation are intentionally left to the application because their OpenTelemetry instrumentation packages are prerelease. Add and register them in the application when that risk is acceptable:

```shell
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore --prerelease
dotnet add package OpenTelemetry.Instrumentation.GrpcNetClient --prerelease
```

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddEntityFrameworkCoreInstrumentation();
        tracing.AddGrpcClientInstrumentation();
    });
```

## Supported Exporters

| Exporter | Toggle | Configuration |
| -------- | ------ | ------------- |
| Console | `Exporters.Console.Enabled` | `Enabled` |
| OTLP | `Exporters.Otlp.Enabled` | `Endpoint`, `Protocol`, `Headers` for enabled logging, tracing, and metrics pipelines |

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

Entity Framework Core and gRPC client instrumentation currently depend on upstream OpenTelemetry prerelease instrumentation packages. Consumers that need those instrumentations should reference and register them in the application until stable upstream packages are available.

## Support and Security

Use the repository issue templates for bug reports and feature requests. Report security issues privately according to the repository `SECURITY.md`; do not disclose suspected vulnerabilities in public issues.

## Migration Notes

This is the initial 1.x package line. Future migrations will be documented in the repository `CHANGELOG.md` with any required code or configuration changes.
