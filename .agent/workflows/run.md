---
description: how to run the application
---
// turbo
1. Stop any existing dotnet processes and run the app:
```powershell
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet run --project "src/CommunityCar.Mvc/CommunityCar.Mvc.csproj" --urls "http://localhost:5010"
```
2. Open the application in your browser:
[http://localhost:5010](http://localhost:5010)
