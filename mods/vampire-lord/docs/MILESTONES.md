# Vampire Lord — Milestones

**Purpose:** Sequence development as small, manually testable vertical slices.  
**Rule:** Complete the active milestone’s acceptance criteria before expanding scope.

Status labels (treat separately):

| Label | Meaning |
|-------|---------|
| IMPLEMENTATION COMPLETE | Code + build done |
| RUNTIME VERIFIED | Proven in a live RimWorld session |

---

## M1 — Wave Director V0

**IMPLEMENTATION COMPLETE:** yes  
**RUNTIME VERIFIED:** yes (2026-10-01 — Joe live session)

### Goal

Prove the gothic keep tower-defence loop inside RimWorld: warning → prepare → escalating raid → recovery → repeat.

### In scope

- Campaign state (`GameComponent` + `ExposeData`)
- Debug-safe activation (not global)
- Wave scheduling (1 day warning lead, 3 days between waves)
- Advance warning letters
- Archetypes Mob / Hunters / Breachers / Fire / Siege via vanilla raid infra
- Threat escalation + raid points formula
- Raid triggering via `IncidentDefOf.RaidEnemy`
- Debug actions
- Save/load of campaign fields

### Non-goals

Custom factions, pawns, weapons, armour, progression, Dark Boons, blood economy, prisoners, bosses, victory, UI, map/castle gen, approach lanes, ideology, quests.

### Acceptance (implementation)

- [x] Campaign can be activated (debug action)
- [x] Campaign state persists (scribe fields present; runtime proof pending)
- [x] Wave scheduled with absolute ticks
- [x] Warning letter path implemented
- [x] Raid fires one warning-lead later (logic present; runtime pending)
- [x] Raid points increase with threat level
- [x] Mob / Breacher / Siege map to distinguishable strategies
- [x] Wave number increments after dispatch
- [x] Next wave schedules after dispatch
- [x] Debug actions present
- [x] Project compiles (`VampireLord.dll`)

### Manual runtime test (Joe / future agent)

1. [x] Enable **Vampire Lord**; start a colony; confirm log `[VampireLord] Initialised successfully.`
2. [x] Dev mode → **Vampire Lord → Start Vampire Lord Campaign**
3. [x] **Show Campaign State** — note `NextWaveTick` / `WarningTick`
4. [x] **Trigger Warning Now** — confirm letter + no duplicate on second call without new schedule
5. [x] **Trigger Wave Now** — confirm raid; threat/wave increment; next wave scheduled  
   Proven state sample: `CampaignActive=True`, `WaveNumber=1`, `ThreatLevel=2`, `LastWaveType=Mob`, `PendingWaveType=Hunters`, `WavePending=True`
6. [ ] Save before warning / after warning / before raid / after raid; reload; confirm schedule integrity *(optional hardening; not required for M1 RUNTIME VERIFIED)*
7. [ ] Advance time ~3 days on an active campaign; confirm warning then raid without debug force *(optional; debug path already proven)*

### Key paths

- `Source/VampireLord/Campaign/`
- `Source/VampireLord/Debug/VampireLordDebugActions.cs`
- `Assemblies/VampireLord.dll`
- `docs/RIMWORLD_API_NOTES.md`

### Known V0 limitations

- Fire archetype does not force incendiary loadouts (vanilla `IncidentParms` has no clean knob).
- Siege/Breaching/Smart strategies resolved by defName; fallback to `ImmediateAttack` if missing.
- Campaign never auto-starts on new games.

---

## M2 — Playtest Keep (identity + courtyard + scenario)

**IMPLEMENTATION COMPLETE:** yes  
**RUNTIME VERIFIED:** yes (2026-10-01 — Joe live session)

### Goal

Start **New Game → Vampire Lord** and already be in the fantasy: keep identity, 1 Vampire Lord + 2 thralls, a simple walled courtyard, Wave Director running — no Dev Mode required for the happy path.

### Player choices locked

- Starting cast: **1 Vampire Lord (Sanguophage) + 2 thralls (Baseliner)**
- Keep footprint: **simple walled courtyard** (not full procedural castle / approach lanes)

### In scope

- Keep identity: scenario name/description, start dialog, warning-letter polish (Black Keep / torchlight fantasy; product name stays Vampire Lord)
- Default `ScenarioDef` with cast, starter gear/materials, standing arrival
- Courtyard placed around player start (stone walls, open south gate, simple paved interior)
- Outside map revealed after fog gen (full-map unfog)
- Wave Director **auto-starts** on this scenario (`PostGameStart`)
- Biotech required for Sanguophage lord (declared in About)
- Debug actions retained for force-testing

### Non-goals

Blood economy, castle progression between waves, custom UI panel, custom factions/weapons, bosses/victory, full approach-lane castle gen, ideology deep dive.

### Acceptance (implementation)

- [x] Scenario appears in New Game as **Vampire Lord**
- [x] Configure-pawns page requests 1 Sanguophage + 2 Baseliner (labels Vampire Lord / Thrall)
- [x] Starter items/materials present in scenario parts
- [x] Courtyard walls + open gate + interior pave spawn at player start
- [x] Campaign activates without Dev Mode on scenario start
- [x] Warning letters still use keep flavour
- [x] Project compiles (`VampireLord.dll`)
- [x] Docs/changelogs updated

### Manual runtime test (Joe)

1. [x] `git pull` → junction already points at `C:\McRimworld\mods\vampire-lord`
2. [x] Enable **Vampire Lord** (+ Biotech); The Ark can stay enabled (stays inactive)
3. [x] New Game → pick **Vampire Lord** scenario → finish setup
4. [x] Confirm: courtyard keep, open south gate, outside map visible/pathable, campaign auto-started  
   Log sample: `open gate`, `Revealed map (PostMapGenerate): unfogged 7807/62500`, `Campaign activated`, `Reassigned 3 humanlike`, Ark `Active=False`
5. [ ] Trigger Warning / Wave (or wait) → keep-flavoured letter + raid *(optional; M1 raid path already RUNTIME VERIFIED)*
6. [ ] Optional: save/load once

### Key paths

- `Defs/Scenarios/`
- `Source/VampireLord/Scenario/`
- `About/About.xml`
- `docs/CHANGELOG.md`

### Known V0 limitations

- Courtyard is a rectangle footprint, not an authored castle map
- Map size/biome still chosen on the world/new-game pages (scenario does not force a custom planet)
- Biotech DLC required for the default scenario cast
- Ideology `Page_ChooseIdeoPreset` can still NRE on settle (game continues; ideo is pre-generated)
- Unrelated noise in Joe's stack: Anomaly `StartedNewGame` NRE, History `Sequence contains no elements`

---

## M3 — Blood Tithe V0 (recommended)

**PLANNING:** yes (2026-10-01)  
**IMPLEMENTATION COMPLETE:** no  
**RUNTIME VERIFIED:** no

### Goal

After M1 (threat) and M2 (stage), give the player the **vampire fantasy between waves**: the keep feeds on blood. Killing the host fills a Keep Blood Reserve; running dry hurts; stocking up is the prep loop.

### Why this M3 (product)

| Slice | What it proves |
|-------|----------------|
| M1 Wave Director | Threats arrive on a schedule |
| M2 Playtest Keep | You start *in* the Black Keep |
| **M3 Blood Tithe** | You are a **Vampire Lord** — blood is the keep’s fuel |

Castle upgrades, approach lanes, and a custom UI panel all matter later; none of them are as unique to this fantasy as blood.

### Player fantasy (one sentence)

Survive the wave → harvest blood → stock the keep → face the next host hungrier or better fed.

### In scope (V0 — small vertical slice)

- Keep Blood Reserve on `VampireLordCampaignGameComponent` (scribed; save/load)
- Gain blood when enemies die during an active Vampire Lord campaign (wave window or always-while-active — decide in impl notes)
- Simple spend/drain rules V0 (pick one primary pressure in impl):
  - **A)** slow drain over time while campaign active, or
  - **B)** each new wave consumes a blood cost (failing cost = harder raid / mood / hemogen packs spawn penalty)
- After-wave letter: blood gained / current reserve / warning if low
- Debug: Show Blood Tithe, Add/Spend Blood, Force Low Blood
- Scenario path still auto-starts campaign (no Dev Mode required for happy path)
- Docs: API notes for Biotech hemogen hooks actually inspected

### Non-goals (explicitly deferred)

- Prisoner blood farm / extraction buildings
- Dark Boons / permanent upgrades purchased with blood
- Custom UI panel (letters + debug + optional log for V0)
- Castle wall upgrades / build points between waves
- Approach-lane castle gen
- Custom factions / weapons / armour
- Full Ideology redesign (separate optional hotfix for ChooseIdeoPreset NRE)
- Bosses / victory condition

### Acceptance (implementation — not started)

- [ ] Blood Reserve field persists (ExposeData)
- [ ] Enemy deaths under active campaign credit the reserve (logged)
- [ ] At least one spend/drain pressure rule live
- [ ] After-wave (or threshold) letter communicates blood state
- [ ] Debug actions for inspect / add / spend
- [ ] Compiles; changelogs updated
- [ ] Manual runtime checklist written for Joe

### Manual runtime test (draft — fill when implementing)

1. New Game → Vampire Lord → confirm campaign active
2. Force or wait a wave; kill enemies → reserve increases (log/letter)
3. Save/load → reserve unchanged
4. Drive reserve low → pressure rule fires (letter or raid modifier)
5. Debug Add Blood → pressure clears / letter reflects stock

### Key paths (expected)

- `Source/VampireLord/Campaign/` (state + tithe logic)
- `Source/VampireLord/Debug/`
- `docs/RIMWORLD_API_NOTES.md` (hemogen / corpse / pawn-kill hooks)
- `Assemblies/VampireLord.dll`

### Risks / open decisions (resolve at impl start)

1. **Gain trigger:** `Pawn.Kill` Harmony vs incident/wave bookkeeping vs corpse loot — prefer least invasive; record in API notes.
2. **Pressure rule:** time drain vs per-wave cost (recommend **B per-wave cost** for clearer tower-defence prep).
3. **Vanilla hemogen:** V0 may track a **keep meter** separate from pawn hemogen, and optionally drop/consume `HemogenPack` as flavour — do not rewrite Biotech gene need unless needed.

### Recommended default for open decisions

- Gain: credit on humanlike (or any) enemy death while `CampaignActive`
- Pressure: **wave blood cost** before raid fires (insufficient blood → higher raid points or warning + thrall mood hit)
- Presentation: letters only (no custom window)

---

## M4+ — FUTURE (not planned yet)

Each needs its own milestone card before coding.

| Candidate | Fantasy |
|-----------|---------|
| Keep Fortification V0 | Between waves, spend blood/materials to reinforce walls / add sandbags |
| Approach Lanes V0 | Raids prefer a cleared south road into the gate |
| Campaign UI panel | Always-on Blood / Wave / Threat readout |
| Prisoners & Tithe jobs | Capture → extract → stock the keep |
| Dark Boons | Permanent powers bought with blood |
| Ideology polish | Skip/fix ChooseIdeoPreset NRE on settle |

---

## Planning note

**No M3 code until Joe confirms this card** (or names a different M3 from the M4+ table). Planning ships as docs only.
