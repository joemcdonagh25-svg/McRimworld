# Vampire Lord — Changelog

Newest entries first.

---

## 2026-10-01 — M2 Playtest Keep (IMPLEMENTATION COMPLETE)

### What shipped
- New Game scenario **Vampire Lord** (`VampireLord_PlaytestKeep`): keep identity copy, start dialog, standing arrival.
- Starting cast: **1 Sanguophage (Vampire Lord) + 2 Baseliner thralls** via xenotype configure page.
- Starter gear/materials (food, medicine, steel/wood/granite blocks, components, rifles, gladius, parkas, hemogen packs) + Smithing / Complex Clothing research.
- `ScenPart_VampireLordPlaytestSetup`: ~15×15 granite courtyard, flagstone interior, south gate/door; Wave Director **auto-starts** on `PostGameStart`.
- Warning letters polished for Black Keep fantasy (product name remains Vampire Lord).
- About.xml declares **Biotech** dependency; rebuilt `Assemblies/VampireLord.dll`.

### What we learned
- Scenario-scoped `ScenPart` (`GenerateIntoMap` + `PostGameStart`) keeps the courtyard/auto-campaign off non-scenario colonies — better than patching global `MapGeneratorDef`.
- Place courtyard on `MapGenerator.PlayerStartSpot` before pawns arrive; re-assert start spot to courtyard center.
- Xenotype cast uses `ScenPart_ConfigPage_ConfigureStartingPawns_Xenotypes` + `XenotypeDefOf.Sanguophage` / `Baseliner`.

### Key paths
- `Defs/Scenarios/VampireLordScenario.xml`
- `Source/VampireLord/Scenario/ScenPart_VampireLordPlaytestSetup.cs`
- `Source/VampireLord/Campaign/VampireLordLetters.cs`
- `About/About.xml`
- `Assemblies/VampireLord.dll`

### Operator notes
- Junction already: `C:\McRimworld\mods\vampire-lord` → RimWorld `Mods` (pull main / this branch, rebuild not required if DLL committed).
- Enable Biotech + Vampire Lord → New Game → **Vampire Lord**.
- Debug force actions still available under Dev Mode.

### Next steps
- RUNTIME VERIFIED: New Game path through courtyard + auto campaign + warning/raid.
- Do not start M3 until Joe asks.

---

## 2026-10-01 — M1 RUNTIME VERIFIED (live RimWorld)

### What shipped
- Docs only: M1 marked **RUNTIME VERIFIED** after Joe’s live session on Windows (junction `C:\McRimworld\mods\vampire-lord` → RimWorld `Mods`).
- No code/DLL change.

### What we learned
- **Show Campaign State** prints to the Dev Mode log (`[VampireLord] Campaign state:`), not a map popup.
- Junction install works: edits under `C:\McRimworld\mods\vampire-lord` are what RimWorld loads.
- Debug path is enough to flip M1: Start Campaign → Trigger Warning → Trigger Wave → Show Campaign State with threat/wave up.

### Key paths
- `docs/MILESTONES.md`
- `docs/CHANGELOG.md`
- `README.md`

### Operator notes
- Proven sample: `CampaignActive=True`, `WaveNumber=1`, `ThreatLevel=2`, `LastWaveType=Mob`, `PendingWaveType=Hunters`.
- Optional follow-ups (not blocking M1): save/load schedule integrity; natural 3-day warning→raid without debug force.

### Next steps
- Do not start M2 until Joe asks.

---

## 2026-10-01 — M1 Wave Director V0 (IMPLEMENTATION COMPLETE)

### What shipped
- New RimWorld 1.6 mod **Vampire Lord** under `mods/vampire-lord/` in McRimworld.
- Wave Director V0: campaign `GameComponent`, absolute-tick scheduling (1-day warning, 3-day gap), advance-warning letters, five archetypes via vanilla `RaidEnemy`, threat/raid-point escalation, Dev Mode debug actions, save/load scribe fields.
- Docs: `docs/MILESTONES.md`, `docs/RIMWORLD_API_NOTES.md`, this CHANGELOG.
- Built `Assemblies/VampireLord.dll` (0 errors) against `Krafs.Rimworld.Ref` 1.6.4871 in Cloud; csproj also accepts local Steam Managed path.

### What we learned
- Prefer `VampireLord*` types for mod identity; keep gothic keep letter flavour (Torchlight / Black Keep place fantasy) without renaming the product to Black Keep.
- `GenDate` lives in `RimWorld`, not `Verse`.
- `RaidStrategyDefOf` only exposes a few strategies; Breaching/Siege/Smart need `DefDatabase<RaidStrategyDef>.GetNamedSilentFail`.
- Fire waves cannot force Molotov kits through `IncidentParms` alone — documented limitation.

### Key paths
- `mods/vampire-lord/About/About.xml` — packageId `joemcdonagh.vampirelord`
- `mods/vampire-lord/Source/VampireLord/`
- `mods/vampire-lord/Assemblies/VampireLord.dll`
- `mods/vampire-lord/docs/`

### Operator notes
- Campaign does **not** auto-enable. Use Dev Mode → Vampire Lord → **Start Vampire Lord Campaign**.
- Junction/copy `mods/vampire-lord` into RimWorld `Mods` (sibling to The Ark).
- Build: `dotnet build ./mods/vampire-lord/Source/VampireLord/VampireLord.csproj`

### Next steps
- RUNTIME VERIFIED pass in RimWorld (save/load + letter + raid + strategy defNames).
- Do not start M2 until Joe asks.
