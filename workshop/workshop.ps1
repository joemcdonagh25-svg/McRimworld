# McRimworld Workshop launcher entry point.
#
# Thin wrapper so the launcher has "a simple Windows entry point" without
# duplicating any logic: everything lives in the mcrimworld_workshop Python
# package next to this script.
#
# Examples:
#   .\workshop.ps1 env
#   .\workshop.ps1 list
#   .\workshop.ps1 inspect lantern-village
#   .\workshop.ps1 validate lantern-village
#   .\workshop.ps1 activate lantern-village
#   .\workshop.ps1 launch
#   .\workshop.ps1 restore

$ErrorActionPreference = "Stop"
# This script lives at workshop/workshop.ps1; the mcrimworld_workshop package
# is a sibling of this script, so run with workshop/ as the working directory.
$workshopDir = $PSScriptRoot

$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    $python = Get-Command py -ErrorAction SilentlyContinue
}
if (-not $python) {
    Write-Error "No Python interpreter found on PATH (tried 'python' and 'py'). Install Python 3.10+ to use the Workshop launcher."
}

Push-Location $workshopDir
try {
    & $python.Source -m mcrimworld_workshop @args
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
