# Chat Feature Setup Script
# This script automates the setup of the chat feature

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CommunityCar Chat Feature Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the correct directory
if (-not (Test-Path "CommunityCar.sln")) {
    Write-Host "Error: Please run this script from the solution root directory" -ForegroundColor Red
    exit 1
}

Write-Host "Step 1: Checking prerequisites..." -ForegroundColor Yellow

# Check if dotnet is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 9.0 SDK" -ForegroundColor Red
    exit 1
}

# Check if EF Core tools are installed
try {
    $efVersion = dotnet ef --version
    Write-Host "✓ EF Core tools found" -ForegroundColor Green
} catch {
    Write-Host "✗ EF Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "✓ EF Core tools installed" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 2: Creating database migration..." -ForegroundColor Yellow

# Create migration
$migrationName = "AddChatEntities"
try {
    dotnet ef migrations add $migrationName `
        --project src/CommunityCar.Infrastructure `
        --startup-project src/CommunityCar.Mvc `
        --context ApplicationDbContext
    
    Write-Host "✓ Migration created successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to create migration" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 3: Applying database migration..." -ForegroundColor Yellow

# Apply migration
try {
    dotnet ef database update `
        --project src/CommunityCar.Infrastructure `
        --startup-project src/CommunityCar.Mvc `
        --context ApplicationDbContext
    
    Write-Host "✓ Database updated successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to update database" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check your connection string in appsettings.json" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Step 4: Building solution..." -ForegroundColor Yellow

try {
    dotnet build --configuration Release
    Write-Host "✓ Solution built successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Build failed" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Configure SignalR in Program.cs (see docs/CHAT_QUICK_START.md)" -ForegroundColor White
Write-Host "2. Add navigation link to your layout" -ForegroundColor White
Write-Host "3. Run the application: cd src/CommunityCar.Mvc && dotnet run" -ForegroundColor White
Write-Host "4. Navigate to /Communications/Chats" -ForegroundColor White
Write-Host ""
Write-Host "Documentation:" -ForegroundColor Yellow
Write-Host "- Quick Start: docs/CHAT_QUICK_START.md" -ForegroundColor White
Write-Host "- Full Documentation: docs/CHAT_FEATURE.md" -ForegroundColor White
Write-Host "- Implementation Summary: docs/CHAT_IMPLEMENTATION_SUMMARY.md" -ForegroundColor White
Write-Host ""
Write-Host "Happy chatting! 💬" -ForegroundColor Cyan
