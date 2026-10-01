using LudeonTK;
using VampireLord.Campaign;
using Verse;

namespace VampireLord.Debug
{
    /// <summary>
    /// Dev-mode tools for activating and verifying the Wave Director.
    /// Campaign does not auto-start — use these actions.
    /// </summary>
    public static class VampireLordDebugActions
    {
        [DebugAction(
            category = "Vampire Lord",
            name = "Start Vampire Lord Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StartVampireLordCampaign()
        {
            if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
            {
                Log.Error("[VampireLord] No campaign component on Current.Game.");
                return;
            }

            VampireLordWaveDirector.ActivateCampaign(campaign);
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Stop Vampire Lord Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StopVampireLordCampaign()
        {
            if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
            {
                Log.Error("[VampireLord] No campaign component on Current.Game.");
                return;
            }

            VampireLordWaveDirector.StopCampaign(campaign);
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Trigger Warning Now",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TriggerWarningNow()
        {
            if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
            {
                Log.Error("[VampireLord] No campaign component on Current.Game.");
                return;
            }

            VampireLordWaveDirector.TriggerWarningNow(campaign);
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Trigger Wave Now",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TriggerWaveNow()
        {
            if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
            {
                Log.Error("[VampireLord] No campaign component on Current.Game.");
                return;
            }

            VampireLordWaveDirector.TriggerWaveNow(campaign);
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Show Campaign State",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ShowCampaignState()
        {
            if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
            {
                Log.Error("[VampireLord] No campaign component on Current.Game.");
                return;
            }

            Log.Message("[VampireLord] Campaign state:\n" + VampireLordWaveDirector.FormatState(campaign));
        }
    }
}
