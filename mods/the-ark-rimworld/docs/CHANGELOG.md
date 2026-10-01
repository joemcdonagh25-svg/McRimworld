# The Ark — Changelog

Newest entries first. Records what shipped, what we learned, key paths, operator notes, and next steps.

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
