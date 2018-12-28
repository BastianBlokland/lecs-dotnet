#!/bin/sh
set -e

TEST_RESULT_PATH="./../../artifacts/xunit.results.xml"
COVERAGE_RESULT_PATH="./../../artifacts/coverage.cobertura.xml"

# Run test
dotnet test src/Lecs.Tests/Lecs.Tests.csproj \
    --logger "xunit;LogFilePath=$TEST_RESULT_PATH" \
    /p:CollectCoverage=true /p:Include="[Lecs]*" /p:UseSourceLink=true \
    /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$COVERAGE_RESULT_PATH
