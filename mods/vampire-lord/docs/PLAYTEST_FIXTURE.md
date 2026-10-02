# Vampire Lord — Less-fussy playtest

**Goal:** Load once, poke debug, iterate. Minimal letters. Minimal New Games.

## Defaults (ON at campaign start)

| Flag | Effect |
|------|--------|
| **AutoFortify** | Sandbags place themselves each prep window — no Accept letter |
| **Playtest Pace** | ~1 day between waves, ~0.25 day warning (not 3+1) |
| **Quiet Letters** | Skip harvest + fortify-complete spam; keep warning + tithe |

Toggle any of these off in Dev Mode if you want the “full” UI.

Game start dialog is removed from the scenario (one less click).

---

## Once

1. Pull + restart RimWorld.
2. **New Game → Vampire Lord** → finish setup; keep loads; campaign auto-starts.
3. Optional: Dev Mode → **Ensure Prep Fixture** (resets prep + reapplies defaults).
4. **Save** as `VL_prep`.

---

## Each code change

1. Pull / rebuild DLL → **restart RimWorld**.
2. **Load `VL_prep`**.
3. Use:

| Want | Action |
|------|--------|
| Next raid now | **Trigger Wave Now** |
| Warning only | **Trigger Warning Now** |
| Sandbags now | **Force Fortify Now** (or wait — AutoFortify does it) |
| Reset prep | **Ensure Prep Fixture** → re-save `VL_prep` if needed |
| Inspect | **Show Campaign State** / **Print Fixture Workflow** |

New Game only for settle / Ideology / scenario path tests.
