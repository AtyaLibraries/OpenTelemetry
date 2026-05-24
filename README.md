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

This package provides host-facing OpenTelemetry SDK setup, instrumentation registration, resource metadata, and OTLP exporter configuration for Atya diagnostics packages.

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

Production publishing is handled by `.github/workflows/publish-nuget.yml`.
Release versions use stable SemVer such as `v1.0.0`. Publishing is deliberate: the workflow runs only for stable version tags or manual dispatch. It restores, audits, formats, builds, tests with coverage, packs with validation, uploads `.nupkg` and `.snupkg` artifacts, pushes to NuGet, creates the version tag when needed, and creates the GitHub release. EF Core and gRPC OpenTelemetry instrumentations currently remain upstream prerelease dependencies; that dependency exception is intentional for the `1.0.0` package line.
