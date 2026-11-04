using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Scores tiles for The Lantern Village. Preferred biome is Boreal Forest
    /// (cold woodland folk-horror clearing).
    /// </summary>
    public static class MapValidator
    {
        public const string PreferredBiomeDefName = "BorealForest";

        public struct Candidate
        {
            public Tile Tile;
            public float Score;
        }

        public static float ScoreTile(Tile tile)
        {
            if (tile == null || tile.WaterCovered)
            {
                return float.NegativeInfinity;
            }

            float score = 0f;

            if (tile.PrimaryBiome?.defName == PreferredBiomeDefName)
            {
                score += 10f;
            }
            else
            {
                return float.NegativeInfinity; // biome is a hard requirement
            }

            if (tile.hilliness == Hilliness.LargeHills)
            {
                score += 5f;
            }
            else if (tile.hilliness == Hilliness.SmallHills)
            {
                score += 2f;
            }

            // Boreal: prefer cool, not arctic-dead.
            if (tile.temperature >= -5f && tile.temperature <= 10f)
            {
                score += 3f;
            }

            if (tile.IsCoastal)
            {
                score += 1f;
            }

            return score;
        }

        public static List<Candidate> FindBestTiles(int maxTilesToScan, int maxResults)
        {
            var results = new List<Candidate>();
            WorldGrid grid = Find.WorldGrid;
            if (grid == null)
            {
                return results;
            }

            int scanned = 0;
            foreach (Tile tile in grid.Tiles)
            {
                if (scanned >= maxTilesToScan)
                {
                    break;
                }
                scanned++;

                float score = ScoreTile(tile);
                if (float.IsNegativeInfinity(score))
                {
                    continue;
                }
                results.Add(new Candidate { Tile = tile, Score = score });
            }

            return results.OrderByDescending(c => c.Score).Take(maxResults).ToList();
        }

        /// <summary>
        /// Fallback when no Boreal Forest tile is found within the scan limit.
        /// Prefers other cold forest biomes, then any walkable land with hills.
        /// </summary>
        public static List<Candidate> FindBestTilesRelaxed(int maxTilesToScan, int maxResults)
        {
            var results = new List<Candidate>();
            WorldGrid grid = Find.WorldGrid;
            if (grid == null)
            {
                return results;
            }

            int scanned = 0;
            foreach (Tile tile in grid.Tiles)
            {
                if (scanned >= maxTilesToScan)
                {
                    break;
                }
                scanned++;

                if (tile == null || tile.WaterCovered)
                {
                    continue;
                }

                float score = 0f;
                string biome = tile.PrimaryBiome?.defName;
                if (biome == PreferredBiomeDefName)
                {
                    score += 10f;
                }
                else if (biome == "TemperateForest" || biome == "ColdBog" || biome == "Tundra")
                {
                    score += 4f;
                }
                else
                {
                    score += 1f;
                }

                if (tile.hilliness == Hilliness.LargeHills)
                {
                    score += 5f;
                }
                else if (tile.hilliness == Hilliness.SmallHills)
                {
                    score += 2f;
                }

                results.Add(new Candidate { Tile = tile, Score = score });
            }

            return results.OrderByDescending(c => c.Score).Take(maxResults).ToList();
        }

        [DebugAction("Lantern Village", "Log best landing tiles", requiresIdeology: true, requiresBiotech: true)]
        public static void DebugLogBestTiles()
        {
            List<Candidate> best = FindBestTiles(8000, 5);
            if (best.Count == 0)
            {
                best = FindBestTilesRelaxed(8000, 5);
                Log.Message("[Lantern Village] No strict BorealForest tiles; showing relaxed matches:");
            }

            foreach (Candidate c in best)
            {
                Log.Message($"[Lantern Village] candidate score={c.Score} biome={c.Tile.PrimaryBiome?.defName} hilliness={c.Tile.hilliness} temp={c.Tile.temperature}");
            }
        }
    }
}
