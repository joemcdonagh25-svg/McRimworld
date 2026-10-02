using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheArk.Campaign
{
    /// <summary>
    /// Authoritative persistent Ark campaign state for the current game save.
    /// M3 session + LandingNumber; M4 landing timer; M5 Pursuit-from-time; M6 band letters.
    /// </summary>
    public class ArkCampaignGameComponent : GameComponent
    {
        public const string PlaytestScenarioName = "The Ark";
        private const int LandingDetectIntervalTicks = 60;

        // Backing fields with explicit defaults. Scribe labels are stable save-format IDs — do not rename lightly.
        private bool campaignActive = false;
        private int campaignDay = 0;
        private int landingNumber = 0;
        private int arkTier = 0;
        private int pursuit = 0;

        // M3 landing session (temporary while landed; scribed so save/load mid-landing keeps session).
        private bool landingSessionActive = false;
        private int landingSessionMapId = -1;
        // Prevents wasSpawnedViaGravShipLanding from re-firing after End Session on the same map.
        private int lastCountedLandingMapId = -1;

        // M4: elapsed ticks while landing session is active (session-scoped; reset on end).
        private int landingSessionTicks = 0;

        // M5: how many whole landed days have already granted Pursuit this session (reset on end).
        private int landingPursuitDaysApplied = 0;

        // M6: last band we notified (-1 = none). Scribed so load does not re-spam letters.
        private int lastNotifiedPursuitBand = -1;

        // Runtime-only edge detect for Odyssey travel → land (not scribed).
        private bool prevGravshipTravelling;

        public bool CampaignActive
        {
            get => campaignActive;
            set => campaignActive = value;
        }

        public int CampaignDay
        {
            get => campaignDay;
            set => campaignDay = value;
        }

        public int LandingNumber
        {
            get => landingNumber;
            set => landingNumber = value;
        }

        public int ArkTier
        {
            get => arkTier;
            set => arkTier = value;
        }

        public int Pursuit => pursuit;

        public ArkPursuit.Band PursuitBand => ArkPursuit.BandFor(pursuit);

        public bool LandingSessionActive => landingSessionActive;

        public int LandingSessionMapId => landingSessionMapId;

        public int LandingSessionTicks => landingSessionTicks;

        /// <summary>Elapsed landing time in whole days (floor). Session-scoped — not CampaignDay.</summary>
        public int LandingSessionDaysWhole => landingSessionTicks / GenDate.TicksPerDay;

        /// <summary>
        /// Required by <see cref="Game.FillComponents"/> — Activator passes the current <see cref="Game"/>.
        /// </summary>
        public ArkCampaignGameComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref campaignActive, "arkCampaignActive", false);
            Scribe_Values.Look(ref campaignDay, "arkCampaignDay", 0);
            Scribe_Values.Look(ref landingNumber, "arkLandingNumber", 0);
            Scribe_Values.Look(ref arkTier, "arkTier", 0);
            Scribe_Values.Look(ref pursuit, "arkPursuit", 0);
            Scribe_Values.Look(ref landingSessionActive, "arkLandingSessionActive", false);
            Scribe_Values.Look(ref landingSessionMapId, "arkLandingSessionMapId", -1);
            Scribe_Values.Look(ref lastCountedLandingMapId, "arkLastCountedLandingMapId", -1);
            Scribe_Values.Look(ref landingSessionTicks, "arkLandingSessionTicks", 0);
            Scribe_Values.Look(ref landingPursuitDaysApplied, "arkLandingPursuitDaysApplied", 0);
            Scribe_Values.Look(ref lastNotifiedPursuitBand, "arkLastNotifiedPursuitBand", -1);
        }

        public override void StartedNewGame()
        {
            TryActivateFromPlaytestScenario("StartedNewGame");
            SyncGravshipTravelWatch();
            SyncPursuitBandBaseline();
            LogCampaignState("StartedNewGame");
        }

        public override void LoadedGame()
        {
            SyncGravshipTravelWatch();
            SyncPursuitBandBaseline();
            LogCampaignState("LoadedGame");
        }

        public override void GameComponentTick()
        {
            if (!campaignActive)
            {
                return;
            }

            // M4/M5: timer + Pursuit-from-time only while a landing session is active.
            if (landingSessionActive)
            {
                landingSessionTicks++;
                ApplyPursuitForLandedDays();
                return;
            }

            if (Find.TickManager == null || Find.TickManager.TicksGame % LandingDetectIntervalTicks != 0)
            {
                return;
            }

            TryDetectGravshipLanding();
        }

        /// <summary>Clamp and set Pursuit; optionally fire M6 band letter when the band changes.</summary>
        public void SetPursuit(int value, string reason, bool notifyBand = true)
        {
            int clamped = ArkPursuit.Clamp(value);
            if (clamped == pursuit)
            {
                return;
            }

            int previous = pursuit;
            pursuit = clamped;
            Log.Message(
                $"[The Ark] Pursuit set ({reason}): {previous} → {pursuit} " +
                $"({ArkPursuit.BandLabel(ArkPursuit.BandFor(pursuit))})");

            if (notifyBand)
            {
                MaybeNotifyPursuitBand();
            }
        }

        /// <summary>
        /// Begin a landing session and increment LandingNumber once.
        /// Ignores when campaign inactive or a session is already active (no spam).
        /// </summary>
        public bool TryBeginLandingSession(Map map, string reason, bool allowRecountSameMap = false)
        {
            if (!campaignActive)
            {
                Log.Warning($"[The Ark] Landing ignored ({reason}): campaign not active.");
                return false;
            }

            if (landingSessionActive)
            {
                Log.Message(
                    $"[The Ark] Landing ignored ({reason}): session already active " +
                    $"(mapId={landingSessionMapId}, Landing={landingNumber}).");
                return false;
            }

            int mapId = map != null ? map.uniqueID : -1;
            if (!allowRecountSameMap && mapId >= 0 && mapId == lastCountedLandingMapId)
            {
                Log.Message(
                    $"[The Ark] Landing ignored ({reason}): mapId={mapId} already counted " +
                    $"(Landing={landingNumber}). End session + Simulate Landing to force recount.");
                return false;
            }

            landingSessionActive = true;
            landingSessionMapId = mapId;
            lastCountedLandingMapId = mapId;
            landingSessionTicks = 0;
            landingPursuitDaysApplied = 0;
            landingNumber++;

            Log.Message(
                $"[The Ark] Landing session STARTED ({reason}): " +
                $"mapId={landingSessionMapId}, LandingNumber={landingNumber}, Session=True, TimerTicks=0");
            return true;
        }

        /// <summary>
        /// End the temporary landing session without changing durable LandingNumber / Pursuit.
        /// Clears the session timer (M4). M8 will call this on real departure.
        /// </summary>
        public bool EndLandingSession(string reason)
        {
            if (!landingSessionActive)
            {
                Log.Message($"[The Ark] End landing ignored ({reason}): no active session.");
                return false;
            }

            int endedMapId = landingSessionMapId;
            int endedTicks = landingSessionTicks;
            landingSessionActive = false;
            landingSessionMapId = -1;
            landingSessionTicks = 0;
            landingPursuitDaysApplied = 0;

            Log.Message(
                $"[The Ark] Landing session ENDED ({reason}): " +
                $"wasMapId={endedMapId}, LandingNumber={landingNumber}, Session=False, " +
                $"TimerWasTicks={endedTicks} (~{FormatTicksAsDays(endedTicks)}d), " +
                $"{ArkPursuit.Format(pursuit)}");
            return true;
        }

        /// <summary>Dev/test helper: add ticks to the landing timer while a session is active.</summary>
        public bool AddLandingSessionTicks(int ticks, string reason)
        {
            if (!landingSessionActive)
            {
                Log.Warning($"[The Ark] Add landing timer ignored ({reason}): no active session.");
                return false;
            }

            if (ticks == 0)
            {
                return true;
            }

            landingSessionTicks += ticks;
            if (landingSessionTicks < 0)
            {
                landingSessionTicks = 0;
            }

            ApplyPursuitForLandedDays();

            Log.Message(
                $"[The Ark] Landing timer adjusted ({reason}): " +
                $"delta={ticks}, TimerTicks={landingSessionTicks} (~{FormatTicksAsDays(landingSessionTicks)}d), " +
                $"{ArkPursuit.Format(pursuit)}");
            return true;
        }

        /// <summary>
        /// Playtest scenario uses vanilla ScenParts only; campaign activate happens here when
        /// New Game selected The Ark (scenario name match).
        /// </summary>
        private void TryActivateFromPlaytestScenario(string context)
        {
            RimWorld.Scenario scen = Find.Scenario;
            if (scen == null || scen.name != PlaytestScenarioName)
            {
                return;
            }

            if (!campaignActive)
            {
                campaignActive = true;
                Log.Message($"[The Ark] Campaign activated from playtest scenario ({context}).");
            }
        }

        /// <summary>M5: +1 Pursuit per whole landed day this session, clamp 0–100.</summary>
        private void ApplyPursuitForLandedDays()
        {
            int days = LandingSessionDaysWhole;
            while (landingPursuitDaysApplied < days)
            {
                landingPursuitDaysApplied++;
                if (pursuit >= ArkPursuit.Max)
                {
                    continue;
                }

                SetPursuit(pursuit + ArkPursuit.PursuitPerLandedDay, "LandedDay", notifyBand: true);
            }
        }

        private void MaybeNotifyPursuitBand()
        {
            ArkPursuit.Band band = ArkPursuit.BandFor(pursuit);
            int bandInt = (int)band;
            if (lastNotifiedPursuitBand == bandInt)
            {
                return;
            }

            ArkPursuit.Band? previous = lastNotifiedPursuitBand >= 0
                ? (ArkPursuit.Band)lastNotifiedPursuitBand
                : (ArkPursuit.Band?)null;
            lastNotifiedPursuitBand = bandInt;
            ArkPursuitLetters.SendBandChanged(pursuit, band, previous);
        }

        /// <summary>Baseline band after new/load so we do not re-letter the current band.</summary>
        private void SyncPursuitBandBaseline()
        {
            lastNotifiedPursuitBand = (int)ArkPursuit.BandFor(pursuit);
        }

        private void TryDetectGravshipLanding()
        {
            if (!ModsConfig.OdysseyActive || Find.World == null)
            {
                return;
            }

            WorldComponent_GravshipController controller =
                Find.World.GetComponent<WorldComponent_GravshipController>();
            bool travelling = controller != null && controller.IsGravshipTravelling;
            bool travelEnded = prevGravshipTravelling && !travelling;
            prevGravshipTravelling = travelling;

            Map flaggedMap = FindGravshipLandedPlayerMap();
            if (travelEnded)
            {
                Map map = flaggedMap ?? Find.CurrentMap ?? Find.AnyPlayerHomeMap;
                TryBeginLandingSession(
                    map,
                    flaggedMap != null
                        ? "OdysseyTravelEnded+wasSpawnedViaGravShipLanding"
                        : "OdysseyTravelEnded",
                    allowRecountSameMap: true);
                return;
            }

            if (!travelling && flaggedMap != null)
            {
                TryBeginLandingSession(flaggedMap, "wasSpawnedViaGravShipLanding", allowRecountSameMap: false);
            }
        }

        private static Map FindGravshipLandedPlayerMap()
        {
            if (Find.Maps == null)
            {
                return null;
            }

            foreach (Map map in Find.Maps)
            {
                if (map == null || !map.wasSpawnedViaGravShipLanding)
                {
                    continue;
                }

                if (map.IsPlayerHome || map.mapPawns?.AnyColonistSpawned == true)
                {
                    return map;
                }
            }

            return null;
        }

        private void SyncGravshipTravelWatch()
        {
            if (!ModsConfig.OdysseyActive || Find.World == null)
            {
                prevGravshipTravelling = false;
                return;
            }

            WorldComponent_GravshipController controller =
                Find.World.GetComponent<WorldComponent_GravshipController>();
            prevGravshipTravelling = controller != null && controller.IsGravshipTravelling;
        }

        private void LogCampaignState(string context)
        {
            Log.Message(
                $"[The Ark] Campaign state ({context}): " +
                $"Active={campaignActive}, Day={campaignDay}, Landing={landingNumber}, Tier={arkTier}, " +
                $"{ArkPursuit.Format(pursuit)}, " +
                $"LandingSession={landingSessionActive}, SessionMapId={landingSessionMapId}, " +
                $"TimerTicks={landingSessionTicks} (~{FormatTicksAsDays(landingSessionTicks)}d)");
        }

        public static string FormatTicksAsDays(int ticks)
        {
            return (ticks / (float)GenDate.TicksPerDay).ToString("0.00");
        }
    }
}
