# McRimworld — Changelog

Newest entries first. Records what shipped, what we learned, key paths, operator notes, and next steps.

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
- Merge draft PR when ready.
- Joe runtime verify in RimWorld (see `mods/vampire-lord/docs/MILESTONES.md`).
- Do not start Vampire Lord M2 until Joe asks.

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
