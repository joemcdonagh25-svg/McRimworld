using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Runtime ThingDef / TerrainDef resolution for the prebuilt hamlet.
    /// Prefers installed medieval-mod assets when present; falls back to vanilla
    /// thematically appropriate alternatives. Never invents defNames that fail silently
    /// without a logged fallback path.
    /// </summary>
    internal static class LanternHamletDefs
    {
        public static ThingDef ResolveThing(params string[] candidates)
        {
            if (candidates == null)
            {
                return null;
            }

            foreach (string name in candidates)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                if (def != null)
                {
                    return def;
                }
            }

            return null;
        }

        public static TerrainDef ResolveTerrain(params string[] candidates)
        {
            if (candidates == null)
            {
                return null;
            }

            foreach (string name in candidates)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                TerrainDef def = DefDatabase<TerrainDef>.GetNamedSilentFail(name);
                if (def != null)
                {
                    return def;
                }
            }

            return null;
        }

        public static ThingDef WallStuff(ThingDef wall)
        {
            if (wall == null || !wall.MadeFromStuff)
            {
                return null;
            }

            return ResolveThing("WoodLog", "BlocksSandstone", "Steel") ?? ThingDefOf.WoodLog;
        }

        public static ThingDef FurnitureStuff(ThingDef furniture)
        {
            if (furniture == null || !furniture.MadeFromStuff)
            {
                return null;
            }

            return ResolveThing("WoodLog", "BlocksSandstone") ?? ThingDefOf.WoodLog;
        }

        public static ThingDef Wall => ResolveThing(
            "DankPyon_LogWall",
            "DankPyon_Wall",
            "Wall");

        public static ThingDef PalisadeSegment => ResolveThing(
            "DankPyon_Palisade",
            "DankPyon_PalisadeWall",
            "Wall");

        public static ThingDef Door => ResolveThing("Door", "Autodoor");

        public static ThingDef Bed => ResolveThing("Bed", "RoyalBed", "SleepingSpot");

        public static ThingDef Table => ResolveThing("Table2x2c", "Table1x2c", "Table2x4c");

        public static ThingDef Chair => ResolveThing("DiningChair", "Stool");

        public static ThingDef Hearth => ResolveThing("Campfire");

        public static ThingDef Lamp => ResolveThing(
            "TorchLamp",
            "StandingLamp",
            "WallLamp",
            "Candle");

        public static ThingDef Stove => ResolveThing(
            "FueledStove",
            "ElectricStove",
            "Campfire");

        public static ThingDef Smithy => ResolveThing(
            "FueledSmithy",
            "ElectricSmithy",
            "TableMachining",
            "CraftingSpot");

        public static ThingDef CraftSpot => ResolveThing(
            "CraftingSpot",
            "HandTailoringBench",
            "TableSculpting");

        public static ThingDef Shelf => ResolveThing("Shelf", "ShelfSmall");

        public static ThingDef Recreation => ResolveThing(
            "ChessTable",
            "PokerTable",
            "HorseshoesPin",
            "BilliardsTable");

        public static ThingDef Fence => ResolveThing("Fence");

        public static ThingDef Gate => ResolveThing("Gate", "Door");

        public static ThingDef Hitching => ResolveThing("HitchingSpot", "PenMarker");

        public static ThingDef AnimalBed => ResolveThing("AnimalBed", "SleepingSpot");

        public static ThingDef Sandbags => ResolveThing("Sandbags");

        public static ThingDef Column => ResolveThing("Column");

        public static ThingDef PlantCrop => ResolveThing(
            "Plant_Potato",
            "Plant_Corn",
            "Plant_Rice",
            "Plant_Cotton",
            "Plant_Healroot");

        public static TerrainDef FloorInterior => ResolveTerrain(
            "WoodPlankFloor",
            "StrawMatting",
            "FlagstoneSandstone",
            "PavedTile") ?? TerrainDefOf.FlagstoneSandstone;

        public static TerrainDef FloorPath => ResolveTerrain(
            "PackedDirt",
            "StrawMatting",
            "PavedTile") ?? TerrainDefOf.PackedDirt;

        public static TerrainDef FloorGreen => ResolveTerrain(
            "PackedDirt",
            "Soil") ?? TerrainDefOf.Soil;

        public static IEnumerable<string> MissingCriticalSummary()
        {
            if (Wall == null)
            {
                yield return "Wall";
            }

            if (Door == null)
            {
                yield return "Door";
            }

            if (Bed == null)
            {
                yield return "Bed";
            }
        }
    }
}
