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
            // Def is optional for scenario-embedded custom parts; only treat as null if other defs break.
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

            CellRect exterior = CellRect.CenteredOn(center, CourtyardHalfSize);
            exterior = exterior.ClipInsideMap(map);
            CellRect interior = exterior.ContractedBy(1);

            ThingDef wallDef = ThingDefOf.Wall;
            ThingDef doorDef = ThingDefOf.Door;
            ThingDef stuff = ThingDefOf.BlocksGranite;
            TerrainDef floor = TerrainDefOf.FlagstoneSandstone;

            // Clear footprint (plants, chunks, wreckage) so walls can spawn.
            foreach (IntVec3 cell in exterior.Cells)
            {
                ClearCellForKeep(map, cell);
            }

            // Interior paving.
            foreach (IntVec3 cell in interior.Cells)
            {
                if (cell.InBounds(map))
                {
                    map.terrainGrid.SetTerrain(cell, floor);
                }
            }

            // Perimeter walls with a single south-facing gate.
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

                if (cell == gateCell)
                {
                    SpawnKeepThing(ThingMaker.MakeThing(doorDef, stuff), cell, map);
                    continue;
                }

                SpawnKeepThing(ThingMaker.MakeThing(wallDef, stuff), cell, map);
            }

            // Keep player start inside the courtyard.
            MapGenerator.PlayerStartSpot = center;

            Log.Message(
                $"{LogPrefix} Playtest keep courtyard placed at {center} " +
                $"(size {exterior.Width}x{exterior.Height}, gate {gateCell}).");
        }

        private static void ClearCellForKeep(Map map, IntVec3 cell)
        {
            List<Thing> things = cell.GetThingList(map).ToList();
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing == null || thing.def == null)
                {
                    continue;
                }

                // Keep terrain / fog etc.; remove plants, stone chunks, wreckage, buildings in footprint.
                if (thing.def.category == ThingCategory.Plant
                    || thing.def.category == ThingCategory.Item
                    || thing.def.category == ThingCategory.Building
                    || thing.def.IsFilth)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
        }

        private static void SpawnKeepThing(Thing thing, IntVec3 cell, Map map)
        {
            if (thing == null || !cell.InBounds(map))
            {
                return;
            }

            GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);
        }
    }
}
