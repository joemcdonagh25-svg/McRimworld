# McRimworld Workshop

Turns a structured experience specification into an installed, playable RimWorld experience: **Describe → Build → Play.**

This is the generic, reusable layer. It knows nothing about Lantern Village, The Ark, or Vampire Lord specifically — it only knows how to read a manifest conforming to `schema/experience.schema.json` and act on it. See [`../docs/WORKSHOP_CONTRACT.md`](../docs/WORKSHOP_CONTRACT.md) for the full contract and [`../docs/WORKSHOP_MILESTONES.md`](../docs/WORKSHOP_MILESTONES.md) for what's actually implemented vs. planned.

## Layout

```
workshop/
  schema/experience.schema.json   # the manifest contract
  experiences/*.json              # one manifest per experience
  mcrimworld_workshop/            # the Python package implementing discovery/profiles/CLI
  tests/                          # unit tests (python -m unittest)
  state/                          # generated at runtime: backups + activation record (gitignored)
  workshop.ps1                    # Windows entry point
```

## Using it

From the repository root, or from inside `workshop/`:

```powershell
.\workshop\workshop.ps1 env                      # what does this machine have installed?
.\workshop\workshop.ps1 list                      # what experiences exist?
.\workshop\workshop.ps1 inspect lantern-village   # show one manifest
.\workshop\workshop.ps1 validate lantern-village  # schema + "is everything installed?"
.\workshop\workshop.ps1 activate lantern-village  # back up current mods, apply this profile
.\workshop\workshop.ps1 launch                    # start RimWorld via Steam with the active profile
.\workshop\workshop.ps1 restore                   # undo the last activate
```

Equivalent without the wrapper: `python -m mcrimworld_workshop <command>` from inside `workshop/`.

Run the tests with:

```powershell
cd workshop
python -m unittest discover -s tests -v
```

No extra packages are required — everything is Python 3.10+ standard library, by design (see "Why no dependencies" below).

## Adding a new experience

1. Design the experience with ChatGPT; ask it to produce JSON conforming to `schema/experience.schema.json`. Give it the schema file directly — it's the contract.
2. Save the result as `workshop/experiences/<id>.json`.
3. In Claude Code, say what you want done (e.g. "validate and activate `<id>`"), or run the commands above yourself.
4. Claude Code should: validate the manifest, run `env`, run `validate` and report exactly what's missing, and only activate once you've confirmed real `packageId`/`workshopId` values and subscribed to what's needed. See the contract doc for the full checklist.

## Scoped instructions for Claude Code sessions working in `workshop/`

These add to, and do not replace, `.cursor/rules/rimworld-engineering.mdc` (which governs the C# mods under `mods/`). That file's PLAN → IMPLEMENT → REVIEW → FIX → VERIFY discipline and its verification-state vocabulary (`IMPLEMENTATION COMPLETE` / `RUNTIME VERIFIED`, never claimed early) apply here too.

Specific to this subsystem:

- **This code is generic.** If you catch yourself writing `if experience_id == "lantern-village"` anywhere under `mcrimworld_workshop/`, stop — experience-specific behaviour belongs in a manifest under `experiences/`, not in the installer.
- **Never touch `mods/the-ark-rimworld` or `mods/vampire-lord` from here.** If a new experience needs a capability one of those mods already has, that's a signal to extract a shared piece deliberately, not to reach into their source.
- **Never write outside `workshop/state/` and the real RimWorld `Config/ModsConfig.xml`.** In particular, never touch `Saves/`, `Scenarios/`, or `Prefs.xml` from this package — that's the territory of `tools/fix-rimworld-black-screen.ps1` and out of scope here.
- **`activate` must always back up first.** If you're changing `profiles.py`, keep `activate_profile` unconditionally calling `backup_config` before it writes anything, and keep the activation record (`state/activation-state.json`) as the single source of truth `restore` reads from.
- **Don't add a JSON Schema dependency casually.** `schema_validate.py` is intentionally a minimal hand-rolled validator so the Workshop has zero third-party dependencies and works offline on a fresh Windows box. If the real schema's needs outgrow it, say so explicitly before reaching for `jsonschema` or similar.
- **Don't implement M2+ behaviour under an M1 PR.** `scenario`, `startingPawns`, `world`, `campaign`, and one-click `launch` are real, named future milestones (see `docs/WORKSHOP_MILESTONES.md`) — leave their `implementation.status` as `specified` and say so, rather than quietly building a piece of them.
- **Prefer running `env`/`validate`/`inspect`/`list` against the real machine** when checking your work — they're read-only. Treat `activate`/`restore` against the *real* `ModsConfig.xml` as something to ask about first, the same way any other action that mutates the user's local environment would be; the test suite's temp-directory coverage is what you rely on by default.

### Why no dependencies

Minimal complexity and local-first both point the same way: the Workshop should run on a bare Windows + Python install without `pip install` first. Everything here is stdlib (`argparse`, `json`, `xml.etree.ElementTree`, `pathlib`, `subprocess`, `winreg`). If that stops being viable, document why before adding a dependency.
