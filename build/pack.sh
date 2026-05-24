#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${CONFIGURATION:-Release}"
VERSION_SUFFIX="${VERSION_SUFFIX:-}"
ALLOW_PACK_WITHOUT_GIT="${ALLOW_PACK_WITHOUT_GIT:-false}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
SOLUTION_FILE="./OpenTelemetry.sln"
PACKAGE_PROJECT="./src/OpenTelemetry/OpenTelemetry.csproj"
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
run_vulnerability_audit
dotnet format "$SOLUTION_FILE" --verify-no-changes --verbosity minimal --no-restore
dotnet build "$SOLUTION_FILE" --configuration "$CONFIGURATION" --no-restore --verbosity minimal
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

pack_args=(
  pack
  "$PACKAGE_PROJECT"
  --configuration "$CONFIGURATION"
  --no-build
  --output "./artifacts/packages"
  --verbosity minimal
  -p:EnablePackageValidation=true
)

if [ -n "$VERSION_SUFFIX" ]; then
  pack_args+=("/p:VersionSuffix=$VERSION_SUFFIX")
fi

if [ "$ALLOW_PACK_WITHOUT_GIT" = "true" ]; then
  pack_args+=("-p:AllowPackWithoutGit=true")
fi

dotnet "${pack_args[@]}"
