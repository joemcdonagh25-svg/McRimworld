using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VampireLord.Campaign;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// M2 playtest setup: place a simple walled courtyard at player start, reveal surroundings, auto-start Wave Director.
    /// Used only by the Vampire Lord scenario (not global).
    /// </summary>
    public class ScenPart_VampireLordPlaytestSetup : ScenPart
    {
        private const int CourtyardHalfSize = 7; // exterior ~15x15
        private const int GateHalfWidth = 1; // 3-cell open gate
        private const int UnfogPadding = 40;
        private const string LogPrefix = "[VampireLord]";

        private CellRect lastCourtyard = CellRect.Empty;
        private IntVec3 lastGateCenter = IntVec3.Invalid;

        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            TryPlaceCourtyard(map);
        }

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            // Fog is applied during map gen after ScenParts.GenerateIntoMap — unfog here so the wilds are visible/pathable.
            TryRevealAroundKeep(map);
        }

        public override void PostWorldGenerate()
        {
            base.PostWorldGenerate();
            TryEnsurePlayerIdeo();
        }

        public override void PostGameStart()
        {
            base.PostGameStart();
            TryEnsureStartingPawnsAreColonists();
            TryAutoStartCampaign();
        }

        public override string Summary(RimWorld.Scenario scen)
        {
            return "Starts in a simple walled courtyard keep with an open gate; surroundings revealed; Wave Director auto-starts.";
        }

        public override bool HasNullDefs()
        {
            return false;
        }

        private static void TryEnsurePlayerIdeo()
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }

            Faction player = Faction.OfPlayer;
            if (player?.ideos == null)
            {
                Log.Warning($"{LogPrefix} Ideology active but player faction ideos tracker missing.");
                return;
            }

            if (player.ideos.PrimaryIdeo != null)
            {
                return;
            }

            try
            {
                var parms = new IdeoGenerationParms(player.def);
                player.ideos.ChooseOrGenerateIdeo(parms);
                Log.Message($"{LogPrefix} Generated starting ideoligion for player faction (avoids broken ChooseIdeoPreset).");
            }
            catch (System.Exception e)
            {
                Log.Warning($"{LogPrefix} Failed to generate starting ideoligion: {e.Message}");
            }
        }

        private static void TryEnsureStartingPawnsAreColonists()
        {
            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map == null || Faction.OfPlayer == null)
            {
                return;
            }

            int fixedCount = 0;
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike)
                {
                    continue;
                }

                if (pawn.Faction != Faction.OfPlayer)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                Log.Message($"{LogPrefix} Reassigned {fixedCount} humanlike pawn(s) to player faction.");
            }
        }

        private static void TryAutoStartCampaign()
        {
            if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
            {
                Log.Warning($"{LogPrefix} Playtest setup: no campaign component — cannot auto-start.");
                return;
            }

            if (campaign.CampaignActive)
            {
                Log.Message($"{LogPrefix} Playtest setup: campaign already active.");
                return;
            }

            VampireLordWaveDirector.ActivateCampaign(campaign);
            Log.Message($"{LogPrefix} Playtest setup: campaign auto-started for Vampire Lord scenario.");
        }

        private void TryPlaceCourtyard(Map map)
        {
            if (map == null)
            {
                return;
            }

            IntVec3 center = MapGenerator.PlayerStartSpotValid
                ? MapGenerator.PlayerStartSpot
                : map.Center;

            if (!center.InBounds(map))
            {
                center = map.Center;
            }

            CellRect exterior = CellRect.CenteredOn(center, CourtyardHalfSize).ClipInsideMap(map);
            CellRect interior = exterior.ContractedBy(1);
            lastCourtyard = exterior;

            ThingDef wallDef = ThingDefOf.Wall;
            ThingDef stuff = ThingDefOf.BlocksGranite;
            TerrainDef floor = TerrainDefOf.FlagstoneSandstone;
            Faction player = Faction.OfPlayer;

            foreach (IntVec3 cell in exterior.Cells)
            {
                ClearCellForKeep(map, cell, clearItems: false);
            }

            foreach (IntVec3 cell in interior.Cells)
            {
                if (cell.InBounds(map))
                {
                    map.terrainGrid.SetTerrain(cell, floor);
                }
            }

            IntVec3 gateCenter = exterior.GetCenterCellOnEdge(Rot4.South);
            if (!gateCenter.InBounds(map))
            {
                gateCenter = new IntVec3(center.x, 0, exterior.minZ);
            }

            lastGateCenter = gateCenter;

            foreach (IntVec3 cell in exterior.EdgeCells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                // Open south gate (no door) so pathing to the wilds is never blocked by a closed door.
                if (IsGateCell(cell, gateCenter))
                {
                    ClearCellForKeep(map, cell, clearItems: true);
                    continue;
                }

                ClearCellForKeep(map, cell, clearItems: true);
                SpawnPlayerBuilding(ThingMaker.MakeThing(wallDef, stuff), cell, map, player);
            }

            // Clear a short approach path south of the gate (plants/rocks) so the exit isn't visually "dead".
            ClearApproachLane(map, gateCenter);

            MapGenerator.PlayerStartSpot = center;

            Log.Message(
                $"{LogPrefix} Playtest keep courtyard placed at {center} " +
                $"(size {exterior.Width}x{exterior.Height}, open gate {gateCenter}, faction={player?.Name}).");
        }

        private void TryRevealAroundKeep(Map map)
        {
            if (map?.fogGrid == null || lastCourtyard.IsEmpty)
            {
                return;
            }

            CellRect reveal = lastCourtyard.ExpandedBy(UnfogPadding).ClipInsideMap(map);
            int unfogged = 0;
            foreach (IntVec3 cell in reveal.Cells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                if (map.fogGrid.IsFogged(cell))
                {
                    map.fogGrid.Unfog(cell);
                    unfogged++;
                }
            }

            if (lastGateCenter.IsValid)
            {
                map.fogGrid.FloodUnfogAdjacent(lastGateCenter, false);
            }

            Log.Message($"{LogPrefix} Revealed {unfogged} cells around keep (padding {UnfogPadding}) so the outside map is playable.");
        }

        private static bool IsGateCell(IntVec3 cell, IntVec3 gateCenter)
        {
            return cell.z == gateCenter.z
                && cell.x >= gateCenter.x - GateHalfWidth
                && cell.x <= gateCenter.x + GateHalfWidth;
        }

        private static void ClearApproachLane(Map map, IntVec3 gateCenter)
        {
            for (int dz = 1; dz <= 6; dz++)
            {
                for (int dx = -GateHalfWidth; dx <= GateHalfWidth; dx++)
                {
                    IntVec3 cell = new IntVec3(gateCenter.x + dx, 0, gateCenter.z - dz);
                    if (cell.InBounds(map))
                    {
                        ClearCellForKeep(map, cell, clearItems: false);
                    }
                }
            }
        }

        private static void ClearCellForKeep(Map map, IntVec3 cell, bool clearItems)
        {
            List<Thing> things = cell.GetThingList(map).ToList();
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing?.def == null)
                {
                    continue;
                }

                bool remove =
                    thing.def.category == ThingCategory.Plant
                    || thing.def.category == ThingCategory.Building
                    || thing.def.IsFilth
                    || (clearItems && thing.def.category == ThingCategory.Item);

                if (remove)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
        }

        private static void SpawnPlayerBuilding(Thing thing, IntVec3 cell, Map map, Faction player)
        {
            if (thing == null || !cell.InBounds(map))
            {
                return;
            }

            if (player != null)
            {
                thing.SetFactionDirect(player);
            }

            GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);

            if (player != null && thing.Faction != player)
            {
                thing.SetFaction(player);
            }
        }
    }
}
