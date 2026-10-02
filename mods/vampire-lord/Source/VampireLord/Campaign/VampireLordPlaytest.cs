using System.Text;
using RimWorld;
using VampireLord.Scenario;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Faster, less-fussy playtest defaults: fixture save, auto-fortify, fast pace, quiet letters.
    /// </summary>
    public static class VampireLordPlaytest
    {
        private const string LogPrefix = "[VampireLord]";

        /// <summary>
        /// Put the campaign back into a between-wave prep window with enough blood to fortify.
        /// Also reapplies less-fussy playtest defaults.
        /// </summary>
        public static void EnsurePrepFixture(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return;
            }

            ApplyLessFussyDefaults(campaign);

            if (!campaign.CampaignActive)
            {
                VampireLordWaveDirector.ActivateCampaign(campaign);
                // Activate already applied defaults + scheduled prep; still top up below.
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
            int between = (int)(campaign.EffectiveDaysBetweenWaves * GenDate.TicksPerDay);
            int lead = (int)(campaign.EffectiveWarningLeadDays * GenDate.TicksPerDay);

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
                $"AutoFortify={campaign.AutoFortifyPlaytest}, Pace={campaign.PlaytestPace}, " +
                $"Quiet={campaign.PlaytestQuietLetters}. Save as VL_prep.");
        }

        public static void ApplyLessFussyDefaults(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return;
            }

            campaign.AutoFortifyPlaytest = true;
            campaign.PlaytestPace = true;
            campaign.PlaytestQuietLetters = true;
        }

        public static void SetAutoFortify(VampireLordCampaignGameComponent campaign, bool enabled)
        {
            if (campaign == null)
            {
                return;
            }

            campaign.AutoFortifyPlaytest = enabled;
            Log.Message(
                $"{LogPrefix} Auto-Fortify {(enabled ? "ON" : "OFF")} " +
                (enabled ? "(no Accept letter)." : "(Accept letter returns)."));
        }

        public static void ToggleAutoFortify(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return;
            }

            SetAutoFortify(campaign, !campaign.AutoFortifyPlaytest);
        }

        public static void TogglePlaytestPace(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return;
            }

            campaign.PlaytestPace = !campaign.PlaytestPace;
            Log.Message(
                $"{LogPrefix} Playtest Pace {(campaign.PlaytestPace ? "ON" : "OFF")} " +
                $"({campaign.EffectiveDaysBetweenWaves:0.##}d between waves, " +
                $"{campaign.EffectiveWarningLeadDays:0.##}d warning).");
        }

        public static void ToggleQuietLetters(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return;
            }

            campaign.PlaytestQuietLetters = !campaign.PlaytestQuietLetters;
            Log.Message(
                $"{LogPrefix} Quiet Letters {(campaign.PlaytestQuietLetters ? "ON" : "OFF")} " +
                "(harvest/fortify-complete suppressed when ON; warnings+tithe always stay).");
        }

        public static string FormatFixtureChecklist(VampireLordCampaignGameComponent campaign)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Vampire Lord — less-fussy playtest ===");
            sb.AppendLine("Defaults ON at campaign start: AutoFortify, fast Pace (1d waves), Quiet letters.");
            sb.AppendLine();
            sb.AppendLine("ONCE:");
            sb.AppendLine("  New Game → Vampire Lord → wait for keep → save VL_prep");
            sb.AppendLine("  (or Dev Mode → Ensure Prep Fixture → save VL_prep)");
            sb.AppendLine();
            sb.AppendLine("EACH CHANGE:");
            sb.AppendLine("  Restart → load VL_prep → Trigger Wave / Force Fortify as needed");
            sb.AppendLine();
            sb.AppendLine("CURRENT:");
            if (campaign == null)
            {
                sb.AppendLine("  (no campaign)");
            }
            else
            {
                sb.AppendLine($"  Active={campaign.CampaignActive}");
                sb.AppendLine($"  AutoFortify={campaign.AutoFortifyPlaytest}");
                sb.AppendLine($"  Pace={campaign.PlaytestPace} ({campaign.EffectiveDaysBetweenWaves:0.##}d)");
                sb.AppendLine($"  QuietLetters={campaign.PlaytestQuietLetters}");
                sb.AppendLine($"  Blood={campaign.BloodReserve}");
                sb.AppendLine($"  PrepWindow={VampireLordFortify.IsPrepWindow(campaign)}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
