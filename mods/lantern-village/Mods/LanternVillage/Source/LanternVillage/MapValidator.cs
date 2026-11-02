using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Scores tiles of the currently generated world against The Lantern Village's
    /// target site (temperate forest, large hills, long growing season, coastal/river
    /// a bonus) and returns the best candidates within a bounded scan. This does not
    /// regenerate the world across multiple seeds automatically -- that would require
    /// driving the world-creation UI flow (Page_CreateWorldParams), which has not been
    /// verified in this session. What is implemented here is the bounded tile-scoring
    /// routine itself, callable from a debug action or a future launcher hook.
    /// </summary>
    public static class MapValidator
    {
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

            if (tile.PrimaryBiome?.defName == "TemperateForest")
            {
                score += 10f;
            }
            else
            {
                return float.NegativeInfinity; // biome is a hard requirement, not just a preference
            }

            if (tile.hilliness == Hilliness.LargeHills)
            {
                score += 5f;
            }
            else if (tile.hilliness == Hilliness.SmallHills)
            {
                score += 2f;
            }

            // Longer growing season roughly correlates with warmer average temperature
            // for a temperate biome; this is an approximation, not a verified vanilla
            // "40/60" formula (no such literal field exists on Tile).
            if (tile.temperature >= 10f && tile.temperature <= 18f)
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
        /// Same scan as ScoreTile, but without the hard TemperateForest requirement --
        /// just avoids water and prefers hills/mild temperature. Used only as a fallback
        /// when no temperate forest tile turns up within the scan limit (e.g. a world
        /// seed with little temperate forest coverage), so automatic selection always
        /// finds *something* playable rather than leaving the player stuck.
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
                if (tile.hilliness == Hilliness.LargeHills) score += 5f;
                else if (tile.hilliness == Hilliness.SmallHills) score += 2f;
                if (tile.temperature >= 5f && tile.temperature <= 22f) score += 3f;
                if (tile.IsCoastal) score += 1f;

                results.Add(new Candidate { Tile = tile, Score = score });
            }

            return results.OrderByDescending(c => c.Score).Take(maxResults).ToList();
        }

        [DebugAction("Lantern Village", "Find best hamlet tile (scan 2000)", requiresIdeology: true, requiresBiotech: true)]
        public static void DebugFindBestTile()
        {
            List<Candidate> best = FindBestTiles(2000, 5);
            if (best.Count == 0)
            {
                Log.Message("[Lantern Village] No matching temperate forest tile found in the first 2000 scanned.");
                return;
            }
            foreach (Candidate c in best)
            {
                Log.Message($"[Lantern Village] Candidate tile score {c.Score}: hilliness={c.Tile.hilliness}, temp={c.Tile.temperature}, coastal={c.Tile.IsCoastal}");
            }
        }
    }
}
