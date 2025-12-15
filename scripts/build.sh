#!/bin/bash

# PortXOR Build Script
# Builds the PortXOR project and runs tests

echo "Building PortXOR..."

# Build the main project
cd src
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Run tests
cd ../tests
dotnet test

if [ $? -ne 0 ]; then
    echo "Tests failed!"
    exit 1
fi

echo "Build completed successfully!"

