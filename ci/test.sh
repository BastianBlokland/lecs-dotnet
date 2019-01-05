#!/bin/sh

COVERAGE_THRESHOLD=75
TEST_RESULT_PATH="./../../artifacts/xunit.results.xml"
COVERAGE_RESULT_PATH="./../../artifacts/coverage.cobertura.xml"
REPORT_RESULT_PATH="./../../artifacts/coverage_report"

# Diagnostics
echo "Used tooling: '$(which dotnet)'"
echo "Installed sdks:"
dotnet --list-sdks
echo "";

# Build the solution
dotnet build --configuration Debug src/Lecs.sln

# Run test
dotnet test src/Lecs.Tests/Lecs.Tests.csproj \
    --logger "xunit;LogFilePath=$TEST_RESULT_PATH" \
    /p:CollectCoverage=true /p:Include="[Lecs]*" /p:UseSourceLink=true /p:Threshold=$COVERAGE_THRESHOLD \
    /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$COVERAGE_RESULT_PATH
EXIT_CODE=$?

# Create coverage report
(cd src/Lecs.Tests && \
    dotnet reportgenerator \
    "-reports:$COVERAGE_RESULT_PATH" "-targetdir:$REPORT_RESULT_PATH" "-reporttypes:HtmlInline_AzurePipelines")

exit $EXIT_CODE
