using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Schedules waves, picks archetypes, issues warnings, and advances threat after raids.
    /// </summary>
    public static class VampireLordWaveDirector
    {
        public static void ActivateCampaign(VampireLordCampaignGameComponent campaign)
        {
            campaign.CampaignActive = true;
            campaign.WaveNumber = 0;
            campaign.ThreatLevel = VampireLordTuning.InitialThreatLevel;
            campaign.WarningIssued = false;
            campaign.WavePending = false;
            campaign.SameArchetypeStreak = 0;
            // Less-fussy playtest defaults (can toggle off in Dev Mode).
            campaign.AutoFortifyPlaytest = true;
            campaign.PlaytestPace = true;
            campaign.PlaytestQuietLetters = true;
            VampireLordBloodTithe.ResetSessionCredits();
            VampireLordBloodTithe.EnsureStartingReserve(campaign);
            ScheduleNextWave(campaign);
            VampireLordFortify.NotifyPrepWindowOpened(campaign);
            Log.Message(
                $"[VampireLord] Campaign activated. Blood={campaign.BloodReserve}, " +
                $"AutoFortify=ON, Pace={campaign.EffectiveDaysBetweenWaves:0.##}d between waves, QuietLetters=ON.");
        }

        public static void StopCampaign(VampireLordCampaignGameComponent campaign)
        {
            campaign.CampaignActive = false;
            campaign.WavePending = false;
            // Progress (WaveNumber / ThreatLevel / LastWaveType) intentionally retained.
            Log.Message("[VampireLord] Campaign stopped.");
        }

        public static void ScheduleNextWave(VampireLordCampaignGameComponent campaign)
        {
            int now = Find.TickManager.TicksGame;
            int between = DaysToTicks(campaign.EffectiveDaysBetweenWaves);
            int lead = DaysToTicks(campaign.EffectiveWarningLeadDays);

            campaign.PendingWaveType = SelectArchetype(campaign);
            campaign.NextWaveTick = now + between;
            campaign.WarningTick = campaign.NextWaveTick - lead;
            if (campaign.WarningTick < now)
            {
                campaign.WarningTick = now;
            }

            campaign.WarningIssued = false;
            campaign.WavePending = true;

            Log.Message(
                $"[VampireLord] Scheduled Wave {campaign.UpcomingWaveNumber}: {campaign.PendingWaveType}.");
        }

        public static void EvaluateSchedule(VampireLordCampaignGameComponent campaign)
        {
            if (!campaign.CampaignActive || !campaign.WavePending)
            {
                return;
            }

            int now = Find.TickManager.TicksGame;

            if (!campaign.WarningIssued && now >= campaign.WarningTick)
            {
                IssueWarning(campaign);
            }

            if (now >= campaign.NextWaveTick)
            {
                TryTriggerPendingWave(campaign, fromSchedule: true);
            }
        }

        public static void IssueWarning(VampireLordCampaignGameComponent campaign)
        {
            if (!campaign.WavePending)
            {
                EnsurePendingWave(campaign);
            }

            VampireLordLetters.SendAdvanceWarning(campaign);
            campaign.WarningIssued = true;
            Log.Message($"[VampireLord] Warning issued for Wave {campaign.UpcomingWaveNumber}.");
        }

        public static void TriggerWarningNow(VampireLordCampaignGameComponent campaign)
        {
            if (!campaign.CampaignActive)
            {
                Log.Warning("[VampireLord] Cannot trigger warning — campaign is not active.");
                return;
            }

            EnsurePendingWave(campaign);
            // Pull warning/wave timing forward so the schedule stays consistent after a forced warning.
            int now = Find.TickManager.TicksGame;
            campaign.WarningTick = now;
            campaign.NextWaveTick = now + DaysToTicks(campaign.EffectiveWarningLeadDays);
            IssueWarning(campaign);
        }

        public static void TriggerWaveNow(VampireLordCampaignGameComponent campaign)
        {
            if (!campaign.CampaignActive)
            {
                Log.Warning("[VampireLord] Cannot trigger wave — campaign is not active.");
                return;
            }

            EnsurePendingWave(campaign);
            TryTriggerPendingWave(campaign, fromSchedule: false);
        }

        public static void EnsurePendingWave(VampireLordCampaignGameComponent campaign)
        {
            if (campaign.WavePending)
            {
                return;
            }

            ScheduleNextWave(campaign);
        }

        public static bool TryTriggerPendingWave(VampireLordCampaignGameComponent campaign, bool fromSchedule)
        {
            if (!campaign.WavePending)
            {
                EnsurePendingWave(campaign);
            }

            // Harvest letter for kills since previous wave, then pay the tithe for this assault.
            VampireLordBloodTithe.NotifyWaveHarvest(campaign);
            float starvedMult = VampireLordBloodTithe.ApplyWaveCost(campaign);

            float points = RaidPointsFor(campaign.ThreatLevel) * starvedMult;
            Log.Message(
                $"[VampireLord] Triggering Wave {campaign.UpcomingWaveNumber} at {points:0} raid points " +
                $"(blood x{starvedMult:0.00}).");

            bool ok = VampireLordRaidLauncher.TryLaunchRaid(campaign.PendingWaveType, points, out string failReason);
            if (!ok)
            {
                Log.Warning(
                    $"[VampireLord] Wave {campaign.UpcomingWaveNumber} failed to launch ({failReason}). Retrying later.");
                // Refund spent tithe so a failed launch does not drain the keep.
                if (campaign.LastWaveBloodSpent > 0)
                {
                    VampireLordBloodTithe.AddBlood(
                        campaign,
                        campaign.LastWaveBloodSpent,
                        "refund failed wave launch");
                    campaign.LastWaveBloodSpent = 0;
                    campaign.LastWaveBloodStarved = false;
                }

                campaign.NextWaveTick = Find.TickManager.TicksGame + VampireLordTuning.RaidRetryDelayTicks;
                if (!campaign.WarningIssued)
                {
                    campaign.WarningTick = Find.TickManager.TicksGame;
                }

                return false;
            }

            OnWaveDispatched(campaign);
            return true;
        }

        public static void OnWaveDispatched(VampireLordCampaignGameComponent campaign)
        {
            VampireLordWaveType dispatched = campaign.PendingWaveType;
            if (dispatched == campaign.LastWaveType)
            {
                campaign.SameArchetypeStreak++;
            }
            else
            {
                campaign.SameArchetypeStreak = 1;
                campaign.LastWaveType = dispatched;
            }

            campaign.WavePending = false;
            campaign.WarningIssued = false;
            campaign.WaveNumber++;
            campaign.ThreatLevel++;
            campaign.BloodGainedSinceWave = 0;

            Log.Message(
                $"[VampireLord] Wave {campaign.WaveNumber} dispatched. Threat level is now {campaign.ThreatLevel}. " +
                $"Blood Reserve={campaign.BloodReserve}.");

            ScheduleNextWave(campaign);
            VampireLordFortify.NotifyPrepWindowOpened(campaign);
        }

        public static float RaidPointsFor(int threatLevel)
        {
            return VampireLordTuning.BaseRaidPoints +
                   (threatLevel * VampireLordTuning.RaidPointsPerThreatLevel);
        }

        public static VampireLordWaveType SelectArchetype(VampireLordCampaignGameComponent campaign)
        {
            int upcoming = campaign.UpcomingWaveNumber;
            List<VampireLordWaveType> pool = BuildPool(upcoming);

            if (campaign.SameArchetypeStreak >= VampireLordTuning.MaxSameArchetypeInARow)
            {
                pool.RemoveAll(t => t == campaign.LastWaveType);
                if (pool.Count == 0)
                {
                    pool = BuildPool(upcoming);
                }
            }

            // Deterministic-enough pick: rotate by completed wave count within the allowed pool.
            int index = campaign.WaveNumber % pool.Count;
            return pool[index];
        }

        private static List<VampireLordWaveType> BuildPool(int upcomingWaveNumber)
        {
            var pool = new List<VampireLordWaveType>
            {
                VampireLordWaveType.Mob,
                VampireLordWaveType.Hunters
            };

            if (upcomingWaveNumber >= VampireLordTuning.MinWaveForBreachers)
            {
                pool.Add(VampireLordWaveType.Breachers);
            }

            if (upcomingWaveNumber >= VampireLordTuning.MinWaveForFire)
            {
                pool.Add(VampireLordWaveType.Fire);
            }

            if (upcomingWaveNumber >= VampireLordTuning.MinWaveForSiege)
            {
                pool.Add(VampireLordWaveType.Siege);
            }

            return pool;
        }

        public static string FormatState(VampireLordCampaignGameComponent campaign)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CampaignActive={campaign.CampaignActive}");
            sb.AppendLine($"WaveNumber={campaign.WaveNumber}");
            sb.AppendLine($"UpcomingWaveNumber={campaign.UpcomingWaveNumber}");
            sb.AppendLine($"ThreatLevel={campaign.ThreatLevel}");
            sb.AppendLine($"PendingWaveType={campaign.PendingWaveType}");
            sb.AppendLine($"WarningIssued={campaign.WarningIssued}");
            sb.AppendLine($"WavePending={campaign.WavePending}");
            sb.AppendLine($"WarningTick={campaign.WarningTick}");
            sb.AppendLine($"NextWaveTick={campaign.NextWaveTick}");
            sb.AppendLine($"TicksUntilWarning={campaign.TicksUntilWarning}");
            sb.AppendLine($"TicksUntilWave={campaign.TicksUntilWave}");
            sb.AppendLine($"LastWaveType={campaign.LastWaveType}");
            sb.AppendLine($"SameArchetypeStreak={campaign.SameArchetypeStreak}");
            sb.AppendLine($"RaidPointsNext={RaidPointsFor(campaign.ThreatLevel):0}");
            sb.AppendLine($"BloodReserve={campaign.BloodReserve}");
            sb.AppendLine($"BloodGainedSinceWave={campaign.BloodGainedSinceWave}");
            sb.AppendLine($"WaveBloodCostNext={VampireLordBloodTithe.WaveBloodCost(campaign)}");
            sb.AppendLine($"LastWaveBloodStarved={campaign.LastWaveBloodStarved}");
            sb.AppendLine($"LastWaveBloodSpent={campaign.LastWaveBloodSpent}/{campaign.LastWaveBloodCost}");
            sb.AppendLine($"FortifyPrepWindow={VampireLordFortify.IsPrepWindow(campaign)}");
            sb.AppendLine($"FortifyPurchasesThisWindow={campaign.FortifyPurchasesThisWindow}/{VampireLordTuning.FortifyMaxPerPrepWindow}");
            sb.AppendLine($"LastFortifyWaveNumber={campaign.LastFortifyWaveNumber}");
            sb.AppendLine($"AutoFortifyPlaytest={campaign.AutoFortifyPlaytest}");
            sb.AppendLine($"PlaytestPace={campaign.PlaytestPace} ({campaign.EffectiveDaysBetweenWaves:0.##}d / warn {campaign.EffectiveWarningLeadDays:0.##}d)");
            sb.AppendLine($"PlaytestQuietLetters={campaign.PlaytestQuietLetters}");
            sb.AppendLine($"ShowCampaignHud={campaign.ShowCampaignHud}");
            return sb.ToString().TrimEnd();
        }

        private static int DaysToTicks(float days)
        {
            return (int)(days * GenDate.TicksPerDay);
        }
    }
}
