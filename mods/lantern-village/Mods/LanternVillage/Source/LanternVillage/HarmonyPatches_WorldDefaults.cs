using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace LanternVillage
{
    /// <summary>
    /// Forces sane world-generation defaults when The Lantern Village scenario is
    /// active, overriding whatever RimWorld's own "Create world" page remembered from
    /// a previous session (it keeps sticky settings across games). In particular
    /// `landmarkDensity` -- RimWorld's term for the named special locations shown on
    /// the world map -- can get left on a low setting from unrelated earlier testing,
    /// which is what "no important locations in this world" turned out to mean.
    /// These are defaults on the page, not locks; the player can still change them.
    /// </summary>
    [HarmonyPatch(typeof(Page_CreateWorldParams))]
    [HarmonyPatch(MethodType.Constructor)]
    public static class Patch_PageCreateWorldParams_Defaults
    {
        public static void Postfix(Page_CreateWorldParams __instance)
        {
            if (!ScenarioGate.IsActive())
            {
                return;
            }

            // Both fields are private on Page_CreateWorldParams -- verified by reflection.
            AccessTools.Field(typeof(Page_CreateWorldParams), "landmarkDensity").SetValue(__instance, LandmarkDensity.Normal);
            AccessTools.Field(typeof(Page_CreateWorldParams), "population").SetValue(__instance, OverallPopulation.Normal);
        }
    }
}
