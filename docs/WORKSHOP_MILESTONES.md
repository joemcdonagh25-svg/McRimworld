# McRimworld Workshop — Milestones

**Purpose:** Sequence the Workshop (the generic "describe → build → play" system) as small, verifiable slices, the same way each mod under `mods/` sequences its own milestones.

Status labels (same vocabulary as the experience schema's `implementation.status`):

| Label | Meaning |
|---|---|
| SPECIFIED | Described in a manifest or doc only |
| IMPLEMENTED COMPLETE | Code exists and does what's described |
| COMPILE/UNIT VERIFIED | Automated tests pass |
| RUNTIME VERIFIED | Proven against a real local RimWorld install / live session |
| BLOCKED | Cannot proceed; reason recorded |

---

## M1 — Shared installer, schema and profile system

**IMPLEMENTATION COMPLETE:** yes
**UNIT VERIFIED:** yes (`workshop/tests`, 25 tests, `python -m unittest discover -s workshop/tests`)
**RUNTIME VERIFIED:** partial — `env` and `validate` were run against Joe's real machine and correctly found Steam, RimWorld, installed DLC, and which Lantern Village mods are/aren't subscribed. `activate`/`restore` are unit-tested against temp directories only; they have **not** been run against the real `ModsConfig.xml` as part of this milestone, to avoid mutating Joe's live mod setup without a specific ask.

### Goal

Build the reusable foundation everything else plugs into: a schema for describing an experience, read-only environment discovery, a mod profile manager with safe backup/restore, and a CLI launcher — proven against the Lantern Village fixture, without touching The Ark or Vampire Lord.

### In scope

- `workshop/schema/experience.schema.json` — the experience specification contract.
- `workshop/mcrimworld_workshop/discovery.py` — Steam / RimWorld / Mods / Workshop content / user config / saves / DLC / `.NET` SDK discovery.
- `workshop/mcrimworld_workshop/profiles.py` — read `ModsConfig.xml`, scan installed mods, resolve load order, validate requirements, backup/activate/restore.
- `workshop/mcrimworld_workshop/manifest.py` + `schema_validate.py` — dependency-free manifest validation.
- `workshop/mcrimworld_workshop/launcher.py` + `cli.py` + `workshop.ps1` — the CLI launcher (`env`, `list`, `inspect`, `validate`, `activate`, `restore`, `launch`).
- `workshop/experiences/lantern-village.json` — validation fixture for all of the above.
- `workshop/tests/` — automated coverage, including config backup/restore.

### Non-goals (explicitly deferred)

Scenario generation, starting-pawn generation, world/map generation, prebuilt settlements, one-click new-game/resume, save isolation, the campaign director, and anything that needs a specific experience's gameplay to actually run. `launch` starts RimWorld via Steam with whatever profile is active — it does not pick a scenario or load a save.

### Acceptance

- [x] The Ark and Vampire Lord are unchanged (`git diff` against `main` touches no file under `mods/`).
- [ ] ~~remain buildable~~ — **pre-existing, unrelated build gap found and left alone**: `dotnet build` on both `.csproj` files fails on a clean `origin/main` checkout (confirmed before touching anything in `workshop/`) because neither project references `UnityEngine.CoreModule` (only `Assembly-CSharp`), so any `UnityEngine`/`Rect`/`Vector2` usage (e.g. `Dialog_ArkCampaignDebug.cs`, `VampireLordCampaignHud.cs`, `Page_VampireLordAutoIdeo.cs`) fails to compile via the NuGet `Krafs.Rimworld.Ref` fallback path. This predates and is unrelated to Workshop M1; left unfixed per "do not modify either existing mod unless strictly necessary and explicitly justified" — flagging for Joe to decide whether/when to fix.
- [x] `workshop/experiences/lantern-village.json` parses and validates against the schema.
- [x] The installed RimWorld environment can be detected (`env` command — runtime-verified on Joe's machine: Steam, RimWorld 1.6 install, all five owned DLC, 158 saves, .NET 8 SDK all correctly found).
- [x] Missing and conflicting mods can be reported (`validate lantern-village` — runtime-verified: correctly reported 4 of 5 required Workshop mods as not installed, and did not false-positive the one that happens to already be installed).
- [x] A mod profile can be activated safely and restored — unit-verified (`test_activate_then_restore_round_trips_exactly`, `test_no_saves_or_unrelated_files_are_touched`). Not yet run against the live `ModsConfig.xml`.
- [x] The launcher can start RimWorld using the selected profile (`launch` constructs and opens `steam://rungameid/294100`; not runtime-exercised in this session because it would launch the game unattended).
- [x] Automated tests cover configuration backup and restoration (`workshop/tests/test_profiles.py::BackupAndRestoreTests`).
- [x] No existing saves were changed (tests assert this directly; `env`/`validate`/`inspect`/`list` are read-only and are all that was run against the real machine).
- [x] This report states exactly what was verified and what wasn't (see above).

### Known risks / deviations

- Lantern Village's mod `packageId`/`workshopId` values are **best-effort guesses** from mod names (marked in the manifest's `notes` fields). They must be confirmed against the actual subscribed Workshop items before this experience moves past `specified`.
- `activate`/`restore` have not been runtime-verified against the real `ModsConfig.xml`. Recommended next step: run `workshop/workshop.ps1 activate lantern-village` once mods are actually subscribed, confirm RimWorld's Mods screen shows exactly the expected set, then `restore` and confirm it's back to the prior state.
- The mini JSON Schema validator in `schema_validate.py` implements only the keyword subset the experience schema uses (see its module docstring) — it is not a general-purpose validator.
- Steam/RimWorld path discovery assumes the conventions already used by `tools/fix-rimworld-black-screen.ps1` and `TheArk.csproj` (default Steam install dir, `steamapps/common/RimWorld`). A non-default Steam library is handled via `libraryfolders.vdf` parsing but has not been tested against a genuinely multi-library setup.

---

## M2 — Scenario and ideology generation

Turn a manifest's `scenario.desired` / `ideology.desired` into an actual `ScenarioDef` + Ideoligion, following the `ScenPart_*PlaytestSetup` pattern already used by The Ark and Vampire Lord. Not started.

## M3 — Starting pawns, world, map and prebuilt settlement generation

Turn `startingPawns.desired` / `world.desired` into real map generation and starting colonist configuration. Not started.

## M4 — One-click start/resume, save isolation, end-to-end runtime validation

Implement the `launch` behaviour the schema already allows experiences to describe: select-and-start a scenario, or resume a specific save, without manual steps in RimWorld's own menus; isolate each experience's saves so they don't collide. Not started.

## M5 — Reusable custom event and campaign director

Turn `campaign.desired` into a generic, data-driven event/escalation director (Lantern Village's "folklore to horror" arc is the first real user). Not started.
