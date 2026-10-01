using LudeonTK;
using TheArk.Campaign;
using Verse;

namespace TheArk.Debug
{
    /// <summary>
    /// Dev Mode debug-action menu entries for Ark campaign instrumentation (V1.1 / M2).
    /// </summary>
    public static class ArkCampaignDebugActions
    {
        [DebugAction(
            category = "The Ark (DEV)",
            name = "Open Campaign Debug",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void OpenCampaignDebug()
        {
            if (!Prefs.DevMode)
            {
                Log.Warning("[The Ark] [DEV] Campaign debug UI requires Dev Mode.");
                return;
            }

            if (!ArkCampaignDebugOps.TryGet(out _))
            {
                Log.Error("[The Ark] [DEV] No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            Find.WindowStack.Add(new Dialog_ArkCampaignDebug());
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "Log Campaign State",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogCampaignState()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Error("[The Ark] [DEV] No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            ArkCampaignDebugOps.LogState("DebugAction", campaign);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "Apply M1 Persistence Fixture",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ApplyM1PersistenceFixture()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Error("[The Ark] [DEV] No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            ArkCampaignDebugOps.ApplyPersistenceFixture(campaign);
        }
    }
}
