using Verse;

namespace TheArk.Campaign
{
    /// <summary>
    /// Authoritative persistent Ark campaign state for the current game save.
    /// No gameplay effects in M1 — persistence and access only.
    /// </summary>
    public class ArkCampaignGameComponent : GameComponent
    {
        // Backing fields with explicit defaults. Scribe labels are stable save-format IDs — do not rename lightly.
        private bool campaignActive = false;
        private int campaignDay = 0;
        private int landingNumber = 0;
        private int arkTier = 0;
        private int pursuit = 0;

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
        }

        public override void StartedNewGame()
        {
            LogCampaignState("StartedNewGame");
        }

        public override void LoadedGame()
        {
            LogCampaignState("LoadedGame");
        }

        private void LogCampaignState(string context)
        {
            Log.Message(
                $"[The Ark] Campaign state ({context}): " +
                $"Active={campaignActive}, Day={campaignDay}, Landing={landingNumber}, Tier={arkTier}, Pursuit={pursuit}");
        }
    }
}
