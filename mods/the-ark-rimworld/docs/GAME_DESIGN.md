# The Ark — Game Design

**Status:** Design authority for THE ARK (RimWorld 1.6 total conversion).  
**Scope:** Product fantasy, pillars, and systems intent. Not an implementation plan.  
**Implementation authority:** See `ARCHITECTURE.md`, `DEVELOPMENT_RULES.md`, and `MILESTONES.md`.

---

## One-line fantasy

The gravship is the colony. You land, take what you need, and keep moving — or Pursuit finds you.

---

## Core fantasy

THE ARK is a **nomadic gravship campaign**. The player does not establish a conventional permanent surface settlement. The ship is the only lasting home; the surface is a series of temporary expeditions.

### Campaign loop

```
LAND
→ establish expedition
→ identify objective
→ explore
→ salvage
→ complete objective
→ decide whether to remain
→ escalating pursuit
→ evacuate
→ upgrade Ark
→ select destination
→ LAND
```

### Tonal touchstones (inspiration only)

- Battlestar Galactica
- Firefly
- Homeworld
- Alien

These are **tonal references only**. Do not copy protected characters, terminology, art, or narrative content.

---

## Design pillars

### 1. The ship is the colony

The gravship is the player's only permanent home. Surface infrastructure is temporary. Permanent surface optimisation must never become the dominant winning strategy.

### 2. Time is a resource

Remaining on a landed map increases **Pursuit**. The player constantly balances further opportunity against rising danger.

### 3. Every landing has purpose

Each major landing should contain a reason for being there. Examples:

- salvage
- navigation data
- rescue
- rare technology
- recruitment
- resources
- investigation

### 4. People are roles

Crew stay small enough that individual specialists matter.

| Phase | Target crew size |
|-------|------------------|
| Early | 3–5 |
| Midgame | 6–9 |
| Late | 10–14 |
| Hard maximum | 16 human crew |

Recruitment should primarily **fill capability gaps**, not simply inflate population.

### 5. Salvage drives progression

The campaign discourages conventional long-term resource-industrial expansion on the surface. Important progression comes from expeditions, salvage, quests, discoveries, and trade.

### 6. The Ark physically records progression

Ship capability is a major progression system. Ark **tiers** are the visible backbone of how far the campaign has come (see below).

### 7. The player must keep moving

The game must create systemic reasons to leave. Evacuation under pressure is intended; farming endless raids at max Pursuit is not.

---

## Pursuit

Pursuit is a core campaign pressure system. Conceptual states:

| Range | State |
|-------|--------|
| 0–20 | QUIET |
| 21–40 | NOTICED |
| 41–60 | HUNTED |
| 61–80 | BESIEGED |
| 81–100 | HARBINGER |

### Intent

- **Initial driver:** primarily time since landing.
- **Later possible inputs:** wealth, crew count, Ark tier, player actions, expedition events, campaign events.
- **At maximum Pursuit:** the intended response is generally **evacuation**, not conventional raid farming.
- **Harbinger:** must eventually become more interesting than simply increasing raid points.

**Do not implement Pursuit until its milestone.** Keep early versions deliberately simple.

---

## Crew

### Starting crew concept (design roles)

These are **design roles**, not necessarily custom RimWorld pawn classes. Prefer existing RimWorld systems (backstories, skills, traits, genes, xenotypes) wherever practical.

| Role | Primary | Secondary |
|------|---------|-----------|
| Captain | Social | Shooting / Intellectual |
| Engineer | Construction | Crafting / Intellectual |
| Survivalist | Medical | Plants / Cooking |

### Potential recruitment roles

- Marine
- Scientist
- Quartermaster
- Mechanitor
- Doctor
- Scout
- Gene specialist

---

## Ark progression tiers

| Tier | Name | Intent |
|------|------|--------|
| 0 | WRECK | Damaged and grounded. Initial objective: restore flight capability. |
| 1 | SURVIVOR | Minimal viable mobile home. Approx. 4–5 crew capability. |
| 2 | EXPLORER | Player begins selecting specialist ship capabilities. Cannot support every module at once. |
| 3 | FRIGATE | Approx. 8–10 crew. Redundant systems and specialist facilities. |
| 4 | ARK | Approx. 12–16 crew. Highly capable but **not** completely self-sufficient. The outside world must remain relevant. |

### Explorer-era module examples

- Medical
- Workshop
- Laboratory
- Armoury
- Crew quarters
- Cargo
- Drone bay
- Hydroponics

Module sets are a **progression choice**, not a free checklist.

---

## Expeditions

Landings should eventually support:

- primary objective
- optional objectives
- biome / location context
- rewards
- Pursuit state
- evacuation decision

### Example (design illustration only)

**EXPEDITION 04**

- **Biome:** Temperate ruins  
- **Primary:** Recover navigation core  
- **Optional:** Rescue survivor; recover persona weapon; investigate ancient complex  
- **Pursuit:** QUIET  

**Do not implement expeditions until their milestone.**

---

## Campaign state (conceptual)

A persistent campaign concept might eventually include fields such as:

- CampaignActive
- ArkTier
- CampaignDay
- LandingNumber
- PursuitLevel
- CrewRoster
- DiscoveredLocations
- RecoveredArtifacts
- UnlockedModules
- StarMapFragments
- CampaignFlags

This list is **conceptual**. Do not treat it as a mandatory data model. When implementation begins, choose the correct RimWorld save / persistence architecture first. Campaign state must have **one clear authoritative owner** (see `ARCHITECTURE.md`).

---

## Non-goals (design)

- Recreating RimWorld's health, inventory, crafting, or combat systems from scratch.
- A permanent surface megabase as the primary play mode.
- Copying protected IP from inspirational media.
- Implementing the full campaign fantasy in a single milestone.

---

## Related documents

- `ARCHITECTURE.md` — system boundaries and technical philosophy  
- `DEVELOPMENT_RULES.md` — how engineers and AI agents may change the codebase  
- `MILESTONES.md` — sequenced vertical slices  
- `RIMWORLD_API_NOTES.md` — verified API findings only  
