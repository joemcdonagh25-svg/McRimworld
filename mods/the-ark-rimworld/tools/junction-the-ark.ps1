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

$oldDuplicate = Join-Path $RimWorldMods "TheArk"
if (Test-Path $oldDuplicate) {
    Write-Host ""
    Write-Host "WARNING: Old duplicate mod folder still exists:"
    Write-Host "  $oldDuplicate"
    Write-Host "RimWorld logs: Tried loading mod with the same packageId multiple times: joemcdonagh.theark"
    Write-Host "Rename or delete that OLD folder. Keep only: $link"
}

Write-Host ""
Write-Host "OK. Junction created:"
Write-Host "  $link"
Write-Host "  -> $target"
Write-Host ""
Write-Host "Next:"
Write-Host "  1) DELETE or rename Mods\TheArk if it exists (old duplicate)"
Write-Host "  2) Open RimWorld -> Mods"
Write-Host "  3) Enable 'The Ark' (joemcdonagh.theark) + Biotech"
Write-Host "  4) Restart RimWorld"
Write-Host "  5) Player.log should contain LOADED and NO ConfigurePawns ConfigError"
Write-Host "  6) New Game -> The Ark"
