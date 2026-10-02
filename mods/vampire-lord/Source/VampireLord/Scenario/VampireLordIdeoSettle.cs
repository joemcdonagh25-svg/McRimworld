using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// Ideology settle helpers for the Vampire Lord scenario.
    /// Vanilla <see cref="Page_ChooseIdeoPreset.PostOpen"/> NREs when
    /// <c>Faction.OfPlayer.def.allowedCultures</c> is null — common with odd start stacks.
    /// We harden cultures, assign a classic player ideo, then skip that page.
    /// </summary>
    public static class VampireLordIdeoSettle
    {
        private const string LogPrefix = "[VampireLord]";

        /// <summary>
        /// Ensure player faction can pick a culture, assign a classic ideoligion, and run
        /// <see cref="Scenario.PostIdeoChosen"/> (pawn-config pages depend on this).
        /// </summary>
        public static bool TryApplyClassicAndNotify(string phase)
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

                // Classic mode: playtest keep does not need the Ideo designer UI.
                Find.IdeoManager.classicMode = true;

                IdeoGenerationParms genParms = new IdeoGenerationParms(player.def);
                Ideo classicIdeo = IdeoGenerator.GenerateClassicIdeo(culture, genParms, noExpansionIdeo: false);
                if (classicIdeo == null)
                {
                    Log.Warning($"{LogPrefix} Ideo settle ({phase}): GenerateClassicIdeo returned null.");
                    return false;
                }

                AssignIdeoToPlayer(classicIdeo);

                // Match vanilla Page_ChooseIdeoPreset: fill empty NPC ideos so later settle is quieter.
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

                    FactionDef def = faction.def;
                    if (def == null)
                    {
                        continue;
                    }

                    if (def.fixedIdeo)
                    {
                        IdeoGenerationParms parms = new IdeoGenerationParms(
                            def,
                            forceNoExpansionIdeo: false,
                            null,
                            null,
                            name: def.ideoName,
                            styles: def.styles,
                            deities: def.deityPresets,
                            hidden: def.hiddenIdeo,
                            description: def.ideoDescription,
                            forcedMemes: def.forcedMemes,
                            classicExtra: false,
                            forceNoWeaponPreference: false,
                            forNewFluidIdeo: false,
                            fixedIdeo: true,
                            requiredPreceptsOnly: def.requiredPreceptsOnly);
                        faction.ideos.ChooseOrGenerateIdeo(parms);
                    }
                    else
                    {
                        faction.ideos.ChooseOrGenerateIdeo(new IdeoGenerationParms(def));
                    }
                }

                Find.IdeoManager.RemoveUnusedStartingIdeos();
                Find.Scenario.PostIdeoChosen();

                Log.Message(
                    $"{LogPrefix} Ideo settle ({phase}): classic ideo '{classicIdeo.name}' " +
                    $"(culture={culture.defName}); skipped ChooseIdeoPreset UI.");
                return true;
            }
            catch (System.Exception e)
            {
                Log.Warning($"{LogPrefix} Ideo settle ({phase}) failed: {e.Message}");
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
