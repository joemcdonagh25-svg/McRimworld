using TheArk.Campaign;
using TheArk.Debug;
using RimWorld;
using Verse;

namespace TheArk.Scenario
{
    /// <summary>
    /// Playtest-only: activate Ark campaign state when starting via The Ark scenario.
    /// Does not place a gravship yet — Odyssey wreckage/start is a later milestone.
    /// </summary>
    public class ScenPart_ArkPlaytestSetup : ScenPart
    {
        public override void PostGameStart()
        {
            base.PostGameStart();
            TryActivateCampaign();
        }

        public override string Summary(RimWorld.Scenario scen)
        {
            return "Activates Ark campaign state for playtest; use Dev Mode → The Ark (DEV) for the M1 fixture.";
        }

        private static void TryActivateCampaign()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Warning("[The Ark] Playtest setup: no ArkCampaignGameComponent — cannot activate campaign.");
                return;
            }

            if (!campaign.CampaignActive)
            {
                ArkCampaignDebugOps.SetCampaignActive(campaign, true);
            }

            ArkCampaignDebugOps.LogState("Scenario.PostGameStart", campaign);
            Log.Message(
                "[The Ark] Playtest scenario ready. Dev Mode → The Ark (DEV) → Apply M1 Persistence Fixture " +
                "for the save/quit/load proof.");
        }
    }
}
