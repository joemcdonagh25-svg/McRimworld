# Vampire Lord — Less-fussy playtest + DebugActions

**Goal:** Load once, poke debug, iterate. Minimal letters. Minimal New Games.

## Defaults (ON at campaign start)

| Flag | Effect |
|------|--------|
| **AutoFortify** | Sandbags place themselves each prep window — no Accept letter |
| **Playtest Pace** | ~1 day between waves, ~0.25 day warning (not 3+1) |
| **Quiet Letters** | Skip harvest + fortify-complete spam; keep warning + tithe |

---

## Quicktest + `VL:` DebugActions

Vampire Lord ships a **developer-only** DebugAction suite (no gameplay UI).

### Workflow

```text
Build → Launch RimWorld → Quicktest (or load VL_prep)
→ Dev Mode → Debug Actions → search "VL:"
→ Create combat/campaign state → Test → RESET → Repeat
```

### Production systems reused

| Action family | Calls |
|---------------|-------|
| Waves | `VampireLordWaveDirector.TriggerWaveNow` / `RaidLauncher` |
| Keep Blood | `VampireLordBloodTithe` |
| Fortify | `VampireLordFortify` |
| Prep fixture | `VampireLordPlaytest.EnsurePrepFixture` |
| Home | `VampireLordPlayerHome.TryEnsure` |
| Pawn hemogen | Biotech `Gene_Hemogen.Value` |
| Time | `TickManager.DebugSetTicksGame` + `GenLocalDate.HourOfDay` |
| Keep repair/damage | Courtyard walls/sandbags via `VampireLordKeepLayout` |

### Presets (test fixtures — not balance)

| Preset | Intent |
|--------|--------|
| **Fresh Lord** | Campaign on, night, prep window, low threat, healthy keep |
| **Early Siege** | Night + small production wave |
| **Mid Siege** | Mid threat + medium wave + keep ~50% HP |
| **Last Stand** | Midnight + high threat + heavy wave + thin blood cushion |
| **Near Defeat** | Empty keep blood + empty hemogen + damaged keep + heavy wave |

### Save/load check

1. Apply **VL: Preset — Last Stand**
2. **VL: Log Save Test Snapshot** — copy the line
3. Save → menu → load
4. **VL: Log Save Test Snapshot** — compare

Prefix: `[VampireLord SAVE TEST]`

### Temporary / scoped behaviour

- **Wave enemies:** VL does not tag raid pawns. Kill/Remove operate on **hostile humanlikes** on the map.
- **End Current Wave:** VL completes wave bookkeeping on raid **launch** (`OnWaveDispatched`). End = kill hostiles (no separate victory rewards).
- **Sized waves:** set `ThreatLevel` then call production `TriggerWaveNow` (not a duplicate raid generator).
- **Refill Defences:** repair keep walls/sandbags; optional Force Fortify if in prep window. No turret-ammo system yet.

### Omitted (systems not in V0)

- Make Selected Pawn Vampire Lord (no lord-designation beyond Sanguophage xenotype)
- Progression tiers / victory / defeat triggers
- Max Threat (no production max — use **Set Threat**)
- Formal castle-core HP meter (repair/damage uses courtyard buildings only)

---

## Once (fixture save)

1. Pull + restart RimWorld.
2. **New Game → Vampire Lord** → finish setup; keep loads; campaign auto-starts.
3. Optional: **VL: Ensure Prep Fixture**.
4. **Save** as `VL_prep`.

---

## Each code change

1. Pull / rebuild DLL → **restart RimWorld**.
2. **Load `VL_prep`** (or Quicktest).
3. Search **`VL:`** — use presets / Trigger Wave / Force Fortify as needed.

New Game only for settle / Ideology / scenario path tests.
