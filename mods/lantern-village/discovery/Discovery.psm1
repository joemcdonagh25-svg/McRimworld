#Requires -Version 5.1
<#
  Discovery.psm1
  Locates Steam, RimWorld, mods, and user data directories on this machine.
  Pure read-only discovery functions; no installer side effects live here.
#>

Set-StrictMode -Version Latest

function Find-SteamInstall {
    [CmdletBinding()]
    param()

    $candidates = @()

    $hkcu = Get-ItemProperty -Path "HKCU:\Software\Valve\Steam" -ErrorAction SilentlyContinue
    if ($hkcu -and $hkcu.SteamPath) {
        $candidates += ($hkcu.SteamPath -replace '/', '\')
    }

    $hklm = Get-ItemProperty -Path "HKLM:\SOFTWARE\WOW6432Node\Valve\Steam" -ErrorAction SilentlyContinue
    if ($hklm -and $hklm.InstallPath) {
        $candidates += $hklm.InstallPath
    }

    foreach ($p in @("C:\Program Files (x86)\Steam", "C:\Program Files\Steam", "D:\Steam", "E:\Steam")) {
        $candidates += $p
    }

    foreach ($c in $candidates) {
        if ($c -and (Test-Path $c) -and (Test-Path (Join-Path $c "steam.exe"))) {
            return (Resolve-Path $c).Path
        }
    }
    return $null
}

function Get-SteamLibraryFolders {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$SteamPath)

    $libraries = @($SteamPath)
    $vdf = Join-Path $SteamPath "steamapps\libraryfolders.vdf"
    if (Test-Path $vdf) {
        $content = Get-Content $vdf -Raw
        $matches = [regex]::Matches($content, '"path"\s*"([^"]+)"')
        foreach ($m in $matches) {
            $p = $m.Groups[1].Value -replace '\\\\', '\'
            if ($p -and (Test-Path $p) -and ($libraries -notcontains $p)) {
                $libraries += $p
            }
        }
    }
    # Force array output -- PowerShell unwraps a single-element array to a scalar
    # when it leaves a function, which breaks callers that index the result with [0].
    # (Write-Output -NoEnumerate is the unambiguous fix; a bare "return ,$x" was tried
    # first and actually double-wrapped the array -- caught by the test suite.)
    Write-Output -NoEnumerate $libraries
}

function Find-RimWorldInstall {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$SteamPath)

    $libraries = @(Get-SteamLibraryFolders -SteamPath $SteamPath)
    $found = @()
    foreach ($lib in $libraries) {
        $p = Join-Path $lib "steamapps\common\RimWorld"
        if (Test-Path (Join-Path $p "RimWorldWin64.exe")) {
            $found += (Resolve-Path $p).Path
        }
    }
    # See the comment in Get-SteamLibraryFolders -- always return a true array.
    Write-Output -NoEnumerate $found
}

function Get-RimWorldVersion {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RimWorldPath)

    $verFile = Join-Path $RimWorldPath "Version.txt"
    if (Test-Path $verFile) {
        return (Get-Content $verFile -Raw).Trim()
    }
    return $null
}

function Get-RimWorldAppManifest {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$SteamPath, [Parameter(Mandatory)][string]$RimWorldPath)

    # RimWorldPath is .../steamapps/common/RimWorld ; the manifest lives one level up from "common"
    $steamappsDir = Split-Path (Split-Path $RimWorldPath -Parent) -Parent
    $acf = Join-Path $steamappsDir "appmanifest_294100.acf"
    if (Test-Path $acf) {
        return Get-Content $acf -Raw
    }
    return $null
}

function Get-OwnedDlc {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$AcfContent)

    $dlcMap = @{
        "1149640" = "Royalty"
        "1392840" = "Ideology"
        "1826140" = "Biotech"
        "2380740" = "Anomaly"
        "3022790" = "Odyssey"
    }
    $owned = @()
    foreach ($id in $dlcMap.Keys) {
        if ($AcfContent -match [regex]::Escape($id)) {
            $owned += $dlcMap[$id]
        }
    }
    return $owned
}

function Get-WorkshopContentPath {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$SteamPath)

    $p = Join-Path $SteamPath "steamapps\workshop\content\294100"
    if (Test-Path $p) { return (Resolve-Path $p).Path }
    return $null
}

function Get-RimWorldUserConfigDir {
    [CmdletBinding()]
    param()
    $p = Join-Path $env:USERPROFILE "AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios"
    if (Test-Path $p) { return (Resolve-Path $p).Path }
    return $null
}

function Get-ModsConfigPath {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$UserConfigDir)
    return (Join-Path $UserConfigDir "Config\ModsConfig.xml")
}

function Get-ScenariosDir {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$UserConfigDir)
    return (Join-Path $UserConfigDir "Scenarios")
}

function Get-SavesDir {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$UserConfigDir)
    return (Join-Path $UserConfigDir "Saves")
}

function Get-LocalModsDir {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RimWorldPath)
    return (Join-Path $RimWorldPath "Mods")
}

function Resolve-ModPackageId {
    <#
      Reads a mod's own About.xml and returns its top-level packageId.
      Never derive a packageId from a Workshop title or numeric id -- always read it from the file.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ModFolder)

    $aboutFile = Join-Path $ModFolder "About\About.xml"
    if (-not (Test-Path $aboutFile)) { return $null }
    try {
        [xml]$xml = Get-Content $aboutFile -Raw -ErrorAction Stop
        $pid_ = $xml.ModMetaData.packageId
        if ($pid_) { return $pid_.Trim() }
    } catch {
        Write-Warning "Failed to parse $aboutFile : $($_.Exception.Message)"
    }
    return $null
}

function Find-ModByWorkshopId {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$WorkshopContentPath, [Parameter(Mandatory)][string]$WorkshopId)

    $p = Join-Path $WorkshopContentPath $WorkshopId
    if (Test-Path $p) {
        return [PSCustomObject]@{
            WorkshopId = $WorkshopId
            Path       = (Resolve-Path $p).Path
            PackageId  = Resolve-ModPackageId -ModFolder $p
        }
    }
    return $null
}

function Get-ActiveModIds {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ModsConfigPath)

    if (-not (Test-Path $ModsConfigPath)) { return @() }
    [xml]$xml = Get-Content $ModsConfigPath -Raw
    return @($xml.ModsConfigData.activeMods.li)
}

Export-ModuleMember -Function *
