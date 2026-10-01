using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheArk.Campaign
{
    /// <summary>
    /// Authoritative persistent Ark campaign state for the current game save.
    /// M3: landing detection opens a temporary landing session and increments LandingNumber once.
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

        public int Pursuit
        {
            get => pursuit;
            set => pursuit = value;
        }

        public bool LandingSessionActive => landingSessionActive;

        public int LandingSessionMapId => landingSessionMapId;

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
        }

        public override void StartedNewGame()
        {
            TryActivateFromPlaytestScenario("StartedNewGame");
            SyncGravshipTravelWatch();
            LogCampaignState("StartedNewGame");
        }

        public override void LoadedGame()
        {
            SyncGravshipTravelWatch();
            LogCampaignState("LoadedGame");
        }

        public override void GameComponentTick()
        {
            if (!campaignActive || landingSessionActive)
            {
                return;
            }

            if (Find.TickManager == null || Find.TickManager.TicksGame % LandingDetectIntervalTicks != 0)
            {
                return;
            }

            TryDetectGravshipLanding();
        }

        /// <summary>
        /// Begin a landing session and increment LandingNumber once.
        /// Ignores when campaign inactive or a session is already active (no spam).
        /// </summary>
        /// <param name="allowRecountSameMap">
        /// True for Dev Simulate / travel-ended edges. False for map-flag polling so ending a
        /// session on the same gravship map does not immediately re-count.
        /// </param>
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
            landingNumber++;

            Log.Message(
                $"[The Ark] Landing session STARTED ({reason}): " +
                $"mapId={landingSessionMapId}, LandingNumber={landingNumber}, Session=True");
            return true;
        }

        /// <summary>
        /// End the temporary landing session without changing durable LandingNumber.
        /// M8 will call this (or equivalent) on real departure; Dev Mode can call it for re-test.
        /// </summary>
        public bool EndLandingSession(string reason)
        {
            if (!landingSessionActive)
            {
                Log.Message($"[The Ark] End landing ignored ({reason}): no active session.");
                return false;
            }

            int endedMapId = landingSessionMapId;
            landingSessionActive = false;
            landingSessionMapId = -1;

            Log.Message(
                $"[The Ark] Landing session ENDED ({reason}): " +
                $"wasMapId={endedMapId}, LandingNumber={landingNumber}, Session=False");
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

            // Missed travel edge (e.g. loaded mid-settle): count once when a player map carries the flag.
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
                $"Active={campaignActive}, Day={campaignDay}, Landing={landingNumber}, Tier={arkTier}, Pursuit={pursuit}, " +
                $"LandingSession={landingSessionActive}, SessionMapId={landingSessionMapId}");
        }
    }
}
