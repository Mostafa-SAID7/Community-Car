@echo off
REM Fast run script for development with hot reload

echo Starting application with hot reload...

REM Set environment variables for faster builds
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_NOLOGO=1

REM Navigate to Mvc project
cd src\CommunityCar.Mvc

REM Run with watch (hot reload)
dotnet watch run --no-hot-reload

cd ..\..
