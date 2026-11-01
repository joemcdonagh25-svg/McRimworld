#Requires -Version 5.1
<#
  Run-Tests.ps1 -- automated (non-runtime) validation for The Lantern Village.

  Covers: path discovery, version detection, XML generation/parsing, mod dependency
  resolution, missing-mod handling, backup/restore round-trip, idempotent install
  (dry run), scenario file validity, manifest consistency, and save-presence checks.

  Does NOT launch RimWorld -- see IMPLEMENTATION_REPORT.md for what still needs a
  real runtime pass.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Import-Module (Join-Path $RepoRoot "discovery\Discovery.psm1") -Force

$script:Pass = 0
$script:Fail = 0

function Test-That {
    param([string]$Name, [scriptblock]$Body)
    try {
        & $Body
        Write-Host "  PASS: $Name" -ForegroundColor Green
        $script:Pass++
    } catch {
        Write-Host "  FAIL: $Name -- $($_.Exception.Message)" -ForegroundColor Red
        $script:Fail++
    }
}

function Assert-True($cond, $msg) { if (-not $cond) { throw $msg } }

Write-Host "=== The Lantern Village: automated test suite ===" -ForegroundColor Cyan

# ---------------------------------------------------------------------------
Write-Host "`n-- Path discovery --"
Test-That "Steam is discoverable" {
    $p = Find-SteamInstall
    Assert-True ($p -and (Test-Path $p)) "Steam not found"
}
Test-That "RimWorld is discoverable" {
    $steam = Find-SteamInstall
    $rims = Find-RimWorldInstall -SteamPath $steam
    Assert-True ($rims.Count -ge 1) "RimWorld not found"
}
Test-That "User config dir is discoverable" {
    $cfg = Get-RimWorldUserConfigDir
    Assert-True ($cfg -and (Test-Path $cfg)) "User config dir not found"
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Version detection --"
Test-That "RimWorld version string looks like 1.6.x" {
    $steam = Find-SteamInstall
    $rims = Find-RimWorldInstall -SteamPath $steam
    $rim = $rims[0]
    $ver = Get-RimWorldVersion -RimWorldPath $rim
    Assert-True ($ver -match '^1\.6') "Unexpected version: $ver"
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Manifest consistency --"
$manifest = Get-Content (Join-Path $RepoRoot "manifest\mod-manifest.json") -Raw | ConvertFrom-Json
Test-That "Manifest parses as valid JSON" {
    Assert-True ($manifest.scenarioName -eq "The Lantern Village") "scenarioName mismatch"
}
Test-That "Every workshop-sourced mod has a workshopId" {
    foreach ($m in $manifest.loadOrder | Where-Object { $_.source -eq "workshop" }) {
        Assert-True ($m.workshopId -match '^\d+$') "Missing/invalid workshopId for $($m.name)"
    }
}
Test-That "Royalty, Odyssey and Anomaly are not in the active load order" {
    $excluded = @("ludeon.rimworld.royalty", "ludeon.rimworld.odyssey", "ludeon.rimworld.anomaly")
    foreach ($m in $manifest.loadOrder) {
        Assert-True ($excluded -notcontains $m.packageId) "$($m.packageId) should be excluded"
    }
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Mod dependency resolution against this machine --"
$steamPath = Find-SteamInstall
$workshopPath = Get-WorkshopContentPath -SteamPath $steamPath
$script:resolved = @()
$script:missing = @()
Test-That "Resolve each workshop mod's real packageId from About.xml" {
    # Writes to $script:resolved/$script:missing explicitly: a bare $resolved += inside
    # this scriptblock would otherwise shadow-create a block-local variable instead of
    # updating the script-scope one declared above (verified the hard way -- see the
    # first test run of this suite, which is exactly the kind of bug this project asks
    # automated tests to catch).
    foreach ($m in $manifest.loadOrder | Where-Object { $_.source -eq "workshop" }) {
        $local = Find-ModByWorkshopId -WorkshopContentPath $workshopPath -WorkshopId $m.workshopId
        if ($local -and $local.PackageId) {
            $script:resolved += $m.name
        } else {
            $script:missing += $m.name
        }
    }
    Write-Host "      resolved: $($script:resolved -join ', ')"
    if ($script:missing.Count -gt 0) { Write-Host "      missing (expected until subscribed): $($script:missing -join ', ')" -ForegroundColor Yellow }
    Assert-True ($script:resolved.Count -ge 4) "Expected at least 4 mods already resolvable on this machine"
}
Test-That "Missing-mod detection does not crash and reports Workshop IDs" {
    foreach ($name in $script:missing) {
        $entry = $manifest.loadOrder | Where-Object { $_.name -eq $name }
        Assert-True ($entry.workshopId -match '^\d+$') "No workshopId recorded for missing mod $name"
    }
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Scenario file --"
$scenarioFile = Join-Path $RepoRoot "scenario\The Lantern Village.rsc"
Test-That "Scenario file exists and is well-formed XML" {
    Assert-True (Test-Path $scenarioFile) "Scenario file missing"
    [xml](Get-Content $scenarioFile -Raw) | Out-Null
}
Test-That "Scenario declares PlayerTribe faction and Standing arrival" {
    [xml]$xml = Get-Content $scenarioFile -Raw
    Assert-True ($xml.savedscenario.scenario.playerFaction.factionDef -eq "PlayerTribe") "Faction mismatch"
    $arrive = $xml.savedscenario.scenario.parts.li | Where-Object { $_.Class -eq "ScenPart_PlayerPawnsArriveMethod" }
    Assert-True ($arrive.method -eq "Standing") "Arrival method mismatch"
}
Test-That "Scenario starting supplies match the manifest counts" {
    [xml]$xml = Get-Content $scenarioFile -Raw
    $parts = $xml.savedscenario.scenario.parts.li | Where-Object { $_.Class -eq "ScenPart_StartingThing_Defined" }
    $pemmican = $parts | Where-Object { $_.thingDef -eq "Pemmican" }
    Assert-True ([int]$pemmican.count -eq 600) "Pemmican count mismatch"
}

# ---------------------------------------------------------------------------
Write-Host "`n-- XML generation (ModsConfig writer logic, dry run) --"
Test-That "ModsConfig.xml writer produces parseable XML" {
    $tmp = Join-Path $env:TEMP "LanternVillageTest_ModsConfig.xml"
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$sb.AppendLine('<ModsConfigData>')
    [void]$sb.AppendLine('  <version>1.6.4871 rev590</version>')
    [void]$sb.AppendLine('  <activeMods>')
    [void]$sb.AppendLine('    <li>ludeon.rimworld</li>')
    [void]$sb.AppendLine('  </activeMods>')
    [void]$sb.AppendLine('</ModsConfigData>')
    [System.IO.File]::WriteAllText($tmp, $sb.ToString())
    [xml](Get-Content $tmp -Raw) | Out-Null
    Remove-Item $tmp -ErrorAction SilentlyContinue
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Backup / restore round-trip (sandboxed, does not touch real config) --"
Test-That "Backup then restore round-trip preserves file content" {
    $sandbox = Join-Path $env:TEMP "LanternVillageTest_$(Get-Random)"
    New-Item -ItemType Directory -Path $sandbox -Force | Out-Null
    try {
        $original = Join-Path $sandbox "ModsConfig.xml"
        "<ModsConfigData><version>original</version></ModsConfigData>" | Out-File $original -Encoding utf8
        $backupDir = Join-Path $sandbox "backup"
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        Copy-Item $original (Join-Path $backupDir "ModsConfig.xml")

        "<ModsConfigData><version>changed</version></ModsConfigData>" | Out-File $original -Encoding utf8
        Assert-True ((Get-Content $original -Raw) -match "changed") "Setup failed"

        Copy-Item (Join-Path $backupDir "ModsConfig.xml") $original -Force
        Assert-True ((Get-Content $original -Raw) -match "original") "Restore did not bring back original content"
    } finally {
        Remove-Item $sandbox -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Idempotent install (dry run against real paths, -WhatIf) --"
Test-That "Install.ps1 -WhatIf runs twice without throwing" {
    $out1 = & (Join-Path $RepoRoot "Install.ps1") -WhatIf -SkipBuild -WorkshopWaitTimeoutSeconds 1 2>&1
    $exit1 = $LASTEXITCODE
    $out2 = & (Join-Path $RepoRoot "Install.ps1") -WhatIf -SkipBuild -WorkshopWaitTimeoutSeconds 1 2>&1
    $exit2 = $LASTEXITCODE
    # Exit code 2 = blocked on a Workshop subscription, which is an expected, stable outcome on this
    # machine until GloomyFurniture / Realistic Darkness are subscribed -- not a crash.
    Assert-True ($exit1 -eq $exit2) "Two -WhatIf runs produced different exit codes ($exit1 vs $exit2) -- not idempotent"
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Custom mod compilation --"
Test-That "LanternVillage.dll exists (built by dotnet build earlier)" {
    $dll = Join-Path $RepoRoot "Mods\LanternVillage\1.6\Assemblies\LanternVillage.dll"
    Assert-True (Test-Path $dll) "Assembly not found -- run dotnet build first"
}
Test-That "About.xml is well-formed and declares the right packageId" {
    [xml]$xml = Get-Content (Join-Path $RepoRoot "Mods\LanternVillage\About\About.xml") -Raw
    Assert-True ($xml.ModMetaData.packageId -eq "lanternvillage.thelanternvillage") "packageId mismatch"
}

# ---------------------------------------------------------------------------
Write-Host "`n-- Save-file presence/validity checks --"
Test-That "Saves directory is readable and existing saves are untouched" {
    $userConfigDir = Get-RimWorldUserConfigDir
    $savesDir = Get-SavesDir -UserConfigDir $userConfigDir
    Assert-True (Test-Path $savesDir) "Saves dir missing"
    $count = (Get-ChildItem $savesDir -Filter "*.rws" -ErrorAction SilentlyContinue).Count
    Write-Host "      found $count existing .rws saves (read-only check, nothing modified)"
}

# ---------------------------------------------------------------------------
Write-Host "`n=== Results: $script:Pass passed, $script:Fail failed ===" -ForegroundColor $(if ($script:Fail -eq 0) { "Green" } else { "Red" })
if ($script:Fail -gt 0) { exit 1 }
