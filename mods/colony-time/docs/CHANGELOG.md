# Colony Time — Changelog

Newest entries first.

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
