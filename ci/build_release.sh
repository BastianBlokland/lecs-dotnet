#!/bin/sh
set -e

# Build the solution in release configuration
dotnet build --configuration Release /p:TreatWarningsAsErrors=true /warnaserror src/Lecs.sln
