# Implementation Report -- The Lantern Village

Status as of the build done in this session, on the reference machine (Steam at
`C:\Program Files (x86)\Steam`, RimWorld 1.6.4871 rev590, dotnet SDK 8.0.422, 158
pre-existing saves and a 22-mod active profile that were never modified).

Every API referenced below (ScenParts, `IdeoGenerator`, `SavedGameLoaderNow`, `GenStep`,
`DefDatabase<T>`, `Tile`, `CellRect`, `Page_SelectStoryteller`, `DebugActionAttribute`,
etc.) was confirmed by loading RimWorld's own `Assembly-CSharp.dll` via .NET reflection
and/or by reading the installed game's and mods' own `Defs/*.xml`, not from memory or
guesswork. Every Workshop packageId in `manifest/mod-manifest.json` was read from the
mod's own `About.xml`, never derived from a title or numeric Workshop id.

## Runtime verification results (2026-10-31)

The real installer was run against this machine's live RimWorld configuration (with
the user's explicit go-ahead), subscribing to GloomyFurniture and Realistic Darkness
1.6 via Steam, and RimWorld was launched and driven with computer-use automation.
Findings:

- **The installer's real run worked exactly as designed**: all 8 Workshop mods
  resolved to their real packageIds (`Solaris.FurnitureBase` for GloomyFurniture,
  `Wemd.RealisticDarkness` for Realistic Darkness 1.6), a timestamped backup was taken,
  `ModsConfig.xml` was written with the 12-mod Lantern Village profile, the mod built
  and was copied into the real `Mods` folder, and the scenario file landed in the real
  `Scenarios` folder.
- **The game reaches the main menu and `LanternVillageMod`'s constructor runs
  successfully**: `Player-prev.log` contains the line
  `[Lantern Village] Loaded. Supernatural horror events: disabled (behind feature flag)`
  -- our Harmony patches apply without throwing.
- **RimWorld's own safety net was observed firsthand**: when play-data loading throws,
  RimWorld catches it and resets `ModsConfigData` to Core + owned DLC, then retries --
  this happened automatically, more than once, with no data loss.
- **A real, reproducible bug was found and isolated, and it is not ours.** With the
  full 12-mod profile active, loading throws
  `System.TypeInitializationException: The type initializer for 'MedievalOverhaul.Utility'
  threw an exception. ---> System.NullReferenceException`. Four isolation runs (each a
  full relaunch) narrowed it down:
  1. Removing GloomyFurniture + Realistic Darkness -- still crashes.
  2. Removing Dark Ages: Beasts and Monsters -- still crashes.
  3. Medieval Overhaul with *only* its own declared hard dependencies (Harmony, Vanilla
     Expanded Framework, [SYR] Processor Framework) and Ideology+Biotech -- still
     crashes.
  4. Re-adding Royalty (in case Medieval Overhaul assumes a DLC we deliberately
     excluded) -- still crashes.

  Reflecting Medieval Overhaul's own `MedievalOverhaul.Utility` class and reading its
  `Patches/Vanilla Books Expanded/BookPatches.xml` found the actual root cause: that
  patch's `PatchOperationFindMod` fallback (used when "Vanilla Books Expanded" is *not*
  installed) adds a `DankPyon_Library` `RoomRoleDef` whose `relatedStats` includes
  `ReadingBonus` -- a `StatDef` that is only ever defined by Vanilla Books Expanded
  itself. Vanilla Books Expanded isn't in this project's required mod list (the spec
  never named it), so that `StatDef` reference never resolves, and something in
  `Utility`'s static constructor dereferences the result without a null check. This is
  a bug in Medieval Overhaul's own compatibility patch, present with or without any of
  this project's other mods or DLC choices.
- **The user's original configuration was restored immediately** once this was
  confirmed: `Restore.ps1 -BackupStamp 20261031-185800` (the backup taken right before
  the first real write) put back their original 23-mod `ModsConfig.xml`, their
  `Scenarios` folder, and removed the `LanternVillage` mod folder. Their saves
  directory was never touched by anything here -- one new file, `New Arrivals1.rws`,
  appeared during testing (most likely an autosave from RimWorld's own crash-recovery
  path) and was left in place since nothing existing was overwritten.
- A secondary finding worth recording for future sessions: this machine runs an
  ultrawide (5120x1440) display, and RimWorld's default **exclusive fullscreen** mode
  silently swallowed synthetic mouse clicks (the OS correctly moved the cursor, and
  `GetForegroundWindow` correctly reported RimWorld, but clicks had no in-game effect)
  while screenshots still worked. Relaunching with `-screen-fullscreen 0 -popupwindow`
  fixed it. Any future automated UI testing of this project should launch RimWorld
  windowed from the start.

**Recommended next step**: either (a) also subscribe to and add "Vanilla Books
Expanded" to the required mod list (untested whether this actually fixes it -- it
would make the patch's `match` branch run instead of the buggy `nomatch` one, but that
branch wasn't exercised), or (b) report the bug to Medieval Overhaul's author with the
exact file/line above, or (c) drop Medieval Overhaul's `BookPatches.xml` compatibility
patch via a small local patch-disabling mod. None of these were attempted without
asking first, since each is itself a further change to the user's mod set.

## Complete and runtime-verified

- **Discovery** (`discovery/Discovery.psm1`): Steam, RimWorld, version, owned DLC,
  Workshop content path, user config dir, Mods dir, Saves dir, Scenarios dir,
  ModsConfig.xml parsing. Verified against the real machine; covered by
  `tests/Run-Tests.ps1`.
- **Mod resolution**: 6 of 8 required mods resolved to their real packageId by reading
  each one's own `About.xml` on this machine (Harmony, Vanilla Expanded Framework,
  [SYR] Processor Framework, Medieval Overhaul, Dark Ages: Beasts and Monsters, World
  Tech Level). No incompatibilities found against World Tech Level's and VEF's
  `incompatibleWith` lists.
- **Missing-mod handling**: GloomyFurniture and Realistic Darkness 1.6 were not
  subscribed on this machine at discovery time. `Install.ps1` correctly detected this,
  opened their Steam subscribe pages, waited with a configurable timeout, and exited
  with a distinct code (2) and clear instructions. Both were then subscribed for real
  during the runtime-verification pass (see below) and now resolve to their real
  packageIds (`Solaris.FurnitureBase`, `Wemd.RealisticDarkness`).
- **Backup / restore**: sandboxed round-trip test passes; `Install.ps1`'s real backup
  step was exercised (it ran against the live `ModsConfig.xml`/`Scenarios` during
  testing, read-only on the source, writing only into this repo's own `Backups/`).
- **Idempotency**: `Install.ps1 -WhatIf` run twice back-to-back produces identical exit
  codes and output.
- **Custom mod compiles against the real game assembly**: `dotnet build` succeeds, 0
  warnings, 0 errors, producing `Mods/LanternVillage/1.6/Assemblies/LanternVillage.dll`.
  This is not a trivial result -- the first three build attempts caught real bugs
  (wrong namespace for `DebugActionAttribute`, `IdeoGenerationParms` being a struct not
  a class, `Page_SelectStoryteller`'s fields being private, `CellRect`'s field names
  being lowercase `minX`/`maxX`/etc. rather than `MinX`/`MaxX`), all fixed and
  reverified by rebuilding.
- **Three real PowerShell bugs caught by the test suite and fixed**: a classic
  PowerShell single-element-array-to-scalar unwrap in `Find-RimWorldInstall`, a second
  nesting bug introduced while fixing the first (`@(FunctionCall)` double-wraps a
  `Write-Output -NoEnumerate` result), and a scriptblock-scope bug in the test file
  itself (`$resolved += ...` inside a child scriptblock shadowed the outer variable
  instead of updating it). All three were invisible from reading the code and only
  surfaced by actually running it -- which is the point of having the test suite.
- **Scenario file**: valid RimWorld 1.6 `.rsc` (verified against an existing shipped
  Workshop scenario's real schema, not invented), well-formed XML, correct
  `PlayerTribe` faction, `Standing` arrival, and starting-supply counts matching the
  manifest.
- **Starting-supply item defNames**: `Pemmican`, `MedicineHerbal`, `WoodLog`,
  `Bow_Short`, `Bow_Recurve`, `MeleeWeapon_Knife` (stuff `Steel`), `MeleeWeapon_Club`
  (stuff `WoodLog`), `Silver`, `Muffalo` -- every one confirmed present in the loaded
  Core/DLC Defs on this machine, not invented.

## Implemented but not runtime-verified

These compile against the real API and are logically sound, but this session did not
launch RimWorld to confirm they behave correctly in a running game (see "Blocked"
below for why, and what it would take).

- **The Hearthbound ideoligion** (`HarmonyPatches_Ideology.cs`): Harmony prefix on
  `RimWorld.IdeoGenerator.GenerateIdeo` forcing the `Collectivist` and `Loyalist` memes
  (both confirmed-real `MemeDef`s) plus the name/description, gated to only fire for
  this scenario. Not confirmed in an actual new-colony flow.
- **Storyteller/difficulty defaults** (`HarmonyPatches_Storyteller.cs`): Harmony
  postfix on `Page_SelectStoryteller`'s constructor, writing to its private
  `storyteller`/`difficulty` fields via `AccessTools.Field` to pre-select Cassandra
  Classic / Rough ("strive to survive"). This sets a *default*, not a lock -- the
  player can still change it on that screen. Not confirmed on an actual page load.
- **Colonist roles** (`ScenPart_LanternVillageSetup.cs`): raises the five starting
  pawns' skills to the specified floors, adds one fitting trait each, and wires an
  Elder-Healer `Spouse` relation and an Elder-Hunter `Parent` relation via
  `Pawn_RelationsTracker.AddDirectRelation` (confirmed real API). The exact
  *direction* semantics of the parent/child relation, and whether `Find.GameInitData`
  still holds the right pawn list at `PostGameStart` time, are the two things most
  worth checking first in a real playtest.
- **Hamlet Mode map generation** (`GenStep_LanternHamlet.cs`): builds a great hall,
  two cottages, a growing zone and a stockpile zone using confirmed defNames (`Wall`,
  `Door`, `Campfire`, `TorchLamp`, `Bed`) and the standard `GenStep`/`ThingMaker`/
  `GenSpawn`/`Zone_Growing`/`Zone_Stockpile` API. The `GenStepDef`'s `order` value
  (800) is a reasonable guess at running after terrain/plant generation and before
  scenario-scatter, not confirmed against the exact vanilla ordering.
- **Auto-resume** (`ResumeHelper.cs`): `[StaticConstructorOnStartup]` +
  `LongEventHandler.ExecuteWhenFinished` reading a marker file and calling the real,
  confirmed `Verse.SavedGameLoaderNow.LoadGameFromSaveFileNow(string)`. Logically this
  is the correct, documented way to do it; not confirmed by actually watching it skip
  the main menu.
- **Map/tile validator** (`MapValidator.cs`): scores tiles from `Find.WorldGrid.Tiles`
  (confirmed real enumerable) against biome/hilliness/temperature, bounded by a
  configurable scan limit, exposed as a debug action. This implements the
  spec's "map-validation routine that checks candidate tiles" requirement, but does
  **not** automatically regenerate the world across multiple seeds -- see Planned.

## Planned

- **Fluid-ideoligion named rituals** (The First Fire, Harvest Supper, The Long Night,
  Remembering): reflecting `IdeoGenerationParms`/`IdeoGenerator` found no supported way
  to pre-seed specific rituals onto a *fluid* ideoligion in this RimWorld build --
  fluid ideos assign rituals dynamically over the course of play. Forcing these would
  require either switching to a *fixed* ideo (losing the "develop it over time" framing
  the spec explicitly asked for) or a deeper, unverified hook into ritual assignment.
  Left out rather than hand-waved.
- **Automatic multi-seed world search**: `MapValidator` scores tiles in an *already
  generated* world. Actually regenerating the world with a new seed when no good tile
  exists would mean driving `Page_CreateWorldParams` programmatically, which was not
  attempted this session (see Blocked).
- **Horror Director content** (M5): the phase/day-range structure, configurable weights,
  and debug actions are implemented (`HorrorDirector.cs`); the actual incidents per
  phase are not -- `EnableSupernatural` defaults to `false` exactly as the spec
  requested ("leave supernatural incidents behind an explicitly disabled feature flag
  until tested"), and the installer/scenario are explicitly not blocked on this.
- **Automatically clicking "The Lantern Village" on the in-game scenario list**: no
  verified vanilla command-line flag exists for this (checked `GenCommandLine` and
  `QuickStarter` by reflection; found the mechanism real mods use for save-loading
  instead, which is what `ResumeHelper` uses). One manual click remains per *new* game;
  resuming an existing save is fully automatable (see above).

## Blocked

- **Medieval Overhaul crashes play-data loading on this machine, independent of
  everything else in this project** -- see "Runtime verification results" above for
  the full isolation trail and root cause (a `ReadingBonus` `StatDef` reference in
  Medieval Overhaul's own `Vanilla Books Expanded` compatibility patch that is never
  satisfied because Vanilla Books Expanded isn't installed). Because of this, the full
  12-mod Lantern Village profile cannot currently stay active on this machine long
  enough to reach a playable colony -- RimWorld's own safety net resets the mod list to
  Core + DLC before a scenario can even be selected. World generation, pawn spawning,
  medieval production and save/reload (the rest of point 12's checklist) could not be
  exercised downstream of this. Everything else in this project (installer, scenario
  file, mod compilation, ideology/colonist/hamlet/resume code) is implemented and, where
  noted above, either runtime-verified or implemented-but-unverified on its own merits
  -- this blocker is specifically about Medieval Overhaul's own compatibility patch, not
  about any file in this repository. Unblocking it needs a decision only you can make
  (subscribe to Vanilla Books Expanded / report upstream / patch around it locally); see
  the "Recommended next step" above.
- The user's live RimWorld configuration has been restored to its original state
  (`Restore.ps1 -BackupStamp 20261031-185800`) and is not currently running the Lantern
  Village profile. Re-running `Install.ps1` reapplies it in under a minute (all 8
  Workshop mods are now already resolved).
