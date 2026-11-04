using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Builds the prebuilt Lantern Village medieval hamlet into a freshly generated map.
    /// Pattern mirrors Vampire Lord's ScenPart courtyard placement: clear soft things,
    /// spawn real buildings via ThingMaker/GenSpawn (never blueprints), set floors/roofs,
    /// and run only during new-map generation — not on save load.
    /// </summary>
    public static class LanternHamletBuilder
    {
        private const string LogPrefix = "[Lantern Village]";
        private const int VillageWidth = 40;
        private const int VillageHeight = 45;

        private static int s_lastBuiltMapId = -1;

        public static bool WasBuiltFor(Map map)
        {
            return map != null && s_lastBuiltMapId == map.uniqueID;
        }

        public static bool TryBuild(Map map)
        {
            if (map == null)
            {
                Log.Warning($"{LogPrefix} TryBuild called with null map.");
                return false;
            }

            Log.Message($"{LogPrefix} TryBuild starting on map {map.uniqueID} (size {map.Size}).");

            if (s_lastBuiltMapId == map.uniqueID)
            {
                Log.Message($"{LogPrefix} Hamlet already built for map {map.uniqueID}; skipping.");
                return false;
            }

            ThingDef wall = LanternHamletDefs.Wall;
            ThingDef door = LanternHamletDefs.Door;
            ThingDef wood = ThingDefOf.WoodLog;

            List<string> missing = LanternHamletDefs.MissingCriticalSummary().ToList();
            if (missing.Count > 0 || wall == null || door == null || wood == null)
            {
                Log.Warning($"{LogPrefix} Cannot build hamlet; missing critical defs: {string.Join(", ", missing)}");
                return false;
            }

            try
            {
                IntVec3 center = ResolveVillageCenter(map);
                CellRect village = CellRect.CenteredOn(center, VillageWidth / 2, VillageHeight / 2).ClipInsideMap(map);
                if (village.Width < 24 || village.Height < 28)
                {
                    // Fall back to map center if the start spot sits on the edge.
                    center = map.Center;
                    village = CellRect.CenteredOn(center, VillageWidth / 2, VillageHeight / 2).ClipInsideMap(map);
                }

                Faction player = Faction.OfPlayer;
                TerrainDef floor = LanternHamletDefs.FloorInterior;
                TerrainDef path = LanternHamletDefs.FloorPath;
                TerrainDef green = LanternHamletDefs.FloorGreen;

                // Organic relative layout (offsets from village min corner, not a rigid grid).
                IntVec3 origin = new IntVec3(village.minX, 0, village.minZ);
                CellRect greatHall = RectAt(origin, 14, 30, 11, 9);
                CellRect cottageA = RectAt(origin, 3, 28, 7, 7);
                CellRect cottageB = RectAt(origin, 4, 18, 7, 7);
                CellRect cottageC = RectAt(origin, 28, 27, 7, 7);
                CellRect kitchen = RectAt(origin, 26, 18, 8, 7);
                CellRect smithy = RectAt(origin, 29, 9, 8, 7);
                CellRect barn = RectAt(origin, 4, 7, 9, 8);
                CellRect gardenWest = RectAt(origin, 15, 6, 6, 6);
                CellRect gardenEast = RectAt(origin, 22, 5, 6, 6);
                CellRect greenRect = RectAt(origin, 14, 16, 11, 10);
                CellRect watchpost = RectAt(origin, 17, 1, 5, 4);

                ClearFootprint(map, village, clearItems: false);

                // Soft village-green floor and paths before buildings.
                Pave(map, greenRect, green);
                CarvePath(map, greatHall.CenterCell, greenRect.CenterCell, path);
                CarvePath(map, cottageA.CenterCell, greenRect.CenterCell, path);
                CarvePath(map, cottageB.CenterCell, greenRect.CenterCell, path);
                CarvePath(map, cottageC.CenterCell, greenRect.CenterCell, path);
                CarvePath(map, kitchen.CenterCell, greenRect.CenterCell, path);
                CarvePath(map, smithy.CenterCell, greenRect.CenterCell, path);
                CarvePath(map, barn.CenterCell, greenRect.CenterCell, path);
                CarvePath(map, greenRect.CenterCell, watchpost.CenterCell, path);

                BuildRoom(map, greatHall, wall, door, wood, floor, player, Rot4.South);
                FurnishGreatHall(map, greatHall, player, wood);

                BuildRoom(map, cottageA, wall, door, wood, floor, player, Rot4.East);
                FurnishCottage(map, cottageA, player, wood, beds: 2);

                BuildRoom(map, cottageB, wall, door, wood, floor, player, Rot4.East);
                FurnishCottage(map, cottageB, player, wood, beds: 2);

                BuildRoom(map, cottageC, wall, door, wood, floor, player, Rot4.West);
                FurnishCottage(map, cottageC, player, wood, beds: 1);

                BuildRoom(map, kitchen, wall, door, wood, floor, player, Rot4.West);
                FurnishKitchen(map, kitchen, player, wood);

                BuildRoom(map, smithy, wall, door, wood, floor, player, Rot4.West);
                FurnishSmithy(map, smithy, player, wood);

                BuildRoom(map, barn, wall, door, wood, floor, player, Rot4.East);
                FurnishBarn(map, barn, player, wood);

                MakeGrowingZone(map, gardenWest, "West garden");
                MakeGrowingZone(map, gardenEast, "East garden");
                TryPlantCrops(map, gardenWest);
                TryPlantCrops(map, gardenEast);

                FurnishVillageGreen(map, greenRect, player, wood);
                BuildPartialPalisade(map, village, watchpost, player, wood, path);
                BuildWatchpost(map, watchpost, player, wood);

                MapGenerator.PlayerStartSpot = greenRect.CenterCell.ClampInsideMap(map);
                s_lastBuiltMapId = map.uniqueID;

                Log.Message(
                    $"{LogPrefix} Prebuilt hamlet placed at {center} " +
                    $"(village {village.Width}x{village.Height}, map={map.uniqueID}).");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"{LogPrefix} Hamlet build failed: {ex}");
                return false;
            }
        }

        private static IntVec3 ResolveVillageCenter(Map map)
        {
            IntVec3 center = MapGenerator.PlayerStartSpotValid
                ? MapGenerator.PlayerStartSpot
                : map.Center;

            if (!center.InBounds(map))
            {
                center = map.Center;
            }

            // Nudge off water / impassable if the start spot is awkward.
            if (!IsBuildableFootprintCell(map, center))
            {
                foreach (IntVec3 candidate in GenRadial.RadialCellsAround(center, 12f, true))
                {
                    if (candidate.InBounds(map) && IsBuildableFootprintCell(map, candidate))
                    {
                        return candidate;
                    }
                }
            }

            return center;
        }

        private static bool IsBuildableFootprintCell(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map))
            {
                return false;
            }

            TerrainDef terrain = cell.GetTerrain(map);
            if (terrain == null || terrain.IsWater)
            {
                return false;
            }

            // Plants can make a cell look blocked; Walkable is enough for a village footprint.
            return cell.Walkable(map);
        }

        private static CellRect RectAt(IntVec3 origin, int localX, int localZ, int width, int height)
        {
            IntVec3 min = new IntVec3(origin.x + localX, 0, origin.z + localZ);
            return CellRect.FromLimits(min.x, min.z, min.x + width - 1, min.z + height - 1);
        }

        private static void ClearFootprint(Map map, CellRect rect, bool clearItems)
        {
            foreach (IntVec3 cell in rect.Cells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                TerrainDef terrain = cell.GetTerrain(map);
                if (terrain != null && terrain.IsWater)
                {
                    continue;
                }

                ClearCell(map, cell, clearItems);
            }
        }

        private static void ClearCell(Map map, IntVec3 cell, bool clearItems)
        {
            List<Thing> things = cell.GetThingList(map).ToList();
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing?.def == null)
                {
                    continue;
                }

                // Never destroy pawns or non-plant animals during build.
                if (thing is Pawn)
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

        private static void Pave(Map map, CellRect rect, TerrainDef terrain)
        {
            if (terrain == null)
            {
                return;
            }

            foreach (IntVec3 cell in rect.Cells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                TerrainDef existing = cell.GetTerrain(map);
                if (existing != null && existing.IsWater)
                {
                    continue;
                }

                map.terrainGrid.SetTerrain(cell, terrain);
            }
        }

        private static void CarvePath(Map map, IntVec3 from, IntVec3 to, TerrainDef path)
        {
            if (path == null)
            {
                return;
            }

            foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(from, to))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                TerrainDef existing = cell.GetTerrain(map);
                if (existing != null && existing.IsWater)
                {
                    continue;
                }

                ClearCell(map, cell, clearItems: false);
                if (cell.GetEdifice(map) == null)
                {
                    map.terrainGrid.SetTerrain(cell, path);
                }
            }
        }

        private static void BuildRoom(
            Map map,
            CellRect exterior,
            ThingDef wallDef,
            ThingDef doorDef,
            ThingDef stuff,
            TerrainDef floor,
            Faction player,
            Rot4 doorFacing)
        {
            exterior = exterior.ClipInsideMap(map);
            if (exterior.Width < 3 || exterior.Height < 3)
            {
                return;
            }

            CellRect interior = exterior.ContractedBy(1);
            int waterCells = 0;
            foreach (IntVec3 cell in exterior.Cells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                TerrainDef existing = cell.GetTerrain(map);
                if (existing != null && existing.IsWater)
                {
                    waterCells++;
                    continue;
                }

                ClearCell(map, cell, clearItems: true);
            }

            // Skip rooms that would be mostly flooded.
            if (waterCells > Math.Max(2, exterior.Area / 5))
            {
                Log.Warning($"{LogPrefix} Skipping room at {exterior.CenterCell}: too much water ({waterCells} cells).");
                return;
            }

            Pave(map, interior, floor);

            IntVec3 doorCell = exterior.GetCenterCellOnEdge(doorFacing);
            if (!doorCell.InBounds(map))
            {
                doorCell = new IntVec3(exterior.CenterCell.x, 0, exterior.minZ);
            }

            ThingDef wallStuff = LanternHamletDefs.WallStuff(wallDef) ?? stuff;
            ThingDef doorStuff = doorDef != null && doorDef.MadeFromStuff ? wallStuff : null;

            foreach (IntVec3 cell in exterior.EdgeCells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                ClearCell(map, cell, clearItems: true);
                if (cell == doorCell && doorDef != null)
                {
                    SpawnBuilding(doorDef, doorStuff, cell, map, player, doorFacing);
                }
                else
                {
                    SpawnBuilding(wallDef, wallStuff, cell, map, player, Rot4.North);
                }
            }

            RoofInterior(map, interior);
        }

        private static void RoofInterior(Map map, CellRect interior)
        {
            RoofDef roof = RoofDefOf.RoofConstructed;
            foreach (IntVec3 cell in interior.Cells)
            {
                if (cell.InBounds(map))
                {
                    map.roofGrid.SetRoof(cell, roof);
                }
            }
        }

        private static void FurnishGreatHall(Map map, CellRect exterior, Faction player, ThingDef wood)
        {
            CellRect interior = exterior.ContractedBy(1);
            IntVec3 center = interior.CenterCell;

            SpawnBuilding(LanternHamletDefs.Hearth, null, center, map, player, Rot4.North);

            ThingDef table = LanternHamletDefs.Table;
            ThingDef chair = LanternHamletDefs.Chair;
            ThingDef lamp = LanternHamletDefs.Lamp;
            ThingDef recreation = LanternHamletDefs.Recreation;

            IntVec3 tableCell = center + new IntVec3(0, 0, -2);
            SpawnBuilding(table, LanternHamletDefs.FurnitureStuff(table) ?? wood, tableCell, map, player, Rot4.North);

            if (chair != null)
            {
                ThingDef chairStuff = LanternHamletDefs.FurnitureStuff(chair) ?? wood;
                SpawnBuilding(chair, chairStuff, tableCell + IntVec3.West, map, player, Rot4.East);
                SpawnBuilding(chair, chairStuff, tableCell + IntVec3.East, map, player, Rot4.West);
                SpawnBuilding(chair, chairStuff, tableCell + IntVec3.South, map, player, Rot4.North);
                SpawnBuilding(chair, chairStuff, tableCell + IntVec3.North, map, player, Rot4.South);
            }

            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.minX, 0, interior.minZ), map, player, Rot4.North);
            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.maxX, 0, interior.maxZ), map, player, Rot4.North);
            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.minX, 0, interior.maxZ), map, player, Rot4.North);
            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.maxX, 0, interior.minZ), map, player, Rot4.North);

            if (recreation != null)
            {
                SpawnBuilding(
                    recreation,
                    LanternHamletDefs.FurnitureStuff(recreation) ?? wood,
                    new IntVec3(interior.maxX - 1, 0, interior.minZ + 1),
                    map,
                    player,
                    Rot4.North);
            }
        }

        private static void FurnishCottage(Map map, CellRect exterior, Faction player, ThingDef wood, int beds)
        {
            CellRect interior = exterior.ContractedBy(1);
            ThingDef bedDef = LanternHamletDefs.Bed;
            ThingDef lamp = LanternHamletDefs.Lamp;
            ThingDef shelf = LanternHamletDefs.Shelf;

            int placed = 0;
            foreach (IntVec3 cell in interior.Cells)
            {
                if (placed >= beds || bedDef == null)
                {
                    break;
                }

                // Keep the south edge clearer for door traffic when door faces south/east/west.
                if (cell.z == interior.minZ)
                {
                    continue;
                }

                if (SpawnBuilding(bedDef, LanternHamletDefs.FurnitureStuff(bedDef) ?? wood, cell, map, player, Rot4.South))
                {
                    placed++;
                }
            }

            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.minX, 0, interior.maxZ), map, player, Rot4.North);
            if (shelf != null)
            {
                SpawnBuilding(shelf, LanternHamletDefs.FurnitureStuff(shelf) ?? wood, new IntVec3(interior.maxX, 0, interior.maxZ), map, player, Rot4.South);
            }
        }

        private static void FurnishKitchen(Map map, CellRect exterior, Faction player, ThingDef wood)
        {
            CellRect interior = exterior.ContractedBy(1);
            ThingDef stove = LanternHamletDefs.Stove;
            ThingDef shelf = LanternHamletDefs.Shelf;
            ThingDef table = LanternHamletDefs.Table;
            ThingDef lamp = LanternHamletDefs.Lamp;

            SpawnBuilding(stove, LanternHamletDefs.FurnitureStuff(stove), new IntVec3(interior.minX, 0, interior.maxZ), map, player, Rot4.South);
            SpawnBuilding(table, LanternHamletDefs.FurnitureStuff(table) ?? wood, interior.CenterCell, map, player, Rot4.North);
            SpawnBuilding(shelf, LanternHamletDefs.FurnitureStuff(shelf) ?? wood, new IntVec3(interior.maxX, 0, interior.maxZ), map, player, Rot4.South);
            SpawnBuilding(shelf, LanternHamletDefs.FurnitureStuff(shelf) ?? wood, new IntVec3(interior.maxX, 0, interior.maxZ - 1), map, player, Rot4.South);
            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.minX, 0, interior.minZ), map, player, Rot4.North);

            MakeStockpileZone(map, CellRect.FromLimits(interior.minX + 1, interior.minZ, interior.maxX - 1, interior.minZ + 1), StorageSettingsPreset.DefaultStockpile, "Pantry");
        }

        private static void FurnishSmithy(Map map, CellRect exterior, Faction player, ThingDef wood)
        {
            CellRect interior = exterior.ContractedBy(1);
            ThingDef smithy = LanternHamletDefs.Smithy;
            ThingDef craft = LanternHamletDefs.CraftSpot;
            ThingDef shelf = LanternHamletDefs.Shelf;
            ThingDef lamp = LanternHamletDefs.Lamp;

            SpawnBuilding(smithy, LanternHamletDefs.FurnitureStuff(smithy), new IntVec3(interior.minX, 0, interior.CenterCell.z), map, player, Rot4.East);
            SpawnBuilding(craft, LanternHamletDefs.FurnitureStuff(craft), new IntVec3(interior.maxX, 0, interior.CenterCell.z), map, player, Rot4.West);
            SpawnBuilding(shelf, LanternHamletDefs.FurnitureStuff(shelf) ?? wood, new IntVec3(interior.CenterCell.x, 0, interior.maxZ), map, player, Rot4.South);
            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.minX, 0, interior.minZ), map, player, Rot4.North);

            MakeStockpileZone(map, CellRect.FromLimits(interior.minX + 1, interior.minZ, interior.maxX - 1, interior.minZ), StorageSettingsPreset.DefaultStockpile, "Smithy stores");
        }

        private static void FurnishBarn(Map map, CellRect exterior, Faction player, ThingDef wood)
        {
            CellRect interior = exterior.ContractedBy(1);
            ThingDef hitch = LanternHamletDefs.Hitching;
            ThingDef animalBed = LanternHamletDefs.AnimalBed;
            ThingDef lamp = LanternHamletDefs.Lamp;
            ThingDef shelf = LanternHamletDefs.Shelf;

            SpawnBuilding(hitch, null, new IntVec3(interior.minX + 1, 0, interior.CenterCell.z), map, player, Rot4.North);
            SpawnBuilding(hitch, null, new IntVec3(interior.maxX - 1, 0, interior.CenterCell.z), map, player, Rot4.North);
            SpawnBuilding(animalBed, LanternHamletDefs.FurnitureStuff(animalBed) ?? wood, new IntVec3(interior.minX + 1, 0, interior.maxZ), map, player, Rot4.South);
            SpawnBuilding(animalBed, LanternHamletDefs.FurnitureStuff(animalBed) ?? wood, new IntVec3(interior.maxX - 1, 0, interior.maxZ), map, player, Rot4.South);
            SpawnBuilding(shelf, LanternHamletDefs.FurnitureStuff(shelf) ?? wood, new IntVec3(interior.CenterCell.x, 0, interior.minZ), map, player, Rot4.North);
            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, new IntVec3(interior.minX, 0, interior.minZ), map, player, Rot4.North);

            // Drop a little hay if the def exists so animals have something to eat immediately.
            ThingDef hay = DefDatabase<ThingDef>.GetNamedSilentFail("Hay");
            if (hay != null)
            {
                IntVec3 hayCell = new IntVec3(interior.CenterCell.x, 0, interior.minZ + 1);
                if (hayCell.InBounds(map) && hayCell.Standable(map) && hayCell.GetEdifice(map) == null)
                {
                    Thing stack = ThingMaker.MakeThing(hay);
                    stack.stackCount = Math.Min(75, hay.stackLimit);
                    GenSpawn.Spawn(stack, hayCell, map, WipeMode.Vanish);
                }
            }
        }

        private static void FurnishVillageGreen(Map map, CellRect green, Faction player, ThingDef wood)
        {
            ThingDef lamp = LanternHamletDefs.Lamp;
            ThingDef chair = LanternHamletDefs.Chair;
            ThingDef column = LanternHamletDefs.Column;

            // Warm lantern ring around the green.
            IntVec3[] lampSpots =
            {
                new IntVec3(green.minX, 0, green.minZ),
                new IntVec3(green.maxX, 0, green.minZ),
                new IntVec3(green.minX, 0, green.maxZ),
                new IntVec3(green.maxX, 0, green.maxZ),
                green.CenterCell + new IntVec3(-3, 0, 0),
                green.CenterCell + new IntVec3(3, 0, 0),
            };

            foreach (IntVec3 spot in lampSpots)
            {
                if (spot.InBounds(map) && spot.GetEdifice(map) == null)
                {
                    SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, spot, map, player, Rot4.North);
                }
            }

            if (chair != null)
            {
                ThingDef chairStuff = LanternHamletDefs.FurnitureStuff(chair) ?? wood;
                SpawnBuilding(chair, chairStuff, green.CenterCell + new IntVec3(-1, 0, 1), map, player, Rot4.South);
                SpawnBuilding(chair, chairStuff, green.CenterCell + new IntVec3(1, 0, 1), map, player, Rot4.South);
            }

            // Leave a couple of decorative columns / posts if available; otherwise skip.
            if (column != null)
            {
                SpawnBuilding(column, LanternHamletDefs.FurnitureStuff(column) ?? wood, green.CenterCell + new IntVec3(-4, 0, -3), map, player, Rot4.North);
                SpawnBuilding(column, LanternHamletDefs.FurnitureStuff(column) ?? wood, green.CenterCell + new IntVec3(4, 0, -3), map, player, Rot4.North);
            }

            // Preserve / plant a few trees on the green edges for woodland character.
            ThingDef tree = DefDatabase<ThingDef>.GetNamedSilentFail("Plant_TreeOak")
                ?? DefDatabase<ThingDef>.GetNamedSilentFail("Plant_TreePine")
                ?? DefDatabase<ThingDef>.GetNamedSilentFail("Plant_TreePoplar");
            if (tree != null)
            {
                TrySpawnPlant(map, tree, new IntVec3(green.minX + 1, 0, green.maxZ - 1));
                TrySpawnPlant(map, tree, new IntVec3(green.maxX - 1, 0, green.maxZ - 1));
            }
        }

        private static void BuildPartialPalisade(Map map, CellRect village, CellRect watchpost, Faction player, ThingDef wood, TerrainDef path)
        {
            ThingDef palisade = LanternHamletDefs.PalisadeSegment ?? LanternHamletDefs.Wall;
            if (palisade == null)
            {
                return;
            }

            ThingDef stuff = LanternHamletDefs.WallStuff(palisade) ?? wood;
            IntVec3 gateCenter = new IntVec3(watchpost.CenterCell.x, 0, village.minZ);
            if (!gateCenter.InBounds(map))
            {
                gateCenter = village.GetCenterCellOnEdge(Rot4.South);
            }

            const int gateHalfWidth = 1; // 3-cell open gate with a real door in the middle when available

            // Incomplete ring: full south with gate, most of east/west, partial north gaps (organic unfinished look).
            foreach (IntVec3 cell in village.EdgeCells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                bool onSouth = cell.z == village.minZ;
                bool onNorth = cell.z == village.maxZ;
                bool onWest = cell.x == village.minX;
                bool onEast = cell.x == village.maxX;

                // Leave intentional gaps on the north edge so it feels unfinished, not fortress-complete.
                if (onNorth && (cell.x % 5 == 0 || cell.x % 5 == 1))
                {
                    continue;
                }

                // Skip corners of the north gaps already handled; also skip a few west/east mid gaps.
                if ((onWest || onEast) && cell.z > village.minZ + 8 && cell.z < village.maxZ - 8 && cell.z % 7 == 0)
                {
                    continue;
                }

                if (onSouth && cell.x >= gateCenter.x - gateHalfWidth && cell.x <= gateCenter.x + gateHalfWidth)
                {
                    ClearCell(map, cell, clearItems: true);
                    map.terrainGrid.SetTerrain(cell, path);
                    if (cell.x == gateCenter.x)
                    {
                        ThingDef gate = LanternHamletDefs.Gate ?? LanternHamletDefs.Door;
                        SpawnBuilding(gate, gate != null && gate.MadeFromStuff ? stuff : null, cell, map, player, Rot4.South);
                    }

                    continue;
                }

                if (!(onSouth || onNorth || onWest || onEast))
                {
                    continue;
                }

                ClearCell(map, cell, clearItems: true);
                SpawnBuilding(palisade, stuff, cell, map, player, Rot4.North);
            }

            // Short cleared approach south of the gate (Vampire Lord approach-lane pattern).
            for (int dz = 1; dz <= 5; dz++)
            {
                for (int dx = -gateHalfWidth; dx <= gateHalfWidth; dx++)
                {
                    IntVec3 cell = new IntVec3(gateCenter.x + dx, 0, gateCenter.z - dz);
                    if (!cell.InBounds(map))
                    {
                        continue;
                    }

                    ClearCell(map, cell, clearItems: false);
                    if (cell.GetEdifice(map) == null)
                    {
                        map.terrainGrid.SetTerrain(cell, path);
                    }
                }
            }
        }

        private static void BuildWatchpost(Map map, CellRect watch, Faction player, ThingDef wood)
        {
            watch = watch.ClipInsideMap(map);
            ThingDef sandbags = LanternHamletDefs.Sandbags;
            ThingDef lamp = LanternHamletDefs.Lamp;
            ThingDef wall = LanternHamletDefs.Wall;
            ThingDef stuff = LanternHamletDefs.WallStuff(wall) ?? wood;

            // Three-sided cover facing south (forest approach), open to the village north.
            foreach (IntVec3 cell in watch.EdgeCells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                if (cell.z == watch.maxZ)
                {
                    continue; // open toward village
                }

                ClearCell(map, cell, clearItems: true);
                if (sandbags != null)
                {
                    SpawnBuilding(sandbags, LanternHamletDefs.FurnitureStuff(sandbags) ?? wood, cell, map, player, Rot4.North);
                }
                else if (wall != null)
                {
                    SpawnBuilding(wall, stuff, cell, map, player, Rot4.North);
                }
            }

            SpawnBuilding(lamp, LanternHamletDefs.FurnitureStuff(lamp) ?? wood, watch.CenterCell, map, player, Rot4.North);
        }

        private static void MakeGrowingZone(Map map, CellRect rect, string label)
        {
            rect = rect.ClipInsideMap(map);
            Zone_Growing zone = new Zone_Growing(map.zoneManager)
            {
                label = label
            };

            foreach (IntVec3 cell in rect.Cells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                TerrainDef terrain = cell.GetTerrain(map);
                if (terrain != null && terrain.IsWater)
                {
                    continue;
                }

                ClearCell(map, cell, clearItems: false);
                if (cell.GetEdifice(map) == null && (cell.Standable(map) || cell.GetTerrain(map) != null))
                {
                    if (!cell.GetTerrain(map).IsWater)
                    {
                        zone.AddCell(cell);
                    }
                }
            }

            if (zone.Cells.Count > 0)
            {
                map.zoneManager.RegisterZone(zone);
            }
        }

        private static void MakeStockpileZone(Map map, CellRect rect, StorageSettingsPreset preset, string label)
        {
            rect = rect.ClipInsideMap(map);
            Zone_Stockpile zone = new Zone_Stockpile(preset, map.zoneManager)
            {
                label = label
            };

            foreach (IntVec3 cell in rect.Cells)
            {
                if (cell.InBounds(map) && cell.Standable(map) && cell.GetEdifice(map) == null)
                {
                    zone.AddCell(cell);
                }
            }

            if (zone.Cells.Count > 0)
            {
                map.zoneManager.RegisterZone(zone);
            }
        }

        private static void TryPlantCrops(Map map, CellRect rect)
        {
            ThingDef crop = LanternHamletDefs.PlantCrop;
            if (crop == null)
            {
                return;
            }

            foreach (IntVec3 cell in rect.Cells)
            {
                if (!cell.InBounds(map) || cell.GetEdifice(map) != null)
                {
                    continue;
                }

                if (!PlantUtility.GrowthSeasonNow(cell, map, crop))
                {
                    continue;
                }

                TrySpawnPlant(map, crop, cell);
            }
        }

        private static void TrySpawnPlant(Map map, ThingDef plantDef, IntVec3 cell)
        {
            if (plantDef == null || !cell.InBounds(map) || cell.GetEdifice(map) != null)
            {
                return;
            }

            if (cell.GetPlant(map) != null)
            {
                return;
            }

            try
            {
                Plant plant = (Plant)ThingMaker.MakeThing(plantDef);
                plant.Growth = 0.35f;
                GenSpawn.Spawn(plant, cell, map, WipeMode.Vanish);
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogPrefix} Could not spawn plant {plantDef.defName} at {cell}: {ex.Message}");
            }
        }

        private static bool SpawnBuilding(ThingDef def, ThingDef stuff, IntVec3 cell, Map map, Faction player, Rot4 rot)
        {
            if (def == null || !cell.InBounds(map))
            {
                return false;
            }

            // Do not spawn over water or existing edifices.
            TerrainDef terrain = cell.GetTerrain(map);
            if (terrain != null && terrain.IsWater)
            {
                return false;
            }

            if (cell.GetEdifice(map) != null)
            {
                return false;
            }

            // Multi-cell buildings need a clear footprint.
            CellRect occupied = GenAdj.OccupiedRect(cell, rot, def.size);
            foreach (IntVec3 c in occupied)
            {
                if (!c.InBounds(map) || c.GetEdifice(map) != null)
                {
                    return false;
                }

                TerrainDef t = c.GetTerrain(map);
                if (t != null && t.IsWater)
                {
                    return false;
                }
            }

            try
            {
                foreach (IntVec3 c in occupied)
                {
                    ClearCell(map, c, clearItems: true);
                }

                Thing thing = def.MadeFromStuff && stuff != null
                    ? ThingMaker.MakeThing(def, stuff)
                    : ThingMaker.MakeThing(def);

                if (player != null && thing.def.CanHaveFaction)
                {
                    thing.SetFactionDirect(player);
                }

                GenSpawn.Spawn(thing, cell, map, rot, WipeMode.Vanish);

                if (player != null && thing.def.CanHaveFaction && thing.Faction != player)
                {
                    thing.SetFaction(player);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogPrefix} Failed to spawn {def.defName} at {cell}: {ex.Message}");
                return false;
            }
        }
    }
}
