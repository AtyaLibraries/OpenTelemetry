[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipTests,

    [switch]$SkipFormat,

    [switch]$SkipVulnerabilityAudit
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$solutionFile = ".\OpenTelemetry.sln"
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

    if (-not $SkipVulnerabilityAudit) {
        Invoke-VulnerabilityAudit
    }

    if (-not $SkipFormat) {
        dotnet format $solutionFile --verify-no-changes --verbosity minimal --no-restore
    }

    dotnet build $solutionFile --configuration $Configuration --no-restore --verbosity minimal

    if (-not $SkipTests) {
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
    }
}
finally {
    Pop-Location
}
