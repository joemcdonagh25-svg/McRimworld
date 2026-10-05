# Creates/updates a Windows junction so RimWorld loads Colony Time from McRimworld.
# Run in PowerShell (ok if not Admin for junctions on the same drive).
#
# Success looks like: a Mods\colony-time folder that contains About\ and Assemblies\.

param(
    [string]$McRimworldRoot = "C:\McRimworld",
    [string]$RimWorldMods = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods"
)

$ErrorActionPreference = "Stop"

$target = Join-Path $McRimworldRoot "mods\colony-time"
$link = Join-Path $RimWorldMods "colony-time"

if (-not (Test-Path (Join-Path $target "About\About.xml"))) {
    Write-Error @"
Target missing About.xml: $target

Colony Time is not on main yet — you need the PR branch locally first:

  cd $McRimworldRoot
  git fetch origin
  git checkout cursor/colony-time-v0-25e9

Or pass -McRimworldRoot if your clone lives elsewhere.
"@
}

if (-not (Test-Path (Join-Path $target "Assemblies\ColonyTime.dll"))) {
    Write-Error "Target missing Assemblies\ColonyTime.dll: $target`nPull/rebuild the mod first."
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
Write-Host "  1) Confirm this folder exists and contains About\About.xml:"
Write-Host "       $link"
Write-Host "  2) Open RimWorld -> Mods"
Write-Host "  3) Enable Harmony (brrainz.harmony), then Colony Time"
Write-Host "  4) Restart RimWorld"
Write-Host "  5) Player.log should contain: [Colony Time] Initialized."
Write-Host "  6) Load Game — each save should show Played / Colony lines"
