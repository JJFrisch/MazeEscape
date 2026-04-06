#!/bin/bash

# MazeEscape Local Development Setup Script
# Usage: bash ./setup.sh
# Requires: bash, curl, git, and .NET SDK installed

set -e  # Exit on error

echo "🚀 MazeEscape Development Setup"
echo "================================="
echo ""

# Color codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Check prerequisites
echo "📋 Checking prerequisites..."
echo ""

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}✗ .NET SDK not found${NC}"
    echo "Install from: https://dotnet.microsoft.com/download/dotnet/9.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK found: $DOTNET_VERSION${NC}"

# Check Git
if ! command -v git &> /dev/null; then
    echo -e "${RED}✗ Git not found${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Git installed${NC}"

echo ""
echo "🔧 Setting up dependencies..."

# Restore packages
echo "  → Restoring NuGet packages..."
dotnet restore > /dev/null 2>&1
echo -e "  ${GREEN}✓ Packages restored${NC}"

# Build solution
echo "  → Building solution..."
dotnet build > /dev/null 2>&1
echo -e "  ${GREEN}✓ Solution built${NC}"

# Create App_Data if needed
echo "  → Creating data folders..."
mkdir -p MazeEscape.Api/App_Data
echo -e "  ${GREEN}✓ Folders created${NC}"

echo ""
echo "✅ Setup complete!"
echo ""
echo "📖 Next steps:"
echo ""
echo "1. Run the API:"
echo "   ${YELLOW}dotnet run --project MazeEscape.Api${NC}"
echo ""
echo "2. Test health endpoint (in another terminal):"
echo "   ${YELLOW}curl http://localhost:5000/api/health${NC}"
echo ""
echo "3. See LOCAL_DEVELOPMENT.md for full guide"
echo ""
