# Feature: Prebuilt Lantern Village Hamlet

Status: IMPLEMENTATION COMPLETE — COMPILE VERIFIED — RUNTIME VERIFICATION PENDING (no RimWorld on this Cloud agent)

## Objective

When a new **The Lantern Village** colony map generates, spawn a fully constructed, functional, organic medieval woodland hamlet (~40×45). Buildings must exist as real things (no blueprints). Do not rebuild on save load.

## Existing State

- `GenStep_LanternHamlet` exists but is **never run**: `GenStepDef` is not patched into any `MapGeneratorDef.genSteps`.
- Even if it ran, it is gated by `HamletModeState.HamletModeRequested` (defaults false / Founding Mode).
- Existing builder is minimal (hall + 2 cottages + zones) and lacks roofs, kitchen, smithy, barn, palisade, watchpost, gardens-with-plants.
- Vampire Lord pattern: `ScenPart.GenerateIntoMap` + `ThingMaker`/`GenSpawn`/`ClearCell` — proven, once-per-new-map, not on load.
- Scenario already includes `ScenPart_LanternVillageSetup`.

## Architecture

- Authoritative build: `LanternHamletBuilder.TryBuild(map)` called from `ScenPart_LanternVillageSetup.GenerateIntoMap`.
- Always runs for The Lantern Village scenario (prebuilt hamlet is essential).
- Once-per-map via `map.uniqueID` guard; save load does not call `GenerateIntoMap`.
- Def resolution at runtime with medieval-mod preference and vanilla fallbacks.
- GenStep retained as thin delegate to the same builder (harmless if MapGenerator never references it).

## Files

- Create: `LanternHamletBuilder.cs`, `LanternHamletDefs.cs`
- Modify: `ScenPart_LanternVillageSetup.cs`, `GenStep_LanternHamlet.cs`, `ScenPartDefs.xml`, scenario `.rsc` order, `LanternVillage.csproj`
- Untouched: installer scripts, Horror Director content, sibling mods

## Lifecycle / Persistence

- New map only via ScenPart `GenerateIntoMap`.
- Buildings persist as normal map things; no Scribe for layout.
- No reconstruction on `LoadedGame`.

## Invariants

- Scenario-gated only.
- No blueprints/designations.
- Stay in bounds; clear plants/rocks; avoid water/impassable.
- Partial palisade, not fortress/killbox.
- Organic offsets, not a rigid grid.

## Verification

- Compile against Krafs.Rimworld.Ref / Steam Managed.
- Runtime: new Lantern Village game shows constructed hamlet; save/reload does not duplicate.
