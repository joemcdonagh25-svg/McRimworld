using RimWorld;
using Verse;

namespace TheArk.Campaign
{
    /// <summary>
    /// M7: one Pursuit-driven gameplay consequence — vanilla ManhunterPack at BESIEGED+.
    /// Fixed points (not raid-point escalation); supports leave-don't-farm fantasy.
    /// </summary>
    public static class ArkPursuitIncident
    {
        /// <summary>Fire when Pursuit first reaches this value (BESIEGED entry) during a landing.</summary>
        public const int TriggerPursuit = 61;

        /// <summary>
        /// Fixed threat points for the pack. Modest pack size — not scaled with Pursuit
        /// (avoids farming-friendly raid ladders).
        /// </summary>
        public const float FixedManhunterPoints = 350f;

        public static bool TryFireManhunterPack(Map map, string reason, out string failReason)
        {
            if (map == null)
            {
                failReason = "no map";
                return false;
            }

            IncidentDef def = IncidentDefOf.ManhunterPack;
            if (def?.Worker == null)
            {
                failReason = "IncidentDefOf.ManhunterPack unavailable";
                return false;
            }

            var parms = new IncidentParms
            {
                target = map,
                points = FixedManhunterPoints,
                forced = true,
                bypassStorytellerSettings = true
            };

            bool ok = def.Worker.TryExecute(parms);
            if (!ok)
            {
                failReason = "ManhunterPack.TryExecute returned false";
                Log.Warning($"[The Ark] Pursuit incident FAIL ({reason}): {failReason} map={map}");
                return false;
            }

            failReason = null;
            Log.Message(
                $"[The Ark] Pursuit incident FIRED ({reason}): ManhunterPack points={FixedManhunterPoints} " +
                $"mapId={map.uniqueID}");
            return true;
        }
    }
}
