# The Lantern Village

A one-click Windows installer and launcher for a reproducible RimWorld 1.6 scenario:
five villagers settle an isolated temperate forest and build a Grimm-inspired folk
horror hamlet. Inside the light is community. Outside the light is the unknown.

## Quick start

1. Double-click [`Launch The Lantern Village.bat`](Launch%20The%20Lantern%20Village.bat).
2. First run installs everything (mods, scenario, the custom support mod) and asks
   Founding Mode or Hamlet Mode.
3. RimWorld opens. Pick **The Lantern Village** from the scenario list (mod ScenarioDef;
   may also appear under Custom if the installer .rsc is present). Start a **new** game —
   loading an old save will not rebuild the hamlet.
4. Later runs offer to resume your save automatically.

To undo everything and get your previous RimWorld setup back, run
[`Restore Previous RimWorld Configuration.bat`](Restore%20Previous%20RimWorld%20Configuration.bat).

## What gets installed

See [`manifest/mod-manifest.json`](manifest/mod-manifest.json) for the full, versioned
list. In short: Harmony, Vanilla Expanded Framework, Medieval Overhaul, [SYR] Processor
Framework, Dark Ages: Beasts and Monsters, World Tech Level, GloomyFurniture, and
Realistic Darkness 1.6, plus the Ideology and Biotech DLC. Royalty, Odyssey and Anomaly
are explicitly excluded from the active profile.

Every packageId in the manifest was read from each mod's own `About.xml` on this
machine -- never guessed from a Workshop title or numeric id. Two mods (GloomyFurniture,
Realistic Darkness 1.6) were not yet subscribed to on the machine this was built on;
`Install.ps1` opens their Steam subscribe pages for you and waits for them to download,
then resolves their real packageIds once they land.

## Safety

- `Install.ps1` always takes a timestamped backup of your `ModsConfig.xml`, `Scenarios`
  folder, and any pre-existing `LanternVillage` mod folder into this repo's `Backups/`
  folder before changing anything.
- Nothing here ever reads, modifies, or deletes your existing `.rws` save files.
- Re-running `Install.ps1` is idempotent -- see `tests/Run-Tests.ps1`.

## Repository layout

- `Install.ps1` / `Restore.ps1` -- the installer and its undo.
- `Launch.ps1` / `Launch The Lantern Village.bat` -- the one-click launcher.
- `manifest/mod-manifest.json` -- versioned mod list, dependency order, scenario
  settings, and what was discovered on the reference machine.
- `scenario/The Lantern Village.rsc` -- the scenario file, in RimWorld's real save
  format (verified against an existing shipped Workshop scenario, not invented).
- `discovery/Discovery.psm1` -- reusable Steam/RimWorld path-discovery functions.
- `Mods/LanternVillage/` -- the custom support mod (C# + XML defs). Generates The
  Hearthbound ideoligion, applies the five colonists' roles, the Hamlet Mode map
  generator, the horror phase controller, and the auto-resume hook.
- `tests/Run-Tests.ps1` -- automated test suite (see [Validation](#validation)).

## Validation

Run:

```bash
powershell -NoProfile -ExecutionPolicy Bypass -File tests\Run-Tests.ps1
```

This checks path discovery, version detection, manifest consistency, mod-dependency
resolution against your actual Workshop folder, scenario file validity, the
ModsConfig.xml writer, a sandboxed backup/restore round-trip, idempotent installation
(`Install.ps1 -WhatIf` run twice), that the mod assembly builds, and that your saves are
left untouched. It does not launch RimWorld -- see
[`IMPLEMENTATION_REPORT.md`](IMPLEMENTATION_REPORT.md) for what still needs a real
runtime pass, and for exactly what is/isn't verified, in the project's own terms.

## Building the mod yourself

```bash
cd "Mods/LanternVillage/Source/LanternVillage"
dotnet build -c Release
```

This references RimWorld's own `Managed/*.dll` directly (via `Directory.Build.props`;
override `RimWorldDir` there if your install path differs) rather than copying or
redistributing them, and pulls Harmony and the .NET Framework reference assemblies from
NuGet. No Visual Studio install required -- only the `dotnet` SDK.

## Known limitations

See [`IMPLEMENTATION_REPORT.md`](IMPLEMENTATION_REPORT.md) for the authoritative,
categorized list (complete & verified / implemented but not runtime-verified / planned
/ blocked). The short version:

- You still click "The Lantern Village" once on the in-game scenario list -- there is no
  verified vanilla command-line flag to skip that screen.
- Storyteller/difficulty are pre-selected defaults on that screen, not locked choices.
- Fluid-ideoligion rituals (The First Fire, Harvest Supper, The Long Night, Remembering)
  are not force-seeded; RimWorld assigns rituals to fluid ideos dynamically over play.
- The horror phase controller's supernatural events are implemented as a disabled
  feature flag (`HorrorDirector.EnableSupernatural = false`) pending real playtesting
  against this exact mod list.
