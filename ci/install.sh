#!/bin/sh
set -e

SDK_VERSION="3.0.100-preview-009812"

# Install the dotnet core 2.0 sdk
# We need a 2.x sdk because not all tooling works with 3.0 yet
echo "Installing core '2.x' sdk"
curl -sSL https://dot.net/v1/dotnet-install.sh | \
    bash -s -- -Channel 2.0 -Verbose

# Install the dotnet core 3.0 sdk
echo "Installing core '$SDK_VERSION' sdk"
curl -sSL https://dot.net/v1/dotnet-install.sh | \
    bash -s -- -Version $SDK_VERSION -Verbose
