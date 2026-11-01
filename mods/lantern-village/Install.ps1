#Requires -Version 5.1
<#
  Install.ps1 -- The Lantern Village installer.

  Discovers Steam/RimWorld, verifies/resolves the required mod list, backs up the
  user's existing mod configuration and scenario folder, activates the Lantern
  Village mod profile, builds the custom mod assembly, and installs the scenario.

  Safe to re-run (idempotent): re-running always produces the same end state and
  always backs up whatever was live immediately beforehand.

  This script makes real changes to your RimWorld configuration (ModsConfig.xml,
  Mods folder, Scenarios folder). It will not run anything silently -- read the
  prompts.
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [switch]$WhatIf,
    [int]$WorkshopWaitTimeoutSeconds = 180
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module (Join-Path $RepoRoot "discovery\Discovery.psm1") -Force

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg) { Write-Host "    OK: $msg" -ForegroundColor Green }
function Write-Warn2($msg) { Write-Host "    WARNING: $msg" -ForegroundColor Yellow }
function Write-Fail($msg) { Write-Host "    FAILED: $msg" -ForegroundColor Red }

$manifest = Get-Content (Join-Path $RepoRoot "manifest\mod-manifest.json") -Raw | ConvertFrom-Json

# ---------------------------------------------------------------------------
Write-Step "Discovering Steam and RimWorld"
$steamPath = Find-SteamInstall
if (-not $steamPath) { Write-Fail "Steam not found."; exit 1 }
Write-Ok "Steam: $steamPath"

$rimWorldInstalls = Find-RimWorldInstall -SteamPath $steamPath
# Deliberately not "@(Find-RimWorldInstall ...)" -- wrapping a direct function call whose
# own body already forces array output via Write-Output -NoEnumerate double-nests the
# result (the @() pipeline-collects the single NoEnumerate object into another array).
# Plain assignment binds $rimWorldInstalls straight to the array. Caught by the test suite.
if ($rimWorldInstalls.Count -eq 0) { Write-Fail "RimWorld (app 294100) not found in any Steam library."; exit 1 }
if ($rimWorldInstalls.Count -gt 1) {
    Write-Warn2 "Multiple RimWorld installations found:"
    $rimWorldInstalls | ForEach-Object { Write-Host "      $_" }
}
$rimWorldPath = $rimWorldInstalls[0]
Write-Ok "RimWorld: $rimWorldPath"

$version = Get-RimWorldVersion -RimWorldPath $rimWorldPath
Write-Ok "Version: $version"
if ($version -notmatch '^1\.6') {
    Write-Warn2 "This installer was built and verified against RimWorld 1.6. Detected version: $version"
}

$acf = Get-RimWorldAppManifest -SteamPath $steamPath -RimWorldPath $rimWorldPath
$ownedDlc = @()
if ($acf) { $ownedDlc = Get-OwnedDlc -AcfContent $acf }
Write-Ok "Owned DLC: $($ownedDlc -join ', ')"
foreach ($req in $manifest.requiredDlc) {
    if ($ownedDlc -notcontains $req.name) {
        Write-Fail "Required DLC not owned: $($req.name)"
        exit 1
    }
}
Write-Ok "Required DLC present: $($manifest.requiredDlc.name -join ', ')"

$workshopPath = Get-WorkshopContentPath -SteamPath $steamPath
$userConfigDir = Get-RimWorldUserConfigDir
if (-not $userConfigDir) { Write-Fail "RimWorld user config directory not found. Has the game been run at least once?"; exit 1 }
$modsConfigPath = Get-ModsConfigPath -UserConfigDir $userConfigDir
$scenariosDir = Get-ScenariosDir -UserConfigDir $userConfigDir
$savesDir = Get-SavesDir -UserConfigDir $userConfigDir
$localModsDir = Get-LocalModsDir -RimWorldPath $rimWorldPath
Write-Ok "User config dir: $userConfigDir"
Write-Ok "Saves found: $((Get-ChildItem $savesDir -Filter '*.rws' -ErrorAction SilentlyContinue).Count) (will not be touched)"

# ---------------------------------------------------------------------------
Write-Step "Resolving required mods"
$resolvedMods = @()
$blockedMods = @()
foreach ($entry in $manifest.loadOrder) {
    if ($entry.source -in @("core", "dlc")) {
        $resolvedMods += [PSCustomObject]@{ PackageId = $entry.packageId; Name = $entry.name; Source = $entry.source }
        continue
    }
    $local = Find-ModByWorkshopId -WorkshopContentPath $workshopPath -WorkshopId $entry.workshopId
    if ($local -and $local.PackageId) {
        Write-Ok "$($entry.name) [$($entry.workshopId)] -> packageId '$($local.PackageId)'"
        $resolvedMods += [PSCustomObject]@{ PackageId = $local.PackageId; Name = $entry.name; Source = "workshop" }
    } else {
        Write-Warn2 "$($entry.name) [$($entry.workshopId)] is not subscribed/downloaded yet."
        $blockedMods += $entry
    }
}

if ($blockedMods.Count -gt 0) {
    Write-Step "Workshop subscription required (explicit action needed from you)"
    foreach ($m in $blockedMods) {
        $url = "steam://url/CommunityFilePage/$($m.workshopId)"
        Write-Host "    Opening Steam to subscribe to '$($m.name)': $url"
        if (-not $WhatIf) { Start-Process $url }
    }
    Write-Host "    Waiting up to $WorkshopWaitTimeoutSeconds seconds for the subscribed files to download..."
    $deadline = (Get-Date).AddSeconds($WorkshopWaitTimeoutSeconds)
    while ((Get-Date) -lt $deadline -and $blockedMods.Count -gt 0) {
        Start-Sleep -Seconds 5
        $stillBlocked = @()
        foreach ($m in $blockedMods) {
            $local = Find-ModByWorkshopId -WorkshopContentPath $workshopPath -WorkshopId $m.workshopId
            if ($local -and $local.PackageId) {
                Write-Ok "$($m.name) downloaded -> packageId '$($local.PackageId)'"
                $resolvedMods += [PSCustomObject]@{ PackageId = $local.PackageId; Name = $m.name; Source = "workshop" }
            } else {
                $stillBlocked += $m
            }
        }
        $blockedMods = $stillBlocked
    }
    if ($blockedMods.Count -gt 0) {
        Write-Fail "Still missing after waiting: $($blockedMods.Name -join ', ')"
        Write-Host "    Subscribe to these in Steam, let them finish downloading, then re-run Install.ps1."
        Write-Host "    (Re-running is safe -- already-resolved mods will not be re-prompted.)"
        exit 2
    }
}

$resolvedMods += [PSCustomObject]@{ PackageId = "lanternvillage.thelanternvillage"; Name = "The Lantern Village"; Source = "local" }

# ---------------------------------------------------------------------------
Write-Step "Backing up existing configuration"
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupDir = Join-Path $RepoRoot "Backups\$stamp"
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

if (Test-Path $modsConfigPath) {
    Copy-Item $modsConfigPath (Join-Path $backupDir "ModsConfig.xml")
    Write-Ok "Backed up ModsConfig.xml -> $backupDir\ModsConfig.xml"
}
if (Test-Path $scenariosDir) {
    Copy-Item $scenariosDir (Join-Path $backupDir "Scenarios") -Recurse
    Write-Ok "Backed up Scenarios folder -> $backupDir\Scenarios"
}
$existingLvMod = Join-Path $localModsDir "LanternVillage"
if (Test-Path $existingLvMod) {
    Copy-Item $existingLvMod (Join-Path $backupDir "LanternVillage-Mod") -Recurse
    Write-Ok "Backed up existing LanternVillage mod folder"
}
"$modsConfigPath" | Out-File (Join-Path $backupDir "restore-target-modsconfig.txt") -Encoding utf8
Write-Ok "Backup complete: $backupDir"

# ---------------------------------------------------------------------------
Write-Step "Writing ModsConfig.xml (Lantern Village profile)"
if ($WhatIf) {
    Write-Host "    [WhatIf] Would activate: $($resolvedMods.PackageId -join ', ')"
} else {
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$sb.AppendLine('<ModsConfigData>')
    [void]$sb.AppendLine("  <version>$version</version>")
    [void]$sb.AppendLine('  <activeMods>')
    foreach ($m in $resolvedMods) {
        [void]$sb.AppendLine("    <li>$($m.PackageId)</li>")
    }
    [void]$sb.AppendLine('  </activeMods>')
    [void]$sb.AppendLine('  <knownExpansions>')
    [void]$sb.AppendLine('    <li>ludeon.rimworld.ideology</li>')
    [void]$sb.AppendLine('    <li>ludeon.rimworld.biotech</li>')
    [void]$sb.AppendLine('  </knownExpansions>')
    [void]$sb.AppendLine('</ModsConfigData>')
    [System.IO.File]::WriteAllText($modsConfigPath, $sb.ToString(), (New-Object System.Text.UTF8Encoding($false)))
    Write-Ok "ModsConfig.xml written with $($resolvedMods.Count) active mods"
}

# ---------------------------------------------------------------------------
Write-Step "Building the custom mod assembly"
$modSource = Join-Path $RepoRoot "Mods\LanternVillage"
if (-not $SkipBuild) {
    $csproj = Join-Path $modSource "Source\LanternVillage\LanternVillage.csproj"
    Push-Location (Split-Path $csproj -Parent)
    try {
        & dotnet build $csproj -c Release 2>&1 | ForEach-Object { Write-Host "    $_" }
        if ($LASTEXITCODE -ne 0) { Write-Fail "dotnet build failed (exit $LASTEXITCODE)"; exit 3 }
        Write-Ok "Build succeeded"
    } finally {
        Pop-Location
    }
} else {
    Write-Warn2 "Skipping build (-SkipBuild)"
}

# ---------------------------------------------------------------------------
Write-Step "Installing the mod into RimWorld's Mods folder"
$destMod = Join-Path $localModsDir "LanternVillage"
if (-not $WhatIf) {
    New-Item -ItemType Directory -Path $destMod -Force | Out-Null
    foreach ($item in @("About", "LoadFolders.xml", "1.6")) {
        $src = Join-Path $modSource $item
        $dest = Join-Path $destMod $item
        if (Test-Path $src) {
            # Remove any existing copy first: Copy-Item -Recurse nests the source INSIDE
            # an already-existing destination directory instead of overwriting its
            # contents (e.g. produces .../1.6/1.6/Assemblies on a second install). Safe
            # here because the pre-existing mod folder was already backed up above.
            if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
            Copy-Item $src $dest -Recurse -Force
        }
    }
    Write-Ok "Installed to $destMod"
} else {
    Write-Host "    [WhatIf] Would copy About/LoadFolders.xml/1.6 to $destMod"
}

# ---------------------------------------------------------------------------
Write-Step "Installing the scenario file"
$scenarioSrc = Join-Path $RepoRoot "scenario\The Lantern Village.rsc"
$scenarioDest = Join-Path $scenariosDir "The Lantern Village.rsc"
if (-not $WhatIf) {
    New-Item -ItemType Directory -Path $scenariosDir -Force | Out-Null
    Copy-Item $scenarioSrc $scenarioDest -Force
    Write-Ok "Scenario installed: $scenarioDest"
} else {
    Write-Host "    [WhatIf] Would copy scenario to $scenarioDest"
}

# ---------------------------------------------------------------------------
Write-Step "Post-install validation"
$okAll = $true
if (-not $WhatIf) {
    try { [xml](Get-Content $modsConfigPath -Raw) | Out-Null; Write-Ok "ModsConfig.xml is well-formed" } catch { Write-Fail "ModsConfig.xml invalid: $_"; $okAll = $false }
    try { [xml](Get-Content $scenarioDest -Raw) | Out-Null; Write-Ok "Scenario file is well-formed" } catch { Write-Fail "Scenario file invalid: $_"; $okAll = $false }
    $dll = Join-Path $destMod "1.6\Assemblies\LanternVillage.dll"
    if (Test-Path $dll) { Write-Ok "Mod assembly present: $dll" } elseif (-not $SkipBuild) { Write-Fail "Mod assembly missing: $dll"; $okAll = $false }
}

if ($okAll) {
    Write-Host "`nInstall complete. Run 'Launch The Lantern Village.bat' to start." -ForegroundColor Green
} else {
    Write-Host "`nInstall finished with problems -- see FAILED lines above." -ForegroundColor Red
    exit 4
}
