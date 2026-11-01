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

        public static bool IsActive()
        {
            Scenario scen = Find.Scenario;
            return scen != null && scen.name == ScenarioName;
        }
    }
}
