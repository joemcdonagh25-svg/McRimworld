# McRimworld Workshop — Implementation Contract

**Audience:** the user pasting a new experience specification, and the Claude Code session that consumes it.

This document defines the contract between a ChatGPT-authored experience specification and the Workshop tooling in `workshop/`. It is scoped to what M1 actually implements; later milestones extend it (see [`WORKSHOP_MILESTONES.md`](WORKSHOP_MILESTONES.md)).

## The workflow this supports

1. You develop a game concept with ChatGPT.
2. ChatGPT produces a JSON document conforming to [`workshop/schema/experience.schema.json`](../workshop/schema/experience.schema.json).
3. You save it as `workshop/experiences/<id>.json` and paste the manifest (or just say "build `<id>`") into Claude Code.
4. Claude Code (or you, via `workshop/workshop.ps1`) validates it, reports what's missing on this machine, and — once requirements are met — activates its mod profile.
5. You launch RimWorld through the Workshop launcher.
6. Playtest findings go back into the next version of the manifest.

M1 implements steps 2–4 and a best-effort step 5 (Steam launch, not one-click new-game/resume — that's M4). Step 6 is a human process, not tooling.

## What a manifest must contain

Every experience manifest is a single JSON file validated against `experience.schema.json`. At minimum it declares:

- **Identity**: `id`, `version`, `name`, `schemaVersion`.
- **RimWorld target**: `rimworld.version`, and any `rimworld.dlc.required` / `optional`.
- **Mods**: every Workshop/local mod the experience needs (`mods[]`), plus any mod that must be built from source inside this repo (`customMods[]`, e.g. a sibling of `mods/vampire-lord`).
- **Load order**: either implied by each mod's `dependsOn` / `loadAfter` / `loadBefore`, or an explicit `loadOrder.sequence`.

Everything else — `scenario`, `startingPawns`, `world`, `ideology`, `campaign`, `launch` — follows the same two-part shape:

```json
{
  "desired": { "...": "whatever the design calls for; schema-open on purpose" },
  "implementation": { "status": "specified", "files": [], "notes": "" }
}
```

`desired` is the design intent (what ChatGPT wrote). `implementation.status` is one of `specified`, `implemented`, `runtime_verified`, `blocked` — **never claim `runtime_verified` without an actual observed RimWorld session.** This separation exists so a manifest can describe an entire experience up front while implementation catches up milestone by milestone, without ever lying about what's actually built.

## How Claude Code should consume a manifest

1. **Validate first.** Run `workshop/workshop.ps1 validate <id>` (or call `mcrimworld_workshop.manifest.load_manifest` directly). Do not hand-edit around schema errors — fix the manifest or the schema, not both at once without saying so.
2. **Check the environment.** Run `workshop/workshop.ps1 env`. If Steam, RimWorld, or the `.NET` SDK aren't found, say so plainly; don't guess paths.
3. **Report missing mods, don't fetch them.** The Workshop never downloads or subscribes to Workshop content. `validate` lists missing `packageId`s and, where `workshopId` is known, a subscribe link. Confirm guessed `packageId`/`workshopId` values against the real Workshop item before treating a manifest as ready — the Lantern Village fixture ships with several explicitly marked as guesses.
4. **Activate only when requirements are met**, or when the user explicitly accepts a partial/degraded activation. `activate` always backs up the existing `ModsConfig.xml` first and records enough state for a single `restore` to undo it.
5. **Never implement ahead of the milestone you're on.** If a manifest's `scenario`/`startingPawns`/`world` sections describe generation behaviour, and you're doing M1 work, leave `implementation.status: "specified"` and say explicitly that generation is out of scope until M2/M3.
6. **Preserve existing mods.** Nothing in `workshop/` may modify `mods/the-ark-rimworld` or `mods/vampire-lord` to implement a new experience. If a new experience needs a genuinely shared capability, propose extracting it — don't fork mod code silently.

## Non-negotiables (carried over from the mod engineering rules)

- Non-destructive: every mutation to the user's real RimWorld install goes through `profiles.activate_profile`, which backs up first. Nothing in `workshop/` ever touches `Saves/`.
- Deterministic: an activation record (`workshop/state/activation-state.json`, gitignored) captures what was active before, so `restore` is exact, not best-effort.
- Verified language: compile/implementation success and runtime success are always reported separately. See `docs/WORKSHOP_MILESTONES.md` for the status vocabulary.
