using System.Collections.Generic;
using System.Text;
using RimWorld;
using VampireLord.Scenario;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// M4 Keep Fortification V0: spend Blood Tithe between waves on gate sandbags.
    /// No Harmony — placement via GenSpawn; gate from <see cref="VampireLordKeepLayout"/>.
    /// </summary>
    public static class VampireLordFortify
    {
        private const string LogPrefix = "[VampireLord]";
        private const string SandbagsDefName = "Sandbags";

        public static bool IsPrepWindow(VampireLordCampaignGameComponent campaign)
        {
            // Between schedule and warning: wave is pending but scouts have not spoken yet.
            return campaign != null
                   && campaign.CampaignActive
                   && campaign.WavePending
                   && !campaign.WarningIssued;
        }

        public static bool CanPurchase(
            VampireLordCampaignGameComponent campaign,
            bool forced,
            out string reason)
        {
            reason = null;
            if (campaign == null || !campaign.CampaignActive)
            {
                reason = "Campaign inactive.";
                return false;
            }

            if (!forced && !IsPrepWindow(campaign))
            {
                reason = "Fortify only between waves (before the advance warning).";
                return false;
            }

            if (!forced &&
                campaign.FortifyPurchasesThisWindow >= VampireLordTuning.FortifyMaxPerPrepWindow)
            {
                reason = "Gate already fortified this prep window.";
                return false;
            }

            if (campaign.BloodReserve < VampireLordTuning.FortifyBloodCost)
            {
                reason =
                    $"Need {VampireLordTuning.FortifyBloodCost} blood " +
                    $"(have {campaign.BloodReserve}).";
                return false;
            }

            Map map = ResolveKeepMap();
            if (map == null)
            {
                reason = "No keep map.";
                return false;
            }

            if (!VampireLordKeepLayout.TryGetGateCenter(map, out _))
            {
                reason = "No gate position.";
                return false;
            }

            if (DefDatabase<ThingDef>.GetNamedSilentFail(SandbagsDefName) == null)
            {
                reason = "Sandbags def missing.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when a new prep window opens (campaign activate / after a wave schedules the next).
        /// Schedules the Accept letter a few ticks later so PostGameStart settle does not swallow it.
        /// </summary>
        public static void NotifyPrepWindowOpened(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null || !campaign.CampaignActive)
            {
                return;
            }

            campaign.FortifyPurchasesThisWindow = 0;
            campaign.FortifyOfferSentThisWindow = false;
            campaign.FortifyOfferDueTick =
                Find.TickManager.TicksGame + VampireLordTuning.FortifyOfferDelayTicks;
            Log.Message(
                $"{LogPrefix} Fortify offer scheduled at tick {campaign.FortifyOfferDueTick} " +
                $"(Blood={campaign.BloodReserve}). Look for letter 'Blood for the Walls' — not the Quests tab.");
        }

        /// <summary>Tick hook: deliver a scheduled fortify letter when due.</summary>
        public static void EvaluatePendingOffer(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null || !campaign.CampaignActive)
            {
                return;
            }

            if (campaign.FortifyOfferSentThisWindow || campaign.FortifyOfferDueTick < 0)
            {
                return;
            }

            if (Find.TickManager.TicksGame < campaign.FortifyOfferDueTick)
            {
                return;
            }

            // Still in prep? If warning already fired, drop the pending offer.
            if (!IsPrepWindow(campaign))
            {
                campaign.FortifyOfferDueTick = -1;
                Log.Message($"{LogPrefix} Fortify offer cancelled — prep window closed before delivery.");
                return;
            }

            TrySendOfferLetter(campaign);
        }

        public static void TrySendOfferLetter(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null || !campaign.CampaignActive)
            {
                return;
            }

            if (campaign.FortifyOfferSentThisWindow)
            {
                Log.Message($"{LogPrefix} Fortify offer already sent this prep window.");
                return;
            }

            if (!IsPrepWindow(campaign))
            {
                Log.Message($"{LogPrefix} Fortify offer skipped — not in prep window.");
                return;
            }

            if (campaign.FortifyPurchasesThisWindow >= VampireLordTuning.FortifyMaxPerPrepWindow)
            {
                Log.Message($"{LogPrefix} Fortify offer skipped — already fortified this window.");
                return;
            }

            // Send even if blood/map is briefly unready — Accept disables until purchase works.
            VampireLordLetters.SendFortifyOffer(campaign);
            campaign.FortifyOfferSentThisWindow = true;
            campaign.FortifyOfferDueTick = -1;
            Log.Message(
                $"{LogPrefix} Fortify offer letter sent " +
                $"(cost={VampireLordTuning.FortifyBloodCost}, Reserve={campaign.BloodReserve}). " +
                "Check the letter stack (top-right), not Quests.");
        }

        public static bool TryPurchase(
            VampireLordCampaignGameComponent campaign,
            bool forced,
            string reason)
        {
            if (!CanPurchase(campaign, forced, out string block))
            {
                Log.Warning($"{LogPrefix} Fortify blocked ({reason}): {block}");
                return false;
            }

            Map map = ResolveKeepMap();
            if (!VampireLordKeepLayout.TryGetGateCenter(map, out IntVec3 gate))
            {
                Log.Warning($"{LogPrefix} Fortify failed ({reason}): no gate.");
                return false;
            }

            int spent = VampireLordBloodTithe.SpendBlood(
                campaign,
                VampireLordTuning.FortifyBloodCost,
                $"fortify gate ({reason})");
            if (spent < VampireLordTuning.FortifyBloodCost)
            {
                // Should not happen after CanPurchase; refund and abort.
                if (spent > 0)
                {
                    VampireLordBloodTithe.AddBlood(campaign, spent, "fortify abort refund");
                }

                Log.Warning($"{LogPrefix} Fortify aborted — could not pay full cost.");
                return false;
            }

            int placed = PlaceGateSandbags(map, gate);
            campaign.FortifyPurchasesThisWindow++;
            campaign.LastFortifyPlacedCount = placed;
            campaign.LastFortifyWaveNumber = campaign.WaveNumber;

            VampireLordLetters.SendFortifyComplete(campaign, placed);
            Log.Message(
                $"{LogPrefix} Fortified gate ({reason}): placed {placed} sandbags at {gate}. " +
                $"Reserve={campaign.BloodReserve}, PurchasesThisWindow={campaign.FortifyPurchasesThisWindow}.");
            return placed > 0;
        }

        public static string FormatState(VampireLordCampaignGameComponent campaign)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"PrepWindow={IsPrepWindow(campaign)}");
            sb.AppendLine($"FortifyBloodCost={VampireLordTuning.FortifyBloodCost}");
            sb.AppendLine($"FortifyPurchasesThisWindow={campaign.FortifyPurchasesThisWindow}/{VampireLordTuning.FortifyMaxPerPrepWindow}");
            sb.AppendLine($"FortifyOfferSentThisWindow={campaign.FortifyOfferSentThisWindow}");
            sb.AppendLine($"FortifyOfferDueTick={campaign.FortifyOfferDueTick}");
            sb.AppendLine($"LastFortifyWaveNumber={campaign.LastFortifyWaveNumber}");
            sb.AppendLine($"LastFortifyPlacedCount={campaign.LastFortifyPlacedCount}");
            sb.AppendLine($"BloodReserve={campaign.BloodReserve}");
            sb.AppendLine($"CanPurchase={CanPurchase(campaign, forced: false, out string reason)} ({reason ?? "ok"})");
            Map map = ResolveKeepMap();
            if (map != null && VampireLordKeepLayout.TryGetGateCenter(map, out IntVec3 gate))
            {
                sb.AppendLine($"Gate={gate} Map={map}");
            }
            else
            {
                sb.AppendLine("Gate=(none)");
            }

            return sb.ToString().TrimEnd();
        }

        private static int PlaceGateSandbags(Map map, IntVec3 gate)
        {
            ThingDef sandbagsDef = DefDatabase<ThingDef>.GetNamedSilentFail(SandbagsDefName);
            Faction player = Faction.OfPlayer;
            if (sandbagsDef == null || player == null)
            {
                return 0;
            }

            ThingDef stuff = GenStuff.DefaultStuffFor(sandbagsDef) ?? ThingDefOf.Cloth;
            List<IntVec3> cells = BuildSandbagCells(gate);
            int placed = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                IntVec3 cell = cells[i];
                if (!cell.InBounds(map) || !cell.Walkable(map))
                {
                    continue;
                }

                if (cell.GetEdifice(map) != null)
                {
                    continue;
                }

                ClearSoftThings(map, cell);
                Thing bags = ThingMaker.MakeThing(sandbagsDef, stuff);
                if (bags == null)
                {
                    continue;
                }

                bags.SetFactionDirect(player);
                GenSpawn.Spawn(bags, cell, map, WipeMode.VanishOrMoveAside);
                if (bags.Faction != player)
                {
                    bags.SetFaction(player);
                }

                placed++;
            }

            return placed;
        }

        /// <summary>
        /// Short chevron / flanks just south of the open gate, center path left open.
        /// </summary>
        private static List<IntVec3> BuildSandbagCells(IntVec3 gate)
        {
            var cells = new List<IntVec3>(8);
            int gz2 = gate.z - 2;
            int gz3 = gate.z - 3;

            // Flanks at first row south of gate (leave 3-cell gate lane open).
            cells.Add(new IntVec3(gate.x - 2, 0, gz2));
            cells.Add(new IntVec3(gate.x + 2, 0, gz2));
            cells.Add(new IntVec3(gate.x - 3, 0, gz2));
            cells.Add(new IntVec3(gate.x + 3, 0, gz2));

            // Outer hooks one step further south.
            cells.Add(new IntVec3(gate.x - 2, 0, gz3));
            cells.Add(new IntVec3(gate.x + 2, 0, gz3));

            return cells;
        }

        private static void ClearSoftThings(Map map, IntVec3 cell)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = things.Count - 1; i >= 0; i--)
            {
                Thing thing = things[i];
                if (thing?.def == null)
                {
                    continue;
                }

                if (thing.def.category == ThingCategory.Plant || thing.def.IsFilth)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
        }

        private static Map ResolveKeepMap()
        {
            Map current = Find.CurrentMap;
            if (current != null && (current.IsPlayerHome || HasColonists(current)))
            {
                return current;
            }

            if (Find.AnyPlayerHomeMap != null)
            {
                return Find.AnyPlayerHomeMap;
            }

            List<Map> maps = Find.Maps;
            if (maps == null)
            {
                return null;
            }

            for (int i = 0; i < maps.Count; i++)
            {
                if (HasColonists(maps[i]))
                {
                    return maps[i];
                }
            }

            return Find.CurrentMap;
        }

        private static bool HasColonists(Map map)
        {
            return map?.mapPawns != null && map.mapPawns.FreeColonistsSpawnedCount > 0;
        }
    }
}
