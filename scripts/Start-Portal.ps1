$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
dotnet build World.GOV.sln --configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
dotnet run --project src/World.GOV.Cli --configuration Release --no-build -- export --output src/World.GOV.Web/wwwroot/data --build-status succeeded
if ($LASTEXITCODE -ne 0) { throw 'Content publication failed.' }
Write-Host 'Open http://localhost:5255 in your browser. Press Ctrl+C to stop.'
dotnet run --project src/World.GOV.Web --configuration Release --no-build --launch-profile http
if ($LASTEXITCODE -ne 0) { throw 'Portal server stopped with an error.' }
