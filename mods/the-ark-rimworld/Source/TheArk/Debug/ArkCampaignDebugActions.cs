using LudeonTK;
using TheArk.Campaign;
using Verse;

namespace TheArk.Debug
{
    /// <summary>
    /// Dev Mode debug-action menu entries for Ark campaign instrumentation.
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

        [DebugAction(
            category = "The Ark (DEV)",
            name = "Simulate Landing",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SimulateLanding()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Error("[The Ark] [DEV] No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            ArkCampaignDebugOps.SimulateLanding(campaign);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "End Landing Session",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void EndLandingSession()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Error("[The Ark] [DEV] No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            ArkCampaignDebugOps.EndLandingSession(campaign);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "Advance Landing Timer +1 Day",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AdvanceLandingTimerOneDay()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Error("[The Ark] [DEV] No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            ArkCampaignDebugOps.AdvanceLandingTimerOneDay(campaign);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "Jump Pursuit To Next Band",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void JumpPursuitToNextBand()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Error("[The Ark] [DEV] No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            ArkCampaignDebugOps.JumpPursuitToNextBand(campaign);
        }
    }
}
