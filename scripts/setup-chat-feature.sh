#!/bin/bash

# Chat Feature Setup Script
# This script automates the setup of the chat feature

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}  CommunityCar Chat Feature Setup${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""

# Check if we're in the correct directory
if [ ! -f "CommunityCar.sln" ]; then
    echo -e "${RED}Error: Please run this script from the solution root directory${NC}"
    exit 1
fi

echo -e "${YELLOW}Step 1: Checking prerequisites...${NC}"

# Check if dotnet is installed
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✓ .NET SDK found: $DOTNET_VERSION${NC}"
else
    echo -e "${RED}✗ .NET SDK not found. Please install .NET 9.0 SDK${NC}"
    exit 1
fi

# Check if EF Core tools are installed
if dotnet ef --version &> /dev/null; then
    echo -e "${GREEN}✓ EF Core tools found${NC}"
else
    echo -e "${YELLOW}✗ EF Core tools not found. Installing...${NC}"
    dotnet tool install --global dotnet-ef
    echo -e "${GREEN}✓ EF Core tools installed${NC}"
fi

echo ""
echo -e "${YELLOW}Step 2: Creating database migration...${NC}"

# Create migration
MIGRATION_NAME="AddChatEntities"
if dotnet ef migrations add $MIGRATION_NAME \
    --project src/CommunityCar.Infrastructure \
    --startup-project src/CommunityCar.Mvc \
    --context ApplicationDbContext; then
    echo -e "${GREEN}✓ Migration created successfully${NC}"
else
    echo -e "${RED}✗ Failed to create migration${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Step 3: Applying database migration...${NC}"

# Apply migration
if dotnet ef database update \
    --project src/CommunityCar.Infrastructure \
    --startup-project src/CommunityCar.Mvc \
    --context ApplicationDbContext; then
    echo -e "${GREEN}✓ Database updated successfully${NC}"
else
    echo -e "${RED}✗ Failed to update database${NC}"
    echo ""
    echo -e "${YELLOW}Please check your connection string in appsettings.json${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Step 4: Building solution...${NC}"

if dotnet build --configuration Release; then
    echo -e "${GREEN}✓ Solution built successfully${NC}"
else
    echo -e "${RED}✗ Build failed${NC}"
    exit 1
fi

echo ""
echo -e "${CYAN}========================================${NC}"
echo -e "${GREEN}  Setup Complete!${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "1. Configure SignalR in Program.cs (see docs/CHAT_QUICK_START.md)"
echo -e "2. Add navigation link to your layout"
echo -e "3. Run the application: cd src/CommunityCar.Mvc && dotnet run"
echo -e "4. Navigate to /Communications/Chats"
echo ""
echo -e "${YELLOW}Documentation:${NC}"
echo -e "- Quick Start: docs/CHAT_QUICK_START.md"
echo -e "- Full Documentation: docs/CHAT_FEATURE.md"
echo -e "- Implementation Summary: docs/CHAT_IMPLEMENTATION_SUMMARY.md"
echo ""
echo -e "${CYAN}Happy chatting! 💬${NC}"
