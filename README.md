# OpenTelemetry

[![CI](https://github.com/AtyaLibraries/OpenTelemetry/actions/workflows/ci.yml/badge.svg)](https://github.com/AtyaLibraries/OpenTelemetry/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Atya.Diagnostics.OpenTelemetry.svg)](https://www.nuget.org/packages/Atya.Diagnostics.OpenTelemetry)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

OpenTelemetry is the repository for the `Atya.Diagnostics.OpenTelemetry` NuGet package.

| | |
| --- | --- |
| Repository | [https://github.com/AtyaLibraries/OpenTelemetry](https://github.com/AtyaLibraries/OpenTelemetry) |
| NuGet | Atya.Diagnostics.OpenTelemetry |
| License | MIT |
| Target framework | net10.0 |

This package provides host-facing OpenTelemetry SDK setup, stable instrumentation registration, resource metadata, and OTLP exporter configuration for Atya diagnostics packages.

## Layout

```text
.
|-- src/OpenTelemetry/
|-- tests/OpenTelemetry.UnitTests/
|-- samples/OpenTelemetry.Samples.Console/
|-- benchmarks/OpenTelemetry.Benchmarks/
|-- build/
\-- .github/
```

## Build and test

PowerShell:

```powershell
./build/build.ps1 -Configuration Release
./build/pack.ps1 -Configuration Release
```

Bash:

```bash
./build/build.sh
./build/pack.sh
```

The build script runs restore, vulnerability audit, format verification, Release build, and unit tests with coverage thresholds. Restore uses committed NuGet lock files for a reproducible dependency graph. The pack script repeats the release gates and packs with package validation enabled. Artifacts land in `artifacts/packages/`.

Production packing must run from a Git checkout so MinVer and SourceLink can stamp release provenance. Local package smoke tests outside Git can pass `-AllowPackWithoutGit` to `build/pack.ps1` or set `ALLOW_PACK_WITHOUT_GIT=true` for `build/pack.sh`.

## Consumer guidance

Package-specific usage guidance lives in `src/OpenTelemetry/README.md`.

## Maintenance

- Version history: `CHANGELOG.md`
- Security reporting: `SECURITY.md`
- Dependency policy: `DEPENDENCIES.md`
- Dependency updates: Dependabot for NuGet and GitHub Actions
- Vulnerability gate: `dotnet list .\OpenTelemetry.sln package --vulnerable --include-transitive`

## Release

Release versions use stable SemVer such as `v1.0.0`. Releases are published by the central publisher, not by a workflow in this repository. After promoting `development` to `master` and tagging the `master` commit, a maintainer runs `gh workflow run publish.yml --repo AtyaLibraries/publisher -f repository=AtyaLibraries/OpenTelemetry -f ref=<tag>`; see the publisher RUNBOOK (https://github.com/AtyaLibraries/publisher/blob/main/docs/RUNBOOK.md).

EF Core and gRPC OpenTelemetry instrumentation packages are prerelease upstream packages, so this package does not reference or register them. Consumers that need those instrumentations should add and register them in the application:

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
