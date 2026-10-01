# Vampire Lord — Changelog

Newest entries first.

---

## 2026-10-01 — M2 open gate + reveal outside map

### What shipped
- South **open gate** (3 cells, no door) so pathing to the wilds is never blocked.
- After fog gen: **unfog** keep + 40-cell padding and flood-unfog the gate so outside terrain is visible.
- Clear a short approach lane south of the gate.
- Pre-generate player ideoligion when Ideology is active (mitigates ChooseIdeoPreset NRE).
- Reassign humanlike map pawns to player faction at game start if needed.
- Drop `ParentName="ScenarioBase"` (explicit PlayerFaction / PlanetLayerFixed only).
- Rebuild DLL.

### What we learned
- Fog applies after `GenerateIntoMap`; revealing must happen in `PostMapGenerate` or the wilds look like empty void.
- Closed doors + fog made the keep feel like a sealed void with “no path outside”.
- Ideology `Page_ChooseIdeoPreset` NRE still appears in Joe’s stack during settle; generating an ideo early + skipping ScenarioBase may reduce broken new-game state (non-colonist / empty history).

### Key paths
- `Source/VampireLord/Scenario/ScenPart_VampireLordPlaytestSetup.cs`
- `Defs/Scenarios/VampireLordScenario.xml`

### Operator notes
- Pull + restart + **New Game**. You should see terrain outside the open south gate and be able to walk out.

### Next steps
- Joe verify outside map + pathing; note whether Ideology page still errors.

---

## 2026-10-01 — M2 keep walls owned by player + gladius stuff

### What shipped
- Courtyard walls/door now `SetFaction` to **Faction.OfPlayer** so they can be deconstructed / managed as colony buildings.
- Place courtyard **before** pawn arrival; stop wiping starting items when clearing the footprint.
- Gladius starting thing gets `<stuff>Steel</stuff>` (fixes MakeThing madeFromStuff error).
- Rebuilt DLL.

### What we learned
- Unfactioned spawned walls look like ruins: not claimable/deconstructable the way players expect.
- Clearing `ThingCategory.Item` in the courtyard after gear drop could destroy starter loot.

### Key paths
- `Source/VampireLord/Scenario/ScenPart_VampireLordPlaytestSetup.cs`
- `Defs/Scenarios/VampireLordScenario.xml`

### Operator notes
- Pull + restart + **New Game** (old saves keep old unfactioned walls).
- Ideology/Anomaly NREs in Joe’s log look separate from courtyard ownership; re-check after this fix.

### Next steps
- Joe retry New Game; confirm walls deconstruct and campaign still auto-starts.

---

## 2026-10-01 — M2 scenario config fix (1.6 required parts)

### What shipped
- Fix `VampireLord_PlaytestKeep` ConfigErrors: add `ScenPart_PlayerFaction`, `ScenPart_PlanetLayerFixed` (Surface), `ParentName="ScenarioBase"`.
- Add `ScenPartDef`s for start dialog + playtest setup; wire `<def>` on those parts (was causing null def / NRE on Next).
- Rebuild DLL (`HasNullDefs` safety on custom ScenPart).

### What we learned
- RimWorld 1.6 scenario validation requires playerFaction + surfaceLayer; missing them blocks scenario Next with NullReferenceException.
- Scenario-embedded custom ScenParts still need a `ScenPartDef` (or null-def handling) for ErrorCheckAllDefs.

### Key paths
- `Defs/Scenarios/VampireLordScenario.xml`
- `Defs/ScenParts/VampireLordScenParts.xml`
- `Source/VampireLord/Scenario/ScenPart_VampireLordPlaytestSetup.cs`

### Operator notes
- Pull / restart RimWorld after merge. New Game → Vampire Lord should proceed past scenario select.

### Next steps
- Joe RUNTIME VERIFIED on New Game path.

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
