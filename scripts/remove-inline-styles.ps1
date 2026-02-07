# PowerShell script to remove inline styles from views

$viewsPath = "src/CommunityCar.Mvc/Views"
$areasPath = "src/CommunityCar.Mvc/Areas"

# Function to remove style blocks from files
function Remove-StyleBlocks {
    param (
        [string]$filePath
    )
    
    $content = Get-Content $filePath -Raw
    
    # Remove <style>...</style> blocks
    $content = $content -replace '(?s)<style>.*?</style>\s*', ''
    
    # Save the file
    Set-Content -Path $filePath -Value $content -NoNewline
    
    Write-Host "Processed: $filePath"
}

# Process all .cshtml files
Get-ChildItem -Path $viewsPath -Filter *.cshtml -Recurse | ForEach-Object {
    Remove-StyleBlocks -filePath $_.FullName
}

Get-ChildItem -Path $areasPath -Filter *.cshtml -Recurse | ForEach-Object {
    Remove-StyleBlocks -filePath $_.FullName
}

Write-Host "Done! All inline style blocks removed."
