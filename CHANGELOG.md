# McRimworld — Changelog

Newest entries first. Records what shipped, what we learned, key paths, operator notes, and next steps.

---

## 2026-10-01 — The Ark scenario visibility (diagnostic + vanilla parts)

### What shipped
- The Ark playtest scenario uses vanilla ScenParts only; campaign activate stays in GameComponent.
- Bootstrap logs whether `TheArk_Playtest` ScenarioDef loaded (Player.log checklist).
- Branch: `cursor/ark-scenario-visible-5195` → draft PR into `main`.

### What we learned
- If the mod is not the McRimworld `mods/the-ark-rimworld` root (or a duplicate packageId shadows it), New Game never sees the scenario.

### Key paths
- `mods/the-ark-rimworld/Defs/Scenarios/TheArkScenario.xml`
- `mods/the-ark-rimworld/Source/TheArk/TheArkBootstrap.cs`
- `README.md`

### Operator notes
- Pull → junction `mods/the-ark-rimworld` → enable The Ark → restart → confirm `TheArk_Playtest LOADED` in Player.log.

### Next steps
- Merge; Joe confirm scenario appears.

---

<<<<<<< HEAD
## 2026-10-01 — The Ark scenario list fix

### What shipped
- Soften Odyssey dependency so New Game → **The Ark** can appear without Odyssey enabled.
- Backup campaign activate on `StartedNewGame` when scenario name is The Ark.
- Branch: `cursor/ark-scenario-list-fix-5195` → draft PR into `main`.

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
- Merge; Joe confirm scenario visible; then fixture save/load proof.
=======
## 2026-10-01 — Vampire Lord M2 outside access / full-map reveal

### What shipped
- Open south gate, **full-map unfog** (PostMapGenerate + PostGameStart), PackedDirt approach lane, ideo/colonist safety; DLL rebuild.
- Branch: `cursor/vampire-lord-m2-outside-access-c5ad`.
- Joe’s pasted log was from the ownership DLL (`gate` text, no reveal line) — that build never cleared fog outside the keep.

### Operator notes
- After merge: pull `C:\McRimworld`, restart, New Game → Vampire Lord. Look for `open gate` + `Revealed map` in the log, then walk south.

### Next steps
- Merge; Joe runtime check of outside terrain + pathing.
>>>>>>> origin/main

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
