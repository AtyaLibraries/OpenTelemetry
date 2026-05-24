[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$VersionSuffix,

    [switch]$AllowPackWithoutGit
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$solutionFile = ".\OpenTelemetry.sln"
$packageProject = ".\src\OpenTelemetry\OpenTelemetry.csproj"
$testProject = ".\tests\OpenTelemetry.UnitTests\OpenTelemetry.UnitTests.csproj"
$coverageOutput = Join-Path $root "artifacts\coverage\"

function Invoke-VulnerabilityAudit {
    $audit = dotnet list $solutionFile package --vulnerable --include-transitive
    $auditExitCode = $LASTEXITCODE
    $audit | Write-Host

    if ($auditExitCode -ne 0) {
        exit $auditExitCode
    }

    if ($audit -match "has the following vulnerable packages") {
        throw "Vulnerable packages found."
    }
}

Push-Location $root
try {
    dotnet restore $solutionFile --verbosity minimal
    Invoke-VulnerabilityAudit
    dotnet format $solutionFile --verify-no-changes --verbosity minimal --no-restore
    dotnet build $solutionFile --configuration $Configuration --no-restore --verbosity minimal
    dotnet test $testProject `
        --configuration $Configuration `
        --no-build `
        --logger "trx;LogFileName=test-results.trx" `
        --collect "XPlat Code Coverage" `
        --results-directory .\artifacts\test-results `
        --verbosity minimal `
        /p:CollectCoverage=true `
        "/p:CoverletOutput=$coverageOutput" `
        /p:CoverletOutputFormat=cobertura `
        /p:Threshold=100 `
        /p:ThresholdType=line%2cbranch `
        /p:ThresholdStat=total

    $packArgs = @(
        "pack",
        $packageProject,
        "--configuration",
        $Configuration,
        "--no-build",
        "--output",
        ".\artifacts\packages",
        "--verbosity",
        "minimal",
        "-p:EnablePackageValidation=true"
    )

    if ($VersionSuffix) {
        $packArgs += "/p:VersionSuffix=$VersionSuffix"
    }

    if ($AllowPackWithoutGit) {
        $packArgs += "-p:AllowPackWithoutGit=true"
    }

    dotnet @packArgs
}
finally {
    Pop-Location
}
