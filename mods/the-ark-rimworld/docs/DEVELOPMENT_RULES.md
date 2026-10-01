# The Ark — Development Rules

**Audience:** Human engineers and AI coding agents working on THE ARK.  
**Goal:** Keep the known-good foundation working while shipping small, verified vertical slices.

---

## Non-negotiable foundation

The repository already contains a verified RimWorld 1.6 mod foundation:

- Target framework: `net472`
- Compiles with zero errors
- References installed RimWorld `Assembly-CSharp.dll` (local path; not committed)
- Produces `Assemblies/TheArk.dll`
- Junction-linked into RimWorld's Mods directory for development
- Mod enables; `TheArkBootstrap` logs `[The Ark] Initialised successfully.`

**This state must remain working.** Do not change the existing working build configuration unless a milestone explicitly requires it.

---

## Extend, do not recreate

Vanilla RimWorld and Odyssey remain authoritative where practical.

- Use RimWorld health, inventory, jobs, needs, and combat.
- Prefer Odyssey gravship systems over a parallel ship stack.
- Prefer existing world / map / incident / quest systems.
- Prefer supported Verse / RimWorld extension mechanisms.
- Use Harmony **only** when there is no reasonable existing extension point.

---

## Never invent RimWorld APIs

Before using an unfamiliar RimWorld or Odyssey API:

1. Inspect available referenced assemblies or other reliable source evidence.
2. Determine the **actual** API surface.
3. Document useful findings in `docs/RIMWORLD_API_NOTES.md`.
4. Only then implement against it.

Do **not** write speculative integration code against plausible-sounding APIs.

---

## Milestone delivery bar

Every implementation milestone must:

- compile successfully with **zero** compile errors;
- avoid unrelated changes;
- preserve the existing working mod bootstrap;
- provide a concise summary of files changed;
- state how the feature should be **manually tested** in RimWorld.

Prefer small, testable vertical slices. Do not attempt to implement the entire design at once. Do not silently expand scope.

If a requested feature requires architectural work outside the requested milestone, **explain that before implementing**.

---

## Repository hygiene

| Rule | Detail |
|------|--------|
| No proprietary DLLs | Never copy RimWorld, Odyssey, or Unity proprietary assemblies into this repo |
| No proprietary assets | Never commit proprietary game assets |
| Build output | `Assemblies/TheArk.dll` is produced by build; follow existing project conventions |
| Docs authority | Design changes update `docs/` — do not let code silently diverge from documented intent without updating docs |

---

## Campaign state ownership

- Campaign state must have **one clear authoritative owner**.
- Avoid distributing authoritative campaign state across unrelated pawns, buildings, or maps.
- Do not blindly implement the conceptual `ArkState` field list from `GAME_DESIGN.md`.
- When implementation begins, first determine the appropriate RimWorld save / persistence architecture (see M1).

---

## UI rules

- Gameplay systems must not become tightly coupled to UI.
- UI reads state and invokes explicit commands / actions where necessary.
- Debug UI (M2) is allowed to be crude; player-facing UI must still respect the read / command split.

---

## Change discipline

1. Work only on the active milestone unless the user explicitly expands scope.
2. Keep diffs focused: no drive-by refactors, no unrelated cleanup.
3. After code changes, run the project build as a regression check.
4. Report: files changed, how to test in RimWorld, any new API notes, any deferred ideas logged.

---

## AI agent checklist (before marking work done)

- [ ] Did this milestone stay within its non-goals?
- [ ] Did the project still compile with zero errors?
- [ ] Is bootstrap still intact?
- [ ] Were unfamiliar APIs verified and noted in `RIMWORLD_API_NOTES.md`?
- [ ] Were deferred ideas logged (e.g. `ROADS_NOT_TAKEN.md`)?
- [ ] Is the manual test path written in plain language?

---

## Related documents

- `GAME_DESIGN.md` — what we are building for players  
- `ARCHITECTURE.md` — system boundaries and persistence principles  
- `MILESTONES.md` — sequence and acceptance criteria  
- `RIMWORLD_API_NOTES.md` — verified API evidence only  
