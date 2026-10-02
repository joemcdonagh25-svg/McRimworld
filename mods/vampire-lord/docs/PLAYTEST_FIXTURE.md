# Vampire Lord — Fixture-save playtest workflow

**Goal:** Stop doing New Game for every DLL change. Load `VL_prep`, poke debug actions, iterate.

---

## Once (create the fixture)

1. Pull latest Vampire Lord + restart RimWorld.
2. **New Game → Vampire Lord** → finish setup; let the keep load (campaign auto-starts).
3. Dev Mode → **Vampire Lord** → **Ensure Prep Fixture**  
   (blood topped up, prep window open, player home ensured, fortify offer scheduled / auto-applied)
4. Optional: **Toggle Auto-Fortify Playtest** → ON  
   (sandbags place themselves each prep window — no Accept letter)
5. **Save** as `VL_prep` (or any name you will reuse).

Dev Mode → **Print Fixture Workflow** dumps this checklist into the log anytime.

---

## Each code change

1. Pull / rebuild `Assemblies/VampireLord.dll` → **restart RimWorld** (C# requires restart).
2. **Load `VL_prep`** — do not New Game.
3. Hit the debug action you care about:

| Want | Debug action |
|------|----------------|
| Place sandbags now | **Force Fortify Now** |
| Auto sandbags each prep | **Toggle Auto-Fortify Playtest** |
| Reset prep + blood | **Ensure Prep Fixture** (then re-save `VL_prep` if you want) |
| Fire a wave | **Trigger Wave Now** |
| Advance warning only | **Trigger Warning Now** |
| Blood inspect / tweak | **Show / Add / Spend / Force Low Blood** |
| State dump | **Show Campaign State** / **Show Fortify State** |

4. If the save drifts (wrong window, low blood): **Ensure Prep Fixture** → save over `VL_prep`.

---

## Notes

- Fortify offer is a **letter stack** item (`Blood for the Walls`), not a Quests-tab quest. Auto-Fortify skips it.
- Auto-Fortify is a **playtest flag** stored on the campaign (survives save/load). Leave it OFF for “real” Accept-letter checks.
- New Game is only needed for settle / scenario / Ideology path tests.
