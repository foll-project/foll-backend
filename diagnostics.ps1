# diagnostics.ps1 - diagnóstico rápido para problemas de NuGet / OmniSharp
Write-Host "== DOTNET INFO =="
dotnet --info

Write-Host "`n== INSTALLED SDKS =="
dotnet --list-sdks

Write-Host "`n== CLEAN =="
dotnet clean -v minimal

Write-Host "`n== RESTORE =="
dotnet restore -v minimal

Write-Host "`n== BUILD =="
dotnet build -v minimal

Write-Host "`n== PROJECT ASSETS (if exists) looking for Microsoft.OpenApi entries =="
$assets = Join-Path -Path "obj" -ChildPath "project.assets.json"
if (Test-Path $assets) {
    Select-String -Path $assets -Pattern "Microsoft.OpenApi" -SimpleMatch | Select-Object -First 50
} else {
    Write-Host "project.assets.json not found (restore likely failed)"
}

Write-Host "`n== END =="

Write-Host "If the build fails, copy the full output and paste here. Also restart VS Code and restart OmniSharp (Command Palette -> Restart OmniSharp)."
