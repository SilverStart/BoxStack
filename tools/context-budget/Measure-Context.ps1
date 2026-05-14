param(
    [string[]]$Path,
    [string]$Label = "context",
    [string]$BaselinePath,
    [double]$CharsPerToken = 3.5
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Measure-Text {
    param(
        [string]$Name,
        [string]$Text,
        [double]$CharsPerToken
    )

    $lineCount = 0
    if ($Text.Length -gt 0) {
        $lineCount = ($Text -split "`r?`n").Count
    }

    $charCount = $Text.Length
    $estimatedTokens = [math]::Ceiling($charCount / $CharsPerToken)

    [pscustomobject]@{
        Label = $Name
        Lines = $lineCount
        Characters = $charCount
        EstimatedTokens = $estimatedTokens
    }
}

function Read-AllText {
    param([string]$InputPath)

    $resolvedPath = Resolve-Path -LiteralPath $InputPath
    [System.IO.File]::ReadAllText($resolvedPath.Path)
}

$measurements = New-Object System.Collections.Generic.List[object]

if ($Path -and $Path.Count -gt 0) {
    foreach ($inputPath in $Path) {
        $text = Read-AllText -InputPath $inputPath
        $measurements.Add((Measure-Text -Name $inputPath -Text $text -CharsPerToken $CharsPerToken))
    }
}
else {
    $pipelineLines = @($input)
    $stdinText = ""
    if ($pipelineLines.Count -gt 0) {
        $stdinText = $pipelineLines -join [Environment]::NewLine
    }
    else {
        $stdinText = [Console]::In.ReadToEnd()
    }

    $measurements.Add((Measure-Text -Name $Label -Text $stdinText -CharsPerToken $CharsPerToken))
}

if ($BaselinePath) {
    $baselineText = Read-AllText -InputPath $BaselinePath
    $baseline = Measure-Text -Name "baseline: $BaselinePath" -Text $baselineText -CharsPerToken $CharsPerToken
    $measurements.Insert(0, $baseline)

    $baselineTokens = [double]$baseline.EstimatedTokens
    foreach ($measurement in $measurements) {
        $savedTokens = $baselineTokens - [double]$measurement.EstimatedTokens
        $savedPercent = 0.0
        if ($baselineTokens -gt 0) {
            $savedPercent = ($savedTokens / $baselineTokens) * 100.0
        }

        $measurement | Add-Member -NotePropertyName "SavedTokensVsBaseline" -NotePropertyValue ([math]::Round($savedTokens, 0))
        $measurement | Add-Member -NotePropertyName "SavedPercentVsBaseline" -NotePropertyValue ([math]::Round($savedPercent, 1))
    }
}

$measurements | Format-Table -AutoSize
