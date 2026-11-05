using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Ideology settle for The Lantern Village.
    /// Vanilla Page_ChooseIdeoPreset.PostOpen NREs when Faction.OfPlayer.def.allowedCultures
    /// is null (common with PlayerTribe + this mod stack). Harden cultures, assign The
    /// Hearthbound fluid ideo, then skip that page — same approach as Vampire Lord.
    /// </summary>
    public static class LanternVillageIdeoSettle
    {
        private const string LogPrefix = "[Lantern Village]";

        public static bool TryApplyAndNotify(string phase)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return true;
            }

            Faction player = Faction.OfPlayer;
            if (player?.def == null)
            {
                Log.Warning($"{LogPrefix} Ideo settle ({phase}): Faction.OfPlayer or def missing.");
                return false;
            }

            if (player.ideos == null)
            {
                Log.Warning($"{LogPrefix} Ideo settle ({phase}): player ideos tracker missing.");
                return false;
            }

            try
            {
                EnsureAllowedCultures(player.def, phase);
                CultureDef culture = PickCulture(player.def);
                if (culture == null)
                {
                    Log.Warning($"{LogPrefix} Ideo settle ({phase}): no CultureDef available.");
                    return false;
                }

                // Fluid ideo — HarmonyPatches_Ideology forces Hearthbound memes/name when scenario is active.
                Find.IdeoManager.classicMode = false;

                IdeoGenerationParms genParms = new IdeoGenerationParms(player.def)
                {
                    forNewFluidIdeo = true,
                    name = "The Hearthbound",
                    description = "A belief in community, charity, hospitality and the proper care of the dead. " +
                                  "Those who keep the hearth burning are never truly alone."
                };

                // Ensure Collectivist/Loyalist are forced even if the Harmony gate misses.
                List<MemeDef> forced = new List<MemeDef>();
                foreach (string memeName in new[] { "Collectivist", "Loyalist" })
                {
                    MemeDef meme = DefDatabase<MemeDef>.GetNamedSilentFail(memeName);
                    if (meme != null)
                    {
                        forced.Add(meme);
                    }
                }

                if (forced.Count > 0)
                {
                    genParms.forcedMemes = forced;
                }

                Ideo ideo = IdeoGenerator.GenerateIdeo(genParms);
                if (ideo == null)
                {
                    Log.Warning($"{LogPrefix} Ideo settle ({phase}): GenerateIdeo returned null; falling back to classic.");
                    ideo = IdeoGenerator.GenerateClassicIdeo(culture, new IdeoGenerationParms(player.def), noExpansionIdeo: false);
                }

                if (ideo == null)
                {
                    Log.Warning($"{LogPrefix} Ideo settle ({phase}): could not generate any ideo.");
                    return false;
                }

                AssignIdeoToPlayer(ideo);

                foreach (Faction faction in Find.FactionManager.AllFactions)
                {
                    if (faction == null || faction == player || faction.ideos == null)
                    {
                        continue;
                    }

                    Ideo primary = faction.ideos.PrimaryIdeo;
                    if (primary != null && !primary.memes.NullOrEmpty())
                    {
                        continue;
                    }

                    if (faction.def == null)
                    {
                        continue;
                    }

                    faction.ideos.ChooseOrGenerateIdeo(new IdeoGenerationParms(faction.def));
                }

                Find.IdeoManager.RemoveUnusedStartingIdeos();
                Find.Scenario.PostIdeoChosen();

                Log.Message(
                    $"{LogPrefix} Ideo settle ({phase}): '{ideo.name}' " +
                    $"(culture={culture.defName}); skipped ChooseIdeoPreset UI.");
                return true;
            }
            catch (System.Exception e)
            {
                Log.Warning($"{LogPrefix} Ideo settle ({phase}) failed: {e}");
                return false;
            }
        }

        public static void EnsureAllowedCultures(FactionDef factionDef, string phase)
        {
            if (factionDef == null)
            {
                return;
            }

            if (factionDef.allowedCultures == null)
            {
                factionDef.allowedCultures = new List<CultureDef>();
                Log.Message($"{LogPrefix} Ideo settle ({phase}): initialized null allowedCultures on {factionDef.defName}.");
            }

            if (factionDef.allowedCultures.Count > 0)
            {
                return;
            }

            List<CultureDef> all = DefDatabase<CultureDef>.AllDefsListForReading;
            if (all == null || all.Count == 0)
            {
                Log.Warning($"{LogPrefix} Ideo settle ({phase}): DefDatabase<CultureDef> empty.");
                return;
            }

            factionDef.allowedCultures.AddRange(all.Where(c => c != null));
            Log.Message(
                $"{LogPrefix} Ideo settle ({phase}): filled allowedCultures on {factionDef.defName} " +
                $"({factionDef.allowedCultures.Count} cultures).");
        }

        private static CultureDef PickCulture(FactionDef factionDef)
        {
            List<CultureDef> allowed = factionDef.allowedCultures;
            if (allowed != null &&
                allowed.Where(c => c != null).TryRandomElement(out CultureDef fromAllowed))
            {
                return fromAllowed;
            }

            if (DefDatabase<CultureDef>.AllDefsListForReading.Where(c => c != null)
                .TryRandomElement(out CultureDef any))
            {
                return any;
            }

            return DefDatabase<CultureDef>.AllDefs.FirstOrDefault();
        }

        private static void AssignIdeoToPlayer(Ideo ideo)
        {
            Faction.OfPlayer.ideos.SetPrimary(ideo);
            foreach (Ideo existing in Find.IdeoManager.IdeosListForReading)
            {
                existing.initialPlayerIdeo = false;
            }

            ideo.initialPlayerIdeo = true;
            Find.IdeoManager.Add(ideo);
        }
    }
}
