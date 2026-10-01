# McRimworld

Parent repository for Joe’s RimWorld mods. Each playable mod lives under `mods/` and keeps its own identity, docs, and history.

## Layout

```
McRimworld/
  README.md
  CHANGELOG.md
  mods/
    the-ark-rimworld/   # The Ark (git subtree from joemcdonagh25-svg/the-ark-rimworld)
    vampire-lord/       # Vampire Lord (M1 Wave Director)
```

## Mods

| Folder | Package | Status |
|--------|---------|--------|
| `mods/the-ark-rimworld/` | `joemcdonagh.theark` | RimWorld 1.6 — M1 + M2 debug UI + New Game scenario; runtime verify pending (fixture save/load) |
| `mods/vampire-lord/` | `joemcdonagh.vampirelord` | RimWorld 1.6 — M1 + M2 RUNTIME VERIFIED; M3 Blood Tithe implemented (runtime pending) |

### The Ark

Nomadic gravship total conversion: the ship is the colony. Land. Explore. Salvage. Survive. Escape.

- Upstream: https://github.com/joemcdonagh25-svg/the-ark-rimworld
- Imported via **git subtree** (full Ark history preserved under this prefix)
- Design / milestones: `mods/the-ark-rimworld/docs/`
- Junction/copy **`mods/the-ark-rimworld/`** (the folder with `About/` **and** `Defs/`) into RimWorld `Mods` — same pattern as Vampire Lord
- Windows helper: run `mods/the-ark-rimworld/tools/junction-the-ark.ps1` in PowerShell
- Enable **The Ark** in the Mods list (packageId `joemcdonagh.theark`). Disable any older standalone The Ark copy.
- Restart RimWorld. Player.log must contain: `ScenarioDef TheArk_Playtest LOADED` and `ScenarioLister CONTAINS The Ark`
- If you see `MISSING` / no `[The Ark] Initialised`: the mod is not the McRimworld folder (or not enabled)
- Then **New Game → The Ark**
- V1.1 playtest is a surface start (Odyssey not required to see the scenario)

### Vampire Lord

Gothic keep tower-defence campaign. Defend the keep. Read the warning. Prepare the walls. Survive the wave.

- Package: `joemcdonagh.vampirelord`
- Requires **Biotech**
- **M1:** Wave Director V0 — RUNTIME VERIFIED
- **M2:** Playtest Keep — New Game → **Vampire Lord** (1 lord + 2 thralls, courtyard, open gate, map reveal, auto campaign) — RUNTIME VERIFIED
- **M3:** Blood Tithe V0 — IMPLEMENTATION COMPLETE (keep blood reserve, wave tithe cost); runtime verify pending
- Design / milestones: `mods/vampire-lord/docs/`
- Junction/copy `mods/vampire-lord/` into your RimWorld `Mods` folder (the folder that contains `About/` is the mod root)

## Working on a mod

- Prefer editing inside `mods/<mod-id>/` in this parent when coordinating multi-mod work.
- To pull latest Ark into the parent (from McRimworld root):

  ```bash
  git remote add ark https://github.com/joemcdonagh25-svg/the-ark-rimworld.git   # once
  git fetch ark
  git subtree pull --prefix=mods/the-ark-rimworld ark main
  ```

- To push parent commits that only touch Ark back upstream (optional):

  ```bash
  git subtree push --prefix=mods/the-ark-rimworld ark main
  ```

- Never force-push either repo; preserve history.

## Build

```bash
dotnet build ./mods/the-ark-rimworld/Source/TheArk/TheArk.csproj
dotnet build ./mods/vampire-lord/Source/VampireLord/VampireLord.csproj
```

Point RimWorld at the mod folder under `mods/` (or copy/symlink/junction into your RimWorld `Mods` folder) for in-game testing.
