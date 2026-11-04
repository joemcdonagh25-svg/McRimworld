using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Belt-and-suspenders hamlet placement. Joe's Player.log showed no hamlet-build
    /// messages at all while HorrorDirector (ScenarioGate-gated GameComponent) did run —
    /// meaning the ScenPart either was not on the loaded scenario instance or its
    /// GenerateIntoMap never fired. Patching Scenario.GenerateIntoMap / PostMapGenerate
    /// by scenario name matches Vampire Lord's "build during map gen" timing without
    /// depending on the ScenPart surviving .rsc deserialization.
    /// </summary>
    [HarmonyPatch(typeof(Scenario), nameof(Scenario.GenerateIntoMap))]
    public static class Patch_Scenario_GenerateIntoMap_Hamlet
    {
        public static void Postfix(Scenario __instance, Map map)
        {
            if (!ScenarioGate.IsLanternScenario(__instance) || map == null)
            {
                return;
            }

            Log.Message("[Lantern Village] Scenario.GenerateIntoMap — ensuring prebuilt hamlet.");
            LanternHamletBuilder.TryBuild(map);
        }
    }

    [HarmonyPatch(typeof(Scenario), nameof(Scenario.PostMapGenerate))]
    public static class Patch_Scenario_PostMapGenerate_Hamlet
    {
        public static void Postfix(Scenario __instance, Map map)
        {
            if (!ScenarioGate.IsLanternScenario(__instance) || map == null)
            {
                return;
            }

            // Second chance if GenerateIntoMap was skipped or failed before the map was ready.
            if (!LanternHamletBuilder.WasBuiltFor(map))
            {
                Log.Message("[Lantern Village] Scenario.PostMapGenerate — hamlet not present yet; building now.");
                LanternHamletBuilder.TryBuild(map);
            }
        }
    }
}
