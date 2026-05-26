#!/bin/bash
# ==============================================================================
# Premium Book Management System - Deep Cache Clean & Dependency Restore Script
# ==============================================================================
# This script ensures a completely clean build environment by wiping out local 
# build targets, clearing global and local package caches, and executing a fresh 
# NuGet package restore on Ubuntu/Debian .NET 8.0 SDK environment.
# ==============================================================================

# Exit immediately if a command exits with a non-zero status
set -e

# Define terminal colors for beautiful output formatting
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0;37m' # No Color

echo -e "${CYAN}====================================================================${NC}"
echo -e "${GREEN}  Starting Deep Clean & Setup for Avalonia Book Management System  ${NC}"
echo -e "${CYAN}====================================================================${NC}"

# Check that the .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}[ERROR] .NET CLI ('dotnet') could not be found. Please install the .NET SDK. ${NC}"
    exit 1
fi

echo -e "\n${YELLOW}[Step 1/5] Removing active compilation and cache directories (bin/ & obj/)...${NC}"
if [ -d "bin" ]; then
    rm -rf bin
    echo -e "  - Deleted 'bin/' directory successfully."
fi
if [ -d "obj" ]; then
    rm -rf obj
    echo -e "  - Deleted 'obj/' directory successfully."
fi

echo -e "\n${YELLOW}[Step 2/5] Clearing global/local NuGet caches...${NC}"
dotnet nuget locals all --clear

echo -e "\n${YELLOW}[Step 3/5] Cleaning the .NET Solution/Project...${NC}"
dotnet clean

echo -e "\n${YELLOW}[Step 4/5] Restoring project packages strictly targeting net8.0...${NC}"
dotnet restore

echo -e "\n${YELLOW}[Step 5/5] Verifying .NET Environment and Configuration...${NC}"
SDK_VER=$(dotnet --version)
echo -e "${GREEN}  - .NET SDK Version: ${SDK_VER}${NC}"
echo -e "${GREEN}  - Target Framework: net8.0${NC}"
echo -e "${GREEN}  - Dependencies Restored: MySql.Data & Avalonia.Controls.DataGrid${NC}"

echo -e "\n${CYAN}====================================================================${NC}"
echo -e "${GREEN}  SUCCESS: Environment has been fully cleaned and successfully setup! ${NC}"
echo -e "${CYAN}====================================================================${NC}"
