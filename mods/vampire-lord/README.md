# Vampire Lord

Gothic keep tower-defence campaign for RimWorld 1.6.

**M1 status:** IMPLEMENTATION COMPLETE — RUNTIME VERIFIED (Joe live session, 2026-10-01).

## What it does (M1)

After you start the campaign from Dev Mode, the Wave Director:

1. Schedules the next assault (default **3 days** out)
2. Sends an advance warning letter (**1 day** before)
3. Fires a vanilla enemy raid with an archetype (Mob / Hunters / Breachers / Fire / Siege)
4. Raises threat level and schedules the next wave

## Enable

1. Put this folder (`mods/vampire-lord`) in your RimWorld `Mods` directory (or junction it).
2. Enable **Vampire Lord** in the mods menu.
3. Load/start a game with Dev Mode on.
4. Open debug actions → category **Vampire Lord** → **Start Vampire Lord Campaign**.

## Build

```bash
dotnet build ./Source/VampireLord/VampireLord.csproj
```

On Windows with Steam RimWorld installed, the project uses the Managed `Assembly-CSharp.dll`. Elsewhere it falls back to NuGet `Krafs.Rimworld.Ref` 1.6.x.

## Docs

- `docs/MILESTONES.md` — M1 acceptance + status labels
- `docs/RIMWORLD_API_NOTES.md` — inspected APIs and limitations
- `docs/CHANGELOG.md` — newest-first shipping notes

## Sibling mod

Lives beside **The Ark** in the McRimworld parent repo under `mods/`.
