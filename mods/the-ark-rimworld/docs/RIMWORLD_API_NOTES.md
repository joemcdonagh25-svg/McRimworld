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
| Assembly identity observed | `Assembly-CSharp, Version=1.6.9676.17735` |
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

## M1 Persistence Investigation

**Date:** 2026-09-30  
**Goal:** Choose one authoritative persistence owner for durable Ark campaign fields that survives save/load, map changes, and (as far as can be verified) future gravship travel — without implementing M1 yet.  
**Evidence:** Reflection + IL inspection of the referenced RimWorld 1.6 `Assembly-CSharp.dll` (`Version=1.6.9676.17735`) loaded from the Steam Managed folder. No speculative APIs.

### APIs inspected

| Type | Exists | Namespace |
|------|--------|-----------|
| `GameComponent` | Yes | `Verse` |
| `GameComponentUtility` | Yes | `Verse` |
| `WorldComponent` | Yes | `RimWorld.Planet` |
| `MapComponent` | Yes | `Verse` |
| `IExposable` | Yes | `Verse` |
| `Scribe` / `Scribe_Values` / `Scribe_Collections` | Yes | `Verse` |
| `Game` | Yes | `Verse` |
| `World` | Yes | `RimWorld.Planet` |
| `Map` | Yes | `Verse` |
| `Current` | Yes | `Verse` |
| `GenTypes` | Yes | `Verse` |
| `ThingComp` / `ThingWithComps` | Yes | `Verse` |
| `WorldObject` | Yes | `RimWorld.Planet` |
| `WorldComponent_GravshipController` | Yes | `Verse` |
| `Gravship` | Yes | `RimWorld.Planet` |

### Comparison summary

| Mechanism | Scope | Save participation | Survives map remove? | Fit for whole-campaign state |
|-----------|-------|--------------------|----------------------|------------------------------|
| `GameComponent` | Entire `Game` (save) | Scribed inside `Game` as `components` | Yes (not map-owned) | **Best fit** |
| `WorldComponent` | Planet `World` | Scribed inside `World` as `components` | Yes (not map-owned) | Viable alternative |
| `MapComponent` | Single `Map` | Scribed with that map | **No** — map-local; `MapRemoved()` exists | **Reject** for durable campaign |
| `ThingComp` / building / pawn | Single thing | With that thing | No / fragile | **Reject** as authority |
| `WorldObject` (e.g. `Gravship`) | World object instance | With that object | Depends on object lifetime | **Reject** as campaign authority |

---

### 1) `Verse.GameComponent` (recommended)

**Verified signatures / surface**

- `public abstract class GameComponent : IExposable`
- Constructors on abstract base: `protected GameComponent()`
- Virtual lifecycle / hooks observed:
  - `ExposeData()`
  - `FinalizeInit()`
  - `StartedNewGame()`
  - `LoadedGame()`
  - `GameComponentTick()`
  - `GameComponentUpdate()`
  - `GameComponentOnGUI()`
  - `AppendDebugString(StringBuilder sb)`
- Concrete vanilla subclasses observed (all take `Game`):
  - `ctor(Verse.Game game)` — e.g. `GameComponent_Anomaly`, `GameComponent_Bossgroup`, `GameComponent_DebugTools`, etc.

**Instantiation / registration (verified via IL of `Game.FillComponents`)**

1. `GenTypes.AllSubclassesNonAbstract(typeof(GameComponent))` discovers non-abstract subclasses (including mod assemblies once loaded).
2. If `Game.GetComponent(Type)` finds none, creates one with:
   - `Activator.CreateInstance(type, new object[] { this })`
   - where `this` is the `Game` instance (IL: `newarr` length 1, `ldarg.0`, `stelem.ref`).
3. Therefore a mod component **must** expose a public constructor `MyComponent(Game game)`.

**Retrieval (verified)**

- `Game.GetComponent<T>()`
- `Game.GetComponent(Type type)`
- Access the live game via `Current.Game` (`Verse.Current` static property `Game`).
- Pattern: `Current.Game.GetComponent<ArkCampaignGameComponent>()`.
- Note: `Verse.Find` exposes many world/map helpers but **no** `Find.Game` property was present; use `Current.Game`.

**Save / load (verified via IL)**

- `Game.ExposeData()` rejects normal expose for full game load messaging, then exposes `currentMapIndex`, calls `ExposeSmallComponents()`, then deep-looks `world`, collection-looks `maps`, etc.
- `Game.ExposeSmallComponents()` scries label `"components"` via `Scribe_Collections.Look`, then calls `FillComponents()`.
- Meaning: GameComponents are part of the game save blob; after load, missing subclass instances are filled in.

**Utility dispatcher (verified)**

- `GameComponentUtility` static methods: `FinalizeInit`, `StartedNewGame`, `LoadedGame`, `GameComponentTick`, `GameComponentUpdate`, `GameComponentOnGUI`.
- Exact call sites from `Root_Play` / game loop: **UNVERIFIED** (not traced in this pass). Existence of the utility and matching virtuals on `GameComponent` is verified.

**Map-transition implications**

- `Game` owns `List<Map> maps` and `DeinitAndRemoveMap(Map map, bool notifyPlayer)`.
- GameComponents live on `Game.components`, not on a map.
- Therefore they are **not** destroyed merely because a map is removed — verified structurally (ownership), not by runtime gravship test.

---

### 2) `RimWorld.Planet.WorldComponent` (viable alternative)

**Verified signatures / surface**

- `public abstract class WorldComponent : IExposable`
- `public WorldComponent(RimWorld.Planet.World world)`
- Field: `public World world`
- Virtuals: `ExposeData()`, `FinalizeInit(bool fromLoad)`, `WorldComponentTick()`, `WorldComponentUpdate()`, `WorldComponentOnGUI()`
- No `StartedNewGame` / `LoadedGame` virtuals on this type (those exist on `GameComponent`).

**Instantiation / save**

- `World.FillComponents()` mirrors Game: `AllSubclassesNonAbstract(WorldComponent)` + `Activator.CreateInstance(type, object[])` (same pattern; ctor requires `World`).
- `World.ExposeComponents()` scries `"components"` then `FillComponents()`.

**Retrieval**

- `Find.World.GetComponent<T>()` / `World.GetComponent(Type)` ( `Find.World` verified).

**Fit notes**

- Also survives map removal (world-scoped).
- Odyssey places `Verse.WorldComponent_GravshipController` here — good home for **ship travel control**, not necessarily for Ark meta-campaign fields.
- Prefer `GameComponent` for campaign meta-state so ship controller concerns stay separate from campaign authority.

---

### 3) `Verse.MapComponent` (rejected for M1 durable state)

**Verified signatures / surface**

- `public abstract class MapComponent : IExposable`
- `public MapComponent(Verse.Map map)`
- Field: `public Map map`
- Virtuals include `ExposeData()`, `FinalizeInit()`, tick/update/OnGUI/draw, `MapGenerated()`, **`MapRemoved()`**

**Why reject for durable campaign state**

- Explicitly map-owned. When `Game.DeinitAndRemoveMap` removes a map, map-local components are the wrong owner for LandingNumber / ArkTier / Pursuit that must outlive that map.
- Acceptable later for **landing-session** scratch state (M3–M4), not for authoritative campaign fields.

---

### 4) Other mechanisms briefly verified

- `Verse.IExposable`: interface with `void ExposeData()`.
- `Verse.Scribe_Values.Look<T>(ref T value, string label, T defaultValue = default, bool forceSave = false)` — suitable for `bool` / `int` campaign fields.
- `Verse.Scribe.mode` uses `LoadSaveMode`: `Inactive`, `Saving`, `LoadingVars`, `ResolvingCrossRefs`, `PostLoadInit`.
- `ThingComp`: exists; not selected as campaign authority (thing-lifetime coupling). Whether every `ThingComp` is `IExposable` was not relied upon; `ThingWithComps` implements `IExposable`.
- `WorldObject`: exists and is `IExposable`; `RimWorld.Planet.Gravship : WorldObject` exists. **Do not** make the Gravship object the authoritative campaign owner — object lifetime across takeoff/landing is a later Odyssey concern.

---

### Odyssey / gravship implications (verified vs unverified)

**Verified**

- `Game` has property `Gravship` of type `RimWorld.Planet.Gravship` (read/write).
- `RimWorld.Planet.Gravship` derives from `WorldObject` (ship presence as a world object).
- `Verse.WorldComponent_GravshipController : WorldComponent` exists with methods including:
  - `InitiateLanding(Gravship, Map, IntVec3, Rot4)`
  - `InitiateTakeoff(Building_GravEngine, PlanetTile)`
  - plus `ExposeData`, `FinalizeInit(bool)`, GUI/update hooks.
- `Map` has field `bool wasSpawnedViaGravShipLanding`.
- It is therefore verified that Odyssey’s travel/landing **control** is world-scoped (`WorldComponent`), while maps are created/removed around landings — reinforcing that durable Ark campaign state must **not** live on `MapComponent`.

**UNVERIFIED (do not implement against these guesses)**

- Exact lifetime of `Game.Gravship` / `Planet.Gravship` across a full takeoff → travel → landing cycle (destroyed vs reused vs replaced).
- Whether any GameComponent list is cleared during gravship transitions (structurally unlikely; not runtime-tested).
- Precise event to detect “has landed” / “has departed” for M3/M8 (`InitiateLanding` / `InitiateTakeoff` are candidates only).
- Whether Odyssey DLC must be declared in `About.xml` for these types to exist at runtime on all installs (types are present in this machine’s `Assembly-CSharp`; packaging/DLC gating not verified here).

---

### Decision for The Ark (M1)

**Recommended persistence owner:** a single concrete `Verse.GameComponent` subclass owned by `Current.Game`.

**Rationale (short):**

1. Matches “entire current game/campaign” scope.
2. Participates in verified game save/load (`components` + `FillComponents`).
3. Does not die with map removal (unlike `MapComponent`).
4. Offers `StartedNewGame` / `LoadedGame` hooks useful for initialising defaults without Harmony.
5. Leaves Odyssey’s `WorldComponent_GravshipController` free to remain the ship-operations integration point later.

**Implementation status:** Code landed as `TheArk.Campaign.ArkCampaignGameComponent` (see entry below). **Runtime save/load in RimWorld is still pending manual verification.**

**Related milestone:** M1

---

### 2026-09-30 — M1 ArkCampaignGameComponent implementation

- **Goal:** Smallest authoritative persistent campaign state (five primitives, no gameplay effects).
- **Evidence:** Implemented against previously verified `GameComponent` / `Scribe_Values` / `Current.Game.GetComponent<T>()` APIs; `dotnet build` succeeded with 0 errors / 0 warnings.
- **API used:**
  - `TheArk.Campaign.ArkCampaignGameComponent : Verse.GameComponent`
  - Constructor: `public ArkCampaignGameComponent(Verse.Game game)` (required by `FillComponents`)
  - `ExposeData` via `Scribe_Values.Look` with stable labels:
    - `arkCampaignActive` (`bool`, default `false`)
    - `arkCampaignDay` (`int`, default `0`)
    - `arkLandingNumber` (`int`, default `0`)
    - `arkTier` (`int`, default `0`)
    - `arkPursuit` (`int`, default `0`)
  - Lifecycle logs: overrides `StartedNewGame()` and `LoadedGame()` emit one concise `Log.Message` each
  - Access: `TheArk.Campaign.ArkCampaign.TryGet` / `Get` → `Current.Game.GetComponent<ArkCampaignGameComponent>()` (no state cache)
- **Behaviour notes:** Fields have no gameplay effects. Defaults remain zero/false until future systems or debug tools change them.
- **Decision for The Ark:** This component is the sole authoritative owner for durable M1 campaign fields.
- **Runtime status:** **UNVERIFIED** in RimWorld until manual save/load test completes.
- **Related milestone:** M1 (IMPLEMENTATION COMPLETE; RUNTIME VERIFIED pending)

---

### 2026-10-02 — M7 Pursuit incident (ManhunterPack)

- **Goal:** One real gameplay consequence from Pursuit without a human raid ladder.
- **Evidence:** MetadataLoadContext on `Krafs.Rimworld.Ref` 1.6.4871 `RimWorld.IncidentDefOf` + VL pattern in-repo (`VampireLordRaidLauncher`).
- **API:**
  - `RimWorld.IncidentDefOf.ManhunterPack`
  - `IncidentWorker.TryExecute(IncidentParms)`
  - `IncidentParms`: `target`, `points`, `forced`, `bypassStorytellerSettings` (no `faction` / `raidStrategy`)
- **Behaviour notes:** Fixed points (350). Once per landing session via `arkPursuitIncidentFiredThisSession`. Trigger Pursuit ≥ 61 (BESIEGED) while session active.
- **Decision for The Ark:** Prefer ManhunterPack over `RaidEnemy` for M7 — supports leave-don't-farm; Harbinger encounter deferred.
- **Runtime status:** Pending Joe **Run Pursuit Incident Proof**.
- **Related milestone:** M7

### 2026-10-02 — M5/M6 Pursuit growth + band letters (Pressure V0)

- **Goal:** Pursuit rises with landed time; player sees QUIET→HARBINGER via letters.
- **Evidence:** Extends M3/M4 session owner; `Find.LetterStack.ReceiveLetter` (same pattern as Vampire Lord).
- **API / design:**
  - Growth: +1 Pursuit per `GenDate.TicksPerDay` while `LandingSessionActive`; clamp 0–100
  - Bands: 0–20 Quiet, 21–40 Noticed, 41–60 Hunted, 61–80 Besieged, 81–100 Harbinger (`GAME_DESIGN.md`)
  - `LetterDefOf` Neutral / Negative / ThreatSmall / ThreatBig by band
  - Scribe: `arkLandingPursuitDaysApplied`, `arkLastNotifiedPursuitBand`
- **Decision for The Ark:** No Harmony. No MainTab for V0. Letters on band change only (baseline sync on new/load). Incidents deferred to M7.
- **Runtime status:** **RUNTIME VERIFIED** (Joe, 2026-10-02; `Pressure V0 proof PASS` — pursuitGrew, harbinger, pursuitKept).
- **Related milestone:** M5 / M6

### 2026-10-01 — M4 Landing Timer (session ticks)

- **Goal:** Elapsed time since landing, only while landing session active; visible in Dev Mode; no durable-field corruption.
- **Evidence:** Extends M3 session owner; `GameComponent.GameComponentTick` + `GenDate.TicksPerDay`.
- **API:**
  - `Verse.GameComponent.GameComponentTick` — increment `landingSessionTicks` while session active
  - `RimWorld.GenDate.TicksPerDay` — day formatting / +1 day debug advance
  - Scribe: `arkLandingSessionTicks` (`int`, default 0)
- **Decision for The Ark:** Persist accumulated session ticks (reset on begin/end). Do not write `CampaignDay`. Pursuit growth deferred to M5.
- **Runtime status:** **RUNTIME VERIFIED** (Joe, 2026-10-02; `Pressure V0 proof PASS` — timerStartZero, timerDay, timerCleared).
- **Related milestone:** M4

### 2026-10-01 — M3 Landing Detection (no Harmony)

- **Goal:** Detect Ark landing once, open a temporary landing session, increment durable `LandingNumber`.
- **Evidence:** Metadata inspection of `Krafs.Rimworld.Ref` 1.6.4871 + approved M3 V0 proposal.
- **API (verified present):**
  - `Verse.ModsConfig.OdysseyActive`
  - `Verse.WorldComponent_GravshipController`: `IsGravshipTravelling`, `InitiateLanding`, `LandingEnded`, `InitiateTakeoff`
  - `Verse.Map.wasSpawnedViaGravShipLanding`
  - `RimWorld.ScenPart.PostGravshipLanded(Map)`
  - `Verse.GameComponent.GameComponentTick`
- **Decision for The Ark:**
  - No Harmony. Poll travel edge (`IsGravshipTravelling` true→false) and map flag; also hook playtest `PostGravshipLanded`.
  - Session state lives on `ArkCampaignGameComponent` (scribed): `arkLandingSessionActive`, `arkLandingSessionMapId`, `arkLastCountedLandingMapId`.
  - Dev **Simulate Landing** is the V0 runtime proof path (surface playtest has no gravship hop yet).
  - `LandingEnded` / takeoff remain candidates for M8; not required for M3 V0.
- **Runtime status:** Simulate Landing **RUNTIME VERIFIED** (Joe, 2026-10-01). Real Odyssey land still UNVERIFIED.
- **Related milestone:** M3

### 2026-10-01 — ChooseIdeoPreset NRE + non-colonist cascade (playtest harden)

- **Goal:** After ConfigurePawns fix, Joe could select The Ark and activate campaign, but settle hit `Page_ChooseIdeoPreset.PostOpen` NRE and tick cascade (Anomaly/History/goodwill/non-colonist apparel).
- **Evidence:** Joe Player.log 2026-10-01; same stack class already documented on Vampire Lord M2.
- **API:**
  - `RimWorld.Page_ChooseIdeoPreset.PostOpen` (Ideology DLC page after site select)
  - `Faction.ideos.ChooseOrGenerateIdeo(IdeoGenerationParms)` when `PrimaryIdeo` is null
  - `Pawn.SetFaction(Faction.OfPlayer)` for humanlikes on the start map
  - Hooks: `ScenPart.PostWorldGenerate`, `ScenPart.PostGameStart`
- **Decision for The Ark:** Re-enable `ScenPart_ArkPlaytestSetup` with VL-style ideo generate + colonist reassignment. Do not Harmony-patch ChooseIdeoPreset in V1.1; accept possible NRE log line if game continues playable.
- **Related milestone:** M2 / V1.1 playtest harden

### 2026-10-01 — Scenario listing / Odyssey dependency (playtest)

- **Goal:** Why New Game did not show **The Ark**.
- **Evidence:** Operator report + RimWorld mod load behaviour; `ScenarioLister` serves `FromDef` scenarios only from enabled mods' loaded `ScenarioDef`s.
- **Finding:** Hard `modDependencies` on `Ludeon.RimWorld.Odyssey` can leave The Ark unchecked when Odyssey is off, so `TheArk_Playtest` never enters the scenario list. V1.1 playtest is surface-only and does not need Odyssey yet.
- **Decision for The Ark:** Remove hard Odyssey `modDependencies` for now; keep `loadAfter`. Activate campaign via ScenPart and `ArkCampaignGameComponent.StartedNewGame` when `Find.Scenario.name == "The Ark"`.
- **Related milestone:** M2 playtest convenience / hotfix

### 2026-10-01 — Scenario + ScenPart playtest setup (M2 companion)

- **Goal:** New Game entry that activates Ark campaign state without a global map-gen patch.
- **Evidence:** Same `ScenPart` surface as Vampire Lord M2; metadata confirms `RimWorld.ScenPart` hooks including `PostGameStart()`, `GenerateIntoMap(Map)`, and also `PostGravshipLanded` (unused here; candidate for later landing work).
- **API:**
  - `RimWorld.ScenarioDef` / scenario `parts` list
  - `RimWorld.ScenPart` → custom `TheArk.Scenario.ScenPart_ArkPlaytestSetup`
  - `ScenPart_ConfigPage_ConfigureStartingPawns` field `pawnCount`
  - `ScenPart_GameStartDialog`, `ScenPart_PlayerPawnsArriveMethod`, `ScenPart_StartingThing_Defined`, `ScenPart_StartingResearch`
  - Odyssey packageId string present in assembly: `Ludeon.RimWorld.Odyssey`
- **Decision for The Ark:** Scenario-scoped activation of `CampaignActive` in `PostGameStart`. No gravship spawn in this scenario (Odyssey wreckage/start deferred). M1 fixture remains a Dev action.
- **Related milestone:** M2 / V1.1 playtest convenience

### 2026-10-01 — M2 / V1.1 Campaign Debug UI (Dev Mode)

- **Goal:** Inspect and deliberately edit the five M1 campaign fields in-game without a second state owner; enable M1 save/load proof.
- **Evidence:** Metadata inspection of `Krafs.Rimworld.Ref` 1.6.4871 `Assembly-CSharp.dll` (Cloud); Vampire Lord runtime-proven `DebugAction` pattern on the same machine/install family.
- **API:**
  - `LudeonTK.DebugActionAttribute` fields observed: `name`, `category`, `allowedGameStates`, `actionType`, DLC flags, `displayPriority`, `hideInSubMenu`
  - `LudeonTK.AllowedGameStates.PlayingOnMap`
  - `Verse.Window` (`DoWindowContents`, `InitialSize`, `optionalTitle`, `onlyDrawInDevMode`, close/drag flags)
  - `Verse.Find.WindowStack.Add(Window)`
  - `Verse.Listing_Standard`: `Begin` / `End`, `CheckboxLabeled`, `TextFieldNumericLabeled`, `ButtonText`, `Label`, `Gap` / `GapLine`
  - `Verse.Prefs.DevMode` (get/set)
  - `Verse.Widgets` text/button helpers also present (listing wrappers used)
- **Behaviour notes:** Debug actions appear under Dev Mode tools. Draft UI buffers are discarded unless Apply runs. Fixture sets Active=true, Day=47, Landing=6, Tier=2, Pursuit=73.
- **Decision for The Ark:** Ship `ArkCampaignDebugActions` + `Dialog_ArkCampaignDebug` + `ArkCampaignDebugOps`. No Harmony. No MainTab. No player-facing Pursuit UI yet (M6).
- **Runtime status:** UI behaviour **UNVERIFIED** until Joe’s in-RimWorld test.
- **Related milestone:** M2 / V1.1

---

## Pending investigations (not yet verified)

These are **questions**, not APIs. Do not implement against assumed answers.

| Topic | Needed by | Question |
|-------|-----------|----------|
| Campaign persistence owner | M1 | **Resolved:** `ArkCampaignGameComponent` (`GameComponent`). |
| Runtime save/load proof of Ark fields | M1/M2 | Confirm fields round-trip via M1 fixture + debug UI (manual test pending). |
| Gravship landed state | M3 | **Implemented (poll):** travel edge + `wasSpawnedViaGravShipLanding` + `PostGravshipLanded`; real hop UNVERIFIED; Simulate Landing is V0 proof. |
| Gravship departure | M8 | Confirm `InitiateTakeoff` (or other) as reliable session teardown signal. |
| Gravship object lifetime | M3/M8 | Does `Game.Gravship` / `Planet.Gravship` persist, null out, or get replaced across travel? |
| Incident hooks for Pursuit | M7 | **Resolved (impl):** `IncidentDefOf.ManhunterPack` + fixed points; RUNTIME VERIFIED pending Joe proof. |
| Quest / objective scaffolding | M9 | Prefer quests, incidents, custom map components, or another verified pattern for Expedition V0? |

---

## Explicitly out of scope for this file

- Design intent (see `GAME_DESIGN.md`)
- Class diagrams that are not backed by real types
- Copied decompiled proprietary source dumps

Keep entries short, dated, and actionable.
