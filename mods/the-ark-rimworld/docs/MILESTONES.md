# The Ark — Milestones

**Purpose:** Sequence development as small, manually testable vertical slices.  
**Rule:** Complete the active milestone's acceptance criteria before expanding scope.

Design intent lives in `GAME_DESIGN.md`. Architecture constraints live in `ARCHITECTURE.md`. Process rules live in `DEVELOPMENT_RULES.md`.

---

## Status legend

| Status | Meaning |
|--------|---------|
| COMPLETE | Verified and must remain working |
| IMPLEMENTATION COMPLETE | Code + build done; runtime verification may still be pending |
| NEXT | Immediate implementation target when coding resumes |
| PLANNED | Specified; not started |
| FUTURE | Directional; details may change |

---

## M0 — Working Mod Foundation

**Status:** COMPLETE

### Goal

Ship a recognised RimWorld 1.6 mod that loads and initialises.

### Non-goals

- Campaign state
- Pursuit, expeditions, progression, UI systems

### Acceptance criteria

- Project targets `net472` and compiles with zero errors
- `Assemblies/TheArk.dll` is produced
- Mod appears in RimWorld as **The Ark** and can be enabled
- Log contains `[The Ark] Initialised successfully.`

### Manual test

1. Build the project.
2. Launch RimWorld with The Ark enabled.
3. Confirm the success log line.

### Likely technical risks

- Wrong framework / reference path
- DLL not visible to RimWorld Mods folder (junction / copy issues)

---

## M1 — Persistent Campaign Foundation

**Status:** COMPLETE — **RUNTIME VERIFIED** (Joe, 2026-10-01; fixture save/quit/load matched Day=47, Landing=6, Tier=2, Pursuit=73)

### Goal

Establish the smallest authoritative persistent Ark campaign state.

### Initial data (only)

- campaign active
- campaign day
- landing number
- Ark tier
- Pursuit value

### Non-goals

- Gameplay effects from any of these fields
- Expeditions, UI beyond what is required to prove persistence (prefer M2 for UI)
- Full conceptual `ArkState` schema from design docs

### Acceptance criteria

- One clear authoritative persistence owner chosen and documented in `RIMWORLD_API_NOTES.md`
- Fields save and load correctly across a RimWorld save / load cycle
- Bootstrap and M0 behaviour remain intact
- No gameplay consequences yet

### Implementation notes

- Owner: `TheArk.Campaign.ArkCampaignGameComponent` (`Verse.GameComponent`)
- Access: `TheArk.Campaign.ArkCampaign.TryGet` / `Get`
- Scribe labels: `arkCampaignActive`, `arkCampaignDay`, `arkLandingNumber`, `arkTier`, `arkPursuit`
- Build: succeeds with 0 errors (IMPLEMENTATION COMPLETE)
- In-game New Game start: confirmed (scenario list, ideo harden, 3 colonists reassigned, campaign Active=True)
- In-game save/load proof: **RUNTIME VERIFIED** — fixture Applied then LoadedGame matched Active=True, Day=47, Landing=6, Tier=2, Pursuit=73

### Manual test

1. Enable mod; start a new game; confirm log `[The Ark] Campaign state (StartedNewGame): ...` with defaults.
2. Set non-default values for a persistence proof (see report: debugger or M2 — no throwaway mutators shipped in M1).
3. Save, quit RimWorld fully, relaunch, load the save.
4. Confirm log `[The Ark] Campaign state (LoadedGame): ...` matches the values from step 2 (or defaults if step 2 was skipped).

### Likely technical risks

- Choosing the wrong component scope (game vs world vs map) — mitigated by investigation; runtime still pending
- Save / load edge cases on new game vs load
- Accidental coupling to map-local objects

---

## M2 — Campaign Debug UI

**Status:** COMPLETE — **RUNTIME VERIFIED** (Joe, 2026-10-01; New Game start + M1 fixture save/quit/load)

### Goal

Expose M1 state through a minimal development / debug interface.

### Non-goals

- Polished player-facing UX
- New campaign systems

### Acceptance criteria

- Developer can view M1 fields in-game
- Developer can adjust key fields safely for testing (at least Pursuit and tier, or document why not)
- UI reads state and uses explicit commands; no authoritative logic living only in UI widgets

### Implementation notes

- Dev Mode menu category: **The Ark (DEV)**
- Actions: Open Campaign Debug · Log Campaign State · Apply M1 Persistence Fixture
- Window: `[DEV] The Ark — Campaign Debug` — draft fields + Apply / Refresh / Fixture
- Writes via `TheArk.Debug.ArkCampaignDebugOps` into `ArkCampaignGameComponent` only
- Gate: Dev Mode debug actions + `Prefs.DevMode` / `onlyDrawInDevMode`

### Manual test

1. Enable The Ark + Biotech; **New Game → The Ark** (or any map start with the mod on).
2. Confirm start dialog + log show campaign active (`Scenario.PostGameStart` / `StartedNewGame`).
3. If Ideology is on: settle may still log `Page_ChooseIdeoPreset` NRE — confirm crew are **player colonists** and the map is playable.
4. Turn **Dev Mode** on → Debug Actions → **The Ark (DEV)** → **Apply M1 Persistence Fixture** (or Open Campaign Debug → Apply).
5. Confirm log / window: Active=true, Day=47, Landing=6, Tier=2, Pursuit=73.
6. Save → quit RimWorld completely → relaunch → load the save.
7. Confirm `[The Ark] Campaign state (LoadedGame): ...` matches exactly; re-open debug UI and confirm.
8. On success: mark M1 and M2 **RUNTIME VERIFIED**.

### Likely technical risks

- RimWorld UI lifecycle and windowing pitfalls
- Debug tools left enabled for players without a clear gate (document intent)
- Ideology `ChooseIdeoPreset` NRE on settle (known; hardened via ideo generate + colonist reassignment)

---

## M3 — Landing Detection

**Status:** COMPLETE — **RUNTIME VERIFIED** (Joe, 2026-10-01; Dev Mode Simulate Landing → LandingNumber=1, Session=True)

### Goal

Reliably determine when the Ark has landed and establish a landing session.

### Non-goals

- Pursuit growth
- Full expedition content
- Landing timer (M4)
- Departure teardown beyond End Session debug (M8)

### Acceptance criteria

- Verified API path for gravship / landing detection documented in `RIMWORLD_API_NOTES.md`
- Landing session begins exactly once per confirmed landing (no spam / double-fire under normal use)
- Landing number (or equivalent) updates according to design for this milestone

### Implementation notes

- Authoritative owner remains `ArkCampaignGameComponent`
- Session fields (scribed): `LandingSessionActive`, `LandingSessionMapId`, `lastCountedLandingMapId`
- On confirm: session on + `LandingNumber++` (ignored if session already active)
- Detection (no Harmony): poll Odyssey `IsGravshipTravelling` edge + `Map.wasSpawnedViaGravShipLanding`; also `ScenPart.PostGravshipLanded`
- Dev Mode: **Simulate Landing** / **End Landing Session** (surface playtest proof without a real hop)

### Manual test

1. New Game → **The Ark** (campaign active).
2. Dev Mode → **The Ark (DEV)** → **Simulate Landing**.
3. Log: `Landing session STARTED` + LandingNumber up + `LandingSession=True`.
4. Simulate Landing again → ignored (session already active).
5. Optional: End Landing Session → Simulate again → LandingNumber up again.
6. Save / load mid-session → session + LandingNumber persist.
7. On success: mark M3 **RUNTIME VERIFIED**. *(Done via Simulate Landing; real Odyssey hop still optional later.)*

### Likely technical risks

- Odyssey API discovery; false positives from caravan / map transitions
- Multiplayer or multiple-map edge cases (note; may be out of scope)
- Real gravship hop still UNVERIFIED until Joe lands with Odyssey (Simulate Landing proof accepted for V0)

---

## M4 — Landing Timer

**Status:** IMPLEMENTATION COMPLETE — **RUNTIME VERIFIED** pending Pressure V0 pack test

### Goal

Track elapsed time since landing.

### Non-goals

- Pursuit consequences
- Expedition objectives

### Acceptance criteria

- Timer runs only while a landing session is active
- Timer value is visible via debug UI (or equivalent verified readout)
- Timer does not corrupt durable campaign fields

### Implementation notes

- Session field `landingSessionTicks` (scribe `arkLandingSessionTicks`) on `ArkCampaignGameComponent`
- Increments every `GameComponentTick` while `LandingSessionActive`
- Resets to 0 on session start and session end — does **not** write `CampaignDay` / other durable fields
- Persistence: accumulated ticks survive save/load mid-session
- Dev: **Advance Landing Timer +1 Day**; debug UI shows ticks + days

### Manual test

1. Simulate Landing; note TimerTicks=0 / ~0.00d.
2. Unpause briefly **or** Dev → **Advance Landing Timer +1 Day**; confirm timer increases.
3. Save / load while session active; confirm TimerTicks matches (or continues from saved value).
4. End Landing Session; timer clears to 0.
5. On success: mark M4 **RUNTIME VERIFIED**.

### Likely technical risks

- Tick vs day-scale time confusion
- Pausing / time controls interaction

---

## M5 — Pursuit V0

**Status:** IMPLEMENTATION COMPLETE — **RUNTIME VERIFIED** pending Pressure V0 pack test

### Goal

Increase Pursuit based on time since landing.

### Non-goals

- Wealth / crew / tier modifiers
- Incidents and Harbinger special behaviour

### Acceptance criteria

- Pursuit rises with landed time according to a simple, documented curve or rate
- Pursuit clamps or saturates within 0–100 as designed
- No additional drivers required for V0

### Implementation notes

- Rate: **+1 Pursuit per whole landed day** (`GenDate.TicksPerDay`) while landing session active
- Clamp 0–100 via `ArkPursuit`
- Durable `arkPursuit` persists across End Session; growth stops when not landed
- Session field `arkLandingPursuitDaysApplied` prevents double-grant after save/load

### Manual test

Dev Mode → **Run Pressure V0 Proof** → paste `Pressure V0 proof PASS`.

### Likely technical risks

- Rate tuning making pressure invisible or oppressive
- Timer and Pursuit double-counting bugs

---

## M6 — Pursuit Feedback

**Status:** IMPLEMENTATION COMPLETE — **RUNTIME VERIFIED** pending Pressure V0 pack test

### Goal

Provide clear player-facing Pursuit state (QUIET → HARBINGER bands).

### Non-goals

- Full Harbinger encounter design
- Complex multi-factor Pursuit inputs

### Acceptance criteria

- Player can tell which Pursuit band they are in without reading debug-only UI
- Band thresholds match design (0–20 Quiet, etc.)
- Feedback updates when Pursuit crosses thresholds

### Implementation notes

- Bands: QUIET 0–20, NOTICED 21–40, HUNTED 41–60, BESIEGED 61–80, HARBINGER 81–100
- Letter on band change (`ArkPursuitLetters`); baseline synced on new/load (no re-spam)
- Dev: **Jump Pursuit To Next Band**

### Manual test

Dev Mode → **Run Pressure V0 Proof** → paste `Pressure V0 proof PASS`.

### Likely technical risks

- Alert spam vs clarity
- Localisation / message noise

---

## M7 — Pursuit Incident

**Status:** PLANNED

### Goal

Cause **one** verified gameplay consequence from Pursuit.

### Non-goals

- Full raid escalation ladder
- Harbinger final form
- Farming-friendly infinite raid loops as the intended endgame

### Acceptance criteria

- At a defined Pursuit threshold or state, one real gameplay consequence fires
- Consequence is reproducible in manual test
- Evacuation remains the intended strategic response direction (document how this incident supports that)

### Manual test

1. Drive Pursuit to the trigger condition.
2. Observe the single consequence.
3. Confirm it does not require unrelated systems beyond this milestone.

### Likely technical risks

- Incident framework misuse
- Difficulty / raid-point coupling fighting the evacuation fantasy

---

## M8 — Departure Lifecycle

**Status:** PLANNED

### Goal

Correctly end / reset landing-specific state when the gravship departs, while preserving campaign state.

### Non-goals

- Star map destination selection UX polish
- Module upgrades

### Acceptance criteria

- On verified departure: landing session ends; landing timer resets or closes; session-only data cleared
- Durable campaign fields (tier, landing number, Pursuit policy as designed) persist correctly
- Documented behaviour for whether Pursuit decays, freezes, or resets on departure (choose and stick to one for this milestone)

### Manual test

1. Land; accumulate timer / Pursuit.
2. Depart.
3. Confirm session reset and campaign preservation via debug UI / logs.
4. Save / load after departure; confirm consistency.

### Likely technical risks

- Missing departure signal from Odyssey
- Accidentally wiping campaign state with session state

---

## M9 — Expedition V0

**Status:** PLANNED

### Goal

One simple objective / reward loop on a landing.

### Non-goals

- Full optional-objective framework
- Procedural expedition generation at scale

### Acceptance criteria

- A landed session can present one primary objective
- Completing it grants one clear reward
- Objective ties into campaign state without becoming the persistence owner

### Manual test

1. Land into the V0 expedition setup.
2. Complete the objective.
3. Confirm reward and state update.

### Likely technical risks

- Quest / incident / custom map object choice
- Objective state surviving departure incorrectly

---

## M10 — Progression V0

**Status:** PLANNED

### Goal

Completing expeditions contributes to Ark progression.

### Non-goals

- Full module tree
- Tier 4 content complete

### Acceptance criteria

- Expedition completion can advance a documented progression signal (e.g. progress toward next tier or unlock flag)
- Progression is persisted via the campaign owner
- Outside world relevance preserved (do not grant total self-sufficiency)

### Manual test

1. Complete V0 expedition under controlled conditions.
2. Confirm progression field / tier signal changes.
3. Save / load; confirm persistence.

### Likely technical risks

- Progression too fast / too opaque
- Coupling progression to disposable map objects

---

## M11+ — Future slices

**Status:** FUTURE

Directional follow-ons (order may adjust):

- Ship modules (Explorer-era choices; mutual exclusion / capacity limits)
- Richer expeditions (optional objectives, biome context packs)
- Star Map / destination selection
- Harbinger as a distinct pressure fantasy (not only raid points)
- Campaign victory / ending structure
- Content pass (crew roles framing, salvage tables, narrative tone without copying protected IP)

Each future slice must be broken into its own milestone with goal, non-goals, acceptance criteria, manual test, and risks before implementation.

---

## Next implementation target

**M1–M3** COMPLETE / RUNTIME VERIFIED.  
**Pressure V0 pack (M4+M5+M6)** IMPLEMENTATION COMPLETE — one RimWorld boot checklist pending Joe.

Do **not** begin M7 (Pursuit Incident) until Joe asks / approves after Pressure V0 runtime proof.
