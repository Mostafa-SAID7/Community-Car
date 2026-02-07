# PowerShell script to extract inline styles from views and replace with CSS classes

$viewsPath = "src/CommunityCar.Mvc/Views"
$areasPath = "src/CommunityCar.Mvc/Areas"

# Define mappings from inline styles to CSS classes
$styleMappings = @{
    'style="font-size: 0.75rem;"' = 'class="text-xs"'
    'style="font-size: 0.7rem;"' = 'class="text-micro"'
    'style="font-size: 0.65rem;"' = 'class="text-tiny"'
    'style="font-size: 0.6rem;"' = 'class="badge-xs"'
    'style="letter-spacing: 0.05em;"' = 'class="letter-spacing-wide"'
    'style="width: 32px; height: 32px;"' = 'class="size-32"'
    'style="width: 40px; height: 40px;"' = 'class="size-40"'
    'style="width: 45px; height: 45px;"' = 'class="size-45"'
    'style="height: 200px; object-fit: cover;"' = 'class="size-200 object-cover"'
    'style="height: 200px;"' = 'class="size-200"'
    'style="object-fit: cover;"' = 'class="object-cover"'
    'style="top: 80px; z-index: 1;"' = 'class="sticky-sidebar"'
    'style="display: none;"' = 'class="d-none"'
    'style="max-width: 250px;"' = 'class="max-w-250"'
    'style="max-width: 320px;"' = 'class="max-w-320"'
    'style="max-width: 350px;"' = 'class="max-w-350"'
    'style="width: 320px; max-height: 480px; overflow-y: auto;"' = 'class="dropdown-menu-lg max-h-480 overflow-y-auto"'
    'style="width: 350px; max-height: 500px; overflow-y: auto;"' = 'class="dropdown-menu-xl max-h-500 overflow-y-auto"'
    'style="min-width: 150px;"' = 'class="dropdown-menu-sm"'
    'style="font-size: 1.05rem; line-height: 1.7;"' = 'class="content-readable"'
    'style="border-radius: 12px; border: 2px solid #e9ecef; font-size: 1.05rem;"' = 'class="form-control rounded-xl content-readable"'
}

$totalReplacements = 0
$filesModified = 0

Write-Host "=== Extracting Inline Styles ===" -ForegroundColor Cyan
Write-Host ""

# Function to process files
function Process-Files {
    param (
        [string]$path
    )
    
    $files = Get-ChildItem -Path $path -Filter *.cshtml -Recurse
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        $originalContent = $content
        $fileReplacements = 0
        
        foreach ($style in $styleMappings.Keys) {
            if ($content -match [regex]::Escape($style)) {
                $content = $content -replace [regex]::Escape($style), $styleMappings[$style]
                $fileReplacements++
                $totalReplacements++
            }
        }
        
        if ($fileReplacements -gt 0) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
            Write-Host "✓ $relativePath ($fileReplacements replacements)" -ForegroundColor Green
            $filesModified++
        }
    }
}

# Process Views
if (Test-Path $viewsPath) {
    Process-Files -path $viewsPath
}

# Process Areas
if (Test-Path $areasPath) {
    Process-Files -path $areasPath
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Files modified: $filesModified" -ForegroundColor Yellow
Write-Host "Total replacements: $totalReplacements" -ForegroundColor Yellow
Write-Host ""
Write-Host "Done! All common inline styles have been replaced with CSS classes." -ForegroundColor Green
