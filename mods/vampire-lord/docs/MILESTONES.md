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
**RUNTIME VERIFIED:** no

### Goal

Start **New Game → Vampire Lord** and already be in the fantasy: keep identity, 1 Vampire Lord + 2 thralls, a simple walled courtyard, Wave Director running — no Dev Mode required for the happy path.

### Player choices locked

- Starting cast: **1 Vampire Lord (Sanguophage) + 2 thralls (Baseliner)**
- Keep footprint: **simple walled courtyard** (not full procedural castle / approach lanes)

### In scope

- Keep identity: scenario name/description, start dialog, warning-letter polish (Black Keep / torchlight fantasy; product name stays Vampire Lord)
- Default `ScenarioDef` with cast, starter gear/materials, standing arrival
- Courtyard placed around player start (stone walls, one gate/door, simple paved interior)
- Wave Director **auto-starts** on this scenario (`PostGameStart`)
- Biotech required for Sanguophage lord (declared in About)
- Debug actions retained for force-testing

### Non-goals

Blood economy, castle progression between waves, custom UI panel, custom factions/weapons, bosses/victory, full approach-lane castle gen, ideology deep dive.

### Acceptance (implementation)

- [x] Scenario appears in New Game as **Vampire Lord**
- [x] Configure-pawns page requests 1 Sanguophage + 2 Baseliner (labels Vampire Lord / Thrall)
- [x] Starter items/materials present in scenario parts
- [x] Courtyard walls + door + interior pave spawn at player start
- [x] Campaign activates without Dev Mode on scenario start
- [x] Warning letters still use keep flavour
- [x] Project compiles (`VampireLord.dll`)
- [x] Docs/changelogs updated

### Manual runtime test (Joe)

1. `git pull` → junction already points at `C:\McRimworld\mods\vampire-lord`
2. Enable **Vampire Lord** (+ Biotech)
3. New Game → pick **Vampire Lord** scenario → finish setup (temperate recommended)
4. Confirm: courtyard keep, 1 lord + 2 thralls, start dialog, log campaign activated
5. Trigger Warning / Wave (or wait) → keep-flavoured letter + raid
6. Optional: save/load once

### Key paths

- `Defs/Scenarios/`
- `Source/VampireLord/Scenario/`
- `About/About.xml`
- `docs/CHANGELOG.md`

### Known V0 limitations

- Courtyard is a rectangle footprint, not a authored castle map
- Map size/biome still chosen on the world/new-game pages (scenario does not force a custom planet)
- Biotech DLC required for the default scenario cast

---

## M3+ — FUTURE

Candidates (order TBD): blood economy, castle progression, custom UI, richer map/castle generation. Each needs its own milestone card before coding.
