# Vampire Lord

Gothic keep tower-defence campaign for RimWorld 1.6.

**M1 status:** IMPLEMENTATION COMPLETE — RUNTIME VERIFIED (Joe live session, 2026-10-01).  
**M2 status:** IMPLEMENTATION COMPLETE — RUNTIME VERIFIED (Joe live session, 2026-10-01).  
**M3 status:** IMPLEMENTATION COMPLETE — RUNTIME VERIFIED pending (Blood Tithe V0).  
**M4 status:** IMPLEMENTATION COMPLETE — RUNTIME VERIFIED pending (Keep Fortification V0).  
**M5 status:** IMPLEMENTATION COMPLETE — RUNTIME VERIFIED pending (Campaign UI HUD).

Requires **Biotech** (Sanguophage Vampire Lord).

## What it does

### M1 — Wave Director
After the campaign starts, the Wave Director:

1. Schedules the next assault (default **3 days** out)
2. Sends an advance warning letter (**1 day** before)
3. Fires a vanilla enemy raid with an archetype (Mob / Hunters / Breachers / Fire / Siege)
4. Raises threat level and schedules the next wave

### M2 — Playtest Keep
**New Game → Vampire Lord** scenario:

- 1 Vampire Lord (Sanguophage) + 2 thralls (Baseliner)
- Simple granite walled courtyard + open south gate at player start
- Outside map revealed (not a fog void)
- Starter food, medicine, materials, rifles, masterwork longsword, hemogen packs
- Wave Director **auto-starts** (no Dev Mode required)

### M3 — Blood Tithe V0
- Keep Blood Reserve (starts at 40 when campaign activates)
- Fresh hostile humanlike kills feed the reserve (+5)
- Each wave costs blood (`10 + 2×threat`); unpaid → harder raid (×1.35)
- Letters + Dev Mode **Show / Add / Spend / Force Low Blood**

### M4 — Keep Fortification V0
- Spend **15 blood** between waves on sandbags at the south gate (Accept letter)
- 1 package per prep window; blocked after the advance warning
- Dev Mode: **Offer Fortify**, **Force Fortify Now**, **Show Fortify State**

### M5 — Campaign UI V0
- Always-on bottom-left HUD: Blood, tithe due, Wave, Threat, prep/inbound ETA
- Default ON; Dev Mode **Toggle Campaign HUD**

## Enable

1. Junction/copy this folder (`mods/vampire-lord`) into your RimWorld `Mods` directory.
2. Enable **Biotech** and **Vampire Lord**.
3. **New Game → Vampire Lord** (preferred playtest path).

### Debug (optional)
Dev Mode → category **Vampire Lord** → Start / Stop / Trigger Warning / Trigger Wave / Show Campaign State / Blood Tithe / Fortify tools.

**Fast loop (defaults ON):** AutoFortify + 1-day wave pace + quiet letters. See `docs/PLAYTEST_FIXTURE.md` — save `VL_prep`, load it, **Trigger Wave**.

## Build

```bash
dotnet build ./Source/VampireLord/VampireLord.csproj
```

On Windows with Steam RimWorld installed, the project uses the Managed `Assembly-CSharp.dll`. Elsewhere it falls back to NuGet `Krafs.Rimworld.Ref` 1.6.x.

## Docs

- `docs/MILESTONES.md` — M1/M2 acceptance + status labels
- `docs/RIMWORLD_API_NOTES.md` — inspected APIs and limitations
- `docs/CHANGELOG.md` — newest-first shipping notes

## Sibling mod

Lives beside **The Ark** in the McRimworld parent repo under `mods/`.
