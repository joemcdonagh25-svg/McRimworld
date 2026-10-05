# Colony Time — Changelog

Newest entries first.

---

## 2026-10-05 — Load Game label readability

### What shipped
- Taller save rows (72px), wider meta column, brighter Played/Colony color, Small font for those two lines.

### Operator notes
- `git pull` on `cursor/colony-time-v0-25e9` (junction already points at the folder) → restart RimWorld → open Load Game.

---

## 2026-10-05 — Install helper (mod list visibility)

### What shipped
- `tools/junction-colony-time.ps1` — same Mods-junction pattern as The Ark / Vampire Lord.
- README install steps: checkout PR branch, then junction `mods/colony-time` into RimWorld `Mods`.

### What we learned
- Mod missing from the in-game list is almost always “not on disk under RimWorld\Mods” (wrong branch and/or no junction), not a bad About.xml.

### Operator notes
1. `git checkout cursor/colony-time-v0-25e9`
2. Run `mods\colony-time\tools\junction-colony-time.ps1`
3. Confirm `Mods\colony-time\About\About.xml` exists → restart → enable Harmony + Colony Time

---

## 2026-10-05 — V0 Load Game playtime + colony age

### What shipped
- Read-only Load Game extras: `Played` / `Colony` per save from forward-only `.rws` parsing.
- In-memory cache keyed by path + last write UTC + file size.
- Sort control: Last played (default), Hours played, Colony age, Name.
- Narrow Harmony patches on `Dialog_FileList` / `Dialog_SaveFileList` (RimWorld 1.6).
- Unit tests for formatter, reader, and cache.

### What we learned
- `GameInfo.realPlayTimeInteracting` is seconds (`RealTime.realDeltaTime` accumulation).
- `TickManager` Scribe label is `ticksGame` (field `ticksGameInt`).
- Load list UI lives on `Dialog_FileList.DoWindowContents` + `DrawDateAndVersion`; save reload is `Dialog_SaveFileList.ReloadFiles` (async task in 1.6).
- `EntryHeight` is a 40f const inlined into IL — row growth uses a DoWindowContents transpiler.

### Key paths
- `mods/colony-time/About/About.xml`
- `mods/colony-time/Source/ColonyTime/`
- `mods/colony-time/Assemblies/ColonyTime.dll`

### Operator notes
- Enable Harmony first, then Colony Time. Restart RimWorld. Open Load Game.
- Sort choice is session-only (not saved) in V0.

### Next steps
- Runtime verify on Joe’s machine (20+ saves, load/delete, confirm no `.rws` writes).
- Do not start campaign grouping until asked.
