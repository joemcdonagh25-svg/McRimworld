# Colony Time V0

Status: IMPLEMENTATION COMPLETE (runtime verification pending)

## Objective

Show real-world playtime (`realPlayTimeInteracting`) and colony age (`ticksGame`) on RimWorld’s vanilla Load Game screen, with optional sort controls. Read-only utility/UI mod only.

## Existing Architecture

Parent repo `McRimworld` hosts mods under `mods/`. Existing mods target RimWorld **1.6** via Steam Managed path or `Krafs.Rimworld.Ref` **1.6.4871**. No prior Colony Time code. Sibling mods do not use Harmony; this mod will.

### Inspected RimWorld 1.6.4871 targets

| Item | Finding |
|------|---------|
| Version | RimWorld 1.6 (ref package 1.6.4871 / assembly version 1.6.9676.x) |
| Load dialog | `RimWorld.Dialog_SaveFileList_Load` : `Dialog_SaveFileList` : `Dialog_FileList` : `Window` |
| Row UI | `Dialog_FileList.DoWindowContents(Rect)` |
| Date/version UI | `Dialog_FileList.DrawDateAndVersion(SaveFileInfo, Rect)` |
| Row height | `Dialog_FileList.EntryHeight = 40f` (const, inlined) |
| File info width | `FileInfoWidth = 94f` |
| Save list reload | `Dialog_SaveFileList.ReloadFiles()` → async `ReloadFilesTask()` |
| Default order | `GenFilePaths.AllSavedGameFiles` orders by `LastWriteTime` descending |
| Playtime field | `Verse.GameInfo.realPlayTimeInteracting` (`float`, seconds via `RealTime.realDeltaTime`) |
| Colony age field | `Verse.TickManager` exposes `ticksGameInt` as Scribe label `"ticksGame"` |
| Constants | `RimWorld.GenDate.TicksPerDay = 60000`, `DaysPerYear = 60` |

## Required Changes

1. Forward-only XML metadata reader for `.rws`
2. Identity cache (path + LastWriteTimeUtc + size)
3. Formatters for playtime / colony age
4. Narrow Harmony integration on Load Game UI
5. In-memory sort mode (not persisted in V0)
6. About.xml + Harmony external dependency

## Files

Create:

- `mods/colony-time/About/About.xml`
- `mods/colony-time/Source/ColonyTime/*` (mod, reader, cache, formatter, patches)
- `mods/colony-time/Source/ColonyTime.Tests/*` (formatter/reader unit tests)
- `mods/colony-time/docs/*`
- Parent `README.md` / `CHANGELOG.md` entries

## State Ownership

- Authoritative playtime/age: values inside each `.rws` (vanilla)
- Cache: process-lifetime `SaveMetadataCache` only
- Sort mode: static in-memory on the patch class (session only)

No `GameComponent`, `WorldComponent`, `MapComponent`, or Scribe usage.

## Lifecycle Considerations

- Patches apply at mod construction
- Metadata read when Load Game draws rows / sorts
- No map/world/game lifecycle hooks
- Safe to add/remove mid-campaign (no colony state)

## Persistence

None. No Scribe labels. No settings file in V0.

## Invariants

- Never write `.rws`
- Never deserialize full saves into `XmlDocument`
- Parse failures degrade to `—` without breaking the dialog
- Unchanged files parse at most once per cache lifetime
- Default sort matches vanilla last-played ordering

## Failure Modes

- Corrupt/partial XML → null metadata, logged once per path+identity
- Missing Harmony → clear startup log, mod inert
- Other Load Game UI mods (e.g. RimSaves) may conflict; degrade safely
- Async `ReloadFilesTask` may briefly show unsorted list until files appear; re-sort each frame while Load dialog is open

## Architecture / Patch Design

1. **`DoWindowContents` Prefix** (`Dialog_FileList`): only when instance is `Dialog_SaveFileList_Load` (and optionally Save). Reserve top strip for sort control via `ref Rect inRect`. Re-sort `files` list for non-default modes.
2. **`DoWindowContents` Transpiler**: replace inlined `40f` entry height with `ColonyTimeUI.RowHeight` (58f) when current dialog is a save list; else 40f. Prefix sets `ColonyTimeUI.Current`.
3. **`DrawDateAndVersion` Prefix**: draw vanilla date/version plus `Played` / `Colony` lines in Tiny font inside a taller info column (skip original and redraw cleanly to avoid overlap).
4. **`ReloadFiles` Postfix** on `Dialog_SaveFileList`: prune cache entries for deleted saves when practical.

Private field access (`files`) isolated in `DialogFileListAccess`.

## Implementation Sequence

1. Scaffold mod + csproj + About.xml
2. Reader / cache / formatter + unit tests
3. Harmony patches
4. Docs + parent README/CHANGELOG
5. `dotnet build` + tests
6. Adversarial review / fix
7. Runtime verification on Joe’s machine (this cloud env has no RimWorld)

## Verification

- Static: architecture matches plan; no write paths
- Compile: `dotnet build` + unit tests
- Runtime: Joe launches RimWorld with Harmony + Colony Time (see acceptance checklist)

## Out of Scope

Campaign grouping, Steam/cloud saves, thumbnails, async parsing, settings persistence, Save-dialog-only features beyond shared base class safety.