# PowerShell script to remove <style> blocks from views

$viewsPath = "src/CommunityCar.Mvc/Views"
$areasPath = "src/CommunityCar.Mvc/Areas"

$totalRemoved = 0
$filesModified = 0
$filesProcessed = 0

Write-Host "=== Removing Style Blocks from Views ===" -ForegroundColor Cyan
Write-Host ""

function Remove-StyleBlocks {
    param ([string]$path)
    
    if (-not (Test-Path $path)) {
        Write-Host "Path not found: $path" -ForegroundColor Yellow
        return
    }
    
    $files = Get-ChildItem -Path $path -Filter *.cshtml -Recurse
    
    foreach ($file in $files) {
        $script:filesProcessed++
        $content = Get-Content $file.FullName -Raw
        
        if ($null -eq $content) {
            continue
        }
        
        $originalContent = $content
        
        # Remove <style>...</style> blocks (including multiline)
        $pattern = '(?s)<style[^>]*>.*?</style>\s*'
        $matches = [regex]::Matches($content, $pattern)
        
        if ($matches.Count -gt 0) {
            $content = [regex]::Replace($content, $pattern, '')
            
            # Clean up extra blank lines
            $content = $content -replace '(\r?\n){3,}', "`r`n`r`n"
            
            Set-Content -Path $file.FullName -Value $content -NoNewline
            
            $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
            Write-Host "[OK] $relativePath - Removed $($matches.Count) style block(s)" -ForegroundColor Green
            
            $script:filesModified++
            $script:totalRemoved += $matches.Count
        }
    }
}

# Process Views
Write-Host "Processing Views..." -ForegroundColor Yellow
Remove-StyleBlocks -path $viewsPath

# Process Areas
Write-Host "Processing Areas..." -ForegroundColor Yellow
Remove-StyleBlocks -path $areasPath

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Files processed: $filesProcessed" -ForegroundColor White
Write-Host "Files modified: $filesModified" -ForegroundColor Yellow
Write-Host "Style blocks removed: $totalRemoved" -ForegroundColor Yellow
Write-Host ""

if ($totalRemoved -gt 0) {
    Write-Host "[SUCCESS] All style blocks have been removed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "The styles have been moved to:" -ForegroundColor Cyan
    Write-Host "- wwwroot/css/pages/badges.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/chats.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/guides.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/news.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/posts.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/reviews.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/groups.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/info.css" -ForegroundColor White
    Write-Host "- wwwroot/css/pages/qa.css" -ForegroundColor White
    Write-Host "- wwwroot/css/layout/sidebar.css" -ForegroundColor White
} else {
    Write-Host "No style blocks found to remove." -ForegroundColor White
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Test the application to ensure styles are applied correctly" -ForegroundColor White
Write-Host "2. Check that all pages look the same as before" -ForegroundColor White
Write-Host "3. Verify dark mode works correctly" -ForegroundColor White
