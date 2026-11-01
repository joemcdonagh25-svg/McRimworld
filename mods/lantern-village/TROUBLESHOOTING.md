# Troubleshooting

## "Steam not found" / "RimWorld (app 294100) not found"
`Install.ps1` checks the registry (`HKCU:\Software\Valve\Steam`,
`HKLM:\SOFTWARE\WOW6432Node\Valve\Steam`) and common install paths, then reads
`steamapps\libraryfolders.vdf` to check every Steam library, not just the default one.
If it still can't find RimWorld, make sure it has been launched at least once (so its
user config folder exists), and that it's actually app id 294100 (not a pirated or
non-Steam copy -- this project only supports Steam's install/subscription flow).

## "Required DLC not owned"
You need Ideology and Biotech. Royalty, Odyssey and Anomaly are not required (and are
deliberately excluded from the active profile).

## Install.ps1 exits with code 2 ("Still missing after waiting")
GloomyFurniture and/or Realistic Darkness 1.6 weren't subscribed/downloaded in time.
`Install.ps1` opens their Steam subscribe pages automatically -- click **Subscribe** in
the Steam window, wait for the download to finish (watch the Downloads tab), then
re-run `Install.ps1`. It's safe to re-run: already-resolved mods aren't re-prompted.

## "dotnet build failed"
Make sure the .NET SDK is installed (`dotnet --version`). The build pulls Harmony and
`Microsoft.NETFramework.ReferenceAssemblies.net472` from NuGet on first build, so it
needs network access once. If your RimWorld install isn't at the default Steam path,
create `Mods/LanternVillage/Source/LanternVillage/Directory.Build.props.user` (ignored
by git) with:

```xml
<Project>
  <PropertyGroup>
    <RimWorldDir>D:\Your\Path\To\RimWorld</RimWorldDir>
  </PropertyGroup>
</Project>
```

and import it from `Directory.Build.props`, or just edit `Directory.Build.props`
directly.

## The scenario doesn't show up in RimWorld's scenario list
Confirm `scenario/The Lantern Village.rsc` was copied to
`%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Scenarios\`.
Custom scenarios only appear once RimWorld has been (re)started after the file lands
there.

## Mods are active but colonists don't have their intended roles / ideology looks vanilla
Those effects (`ScenPart_LanternVillageSetup`, the ideoligion generation patch, the
storyteller/difficulty defaults) are implemented against RimWorld's real API and
compile cleanly, but have not been confirmed by actually starting a colony in this
session -- see `IMPLEMENTATION_REPORT.md`. Check `Player.log` (same folder as Saves)
for lines starting with `[Lantern Village]`; every failure path in the mod logs a
specific warning or error rather than failing silently.

## I want my old setup back
Run `Restore Previous RimWorld Configuration.bat`. It restores the most recent backup
under `Backups/` (`ModsConfig.xml`, the `Scenarios` folder, and removes or restores the
`LanternVillage` mod folder). It never touches your `.rws` save files.

## Something went wrong mid-install and I'm not sure what state I'm in
Every `Install.ps1` run creates a fresh timestamped backup under `Backups/` *before*
changing anything, even if a later step fails. Find the most recent folder under
`Backups/`, or pass `-BackupStamp <folder name>` to `Restore.ps1` to pick a specific one.
