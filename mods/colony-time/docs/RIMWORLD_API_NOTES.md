# Colony Time — RimWorld API Notes

## 2026-10-05 — Load Game dialog targets (1.6.4871)

### Goal
Document real Harmony targets before patching.

### Inspected
- Package: `Krafs.Rimworld.Ref` 1.6.4871 (`Assembly-CSharp` 1.6.9676.x)
- Older public decompile (`josh-m/RW-Decompile`) for behavioural context; 1.6 adds `QuickSearchWidget`, async `ReloadFilesTask`, class-based `SaveFileInfo`

### Types / methods

| Role | Type / member |
|------|----------------|
| Load dialog | `RimWorld.Dialog_SaveFileList_Load` |
| Shared save list | `RimWorld.Dialog_SaveFileList` |
| File list UI | `RimWorld.Dialog_FileList` |
| Row draw | `Dialog_FileList.DoWindowContents(Rect)` |
| Date/version | `Dialog_FileList.DrawDateAndVersion(SaveFileInfo, Rect)` |
| Reload | `Dialog_SaveFileList.ReloadFiles()` / `ReloadFilesTask()` |
| Entry model | `Verse.SaveFileInfo` |
| Default order | `GenFilePaths.AllSavedGameFiles` → `LastWriteTime` descending |
| Playtime | `Verse.GameInfo.realPlayTimeInteracting` (`float` seconds) |
| Colony ticks | `TickManager` Scribe `"ticksGame"` |
| Day/year | `RimWorld.GenDate.TicksPerDay = 60000`, `DaysPerYear = 60` |

### Decision for Colony Time
- Patch `DoWindowContents` (sort bar + entry-height transpiler) and `DrawDateAndVersion` (played/colony lines).
- Do not replace the whole Load Game window.
- Harmony is an external mod dependency (`brrainz.harmony`); do not bundle `0Harmony.dll`.

### XML fields read
```xml
<realPlayTimeInteracting>…</realPlayTimeInteracting>
<ticksGame>…</ticksGame>
```
Forward-only `XmlReader`; stop after both found.
