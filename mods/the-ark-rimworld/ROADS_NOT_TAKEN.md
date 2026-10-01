# Roads Not Taken

Long-term memory of ideas that were considered but **not** implemented yet.  
Tag: `(deferred)`

Agents and humans: before expanding scope, check this file. Prefer the active milestone in `docs/MILESTONES.md`.

---

## Campaign & persistence

- Idea: Full conceptual `ArkState` schema (CrewRoster, DiscoveredLocations, RecoveredArtifacts, UnlockedModules, StarMapFragments, CampaignFlags, etc.) as the first persistence model `(deferred)` — M1 uses only the tiny field set.
- Idea: Distributing campaign flags onto pawns, ship buildings, or per-map components as secondary sources of truth `(deferred)` — architecture forbids multiple authoritative owners.

## Pursuit

- Idea: Multi-factor Pursuit drivers (wealth, crew count, Ark tier, player actions, expedition events, campaign events) `(deferred)` — V0 is time-since-landing only (M5).
- Idea: Harbinger as a distinct encounter / pressure fantasy beyond raid-point escalation `(deferred)` — after M7 baseline consequence exists.
- Idea: Tuned Pursuit decay / freeze / reset policies with player-facing explanation on every departure `(deferred)` — must be chosen explicitly at M8, richer tuning later.

## Expeditions & content

- Idea: Full expedition package (primary + optionals + biome context + rewards + evacuation decision) `(deferred)` — M9 is one simple objective/reward loop.
- Idea: Procedural expedition generation at scale `(deferred)`.
- Idea: Star Map destination selection fantasy `(deferred)` — M11+.

## Progression & ship

- Idea: Full Explorer-era module set with mutual exclusion / capacity limits (Medical, Workshop, Laboratory, Armoury, Crew quarters, Cargo, Drone bay, Hydroponics) `(deferred)` — M11+.
- Idea: Parallel custom ship simulation instead of Odyssey gravship systems `(deferred — rejected direction unless Odyssey proves insufficient; prefer extend Odyssey)`.

## Crew

- Idea: Custom RimWorld pawn classes / races for Captain, Engineer, Survivalist, and recruitment roles `(deferred)` — prefer vanilla skills/backstories/traits first.
- Idea: Hard-enforced 16 human crew cap as a coded system `(deferred)` — design target exists; enforcement timing undecided.

## Technical approaches

- Idea: Harmony-first integration style for most features `(deferred — rejected as default)`; Harmony only when no reasonable extension point exists.
- Idea: Polished player UI before debug UI `(deferred)` — M2 is debug-first.
- Idea: Implementing gameplay systems during documentation pass `(deferred — intentionally skipped)`.
