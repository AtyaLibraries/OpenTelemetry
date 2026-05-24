#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${CONFIGURATION:-Release}"
SKIP_TESTS="${SKIP_TESTS:-false}"
SKIP_FORMAT="${SKIP_FORMAT:-false}"
SKIP_VULNERABILITY_AUDIT="${SKIP_VULNERABILITY_AUDIT:-false}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
SOLUTION_FILE="./OpenTelemetry.sln"
TEST_PROJECT="./tests/OpenTelemetry.UnitTests/OpenTelemetry.UnitTests.csproj"
COVERAGE_OUTPUT="${ROOT_DIR}/artifacts/coverage/"

cd "$ROOT_DIR"

run_vulnerability_audit() {
  local audit
  audit="$(dotnet list "$SOLUTION_FILE" package --vulnerable --include-transitive)"
  printf '%s\n' "$audit"

  if [[ "$audit" == *"has the following vulnerable packages"* ]]; then
    printf '%s\n' "Vulnerable packages found." >&2
    exit 1
  fi
}

dotnet restore "$SOLUTION_FILE" --verbosity minimal

if [ "$SKIP_VULNERABILITY_AUDIT" != "true" ]; then
  run_vulnerability_audit
fi

if [ "$SKIP_FORMAT" != "true" ]; then
  dotnet format "$SOLUTION_FILE" --verify-no-changes --verbosity minimal --no-restore
fi

dotnet build "$SOLUTION_FILE" --configuration "$CONFIGURATION" --no-restore --verbosity minimal

if [ "$SKIP_TESTS" != "true" ]; then
  dotnet test "$TEST_PROJECT" \
    --configuration "$CONFIGURATION" \
    --no-build \
    --logger "trx;LogFileName=test-results.trx" \
    --collect "XPlat Code Coverage" \
    --results-directory "./artifacts/test-results" \
    --verbosity minimal \
    /p:CollectCoverage=true \
    "/p:CoverletOutput=${COVERAGE_OUTPUT}" \
    /p:CoverletOutputFormat=cobertura \
    /p:Threshold=100 \
    /p:ThresholdType=line%2cbranch \
    /p:ThresholdStat=total
fi
