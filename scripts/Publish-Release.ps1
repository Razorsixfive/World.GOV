$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
$releaseDirectory = Join-Path (Get-Location) ('.artifacts/releases/' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
dotnet restore World.GOV.sln
if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
dotnet build World.GOV.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
dotnet test World.GOV.sln --configuration Release --no-build
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
dotnet run --project src/World.GOV.Cli --configuration Release --no-build -- export --output src/World.GOV.Web/wwwroot/data --build-status succeeded
if ($LASTEXITCODE -ne 0) { throw 'Content publication failed.' }
dotnet publish src/World.GOV.Web --configuration Release --no-restore --output (Join-Path $releaseDirectory 'web')
if ($LASTEXITCODE -ne 0) { throw 'Web publication failed.' }
$siteDirectory = Join-Path $releaseDirectory 'web/wwwroot'
Copy-Item -LiteralPath (Join-Path $siteDirectory 'index.html') -Destination (Join-Path $siteDirectory '404.html')
Compress-Archive -Path (Join-Path $siteDirectory '*') -DestinationPath (Join-Path $releaseDirectory 'World.GOV-portal.zip')
Write-Host "Release created at $releaseDirectory. Serve the archive contents over HTTP; do not open index.html as a file."
