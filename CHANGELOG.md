# McRimworld — Changelog

Newest entries first. Records what shipped, what we learned, key paths, operator notes, and next steps.

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
