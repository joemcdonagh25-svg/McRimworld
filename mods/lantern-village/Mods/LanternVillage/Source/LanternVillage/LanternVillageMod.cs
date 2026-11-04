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

            // Must run here, in this mod's own Mod-class constructor, not in a
            // [StaticConstructorOnStartup] class -- see CompatPatch_MedievalOverhaul's
            // own comment for why timing matters. This mod is declared loadBefore
            // Medieval Overhaul in About.xml so this runs before Medieval Overhaul's
            // own Mod-class constructor does.
            CompatPatch_MedievalOverhaul.Apply();

            Log.Message("[Lantern Village] Loaded. Mist/Darkness Director: enabled by default for The Lantern Village scenario (Dev Mode can toggle).");
        }
    }
}
