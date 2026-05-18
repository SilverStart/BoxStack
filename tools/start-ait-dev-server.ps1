param(
    [string]$HostName = "0.0.0.0",
    [int]$Port = 5173,
    [switch]$UsePathPnpm,
    [switch]$CheckOnly
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")
$AitBuildRoot = Join-Path $ProjectRoot "ait-build"

if (-not (Test-Path -LiteralPath $AitBuildRoot -PathType Container)) {
    throw "AIT build folder not found: $AitBuildRoot"
}

if (-not (Test-Path -LiteralPath (Join-Path $AitBuildRoot "package.json") -PathType Leaf)) {
    throw "AIT package.json not found under: $AitBuildRoot"
}

$PnpmCandidates = @()

if (-not $UsePathPnpm -and $env:LOCALAPPDATA) {
    $PnpmCandidates += Join-Path $env:LOCALAPPDATA ".ait-unity-sdk\nodejs\v24.13.0\win-x64\pnpm.cmd"
}

$PathPnpm = Get-Command "pnpm.cmd" -ErrorAction SilentlyContinue
if ($PathPnpm) {
    $PnpmCandidates += $PathPnpm.Source
}

$PathPnpmNoExtension = Get-Command "pnpm" -ErrorAction SilentlyContinue
if ($PathPnpmNoExtension) {
    $PnpmCandidates += $PathPnpmNoExtension.Source
}

$Pnpm = $PnpmCandidates |
    Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) } |
    Select-Object -First 1

if (-not $Pnpm) {
    throw "pnpm was not found. Install the AIT SDK node runtime or add pnpm to PATH."
}

$env:AIT_VITE_HOST = $HostName
$env:AIT_VITE_PORT = [string]$Port

Write-Host "AIT dev server root: $AitBuildRoot"
Write-Host "pnpm: $Pnpm"
Write-Host "Host: $HostName"
Write-Host "Port: $Port"

if ($CheckOnly) {
    Write-Host "CheckOnly passed. Server was not started."
    exit 0
}

Write-Host ""
Write-Host "Local URL:   http://localhost:$Port/index.html"
Write-Host "Phone URL:   use the Network URL printed by Vite, for example http://<PC-LAN-IP>:$Port/index.html"
Write-Host ""
Write-Host "Keep this PowerShell window open while testing. Rebuild WebGL in Unity, then refresh the browser."
Write-Host ""

Push-Location $AitBuildRoot
try {
    & $Pnpm "vite" "--host" $HostName "--port" ([string]$Port) "--clearScreen" "false"
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
