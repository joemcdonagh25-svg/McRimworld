#Requires -Version 5.1
<#
  PullAndReinstall.ps1 -- one command to get the start-fix branch onto this machine
  and rebuild/install the Lantern Village DLL.

  Why this exists: Player.log fingerprints like
    "Press Next when ready"
    "Skipping room … too much water"
  mean RimWorld is still loading an OLD Assemblies\LanternVillage.dll (usually from
  main, or from a prior install that was never -Reinstall'd after git pull).

  Usage (from mods/lantern-village, PowerShell):
    .\PullAndReinstall.ps1

  What it does:
    1. Fetches origin
    2. Checks out cursor/lantern-skip-ideo-crash-42ec (PR #44)
    3. Pulls latest
    4. Runs .\Launch.ps1 -Reinstall  (rebuild + copy DLL + launch)

  After RimWorld loads, Player.log MUST contain:
    [Lantern Village] BUILD LV-START-FIX-2026-03-24a
  If that line is missing, the DLL under LocalLow Mods was not replaced — stop and
  check Install.ps1 output.
#>
[CmdletBinding()]
param(
    [string]$Branch = "cursor/lantern-skip-ideo-crash-42ec",
    [switch]$NoLaunch
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# McRimworld layout: <repo>/mods/lantern-village/PullAndReinstall.ps1 → git root is ../..
$GitRoot = $null
foreach ($candidate in @(
        (Join-Path $RepoRoot "../.."),
        (Join-Path $RepoRoot ".."),
        $RepoRoot
    )) {
    $resolved = Resolve-Path $candidate -ErrorAction SilentlyContinue
    if ($resolved -and (Test-Path (Join-Path $resolved.Path ".git"))) {
        $GitRoot = $resolved.Path
        break
    }
}
if (-not $GitRoot) {
    throw "Could not find a .git directory above $RepoRoot"
}

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }

Push-Location $GitRoot
try {
    Write-Step "Fetching origin"
    git fetch origin $Branch
    if ($LASTEXITCODE -ne 0) { throw "git fetch failed" }

    Write-Step "Checking out $Branch"
    $current = (git rev-parse --abbrev-ref HEAD).Trim()
    if ($current -ne $Branch) {
        git checkout $Branch
        if ($LASTEXITCODE -ne 0) { throw "git checkout $Branch failed — stash/commit local changes first" }
    }

    Write-Step "Pulling latest $Branch"
    git pull origin $Branch
    if ($LASTEXITCODE -ne 0) { throw "git pull failed" }

    $sha = (git rev-parse --short HEAD).Trim()
    Write-Host "    At $Branch @ $sha" -ForegroundColor Green
}
finally {
    Pop-Location
}

Write-Step "Reinstalling Lantern Village (rebuild DLL)"
Push-Location $RepoRoot
try {
    if ($NoLaunch) {
        & (Join-Path $RepoRoot "Install.ps1")
        if ($LASTEXITCODE -ne 0) { throw "Install.ps1 failed" }
    } else {
        & (Join-Path $RepoRoot "Launch.ps1") -Reinstall
        if ($LASTEXITCODE -ne 0) { throw "Launch.ps1 -Reinstall failed" }
    }
}
finally {
    Pop-Location
}

Write-Host "`nDone. In Player.log look for:" -ForegroundColor Green
Write-Host "  [Lantern Village] BUILD LV-START-FIX-2026-03-24a" -ForegroundColor Yellow
Write-Host "If you still see 'Press Next when ready' or 'Skipping room … water', the old DLL is still installed." -ForegroundColor Yellow
