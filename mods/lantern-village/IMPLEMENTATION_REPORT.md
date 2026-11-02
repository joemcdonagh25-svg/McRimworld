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

## SUCCESS: a real colony is running (2026-11-01)

**The Medieval Overhaul blocker is fixed, and a full playthrough was driven end to end
with computer-use automation, on this real machine, with the real 13-mod profile
active.** This supersedes every "Blocked" note below about Medieval Overhaul.

### The actual root cause (found via IL disassembly, not guessing)

Disassembling `MedievalOverhaul.dll` with Mono.Cecil (see
`Mods/LanternVillage/Source/LanternVillage/CompatPatch_MedievalOverhaul.cs` for the
full writeup) found the real bug, two method calls deep from what the stack trace
showed:

1. `MedievalOverhaul.MedievalOverhaulSettings`'s constructor calls
   `Verse.ModLister.GetActiveModWithIdentifier("DankPyon.Medieval.Overhaul", true)`
   purely to log its own version string, then calls `.ModVersion` on the result with no
   null check. In this RimWorld build that lookup returns null at the point mod classes
   are constructed, so the constructor throws -- reproduced with Medieval Overhaul and
   *only* its own declared hard dependencies, independent of every DLC combination and
   every other mod tested (see the "Follow-up" section below for that trail).
2. RimWorld's `LoadedModManager.CreateModClasses()` catches that and logs it as
   non-fatal, but `MedievalOverhaul.Utility`'s own static constructor later calls
   `LoadedModManager.GetMod<MedievalOverhaulSettings>().Content` -- since that mod
   instance was never successfully constructed, `GetMod<T>()` returns null, and the
   unchecked `.Content` access throws a second, fatal `NullReferenceException` during
   `PlayDataLoader.DoPlayLoad()`, which RimWorld cannot recover from. This second
   exception is the one the earlier isolation testing chased.

Neither the "Vanilla Books Expanded" hypothesis nor the DLC-exclusion hypothesis was
the real cause; both were ruled out by direct testing, not assumed.

### The fix: a defensive Harmony patch, with correct timing

`CompatPatch_MedievalOverhaul.cs` applies a Harmony **finalizer** patch to
`MedievalOverhaulSettings`'s constructor (found via `AccessTools.TypeByName`, so this
mod has no hard compile-time or load-time dependency on Medieval Overhaul and does
nothing if it isn't installed) that swallows the first exception, letting the
constructor return normally. That alone wasn't enough on the first attempt -- two
timing/ordering bugs had to be found and fixed too, each one caught by actually
relaunching and reading `Player.log`, not by reasoning about the code:

- The patch must be applied from **this mod's own `Mod`-class constructor**, not a
  `[StaticConstructorOnStartup]` class. `StaticConstructorOnStartup` code runs *after*
  every mod's `Mod`-class constructor has already executed in `CreateModClasses()` --
  by then Medieval Overhaul's constructor has already thrown. Moved into
  `LanternVillageMod`'s own constructor instead.
- Being declared `loadBefore` Medieval Overhaul in `About.xml` was **not sufficient**
  on its own -- confirmed by testing: with that alone, the crash still happened
  identically. `loadBefore`/`loadAfter` only affects Def/content loading order;
  `LoadedModManager.CreateModClasses()` instantiates every active mod's `Mod`-class in
  **`ModsConfig.xml`'s raw list order**, a separate, earlier phase. The actual fix was
  moving `lanternvillage.thelanternvillage`'s entry in `manifest/mod-manifest.json`
  (and so in the `ModsConfig.xml` `Install.ps1` writes) to sit immediately before
  `DankPyon.Medieval.Overhaul`, confirmed by the next relaunch showing
  `[Lantern Village] Applied Medieval Overhaul compatibility patch` *before*
  `Error while instantiating a mod of type MedievalOverhaul.MedievalOverhaulSettings`
  disappeared from the log entirely.

Two more real, unrelated bugs were caught in the same debugging pass, purely from
reading `Player.log` after relaunches:
- An XML comment in `About.xml` containing `--` (illegal inside any XML comment),
  which broke the *entire file's* parsing, silently defaulting this mod's own
  `packageId` and `supportedVersions` -- RimWorld logged
  `Mod LanternVillage is missing packageId in About.xml!` Fixed by removing the `--`.
- `ScenPart_LanternVillageSetup.cs` used `"Medical"` as a `SkillDef` name; RimWorld's
  real skill is `"Medicine"`. Caught from `Player.log`:
  `SkillDef 'Medical' not found; cannot apply Elder's floor.` -- non-fatal (the code's
  own `GetNamedSilentFail` + warning-and-skip design meant it degraded gracefully
  instead of crashing), but wrong. Fixed in both the C# and the manifest.

### What was actually seen running, on screen, this session

With the full, corrected 13-mod profile active (Harmony, Vanilla Expanded Framework,
[SYR] Processor Framework, Vanilla Books Expanded, Medieval Overhaul, Dark Ages: Beasts
and Monsters, World Tech Level, GloomyFurniture, Realistic Darkness 1.6, plus Ideology
and Biotech), driven entirely with computer-use mouse/keyboard automation:

1. RimWorld reaches the main menu -- no crash, no `ModsConfigData` reset.
2. **The Lantern Village** appears under a "Custom" section on the in-game scenario
   list and is selectable, with its real summary text and starting-supply list
   (Pemmican x600, Medicine x20, Wood plank x500, Short bow x2, Recurve bow, Steel
   knife, Wooden club, Silver x150, Muffalo x2) displayed correctly by RimWorld's own
   UI -- matching the manifest exactly.
3. The storyteller page opens with **Cassandra Classic** and **Strive to survive**
   already selected -- confirms `HarmonyPatches_Storyteller.cs` works.
4. World generation completes; a landing site can be selected; character creation
   shows exactly 5 colonists, matching the scenario's `pawnCount`.
5. Clicking Start enters the actual game. The custom `ScenPart_GameStartDialog` text
   ("Five settlers step into a forest clearing...") displays verbatim.
6. **A real map loads with 5 colonist pawns and the configured starting supplies
   visible and selectable on the ground** (wood log stacks, silver, muffalo visible in
   screenshots taken during the session).
7. `Player.log` confirms `[Lantern Village] Horror director entering phase
   'Foundation' (days 1-30).` -- `HorrorDirector`'s `GameComponent` is alive and
   ticking in a real running game, not just compiling.

This is runtime verification of M1 through M4 in the categories below, not merely
"implemented but not runtime-verified" -- moved accordingly.

### Not yet verified in this pass

Dismissing the in-game debug-log overlay and selecting an individual colonist to read
back their renamed role/skills/relationships in the UI (to directly confirm
`ScenPart_LanternVillageSetup`'s pawn-side effects, beyond the log evidence above) and
the ideoligion's actual generated content were not completed before this session ended
-- `Player.log` shows no error from that ScenPart and shows the Medicine-skill warning
resolved by the fix above, but the roles/relationships were not visually confirmed pawn
by pawn. Hamlet Mode's map generation, the resume flow, and a full day/night cycle are
similarly still only implemented-and-building-clean, not watched running. Two
cosmetic-only, non-fatal issues remain in the log and were not chased further this
session: `Config error in LanternVillageSetup: scenPartClass is null` (our
`ScenPartDef` lacks a `scenPartClass` XML field some validator expects, even though the
`.rsc`'s own `Class="..."` attribute is what actually matters) and a handful of
`Could not resolve cross-reference to Verse.ThingDef named DankPyon_Hide_*` warnings
from Medieval Overhaul/Dark Ages Beasts content unrelated to anything in this project.

### State left on this machine

The user's original 23-mod `ModsConfig.xml`, `Scenarios` folder, and the absence of a
`LanternVillage` mods folder have all been restored and verified clean after this run
(no RimWorld process left running). `manifest/mod-manifest.json` and `About.xml` now
encode the correct 13-mod profile and load-order fix; re-running `Install.ps1` and
relaunching reproduces the working state above.

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
- **Storyteller/difficulty defaults** (`HarmonyPatches_Storyteller.cs`): **now
  runtime-verified** (2026-11-01 session) -- the storyteller page was observed opening
  with Cassandra Classic and "Strive to survive" already selected. Moved from this
  list; see "SUCCESS" section above. (Still a default, not a lock -- the player can
  change it.)
- **Colonist roles** (`ScenPart_LanternVillageSetup.cs`): raises the five starting
  pawns' skills to the specified floors, adds one fitting trait each, and wires an
  Elder-Healer `Spouse` relation and an Elder-Hunter `Parent` relation via
  `Pawn_RelationsTracker.AddDirectRelation` (confirmed real API). Partially
  runtime-verified: the 2026-11-01 session confirmed the ScenPart runs without error
  (and fixed a real `"Medical"` vs `"Medicine"` `SkillDef`-name bug found via
  `Player.log`), and the scenario correctly produced exactly 5 colonists on the
  character-creation screen. Not yet individually confirmed: the exact *direction*
  semantics of the parent/child relation, and visually inspecting a spawned colonist's
  renamed role/skills/relationships in the in-game UI pawn by pawn.
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

- ~~Medieval Overhaul crashes play-data loading~~ **RESOLVED 2026-11-01.** The crash was
  real and was conclusively isolated to a specific bug in Medieval Overhaul's own
  `MedievalOverhaulSettings` constructor (found by disassembling its IL, not guessing --
  see the "SUCCESS" section at the top of this report for the exact root cause and the
  fix). It is worked around by a defensive Harmony finalizer patch shipped in this
  mod (`CompatPatch_MedievalOverhaul.cs`), applied at the correct point in mod-load
  order (this mod's entry was moved to sit immediately before Medieval Overhaul's in
  `ModsConfig.xml`). A full colony was started and run on screen with the real 13-mod
  profile active. This did not need an upstream fix or dropping the mod after all.
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
