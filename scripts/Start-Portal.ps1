param(
    [ValidateRange(1024, 65535)]
    [int]$Port = 5255,
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
$portalUrl = "http://localhost:$Port"

$connection = New-Object System.Net.Sockets.TcpClient
try {
    $pendingConnection = $connection.ConnectAsync('localhost', $Port)
    $portInUse = $pendingConnection.Wait(1000) -and $connection.Connected
}
catch {
    $portInUse = $false
}
finally {
    $connection.Dispose()
}

if ($portInUse) {
    $existingManifest = $null
    try {
        $existingManifest = Invoke-RestMethod -Uri "$portalUrl/data/manifest.json" -UseBasicParsing -TimeoutSec 3
    }
    catch {
        # An occupied port may belong to an unrelated program.
    }
    if ($existingManifest.schemaVersion -eq 1 -and
        $existingManifest.datasets -contains 'countries.json' -and
        $existingManifest.datasets -contains 'timelines' -and
        $existingManifest.datasets -contains 'build-report.json') {
        Write-Host "World.GOV is already running at $portalUrl."
        Write-Host 'To rebuild and restart, stop the existing portal with Ctrl+C in its terminal first.'
        if (-not $NoBrowser) { Start-Process -FilePath $portalUrl }
        return
    }
    throw "Port $Port is already used by another program. Choose another port: powershell -NoProfile -File scripts/Start-Portal.ps1 -Port 5256"
}

dotnet build World.GOV.sln --configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
dotnet run --project src/World.GOV.Cli --configuration Release --no-build -- export --output src/World.GOV.Web/wwwroot/data --build-status succeeded
if ($LASTEXITCODE -ne 0) { throw 'Content publication failed.' }
Write-Host "Open $portalUrl in your browser. Press Ctrl+C to stop."
dotnet run --project src/World.GOV.Web --configuration Release --no-build --launch-profile http --urls $portalUrl
if ($LASTEXITCODE -ne 0) { throw 'Portal server stopped with an error.' }
