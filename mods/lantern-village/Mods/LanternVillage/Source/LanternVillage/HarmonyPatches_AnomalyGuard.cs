using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// GameComponent_Anomaly.StartedNewGame NREs on some full-DLC stacks when Anomaly
    /// state is incomplete at InitNewGame. Swallow only that NRE so the colony can start;
    /// other exceptions still propagate.
    /// </summary>
    [HarmonyPatch(typeof(GameComponent_Anomaly), nameof(GameComponent_Anomaly.StartedNewGame))]
    public static class Patch_GameComponent_Anomaly_StartedNewGame
    {
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                Log.Warning(
                    "[Lantern Village] Suppressed NullReferenceException from " +
                    "GameComponent_Anomaly.StartedNewGame so new-game init can continue.");
                return null;
            }

            return __exception;
        }
    }
}
