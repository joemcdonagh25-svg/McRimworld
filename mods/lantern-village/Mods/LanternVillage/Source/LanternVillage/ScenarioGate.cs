using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Every patch in this mod is gated through here so that subscribing to this mod
    /// never changes behaviour for any scenario other than The Lantern Village itself.
    /// </summary>
    public static class ScenarioGate
    {
        public const string ScenarioName = "The Lantern Village";
        public const string ScenarioDefName = "LanternVillage_Hamlet";

        public static bool IsActive()
        {
            return IsLanternScenario(Find.Scenario);
        }

        /// <summary>
        /// Prefer this during map-gen hooks: pass the Scenario instance Harmony gives us
        /// instead of relying on Find.Scenario, which can be unset earlier in the pipeline.
        /// </summary>
        public static bool IsLanternScenario(Scenario scen)
        {
            if (scen == null)
            {
                return false;
            }

            if (scen.name == ScenarioName)
            {
                return true;
            }

            // ScenarioDef-backed starts may expose the defName via ToString/file path in some
            // builds; also accept the short label used in ScenarioDef XML.
            return scen.name == "Lantern Village";
        }
    }
}
