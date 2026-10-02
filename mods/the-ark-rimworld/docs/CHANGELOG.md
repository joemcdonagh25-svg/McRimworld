# The Ark — Changelog

Newest entries first. Records what shipped, what we learned, key paths, operator notes, and next steps.

---

## 2026-10-01 — M3 Landing Detection RUNTIME VERIFIED (Joe Player.log)

### What shipped
- Docs only: mark M3 **RUNTIME VERIFIED** from Joe’s live Simulate Landing.

### What we learned
- Log: `[The Ark] Landing session STARTED (Dev.SimulateLanding): mapId=0, LandingNumber=1, Session=True`
- New Game start still shows known Ideology/Anomaly/History noise; does not block M3 proof.

### Operator notes
- No M4 Landing Timer until Joe asks.

### Next steps
- Stop; Joe chooses next Ark milestone when ready.

---

## 2026-10-01 — M3 Landing Detection V0

### What shipped
- Landing session on `ArkCampaignGameComponent`: begin once → `LandingNumber++`; ignore while session active.
- Detection without Harmony: Odyssey travel edge + `wasSpawnedViaGravShipLanding` poll; `ScenPart.PostGravshipLanded`.
- Dev Mode: **Simulate Landing**, **End Landing Session**; debug UI + state strings include session fields.
- Rebuild `Assemblies/TheArk.dll`.

### What we learned
- `WorldComponent_GravshipController.IsGravshipTravelling` + map flag are usable without patching `InitiateLanding`.
- Surface playtest still needs Dev Simulate for proof until a real gravship New Game / hop exists.

### Key paths
- `Source/TheArk/Campaign/ArkCampaignGameComponent.cs`
- `Source/TheArk/Scenario/ScenPart_ArkPlaytestSetup.cs`
- `Source/TheArk/Debug/ArkCampaignDebugOps.cs`
- `Source/TheArk/Debug/ArkCampaignDebugActions.cs`
- `docs/RIMWORLD_API_NOTES.md`

### Operator notes
1. Pull / merge → restart
2. New Game → The Ark (or load campaign save)
3. Dev Mode → The Ark (DEV) → **Simulate Landing**
4. Expect `Landing session STARTED` and LandingNumber +1; second Simulate ignored until **End Landing Session**

### Next steps
- Done: Joe RUNTIME VERIFIED via Simulate Landing (see entry above).

---

## 2026-10-01 — M1 + M2 RUNTIME VERIFIED (Joe Player.log)

### What shipped
- Docs only: mark M1 Persistent Campaign + M2 Campaign Debug UI **RUNTIME VERIFIED** from Joe’s live RimWorld session.

### What we learned
- Fixture apply: `[The Ark] [DEV] Applied M1 persistence fixture: Active=True, Day=47, Landing=6, Tier=2, Pursuit=73`
- After full quit + load save `New Arrivals6`: `[The Ark] Campaign state (LoadedGame): Active=True, Day=47, Landing=6, Tier=2, Pursuit=73` — exact match.
- New Game path also confirmed earlier: ScenarioDef listed, ideo generate, 3 colonists reassigned, campaign Active=True.
- Known noise (not blocking M1 proof): ChooseIdeoPreset NRE, Anomaly StartedNewGame NRE, History empty-sequence, Ideo_12 missing on load (Ideology settle skip residue). Campaign fields still persist correctly.

### Operator notes
- M1 + M2 playtest loop is proven on Joe’s machine.
- No M3 Landing Detection until Joe asks.

### Next steps
- Stop for approval; Joe chooses next Ark milestone when ready.

---

## 2026-10-01 — New Game → The Ark RUNTIME CONFIRMED (Joe Player.log)

### What shipped
- Docs only: record Joe’s live confirm after PR #19.

### What we learned
- Success lines present: ScenarioDef loaded, ScenarioLister contains The Ark, ideoligion generated (`Haxor-Mankindism`), **Reassigned 3** humanlike pawns to player faction, campaign `Active=True` at `Scenario.PostGameStart` and `StartedNewGame`.
- Known noise still fires and did not block playable start: `Page_ChooseIdeoPreset` NRE, Anomaly `StartedNewGame` NRE, History `Sequence contains no elements`.

### Operator notes
- New Game start path is good enough for playtest.
- Superseded: full M1/M2 RUNTIME VERIFIED claimed in newer entry after fixture save/load.

### Next steps
- Done (see M1 + M2 RUNTIME VERIFIED entry).

---

## 2026-10-01 — Harden Ideo/colonist start after settle (Joe Player.log)

### What shipped
- Re-wire `ScenPart_ArkPlaytestSetup` into The Ark scenario (was stripped during minimal-scenario diagnostics).
- On `PostWorldGenerate` / `PostGameStart`: generate player ideoligion if Ideology is on and missing; reassign humanlike map pawns to player faction.
- Start dialog + minimal starting supplies (meals, medicine, steel, components).
- Rebuild `Assemblies/TheArk.dll`.

### What we learned
- After ConfigurePawns fix, Joe’s log shows clean scenario load + campaign activate, then `Page_ChooseIdeoPreset.PostOpen` NRE on settle (same class as Vampire Lord).
- Cascading tick noise followed: Anomaly `StartedNewGame` NRE, empty History, goodwill, **non-colonist** apparel — VL’s colonist reassignment is the practical harden.
- ChooseIdeoPreset NRE may still appear in the log; game should continue with a generated ideo.

### Key paths
- `Source/TheArk/Scenario/ScenPart_ArkPlaytestSetup.cs`
- `Defs/ScenParts/TheArkScenParts.xml`
- `Defs/Scenarios/TheArkScenario.xml`
- `Assemblies/TheArk.dll`

### Operator notes
1. Pull `main` after merge
2. Restart RimWorld (The Ark + Biotech)
3. New Game → **The Ark** → pick site → Next
4. Expect possible Ideology NRE in log; colony crew should be player colonists; campaign active
5. Dev Mode → The Ark (DEV) → Apply M1 Persistence Fixture for save/load proof

### Next steps
- Done: Joe confirmed playable start (see entry above). Fixture save/load still pending.

---

## 2026-10-01 — Fix ConfigurePawns null def (scenario ConfigError)

### What shipped
- Replace invalid `<def>ConfigurePawns</def>` with **`ConfigurePawnsXenotypes`** + 3x Baseliner (proven on Joe's machine via Vampire Lord).
- About.xml: Biotech dependency for Baseliner cast; install note about duplicate `Mods\TheArk`.
- Junction script warns if old `Mods\TheArk` still exists.

### What we learned
- Joe's Player.log: `No RimWorld.ScenPartDef named ConfigurePawns found` → ConfigError → scenario broken in New Game UI.
- Also: duplicate packageId folders `Mods\TheArk` and `Mods\the-ark-rimworld` (RimWorld ignores duplicates).
- ScenarioLister still contained The Ark, but ConfigError prevented usable listing/selection.

### Key paths
- `Defs/Scenarios/TheArkScenario.xml`
- `About/About.xml`
- `tools/junction-the-ark.ps1`

### Operator notes
1. Pull/merge
2. Delete `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\TheArk`
3. Keep junction `Mods\the-ark-rimworld` → `C:\McRimworld\mods\the-ark-rimworld`
4. Enable The Ark + Biotech → restart → New Game → **The Ark**

### Next steps
- Joe confirm scenario appears with no ConfigurePawns error.

---

## 2026-10-01 — Minimal scenario + install diagnostics

### What shipped
- Playtest scenario stripped to required 1.6 parts only (PlayerFaction, PlanetLayerFixed, ConfigurePawns, Standing arrival).
- Loud Player.log diagnostics: mod root/packageId, related mods, ScenarioDef ConfigErrors, ScenarioLister contains check.
- `tools/junction-the-ark.ps1` to create the Windows Mods junction to `C:\McRimworld\mods\the-ark-rimworld`.

### What we learned
- Vampire Lord listing works for Joe; Ark still missing strongly suggests The Ark mod root is not junctioned/enabled (or an old packageId copy shadows it), not missing GitHub XML.

### Key paths
- `Defs/Scenarios/TheArkScenario.xml`
- `Source/TheArk/TheArkBootstrap.cs`
- `tools/junction-the-ark.ps1`

### Operator notes
1. Pull this change
2. PowerShell: `mods\the-ark-rimworld\tools\junction-the-ark.ps1`
3. Enable The Ark → restart
4. Paste every Player.log line that starts with `[The Ark]`

### Next steps
- Joe paste `[The Ark]` log lines if still missing from New Game.

---

## 2026-10-01 — Scenario visibility: vanilla parts + load diagnostic

### What shipped
- Playtest scenario parts are **vanilla-only** (no custom ScenPart Class) so listing does not depend on resolving mod C# types in XML.
- Startup log after defs load: `ScenarioDef TheArk_Playtest LOADED` or a clear MISSING warning with install checklist.
- Campaign still activates via `ArkCampaignGameComponent` when scenario name is The Ark.

### What we learned
- If Player.log never shows `[The Ark] Initialised successfully.`, the mod folder is not the McRimworld `mods/the-ark-rimworld` root (or the mod is disabled).
- A second older The Ark install with the same `packageId` can shadow the McRimworld copy that has `Defs/`.

### Key paths
- `Defs/Scenarios/TheArkScenario.xml`
- `Source/TheArk/TheArkBootstrap.cs`
- `Source/TheArk/Campaign/ArkCampaignGameComponent.cs`

### Operator notes
1. Pull `main` / this PR on `C:\McRimworld`
2. Junction **only** `C:\McRimworld\mods\the-ark-rimworld` → RimWorld `Mods\the-ark-rimworld`
3. Remove/disable any other The Ark mod folder
4. Enable The Ark → restart → search Player.log for `TheArk_Playtest LOADED`
5. New Game → The Ark

### Next steps
- Joe confirm LOADED line + scenario list entry.

---

## 2026-10-01 — Fix: New Game scenario not listing

### What shipped
- Removed hard `modDependencies` on Odyssey so The Ark can enable and list its scenario without Odyssey active (V1.1 is a surface playtest).
- Kept `loadAfter` Odyssey when present.
- `ArkCampaignGameComponent.StartedNewGame` also activates campaign when `Find.Scenario.name == "The Ark"` (backup to ScenPart).
- ASCII-only scenario copy (avoid special punctuation in XML text).

### What we learned
- Unmet hard DLC dependencies often leave a mod unchecked/disabled → its ScenarioDefs never appear in New Game.
- Scenario listing also requires the mod root junction (`…/mods/the-ark-rimworld` containing `About/`), not the McRimworld parent folder.

### Key paths
- `About/About.xml`
- `Defs/Scenarios/TheArkScenario.xml`
- `Source/TheArk/Campaign/ArkCampaignGameComponent.cs`

### Operator notes
1. `git pull` on `C:\McRimworld`
2. Junction `C:\McRimworld\mods\the-ark-rimworld` → RimWorld `Mods\the-ark-rimworld` (create if missing)
3. Mods list: enable **The Ark** (Odyssey optional for this playtest)
4. Restart RimWorld → New Game → **The Ark**

### Next steps
- Joe confirm scenario appears; then M1 fixture save/load proof.

---

## 2026-10-01 — M2 / V1.1 Campaign Debug UI (implementation)

### What shipped
- New Game scenario **The Ark** (`TheArk_Playtest`) + `ScenPart_ArkPlaytestSetup` (sets `CampaignActive` on start; Odyssey dependency in About).
- RimWorld 1.6 scenario scaffolding: `ParentName="ScenarioBase"`, `PlayerFaction`, `PlanetLayerFixed`, `Defs/ScenParts/TheArkScenParts.xml` (avoids New Game Next NRE).
- Dev Mode category **The Ark (DEV)** with Open Campaign Debug, Log Campaign State, Apply M1 Persistence Fixture.
- Crude `[DEV] The Ark — Campaign Debug` window: view/edit all five M1 fields; Apply / Refresh / Fixture.
- Explicit writes via `ArkCampaignDebugOps` into `ArkCampaignGameComponent` only (no duplicate state).
- `TheArk.csproj`: Steam Managed path when present; else `Krafs.Rimworld.Ref` 1.6.4871 for Cloud/CI.
- Rebuilt `Assemblies/TheArk.dll` (0 errors / 0 warnings).
- Docs: API notes, milestones (M2 IMPLEMENTATION COMPLETE; runtime pending).

### What we learned
- `Listing_Standard.TextFieldNumericLabeled` is enough for integer draft buffers without owning campaign state.
- `onlyDrawInDevMode` + `Prefs.DevMode` keep this out of normal player UI; debug-actions menu is the entry point (same family as Vampire Lord).
- Scenario-scoped `ScenPart` keeps campaign auto-activate off sandbox/other starts; gravship wreckage start is still deferred (no inventing Odyssey start XML).

### Key paths
- `Defs/Scenarios/TheArkScenario.xml`
- `Source/TheArk/Scenario/ScenPart_ArkPlaytestSetup.cs`
- `About/About.xml`
- `Source/TheArk/Debug/ArkCampaignDebugOps.cs`
- `Source/TheArk/Debug/Dialog_ArkCampaignDebug.cs`
- `Source/TheArk/Debug/ArkCampaignDebugActions.cs`
- `Source/TheArk/TheArk.csproj`
- `Assemblies/TheArk.dll`
- `docs/RIMWORLD_API_NOTES.md`
- `docs/MILESTONES.md`

### Operator notes
- Build: `dotnet build .\Source\TheArk\TheArk.csproj` (from `mods/the-ark-rimworld`)
- Junction/copy `mods/the-ark-rimworld/` into RimWorld `Mods`.
- New Game → **The Ark** (Odyssey on) → Dev Mode → **Apply M1 Persistence Fixture** → save → full quit → load.
- Fixture values: Active=true, Day=47, Landing=6, Tier=2, Pursuit=73.
- Do not mark M1/M2 RUNTIME VERIFIED until save → full quit → load matches.

### Next steps
- Joe: run the M1 persistence fixture test via New Game → The Ark.
- On approval only: V1.2 / M3 Landing Detection (inspect Odyssey APIs first).

---

## 2026-09-30 — M1 Persistent Campaign Foundation (implementation)

### What shipped
- Authoritative durable campaign state on `TheArk.Campaign.ArkCampaignGameComponent` (`Verse.GameComponent`).
- Five primitives only (no gameplay effects): `CampaignActive`, `CampaignDay`, `LandingNumber`, `ArkTier`, `Pursuit`.
- Stable Scribe labels: `arkCampaignActive`, `arkCampaignDay`, `arkLandingNumber`, `arkTier`, `arkPursuit`.
- Thin access helper `TheArk.Campaign.ArkCampaign` (`TryGet` / `Get`) — no duplicate state cache.
- Concise `StartedNewGame` / `LoadedGame` log lines for manual verification.
- Documented M1 persistence investigation + implementation notes in `docs/RIMWORLD_API_NOTES.md`.
- Milestone status: **IMPLEMENTATION COMPLETE**; **RUNTIME VERIFIED** still pending in-RimWorld save/load.

### What we learned
- RimWorld 1.6 auto-discovers non-abstract `GameComponent` subclasses via `Game.FillComponents` and constructs them with `Activator.CreateInstance(type, new object[] { game })`.
- `MapComponent` is the wrong owner for campaign fields that must survive gravship map churn; Odyssey’s `WorldComponent_GravshipController` is the ship-ops hook, not Ark meta-state.

### Key paths
- `Source/TheArk/Campaign/ArkCampaignGameComponent.cs`
- `Source/TheArk/Campaign/ArkCampaign.cs`
- `Assemblies/TheArk.dll`
- `docs/RIMWORLD_API_NOTES.md`
- `docs/MILESTONES.md`

### Operator notes
- Build: `dotnet build .\Source\TheArk\TheArk.csproj`
- Manual proof: new game → campaign log → (optional debugger set non-defaults) → save → full quit → load → `LoadedGame` log must match.
- Do not claim runtime verification until that RimWorld test is done.

### Next steps
- Run the M1 manual RimWorld save/load test.
- On request only: M2 Campaign Debug UI.
