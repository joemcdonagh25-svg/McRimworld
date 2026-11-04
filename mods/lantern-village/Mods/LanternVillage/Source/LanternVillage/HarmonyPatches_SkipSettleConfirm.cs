using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Page_SelectStartingSite.DoNext always goes through
    /// SettlementProximityGoodwillUtility.CheckConfirmSettle, which pops a confirm
    /// dialog when near other settlements — so the player still has to click even
    /// after we auto-select a tile. For The Lantern Village, invoke settleAction
    /// immediately and skip the dialog.
    /// </summary>
    [HarmonyPatch(typeof(SettlementProximityGoodwillUtility), nameof(SettlementProximityGoodwillUtility.CheckConfirmSettle))]
    public static class Patch_SettlementProximityGoodwillUtility_CheckConfirmSettle
    {
        public static bool Prefix(Action settleAction)
        {
            if (!ScenarioGate.IsActive())
            {
                return true;
            }

            try
            {
                Log.Message("[Lantern Village] Auto-confirming landing-site settle (skipping proximity dialog).");
                settleAction?.Invoke();
            }
            catch (Exception e)
            {
                Log.Warning($"[Lantern Village] Auto-confirm settle failed; falling back to vanilla dialog: {e.Message}");
                return true;
            }

            return false;
        }
    }
}
