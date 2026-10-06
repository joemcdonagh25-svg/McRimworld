using System;
using LudeonTK;
using TheArk.Campaign;
using Verse;

namespace TheArk.Debug
{
    /// <summary>
    /// Developer-only DebugActions for Quicktest / Dev Mode campaign-state iteration.
    /// Not a gameplay system. Does not own or cache campaign state — always reads/writes
    /// <see cref="ArkCampaignGameComponent"/> via <see cref="ArkCampaign.TryGet"/>.
    /// Search Dev Actions for "ARK:".
    /// </summary>
    public static class ArkDebugActions
    {
        // Dialog_Slider needs a UI upper bound. These are entry convenience ranges only —
        // not game-balance caps (except Pursuit, which uses ArkPursuit.Max already in the project).
        private const int CampaignDaySliderMax = 100_000;
        private const int LandingNumberSliderMax = 10_000;
        private const int ArkTierSliderMax = 20;

        private static bool TryGetCampaign(out ArkCampaignGameComponent campaign)
        {
            if (ArkCampaign.TryGet(out campaign))
            {
                return true;
            }

            Log.Warning("[The Ark] No ArkCampaignGameComponent (no current game or component missing).");
            campaign = null;
            return false;
        }

        private static string FormatFields(ArkCampaignGameComponent campaign)
        {
            return
                "CampaignActive: " + campaign.CampaignActive + "\n" +
                "CampaignDay: " + campaign.CampaignDay + "\n" +
                "LandingNumber: " + campaign.LandingNumber + "\n" +
                "ArkTier: " + campaign.ArkTier + "\n" +
                "Pursuit: " + campaign.Pursuit;
        }

        private static string FormatSaveTestSnapshot(ArkCampaignGameComponent campaign)
        {
            return
                "[The Ark SAVE TEST] " +
                "Active=" + campaign.CampaignActive +
                " Day=" + campaign.CampaignDay +
                " Landing=" + campaign.LandingNumber +
                " Tier=" + campaign.ArkTier +
                " Pursuit=" + campaign.Pursuit;
        }

        private static void ApplyPreset(
            ArkCampaignGameComponent campaign,
            string presetName,
            bool active,
            int day,
            int landing,
            int tier,
            int pursuit)
        {
            campaign.CampaignActive = active;
            campaign.CampaignDay = day;
            campaign.LandingNumber = landing;
            campaign.ArkTier = tier;
            campaign.SetPursuit(pursuit, "Debug.Preset." + presetName, notifyBand: true);
            Log.Message("[The Ark] Preset applied — " + presetName + ":\n" + FormatFields(campaign));
        }

        private static void PromptInt(
            string title,
            int current,
            int min,
            int max,
            Action<int> apply)
        {
            int start = current;
            if (start < min)
            {
                start = min;
            }

            if (start > max)
            {
                start = max;
            }

            Find.WindowStack.Add(new Dialog_Slider(title, min, max, apply, start));
        }

        // --- Inspect ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Print Campaign State",
            allowedGameStates = AllowedGameStates.PlayingOnMap,
            displayPriority = 500)]
        private static void PrintCampaignState()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            Log.Message("[The Ark]\n" + FormatFields(campaign));
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Log Save/Load Test Snapshot",
            allowedGameStates = AllowedGameStates.PlayingOnMap,
            displayPriority = 490)]
        private static void LogSaveLoadTestSnapshot()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            Log.Message(FormatSaveTestSnapshot(campaign));
        }

        // --- Activate / deactivate ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Activate Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ActivateCampaign()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            bool previous = campaign.CampaignActive;
            campaign.CampaignActive = true;
            Log.Message("[The Ark] CampaignActive changed: " + previous + " -> true");
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Deactivate Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DeactivateCampaign()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            bool previous = campaign.CampaignActive;
            campaign.CampaignActive = false;
            Log.Message("[The Ark] CampaignActive changed: " + previous + " -> false");
        }

        // --- CampaignDay ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Advance Campaign Day",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AdvanceCampaignDay()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            int previous = campaign.CampaignDay;
            campaign.CampaignDay = previous + 1;
            Log.Message("[The Ark] CampaignDay changed: " + previous + " -> " + campaign.CampaignDay);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Advance Campaign Day +10",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AdvanceCampaignDayPlus10()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            int previous = campaign.CampaignDay;
            campaign.CampaignDay = previous + 10;
            Log.Message("[The Ark] CampaignDay changed: " + previous + " -> " + campaign.CampaignDay);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Set Campaign Day",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetCampaignDay()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            PromptInt(
                "Set CampaignDay",
                campaign.CampaignDay,
                0,
                CampaignDaySliderMax,
                value =>
                {
                    if (!TryGetCampaign(out ArkCampaignGameComponent live))
                    {
                        return;
                    }

                    int clamped = Math.Max(0, value);
                    int previous = live.CampaignDay;
                    live.CampaignDay = clamped;
                    Log.Message("[The Ark] CampaignDay changed: " + previous + " -> " + live.CampaignDay);
                });
        }

        // --- LandingNumber ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Set Landing Number",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetLandingNumber()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            PromptInt(
                "Set LandingNumber",
                campaign.LandingNumber,
                0,
                LandingNumberSliderMax,
                value =>
                {
                    if (!TryGetCampaign(out ArkCampaignGameComponent live))
                    {
                        return;
                    }

                    int clamped = Math.Max(0, value);
                    int previous = live.LandingNumber;
                    live.LandingNumber = clamped;
                    Log.Message("[The Ark] LandingNumber changed: " + previous + " -> " + live.LandingNumber);
                });
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Increment Landing Number",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void IncrementLandingNumber()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            int previous = campaign.LandingNumber;
            campaign.LandingNumber = previous + 1;
            Log.Message("[The Ark] LandingNumber changed: " + previous + " -> " + campaign.LandingNumber);
        }

        // --- ArkTier ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Set Ark Tier",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetArkTier()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            PromptInt(
                "Set ArkTier",
                campaign.ArkTier,
                0,
                ArkTierSliderMax,
                value =>
                {
                    if (!TryGetCampaign(out ArkCampaignGameComponent live))
                    {
                        return;
                    }

                    int clamped = Math.Max(0, value);
                    int previous = live.ArkTier;
                    live.ArkTier = clamped;
                    Log.Message("[The Ark] ArkTier changed: " + previous + " -> " + live.ArkTier);
                });
        }

        // --- Pursuit ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Set Pursuit",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetPursuit()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            PromptInt(
                "Set Pursuit",
                campaign.Pursuit,
                ArkPursuit.Min,
                ArkPursuit.Max,
                value =>
                {
                    if (!TryGetCampaign(out ArkCampaignGameComponent live))
                    {
                        return;
                    }

                    int previous = live.Pursuit;
                    live.SetPursuit(value, "Debug.SetPursuit", notifyBand: true);
                    Log.Message("[The Ark] Pursuit changed: " + previous + " -> " + live.Pursuit);
                });
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Pursuit +10",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PursuitPlus10()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            int previous = campaign.Pursuit;
            campaign.SetPursuit(previous + 10, "Debug.PursuitPlus10", notifyBand: true);
            Log.Message("[The Ark] Pursuit changed: " + previous + " -> " + campaign.Pursuit);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Pursuit -10",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PursuitMinus10()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            int previous = campaign.Pursuit;
            int next = Math.Max(0, previous - 10);
            campaign.SetPursuit(next, "Debug.PursuitMinus10", notifyBand: true);
            Log.Message("[The Ark] Pursuit changed: " + previous + " -> " + campaign.Pursuit);
        }

        // --- Artificial test presets (not canonical balance) ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Preset — Fresh Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetFreshCampaign()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            // Artificial developer fixture — not balance.
            ApplyPreset(campaign, "Fresh Campaign", true, 0, 0, 0, 0);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Preset — Early Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetEarlyCampaign()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            // Artificial developer fixture — not balance.
            ApplyPreset(campaign, "Early Campaign", true, 10, 1, 1, 20);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Preset — Mid Campaign",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetMidCampaign()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            // Artificial developer fixture — not balance.
            ApplyPreset(campaign, "Mid Campaign", true, 40, 3, 2, 50);
        }

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: Preset — High Pursuit",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetHighPursuit()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            // Artificial developer fixture — not balance.
            ApplyPreset(campaign, "High Pursuit", true, 60, 4, 3, 90);
        }

        // --- Reset (destructive) ---

        [DebugAction(
            category = "The Ark (DEV)",
            name = "ARK: !!! RESET Campaign State !!!",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetCampaignState()
        {
            if (!TryGetCampaign(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "Reset ALL Ark campaign fields to defaults?\n\n" +
                "CampaignActive=false, CampaignDay=0, LandingNumber=0, ArkTier=0, Pursuit=0\n\n" +
                "Landing session / timer fields are not cleared by this action.",
                () =>
                {
                    if (!TryGetCampaign(out ArkCampaignGameComponent live))
                    {
                        return;
                    }

                    live.CampaignActive = false;
                    live.CampaignDay = 0;
                    live.LandingNumber = 0;
                    live.ArkTier = 0;
                    live.SetPursuit(0, "Debug.Reset", notifyBand: false);
                    Log.Message("[The Ark] RESET Campaign State applied:\n" + FormatFields(live));
                },
                destructive: true,
                title: "ARK: !!! RESET Campaign State !!!"));
        }
    }
}
