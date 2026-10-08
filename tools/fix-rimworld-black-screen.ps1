#Requires -Version 5.1
<#
.SYNOPSIS
  Unstick RimWorld when it boots, jumps resolution, then black-screens / hard-crashes.

.DESCRIPTION
  Matches the failure mode Joe hit after recent pulls:
    - Unity/Prefs apply fullscreen 2560x1440 after launch
    - Main-menu UI dies (DevToolStarterOnGUI / MusicManagerEntry NRE spam, or empty Player.log)

  This script does NOT delete saves. It:
    1. Kills RimWorld if still running
    2. Backs up Config
    3. Writes a safe windowed Prefs.xml (1280x720, Dev Mode off)
    4. Writes a vanilla-only ModsConfig.xml (Core + owned DLCs; no workshop / no McRimworld mods)
    5. Clears Unity registry resolution overrides that ignore Prefs.xml
    6. Prints Steam launch options + next steps

.NOTES
  Run in PowerShell (no admin needed for Prefs; registry edit is HKCU only).
  After main menu works: re-enable The Ark / Vampire Lord one at a time in Mods.
#>

$ErrorActionPreference = 'Stop'

$ludeonRoot = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios'
$configDir = Join-Path $ludeonRoot 'Config'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupRoot = Join-Path $ludeonRoot "Config-backup-$stamp"

Write-Host ''
Write-Host '=== RimWorld black-screen / resolution-crash recovery ===' -ForegroundColor Cyan
Write-Host "Config: $configDir"
Write-Host ''

# --- 1) Kill hung RimWorld ---
$procs = @(Get-Process -Name 'RimWorldWin64','RimWorld' -ErrorAction SilentlyContinue)
if ($procs.Count -gt 0) {
    Write-Host 'Stopping RimWorld processes...'
    $procs | Stop-Process -Force
    Start-Sleep -Seconds 2
} else {
    Write-Host 'RimWorld is not running.'
}

# --- 2) Backup Config ---
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir -Force | Out-Null
    Write-Host "Created missing Config folder: $configDir"
} else {
    Write-Host "Backing up Config -> $backupRoot"
    Copy-Item -Path $configDir -Destination $backupRoot -Recurse -Force
}

# --- 3) Safe Prefs.xml (windowed, modest res, Dev Mode off) ---
$prefsPath = Join-Path $configDir 'Prefs.xml'
$safePrefs = @'
<?xml version="1.0" encoding="utf-8"?>
<PrefsData>
  <volumeMaster>1</volumeMaster>
  <volumeGame>1</volumeGame>
  <volumeMusic>0.5</volumeMusic>
  <volumeAmbient>1</volumeAmbient>
  <volumeUI>1</volumeUI>
  <screenWidth>1280</screenWidth>
  <screenHeight>720</screenHeight>
  <fullscreen>False</fullscreen>
  <uiScale>1</uiScale>
  <customCursorEnabled>True</customCursorEnabled>
  <hatsOnlyOnMap>False</hatsOnlyOnMap>
  <plantWindSway>True</plantWindSway>
  <screenShakeIntensity>1</screenShakeIntensity>
  <textureCompression>True</textureCompression>
  <showRealtimeClock>False</showRealtimeClock>
  <disableTinyText>False</disableTinyText>
  <runInBackground>True</runInBackground>
  <edgeScreenScroll>True</edgeScreenScroll>
  <temperatureMode>Celsius</temperatureMode>
  <autosaveIntervalDays>1</autosaveIntervalDays>
  <pauseOnLoad>False</pauseOnLoad>
  <automaticPauseMode>MajorThreat</automaticPauseMode>
  <mapDragSensitivity>1.3</mapDragSensitivity>
  <smoothCameraJumps>True</smoothCameraJumps>
  <gravshipCutscenes>True</gravshipCutscenes>
  <autosavesCount>5</autosavesCount>
  <pauseOnError>False</pauseOnError>
  <devMode>False</devMode>
  <debugActionPalette />
  <langFolderName>English</langFolderName>
  <logVerbose>False</logVerbose>
  <openLogOnWarnings>False</openLogOnWarnings>
  <closeLogWindowOnEscape>True</closeLogWindowOnEscape>
  <disableQuickStartCryptoSickness>True</disableQuickStartCryptoSickness>
  <quickStartDevPaletteOn>False</quickStartDevPaletteOn>
  <resetModsConfigOnCrash>True</resetModsConfigOnCrash>
</PrefsData>
'@
Set-Content -Path $prefsPath -Value $safePrefs -Encoding UTF8
Write-Host "Wrote safe Prefs.xml (1280x720 windowed, Dev Mode off)."

# --- 4) Vanilla ModsConfig (keeps DLCs if previously present; drops everything else) ---
$modsPath = Join-Path $configDir 'ModsConfig.xml'
$knownDlcs = @(
    'Ludeon.RimWorld.Royalty',
    'Ludeon.RimWorld.Ideology',
    'Ludeon.RimWorld.Biotech',
    'Ludeon.RimWorld.Anomaly',
    'Ludeon.RimWorld.Odyssey'
)
$active = New-Object System.Collections.Generic.List[string]
$active.Add('Ludeon.RimWorld') | Out-Null

# Preserve DLC entries from backup if we can read them; never keep workshop / McRimworld packs.
$sourceMods = $modsPath
$backupMods = Join-Path $backupRoot 'ModsConfig.xml'
if (Test-Path $backupMods) { $sourceMods = $backupMods }

if (Test-Path $sourceMods) {
    try {
        [xml]$old = Get-Content -Path $sourceMods -Raw
        $nodes = @($old.ModsConfigData.activeMods.li)
        foreach ($id in $nodes) {
            if ($knownDlcs -contains $id -and -not $active.Contains($id)) {
                $active.Add($id) | Out-Null
            }
        }
    } catch {
        Write-Host "Could not parse old ModsConfig; using Core only. ($_)" -ForegroundColor Yellow
    }
}

$liXml = ($active | ForEach-Object { "    <li>$_</li>" }) -join "`n"
$version = '1.6.0'
$safeMods = @"
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>$version</version>
  <activeMods>
$liXml
  </activeMods>
  <knownExpansions>
    <li>Ludeon.RimWorld.Royalty</li>
    <li>Ludeon.RimWorld.Ideology</li>
    <li>Ludeon.RimWorld.Biotech</li>
    <li>Ludeon.RimWorld.Anomaly</li>
    <li>Ludeon.RimWorld.Odyssey</li>
  </knownExpansions>
</ModsConfigData>
"@
Set-Content -Path $modsPath -Value $safeMods -Encoding UTF8
Write-Host "Wrote vanilla ModsConfig.xml (active: $($active -join ', '))."
Write-Host '  (HugsLib / Harmony / The Ark / Vampire Lord / Workshop are OFF for this recovery launch.)'

# --- 5) Unity registry overrides (often win over Prefs.xml and cause the resolution jump) ---
$regPaths = @(
    'HKCU:\Software\Ludeon Studios\RimWorld by Ludeon Studios',
    'HKCU:\Software\Ludeon Studios\Rimworld',
    'HKCU:\Software\Ludeon Studios\RimWorld'
)
foreach ($rp in $regPaths) {
    if (Test-Path $rp) {
        Write-Host "Clearing Unity screen keys under $rp"
        $names = @(
            'Screenmanager Resolution Width_h182942802',
            'Screenmanager Resolution Height_h2627697771',
            'Screenmanager Is Fullscreen mode_h3981298716',
            'Screenmanager Resolution Width',
            'Screenmanager Resolution Height',
            'Screenmanager Is Fullscreen mode'
        )
        foreach ($n in $names) {
            Remove-ItemProperty -Path $rp -Name $n -ErrorAction SilentlyContinue
        }
        # Also set explicit safe values when Unity uses DWORD forms.
        try {
            New-ItemProperty -Path $rp -Name 'Screenmanager Resolution Width_h182942802' -PropertyType DWord -Value 1280 -Force | Out-Null
            New-ItemProperty -Path $rp -Name 'Screenmanager Resolution Height_h2627697771' -PropertyType DWord -Value 720 -Force | Out-Null
            New-ItemProperty -Path $rp -Name 'Screenmanager Is Fullscreen mode_h3981298716' -PropertyType DWord -Value 0 -Force | Out-Null
        } catch {
            # Key name hashes differ by Unity version; deletion above is the important part.
        }
    }
}

# --- 6) Operator next steps ---
Write-Host ''
Write-Host 'NEXT (do these before launching):' -ForegroundColor Green
Write-Host '  1) Steam -> RimWorld -> Properties -> General'
Write-Host '       - UNCHECK "Keep game saves in the Steam Cloud" (stops old Prefs coming back)'
Write-Host '       - Launch Options (paste exactly):'
Write-Host ''
Write-Host '         -windowed -screen-width 1280 -screen-height 720 -force-d3d11 -logfile %USERPROFILE%\Desktop\rimworld-debug.log' -ForegroundColor Yellow
Write-Host ''
Write-Host '  2) Steam -> Properties -> Installed Files -> Verify integrity of game files'
Write-Host '  3) Confirm Steam beta is NONE / default (not RimWorld 1.0) for 1.6 play'
Write-Host '  4) Launch RimWorld from Steam once'
Write-Host ''
Write-Host 'SUCCESS = small windowed MAIN MENU (not black).' -ForegroundColor Green
Write-Host 'Then: Mods -> enable Biotech + Vampire Lord / The Ark one at a time -> restart each time.'
Write-Host ''
Write-Host "Backup kept at: $backupRoot"
Write-Host 'Saves were not touched.'
Write-Host ''
Write-Host 'If it is STILL black after those Steam steps (even with only Core):' -ForegroundColor Yellow
Write-Host '  Reinstall RimWorld. Saves are separate from the game install.'
Write-Host '  See README "Clean reinstall (keep saves)" or ask the agent for the checklist.'
Write-Host ''
