using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Pre-selects Cassandra Classic / "strive to survive" (defName Rough) as the
    /// default highlighted choice on the storyteller page when The Lantern Village
    /// scenario is active. The player can still change it -- this sets a default,
    /// it does not lock the choice, because there is no verified supported way to
    /// force the storyteller/difficulty from the scenario file itself (confirmed by
    /// reflecting RimWorld.ScenPart_* -- no such ScenPart exists in this build).
    /// </summary>
    [HarmonyPatch(typeof(Page_SelectStoryteller))]
    [HarmonyPatch(MethodType.Constructor)]
    public static class Patch_PageSelectStoryteller_Defaults
    {
        public static void Postfix(Page_SelectStoryteller __instance)
        {
            if (!ScenarioGate.IsActive())
            {
                return;
            }

            StorytellerDef cassandra = DefDatabase<StorytellerDef>.GetNamedSilentFail("Cassandra");
            DifficultyDef rough = DefDatabase<DifficultyDef>.GetNamedSilentFail("Rough");

            // Page_SelectStoryteller.storyteller / .difficulty are private fields (verified by
            // reflection), so we go through Harmony's AccessTools rather than a direct field access.
            if (cassandra != null)
            {
                AccessTools.Field(typeof(Page_SelectStoryteller), "storyteller").SetValue(__instance, cassandra);
            }
            else
            {
                Log.Warning("[Lantern Village] StorytellerDef 'Cassandra' not found; leaving default selection.");
            }

            if (rough != null)
            {
                AccessTools.Field(typeof(Page_SelectStoryteller), "difficulty").SetValue(__instance, rough);
            }
            else
            {
                Log.Warning("[Lantern Village] DifficultyDef 'Rough' (Strive to Survive) not found; leaving default selection.");
            }
        }
    }
}
