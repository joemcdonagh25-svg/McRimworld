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

## M3 — Blood Tithe V0

**PLANNING:** yes (Joe confirmed Blood Tithe, 2026-10-01)  
**IMPLEMENTATION COMPLETE:** yes  
**RUNTIME VERIFIED:** no

### Goal

After M1 (threat) and M2 (stage), give the player the **vampire fantasy between waves**: the keep feeds on blood. Killing the host fills a Keep Blood Reserve; running dry hurts; stocking up is the prep loop.

### Player fantasy (one sentence)

Survive the wave → harvest blood → stock the keep → face the next host hungrier or better fed.

### Decisions locked in V0

- Gain: fresh hostile **humanlike** corpses while campaign active (+5 each; corpse age under ~1 day)
- Pressure: **per-wave blood cost** before raid (`10 + 2*threat`); unpaid → raid points ×1.35
- Presentation: letters + debug (no custom UI); keep meter separate from Biotech pawn hemogen
- No Harmony: corpse scan on campaign tick

### In scope (shipped)

- Keep Blood Reserve on `VampireLordCampaignGameComponent` (scribed)
- `VampireLordBloodTithe` credit / spend / wave cost / harvest
- Warning letters include reserve + tithe due
- Tithe paid / starved letters at wave launch; harvest letter when next wave starts if kills accrued
- Debug: Show Blood Tithe, Add/Spend (+/−20), Force Low Blood (0)
- Failed raid launch refunds spent tithe

### Non-goals (deferred)

- Prisoner blood farm / extraction buildings
- Dark Boons / permanent upgrades purchased with blood
- Custom UI panel
- Castle wall upgrades / approach-lane castle gen
- Custom factions / weapons / armour
- Ideology ChooseIdeoPreset polish
- Bosses / victory condition

### Acceptance (implementation)

- [x] Blood Reserve field persists (ExposeData)
- [x] Enemy deaths under active campaign credit the reserve (logged)
- [x] Wave blood cost pressure live (starved raid multiplier)
- [x] Letters communicate blood state (warning / tithe / harvest)
- [x] Debug actions for inspect / add / spend / force low
- [x] Compiles; changelogs updated
- [x] Manual runtime checklist written for Joe

### Manual runtime test (Joe)

1. Pull + restart → New Game → Vampire Lord → campaign active; log `Blood Reserve=40`
2. Dev Mode → **Show Blood Tithe** / **Show Campaign State** — note reserve + next cost
3. **Trigger Wave Now** — tithe letter; reserve drops; if **Force Low Blood** first, starved letter + harder raid
4. Kill hostile humanlikes → log `Blood Tithe +5`; next wave start may show harvest letter
5. Save/load → reserve unchanged; **Show Blood Tithe** again

### Key paths

- `Source/VampireLord/Campaign/VampireLordBloodTithe.cs`
- `Source/VampireLord/Campaign/VampireLordCampaignGameComponent.cs`
- `Source/VampireLord/Campaign/VampireLordWaveDirector.cs`
- `Source/VampireLord/Campaign/VampireLordLetters.cs`
- `Source/VampireLord/Debug/VampireLordDebugActions.cs`
- `Assemblies/VampireLord.dll`

---

## M4 — Keep Fortification V0

**PLANNING:** yes (Joe confirmed Keep Fortification, 2026-10-01)  
**IMPLEMENTATION COMPLETE:** yes  
**RUNTIME VERIFIED:** no

### Goal

Spend Keep Blood Reserve between waves to reinforce the south gate with sandbags — blood sink that changes the keep before the next host.

### Player fantasy (one sentence)

Harvest blood from the host → spend it on the walls before the next warning → hold the gate.

### Decisions locked in V0

- Currency: Keep Blood Reserve (15 per package)
- When: prep window only (`WavePending && !WarningIssued`); blocked after advance warning
- What: player-owned sandbag flanks south of the open gate (center path left open)
- How: Accept letter + Dev Mode (**Offer Fortify**, **Force Fortify Now**, **Show Fortify State**)
- Cap: 1 package per prep window
- No Harmony

### In scope (shipped)

- `VampireLordFortify` purchase / placement / prep-window gate
- `VampireLordKeepLayout` gate memory from playtest courtyard
- Choice letter `VampireLord_FortifyOffer`
- Offer on campaign activate + after each wave schedules the next
- Scribed fortify purchase / last-place fields
- Debug actions

### Non-goals (deferred)

- Full castle rebuild / wall upgrades / turrets
- Approach-lane raid pathing
- Dark Boons / prisoner extraction
- Custom architect tab / always-on UI panel

### Acceptance (implementation)

- [x] Blood can be spent between waves on a keep fortification
- [x] Fortification appears at/near the south gate as player-owned cover
- [x] Cap / window rule (1× per prep window; blocked after warning)
- [x] Letter + debug make the offer obvious
- [x] Compiles; changelogs updated
- [x] Manual runtime checklist for Joe

### Manual runtime test (Joe)

Prefer the fixture loop in `docs/PLAYTEST_FIXTURE.md` after the first New Game.

1. Pull + restart → New Game → Vampire Lord → **Ensure Prep Fixture** → save `VL_prep`
2. Letter path: wait for **Blood for the Walls** (letter stack) **or** turn **Auto-Fortify Playtest** ON
3. Accept / auto → reserve −15; sandbags south of gate; log `Fortified gate`
4. **Show Fortify State** — purchases 1/1; second buy blocked until next prep window
5. **Trigger Warning Now** → fortify blocked; **Force Fortify Now** still works for debug
6. Later iterations: load `VL_prep` instead of New Game

### Key paths

- `Source/VampireLord/Campaign/VampireLordFortify.cs`
- `Source/VampireLord/Campaign/ChoiceLetter_VampireLordFortify.cs`
- `Source/VampireLord/Scenario/VampireLordKeepLayout.cs`
- `Defs/Letters/VampireLordLetters.xml`
- `Assemblies/VampireLord.dll`

---

## M4+ — still parked

| Candidate | Fantasy |
|-----------|---------|
| Approach Lanes V0 | Raids prefer a cleared south road into the gate |
| Campaign UI panel | Always-on Blood / Wave / Threat readout |
| Prisoners & Tithe jobs | Capture → extract → stock the keep |
| Dark Boons | Permanent powers bought with blood |

---

## Planning note

M3 Blood Tithe and M4 Fortification **RUNTIME VERIFIED** await Joe’s checklists. No M5+ until Joe picks a parked card.
