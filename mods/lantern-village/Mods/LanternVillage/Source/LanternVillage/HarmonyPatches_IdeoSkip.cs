using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// The installer .rsc uses vanilla Scenario (no Class=), so GetFirstConfigPage still
    /// inserts Page_ChooseIdeoPreset — which NREs on this mod stack. Prefix the broken
    /// PostOpen: settle The Hearthbound, advance past the page, skip the vanilla body.
    /// Works for both ScenarioDef (LanternVillageScenario) and the .rsc path.
    /// </summary>
    [HarmonyPatch(typeof(Page_ChooseIdeoPreset), nameof(Page_ChooseIdeoPreset.PostOpen))]
    public static class Patch_Page_ChooseIdeoPreset_PostOpen
    {
        public static bool Prefix(Page_ChooseIdeoPreset __instance)
        {
            if (!ScenarioGate.IsActive())
            {
                return true;
            }

            try
            {
                // Harden cultures before anything else can touch Ideo UI state.
                if (Faction.OfPlayer?.def != null)
                {
                    LanternVillageIdeoSettle.EnsureAllowedCultures(Faction.OfPlayer.def, "ChooseIdeoPreset.Prefix");
                }

                LanternVillageIdeoSettle.TryApplyAndNotify("ChooseIdeoPreset.Prefix");

                // Skip flashing this page when Back is pressed later.
                if (__instance.next != null)
                {
                    __instance.next.prev = __instance.prev;
                }

                Log.Message("[Lantern Village] Skipping broken ChooseIdeoPreset UI (Harmony).");
                // DoNext is protected on Page — invoke via AccessTools.
                AccessTools.Method(typeof(Page), "DoNext")?.Invoke(__instance, null);
            }
            catch (System.Exception e)
            {
                Log.Warning($"[Lantern Village] ChooseIdeoPreset skip failed: {e}");
                // Still skip vanilla PostOpen — it NREs on this stack.
            }

            return false;
        }
    }
}
