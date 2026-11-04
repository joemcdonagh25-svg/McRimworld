# Feature: Mist / Darkness Director (Horror Director gameplay)

Status: IMPLEMENTATION COMPLETE — COMPILE VERIFIED — RUNTIME VERIFICATION PENDING

## Objective

Turn Lantern Village Horror Director from phase-logging scaffold into **actual gameplay pressure**: forest mist and unnatural-feeling darkness pulses that escalate across the five campaign phases — without Anomaly DLC.

## Existing State

- `HorrorDirector` GameComponent already tracks phases by days since settle.
- `EnableSupernatural` defaulted false; no incidents fired.
- Profile includes Realistic Darkness; excludes Anomaly (no `UnnaturalDarkness`).
- Vampire Lord pattern: GameComponent + absolute-tick schedule + letters.

## Architecture

Extend `HorrorDirector` only (one authoritative GameComponent):

- Phase entry letters
- Absolute-tick pulse schedule (`nextPulseTick`)
- Mist: custom `GameCondition_ForceWeather` → Fog / FoggyRain
- Darkness: vanilla `GameConditionDefOf.Eclipse` (short windows)
- Late phase: denser cadence; still no Anomaly defs
- Persist schedule + enabled flag via Scribe
- Scenario-gated via `ScenarioGate`

## Phase feel

| Phase | Days | Mist / Dark |
|-------|------|-------------|
| Foundation | 1–30 | Rare light Fog; soft letter |
| Folklore threats | 31–60 | Fog pulses; ThreatSmall letters |
| The strange forest | 61–90 | FoggyRain + occasional short Eclipse |
| The darkness awakens | 91–120 | Longer Eclipse + mist stacking |
| The last light | 121–180 | Dense cadence; Eclipse + thick mist |

## Out of Scope

- Anomaly Unnatural Darkness / Gray Pall
- Full raid redesign / killbox fortress
- Fluid ritual seeding
- Changing hamlet builder

## Verification

- Compile against Krafs.Rimworld.Ref / Steam Managed
- Runtime: new Lantern Village game; confirm mist/eclipse letters and conditions; save/reload keeps schedule
