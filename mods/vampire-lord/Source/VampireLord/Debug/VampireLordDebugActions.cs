using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using VampireLord.Campaign;
using VampireLord.Scenario;
using Verse;

namespace VampireLord.Debug
{
    /// <summary>
    /// Dev Mode / Quicktest harness. Search Debug Actions for <c>VL:</c>.
    /// Mutates production Vampire Lord systems only — never the reverse.
    /// </summary>
    public static class VampireLordDebugActions
    {
        private const string Cat = "Vampire Lord";

        // --- State / inspection ---

        [DebugAction(category = Cat, name = "VL: Print State", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PrintState()
        {
            VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign, warn: false);
            Log.Message(VampireLordDebugHelpers.FormatCompactState(campaign));
        }

        [DebugAction(category = Cat, name = "VL: Print Pawn State", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PrintPawnState()
        {
            if (!VampireLordDebugHelpers.TryGetSelectedPawn(out Pawn pawn))
            {
                return;
            }

            Log.Message(VampireLordDebugHelpers.FormatPawnState(pawn));
        }

        [DebugAction(category = Cat, name = "VL: Log Save Test Snapshot", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogSaveTestSnapshot()
        {
            VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign, warn: false);
            Log.Message(VampireLordDebugHelpers.FormatSaveTestSnapshot(campaign));
        }

        // --- Campaign control (production Wave Director) ---

        [DebugAction(category = Cat, name = "VL: Start Campaign", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StartCampaign()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordWaveDirector.ActivateCampaign(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Stop Campaign", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StopCampaign()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordWaveDirector.StopCampaign(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Ensure Player Home", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void EnsurePlayerHome()
        {
            VampireLordPlayerHome.TryEnsure("DebugAction");
        }

        [DebugAction(category = Cat, name = "VL: Ensure Prep Fixture", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void EnsurePrepFixture()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordPlaytest.EnsurePrepFixture(campaign);
        }

        [DebugAction(
            category = Cat,
            name = "VL: !!! RESET VampireLord Test State !!!",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetTestState()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.ResetCampaignToDefaults(campaign);
            Log.Message(VampireLordDebugHelpers.FormatCompactState(campaign));
        }

        // --- Pawn combat reset / hemogen / deathrest ---

        [DebugAction(category = Cat, name = "VL: Restore Vampire Lord", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RestoreVampireLord()
        {
            if (!VampireLordDebugHelpers.TryGetSelectedPawn(out Pawn pawn))
            {
                return;
            }

            VampireLordDebugHelpers.RestoreVampireCombat(pawn);
        }

        [DebugAction(category = Cat, name = "VL: Fill Blood / Hemogen", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void FillHemogen()
        {
            if (!VampireLordDebugHelpers.TryGetSelectedPawn(out Pawn pawn))
            {
                return;
            }

            if (!VampireLordDebugHelpers.TryFillHemogen(pawn, out string message))
            {
                Log.Warning($"{VampireLordDebugHelpers.LogPrefix} Cannot fill hemogen: {message}");
            }
        }

        [DebugAction(category = Cat, name = "VL: Empty Blood / Hemogen", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void EmptyHemogen()
        {
            if (!VampireLordDebugHelpers.TryGetSelectedPawn(out Pawn pawn))
            {
                return;
            }

            if (!VampireLordDebugHelpers.TryEmptyHemogen(pawn, out string message))
            {
                Log.Warning($"{VampireLordDebugHelpers.LogPrefix} Cannot empty hemogen: {message}");
            }
        }

        [DebugAction(category = Cat, name = "VL: Force Deathrest Ready", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ForceDeathrestReady()
        {
            if (!VampireLordDebugHelpers.TryGetSelectedPawn(out Pawn pawn))
            {
                return;
            }

            if (!VampireLordDebugHelpers.TryForceDeathrestReady(pawn, out string message))
            {
                Log.Warning($"{VampireLordDebugHelpers.LogPrefix} Cannot force deathrest: {message}");
            }
        }

        [DebugAction(category = Cat, name = "VL: Fill Keep Blood", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void FillKeepBlood()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            int old = campaign.BloodReserve;
            int target = VampireLordTuning.StartingBloodReserve;
            if (campaign.BloodReserve < target)
            {
                VampireLordBloodTithe.AddBlood(campaign, target - campaign.BloodReserve, "debug fill keep blood");
            }

            Log.Message($"{VampireLordDebugHelpers.LogPrefix} Keep Blood: {old} -> {campaign.BloodReserve}");
        }

        [DebugAction(category = Cat, name = "VL: Empty Keep Blood", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void EmptyKeepBlood()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            int old = campaign.BloodReserve;
            VampireLordBloodTithe.ForceLowBlood(campaign);
            Log.Message($"{VampireLordDebugHelpers.LogPrefix} Keep Blood: {old} -> {campaign.BloodReserve}");
        }

        [DebugAction(category = Cat, name = "VL: Add Keep Blood (+20)", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddKeepBlood()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordBloodTithe.AddBlood(campaign, 20, "debug");
        }

        [DebugAction(category = Cat, name = "VL: Spend Keep Blood (-20)", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpendKeepBlood()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordBloodTithe.SpendBlood(campaign, 20, "debug");
        }

        // --- Time ---

        [DebugAction(category = Cat, name = "VL: Start Night", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StartNight()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            VampireLordDebugHelpers.TrySetMapHour(map, 22, out _);
        }

        [DebugAction(category = Cat, name = "VL: Start Dawn", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void StartDawn()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            VampireLordDebugHelpers.TrySetMapHour(map, 6, out _);
        }

        [DebugAction(category = Cat, name = "VL: Midnight", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Midnight()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            VampireLordDebugHelpers.TrySetMapHour(map, 0, out _);
        }

        // --- Waves (production Wave Director / RaidLauncher) ---

        [DebugAction(category = Cat, name = "VL: Trigger Warning Now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TriggerWarningNow()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordWaveDirector.TriggerWarningNow(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Spawn Next Campaign Wave", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnNextCampaignWave()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.EnsureCampaignActive(campaign);
            Log.Message(
                $"{VampireLordDebugHelpers.WaveTestPrefix} Triggering wave {campaign.UpcomingWaveNumber} " +
                $"(production TriggerWaveNow).");
            VampireLordWaveDirector.TriggerWaveNow(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Spawn Small Attack Wave", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnSmallAttackWave()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.TriggerSizedWave(campaign, VampireLordTuning.InitialThreatLevel, "small");
        }

        [DebugAction(category = Cat, name = "VL: Spawn Medium Attack Wave", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnMediumAttackWave()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.TriggerSizedWave(campaign, 4, "medium");
        }

        [DebugAction(category = Cat, name = "VL: Spawn Heavy Attack Wave", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnHeavyAttackWave()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.TriggerSizedWave(campaign, 8, "heavy");
        }

        [DebugAction(category = Cat, name = "VL: End Current Wave", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void EndCurrentWave()
        {
            // VL advances wave state on raid launch (OnWaveDispatched). There is no separate
            // "wave victory" production path — clear hostiles so combat can end.
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            Log.Message(
                $"{VampireLordDebugHelpers.WaveTestPrefix} End Current Wave: VL has no separate " +
                "completion rewards; killing hostile humanlikes via normal death.");
            VampireLordDebugHelpers.KillHostileHumanlikes(map);
        }

        [DebugAction(category = Cat, name = "VL: Kill Current Wave Enemies", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void KillCurrentWaveEnemies()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            VampireLordDebugHelpers.KillHostileHumanlikes(map);
        }

        [DebugAction(category = Cat, name = "VL: Remove Current Wave Enemies", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveCurrentWaveEnemies()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            VampireLordDebugHelpers.RemoveHostileHumanlikes(map);
        }

        // --- Threat ---

        [DebugAction(category = Cat, name = "VL: Threat +10", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ThreatPlus10()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.SetThreat(campaign, campaign.ThreatLevel + 10);
        }

        [DebugAction(category = Cat, name = "VL: Threat -10", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ThreatMinus10()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.SetThreat(campaign, campaign.ThreatLevel - 10);
        }

        [DebugAction(category = Cat, name = "VL: Reset Threat", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetThreat()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugHelpers.SetThreat(campaign, VampireLordTuning.InitialThreatLevel);
        }

        [DebugAction(category = Cat, name = "VL: Set Threat", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetThreat()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            var options = new List<int> { 1, 2, 3, 4, 5, 6, 8, 10, 12, 15 };
            Dialog_DebugOptionListLister.ShowSimpleDebugMenu(
                options,
                t => $"Threat {t} (raid pts ~{VampireLordWaveDirector.RaidPointsFor(t):0})",
                t => VampireLordDebugHelpers.SetThreat(campaign, t));
        }

        // --- Keep / fortify ---

        [DebugAction(category = Cat, name = "VL: Refill Defences", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RefillDefences()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            // V0 defences are courtyard walls + gate sandbags (no turret ammo system yet).
            VampireLordDebugHelpers.RepairKeepBuildings(map);
            if (VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign, warn: false)
                && VampireLordFortify.IsPrepWindow(campaign))
            {
                VampireLordFortify.TryPurchase(campaign, forced: true, reason: "debug refill");
            }
        }

        [DebugAction(category = Cat, name = "VL: Repair Castle", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RepairCastle()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            VampireLordDebugHelpers.RepairKeepBuildings(map);
        }

        [DebugAction(category = Cat, name = "VL: Damage Castle to 50%", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DamageCastleToHalf()
        {
            if (!VampireLordDebugHelpers.TryGetMap(out Map map))
            {
                return;
            }

            VampireLordDebugHelpers.DamageKeepBuildingsToHalf(map);
        }

        [DebugAction(category = Cat, name = "VL: Offer Fortify", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void OfferFortify()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordFortify.TrySendOfferLetter(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Force Fortify Now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ForceFortifyNow()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordFortify.TryPurchase(campaign, forced: true, reason: "debug");
        }

        [DebugAction(category = Cat, name = "VL: Show Fortify State", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ShowFortifyState()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            Log.Message($"{VampireLordDebugHelpers.LogPrefix} Fortify state:\n{VampireLordFortify.FormatState(campaign)}");
        }

        // --- Playtest toggles ---

        [DebugAction(category = Cat, name = "VL: Toggle Auto-Fortify", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleAutoFortify()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordPlaytest.ToggleAutoFortify(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Toggle Playtest Pace", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TogglePlaytestPace()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordPlaytest.TogglePlaytestPace(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Toggle Quiet Letters", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleQuietLetters()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordPlaytest.ToggleQuietLetters(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Toggle Campaign HUD", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleCampaignHud()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            campaign.ShowCampaignHud = !campaign.ShowCampaignHud;
            Log.Message(
                $"{VampireLordDebugHelpers.LogPrefix} Campaign HUD {(campaign.ShowCampaignHud ? "ON" : "OFF")}.");
        }

        // --- Presets (test fixtures) ---

        [DebugAction(category = Cat, name = "VL: Preset — Fresh Lord", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetFreshLord()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugPresets.ApplyFreshLord(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Preset — Early Siege", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetEarlySiege()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugPresets.ApplyEarlySiege(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Preset — Mid Siege", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetMidSiege()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugPresets.ApplyMidSiege(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Preset — Last Stand", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetLastStand()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugPresets.ApplyLastStand(campaign);
        }

        [DebugAction(category = Cat, name = "VL: Preset — Near Defeat", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PresetNearDefeat()
        {
            if (!VampireLordDebugHelpers.TryGetCampaign(out VampireLordCampaignGameComponent campaign))
            {
                return;
            }

            VampireLordDebugPresets.ApplyNearDefeat(campaign);
        }
    }
}
