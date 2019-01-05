#!/bin/sh
set -e

FILTER="${FILTER:-"Lecs.Benchmark.*"}"
OUTPUT_PATH="./artifacts/benchmark"

# Diagnostics
echo "Used tooling: '$(which dotnet)'"
echo "Installed sdks:"
dotnet --list-sdks
echo "";

# Build the solution
dotnet build --configuration Release src/Lecs.sln

# Run benchmark
dotnet run -c Release -p src/Lecs.Benchmark/Lecs.Benchmark.csproj \
    --filter $FILTER --exporters GitHub --artifacts $OUTPUT_PATH
