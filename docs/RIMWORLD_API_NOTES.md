# The Ark — RimWorld API Notes

**Purpose:** Verified findings about RimWorld / Odyssey / Verse APIs that this mod may use.  
**Rule:** Only record APIs that were inspected from referenced assemblies or other reliable evidence.  
**Ban:** Speculative “this probably exists” entries presented as fact.

When you discover a useful API, add a dated entry below **before** writing integration code against it.

---

## How to add an entry

Use this template:

```markdown
### YYYY-MM-DD — Short title

- **Goal:** What we needed to know
- **Evidence:** Assembly / type inspection method (e.g. referenced Assembly-CSharp, dnSpy, public docs)
- **API:** Namespace, type, member signatures actually observed
- **Behaviour notes:** What it does / lifecycle caveats
- **Decision for The Ark:** How we will use it (or why we will not)
- **Related milestone:** e.g. M1, M3
```

---

## Environment (verified foundation)

| Item | Value |
|------|--------|
| RimWorld target | 1.6 |
| Mod packageId | `joemcdonagh.theark` |
| Game assembly reference | Local `Assembly-CSharp.dll` via `TheArk.csproj` (`Private=false`) |
| Mod startup hook in use | `Verse.StaticConstructorOnStartup` on `TheArk.TheArkBootstrap` |
| Logging in use | `Verse.Log.Message` |

---

## Verified API entries

### 2026-09-30 — Static startup bootstrap

- **Goal:** Confirm a minimal, reliable mod initialisation signal.
- **Evidence:** Working project code in `Source/TheArk/TheArkBootstrap.cs` compiling against referenced `Assembly-CSharp.dll`; observed in-game log line.
- **API:**
  - Attribute: `Verse.StaticConstructorOnStartup`
  - Logging: `Verse.Log.Message(string)`
- **Behaviour notes:** Static constructor runs on startup when the assembly loads types marked with the attribute. Used as the M0 success signal.
- **Decision for The Ark:** Keep bootstrap intact as regression canary. Additional systems may initialise here or via later verified game/world hooks — do not remove the clear success log without an equal replacement.
- **Related milestone:** M0 (COMPLETE)

---

## Pending investigations (not yet verified)

These are **questions**, not APIs. Do not implement against assumed answers.

| Topic | Needed by | Question |
|-------|-----------|----------|
| Campaign persistence owner | M1 | Which Verse/RimWorld component type is the correct authoritative save owner for durable campaign fields? |
| Gravship landed state | M3 | How does Odyssey expose “gravship has landed on this map” in 1.6? |
| Gravship departure | M8 | What event / flag reliably signals departure for session teardown? |
| Incident hooks for Pursuit | M7 | Best supported path for one Pursuit-driven consequence without fighting vanilla raid logic? |
| Quest / objective scaffolding | M9 | Prefer quests, incidents, custom map components, or another verified pattern for Expedition V0? |

---

## Explicitly out of scope for this file

- Design intent (see `GAME_DESIGN.md`)
- Class diagrams that are not backed by real types
- Copied decompiled proprietary source dumps

Keep entries short, dated, and actionable.
