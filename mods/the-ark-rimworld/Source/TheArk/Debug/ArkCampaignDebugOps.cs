using TheArk.Campaign;
using Verse;

namespace TheArk.Debug
{
    /// <summary>
    /// Explicit write/read operations for development tooling.
    /// Mutates <see cref="ArkCampaignGameComponent"/> only — no duplicate campaign state.
    /// </summary>
    public static class ArkCampaignDebugOps
    {
        public const int FixtureCampaignDay = 47;
        public const int FixtureLandingNumber = 6;
        public const int FixtureArkTier = 2;
        public const int FixturePursuit = 73;

        public static bool TryGet(out ArkCampaignGameComponent campaign)
        {
            return ArkCampaign.TryGet(out campaign);
        }

        public static void SetCampaignActive(ArkCampaignGameComponent campaign, bool value)
        {
            campaign.CampaignActive = value;
        }

        public static void SetCampaignDay(ArkCampaignGameComponent campaign, int value)
        {
            campaign.CampaignDay = value;
        }

        public static void SetLandingNumber(ArkCampaignGameComponent campaign, int value)
        {
            campaign.LandingNumber = value;
        }

        public static void SetArkTier(ArkCampaignGameComponent campaign, int value)
        {
            campaign.ArkTier = value;
        }

        public static void SetPursuit(ArkCampaignGameComponent campaign, int value)
        {
            campaign.Pursuit = value;
        }

        /// <summary>
        /// M1 persistence proof fixture: Active=true, Day=47, Landing=6, Tier=2, Pursuit=73.
        /// Does not open a landing session.
        /// </summary>
        public static void ApplyPersistenceFixture(ArkCampaignGameComponent campaign)
        {
            SetCampaignActive(campaign, true);
            SetCampaignDay(campaign, FixtureCampaignDay);
            SetLandingNumber(campaign, FixtureLandingNumber);
            SetArkTier(campaign, FixtureArkTier);
            SetPursuit(campaign, FixturePursuit);
            Log.Message("[The Ark] [DEV] Applied M1 persistence fixture: " + FormatState(campaign));
        }

        /// <summary>
        /// M3 proof without a real gravship hop: ensure campaign active, then begin landing session.
        /// </summary>
        public static bool SimulateLanding(ArkCampaignGameComponent campaign)
        {
            if (!campaign.CampaignActive)
            {
                SetCampaignActive(campaign, true);
                Log.Message("[The Ark] [DEV] Simulate Landing: campaign was inactive — activated.");
            }

            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            return campaign.TryBeginLandingSession(map, "Dev.SimulateLanding", allowRecountSameMap: true);
        }

        public static bool EndLandingSession(ArkCampaignGameComponent campaign)
        {
            return campaign.EndLandingSession("Dev.EndLandingSession");
        }

        public static string FormatState(ArkCampaignGameComponent campaign)
        {
            return
                $"Active={campaign.CampaignActive}, Day={campaign.CampaignDay}, " +
                $"Landing={campaign.LandingNumber}, Tier={campaign.ArkTier}, Pursuit={campaign.Pursuit}, " +
                $"LandingSession={campaign.LandingSessionActive}, SessionMapId={campaign.LandingSessionMapId}";
        }

        public static void LogState(string context, ArkCampaignGameComponent campaign)
        {
            Log.Message($"[The Ark] [DEV] Campaign state ({context}): {FormatState(campaign)}");
        }
    }
}
