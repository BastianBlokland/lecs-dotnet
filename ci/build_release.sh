#!/bin/sh
set -e

# Diagnostics
echo "Used tooling: '$(which dotnet)'"
echo "Installed sdks:"
dotnet --list-sdks
echo "";

# Build the solution in release configuration
dotnet build --configuration Release /p:TreatWarningsAsErrors=true /warnaserror src/Lecs.sln
