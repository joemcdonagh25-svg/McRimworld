# Vampire Lord — Changelog

Newest entries first.

---

## 2026-10-01 — M1 Wave Director V0 (IMPLEMENTATION COMPLETE)

### What shipped
- New RimWorld 1.6 mod **Vampire Lord** under `mods/vampire-lord/` in McRimworld.
- Wave Director V0: campaign `GameComponent`, absolute-tick scheduling (1-day warning, 3-day gap), advance-warning letters, five archetypes via vanilla `RaidEnemy`, threat/raid-point escalation, Dev Mode debug actions, save/load scribe fields.
- Docs: `docs/MILESTONES.md`, `docs/RIMWORLD_API_NOTES.md`, this CHANGELOG.
- Built `Assemblies/VampireLord.dll` (0 errors) against `Krafs.Rimworld.Ref` 1.6.4871 in Cloud; csproj also accepts local Steam Managed path.

### What we learned
- Prefer `VampireLord*` types for mod identity; keep gothic keep letter flavour (Torchlight / Black Keep place fantasy) without renaming the product to Black Keep.
- `GenDate` lives in `RimWorld`, not `Verse`.
- `RaidStrategyDefOf` only exposes a few strategies; Breaching/Siege/Smart need `DefDatabase<RaidStrategyDef>.GetNamedSilentFail`.
- Fire waves cannot force Molotov kits through `IncidentParms` alone — documented limitation.

### Key paths
- `mods/vampire-lord/About/About.xml` — packageId `joemcdonagh.vampirelord`
- `mods/vampire-lord/Source/VampireLord/`
- `mods/vampire-lord/Assemblies/VampireLord.dll`
- `mods/vampire-lord/docs/`

### Operator notes
- Campaign does **not** auto-enable. Use Dev Mode → Vampire Lord → **Start Vampire Lord Campaign**.
- Junction/copy `mods/vampire-lord` into RimWorld `Mods` (sibling to The Ark).
- Build: `dotnet build ./mods/vampire-lord/Source/VampireLord/VampireLord.csproj`

### Next steps
- RUNTIME VERIFIED pass in RimWorld (save/load + letter + raid + strategy defNames).
- Do not start M2 until Joe asks.
