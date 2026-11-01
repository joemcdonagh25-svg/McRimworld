using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Nudges fresh ideoligion generation toward The Hearthbound's values (community,
    /// charity, hospitality) whenever The Lantern Village scenario is active, by forcing
    /// a couple of real, verified MemeDefs and the ideo's name/description before the
    /// vanilla generator runs. This only touches RimWorld.IdeoGenerator.GenerateIdeo,
    /// a public, documented entry point -- it does not hand-author Ideo save data.
    ///
    /// Note: RimWorld's fluid ideoligions assign rituals (The First Fire, Harvest Supper,
    /// The Long Night, Remembering) dynamically over the course of play. There is no
    /// verified supported API in this RimWorld build to pre-seed specific named rituals
    /// on a fluid ideo, so that part of the spec is intentionally left out here -- see
    /// IMPLEMENTATION_REPORT.md.
    /// </summary>
    [HarmonyPatch(typeof(IdeoGenerator), nameof(IdeoGenerator.GenerateIdeo))]
    public static class Patch_IdeoGenerator_GenerateIdeo
    {
        private static readonly string[] ForcedMemeDefNames = { "Collectivist", "Loyalist" };

        public static void Prefix(ref IdeoGenerationParms parms)
        {
            if (!ScenarioGate.IsActive())
            {
                return;
            }

            if (parms.forcedMemes == null)
            {
                parms.forcedMemes = new List<MemeDef>();
            }

            foreach (string defName in ForcedMemeDefNames)
            {
                MemeDef meme = DefDatabase<MemeDef>.GetNamedSilentFail(defName);
                if (meme == null)
                {
                    Log.Warning($"[Lantern Village] Meme '{defName}' not found in the loaded Ideology definitions; skipping it for The Hearthbound.");
                    continue;
                }
                if (!parms.forcedMemes.Contains(meme))
                {
                    parms.forcedMemes.Add(meme);
                }
            }

            parms.forNewFluidIdeo = true;
            parms.name = "The Hearthbound";
            parms.description = "A belief in community, charity, hospitality and the proper care of the dead. " +
                                 "Those who keep the hearth burning are never truly alone.";
        }
    }
}
