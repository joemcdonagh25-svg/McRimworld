using System.Text;
using RimWorld;
using VampireLord.Scenario;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Faster playtest helpers: fixture-save workflow + optional auto-fortify
    /// so Joe does not click Accept letters while iterating.
    /// </summary>
    public static class VampireLordPlaytest
    {
        private const string LogPrefix = "[VampireLord]";

        /// <summary>
        /// Put the campaign back into a between-wave prep window with enough blood to fortify.
        /// Intended for the VL_prep fixture save — not a player-facing feature.
        /// </summary>
        public static void EnsurePrepFixture(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return;
            }

            if (!campaign.CampaignActive)
            {
                VampireLordWaveDirector.ActivateCampaign(campaign);
            }

            VampireLordBloodTithe.EnsureStartingReserve(campaign);
            if (campaign.BloodReserve < VampireLordTuning.FortifyBloodCost)
            {
                VampireLordBloodTithe.AddBlood(
                    campaign,
                    VampireLordTuning.FortifyBloodCost - campaign.BloodReserve,
                    "prep fixture top-up");
            }

            VampireLordPlayerHome.TryEnsure("PrepFixture");

            int now = Find.TickManager.TicksGame;
            int between = (int)(VampireLordTuning.DaysBetweenWaves * GenDate.TicksPerDay);
            int lead = (int)(VampireLordTuning.WarningLeadDays * GenDate.TicksPerDay);

            campaign.WavePending = true;
            campaign.WarningIssued = false;
            campaign.NextWaveTick = now + between;
            campaign.WarningTick = campaign.NextWaveTick - lead;
            if (campaign.WarningTick < now)
            {
                campaign.WarningTick = now + 1;
            }

            campaign.FortifyPurchasesThisWindow = 0;
            VampireLordFortify.NotifyPrepWindowOpened(campaign);

            Log.Message(
                $"{LogPrefix} Prep fixture ready. Blood={campaign.BloodReserve}, " +
                $"AutoFortify={campaign.AutoFortifyPlaytest}. " +
                "Save this game as VL_prep and reload it for fast iteration.");
        }

        public static void SetAutoFortify(VampireLordCampaignGameComponent campaign, bool enabled)
        {
            if (campaign == null)
            {
                return;
            }

            campaign.AutoFortifyPlaytest = enabled;
            Log.Message(
                $"{LogPrefix} Auto-Fortify Playtest {(enabled ? "ON" : "OFF")}. " +
                (enabled
                    ? "Prep windows will place sandbags automatically (no Accept letter)."
                    : "Prep windows will send the Blood for the Walls letter again."));
        }

        public static void ToggleAutoFortify(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return;
            }

            SetAutoFortify(campaign, !campaign.AutoFortifyPlaytest);
        }

        public static string FormatFixtureChecklist(VampireLordCampaignGameComponent campaign)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Vampire Lord fixture-save workflow ===");
            sb.AppendLine("ONCE:");
            sb.AppendLine("  1. New Game → Vampire Lord → let the keep load");
            sb.AppendLine("  2. Dev Mode → Vampire Lord → Ensure Prep Fixture");
            sb.AppendLine("  3. (Optional) Toggle Auto-Fortify Playtest ON");
            sb.AppendLine("  4. Save game as VL_prep");
            sb.AppendLine();
            sb.AppendLine("EACH CODE CHANGE:");
            sb.AppendLine("  1. Pull / rebuild DLL → restart RimWorld");
            sb.AppendLine("  2. Load VL_prep (skip New Game)");
            sb.AppendLine("  3. Use Force Fortify / Trigger Wave / Add Blood as needed");
            sb.AppendLine("  4. If prep window is messy → Ensure Prep Fixture again, re-save VL_prep");
            sb.AppendLine();
            sb.AppendLine("CURRENT:");
            if (campaign == null)
            {
                sb.AppendLine("  (no campaign component)");
            }
            else
            {
                sb.AppendLine($"  CampaignActive={campaign.CampaignActive}");
                sb.AppendLine($"  AutoFortifyPlaytest={campaign.AutoFortifyPlaytest}");
                sb.AppendLine($"  BloodReserve={campaign.BloodReserve}");
                sb.AppendLine($"  PrepWindow={VampireLordFortify.IsPrepWindow(campaign)}");
                sb.AppendLine($"  FortifyPurchases={campaign.FortifyPurchasesThisWindow}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
