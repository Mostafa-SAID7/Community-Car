# PowerShell script to audit CSS files for hardcoded colors

$cssPath = "src/CommunityCar.Mvc/wwwroot/css"
$excludePatterns = @("*.min.css", "toastr.*", "lib/*")

Write-Host "=== CSS Color Audit ===" -ForegroundColor Cyan
Write-Host ""

# Find all CSS files
$cssFiles = Get-ChildItem -Path $cssPath -Filter *.css -Recurse | Where-Object {
    $exclude = $false
    foreach ($pattern in $excludePatterns) {
        if ($_.Name -like $pattern -or $_.FullName -like "*\lib\*") {
            $exclude = $true
            break
        }
    }
    -not $exclude
}

$totalHardcodedColors = 0
$colorsByFile = @{}

foreach ($file in $cssFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Find hex colors
    $hexColors = [regex]::Matches($content, '#[0-9a-fA-F]{3,6}')
    
    # Find rgb/rgba colors
    $rgbColors = [regex]::Matches($content, 'rgba?\([^)]+\)')
    
    $allColors = @()
    $allColors += $hexColors | ForEach-Object { $_.Value }
    $allColors += $rgbColors | ForEach-Object { $_.Value }
    
    if ($allColors.Count -gt 0) {
        $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
        $colorsByFile[$relativePath] = $allColors
        $totalHardcodedColors += $allColors.Count
    }
}

Write-Host "Found $totalHardcodedColors hardcoded colors in $($colorsByFile.Count) files" -ForegroundColor Yellow
Write-Host ""

foreach ($file in $colorsByFile.Keys | Sort-Object) {
    Write-Host "File: $file" -ForegroundColor Green
    $uniqueColors = $colorsByFile[$file] | Select-Object -Unique
    foreach ($color in $uniqueColors) {
        $count = ($colorsByFile[$file] | Where-Object { $_ -eq $color }).Count
        Write-Host "  $color (used $count times)" -ForegroundColor White
    }
    Write-Host ""
}

Write-Host "=== Recommendations ===" -ForegroundColor Cyan
Write-Host "1. Replace hardcoded colors with CSS variables from abstracts/variables.css"
Write-Host "2. Add new color variables if needed for consistent theming"
Write-Host "3. Ensure all colors support dark mode"
Write-Host ""
