# The Ark — Architecture

**Status:** Engineering architecture for THE ARK.  
**Companion docs:** `GAME_DESIGN.md` (intent), `DEVELOPMENT_RULES.md` (process), `MILESTONES.md` (sequence).

---

## Current verified foundation (must remain working)

| Fact | Detail |
|------|--------|
| Target | RimWorld 1.6, .NET Framework 4.7.2 / `net472` |
| Project | `Source/TheArk/TheArk.csproj` |
| Output | `Assemblies/TheArk.dll` |
| Bootstrap | `TheArkBootstrap` via `[StaticConstructorOnStartup]` |
| Log proof | `[The Ark] Initialised successfully.` |
| Package | `joemcdonagh.theark` (`About/About.xml`) |

Do not change the working build configuration unless a milestone explicitly requires it and the change is justified.

---

## Technical philosophy

**Extend RimWorld. Do not recreate RimWorld.**

Vanilla RimWorld and Odyssey systems remain authoritative wherever practical.

| Prefer | Avoid |
|--------|--------|
| RimWorld health | Custom health system |
| RimWorld inventory / equipment | Parallel inventory |
| Odyssey gravship systems | Parallel ship simulation |
| Existing world / map / incident systems | Homegrown world simulation when vanilla suffices |
| Supported Verse / RimWorld extension points | Speculative private-API assumptions |
| Harmony only when no reasonable extension point exists | Harmony as the default integration tool |

Never invent a RimWorld API and assume it exists. Verify first; document findings in `RIMWORLD_API_NOTES.md`; only then implement.

---

## System boundaries (conceptual)

Treat these as **boundaries**, not mandatory class names:

| Boundary | Responsibility |
|----------|----------------|
| Campaign | Authoritative campaign lifecycle and persistence ownership |
| Pursuit | Pressure value, states, and (later) consequences |
| Expeditions | Landing purpose, objectives, rewards |
| Progression | Ark tiers, modules, capability unlocks |
| Crew | Roster intent, role gaps, recruitment framing |
| Gravship integration | Land / depart / ship capability hooks into Odyssey |
| UI | Presentation and explicit player/debug commands |

### High-level relationship

```
ArkCampaign
    |
    +-- Campaign State
    +-- Pursuit
    +-- Expeditions
    +-- Progression
    +-- Crew
    +-- Gravship Integration
```

UI sits **beside** this graph, not inside authoritative systems:

- UI **reads** state.
- UI invokes **explicit commands / actions** when the player (or a debug tool) must change state.
- Gameplay systems must not become tightly coupled to UI types.

---

## Dependency rules

1. **Dependencies are directional and explicit.** Lower-level or sibling systems do not reach “up” into UI.
2. **One authoritative owner for campaign state.** Do not scatter authoritative campaign fields across unrelated pawns, buildings, or maps.
3. **Landing session state is temporary.** Landing-specific timers and local expedition session data reset or close on departure; durable campaign fields persist.
4. **Prefer composition over god-objects**, but avoid premature micro-frameworks. Small vertical slices first.
5. **Harmony patches** (if any) must be narrow, documented, and justified by a missing extension point.

---

## Persistence principles

Campaign state is conceptual until M1+. Before coding persistence:

1. Determine the appropriate RimWorld save / load architecture (e.g. `GameComponent`, `WorldComponent`, `MapComponent`, or another verified pattern).
2. Choose **one** authoritative owner for durable Ark campaign data.
3. Document the chosen API and rationale in `RIMWORLD_API_NOTES.md`.
4. Keep the initial data set tiny (see M1 in `MILESTONES.md`).

### Conceptual durable fields (not a required schema)

Examples of fields that may eventually live under the campaign owner:

- CampaignActive
- ArkTier
- CampaignDay
- LandingNumber
- PursuitLevel
- CrewRoster
- DiscoveredLocations
- RecoveredArtifacts
- UnlockedModules
- StarMapFragments
- CampaignFlags

Do **not** blindly implement this exact model.

### Conceptual landing-session fields (temporary)

Examples that should generally *not* outlive a departure without an explicit design reason:

- Time since landing
- Active expedition session
- Local objective progress tied only to this landing

---

## Gravship integration stance

- Odyssey gravship behaviour is the source of truth for flight, landing, and ship presence where available.
- The Ark **wraps narrative and campaign meaning** around those systems; it does not replace them with a parallel ship stack.
- Landing detection and departure lifecycle (M3, M8) must be verified against real Odyssey / RimWorld APIs before coding.

---

## Mod package layout (repository)

| Path | Role |
|------|------|
| `About/` | Mod metadata |
| `Assemblies/` | Built `TheArk.dll` (output of compile) |
| `Source/TheArk/` | C# source |
| `Defs/`, `Patches/`, `Languages/`, `Textures/` | Content folders (populate as milestones require) |
| `docs/` | Design and engineering authority |

Never commit proprietary RimWorld, Odyssey, or Unity DLLs or game assets into this repository.

---

## Bootstrap contract

`TheArkBootstrap` is the known-good startup signal. Later systems may initialise from startup, game load, or other verified hooks — but:

- Preserve a clear, logged initialisation path.
- Do not remove or break the existing success log without replacing it with an equally clear regression signal.
- Unrelated refactors must not endanger this contract.

---

## Open architectural decisions (resolve at the named milestone)

| Decision | Resolve by |
|----------|------------|
| Persistence owner type (`GameComponent` vs alternatives) | M1 |
| How “landing” is detected for Odyssey gravships | M3 |
| How departure clears session state without wiping campaign | M8 |
| How expeditions attach to maps / quests / incidents | M9 |
| How modules map onto ship buildings / research / scenario | M11+ |

Until those milestones, treat related ideas as deferred — see repository `ROADS_NOT_TAKEN.md`.
