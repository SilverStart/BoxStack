param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.73f1\Editor\Unity.exe",
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")),
    [string]$OutputPath = "webgl",
    [switch]$Development,
    [switch]$SkipUnityBuild,
    [switch]$SkipAitPublicSync
)

$ErrorActionPreference = "Stop"

$ResolvedProjectRoot = Resolve-Path -LiteralPath $ProjectRoot
$OutputFullPath = Join-Path $ResolvedProjectRoot $OutputPath
$LogsDir = Join-Path $ResolvedProjectRoot "Temp\build-logs"
$LogPath = Join-Path $LogsDir "webgl-build.log"

if (-not $SkipUnityBuild -and -not (Test-Path -LiteralPath $UnityPath -PathType Leaf)) {
    throw "Unity executable not found: $UnityPath"
}

New-Item -ItemType Directory -Force -Path $LogsDir | Out-Null

Write-Host "Project: $($ResolvedProjectRoot.Path)"
Write-Host "Output: $OutputFullPath"

if (-not $SkipUnityBuild) {
    $UnityArgs = @(
        "-batchmode",
        "-quit",
        "-projectPath", $ResolvedProjectRoot.Path,
        "-executeMethod", "BoxStackWebGlBuilder.Build",
        "-outputPath", $OutputFullPath,
        "-logFile", $LogPath
    )

    if ($Development) {
        $UnityArgs += "-development"
    }

    Write-Host "Unity: $UnityPath"
    Write-Host "Log: $LogPath"

    $UnityProcess = Start-Process -FilePath $UnityPath -ArgumentList $UnityArgs -Wait -PassThru -NoNewWindow
    $ExitCode = $UnityProcess.ExitCode

    if ($ExitCode -ne 0) {
        Write-Host ""
        Write-Host "Unity WebGL build failed with exit code $ExitCode."
        Write-Host "See log: $LogPath"
        exit $ExitCode
    }
}
else {
    Write-Host "Skipping Unity build. Syncing existing WebGL output."
}

if (-not $SkipAitPublicSync) {
    $AitPublic = Join-Path $ResolvedProjectRoot "ait-build\public"
    $AitIndex = Join-Path $ResolvedProjectRoot "ait-build\index.html"
    New-Item -ItemType Directory -Force -Path $AitPublic | Out-Null

    foreach ($Name in @("Build", "TemplateData", "Runtime")) {
        $Source = Join-Path $OutputFullPath $Name
        if (Test-Path -LiteralPath $Source) {
            Copy-Item -LiteralPath $Source -Destination $AitPublic -Recurse -Force
        }
    }

    if (Test-Path -LiteralPath $AitIndex -PathType Leaf) {
        $BuildPath = Join-Path $OutputFullPath "Build"
        $Loader = Get-ChildItem -LiteralPath $BuildPath -Filter "*.loader.js" | Select-Object -First 1
        $Data = Get-ChildItem -LiteralPath $BuildPath -Filter "*.data" | Select-Object -First 1
        $Framework = Get-ChildItem -LiteralPath $BuildPath -Filter "*.framework.js" | Select-Object -First 1
        $Wasm = Get-ChildItem -LiteralPath $BuildPath -Filter "*.wasm" | Select-Object -First 1

        if (-not ($Loader -and $Data -and $Framework -and $Wasm)) {
            throw "Could not find all WebGL Build files under: $BuildPath"
        }

        $IndexHtml = Get-Content -LiteralPath $AitIndex -Raw
        $IndexHtml = $IndexHtml -replace 'Build/[^"''\]]+\.loader\.js', "Build/$($Loader.Name)"
        $IndexHtml = $IndexHtml -replace 'Build/[^"''\]]+\.data', "Build/$($Data.Name)"
        $IndexHtml = $IndexHtml -replace 'Build/[^"''\]]+\.framework\.js', "Build/$($Framework.Name)"
        $IndexHtml = $IndexHtml -replace 'Build/[^"''\]]+\.wasm', "Build/$($Wasm.Name)"
        Set-Content -LiteralPath $AitIndex -Value $IndexHtml -NoNewline

        Write-Host "Updated AIT index file references:"
        Write-Host "  $($Loader.Name)"
        Write-Host "  $($Data.Name)"
        Write-Host "  $($Framework.Name)"
        Write-Host "  $($Wasm.Name)"
    }

    Write-Host "Synced WebGL runtime folders to: $AitPublic"
}

Write-Host "WebGL build completed."
