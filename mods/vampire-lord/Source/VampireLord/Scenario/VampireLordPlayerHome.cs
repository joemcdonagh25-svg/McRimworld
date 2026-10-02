using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// Playtest settle can leave the keep as a Camp / non-home MapParent so RimWorld
    /// treats the player like a caravan (no IsPlayerHome, odd faction UI). Force a real
    /// player Settlement when needed.
    /// </summary>
    public static class VampireLordPlayerHome
    {
        private const string LogPrefix = "[VampireLord]";

        public static void TryEnsure(string phase)
        {
            Map map = ResolveKeepMap();
            if (map == null)
            {
                Log.Warning($"{LogPrefix} Ensure player home ({phase}): no keep map found.");
                return;
            }

            if (map.IsPlayerHome)
            {
                Log.Message(
                    $"{LogPrefix} Keep map already player home ({phase}): " +
                    $"Parent={DescribeParent(map.Parent)}.");
                VampireLordFactionHygiene.TryRepair(phase);
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

                    // Settlement + player faction can still report !IsPlayerHome until settle bookkeeping runs.
                    if (!map.IsPlayerHome)
                    {
                        settlement.Notify_MyMapSettled(map);
                    }
                }
                else
                {
                    // Camp / site / other temporary parent → same path as caravan "Settle here".
                    SettleInExistingMapUtility.Settle(map);
                }

                if (!map.IsPlayerHome && map.Parent != null && map.Parent.Faction != player)
                {
                    map.Parent.SetFaction(player);
                }

                if (!map.IsPlayerHome && map.Parent != null)
                {
                    map.Parent.Notify_MyMapSettled(map);
                }

                Log.Message(
                    $"{LogPrefix} Ensured player home ({phase}): before={before}; " +
                    $"after={DescribeMap(map)}.");
            }
            catch (System.Exception e)
            {
                Log.Warning($"{LogPrefix} Ensure player home ({phase}) failed: {e.Message}");
            }

            VampireLordFactionHygiene.TryRepair(phase);
        }

        private static Map ResolveKeepMap()
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
