# Creates/updates a Windows junction so RimWorld loads The Ark from McRimworld.
# Run in PowerShell (ok if not Admin for junctions on the same drive).
#
# Success looks like: a Mods\the-ark-rimworld folder that contains About\ and Defs\.

param(
    [string]$McRimworldRoot = "C:\McRimworld",
    [string]$RimWorldMods = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods"
)

$ErrorActionPreference = "Stop"

$target = Join-Path $McRimworldRoot "mods\the-ark-rimworld"
$link = Join-Path $RimWorldMods "the-ark-rimworld"

if (-not (Test-Path (Join-Path $target "About\About.xml"))) {
    Write-Error "Target missing About.xml: $target`nPull McRimworld first, or pass -McRimworldRoot."
}

if (-not (Test-Path (Join-Path $target "Defs\Scenarios\TheArkScenario.xml"))) {
    Write-Error "Target missing scenario defs: $target\Defs\Scenarios\TheArkScenario.xml"
}

if (-not (Test-Path $RimWorldMods)) {
    Write-Error "RimWorld Mods folder not found: $RimWorldMods`nPass -RimWorldMods if your Steam path differs."
}

if (Test-Path $link) {
    $item = Get-Item $link -Force
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        Write-Host "Removing existing junction: $link"
        cmd /c rmdir "$link"
    }
    else {
        Write-Error "Path exists and is NOT a junction (refusing to delete): $link`nMove/rename that folder, then re-run."
    }
}

cmd /c mklink /J "$link" "$target"
if ($LASTEXITCODE -ne 0) {
    Write-Error "mklink failed with exit $LASTEXITCODE"
}

Write-Host ""
Write-Host "OK. Junction created:"
Write-Host "  $link"
Write-Host "  -> $target"
Write-Host ""
Write-Host "Next:"
Write-Host "  1) Open RimWorld -> Mods"
Write-Host "  2) Enable 'The Ark' (joemcdonagh.theark)"
Write-Host "  3) Restart RimWorld"
Write-Host "  4) Player.log should contain: ScenarioDef TheArk_Playtest LOADED"
Write-Host "  5) New Game -> The Ark"
