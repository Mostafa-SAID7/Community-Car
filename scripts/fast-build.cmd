@echo off
REM Fast build script for development

echo Starting optimized build...

REM Set environment variables for faster builds
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_NOLOGO=1

REM Build with parallel execution
dotnet build --no-restore --nologo /p:UseSharedCompilation=true /p:BuildInParallel=true /maxcpucount

echo Build completed!
