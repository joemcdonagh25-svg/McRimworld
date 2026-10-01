# Vampire Lord — Changelog

Newest entries first.

---

## 2026-10-01 — M4 planning: Keep Fortification V0

### What shipped
- Docs only: M4 milestone card — **Keep Fortification V0** (spend Blood Tithe between waves on gate sandbags).
- Parking lot updated: approach lanes, UI panel, prisoners, Dark Boons remain deferred.
- No M4 code until Joe confirms this card (or picks a different M4).

### What we learned
- After threat + stage + blood stockpile, the missing loop piece is a **blood sink that changes the keep** before the next host.
- V0 stays tiny: one purchase type, letter + debug, no Harmony, no architect tab.

### Key paths
- `docs/MILESTONES.md` (M4 card)
- `README.md`

### Operator notes
- Read the M4 card; say **confirm Keep Fortification** to start impl, or name another parking-lot candidate.

### Next steps
- Joe confirm or redirect; then implementation branch.

---

## 2026-10-01 — Fix: keep is a real player home (not caravan)

### What shipped
- Playtest start / save load ensures the keep map is a player `Settlement` with `IsPlayerHome` true.
- If the MapParent is already a Settlement with the wrong faction → `SetFaction(OfPlayer)`.
- If it is a Camp or other non-Settlement parent → `SettleInExistingMapUtility.Settle` (same as caravan "Settle here").
- Debug: **Ensure Player Home**.
- DLL rebuild.

### What we learned
- Raid colonist-map fallback let waves fire, but RimWorld still treated the keep like a caravan/camp when `IsPlayerHome` stayed false.
- That matches Joe's `GetSituations(PlayerColony)` / faction-dialog NRE spam and "thinks I'm a caravan" feel — world/faction UI expects a player Settlement.
- Fix the settle state; keep the raid fallback as a safety net.

### Key paths
- `Source/VampireLord/Scenario/VampireLordPlayerHome.cs`
- `Source/VampireLord/Scenario/ScenPart_VampireLordPlaytestSetup.cs`
- `Source/VampireLord/Campaign/VampireLordCampaignGameComponent.cs`
- `Source/VampireLord/Debug/VampireLordDebugActions.cs`
- `Assemblies/VampireLord.dll`

### Operator notes
1. Pull + restart RimWorld.
2. Prefer a **new** Vampire Lord game (cleanest). Or load the current keep and use Dev Mode → Vampire Lord → **Ensure Player Home**.
3. Success log: `Ensured player home ... after=IsPlayerHome=True, Parent=Settlement:.../faction=PlayerColony`.
4. Colony UI should feel like a normal colony (not caravan); Trigger Wave still works.

### Next steps
- Joe retest: new game or Ensure Player Home on the save; confirm no caravan feel and quieter faction UI errors.

---

## 2026-10-01 — Fix: wave raid "no player home map"

### What shipped
- Raid target map resolution falls back to any map with free colonists (then CurrentMap) when `IsPlayerHome` / `AnyPlayerHomeMap` are empty.
- Clearer failReason logging if still no map.
- DLL rebuild.

### What we learned
- Joe's Trigger Wave Now failed with `no player home map` after Blood Tithe merge — playtest settle can leave the keep map playable without `IsPlayerHome` true.
- Prefer home maps when present; colonist-bearing map is enough to fire a forced raid.

### Key paths
- `Source/VampireLord/Campaign/VampireLordRaidLauncher.cs`
- `Assemblies/VampireLord.dll`

### Operator notes
- Pull + restart. New or existing Vampire Lord game → Dev Mode → **Trigger Wave Now** should launch (or a more specific fail reason).

### Next steps
- Joe retest wave launch + Blood Tithe starved path.

---

## 2026-10-01 — M3 Blood Tithe V0 (implementation)

### What shipped
- Keep **Blood Reserve** on campaign component (scribed; starts at 40 on activate).
- Fresh hostile humanlike corpses credit +5 while campaign active (corpse scan, no Harmony).
- Wave launch pays tithe `10 + 2*threat`; unpaid → raid points ×1.35 + starved letter.
- Harvest letter when the next wave starts if kills accrued; warning letters show reserve + cost due.
- Debug: Show Blood Tithe, Add/Spend (+/−20), Force Low Blood.
- Failed raid launch refunds spent tithe.
- DLL rebuilt.

### What we learned
- Corpse scan + age gate is enough for V0 without Harmony or Biotech hemogen rewrites.
- Tithe-at-launch is clearer prep pressure than a silent time drain.
- Refund on failed launch avoids punishing map/edge-case raid failures.

### Key paths
- `Source/VampireLord/Campaign/VampireLordBloodTithe.cs`
- `Source/VampireLord/Campaign/VampireLordCampaignGameComponent.cs`
- `Source/VampireLord/Campaign/VampireLordWaveDirector.cs`
- `Source/VampireLord/Campaign/VampireLordLetters.cs`
- `Source/VampireLord/Debug/VampireLordDebugActions.cs`
- `Assemblies/VampireLord.dll`

### Operator notes
1. Pull `C:\McRimworld`, restart RimWorld
2. New Game → Vampire Lord (or Dev Mode Start Campaign)
3. **Show Blood Tithe** → Trigger Wave / Force Low Blood → kill hostiles → save/load

### Next steps
- Joe RUNTIME VERIFIED pass on the checklist in `docs/MILESTONES.md`.

---

## 2026-10-01 — M3 planning: Blood Tithe V0

### What shipped
- Docs only: M3 milestone card — **Blood Tithe V0** (keep blood reserve fed by wave kills; wave-cost pressure recommended).
- M4+ parking lot: fortification, approach lanes, UI panel, prisoners, Dark Boons, Ideology polish.
- No M3 code until Joe confirms this card (or picks a different M3).

### What we learned
- After threat (M1) + stage (M2), blood is the next unique Vampire Lord fantasy; castle upgrades/UI are amplifiers, not the identity.
- Keep V0 tiny: scribed reserve + death credits + one pressure rule + letters/debug.

### Key paths
- `docs/MILESTONES.md` (M3 card)
- `README.md`

### Operator notes
- Read the M3 card; say **confirm Blood Tithe** to start impl, or name another candidate from M4+.

### Next steps
- Joe confirm or redirect; then implementation branch.

---

## 2026-10-01 — M2 RUNTIME VERIFIED (live RimWorld)

### What shipped
- Docs only: M2 Playtest Keep marked **RUNTIME VERIFIED** after Joe's live session.
- Proven: open south gate, map reveal, courtyard, campaign auto-start, thralls reassigned to player; The Ark stayed inactive with both mods enabled.

### What we learned
- Log proof lines that matter: `open gate`, `Revealed map (PostMapGenerate)`, `Campaign activated`, `Reassigned 3 humanlike`.
- Ideology `ChooseIdeoPreset` NRE still fires on settle but does not block the playable keep; Anomaly/History errors are separate noise.
- Enabling The Ark alongside Vampire Lord is fine for this playtest.

### Key paths
- `docs/MILESTONES.md`
- `README.md`
- parent `README.md` / `CHANGELOG.md`

### Operator notes
- No pull required for gameplay (docs-only). M2 happy path is proven.

### Next steps
- No M3 until Joe asks. Optional later: Ideo settle page polish; natural warning/raid on the scenario path.

---

## 2026-10-01 — Fix: scenario ConfigErrors after outside-access merge

### What shipped
- Restored `ParentName="ScenarioBase"` on `VampireLord_PlaytestKeep` (PR #8 had dropped it).
- ASCII-only scenario description text (same lesson as The Ark list fix).
- Kept open gate + full-map reveal DLL from PR #8.

### What we learned
- Joe’s post-merge log: `no playerFaction` / `no surfaceLayer` / `scenario has null part` + Next NRE on scenario select.
- Explicit PlayerFaction / PlanetLayerFixed alone was **not** enough without `ScenarioBase` in this 1.6 + Ideology/Odyssey stack.
- Em dash in scenario copy is risky; prefer ASCII `-`.

### Key paths
- `Defs/Scenarios/VampireLordScenario.xml`

### Operator notes
- Pull + restart. New Game → Vampire Lord should pass scenario Next with no ConfigErrors.
- Then confirm log has `open gate` + `Revealed map` and walk south.

### Next steps
- Joe runtime verify scenario start + outside map.

---

## 2026-10-01 — M2 full-map reveal + paved south exit (harden)

### What shipped
- **Unfog the entire map** after gen (and again at `PostGameStart`) so outside the keep is never a black void.
- Log line to prove new DLL: `Revealed map (PostMapGenerate): unfogged N/Area cells; gate=...`
- Courtyard memory kept in statics so reveal still runs if the ScenPart instance is re-created.
- Gate + 8-cell south approach paved with **PackedDirt** (open 3-cell gate, no door).
- Ideo ensure also runs at `PostGameStart`.

### What we learned
- Joe’s log from ownership build shows `gate (...)` and **no** reveal line — that DLL never unfogged after map fog, so the wilds looked empty with no usable exit.
- Fog applies after `GenerateIntoMap`; padding-only reveal can still feel wrong if fog re-applies — full-map unfog is correct for this playtest scenario.

### Key paths
- `Source/VampireLord/Scenario/ScenPart_VampireLordPlaytestSetup.cs`
- `Assemblies/VampireLord.dll`

### Operator notes
- After merge: pull `C:\McRimworld`, restart RimWorld, **New Game → Vampire Lord**.
- Success = log contains `open gate` + `Revealed map (PostMapGenerate)`, and you can walk south onto visible terrain.

### Next steps
- Joe runtime check; note if Ideology settle page still NREs (separate from outside void).

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
