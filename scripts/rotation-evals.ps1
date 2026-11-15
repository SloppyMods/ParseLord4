$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$Adapter = "C:\Users\you\Documents\Projects\quality-gate\adapters\parselord4-rotation.ps1"

if (-not (Test-Path -LiteralPath $Adapter)) {
    Write-Error "Missing ParseLord4 eval adapter: $Adapter"
    exit 1
}

& powershell -NoProfile -ExecutionPolicy Bypass -File $Adapter -RepoRoot $RepoRoot
exit $LASTEXITCODE
