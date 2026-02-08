# Clear Browser Cache Script
# Comprehensive cache clearing including HTTP cache, GPU cache, and code cache
# Usage: .\clear-browser-cache.ps1 [-Browser Chrome|Edge|Firefox|All]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Chrome", "Edge", "Firefox", "All")]
    [string]$Browser = "All"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Browser Cache Cleanup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to close browser processes
function Close-BrowserProcesses {
    param([string]$ProcessName)
    
    $processes = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($processes) {
        Write-Host "Closing $ProcessName processes..." -ForegroundColor Yellow
        $processes | Stop-Process -Force
        Start-Sleep -Seconds 2
        Write-Host "✓ $ProcessName closed" -ForegroundColor Green
    }
}

# Function to get folder size
function Get-FolderSize {
    param([string]$Path)
    
    if (Test-Path $Path) {
        $size = (Get-ChildItem -Path $Path -Recurse -ErrorAction SilentlyContinue | 
                 Measure-Object -Property Length -Sum -ErrorAction SilentlyContinue).Sum
        return [math]::Round($size / 1MB, 2)
    }
    return 0
}

# Function to clear Chrome cache
function Clear-ChromeCache {
    Write-Host "`nClearing Chrome cache..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "chrome"
    
    $chromePath = "$env:LOCALAPPDATA\Google\Chrome\User Data\Default"
    
    if (Test-Path $chromePath) {
        try {
            # Calculate cache size before clearing
            $cacheSize = Get-FolderSize -Path "$chromePath\Cache"
            $codeCacheSize = Get-FolderSize -Path "$chromePath\Code Cache"
            $gpuCacheSize = Get-FolderSize -Path "$chromePath\GPUCache"
            $totalSize = $cacheSize + $codeCacheSize + $gpuCacheSize
            
            Write-Host "  Cache size before clearing: $totalSize MB" -ForegroundColor Cyan
            
            # Clear HTTP Cache
            if (Test-Path "$chromePath\Cache") {
                Remove-Item "$chromePath\Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ HTTP Cache cleared ($cacheSize MB)" -ForegroundColor Green
            }
            
            # Clear Code Cache (JavaScript, WebAssembly)
            if (Test-Path "$chromePath\Code Cache") {
                Remove-Item "$chromePath\Code Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Code Cache cleared ($codeCacheSize MB)" -ForegroundColor Green
            }
            
            # Clear GPU Cache
            if (Test-Path "$chromePath\GPUCache") {
                Remove-Item "$chromePath\GPUCache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ GPU Cache cleared ($gpuCacheSize MB)" -ForegroundColor Green
            }
            
            # Clear Storage Cache
            if (Test-Path "$chromePath\Storage\ext") {
                Remove-Item "$chromePath\Storage\ext" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Storage Cache cleared" -ForegroundColor Green
            }
            
            # Clear Media Cache
            if (Test-Path "$chromePath\Media Cache") {
                Remove-Item "$chromePath\Media Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Media Cache cleared" -ForegroundColor Green
            }
            
            # Clear Application Cache
            if (Test-Path "$chromePath\Application Cache") {
                Remove-Item "$chromePath\Application Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Application Cache cleared" -ForegroundColor Green
            }
            
            Write-Host "✓ Chrome cache cleared successfully (Total: $totalSize MB)" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Error clearing Chrome cache: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✗ Chrome profile not found" -ForegroundColor Red
    }
}

# Function to clear Edge cache
function Clear-EdgeCache {
    Write-Host "`nClearing Edge cache..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "msedge"
    
    $edgePath = "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default"
    
    if (Test-Path $edgePath) {
        try {
            # Calculate cache size before clearing
            $cacheSize = Get-FolderSize -Path "$edgePath\Cache"
            $codeCacheSize = Get-FolderSize -Path "$edgePath\Code Cache"
            $gpuCacheSize = Get-FolderSize -Path "$edgePath\GPUCache"
            $totalSize = $cacheSize + $codeCacheSize + $gpuCacheSize
            
            Write-Host "  Cache size before clearing: $totalSize MB" -ForegroundColor Cyan
            
            # Clear HTTP Cache
            if (Test-Path "$edgePath\Cache") {
                Remove-Item "$edgePath\Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ HTTP Cache cleared ($cacheSize MB)" -ForegroundColor Green
            }
            
            # Clear Code Cache
            if (Test-Path "$edgePath\Code Cache") {
                Remove-Item "$edgePath\Code Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Code Cache cleared ($codeCacheSize MB)" -ForegroundColor Green
            }
            
            # Clear GPU Cache
            if (Test-Path "$edgePath\GPUCache") {
                Remove-Item "$edgePath\GPUCache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ GPU Cache cleared ($gpuCacheSize MB)" -ForegroundColor Green
            }
            
            # Clear Storage Cache
            if (Test-Path "$edgePath\Storage\ext") {
                Remove-Item "$edgePath\Storage\ext" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Storage Cache cleared" -ForegroundColor Green
            }
            
            # Clear Media Cache
            if (Test-Path "$edgePath\Media Cache") {
                Remove-Item "$edgePath\Media Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Media Cache cleared" -ForegroundColor Green
            }
            
            # Clear Application Cache
            if (Test-Path "$edgePath\Application Cache") {
                Remove-Item "$edgePath\Application Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Application Cache cleared" -ForegroundColor Green
            }
            
            Write-Host "✓ Edge cache cleared successfully (Total: $totalSize MB)" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Error clearing Edge cache: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✗ Edge profile not found" -ForegroundColor Red
    }
}

# Function to clear Firefox cache
function Clear-FirefoxCache {
    Write-Host "`nClearing Firefox cache..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "firefox"
    
    $firefoxPath = "$env:APPDATA\Mozilla\Firefox\Profiles"
    
    if (Test-Path $firefoxPath) {
        $profiles = Get-ChildItem -Path $firefoxPath -Directory
        
        foreach ($profile in $profiles) {
            try {
                Write-Host "  Processing profile: $($profile.Name)" -ForegroundColor Cyan
                
                # Calculate cache size
                $cache2Size = Get-FolderSize -Path "$($profile.FullName)\cache2"
                $startupCacheSize = Get-FolderSize -Path "$($profile.FullName)\startupCache"
                $totalSize = $cache2Size + $startupCacheSize
                
                Write-Host "    Cache size: $totalSize MB" -ForegroundColor Cyan
                
                # Clear main cache
                if (Test-Path "$($profile.FullName)\cache2") {
                    Remove-Item "$($profile.FullName)\cache2" -Recurse -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Main cache cleared ($cache2Size MB)" -ForegroundColor Green
                }
                
                # Clear startup cache
                if (Test-Path "$($profile.FullName)\startupCache") {
                    Remove-Item "$($profile.FullName)\startupCache" -Recurse -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Startup cache cleared ($startupCacheSize MB)" -ForegroundColor Green
                }
                
                # Clear offline cache
                if (Test-Path "$($profile.FullName)\OfflineCache") {
                    Remove-Item "$($profile.FullName)\OfflineCache" -Recurse -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Offline cache cleared" -ForegroundColor Green
                }
                
                # Clear thumbnails
                if (Test-Path "$($profile.FullName)\thumbnails") {
                    Remove-Item "$($profile.FullName)\thumbnails" -Recurse -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Thumbnails cleared" -ForegroundColor Green
                }
            }
            catch {
                Write-Host "    ✗ Error clearing profile cache: $_" -ForegroundColor Red
            }
        }
        
        Write-Host "✓ Firefox cache cleared successfully" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Firefox profiles not found" -ForegroundColor Red
    }
}

# Main execution
Write-Host "Target Browser: $Browser" -ForegroundColor Cyan
Write-Host ""

$totalCleared = 0

if ($Browser -eq "Chrome" -or $Browser -eq "All") {
    Clear-ChromeCache
}

if ($Browser -eq "Edge" -or $Browser -eq "All") {
    Clear-EdgeCache
}

if ($Browser -eq "Firefox" -or $Browser -eq "All") {
    Clear-FirefoxCache
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Cache Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Benefits of clearing cache:" -ForegroundColor Yellow
Write-Host "  • Frees up disk space" -ForegroundColor White
Write-Host "  • Forces fresh download of updated resources" -ForegroundColor White
Write-Host "  • Resolves stale content issues" -ForegroundColor White
Write-Host "  • Improves browser performance" -ForegroundColor White
Write-Host ""
Write-Host "Note: First page load after clearing cache will be slower" -ForegroundColor Yellow
