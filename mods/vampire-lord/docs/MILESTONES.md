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

## M2+ — FUTURE

Not started. Candidates (order TBD): keep identity content, blood economy, castle progression, custom UI, map/castle generation. Each needs its own milestone card before coding.
