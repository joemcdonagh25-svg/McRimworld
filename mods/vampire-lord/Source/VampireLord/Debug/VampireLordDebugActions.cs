using LudeonTK;
using VampireLord.Campaign;
using Verse;

namespace VampireLord.Debug
{
    /// <summary>
    /// Dev-mode tools for Wave Director + Blood Tithe.
    /// Scenario path auto-starts the campaign; these remain for force-testing.
    /// </summary>
    public static class VampireLordDebugActions
    {
        [DebugAction(
            category = "Vampire Lord",
            name = "Start Vampire Lord Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StartVampireLordCampaign()
        {
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
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
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
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
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
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
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
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
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            Log.Message("[VampireLord] Campaign state:\n" + VampireLordWaveDirector.FormatState(campaign));
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Show Blood Tithe",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ShowBloodTithe()
        {
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            Log.Message(
                "[VampireLord] Blood Tithe:\n" +
                $"BloodReserve={campaign.BloodReserve}\n" +
                $"BloodGainedSinceWave={campaign.BloodGainedSinceWave}\n" +
                $"WaveBloodCostNext={VampireLordBloodTithe.WaveBloodCost(campaign)}\n" +
                $"LastWaveBloodStarved={campaign.LastWaveBloodStarved}\n" +
                $"LastWaveBloodSpent={campaign.LastWaveBloodSpent}/{campaign.LastWaveBloodCost}");
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Add Blood (+20)",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddBlood()
        {
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordBloodTithe.AddBlood(campaign, 20, "debug");
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Spend Blood (-20)",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpendBlood()
        {
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordBloodTithe.SpendBlood(campaign, 20, "debug");
        }

        [DebugAction(
            category = "Vampire Lord",
            name = "Force Low Blood (0)",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ForceLowBlood()
        {
            if (!TryGet(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordBloodTithe.ForceLowBlood(campaign);
        }

        private static bool TryGet(out VampireLordCampaignGameComponent campaign)
        {
            if (VampireLordCampaign.TryGet(out campaign))
            {
                return true;
            }

            Log.Error("[VampireLord] No campaign component on Current.Game.");
            return false;
        }
    }
}
