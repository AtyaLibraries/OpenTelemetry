# Dependency Policy

Runtime package versions are centrally managed in `Directory.Packages.props`.
NuGet lock files are committed so CI and release builds restore the same dependency graph that was reviewed locally.

## Runtime Dependencies

Runtime dependencies must be intentional because they flow to consumers through the NuGet package. Direct runtime references are limited to:

- Atya diagnostics/foundation packages required by the public API and implementation.
- Microsoft.Extensions packages required for dependency injection, configuration binding, options validation, and hosting integration.
- OpenTelemetry SDK, OTLP exporter, hosting, and supported instrumentation packages.

Microsoft.Extensions transitive packages used by the runtime graph are centrally pinned to the repository patch level to avoid drifting between `10.0.x` versions.

Entity Framework Core and gRPC client instrumentation currently use upstream OpenTelemetry prerelease packages. These references are intentional for the stable `1.0.0` package line and must be reviewed when updating OpenTelemetry instrumentation versions. The package suppresses NuGet warning `NU5104` for this documented exception only.

## Build, Test, and Benchmark Dependencies

Analyzers, MinVer, SourceLink, test packages, coverage packages, and BenchmarkDotNet are build/test-only and must not flow into the shipped package. Build-only library dependencies use `PrivateAssets="all"`.

Benchmark-only and test-only transitive packages may lag their latest patch when upstream tools pin them and no vulnerability is present. They are reviewed through Dependabot and the CI vulnerability gate.

## Required Gates

Before release:

```shell
dotnet restore .\OpenTelemetry.sln --verbosity minimal
dotnet list .\OpenTelemetry.sln package --vulnerable --include-transitive
dotnet pack .\src\OpenTelemetry\OpenTelemetry.csproj --configuration Release --no-build --output artifacts\packages --verbosity minimal -p:EnablePackageValidation=true
```

The package must not be published if the vulnerability gate reports vulnerable packages.
