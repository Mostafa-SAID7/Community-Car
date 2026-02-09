@echo off
REM Restore NuGet packages with lock file

echo Restoring NuGet packages...

REM Set environment variables
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_NOLOGO=1

REM Restore packages
dotnet restore --locked-mode

echo Package restore completed!
