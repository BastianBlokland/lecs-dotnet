#!/bin/sh
set -e

# Delete old artifacts
rm -rf ./artifacts

# Clean the solution
dotnet clean src/Lecs.sln
