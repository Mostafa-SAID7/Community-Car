# Comprehensive script to replace all inline styles with CSS classes

$viewsPath = "src/CommunityCar.Mvc/Views"
$areasPath = "src/CommunityCar.Mvc/Areas"

# Comprehensive style mappings
$replacements = @(
    # Font sizes
    @{ Pattern = 'style="font-size:\s*0\.75rem;?"'; Replacement = 'class="text-xs"' }
    @{ Pattern = 'style="font-size:\s*0\.7rem;?"'; Replacement = 'class="text-micro"' }
    @{ Pattern = 'style="font-size:\s*0\.65rem;?"'; Replacement = 'class="text-tiny"' }
    @{ Pattern = 'style="font-size:\s*0\.6rem;?"'; Replacement = 'class="badge-xs"' }
    @{ Pattern = 'style="font-size:\s*1\.05rem;\s*line-height:\s*1\.7;?"'; Replacement = 'class="content-readable"' }
    
    # Sizes
    @{ Pattern = 'style="width:\s*32px;\s*height:\s*32px;?"'; Replacement = 'class="size-32"' }
    @{ Pattern = 'style="width:\s*40px;\s*height:\s*40px;?"'; Replacement = 'class="size-40"' }
    @{ Pattern = 'style="width:\s*45px;\s*height:\s*45px;?"'; Replacement = 'class="size-45"' }
    @{ Pattern = 'style="width:\s*48px;\s*height:\s*48px;?"'; Replacement = 'class="size-48"' }
    
    # Heights
    @{ Pattern = 'style="height:\s*200px;\s*object-fit:\s*cover;?"'; Replacement = 'class="img-cover-200"' }
    @{ Pattern = 'style="height:\s*200px;?"'; Replacement = 'class="h-200"' }
    @{ Pattern = 'style="height:\s*150px;?"'; Replacement = 'class="h-150"' }
    @{ Pattern = 'style="height:\s*180px;?"'; Replacement = 'class="h-180"' }
    @{ Pattern = 'style="height:\s*250px;?"'; Replacement = 'class="h-250"' }
    @{ Pattern = 'style="height:\s*300px;?"'; Replacement = 'class="h-300"' }
    
    # Object fit
    @{ Pattern = 'style="object-fit:\s*cover;?"'; Replacement = 'class="object-cover"' }
    
    # Display
    @{ Pattern = 'style="display:\s*none;?"'; Replacement = 'class="d-none"' }
    @{ Pattern = 'style="display:\s*block;?"'; Replacement = 'class="d-block"' }
    @{ Pattern = 'style="display:\s*flex;?"'; Replacement = 'class="d-flex"' }
    
    # Max widths
    @{ Pattern = 'style="max-width:\s*250px;?"'; Replacement = 'class="max-w-250"' }
    @{ Pattern = 'style="max-width:\s*320px;?"'; Replacement = 'class="max-w-320"' }
    @{ Pattern = 'style="max-width:\s*350px;?"'; Replacement = 'class="max-w-350"' }
    @{ Pattern = 'style="max-width:\s*480px;?"'; Replacement = 'class="max-w-480"' }
    @{ Pattern = 'style="max-width:\s*500px;?"'; Replacement = 'class="max-w-500"' }
    
    # Min widths
    @{ Pattern = 'style="min-width:\s*150px;?"'; Replacement = 'class="min-w-150"' }
    @{ Pattern = 'style="min-width:\s*200px;?"'; Replacement = 'class="min-w-200"' }
    
    # Max heights
    @{ Pattern = 'style="max-height:\s*480px;?"'; Replacement = 'class="max-h-480"' }
    @{ Pattern = 'style="max-height:\s*500px;?"'; Replacement = 'class="max-h-500"' }
    
    # Overflow
    @{ Pattern = 'style="overflow-y:\s*auto;?"'; Replacement = 'class="overflow-y-auto"' }
    @{ Pattern = 'style="overflow:\s*auto;?"'; Replacement = 'class="overflow-auto"' }
    
    # Letter spacing
    @{ Pattern = 'style="letter-spacing:\s*0\.05em;?"'; Replacement = 'class="letter-spacing-wide"' }
    
    # Position and z-index
    @{ Pattern = 'style="top:\s*80px;\s*z-index:\s*1;?"'; Replacement = 'class="sticky-sidebar"' }
    @{ Pattern = 'style="z-index:\s*1000;?"'; Replacement = 'class="z-1000"' }
    @{ Pattern = 'style="z-index:\s*100;?"'; Replacement = 'class="z-100"' }
    @{ Pattern = 'style="z-index:\s*10;?"'; Replacement = 'class="z-10"' }
    @{ Pattern = 'style="z-index:\s*1;?"'; Replacement = 'class="z-1"' }
    
    # Complex combinations
    @{ Pattern = 'style="width:\s*320px;\s*max-height:\s*480px;\s*overflow-y:\s*auto;?"'; Replacement = 'class="dropdown-menu-320"' }
    @{ Pattern = 'style="width:\s*350px;\s*max-height:\s*500px;\s*overflow-y:\s*auto;?"'; Replacement = 'class="dropdown-menu-350"' }
    @{ Pattern = 'style="border-radius:\s*12px;\s*border:\s*2px solid [^;]+;\s*font-size:\s*1\.05rem;?"'; Replacement = 'class="form-control-rounded form-control-readable"' }
    
    # Reaction popup
    @{ Pattern = 'style="display:\s*none;\s*position:\s*absolute;\s*top:\s*-110%;\s*left:\s*50%;\s*transform:\s*translateX\(-50%\);\s*z-index:\s*1000;\s*flex-direction:\s*row;\s*gap:\s*4px;?"'; Replacement = 'class="reaction-popup"' }
    
    # Icon sizes
    @{ Pattern = 'style="font-size:\s*3rem;?\s*color:\s*[^;]+;?"'; Replacement = 'class="icon-placeholder"' }
)

$totalReplacements = 0
$filesModified = 0
$filesProcessed = 0

Write-Host "=== Replacing All Inline Styles ===" -ForegroundColor Cyan
Write-Host ""

function Process-Files {
    param ([string]$path)
    
    if (-not (Test-Path $path)) {
        Write-Host "Path not found: $path" -ForegroundColor Yellow
        return
    }
    
    $files = Get-ChildItem -Path $path -Filter *.cshtml -Recurse
    
    foreach ($file in $files) {
        $script:filesProcessed++
        $content = Get-Content $file.FullName -Raw
        $originalContent = $content
        $fileReplacements = 0
        
        foreach ($replacement in $replacements) {
            $matches = [regex]::Matches($content, $replacement.Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            if ($matches.Count -gt 0) {
                $content = [regex]::Replace($content, $replacement.Pattern, $replacement.Replacement, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
                $fileReplacements += $matches.Count
            }
        }
        
        if ($fileReplacements -gt 0) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
            Write-Host "[OK] $relativePath - $fileReplacements replacements" -ForegroundColor Green
            $script:filesModified++
            $script:totalReplacements += $fileReplacements
        }
    }
}

# Process Views
Write-Host "Processing Views..." -ForegroundColor Yellow
Process-Files -path $viewsPath

# Process Areas
Write-Host "Processing Areas..." -ForegroundColor Yellow
Process-Files -path $areasPath

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Files processed: $filesProcessed" -ForegroundColor White
Write-Host "Files modified: $filesModified" -ForegroundColor Yellow
Write-Host "Total replacements: $totalReplacements" -ForegroundColor Yellow
Write-Host ""

if ($totalReplacements -gt 0) {
    Write-Host "[SUCCESS] Replaced inline styles with CSS classes!" -ForegroundColor Green
} else {
    Write-Host "No inline styles found to replace." -ForegroundColor White
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the changes in your views" -ForegroundColor White
Write-Host "2. Test the application to ensure styles are applied correctly" -ForegroundColor White
Write-Host "3. Run 'git diff' to see all changes" -ForegroundColor White
