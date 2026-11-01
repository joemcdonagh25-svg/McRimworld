#Requires -Version 5.1
<#
  Restore.ps1 -- restores RimWorld's previous configuration from the most
  recent backup taken by Install.ps1.

  Restores: ModsConfig.xml, the Scenarios folder, and (if it existed before)
  the LanternVillage mod folder is removed since it was not present pre-install.
  Never touches Saves.
#>
[CmdletBinding()]
param(
    [string]$BackupStamp
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module (Join-Path $RepoRoot "discovery\Discovery.psm1") -Force

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg) { Write-Host "    OK: $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "    FAILED: $msg" -ForegroundColor Red }

$backupsRoot = Join-Path $RepoRoot "Backups"
if (-not (Test-Path $backupsRoot)) { Write-Fail "No backups found at $backupsRoot"; exit 1 }

if ($BackupStamp) {
    $backupDir = Join-Path $backupsRoot $BackupStamp
} else {
    $backupDir = (Get-ChildItem $backupsRoot -Directory | Sort-Object Name -Descending | Select-Object -First 1).FullName
}
if (-not $backupDir -or -not (Test-Path $backupDir)) { Write-Fail "Backup not found."; exit 1 }
Write-Step "Restoring from $backupDir"

$steamPath = Find-SteamInstall
$rimWorldInstalls = Find-RimWorldInstall -SteamPath $steamPath
$rimWorldPath = $rimWorldInstalls[0]
$userConfigDir = Get-RimWorldUserConfigDir
$modsConfigPath = Get-ModsConfigPath -UserConfigDir $userConfigDir
$scenariosDir = Get-ScenariosDir -UserConfigDir $userConfigDir
$localModsDir = Get-LocalModsDir -RimWorldPath $rimWorldPath

$bakModsConfig = Join-Path $backupDir "ModsConfig.xml"
if (Test-Path $bakModsConfig) {
    Copy-Item $bakModsConfig $modsConfigPath -Force
    Write-Ok "Restored ModsConfig.xml"
} else {
    Write-Fail "No ModsConfig.xml backup found in $backupDir"
}

$bakScenarios = Join-Path $backupDir "Scenarios"
if (Test-Path $bakScenarios) {
    Remove-Item $scenariosDir -Recurse -Force -ErrorAction SilentlyContinue
    Copy-Item $bakScenarios $scenariosDir -Recurse -Force
    Write-Ok "Restored Scenarios folder"
}

$destMod = Join-Path $localModsDir "LanternVillage"
$bakMod = Join-Path $backupDir "LanternVillage-Mod"
if (Test-Path $bakMod) {
    Remove-Item $destMod -Recurse -Force -ErrorAction SilentlyContinue
    Copy-Item $bakMod $destMod -Recurse -Force
    Write-Ok "Restored previous LanternVillage mod folder"
} elseif (Test-Path $destMod) {
    Remove-Item $destMod -Recurse -Force
    Write-Ok "Removed LanternVillage mod folder (did not exist before install)"
}

Write-Host "`nRestore complete. Your previous RimWorld configuration is back in place." -ForegroundColor Green
