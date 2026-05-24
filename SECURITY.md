# Security Policy

## Supported Versions

Security fixes are provided for the latest stable `Atya.Diagnostics.OpenTelemetry` release.

| Version | Supported |
| ------- | --------- |
| 1.x     | Yes       |

## Reporting a Vulnerability

Do not report suspected vulnerabilities in public issues.

Report vulnerabilities through GitHub private vulnerability reporting for this repository when available, or contact the package owner listed on NuGet. Include:

- Affected package version.
- Minimal reproduction or vulnerable configuration.
- Impact and exploitability notes.
- Any known mitigation or workaround.

The maintainers will acknowledge valid reports, coordinate a fix, and publish a patched NuGet package when needed.

## Supply Chain Notes

Release packages are built by GitHub Actions from protected repository history. Packages include SourceLink-ready repository metadata and `.snupkg` symbols. Consumers should verify package identity as `Atya.Diagnostics.OpenTelemetry`, use stable versions, and run `dotnet list package --vulnerable --include-transitive` in their own CI.
