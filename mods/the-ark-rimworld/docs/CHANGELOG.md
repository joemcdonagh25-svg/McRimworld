# The Ark — Changelog

Newest entries first. Records what shipped, what we learned, key paths, operator notes, and next steps.

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
