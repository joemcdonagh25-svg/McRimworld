using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// Remembers the playtest keep courtyard / south gate so campaign systems
    /// (fortify, etc.) can place near the entrance without Harmony.
    /// </summary>
    public static class VampireLordKeepLayout
    {
        private static int s_mapId = -1;
        private static IntVec3 s_gateCenter = IntVec3.Invalid;
        private static CellRect s_courtyard = CellRect.Empty;

        public static void Remember(Map map, CellRect courtyard, IntVec3 gateCenter)
        {
            if (map == null)
            {
                return;
            }

            s_mapId = map.uniqueID;
            s_courtyard = courtyard;
            s_gateCenter = gateCenter;
        }

        public static bool TryGetGateCenter(Map map, out IntVec3 gateCenter)
        {
            gateCenter = IntVec3.Invalid;
            if (map == null)
            {
                return false;
            }

            if (s_mapId == map.uniqueID && s_gateCenter.IsValid && s_gateCenter.InBounds(map))
            {
                gateCenter = s_gateCenter;
                return true;
            }

            // Fallback: player start spot, assume south gate on a ~15x15 courtyard.
            IntVec3 start = MapGenerator.PlayerStartSpotValid
                ? MapGenerator.PlayerStartSpot
                : map.Center;
            if (!start.InBounds(map))
            {
                start = map.Center;
            }

            gateCenter = new IntVec3(start.x, 0, start.z - 7);
            if (!gateCenter.InBounds(map))
            {
                gateCenter = start;
            }

            return gateCenter.InBounds(map);
        }

        public static bool TryGetCourtyard(Map map, out CellRect courtyard)
        {
            courtyard = CellRect.Empty;
            if (map != null && s_mapId == map.uniqueID && !s_courtyard.IsEmpty)
            {
                courtyard = s_courtyard;
                return true;
            }

            return false;
        }
    }
}
