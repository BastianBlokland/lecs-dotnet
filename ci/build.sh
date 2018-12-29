#!/bin/sh
set -e

# Diagnostics
echo "Used tooling: '$(which dotnet)'"
echo "Installed sdks:"
dotnet --list-sdks
echo "";

# Build the solution
dotnet build src/Lecs.sln
