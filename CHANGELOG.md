# Changelog

All notable changes to `Atya.Diagnostics.OpenTelemetry` are documented here.

This project follows semantic versioning. Release tags use `vMAJOR.MINOR.PATCH`.

## [Unreleased]

- Changed `OtlpOptions.Protocol` from a string to the nullable OpenTelemetry `OtlpExportProtocol` enum.
- Added opt-in console exporter configuration for local tracing and metrics debugging.

## [1.0.0] - 2026-05-24

Initial stable release.

- Hardened production release documentation, local packaging gates, and repository governance files.
- Added public API baseline tracking files for release review.
- Added runtime pipeline integration coverage for hosted OpenTelemetry startup.
- Added SqlClient, Entity Framework Core, and gRPC client instrumentation toggles.
- Added opt-in SQL command text capture for SqlClient and Entity Framework Core spans.
- Added custom `ActivitySources` and `Meters` options for application telemetry.
- Documented the intentional stable `1.0.0` release policy while EF Core and gRPC instrumentation dependencies remain prerelease.
- Adds `AddAtyaOpenTelemetry` dependency injection extensions for code-based and configuration-based setup.
- Adds options for service identity, resource metadata, tracing, metrics, Observation-layer logging, instrumentations, and OTLP export.
- Supports ASP.NET Core, HttpClient, and .NET runtime instrumentations.
- Supports OTLP exporter endpoint, protocol, and header configuration.
- Ships XML documentation, package README, MIT license, SourceLink-ready metadata, and symbol packages.

[Unreleased]: https://github.com/AtyaLibraries/OpenTelemetry/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/AtyaLibraries/OpenTelemetry/releases/tag/v1.0.0
