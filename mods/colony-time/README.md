# Colony Time

Shows real playtime and colony age for each save on RimWorld’s Load Game screen.

## Status

V0 — IMPLEMENTATION COMPLETE (runtime verification pending on a live RimWorld install)

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/workshop/filedetails/?id=2009463077) (`brrainz.harmony`)

## Install

Junction or copy `mods/colony-time/` (the folder that contains `About/`) into your RimWorld `Mods` folder.

Enable **Harmony**, then **Colony Time**.

## What you should see

On **Load Game**, each save row shows:

- `Played: …` from `realPlayTimeInteracting` (seconds → hours/minutes)
- `Colony: …` from `ticksGame` (60,000 ticks/day, 60 days/year)

Sort menu: Last played (default), Hours played, Colony age, Name.

## Safety

Read-only. Does not rewrite `.rws` files, inject XML, or save any Colony Time state into a colony. Safe to add or remove.

## Build

```bash
dotnet build ./mods/colony-time/Source/ColonyTime/ColonyTime.csproj
dotnet test ./mods/colony-time/Source/ColonyTime.Tests/ColonyTime.Tests.csproj
```

## Docs

- Plan: `docs/implementation/COLONY_TIME_V0_PLAN.md`
- API notes: `docs/RIMWORLD_API_NOTES.md`
- Changelog: `docs/CHANGELOG.md`
