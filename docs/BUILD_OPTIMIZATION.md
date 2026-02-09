# Build Performance Optimization Guide

## What Was Optimized

### 1. Project Files (.csproj)
- **BuildInParallel**: Enables parallel project builds
- **UseSharedCompilation**: Reuses compiler processes across builds
- **ProduceReferenceAssembly**: Creates reference assemblies for faster downstream builds
- **GenerateDocumentationFile**: Disabled in development for faster builds
- **RazorCompileOnBuild**: Optimizes Razor view compilation

### 2. Directory.Build.props (Global Settings)
- **RestorePackagesWithLockFile**: Uses lock files for consistent, faster package restore
- **DisableImplicitNuGetFallbackFolder**: Reduces package search locations
- **RunAnalyzersDuringBuild**: Disabled in Debug mode for faster builds
- **UseCommonOutputDirectory**: Reduces duplicate output files

### 3. NuGet.config
- Centralized package cache in `.nuget/packages`
- Single package source for faster restore

### 4. .editorconfig
- Disabled non-critical analyzers in development
- Keeps important nullable reference warnings

## Build Time Improvements

**Before**: ~232 seconds (full build)
**Expected After**: 
- First build: ~180-200 seconds (20-25% faster)
- Incremental builds: ~10-30 seconds (90% faster)
- Hot reload changes: ~2-5 seconds (98% faster)

## How to Use

### For Development (Recommended)

Use the watch mode for automatic rebuilds on file changes:

```cmd
cd src\CommunityCar.Mvc
dotnet watch run
```

Or use the provided script:

```cmd
scripts\fast-run.cmd
```

### For Full Builds

First time or after pulling changes:

```cmd
dotnet restore
dotnet build
```

Or use the optimized script:

```cmd
scripts\restore-packages.cmd
scripts\fast-build.cmd
```

### For Running the App

```cmd
cd src\CommunityCar.Mvc
dotnet run --no-build
```

## Best Practices

1. **Use `dotnet watch run`** during development - only rebuilds changed files
2. **Avoid `dotnet clean`** unless necessary - it removes cached build artifacts
3. **Use `--no-build` flag** when running if you just built
4. **Keep packages.lock.json** in source control for consistent builds
5. **Close unnecessary Visual Studio instances** - they consume compiler resources

## Environment Variables

Add these to your system or IDE for better performance:

```cmd
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_NOLOGO=1
```

## Troubleshooting

### Build is still slow
- Check if antivirus is scanning build folders - add exclusions for `bin/`, `obj/`, `.nuget/`
- Ensure you're using an SSD, not HDD
- Close other resource-intensive applications
- Update to latest .NET SDK

### Package restore fails
- Delete `.nuget/packages` folder and run `dotnet restore` again
- Check internet connection
- Clear NuGet cache: `dotnet nuget locals all --clear`

### Hot reload not working
- Ensure you're using `dotnet watch run`
- Some changes (like adding new files) require full restart
- Check that file watchers aren't disabled

## Additional Optimizations

### For CI/CD Pipelines
```cmd
dotnet build --configuration Release /p:ContinuousIntegrationBuild=true
```

### For Production Builds
```cmd
dotnet publish -c Release -o ./publish --no-restore
```

## Monitoring Build Performance

To see detailed build timing:

```cmd
dotnet build /p:ReportAnalyzer=true /v:detailed
```

## Summary

These optimizations focus on:
- ✅ Parallel compilation
- ✅ Shared compiler processes
- ✅ Incremental builds
- ✅ Reduced analyzer overhead
- ✅ Optimized package restore
- ✅ Hot reload for development

The biggest improvement comes from using `dotnet watch run` during development, which only rebuilds changed files instead of the entire solution.
