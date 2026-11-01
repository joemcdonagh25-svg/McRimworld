using HarmonyLib;
using Verse;

namespace LanternVillage
{
    public class LanternVillageMod : Mod
    {
        public static Harmony HarmonyInstance;

        public LanternVillageMod(ModContentPack content) : base(content)
        {
            HarmonyInstance = new Harmony("lanternvillage.thelanternvillage");
            HarmonyInstance.PatchAll();
            Log.Message("[Lantern Village] Loaded. Supernatural horror events: " +
                        (HorrorDirector.EnableSupernatural ? "ENABLED" : "disabled (behind feature flag)"));
        }
    }
}
