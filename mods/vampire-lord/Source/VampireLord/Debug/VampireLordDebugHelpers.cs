using System.Collections.Generic;
using System.Text;
using RimWorld;
using VampireLord.Campaign;
using VampireLord.Scenario;
using Verse;

namespace VampireLord.Debug
{
    /// <summary>
    /// Shared lookups and mutations for Dev Mode <c>VL:</c> actions.
    /// Depends on production systems only — production must never call here.
    /// </summary>
    internal static class VampireLordDebugHelpers
    {
        public const string LogPrefix = "[VampireLord]";
        public const string WaveTestPrefix = "[VampireLord WAVE TEST]";
        public const string SaveTestPrefix = "[VampireLord SAVE TEST]";

        public static bool TryGetCampaign(out VampireLordCampaignGameComponent campaign, bool warn = true)
        {
            if (VampireLordCampaign.TryGet(out campaign))
            {
                return true;
            }

            if (warn)
            {
                Log.Warning($"{LogPrefix} Campaign component unavailable.");
            }

            return false;
        }

        public static bool TryGetMap(out Map map, bool warn = true)
        {
            map = VampireLordRaidLauncher.ResolveTargetMap() ?? Find.CurrentMap;
            if (map != null)
            {
                return true;
            }

            if (warn)
            {
                Log.Warning($"{LogPrefix} No current map.");
            }

            return false;
        }

        public static bool TryGetSelectedPawn(out Pawn pawn, bool warn = true)
        {
            pawn = Find.Selector?.SingleSelectedThing as Pawn;
            if (pawn != null)
            {
                return true;
            }

            if (warn)
            {
                Log.Warning($"{LogPrefix} No pawn selected.");
            }

            return false;
        }

        /// <summary>
        /// Vampire Lord is the Sanguophage colonist when present; otherwise first free colonist.
        /// There is no separate VL lord-designation system.
        /// </summary>
        public static bool TryGetVampireLord(out Pawn lord, bool warn = true)
        {
            lord = null;
            if (!TryGetMap(out Map map, warn))
            {
                return false;
            }

            Pawn fallback = null;
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                if (pawn == null)
                {
                    continue;
                }

                fallback ??= pawn;
                if (IsSanguophage(pawn))
                {
                    lord = pawn;
                    return true;
                }
            }

            lord = fallback;
            if (lord != null)
            {
                return true;
            }

            if (warn)
            {
                Log.Warning($"{LogPrefix} No Vampire Lord / colonist found on map.");
            }

            return false;
        }

        public static bool IsSanguophage(Pawn pawn)
        {
            return pawn?.genes?.Xenotype != null && pawn.genes.Xenotype == XenotypeDefOf.Sanguophage;
        }

        public static Gene_Hemogen TryGetHemogenGene(Pawn pawn)
        {
            return pawn?.genes?.GetFirstGeneOfType<Gene_Hemogen>();
        }

        public static Need_Deathrest TryGetDeathrestNeed(Pawn pawn)
        {
            return pawn?.needs?.TryGetNeed<Need_Deathrest>();
        }

        public static string FormatCompactState(VampireLordCampaignGameComponent campaign)
        {
            var sb = new StringBuilder();
            sb.AppendLine(LogPrefix);
            if (campaign == null)
            {
                sb.AppendLine("Active=false (no campaign component)");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine($"Active={campaign.CampaignActive}");
            sb.AppendLine($"Wave={campaign.WaveNumber}");
            sb.AppendLine($"Upcoming={campaign.UpcomingWaveNumber}");
            sb.AppendLine($"Threat={campaign.ThreatLevel}");
            sb.AppendLine($"Pending={campaign.WavePending}");
            sb.AppendLine($"Type={campaign.PendingWaveType}");
            sb.AppendLine($"WarningIssued={campaign.WarningIssued}");
            sb.AppendLine($"Blood={campaign.BloodReserve}");
            sb.AppendLine($"TitheNext={VampireLordBloodTithe.WaveBloodCost(campaign)}");
            sb.AppendLine($"GainedSinceWave={campaign.BloodGainedSinceWave}");
            sb.AppendLine($"LastStarved={campaign.LastWaveBloodStarved}");
            sb.AppendLine($"FortifyPurchases={campaign.FortifyPurchasesThisWindow}");
            sb.AppendLine($"AutoFortify={campaign.AutoFortifyPlaytest}");
            sb.AppendLine($"Pace={campaign.PlaytestPace}");
            sb.AppendLine($"QuietLetters={campaign.PlaytestQuietLetters}");
            sb.AppendLine($"HUD={campaign.ShowCampaignHud}");
            if (TryGetMap(out Map map, warn: false))
            {
                sb.AppendLine($"MapHour={GenLocalDate.HourOfDay(map):00}:00");
                sb.AppendLine($"IsPlayerHome={map.IsPlayerHome}");
            }

            if (TryGetVampireLord(out Pawn lord, warn: false))
            {
                Gene_Hemogen hemogen = TryGetHemogenGene(lord);
                sb.AppendLine($"Lord={lord.LabelShort}/Alive={true}");
                sb.AppendLine($"LordXeno={lord.genes?.Xenotype?.defName ?? "null"}");
                if (hemogen != null)
                {
                    sb.AppendLine($"LordHemogen={hemogen.Value:0.00}/{hemogen.Max:0.00}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string FormatSaveTestSnapshot(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null)
            {
                return $"{SaveTestPrefix} Active=false";
            }

            int hour = -1;
            if (TryGetMap(out Map map, warn: false))
            {
                hour = GenLocalDate.HourOfDay(map);
            }

            return
                $"{SaveTestPrefix} Active={campaign.CampaignActive} Wave={campaign.WaveNumber} " +
                $"Threat={campaign.ThreatLevel} Pending={campaign.WavePending} Type={campaign.PendingWaveType} " +
                $"Blood={campaign.BloodReserve} Warning={campaign.WarningIssued} " +
                $"AutoFortify={campaign.AutoFortifyPlaytest} Pace={campaign.PlaytestPace} " +
                $"Quiet={campaign.PlaytestQuietLetters} HUD={campaign.ShowCampaignHud} Hour={hour}";
        }

        public static string FormatPawnState(Pawn pawn)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{LogPrefix} Pawn State");
            sb.AppendLine($"Name={pawn.LabelShort}");
            sb.AppendLine($"Faction={pawn.Faction?.Name ?? "null"}");
            sb.AppendLine($"Xenotype={pawn.genes?.Xenotype?.defName ?? "null"}");
            sb.AppendLine($"Sanguophage={IsSanguophage(pawn)}");

            Gene_Hemogen hemogen = TryGetHemogenGene(pawn);
            if (hemogen != null)
            {
                sb.AppendLine($"Hemogen={hemogen.Value:0.00}/{hemogen.Max:0.00} ({hemogen.ValuePercent:P0})");
            }
            else
            {
                sb.AppendLine("Hemogen=(none)");
            }

            Need_Deathrest deathrest = TryGetDeathrestNeed(pawn);
            if (deathrest != null)
            {
                sb.AppendLine(
                    $"DeathrestNeed={deathrest.CurLevel:0.00}/{deathrest.MaxLevel:0.00} " +
                    $"Deathresting={deathrest.Deathresting}");
            }
            else
            {
                sb.AppendLine("DeathrestNeed=(none)");
            }

            if (pawn.genes?.GenesListForReading != null)
            {
                var genes = new List<string>();
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene?.def != null)
                    {
                        genes.Add(gene.def.defName);
                    }
                }

                sb.AppendLine($"Genes={string.Join(",", genes)}");
            }

            if (pawn.health?.hediffSet != null)
            {
                sb.AppendLine($"HealthPct={pawn.health.summaryHealth.SummaryHealthPercent:P0}");
                float consciousness = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
                float moving = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving);
                sb.AppendLine($"Consciousness={consciousness:P0}");
                sb.AppendLine($"Moving={moving:P0}");
                sb.AppendLine($"Downed={pawn.Downed} Dead={pawn.Dead}");

                var injuries = new List<string>();
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury injury && injury.Visible)
                    {
                        injuries.Add($"{injury.Label}:{injury.Severity:0.0}");
                    }
                }

                sb.AppendLine(injuries.Count == 0 ? "Injuries=(none)" : $"Injuries={string.Join("; ", injuries)}");
            }

            if (pawn.equipment?.Primary != null)
            {
                sb.AppendLine($"Primary={pawn.equipment.Primary.LabelCap}");
            }
            else
            {
                sb.AppendLine("Primary=(none)");
            }

            if (pawn.apparel?.WornApparel != null)
            {
                var worn = new List<string>();
                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    worn.Add(apparel.LabelCap);
                }

                sb.AppendLine(worn.Count == 0 ? "Apparel=(none)" : $"Apparel={string.Join(", ", worn)}");
            }

            if (pawn.abilities?.abilities != null && pawn.abilities.abilities.Count > 0)
            {
                var abilities = new List<string>();
                foreach (Ability ability in pawn.abilities.abilities)
                {
                    if (ability?.def != null)
                    {
                        abilities.Add(ability.def.defName);
                    }
                }

                sb.AppendLine($"Abilities={string.Join(",", abilities)}");
            }

            sb.AppendLine($"MentalState={pawn.MentalStateDef?.defName ?? "none"}");
            return sb.ToString().TrimEnd();
        }

        public static bool TrySetMapHour(Map map, int hour, out string result)
        {
            if (map == null)
            {
                result = "no map";
                return false;
            }

            hour = ((hour % 24) + 24) % 24;
            int current = GenLocalDate.HourOfDay(map);
            int deltaHours = hour - current;
            if (deltaHours < 0)
            {
                deltaHours += 24;
            }

            if (deltaHours == 0)
            {
                result = $"{hour:00}:00 (already)";
                return true;
            }

            Find.TickManager.DebugSetTicksGame(
                Find.TickManager.TicksGame + (deltaHours * GenDate.TicksPerHour));
            int after = GenLocalDate.HourOfDay(map);
            result = $"{after:00}:00";
            Log.Message($"{LogPrefix} Time changed to {result} (was {current:00}:00).");
            return true;
        }

        public static bool TryFillHemogen(Pawn pawn, out string message)
        {
            Gene_Hemogen gene = TryGetHemogenGene(pawn);
            if (gene == null)
            {
                message = "selected pawn has no Gene_Hemogen.";
                return false;
            }

            float old = gene.Value;
            gene.Value = gene.Max;
            message = $"Hemogen: {old:0.00} -> {gene.Value:0.00}";
            Log.Message($"{LogPrefix} {message} ({pawn.LabelShort}).");
            return true;
        }

        public static bool TryEmptyHemogen(Pawn pawn, out string message)
        {
            Gene_Hemogen gene = TryGetHemogenGene(pawn);
            if (gene == null)
            {
                message = "selected pawn has no Gene_Hemogen.";
                return false;
            }

            float old = gene.Value;
            gene.Value = 0f;
            message = $"Hemogen: {old:0.00} -> {gene.Value:0.00}";
            Log.Message($"{LogPrefix} {message} ({pawn.LabelShort}).");
            return true;
        }

        public static bool TryForceDeathrestReady(Pawn pawn, out string message)
        {
            Need_Deathrest need = TryGetDeathrestNeed(pawn);
            if (need == null)
            {
                message = "selected pawn has no Need_Deathrest.";
                return false;
            }

            // Critical need so JobGiver_GetDeathrest can fire; leave hediff to vanilla.
            float old = need.CurLevel;
            need.CurLevel = 0.05f;
            message = $"Deathrest need: {old:0.00} -> {need.CurLevel:0.00} (Deathresting={need.Deathresting})";
            Log.Message($"{LogPrefix} {message} ({pawn.LabelShort}).");
            return true;
        }

        public static void RestoreVampireCombat(Pawn pawn)
        {
            if (pawn?.health == null)
            {
                Log.Warning($"{LogPrefix} Cannot restore: invalid pawn.");
                return;
            }

            HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(pawn);
            TryFillHemogen(pawn, out _);

            Need_Deathrest need = TryGetDeathrestNeed(pawn);
            if (need != null && !need.Deathresting)
            {
                need.CurLevel = need.MaxLevel;
            }

            Log.Message($"{LogPrefix} Restored combat state for {pawn.LabelShort}.");
        }

        public static void SetThreat(VampireLordCampaignGameComponent campaign, int threat)
        {
            int old = campaign.ThreatLevel;
            if (threat < VampireLordTuning.InitialThreatLevel)
            {
                threat = VampireLordTuning.InitialThreatLevel;
            }

            campaign.ThreatLevel = threat;
            Log.Message($"{LogPrefix} Threat: {old} -> {campaign.ThreatLevel}");
        }

        public static void EnsureCampaignActive(VampireLordCampaignGameComponent campaign)
        {
            if (!campaign.CampaignActive)
            {
                VampireLordWaveDirector.ActivateCampaign(campaign);
            }
        }

        /// <summary>
        /// Fire a production wave at a chosen threat. Uses <see cref="VampireLordWaveDirector.TriggerWaveNow"/>.
        /// </summary>
        public static void TriggerSizedWave(VampireLordCampaignGameComponent campaign, int threat, string label)
        {
            EnsureCampaignActive(campaign);
            SetThreat(campaign, threat);
            VampireLordWaveDirector.EnsurePendingWave(campaign);
            Log.Message($"{WaveTestPrefix} Triggering {label} (threat={campaign.ThreatLevel}, type={campaign.PendingWaveType}).");
            VampireLordWaveDirector.TriggerWaveNow(campaign);
        }

        public static List<Pawn> CollectHostileHumanlikes(Map map)
        {
            var result = new List<Pawn>();
            if (map?.mapPawns == null)
            {
                return result;
            }

            Faction player = Faction.OfPlayer;
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike || pawn.Dead)
                {
                    continue;
                }

                if (pawn.Faction == null || pawn.Faction == player || !pawn.HostileTo(player))
                {
                    continue;
                }

                result.Add(pawn);
            }

            return result;
        }

        public static int KillHostileHumanlikes(Map map)
        {
            List<Pawn> hostiles = CollectHostileHumanlikes(map);
            int killed = 0;
            for (int i = 0; i < hostiles.Count; i++)
            {
                Pawn pawn = hostiles[i];
                if (pawn.Destroyed || pawn.Dead)
                {
                    continue;
                }

                pawn.Kill(null);
                killed++;
            }

            Log.Message($"{WaveTestPrefix} Killed {killed} hostile humanlike(s) (normal death).");
            return killed;
        }

        public static int RemoveHostileHumanlikes(Map map)
        {
            List<Pawn> hostiles = CollectHostileHumanlikes(map);
            int removed = 0;
            for (int i = 0; i < hostiles.Count; i++)
            {
                Pawn pawn = hostiles[i];
                if (pawn.Destroyed)
                {
                    continue;
                }

                pawn.Destroy(DestroyMode.Vanish);
                removed++;
            }

            Log.Message($"{WaveTestPrefix} Removed {removed} hostile humanlike(s) (dev vanish).");
            return removed;
        }

        public static bool TryGetKeepBuildings(Map map, out List<Building> buildings)
        {
            buildings = new List<Building>();
            if (map == null)
            {
                return false;
            }

            if (!VampireLordKeepLayout.TryGetCourtyard(map, out CellRect courtyard))
            {
                // Fallback: player-faction walls/sandbags near start.
                courtyard = CellRect.CenteredOn(
                    MapGenerator.PlayerStartSpotValid ? MapGenerator.PlayerStartSpot : map.Center,
                    8).ClipInsideMap(map);
            }

            Faction player = Faction.OfPlayer;
            foreach (IntVec3 cell in courtyard.Cells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                Building edifice = cell.GetEdifice(map);
                if (edifice == null)
                {
                    continue;
                }

                if (edifice.Faction != null && edifice.Faction != player)
                {
                    continue;
                }

                if (edifice.def == ThingDefOf.Wall
                    || edifice.def == ThingDefOf.Sandbags
                    || edifice.def.IsDoor)
                {
                    if (!buildings.Contains(edifice))
                    {
                        buildings.Add(edifice);
                    }
                }
            }

            // Gate sandbags can sit just outside the courtyard edge.
            if (VampireLordKeepLayout.TryGetGateCenter(map, out IntVec3 gate))
            {
                foreach (IntVec3 cell in CellRect.CenteredOn(gate, 3).ClipInsideMap(map))
                {
                    Building edifice = cell.GetEdifice(map);
                    if (edifice?.def == ThingDefOf.Sandbags && !buildings.Contains(edifice))
                    {
                        buildings.Add(edifice);
                    }
                }
            }

            return buildings.Count > 0;
        }

        public static int RepairKeepBuildings(Map map)
        {
            if (!TryGetKeepBuildings(map, out List<Building> buildings))
            {
                Log.Warning($"{LogPrefix} Repair Keep: no courtyard walls/sandbags found.");
                return 0;
            }

            int repaired = 0;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building b = buildings[i];
                if (b.HitPoints < b.MaxHitPoints)
                {
                    b.HitPoints = b.MaxHitPoints;
                    repaired++;
                }
            }

            Log.Message($"{LogPrefix} Repaired {repaired}/{buildings.Count} keep building(s).");
            return repaired;
        }

        public static int DamageKeepBuildingsToHalf(Map map)
        {
            if (!TryGetKeepBuildings(map, out List<Building> buildings))
            {
                Log.Warning($"{LogPrefix} Damage Keep: no courtyard walls/sandbags found.");
                return 0;
            }

            int damaged = 0;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building b = buildings[i];
                int target = b.MaxHitPoints / 2;
                if (target < 1)
                {
                    target = 1;
                }

                if (b.HitPoints != target)
                {
                    b.HitPoints = target;
                    damaged++;
                }
            }

            Log.Message($"{LogPrefix} Damaged {damaged}/{buildings.Count} keep building(s) to ~50% HP.");
            return damaged;
        }

        public static void ResetCampaignToDefaults(VampireLordCampaignGameComponent campaign)
        {
            campaign.CampaignActive = false;
            campaign.CampaignDay = 0;
            campaign.WaveNumber = 0;
            campaign.ThreatLevel = VampireLordTuning.InitialThreatLevel;
            campaign.NextWaveTick = -1;
            campaign.WarningTick = -1;
            campaign.PendingWaveType = VampireLordWaveType.Mob;
            campaign.WarningIssued = false;
            campaign.WavePending = false;
            campaign.LastWaveType = VampireLordWaveType.Mob;
            campaign.SameArchetypeStreak = 0;
            campaign.BloodReserve = 0;
            campaign.BloodGainedSinceWave = 0;
            campaign.LastWaveBloodStarved = false;
            campaign.LastWaveBloodSpent = 0;
            campaign.LastWaveBloodCost = 0;
            campaign.FortifyPurchasesThisWindow = 0;
            campaign.LastFortifyWaveNumber = -1;
            campaign.LastFortifyPlacedCount = 0;
            campaign.FortifyOfferDueTick = -1;
            campaign.FortifyOfferSentThisWindow = false;
            campaign.AutoFortifyPlaytest = true;
            campaign.PlaytestPace = true;
            campaign.PlaytestQuietLetters = true;
            campaign.ShowCampaignHud = true;
            VampireLordBloodTithe.ResetSessionCredits();
            Log.Message($"{LogPrefix} RESET: VampireLord campaign state restored to production defaults (inactive).");
        }
    }
}
