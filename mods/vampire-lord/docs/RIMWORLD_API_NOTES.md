# Vampire Lord — RimWorld API Notes

**Purpose:** Verified findings about RimWorld / Verse APIs used by this mod.  
**Rule:** Only record APIs inspected from referenced assemblies (or other reliable evidence).  
**Ban:** Speculative “this probably exists” entries presented as fact.

Target: RimWorld **1.6** via `Krafs.Rimworld.Ref` 1.6.4871 (Cloud/CI) or local Steam `Assembly-CSharp.dll` (Joe’s PC).

---

## How to add an entry

```markdown
### YYYY-MM-DD — Short title

- **Goal:** What we needed to know
- **Evidence:** Assembly / inspection method
- **API:** Namespace, type, member signatures actually observed
- **Behaviour notes:** Lifecycle caveats
- **Decision for Vampire Lord:** How we use it (or why not)
- **Related milestone:** e.g. M1
```

---

## Environment

| Item | Value |
|------|--------|
| RimWorld target | 1.6 |
| Mod packageId | `joemcdonagh.vampirelord` |
| Ref package (Cloud) | `Krafs.Rimworld.Ref` 1.6.4871 |
| Local hint path | Steam Managed `Assembly-CSharp.dll` (same pattern as The Ark) |
| Startup hook | `Verse.StaticConstructorOnStartup` → `[VampireLord] Initialised successfully.` |
| Persistence owner | `VampireLord.Campaign.VampireLordCampaignGameComponent` (`Verse.GameComponent`) |

---

## Verified API entries

### 2026-10-01 — GameComponent persistence (aligned with The Ark M1)

- **Goal:** Authoritative campaign state that survives save/load without Harmony.
- **Evidence:** Metadata inspection of `Assembly-CSharp` 1.6 ref; Ark’s verified `FillComponents` notes.
- **API:**
  - `Verse.GameComponent` with ctor `(Game game)`
  - `ExposeData`, `GameComponentTick`, `StartedNewGame`, `LoadedGame`
  - `Current.Game.GetComponent<T>()`
  - `Verse.Scribe_Values.Look`
- **Decision for Vampire Lord:** Sole authoritative owner for Wave Director fields. No static mutable campaign state.
- **Related milestone:** M1

### 2026-10-01 — Timing / ticks

- **Goal:** Absolute-tick scheduling for warning + wave cadence.
- **Evidence:** Metadata inspection.
- **API:**
  - `Verse.Find.TickManager` / `TickManager.TicksGame`
  - `RimWorld.GenDate.TicksPerDay`, `GenDate.DaysPassed`
  - `Verse.GenTicks.TicksGame` (also present; campaign uses `Find.TickManager.TicksGame`)
- **Decision for Vampire Lord:** `NextWaveTick` / `WarningTick` store absolute game ticks. Evaluate every 250 ticks in `GameComponentTick`.
- **Related milestone:** M1

### 2026-10-01 — Letters

- **Goal:** One advance-warning letter per pending wave.
- **Evidence:** Metadata inspection.
- **API:**
  - `RimWorld.LetterDefOf.ThreatBig`
  - `Verse.Find.LetterStack.ReceiveLetter` (string/TaggedString overloads present)
  - `Verse.LetterMaker.MakeLetter` also present (unused in V0; `ReceiveLetter` path is enough)
- **Decision for Vampire Lord:** `VampireLordLetters.SendAdvanceWarning` uses `ReceiveLetter(title, body, LetterDefOf.ThreatBig)`. No second custom arrival letter — vanilla raid letters stand.
- **Runtime status:** Letter appearance **UNVERIFIED** in RimWorld until manual test.
- **Related milestone:** M1

### 2026-10-01 — Map selection

- **Goal:** Fire raids at the player’s keep map.
- **Evidence:** Metadata inspection.
- **API:**
  - `Verse.Map.get_IsPlayerHome`
  - `Verse.Find.CurrentMap`
  - `Verse.Find.AnyPlayerHomeMap`
- **Decision for Vampire Lord:** Prefer current map if `IsPlayerHome`, else `AnyPlayerHomeMap`. If none → log warning, delay retry (`RaidRetryDelayTicks`), do not crash.
- **Related milestone:** M1

### 2026-10-01 — RaidEnemy incident + IncidentParms

- **Goal:** Launch waves through vanilla raid infrastructure (no manual pawn spawn).
- **Evidence:** Metadata inspection of `RimWorld.IncidentParms`, `IncidentDefOf`, `IncidentWorker.TryExecute`.
- **API:**
  - `RimWorld.IncidentDefOf.RaidEnemy`
  - `IncidentWorker.TryExecute(IncidentParms)`
  - `IncidentParms` fields used: `target`, `points`, `faction`, `forced`, `raidStrategy`, `bypassStorytellerSettings`, `generateFightersOnly`
- **Decision for Vampire Lord:** `VampireLordRaidLauncher` builds parms and calls `RaidEnemy.Worker.TryExecute`.
- **Runtime status:** Successful raid spawn / faction pick **UNVERIFIED** until in-game test.
- **Related milestone:** M1

### 2026-10-01 — Raid strategies (archetypes)

- **Goal:** Distinguish Mob / Hunters / Breachers / Fire / Siege via vanilla strategies.
- **Evidence:** Metadata inspection.
- **API:**
  - `RimWorld.RaidStrategyDefOf.ImmediateAttack` (DefOf field verified)
  - Worker types present: `RaidStrategyWorker_ImmediateAttackBreaching`, `…BreachingSmart`, `…Sappers`, `…Smart`, `RaidStrategyWorker_Siege`
  - Lookup: `Verse.DefDatabase<RaidStrategyDef>.GetNamedSilentFail(string)`
- **Decision for Vampire Lord (V0 mapping):**

  | Archetype | Strategy defName preference | Notes |
  |-----------|-----------------------------|-------|
  | Mob | `ImmediateAttack` | Numbers via raid points |
  | Hunters | `ImmediateAttackSmart` → fallback ImmediateAttack | `generateFightersOnly = true` |
  | Breachers | `ImmediateAttackBreaching` → Sappers → ImmediateAttack | Breaching worker type verified |
  | Fire | `ImmediateAttack` | **Limitation:** no IncidentParms field forces Molotov/incendiary kits |
  | Siege | `Siege` → fallback ImmediateAttack | `RaidStrategyWorker_Siege` verified; defName assumed `Siege` (standard vanilla) |

- **Assumption:** Vanilla defNames `ImmediateAttackBreaching`, `ImmediateAttackSmart`, `Siege` match worker types. Confirmed via naming convention + worker type presence; **RUNTIME VERIFY** that `GetNamedSilentFail` resolves them on a live 1.6 install.
- **Related milestone:** M1

### 2026-10-01 — Hostile faction selection

- **Goal:** Avoid hardcoding a faction; stay mod-compatible.
- **Evidence:** Metadata inspection.
- **API:** `RimWorld.FactionManager.RandomRaidableEnemyFaction(bool allowHidden, bool allowDefeated, bool allowNonHumanlike, TechLevel minTechLevel)`
- **Decision for Vampire Lord:** Call with `(false, false, true, TechLevel.Undefined)`.
- **Related milestone:** M1

### 2026-10-01 — Debug actions

- **Goal:** Debug-safe campaign activation (not global auto-start).
- **Evidence:** Metadata inspection.
- **API:**
  - `LudeonTK.DebugActionAttribute` (fields: `category`, `name`, `allowedGameStates`, …)
  - `LudeonTK.AllowedGameStates.PlayingOnMap`
- **Decision for Vampire Lord:** Static methods on `VampireLordDebugActions` with named attribute args. Category **"Vampire Lord"**.
- **Runtime status:** Menu visibility **UNVERIFIED** until Dev mode test.
- **Related milestone:** M1

---

### 2026-10-01 — Scenario + ScenPart playtest setup (M2)

- **Goal:** New Game scenario with xenotype cast, courtyard footprint, campaign auto-start — scenario-scoped only.
- **Evidence:** MetadataLoadContext on `Krafs.Rimworld.Ref` 1.6.4871.
- **API:**
  - `RimWorld.ScenarioDef` / `RimWorld.Scenario` parts list
  - `RimWorld.ScenPart` hooks: `GenerateIntoMap(Map)`, `PostGameStart()`
  - `RimWorld.ScenPart_ConfigPage_ConfigureStartingPawns_Xenotypes` + `RimWorld.XenotypeCount` (`xenotype`, `count`, `description`, `requiredAtStart`, `allowedDevelopmentalStages`)
  - `RimWorld.XenotypeDefOf.Sanguophage`, `Baseliner`
  - `Verse.MapGenerator.PlayerStartSpot` / `PlayerStartSpotValid`
  - `Verse.CellRect.CenteredOn`, `ContractedBy`, `ClipInsideMap`, `GetCenterCellOnEdge(Rot4)`
  - `Verse.ThingMaker.MakeThing` + `Verse.GenSpawn.Spawn` for `ThingDefOf.Wall` / `Door` with `ThingDefOf.BlocksGranite`
  - `RimWorld.TerrainDefOf.FlagstoneSandstone`
  - `RimWorld.ScenPart_GameStartDialog`, `ScenPart_StartingThing_Defined`, `ScenPart_StartingResearch`, `ScenPart_PlayerPawnsArriveMethod`
- **Decision for Vampire Lord:** Custom `ScenPart_VampireLordPlaytestSetup` embedded in scenario XML (no global map-gen patch). Courtyard in `GenerateIntoMap`; `ActivateCampaign` in `PostGameStart`.
- **Related milestone:** M2

---

## Pending / runtime verification

| Topic | Status |
|-------|--------|
| M1 save/load round-trip of VL scribe fields | PENDING (optional) |
| M1 natural 3-day warning→raid without debug | PENDING (optional) |
| M1 RaidEnemy strategy defNames in live session | PARTIAL (Mob wave proven) |
| Fire archetype — no incendiary force | Documented limitation |
| M2 New Game scenario appears + xenotype page | PENDING |
| M2 courtyard + gate at player start | PENDING |
| M2 campaign auto-start without Dev Mode | PENDING |
| M2 start dialog + keep-flavoured warning letter | PENDING |

---

## Explicit non-goals for this file

- Design intent (see MILESTONES / store plan)
- Copied decompiled proprietary source dumps
