#!/bin/bash
set -euo pipefail

VERSION_NEXT="${VERSION_NEXT:-}"
VERSION_NEXT_PREVIEW="${VERSION_NEXT_PREVIEW:-}"
BRANCH="${BRANCH:-}"

if [ "$BRANCH" = "main-dev" ]; then
  if [ -z "$VERSION_NEXT_PREVIEW" ]; then
    echo "::error::main-dev build but VERSION_NEXT_PREVIEW is empty" >&2
    exit 1
  fi
  VERSION="$VERSION_NEXT_PREVIEW"
else
  if [ -z "$VERSION_NEXT" ]; then
    echo "::error::VERSION_NEXT is empty" >&2
    exit 1
  fi
  VERSION="$VERSION_NEXT"
fi

echo "Releasing version: $VERSION"
OUT="/tmp/artifacts/output";
mkdir -p "$OUT"

dotnet build "./src/StatePulse.Net.Analyzers/StatePulse.Net.Analyzers.csproj" -c Release -p:Version=$VERSION
dotnet build "./src/StatePulse.Net.Abstractions/StatePulse.Net.Abstractions.csproj" -c Release -p:Version=$VERSION
dotnet build "./src/StatePulse.NET/StatePulse.NET.csproj" -c Release -p:Version=$VERSION

dotnet pack "./src/StatePulse.Net.Abstractions/StatePulse.Net.Abstractions.csproj" -c Release -o "/tmp/artifacts/output" -p:Version=$VERSION
dotnet pack "./src/StatePulse.NET/StatePulse.NET.csproj" -c Release -o "/tmp/artifacts/output" -p:Version=$VERSION
