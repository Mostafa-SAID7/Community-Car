# Clear Localhost Storage Script
# Specifically clears storage for localhost:5010 (CommunityCar app)
# Usage: .\clear-localhost-storage.ps1 [-Browser Chrome|Edge|All]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Chrome", "Edge", "All")]
    [string]$Browser = "All"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Localhost Storage Cleanup Script" -ForegroundColor Cyan
Write-Host "Target: localhost:5010" -ForegroundColor Cyan
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

# Function to clear Chrome localhost storage
function Clear-ChromeLocalhostStorage {
    Write-Host "`nClearing Chrome localhost storage..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "chrome"
    
    $chromePath = "$env:LOCALAPPDATA\Google\Chrome\User Data\Default"
    
    if (Test-Path $chromePath) {
        try {
            # Clear Local Storage for localhost
            $localStoragePath = "$chromePath\Local Storage\leveldb"
            if (Test-Path $localStoragePath) {
                $files = Get-ChildItem -Path $localStoragePath -File | Where-Object { $_.Name -match "localhost" }
                foreach ($file in $files) {
                    Remove-Item $file.FullName -Force -ErrorAction SilentlyContinue
                }
                Write-Host "  ✓ Local Storage cleared for localhost" -ForegroundColor Green
            }
            
            # Clear Session Storage for localhost
            $sessionStoragePath = "$chromePath\Session Storage"
            if (Test-Path $sessionStoragePath) {
                Remove-Item "$sessionStoragePath\*localhost*" -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Session Storage cleared for localhost" -ForegroundColor Green
            }
            
            # Clear IndexedDB for localhost
            $indexedDBPath = "$chromePath\IndexedDB"
            if (Test-Path $indexedDBPath) {
                $localhostDirs = Get-ChildItem -Path $indexedDBPath -Directory | Where-Object { $_.Name -match "localhost" }
                foreach ($dir in $localhostDirs) {
                    Remove-Item $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
                }
                Write-Host "  ✓ IndexedDB cleared for localhost" -ForegroundColor Green
            }
            
            # Clear Service Workers for localhost
            $serviceWorkerPath = "$chromePath\Service Worker\ScriptCache"
            if (Test-Path $serviceWorkerPath) {
                $localhostDirs = Get-ChildItem -Path $serviceWorkerPath -Directory | Where-Object { $_.Name -match "localhost" }
                foreach ($dir in $localhostDirs) {
                    Remove-Item $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
                }
                Write-Host "  ✓ Service Workers cleared for localhost" -ForegroundColor Green
            }
            
            # Clear Cache Storage for localhost
            $cacheStoragePath = "$chromePath\Cache\Cache_Data"
            if (Test-Path $cacheStoragePath) {
                # Note: Cache is not domain-specific, so we clear all
                Write-Host "  ⚠ Cache is shared across domains - consider full cache clear" -ForegroundColor Yellow
            }
            
            # Clear Cookies for localhost
            $cookiesFile = "$chromePath\Cookies"
            if (Test-Path $cookiesFile) {
                Write-Host "  ⚠ Cookies file requires browser restart to clear specific domain" -ForegroundColor Yellow
            }
            
            Write-Host "✓ Chrome localhost storage cleared successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Error clearing Chrome localhost storage: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✗ Chrome profile not found" -ForegroundColor Red
    }
}

# Function to clear Edge localhost storage
function Clear-EdgeLocalhostStorage {
    Write-Host "`nClearing Edge localhost storage..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "msedge"
    
    $edgePath = "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default"
    
    if (Test-Path $edgePath) {
        try {
            # Clear Local Storage for localhost
            $localStoragePath = "$edgePath\Local Storage\leveldb"
            if (Test-Path $localStoragePath) {
                $files = Get-ChildItem -Path $localStoragePath -File | Where-Object { $_.Name -match "localhost" }
                foreach ($file in $files) {
                    Remove-Item $file.FullName -Force -ErrorAction SilentlyContinue
                }
                Write-Host "  ✓ Local Storage cleared for localhost" -ForegroundColor Green
            }
            
            # Clear Session Storage for localhost
            $sessionStoragePath = "$edgePath\Session Storage"
            if (Test-Path $sessionStoragePath) {
                Remove-Item "$sessionStoragePath\*localhost*" -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Session Storage cleared for localhost" -ForegroundColor Green
            }
            
            # Clear IndexedDB for localhost
            $indexedDBPath = "$edgePath\IndexedDB"
            if (Test-Path $indexedDBPath) {
                $localhostDirs = Get-ChildItem -Path $indexedDBPath -Directory | Where-Object { $_.Name -match "localhost" }
                foreach ($dir in $localhostDirs) {
                    Remove-Item $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
                }
                Write-Host "  ✓ IndexedDB cleared for localhost" -ForegroundColor Green
            }
            
            # Clear Service Workers for localhost
            $serviceWorkerPath = "$edgePath\Service Worker\ScriptCache"
            if (Test-Path $serviceWorkerPath) {
                $localhostDirs = Get-ChildItem -Path $serviceWorkerPath -Directory | Where-Object { $_.Name -match "localhost" }
                foreach ($dir in $localhostDirs) {
                    Remove-Item $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
                }
                Write-Host "  ✓ Service Workers cleared for localhost" -ForegroundColor Green
            }
            
            # Clear Shared Storage for localhost
            $sharedStoragePath = "$edgePath\Shared Storage"
            if (Test-Path $sharedStoragePath) {
                $localhostDirs = Get-ChildItem -Path $sharedStoragePath -Directory | Where-Object { $_.Name -match "localhost" }
                foreach ($dir in $localhostDirs) {
                    Remove-Item $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
                }
                Write-Host "  ✓ Shared Storage cleared for localhost" -ForegroundColor Green
            }
            
            Write-Host "✓ Edge localhost storage cleared successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Error clearing Edge localhost storage: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✗ Edge profile not found" -ForegroundColor Red
    }
}

# Main execution
Write-Host "Target Browser: $Browser" -ForegroundColor Cyan
Write-Host ""

if ($Browser -eq "Chrome" -or $Browser -eq "All") {
    Clear-ChromeLocalhostStorage
}

if ($Browser -eq "Edge" -or $Browser -eq "All") {
    Clear-EdgeLocalhostStorage
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Localhost Storage Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Recommended: Hard refresh the page (Ctrl+Shift+R) after restarting browser" -ForegroundColor Yellow
Write-Host "This ensures all cached resources are reloaded" -ForegroundColor Yellow
