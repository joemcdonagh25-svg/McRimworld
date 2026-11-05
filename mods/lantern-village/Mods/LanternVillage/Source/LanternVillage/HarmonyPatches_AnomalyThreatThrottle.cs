using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// When The Lantern Village is active and Anomaly is installed, the vanilla
    /// storyteller can dump multiple Anomaly threats (fleshbeast attacks, etc.)
    /// into the first weeks. Allow at most one such incident during the Foundation
    /// window, and clamp its points so it is a small fight rather than a swarm.
    /// </summary>
    [HarmonyPatch(typeof(Storyteller), nameof(Storyteller.TryFire))]
    public static class Patch_Storyteller_TryFire_AnomalyThrottle
    {
        public static bool Prefix(FiringIncident fi, ref bool __result)
        {
            if (!ShouldThrottle(fi))
            {
                return true;
            }

            AnomalyStartThrottle throttle = AnomalyStartThrottle.Get();
            if (throttle == null)
            {
                return true;
            }

            if (throttle.earlyAnomalyIncidentsFired >= AnomalyStartThrottle.MaxEarlyAnomalyIncidents)
            {
                Log.Message(
                    $"[Lantern Village] Blocked early Anomaly incident '{fi.def.defName}' " +
                    $"(already fired {throttle.earlyAnomalyIncidentsFired} in first " +
                    $"{AnomalyStartThrottle.EarlyDaysInclusive} days).");
                __result = false;
                return false;
            }

            if (fi.parms != null && fi.parms.points > AnomalyStartThrottle.EarlyAnomalyPointsCap)
            {
                float before = fi.parms.points;
                fi.parms.points = AnomalyStartThrottle.EarlyAnomalyPointsCap;
                Log.Message(
                    $"[Lantern Village] Clamped early Anomaly incident '{fi.def.defName}' " +
                    $"points {before:0.#} → {fi.parms.points:0.#}.");
            }

            return true;
        }

        public static void Postfix(FiringIncident fi, bool __result)
        {
            if (!__result || !ShouldThrottle(fi))
            {
                return;
            }

            AnomalyStartThrottle throttle = AnomalyStartThrottle.Get();
            if (throttle == null)
            {
                return;
            }

            throttle.earlyAnomalyIncidentsFired++;
            Log.Message(
                $"[Lantern Village] Allowed early Anomaly incident '{fi.def.defName}' " +
                $"({throttle.earlyAnomalyIncidentsFired}/" +
                $"{AnomalyStartThrottle.MaxEarlyAnomalyIncidents} in first " +
                $"{AnomalyStartThrottle.EarlyDaysInclusive} days).");
        }

        private static bool ShouldThrottle(FiringIncident fi)
        {
            if (!ScenarioGate.IsActive() || !ModsConfig.AnomalyActive)
            {
                return false;
            }

            if (fi?.def == null || !fi.def.IsAnomalyIncident)
            {
                return false;
            }

            return AnomalyStartThrottle.IsInEarlyWindow();
        }
    }
}
