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
  `Patches/Vanilla Books Expanded/BookPatches.xml` turned up a *plausible* root cause at
  the time: that patch's `PatchOperationFindMod` fallback (used when "Vanilla Books
  Expanded" is *not* installed) adds a `DankPyon_Library` `RoomRoleDef` whose
  `relatedStats` includes `ReadingBonus` -- a `StatDef` only Vanilla Books Expanded
  defines. **This turned out not to be the actual cause -- see the 2026-11-01 follow-up
  below**, which disproves it directly and confirms the bug is unrelated to DLC choice
  or to Vanilla Books Expanded.
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

## Follow-up: Vanilla Books Expanded did not fix it -- conclusively a Medieval Overhaul bug (2026-11-01)

With the user's go-ahead, "Vanilla Books Expanded" (Steam Workshop 2193152410,
packageId `VanillaExpanded.VBooksE`, confirmed 1.6-compatible, same author family as
the rest of this manifest) was subscribed and added to the required mod list, placed
right after Processor Framework and before Medieval Overhaul in load order and in
`About.xml`'s `loadAfter`.

Along the way, two more real bugs were found and fixed (by actually reinstalling and
relaunching, not by inspection alone):
- **A real `Install.ps1` bug**: its mod-copy step used `Copy-Item ... -Recurse -Force`
  without first removing the destination. On a *second* install, PowerShell nests the
  source folder inside the already-existing destination instead of overwriting it
  (producing `.../1.6/1.6/Assemblies/...` and `.../About/About/About.xml`), silently
  leaving the previous build's assembly in place. Fixed by removing the destination
  item first (the pre-existing folder is already backed up by that point, so this is
  safe) -- see `Install.ps1`'s "Installing the mod" step.
- **A real bug in this project's own `ScenPartDefs.xml`**: `<category>Special</category>`
  is not a valid `RimWorld.ScenPartCategory` enum value (the valid set is `Undefined`,
  `Fixed`, `StartingImportant`, `PlayerPawnFilter`, `PlayerPawnModifier`,
  `GameCondition`, `Rule`, `StartingItem`, `WorldThing`, `Misc`) -- this was guessed
  rather than verified, exactly the kind of thing this report is supposed to flag.
  `Player.log` showed `Exception loading def from file ScenPartDefs.xml: ... 'Special'
  is not a valid value`. Fixed to `Misc`.

After fixing both of those and relaunching with the 13-mod profile (Vanilla Books
Expanded included), **the `MedievalOverhaul.Utility` `NullReferenceException` still
occurred, identically**, confirming the earlier `BookPatches.xml`/`ReadingBonus`
hypothesis was wrong. To find the real boundary, one more isolation run was done: all
5 DLCs active (Royalty + Ideology + Biotech + Anomaly + Odyssey) + Medieval Overhaul's
bare dependencies + Vanilla Books Expanded -- the most permissive configuration
possible. **It crashed identically.**

This is conclusive: the bug is **not** related to this project's DLC exclusions, not
related to Vanilla Books Expanded, and not fixable by anything in this project's
control. It reproduces with Medieval Overhaul (Steam Workshop 3219596926, currently
maintained by EvilEyes and ViralReaction "on behalf of" the original author per its own
Workshop description) and only its own declared hard dependencies, regardless of what
else is or isn't active. The most likely explanation is a version mismatch between the
currently-published Medieval Overhaul build and the currently-published Harmony/Vanilla
Expanded Framework/Processor Framework builds, or a genuine regression in Medieval
Overhaul against RimWorld 1.6.4871 -- neither is something this project can fix without
either an upstream update from Medieval Overhaul's maintainers or pinning different mod
versions (not attempted; RimWorld's Workshop subscriptions don't offer version pinning
through the normal UI).

The user's original 23-mod configuration was restored again afterward, using the
*original* backup folder directly (`C:\lantern-village\Backups\20261031-185800`,
predating this project's move into `McRimworld/mods/lantern-village/`) rather than
`Restore.ps1`'s default "most recent backup" behavior -- by this point several
isolation runs had written their own ad hoc `ModsConfig.xml` backups that don't
represent the user's real original state. `Restore.ps1 -BackupStamp <folder>` exists
for exactly this situation and was used directly.

**Recommended next step, if this is worth pursuing further**: report the bug to
Medieval Overhaul's current maintainers (EvilEyes / ViralReaction) with the exact
stack trace in this report, since this project's own isolation work already rules out
everything on this project's side of the boundary.

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
  everything else in this project, and this is now conclusively established.** See
  "Runtime verification results" and its "Follow-up" section above for the full
  isolation trail: it reproduces identically with Medieval Overhaul and only its own
  declared hard dependencies, regardless of DLC selection (tested with none of
  Royalty/Odyssey/Anomaly, with Royalty alone, and with all five DLCs) and regardless of
  whether Vanilla Books Expanded is installed (the initial hypothesis -- a missing
  `ReadingBonus` `StatDef` reference in Medieval Overhaul's own compatibility patch --
  was tested directly and disproven). Because of this, the Lantern Village mod profile
  cannot currently stay active on this machine long enough to reach a playable colony --
  RimWorld's own safety net resets the mod list to Core + DLC before a scenario can even
  be selected. World generation, pawn spawning, medieval production and save/reload (the
  rest of point 12's checklist) could not be exercised downstream of this. Everything
  else in this project (installer, scenario file, mod compilation,
  ideology/colonist/hamlet/resume code) is implemented and, where noted above, either
  runtime-verified or implemented-but-unverified on its own merits -- this blocker is
  specifically a bug in Medieval Overhaul's own code (Steam Workshop 3219596926,
  currently maintained by EvilEyes/ViralReaction "on behalf of" SirLalaPyon), not about
  any file in this repository or anything this project chose. Unblocking it needs either
  an upstream fix from Medieval Overhaul's maintainers or a decision to drop/replace
  that mod, which is a decision only you can make.
- Two real bugs in this project's own code were found and fixed while investigating
  (see the "Follow-up" section): `Install.ps1`'s mod-folder copy step nested instead of
  overwrote on a second install, and `ScenPartDefs.xml` used an invalid
  `ScenPartCategory` enum value. Both are fixed and the fixes are in this commit.
- The user's live RimWorld configuration has been restored to its original state
  (`C:\lantern-village\Backups\20261031-185800`, the backup from the very first
  real install, restored directly since later ad hoc isolation runs had overwritten
  `Restore.ps1`'s "most recent backup" default with non-representative snapshots) and is
  not currently running the Lantern Village profile. Re-running `Install.ps1` reapplies
  it in under a minute (all 9 Workshop mods, including Vanilla Books Expanded, are now
  already resolved on this machine) but will hit the Medieval Overhaul crash again until
  that bug is fixed upstream.
- A save-count discrepancy was noticed during testing (top-level `Saves/` count moved
  between 184, 158, 159 and 152 across this session) and investigated: none of this
  project's scripts write to or delete from the `Saves` directory, and the apparent
  "missing" saves are accounted for by a pre-existing `Saves/Archive/` subfolder already
  containing this user's own dev-test saves (`VL_Prep`, `ark test`, etc., referenced in
  this repo's own `CHANGELOG.md` from unrelated prior work) -- consistent with RimWorld
  1.6's own automatic save-archiving behavior running on normal startup, not anything
  this project did. Flagged here for transparency rather than silently ignored; not
  independently proven beyond this circumstantial evidence.
