using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Hamlet Mode: prebuilds a small, loosely-arranged medieval hamlet around the map
    /// center -- a great hall with a hearth, two timber cottages with beds, a growing
    /// zone, a wood stockpile, and a scatter of lanterns. Deliberately does not build a
    /// palisade; the defensive perimeter is meant to be an early player objective (see
    /// the project spec). Only runs for The Lantern Village scenario's Hamlet Mode.
    ///
    /// Implemented against RimWorld's standard GenStep/ThingMaker/GenSpawn/Zone API and
    /// compiles against the real game assembly, but has not been runtime-verified by
    /// actually generating a map in this session -- see IMPLEMENTATION_REPORT.md.
    /// </summary>
    public class GenStep_LanternHamlet : GenStep
    {
        public override int SeedPart => 194_810_31;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!ScenarioGate.IsActive() || !HamletModeState.HamletModeRequested)
            {
                return;
            }

            IntVec3 center = map.Center;
            ThingDef wall = DefDatabase<ThingDef>.GetNamedSilentFail("Wall");
            ThingDef door = DefDatabase<ThingDef>.GetNamedSilentFail("Door");
            ThingDef campfire = DefDatabase<ThingDef>.GetNamedSilentFail("Campfire");
            ThingDef torch = DefDatabase<ThingDef>.GetNamedSilentFail("TorchLamp");
            ThingDef bed = DefDatabase<ThingDef>.GetNamedSilentFail("Bed");
            ThingDef woodLog = DefDatabase<ThingDef>.GetNamedSilentFail("WoodLog");

            if (wall == null || door == null || woodLog == null)
            {
                Log.Warning("[Lantern Village] Hamlet generator could not find required vanilla defs (Wall/Door/WoodLog); skipping hamlet prebuild.");
                return;
            }

            BuildGreatHall(map, center, wall, door, campfire, torch, woodLog);
            BuildCottage(map, center + new IntVec3(-10, 0, 8), wall, door, bed, torch, woodLog);
            BuildCottage(map, center + new IntVec3(8, 0, 8), wall, door, bed, torch, woodLog);
            MakeGrowingZone(map, center + new IntVec3(0, 0, -12));
            MakeStockpileZone(map, center + new IntVec3(10, 0, -8));
        }

        private static void BuildGreatHall(Map map, IntVec3 center, ThingDef wall, ThingDef door, ThingDef campfire, ThingDef torch, ThingDef stuff)
        {
            CellRect rect = CellRect.CenteredOn(center, 6, 5);
            BuildRectWalls(map, rect, wall, door, stuff);

            if (campfire != null)
            {
                SpawnThing(map, campfire, null, center);
            }
            if (torch != null)
            {
                SpawnThing(map, torch, stuff, rect.minX + 1, rect.minZ + 1);
                SpawnThing(map, torch, stuff, rect.maxX - 1, rect.maxZ - 1);
            }
        }

        private static void BuildCottage(Map map, IntVec3 center, ThingDef wall, ThingDef door, ThingDef bed, ThingDef torch, ThingDef stuff)
        {
            CellRect rect = CellRect.CenteredOn(center, 4, 4);
            BuildRectWalls(map, rect, wall, door, stuff);

            if (bed != null)
            {
                SpawnThing(map, bed, stuff, center);
            }
            if (torch != null)
            {
                SpawnThing(map, torch, stuff, rect.minX + 1, rect.minZ + 1);
            }
        }

        private static void BuildRectWalls(Map map, CellRect rect, ThingDef wall, ThingDef door, ThingDef stuff)
        {
            IntVec3 doorCell = new IntVec3(rect.CenterCell.x, 0, rect.minZ);
            foreach (IntVec3 cell in rect.EdgeCells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }
                if (cell == doorCell && door != null)
                {
                    SpawnThing(map, door, stuff, cell);
                }
                else
                {
                    SpawnThing(map, wall, stuff, cell);
                }
            }
        }

        private static void MakeGrowingZone(Map map, IntVec3 center)
        {
            Zone_Growing zone = new Zone_Growing(map.zoneManager);
            foreach (IntVec3 cell in CellRect.CenteredOn(center, 6, 6))
            {
                if (cell.InBounds(map) && cell.Standable(map))
                {
                    zone.AddCell(cell);
                }
            }
            if (zone.Cells.Count > 0)
            {
                map.zoneManager.RegisterZone(zone);
            }
        }

        private static void MakeStockpileZone(Map map, IntVec3 center)
        {
            Zone_Stockpile zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            foreach (IntVec3 cell in CellRect.CenteredOn(center, 4, 4))
            {
                if (cell.InBounds(map) && cell.Standable(map))
                {
                    zone.AddCell(cell);
                }
            }
            if (zone.Cells.Count > 0)
            {
                map.zoneManager.RegisterZone(zone);
            }
        }

        private static void SpawnThing(Map map, ThingDef def, ThingDef stuff, int x, int z)
        {
            SpawnThing(map, def, stuff, new IntVec3(x, 0, z));
        }

        private static void SpawnThing(Map map, ThingDef def, ThingDef stuff, IntVec3 cell)
        {
            if (!cell.InBounds(map) || !cell.Standable(map))
            {
                return;
            }
            Thing thing = (def.MadeFromStuff && stuff != null) ? ThingMaker.MakeThing(def, stuff) : ThingMaker.MakeThing(def);
            GenSpawn.Spawn(thing, cell, map, Rot4.North);
        }
    }
}
