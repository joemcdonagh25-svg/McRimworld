using RimWorld;
using VampireLord.Campaign;
using VampireLord.Scenario;
using Verse;

namespace VampireLord.Debug
{
    /// <summary>
    /// One-click Dev Mode fixtures. These are test presets — not balance declarations.
    /// They mutate the authoritative <see cref="VampireLordCampaignGameComponent"/> via production APIs.
    /// </summary>
    internal static class VampireLordDebugPresets
    {
        private const string LogPrefix = VampireLordDebugHelpers.LogPrefix;

        /// <summary>TEST FIXTURE — earliest useful VL state.</summary>
        public static void ApplyFreshLord(VampireLordCampaignGameComponent campaign)
        {
            VampireLordDebugHelpers.EnsureCampaignActive(campaign);
            VampireLordPlaytest.EnsurePrepFixture(campaign);
            VampireLordDebugHelpers.SetThreat(campaign, VampireLordTuning.InitialThreatLevel);
            campaign.WaveNumber = 0;
            VampireLordWaveDirector.ScheduleNextWave(campaign);
            VampireLordFortify.NotifyPrepWindowOpened(campaign);

            if (VampireLordDebugHelpers.TryGetMap(out Map map, warn: false))
            {
                VampireLordDebugHelpers.TrySetMapHour(map, 22, out _);
                VampireLordDebugHelpers.RepairKeepBuildings(map);
            }

            if (VampireLordDebugHelpers.TryGetVampireLord(out Pawn lord, warn: false))
            {
                VampireLordDebugHelpers.RestoreVampireCombat(lord);
            }

            VampireLordPlayerHome.TryEnsure("PresetFreshLord");
            Log.Message($"{LogPrefix} Preset — Fresh Lord applied (night prep, low threat, no active combat).");
            Log.Message(VampireLordDebugHelpers.FormatCompactState(campaign));
        }

        /// <summary>TEST FIXTURE — early defensive combat.</summary>
        public static void ApplyEarlySiege(VampireLordCampaignGameComponent campaign)
        {
            VampireLordDebugHelpers.EnsureCampaignActive(campaign);
            VampireLordPlaytest.ApplyLessFussyDefaults(campaign);
            VampireLordBloodTithe.EnsureStartingReserve(campaign);
            campaign.WaveNumber = 0;
            VampireLordDebugHelpers.SetThreat(campaign, 2);

            if (VampireLordDebugHelpers.TryGetMap(out Map map, warn: false))
            {
                VampireLordDebugHelpers.TrySetMapHour(map, 22, out _);
                VampireLordDebugHelpers.RepairKeepBuildings(map);
            }

            if (VampireLordDebugHelpers.TryGetVampireLord(out Pawn lord, warn: false))
            {
                VampireLordDebugHelpers.RestoreVampireCombat(lord);
            }

            VampireLordPlayerHome.TryEnsure("PresetEarlySiege");
            Log.Message($"{LogPrefix} Preset — Early Siege: launching small production wave.");
            VampireLordDebugHelpers.TriggerSizedWave(campaign, 2, "early siege");
        }

        /// <summary>TEST FIXTURE — mid-game combat pressure.</summary>
        public static void ApplyMidSiege(VampireLordCampaignGameComponent campaign)
        {
            VampireLordDebugHelpers.EnsureCampaignActive(campaign);
            VampireLordPlaytest.ApplyLessFussyDefaults(campaign);
            VampireLordBloodTithe.EnsureStartingReserve(campaign);
            campaign.WaveNumber = 3;
            VampireLordDebugHelpers.SetThreat(campaign, 5);

            if (VampireLordDebugHelpers.TryGetMap(out Map map, warn: false))
            {
                VampireLordDebugHelpers.TrySetMapHour(map, 22, out _);
                VampireLordDebugHelpers.DamageKeepBuildingsToHalf(map);
            }

            if (VampireLordDebugHelpers.TryGetVampireLord(out Pawn lord, warn: false))
            {
                VampireLordDebugHelpers.RestoreVampireCombat(lord);
            }

            VampireLordPlayerHome.TryEnsure("PresetMidSiege");
            Log.Message($"{LogPrefix} Preset — Mid Siege: launching medium production wave.");
            VampireLordDebugHelpers.TriggerSizedWave(campaign, 5, "mid siege");
        }

        /// <summary>TEST FIXTURE — Dracula's last stand mood: midnight, high threat, large assault.</summary>
        public static void ApplyLastStand(VampireLordCampaignGameComponent campaign)
        {
            VampireLordDebugHelpers.EnsureCampaignActive(campaign);
            VampireLordPlaytest.ApplyLessFussyDefaults(campaign);
            // Enough blood to pay one tithe, not a comfortable cushion.
            campaign.BloodReserve = VampireLordBloodTithe.WaveBloodCost(campaign) + 5;
            campaign.WaveNumber = 7;
            VampireLordDebugHelpers.SetThreat(campaign, 9);

            if (VampireLordDebugHelpers.TryGetMap(out Map map, warn: false))
            {
                VampireLordDebugHelpers.TrySetMapHour(map, 0, out _);
                VampireLordDebugHelpers.DamageKeepBuildingsToHalf(map);
            }

            if (VampireLordDebugHelpers.TryGetVampireLord(out Pawn lord, warn: false))
            {
                VampireLordDebugHelpers.RestoreVampireCombat(lord);
            }

            VampireLordPlayerHome.TryEnsure("PresetLastStand");
            Log.Message($"{LogPrefix} Preset — Last Stand: midnight, high threat, heavy production wave.");
            VampireLordDebugHelpers.TriggerSizedWave(campaign, 9, "last stand");
        }

        /// <summary>TEST FIXTURE — near-defeat inspection (does not auto-end the game).</summary>
        public static void ApplyNearDefeat(VampireLordCampaignGameComponent campaign)
        {
            VampireLordDebugHelpers.EnsureCampaignActive(campaign);
            VampireLordPlaytest.ApplyLessFussyDefaults(campaign);
            VampireLordBloodTithe.ForceLowBlood(campaign);
            campaign.WaveNumber = 6;
            VampireLordDebugHelpers.SetThreat(campaign, 8);

            if (VampireLordDebugHelpers.TryGetMap(out Map map, warn: false))
            {
                VampireLordDebugHelpers.TrySetMapHour(map, 0, out _);
                VampireLordDebugHelpers.DamageKeepBuildingsToHalf(map);
            }

            if (VampireLordDebugHelpers.TryGetVampireLord(out Pawn lord, warn: false))
            {
                // Injured pressure without wiping identity: empty hemogen only.
                // (No official VL injury-state API — keep temporary and reversible.)
                VampireLordDebugHelpers.TryEmptyHemogen(lord, out _);
            }

            VampireLordPlayerHome.TryEnsure("PresetNearDefeat");
            Log.Message($"{LogPrefix} Preset — Near Defeat: low blood, damaged keep, launching heavy wave.");
            VampireLordDebugHelpers.TriggerSizedWave(campaign, 8, "near defeat");
        }
    }
}
