using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VampireLord.Campaign;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// M2 playtest setup: place a simple walled courtyard at player start, then auto-start the Wave Director.
    /// Used only by the Vampire Lord scenario (not global).
    /// </summary>
    public class ScenPart_VampireLordPlaytestSetup : ScenPart
    {
        private const int CourtyardHalfSize = 7; // exterior ~15x15
        private const string LogPrefix = "[VampireLord]";

        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            TryPlaceCourtyard(map);
        }

        public override void PostGameStart()
        {
            base.PostGameStart();
            TryAutoStartCampaign();
        }

        public override string Summary(RimWorld.Scenario scen)
        {
            return "Starts in a simple walled courtyard keep; Wave Director activates automatically.";
        }

        public override bool HasNullDefs()
        {
            return false;
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

        private static void TryPlaceCourtyard(Map map)
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

            ThingDef wallDef = ThingDefOf.Wall;
            ThingDef doorDef = ThingDefOf.Door;
            ThingDef stuff = ThingDefOf.BlocksGranite;
            TerrainDef floor = TerrainDefOf.FlagstoneSandstone;
            Faction player = Faction.OfPlayer;

            // Clear plants / filth / blocking buildings only (never wipe player starting items).
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

            IntVec3 gateCell = exterior.GetCenterCellOnEdge(Rot4.South);
            if (!gateCell.InBounds(map))
            {
                gateCell = new IntVec3(center.x, 0, exterior.minZ);
            }

            foreach (IntVec3 cell in exterior.EdgeCells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                // Wall/door cells may need chunks/items cleared so the building can spawn.
                ClearCellForKeep(map, cell, clearItems: true);

                Thing building = cell == gateCell
                    ? ThingMaker.MakeThing(doorDef, stuff)
                    : ThingMaker.MakeThing(wallDef, stuff);

                SpawnPlayerBuilding(building, cell, map, player);
            }

            MapGenerator.PlayerStartSpot = center;

            Log.Message(
                $"{LogPrefix} Playtest keep courtyard placed at {center} " +
                $"(size {exterior.Width}x{exterior.Height}, gate {gateCell}, faction={player?.Name}).");
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

            // Player ownership = claimable / deconstructable / home area works as expected.
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
