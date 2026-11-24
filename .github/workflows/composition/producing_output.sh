#!/bin/bash
set -euo pipefail

VERSION="${VERSION_NEXT:-}"
VERSION_NS="${VERSION_NOSUFFIX:-}"
if [ -z "$VERSION_NS" ]; then
    VERSION_NS="$VERSION" # IF NOT PREVIEW SAY THE VERSION NO SUFFIX, IF PREVIEW YOU GET A x.x.x HERE
fi
MAJOR="${VERSION_NS%%.*}"
MAJOR_ZERO_ZERO="${MAJOR}.0.0" # FOR .NET PROJECT ASSEMBLY VERSION.

BRANCH="${BRANCH:-}"
echo "Releasing version: $VERSION"
# Anything to Produce OUTPUT VERSION

OUT="/tmp/artifacts/output";
mkdir -p "$OUT"

dotnet build "./src/StatePulse.Net.Analyzers/StatePulse.Net.Analyzers.csproj" -c Release /p:InformationalVersion=$VERSION /p:Version=$VERSION /p:FileVersion=$VERSION_NS /p:AssemblyVersion=$MAJOR_ZERO_ZERO
dotnet build "./src/StatePulse.Net.Abstractions/StatePulse.Net.Abstractions.csproj" -c Release /p:InformationalVersion=$VERSION /p:Version=$VERSION /p:FileVersion=$VERSION_NS /p:AssemblyVersion=$MAJOR_ZERO_ZERO
dotnet build "./src/StatePulse.NET/StatePulse.NET.csproj" -c Release /p:InformationalVersion=$VERSION /p:Version=$VERSION /p:FileVersion=$VERSION_NS /p:AssemblyVersion=$MAJOR_ZERO_ZERO
dotnet build "./tests/StatPulse.NET.Tests/StatePulse.NET.Tests.csproj" -c Release /p:InformationalVersion=$VERSION /p:Version=$VERSION /p:FileVersion=$VERSION_NS /p:AssemblyVersion=$MAJOR_ZERO_ZERO

dotnet pack "./src/StatePulse.Net.Abstractions/StatePulse.Net.Abstractions.csproj" -c Release -o "/tmp/artifacts/output" /p:InformationalVersion=$VERSION /p:Version=$VERSION /p:FileVersion=$VERSION_NS /p:AssemblyVersion=$MAJOR_ZERO_ZERO
dotnet pack "./src/StatePulse.NET/StatePulse.NET.csproj" -c Release -o "/tmp/artifacts/output" /p:InformationalVersion=$VERSION /p:Version=$VERSION /p:FileVersion=$VERSION_NS /p:AssemblyVersion=$MAJOR_ZERO_ZERO
