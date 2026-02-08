# Clear Browser Storage Script
# Clears Session Storage, Local Storage, Cookies, Cache Storage, and other browser data
# Usage: .\clear-browser-storage.ps1 [-Browser Chrome|Edge|Firefox|All] [-StorageType All|Session|Local|Cookies|Cache]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Chrome", "Edge", "Firefox", "All")]
    [string]$Browser = "All",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Session", "Local", "Cookies", "Cache", "IndexedDB", "ServiceWorkers")]
    [string]$StorageType = "All"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Browser Storage Cleanup Script" -ForegroundColor Cyan
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

# Function to clear Chrome storage
function Clear-ChromeStorage {
    Write-Host "`nClearing Chrome storage..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "chrome"
    
    $chromePath = "$env:LOCALAPPDATA\Google\Chrome\User Data\Default"
    
    if (Test-Path $chromePath) {
        try {
            if ($StorageType -eq "All" -or $StorageType -eq "Session") {
                Remove-Item "$chromePath\Session Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Session Storage cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "Local") {
                Remove-Item "$chromePath\Local Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Local Storage cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "Cookies") {
                Remove-Item "$chromePath\Cookies" -Force -ErrorAction SilentlyContinue
                Remove-Item "$chromePath\Cookies-journal" -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Cookies cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "Cache") {
                Remove-Item "$chromePath\Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$chromePath\Code Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$chromePath\GPUCache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Cache cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "IndexedDB") {
                Remove-Item "$chromePath\IndexedDB" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ IndexedDB cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "ServiceWorkers") {
                Remove-Item "$chromePath\Service Worker" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Service Workers cleared" -ForegroundColor Green
            }
            
            # Additional storage types
            if ($StorageType -eq "All") {
                Remove-Item "$chromePath\File System" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$chromePath\Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$chromePath\Shared Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Additional storage cleared" -ForegroundColor Green
            }
            
            Write-Host "✓ Chrome storage cleared successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Error clearing Chrome storage: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✗ Chrome profile not found" -ForegroundColor Red
    }
}

# Function to clear Edge storage
function Clear-EdgeStorage {
    Write-Host "`nClearing Edge storage..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "msedge"
    
    $edgePath = "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default"
    
    if (Test-Path $edgePath) {
        try {
            if ($StorageType -eq "All" -or $StorageType -eq "Session") {
                Remove-Item "$edgePath\Session Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Session Storage cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "Local") {
                Remove-Item "$edgePath\Local Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Local Storage cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "Cookies") {
                Remove-Item "$edgePath\Cookies" -Force -ErrorAction SilentlyContinue
                Remove-Item "$edgePath\Cookies-journal" -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Cookies cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "Cache") {
                Remove-Item "$edgePath\Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$edgePath\Code Cache" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$edgePath\GPUCache" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Cache cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "IndexedDB") {
                Remove-Item "$edgePath\IndexedDB" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ IndexedDB cleared" -ForegroundColor Green
            }
            
            if ($StorageType -eq "All" -or $StorageType -eq "ServiceWorkers") {
                Remove-Item "$edgePath\Service Worker" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Service Workers cleared" -ForegroundColor Green
            }
            
            # Additional storage types
            if ($StorageType -eq "All") {
                Remove-Item "$edgePath\File System" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$edgePath\Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item "$edgePath\Shared Storage" -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Additional storage cleared" -ForegroundColor Green
            }
            
            Write-Host "✓ Edge storage cleared successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Error clearing Edge storage: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✗ Edge profile not found" -ForegroundColor Red
    }
}

# Function to clear Firefox storage
function Clear-FirefoxStorage {
    Write-Host "`nClearing Firefox storage..." -ForegroundColor Yellow
    
    Close-BrowserProcesses -ProcessName "firefox"
    
    $firefoxPath = "$env:APPDATA\Mozilla\Firefox\Profiles"
    
    if (Test-Path $firefoxPath) {
        $profiles = Get-ChildItem -Path $firefoxPath -Directory
        
        foreach ($profile in $profiles) {
            try {
                Write-Host "  Processing profile: $($profile.Name)" -ForegroundColor Cyan
                
                if ($StorageType -eq "All" -or $StorageType -eq "Session") {
                    Remove-Item "$($profile.FullName)\sessionstore*" -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Session Storage cleared" -ForegroundColor Green
                }
                
                if ($StorageType -eq "All" -or $StorageType -eq "Local") {
                    Remove-Item "$($profile.FullName)\webappsstore.sqlite" -Force -ErrorAction SilentlyContinue
                    Remove-Item "$($profile.FullName)\storage" -Recurse -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Local Storage cleared" -ForegroundColor Green
                }
                
                if ($StorageType -eq "All" -or $StorageType -eq "Cookies") {
                    Remove-Item "$($profile.FullName)\cookies.sqlite" -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Cookies cleared" -ForegroundColor Green
                }
                
                if ($StorageType -eq "All" -or $StorageType -eq "Cache") {
                    Remove-Item "$($profile.FullName)\cache2" -Recurse -Force -ErrorAction SilentlyContinue
                    Remove-Item "$($profile.FullName)\startupCache" -Recurse -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ Cache cleared" -ForegroundColor Green
                }
                
                if ($StorageType -eq "All" -or $StorageType -eq "IndexedDB") {
                    Remove-Item "$($profile.FullName)\storage\default" -Recurse -Force -ErrorAction SilentlyContinue
                    Write-Host "    ✓ IndexedDB cleared" -ForegroundColor Green
                }
            }
            catch {
                Write-Host "    ✗ Error clearing profile: $_" -ForegroundColor Red
            }
        }
        
        Write-Host "✓ Firefox storage cleared successfully" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Firefox profiles not found" -ForegroundColor Red
    }
}

# Main execution
Write-Host "Storage Type: $StorageType" -ForegroundColor Cyan
Write-Host "Target Browser: $Browser" -ForegroundColor Cyan
Write-Host ""

if ($Browser -eq "Chrome" -or $Browser -eq "All") {
    Clear-ChromeStorage
}

if ($Browser -eq "Edge" -or $Browser -eq "All") {
    Clear-EdgeStorage
}

if ($Browser -eq "Firefox" -or $Browser -eq "All") {
    Clear-FirefoxStorage
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: You may need to restart your browser for changes to take full effect." -ForegroundColor Yellow
