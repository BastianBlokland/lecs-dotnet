#!/bin/sh
set -e

FILTER="${FILTER:-"Lecs.Benchmark.*"}"
OUTPUT_PATH="./artifacts/benchmark"

# Run benchmark
dotnet run -c Release -p src/Lecs.Benchmark/Lecs.Benchmark.csproj \
    --filter $FILTER --exporters GitHub --artifacts $OUTPUT_PATH
