# McRimworld — Changelog

Newest entries first. Records what shipped, what we learned, key paths, operator notes, and next steps.

---

## 2026-10-06 — Vampire Lord VL: Quicktest debug harness

### What shipped
- Developer DebugActions prefixed **`VL:`** for Quicktest: state dumps, time, production waves, threat, keep repair, hemogen/deathrest, siege presets, reset, save snapshot.
- Branch: `cursor/vampire-lord-debug-harness-c5ad`.

### Operator notes
- Pull, restart, Quicktest → Dev Mode → search `VL:` → **Preset — Last Stand**.

---

## 2026-10-06 — The Ark Quicktest DebugActions harness (`ARK:`)

### What shipped
- Dev-only DebugActions to inspect/mutate Ark campaign state from Quicktest (presets + save-test snapshot + RESET).
- Branch: `cursor/ark-debug-harness-5195`.

### Operator notes
- Pull → restart → Quicktest → Dev Actions → search **`ARK:`**.

### Next steps
- Joe runtime-verify in RimWorld; merge when ready.

---

## 2026-10-02 — Vampire Lord M5 Campaign UI HUD

### What shipped
- Always-on Blood / Wave / Threat HUD (bottom-left). No Harmony.
- Branch: `cursor/vampire-lord-campaign-ui-c5ad`.

### Operator notes
- Pull, restart, load `VL_prep` — look bottom-left for the Black Keep strip.

---

## 2026-10-02 — The Ark Pressure V0 RUNTIME VERIFIED (Joe Player.log)

### What shipped
- Docs only: mark Pressure V0 pack (M4 Landing Timer + M5 Pursuit V0 + M6 band letters) **RUNTIME VERIFIED** from Joe’s live one-click proof.
- Also clears leftover merge conflict markers in this parent changelog from the #28 / #29 merge order.
- Branch: `cursor/ark-pressure-v0-runtime-verified-5195`.

### What we learned
- Log: `[The Ark] [DEV] Pressure V0 proof PASS: session=True, timerStartZero=True, timerDay=True, pursuitGrew=True, harbinger=True, timerCleared=True, pursuitKept=True, LandingNumber=1, Pursuit=81 (HARBINGER 81–100)`
- New Game still shows known Ideology ChooseIdeoPreset / Anomaly / History noise; does not block Pressure V0 proof.

### Operator notes
- No M7 (Pursuit Incident) until Joe asks.

### Next steps
- Stop; Joe chooses next Ark milestone when ready.

---

## 2026-10-02 — The Ark Pressure V0 pack (M4+M5+M6)

### What shipped
- One playtest pack: Landing Timer + Pursuit-from-landed-days + band letters; one-click **Run Pressure V0 Proof**; DLL rebuild.
- Branch: `cursor/ark-pressure-v0-5195` → merged into `main`.

### Operator notes
- Pull → restart once → New Game → The Ark → Dev Mode → **Run Pressure V0 Proof** → paste `Pressure V0 proof PASS`.

### Next steps
- Done: Joe RUNTIME VERIFIED via one-click proof (see entry above).

---

## 2026-10-02 — Vampire Lord less-fussy playtest defaults

### What shipped
- AutoFortify + fast pace + quiet letters ON by default; no game-start dialog; pack-paced VL agent rule.
- Branch: `cursor/vampire-lord-less-fussy-c5ad`.

### Operator notes
- Pull, restart, one New Game → save `VL_prep` → iterate with Trigger Wave.

---

## 2026-10-02 — Vampire Lord Ideology settle polish (merge)

### What shipped
- Vampire Lord scenario subclasses `Scenario` to replace `Page_ChooseIdeoPreset` with an auto-classic ideo page (harden `allowedCultures`, skip broken UI). No Harmony.
- Branch: `cursor/vampire-lord-ideo-settle-c5ad`.

### Operator notes
- Pull, restart, New Game → Vampire Lord with Ideology on; confirm no ChooseIdeoPreset NRE and AutoIdeoPage log line.

---

## 2026-10-02 — Vampire Lord fixture-save playtest workflow

### What shipped
- `VL_prep` workflow docs + Dev Mode Ensure Prep Fixture / Auto-Fortify Playtest toggle.
- Branch: `cursor/vampire-lord-m4-fortify-c5ad` (updates PR #24).

### Operator notes
- One New Game to make `VL_prep`; then load that save for fortify/wave iteration.

---

## 2026-10-01 — Vampire Lord fortify offer letter fix

### What shipped
- Delayed fortify Accept letter; fix LetterDef sound; clarify letter stack vs Quests.
- Same branch: `cursor/vampire-lord-m4-fortify-c5ad`.

### Operator notes
- Pull/restart; look for **Blood for the Walls** in the letter stack (not Quests).

---

## 2026-10-01 — Vampire Lord M4 Keep Fortification V0

### What shipped
- Spend Blood Tithe between waves on gate sandbags (Accept letter + debug). No Harmony.
- Branch: `cursor/vampire-lord-m4-fortify-c5ad`.

### What we learned
- Prep-window gate (`!WarningIssued`) is enough to time the offer without a custom UI.

### Operator notes
- Pull, restart, New Game → Vampire Lord; accept **Blood for the Walls** or use **Force Fortify Now**.

### Next steps
- Joe RUNTIME VERIFIED; then pick next parked card when ready.

---

## 2026-10-01 — The Ark M3 Landing Detection RUNTIME VERIFIED

### What shipped
- Docs: Joe’s Player.log proves Simulate Landing → `Landing session STARTED`, LandingNumber=1, Session=True.
- Branch: `cursor/ark-m3-runtime-verified-5195`.

### Operator notes
- No M4 until Joe asks.

### Next steps
- Stop; Joe chooses next Ark milestone when ready.

---

## 2026-10-01 — Project-wide RimWorld engineering Cursor rule

### What shipped
- Added always-on Cursor rule for RimWorld mod engineering: PLAN → IMPLEMENT → REVIEW → FIX → VERIFY, lifecycle/persistence/scope discipline, model selection policy.
- Path: `.cursor/rules/rimworld-engineering.mdc` (`alwaysApply: true`).
- Branch: `cursor/rimworld-engineering-rule-be44`.

### What we learned
- Repo had no `.cursor/rules` yet; this is the first project-wide agent contract for multi-agent / human maintenance.

### Operator notes
- Rule applies to all agents in this workspace once merged/pulled; no mod DLL rebuild required.

### Next steps
- Use the rule on subsequent Ark / Vampire Lord work.

---

## 2026-10-01 — The Ark M3 Landing Detection V0

### What shipped
- Landing session + LandingNumber increment; Odyssey poll detection; Dev Simulate Landing / End Session; DLL rebuild.
- Branch: `cursor/ark-m3-landing-detection-5195`.

### Operator notes
- Pull → New Game → The Ark → Dev Mode → Simulate Landing (second call ignored while session active).

### Next steps
- Done (Joe RUNTIME VERIFIED via Simulate Landing).

---

## 2026-10-01 — The Ark M1 + M2 RUNTIME VERIFIED

### What shipped
- Docs: Joe’s Player.log proves M1 fixture persist across save/quit/load (`Active=True, Day=47, Landing=6, Tier=2, Pursuit=73` Applied and LoadedGame).
- Branch: `cursor/ark-newgame-runtime-confirmed-5195`.

### What we learned
- Campaign GameComponent persistence works on Joe’s install despite Ideology settle/load noise.

### Operator notes
- No M3 Landing Detection until Joe asks.

### Next steps
- Stop; Joe chooses next Ark milestone when ready.

---

## 2026-10-01 — The Ark Ideo/colonist start harden (from Joe Player.log)

### What shipped
- Re-enabled Ark playtest ScenPart: generate missing player ideo, reassign non-colonist crew, start dialog + starter supplies; DLL rebuild.
- Branch: `cursor/ark-ideo-colonist-harden-5195` → merged into `main`.

### What we learned
- ConfigurePawns path is clean (scenario loads, campaign activates).
- Next blocker in Joe’s log: `Page_ChooseIdeoPreset.PostOpen` NRE → broken settle cascade (Anomaly/History/goodwill/non-colonist). Same class as Vampire Lord; same harden pattern.

### Key paths
- `mods/the-ark-rimworld/Source/TheArk/Scenario/ScenPart_ArkPlaytestSetup.cs`
- `mods/the-ark-rimworld/Defs/Scenarios/TheArkScenario.xml`
- `mods/the-ark-rimworld/Defs/ScenParts/TheArkScenParts.xml`

### Operator notes
- Pull → restart → New Game → The Ark. Ideo NRE may still log; crew should be colonists and campaign active.

### Next steps
- Done for New Game start (Joe confirmed). Fixture save/load still pending.

---

## 2026-10-01 — Vampire Lord fix: keep as player home Settlement

### What shipped
- Vampire Lord playtest ensure `IsPlayerHome` via Settlement settle / faction assign (not caravan/camp).
- Branch: `cursor/vampire-lord-m3-player-home-c5ad`.

### What we learned
- Raid fallback fixed wave launch; caravan feel + `GetSituations(PlayerColony)` came from missing player Settlement parent.

### Operator notes
- After merge: pull, restart; new Vampire Lord game or Dev Mode **Ensure Player Home**.

### Next steps
- Merge; Joe retest colony feel + wave + Blood Tithe.

---

## 2026-10-01 — Vampire Lord fix: raid home-map fallback

### What shipped
- Wave raids target colonist maps when `IsPlayerHome` is unset; DLL rebuild.
- Branch: `cursor/vampire-lord-m3-raid-home-map-c5ad`.

### Operator notes
- After merge: pull, restart, Trigger Wave Now on the keep map.

### Next steps
- Merge; Joe retest.

---

## 2026-10-01 — The Ark ConfigurePawns fix (from Joe Player.log)

### What shipped
- Scenario uses `ConfigurePawnsXenotypes` + 3 Baseliner (1.6 has no `ConfigurePawns` ScenPartDef).
- Docs/script warn to delete duplicate `Mods\TheArk`.
- Branch: `cursor/ark-configure-pawns-fix-5195` → merged into `main`.

### What we learned
- Player.log root cause: `No RimWorld.ScenPartDef named ConfigurePawns found` + duplicate packageId `TheArk` vs `the-ark-rimworld`.
- Selecting The Ark with null ScenPartDef NREs in scenario info / Next (Joe’s second log).

### Key paths
- `mods/the-ark-rimworld/Defs/Scenarios/TheArkScenario.xml`
- `mods/the-ark-rimworld/About/About.xml`

### Operator notes
- Pull → delete `RimWorld\Mods\TheArk` → enable The Ark + Biotech → New Game → The Ark.

### Next steps
- Joe confirm scenario selectable with clean log (no ConfigurePawns / null def).

---

## 2026-10-01 — Vampire Lord M3 Blood Tithe V0 (implementation)

### What shipped
- Keep Blood Reserve, kill credits, per-wave tithe cost (starved = harder raid), letters, debug actions; DLL rebuild.
- Branch: `cursor/vampire-lord-m3-blood-tithe-c5ad`.

### Operator notes
- After merge: pull, restart, New Game → Vampire Lord → Show Blood Tithe / Trigger Wave / kill hostiles.

### Next steps
- Merge; Joe runtime verify.

---

## 2026-10-01 — The Ark minimal scenario + install diagnostics

### What shipped
- Minimal The Ark ScenarioDef (required 1.6 parts only).
- Player.log diagnostics for mod root, ConfigErrors, ScenarioLister membership.
- `mods/the-ark-rimworld/tools/junction-the-ark.ps1` Windows junction helper.
- Branch: `cursor/ark-scenario-min-diagnostic-5195` → draft PR into `main`.

### What we learned
- VL scenario lists for Joe; Ark still missing ⇒ almost certainly Mods junction/enable for `the-ark-rimworld`, not missing GitHub content.

### Key paths
- `mods/the-ark-rimworld/Defs/Scenarios/TheArkScenario.xml`
- `mods/the-ark-rimworld/Source/TheArk/TheArkBootstrap.cs`
- `mods/the-ark-rimworld/tools/junction-the-ark.ps1`

### Operator notes
- Pull → run junction script → enable The Ark → restart → paste all `[The Ark]` Player.log lines.

### Next steps
- Merge; Joe run junction script and paste log lines if still invisible.

---

## 2026-10-01 — Vampire Lord M3 planning (Blood Tithe V0)

### What shipped
- Docs: M3 milestone card recommending **Blood Tithe V0**; M4+ candidates parked.
- Branch: `cursor/vampire-lord-m3-planning-c5ad`.
- No implementation code in this change.

### Operator notes
- After merge: Joe confirms Blood Tithe (or picks another M3) before any coding.

### Next steps
- Merge planning PR; Joe confirm direction.

---

## 2026-10-01 — Vampire Lord M2 RUNTIME VERIFIED

### What shipped
- Docs: Vampire Lord M2 Playtest Keep marked **RUNTIME VERIFIED** after Joe's live confirm (open gate + map revealed + campaign auto-start).
- Branch: `cursor/vampire-lord-m2-runtime-verified-c5ad`.

### Operator notes
- Docs-only; gameplay already proven on Joe's machine after PR #11.

### Next steps
- Merge docs PR. No M3 until Joe asks.

---

## 2026-10-01 — Vampire Lord M2 restore ScenarioBase (ConfigErrors)

### What shipped
- Restore `ParentName="ScenarioBase"` + ASCII-clean scenario/About text after PR #8 regression.
- Resolve leftover `CHANGELOG.md` merge conflict markers on `main` from PR #8/#9.
- Branch: `cursor/vampire-lord-m2-scenario-parent-c5ad`.

### Operator notes
- After merge: pull, restart, New Game → Vampire Lord (no ConfigErrors), then walk south out of the keep.

### Next steps
- Merge; Joe runtime check.

---

## 2026-10-01 — The Ark scenario visibility (diagnostic + vanilla parts)

### What shipped
- The Ark playtest scenario uses vanilla ScenParts only; campaign activate stays in GameComponent.
- Bootstrap logs whether `TheArk_Playtest` ScenarioDef loaded (Player.log checklist).
- Cleaned accidental git conflict markers left in this CHANGELOG from an earlier merge.
- Branch: `cursor/ark-scenario-visible-5195` → merged into `main`.

### What we learned
- If the mod is not the McRimworld `mods/the-ark-rimworld` root (or a duplicate packageId shadows it), New Game never sees the scenario.
- Player.log line `ScenarioDef TheArk_Playtest LOADED` proves defs loaded; MISSING means install/enable problem.

### Key paths
- `mods/the-ark-rimworld/Defs/Scenarios/TheArkScenario.xml`
- `mods/the-ark-rimworld/Source/TheArk/TheArkBootstrap.cs`
- `README.md`

### Operator notes
- Pull → junction `C:\McRimworld\mods\the-ark-rimworld` → enable The Ark → restart → confirm `TheArk_Playtest LOADED` in Player.log → New Game.

### Next steps
- Joe confirm scenario appears.

---

## 2026-10-01 — The Ark scenario list fix

### What shipped
- Soften Odyssey dependency so New Game → **The Ark** can appear without Odyssey enabled.
- Backup campaign activate on `StartedNewGame` when scenario name is The Ark.
- Branch: `cursor/ark-scenario-list-fix-5195` → merged into `main`.

### What we learned
- Hard `modDependencies` on Odyssey can leave The Ark disabled → scenario invisible.
- Mod root must be junctioned at `mods/the-ark-rimworld/` (About folder), like Vampire Lord.

### Key paths
- `mods/the-ark-rimworld/About/About.xml`
- `mods/the-ark-rimworld/Defs/Scenarios/TheArkScenario.xml`
- `mods/the-ark-rimworld/Source/TheArk/Campaign/ArkCampaignGameComponent.cs`
- `README.md`

### Operator notes
- Pull → ensure Mods junction for The Ark → enable The Ark → restart → New Game list.

### Next steps
- Joe confirm scenario visible; then fixture save/load proof.

---

## 2026-10-01 — Vampire Lord M2 outside access / full-map reveal

### What shipped
- Open south gate, **full-map unfog** (PostMapGenerate + PostGameStart), PackedDirt approach lane, ideo/colonist safety; DLL rebuild.
- Branch: `cursor/vampire-lord-m2-outside-access-c5ad`.
- Joe's pasted log was from the ownership DLL (`gate` text, no reveal line) - that build never cleared fog outside the keep.

### Operator notes
- After merge: pull `C:\McRimworld`, restart, New Game → Vampire Lord. Look for `open gate` + `Revealed map` in the log, then walk south.

### Next steps
- Follow-up: restore ScenarioBase (ConfigErrors regression).

---

## 2026-10-01 — Vampire Lord M2 keep ownership fix

### What shipped
- Player-owned courtyard walls/door; courtyard before pawn spawn; gladius Steel stuff; DLL rebuild.
- Branch: `cursor/vampire-lord-m2-keep-ownership-c5ad`.

### Operator notes
- After merge: pull `C:\McRimworld`, restart RimWorld, start a **new** Vampire Lord game to get owned walls.

### Next steps
- Merge; Joe verify deconstruct + auto campaign.

---

## 2026-10-01 — The Ark M2 / V1.1 Campaign Debug UI

### What shipped
- The Ark Dev Mode debug UI for the five M1 campaign fields (view + explicit Apply / M1 fixture).
- New Game scenario **The Ark** (campaign auto-activates; Odyssey dependency).
- RimWorld 1.6 scenario scaffolding: `ScenarioBase`, `PlayerFaction`, `PlanetLayerFixed`, ScenPartDefs (same class of fix as Vampire Lord M2).
- Rebuilt `mods/the-ark-rimworld/Assemblies/TheArk.dll` (0 errors).
- Branch: `cursor/ark-v1-1-campaign-debug-ui-5195` → PR into `main`.

### What we learned
- Cloud builds need the Vampire-Lord-style `Krafs.Rimworld.Ref` fallback on `TheArk.csproj` when Steam Managed is absent.
- Debug window draft buffers must stay non-authoritative until Apply — same read/command split as design docs.
- Gravship wreckage New Game start deferred; scenario is the door into campaign tooling first.
- 1.6 scenarios need playerFaction + surface layer or New Game Next NREs (learned from VL fix on main).

### Key paths
- `mods/the-ark-rimworld/Defs/Scenarios/TheArkScenario.xml`
- `mods/the-ark-rimworld/Defs/ScenParts/TheArkScenParts.xml`
- `mods/the-ark-rimworld/Source/TheArk/Scenario/`
- `mods/the-ark-rimworld/Source/TheArk/Debug/`
- `mods/the-ark-rimworld/Source/TheArk/TheArk.csproj`
- `mods/the-ark-rimworld/Assemblies/TheArk.dll`
- `mods/the-ark-rimworld/docs/MILESTONES.md`
- `mods/the-ark-rimworld/docs/CHANGELOG.md`

### Operator notes
- Repo: `joemcdonagh25-svg/McRimworld` — branch `cursor/ark-v1-1-campaign-debug-ui-5195` — base `main`.
- No force-push.
- After merge/pull on `C:\McRimworld`: New Game → **The Ark** → Dev Mode → **The Ark (DEV)** → Apply M1 Persistence Fixture → save → full quit → load.

### Next steps
- Joe RUNTIME VERIFIED pass (proves M1 + M2).
- No M3 Landing Detection until Joe asks.

---

## 2026-10-01 — Vampire Lord M2 scenario config fix

### What shipped
- Fix New Game → Vampire Lord crash/config errors: add 1.6 required `PlayerFaction` + `PlanetLayerFixed`, ScenPartDefs for dialog/setup.
- Branch: `cursor/vampire-lord-m2-scenario-fix-c5ad` → draft PR into `main`.

### What we learned
- Selecting a scenario with null/missing required parts NREs in `Page_SelectScenario` / `Scenario.PreConfigure`.

### Key paths
- `mods/vampire-lord/Defs/Scenarios/VampireLordScenario.xml`
- `mods/vampire-lord/Defs/ScenParts/VampireLordScenParts.xml`

### Operator notes
- After merge: `git pull` in `C:\McRimworld`, restart RimWorld, retry New Game → Vampire Lord.

### Next steps
- Merge; Joe runtime verify.

---

## 2026-10-01 — Vampire Lord M2 Playtest Keep

### What shipped
- Vampire Lord **M2 Playtest Keep**: New Game scenario, 1 Sanguophage lord + 2 thrall baseliners, simple courtyard Gen via scenario ScenPart, campaign auto-start, keep-flavoured letters, Biotech dependency.
- Rebuilt `mods/vampire-lord/Assemblies/VampireLord.dll`.
- Branch: `cursor/vampire-lord-m2-playtest-keep-c5ad` → draft PR into `main`.

### What we learned
- Scenario-scoped `ScenPart` is the right seam for courtyard + auto-campaign (avoids affecting Ark / sandbox colonies).
- Joe’s junction (`C:\McRimworld\mods\vampire-lord`) means a `git pull` after merge is enough to pick up the new DLL/Defs.

### Key paths
- `mods/vampire-lord/Defs/Scenarios/VampireLordScenario.xml`
- `mods/vampire-lord/Source/VampireLord/Scenario/ScenPart_VampireLordPlaytestSetup.cs`
- `mods/vampire-lord/About/About.xml`
- `mods/vampire-lord/docs/MILESTONES.md`

### Operator notes
- Repo: `joemcdonagh25-svg/McRimworld` — branch `cursor/vampire-lord-m2-playtest-keep-c5ad` — base `main`.
- No force-push.
- After merge: pull `main` on `C:\McRimworld` → New Game → **Vampire Lord** (Biotech on).

### Next steps
- Merge draft PR; Joe RUNTIME VERIFIED pass on New Game path.
- No M3 until Joe asks.

---

## 2026-10-01 — Vampire Lord M1 RUNTIME VERIFIED

### What shipped
- Parent + mod docs updated: Vampire Lord M1 Wave Director marked **RUNTIME VERIFIED** after Joe’s live RimWorld session.
- Branch: `cursor/vampire-lord-m1-runtime-verified-c5ad` → draft PR into `main`.

### What we learned
- Install path that worked: junction `C:\McRimworld\mods\vampire-lord` → Steam RimWorld `Mods\vampire-lord`.
- Campaign state results appear in the Dev Mode log under `[VampireLord] Campaign state:`.
- Core M1 proof: Start Campaign → warning → raid → threat/wave up (`ThreatLevel=2` after Mob wave).

### Key paths
- `mods/vampire-lord/docs/MILESTONES.md`
- `mods/vampire-lord/docs/CHANGELOG.md`
- `mods/vampire-lord/README.md`
- `README.md`, `CHANGELOG.md`

### Operator notes
- Repo: `joemcdonagh25-svg/McRimworld` — remote `origin` — branch `cursor/vampire-lord-m1-runtime-verified-c5ad` — target `main`.
- Docs-only; no force-push.
- Optional later: save/load + natural timer checks (listed unchecked in milestones).

### Next steps
- Merge draft PR when ready.
- Do not start Vampire Lord M2 until Joe asks.

---

## 2026-10-01 — Vampire Lord M1 Wave Director import

### What shipped
- `mods/vampire-lord/` imported from `vampire-lord-m1.zip` (About, Source, Assemblies/VampireLord.dll, docs).
- Parent README updated with Vampire Lord in the mods table and layout tree.
- Branch: `cursor/vampire-lord-m1-wave-director-038e` → draft PR into `main`.

### What we learned
- Zip was not in this Cloud Agent’s artifact store until attached in-chat; Windows AgentStores path (`bc-f75fb6b8-…`) is local-only and not readable by McRimworld Cloud Agents.
- Zip already used the `mods/vampire-lord/` prefix — extract at repo root.
- Mod root for RimWorld is the folder containing `About/` (junction that folder into `Mods`).

### Key paths
- `mods/vampire-lord/`
- `mods/vampire-lord/About/About.xml` — `joemcdonagh.vampirelord`
- `mods/vampire-lord/Assemblies/VampireLord.dll`
- `mods/vampire-lord/Source/VampireLord/Campaign/VampireLordWaveDirector.cs`
- `mods/vampire-lord/docs/MILESTONES.md`
- `README.md`, `CHANGELOG.md`

### Operator notes
- Repo: `joemcdonagh25-svg/McRimworld` — remote `origin` — branch `cursor/vampire-lord-m1-wave-director-038e` — target PR base `main`.
- No force-push. Additive history only.
- Campaign does **not** auto-start. After merge: enable mod → Dev Mode → **Start Vampire Lord Campaign** → confirm warning → raid → threat up → flips RUNTIME VERIFIED.

### Next steps

---

## 2026-10-01 — Multi-mod parent layout (The Ark subtree)

### What shipped
- McRimworld parent README describing `mods/` layout and operator workflow.
- `mods/the-ark-rimworld/` added via **git subtree** from `https://github.com/joemcdonagh25-svg/the-ark-rimworld` (`main`).
- Full Ark commit history preserved in the parent graph (merge commit + Ark ancestry).
- Branch: `cursor/multi-mod-layout-15d0` → draft PR into `main`.

### What we learned
- No `artifacts/mcrimworld-layout-15d0.bundle` was available in this Cloud Agent environment; rebuilt cleanly with `git subtree add` instead of bundle import.
- Subtree keeps Ark’s own `docs/CHANGELOG.md` and source tree intact under the prefix — parent CHANGELOG tracks parent-level layout only.

### Key paths
- `README.md`
- `CHANGELOG.md`
- `mods/the-ark-rimworld/` (subtree root)
- `mods/the-ark-rimworld/About/About.xml`
- `mods/the-ark-rimworld/docs/`

### Operator notes
- Repo: `joemcdonagh25-svg/McRimworld` — remote `origin` — branch `cursor/multi-mod-layout-15d0` — target PR base `main`.
- No force-push. History is additive (subtree merge).
- Future mods: add another folder under `mods/` (subtree or vendor) and list it in the parent README table.

### Next steps
- Merge draft PR when ready.
- Optional: document Steam/local Mods symlink conventions for multi-mod installs.
- Keep pulling Ark via `git subtree pull --prefix=mods/the-ark-rimworld` as upstream advances.
