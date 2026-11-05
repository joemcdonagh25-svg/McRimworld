using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Ensure the starting map is a real player Settlement (Vampire Lord pattern).
    /// Without this, SettlementDefeatUtility can treat the site as defeated on tick 0
    /// and FreeColonists stays empty ("Non-colonist Healer tried to optimize apparel").
    /// </summary>
    public static class LanternVillagePlayerHome
    {
        private const string LogPrefix = "[Lantern Village]";

        public static void TryEnsure(string phase)
        {
            Map map = ResolveMap();
            if (map == null)
            {
                Log.Warning($"{LogPrefix} Ensure player home ({phase}): no map found.");
                return;
            }

            if (map.IsPlayerHome)
            {
                Log.Message($"{LogPrefix} Map already player home ({phase}): Parent={DescribeParent(map.Parent)}.");
                return;
            }

            Faction player = Faction.OfPlayer;
            if (player == null)
            {
                Log.Warning($"{LogPrefix} Ensure player home ({phase}): Faction.OfPlayer null.");
                return;
            }

            string before = DescribeMap(map);

            try
            {
                MapParent parent = map.Parent;
                if (parent is Settlement settlement)
                {
                    if (settlement.Faction != player)
                    {
                        settlement.SetFaction(player);
                    }
                }
                else
                {
                    SettleInExistingMapUtility.Settle(map);
                }

                if (!map.IsPlayerHome && map.Parent != null && map.Parent.Faction != player)
                {
                    map.Parent.SetFaction(player);
                }

                Log.Message(
                    $"{LogPrefix} Ensured player home ({phase}): before={before}; after={DescribeMap(map)}.");
            }
            catch (System.Exception e)
            {
                Log.Warning($"{LogPrefix} Ensure player home ({phase}) failed: {e.Message}");
            }
        }

        private static Map ResolveMap()
        {
            Map current = Find.CurrentMap;
            if (HasColonists(current))
            {
                return current;
            }

            Map anyHome = Find.AnyPlayerHomeMap;
            if (anyHome != null)
            {
                return anyHome;
            }

            List<Map> maps = Find.Maps;
            if (maps != null)
            {
                for (int i = 0; i < maps.Count; i++)
                {
                    if (HasColonists(maps[i]) || LanternHamletBuilder.WasBuiltFor(maps[i]))
                    {
                        return maps[i];
                    }
                }
            }

            return current;
        }

        private static bool HasColonists(Map map)
        {
            return map?.mapPawns != null && map.mapPawns.FreeColonistsSpawnedCount > 0;
        }

        private static string DescribeMap(Map map)
        {
            if (map == null)
            {
                return "null";
            }

            return
                $"IsPlayerHome={map.IsPlayerHome}, Parent={DescribeParent(map.Parent)}, " +
                $"colonists={map.mapPawns?.FreeColonistsSpawnedCount ?? -1}";
        }

        private static string DescribeParent(MapParent parent)
        {
            if (parent == null)
            {
                return "null";
            }

            return $"{parent.def?.defName}:{parent.GetType().Name}/faction={parent.Faction?.def?.defName}";
        }
    }
}
