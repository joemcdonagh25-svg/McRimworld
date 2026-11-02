using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Works around a real bug in Medieval Overhaul (Steam Workshop 3219596926), found by
    /// disassembling its compiled IL rather than guessing:
    ///
    /// MedievalOverhaul.MedievalOverhaulSettings's constructor calls
    /// Verse.ModLister.GetActiveModWithIdentifier("DankPyon.Medieval.Overhaul", true) purely
    /// to log its own version string, then immediately calls .ModVersion on the result
    /// without a null check. In this RimWorld build that lookup returns null at the point
    /// mod classes are constructed (reproduced with Medieval Overhaul and ONLY its own
    /// declared hard dependencies active -- independent of DLC choice and independent of
    /// every other mod tested), so the constructor throws a NullReferenceException.
    ///
    /// RimWorld's LoadedModManager.CreateModClasses() catches that and logs it as
    /// non-fatal ("Error while instantiating a mod of type ..."), but the damage carries
    /// forward: MedievalOverhaul.Utility's own static constructor later calls
    /// LoadedModManager.GetMod&lt;MedievalOverhaulSettings&gt;().Content -- since that mod
    /// instance was never successfully constructed, GetMod&lt;T&gt;() returns null, and the
    /// unchecked .Content access throws a second, fatal NullReferenceException during
    /// Verse.PlayDataLoader.DoPlayLoad(), which RimWorld cannot recover from.
    ///
    /// Harmony Finalizer patches can swallow an exception thrown by their target method, so
    /// patching MedievalOverhaulSettings's constructor to suppress that first exception lets
    /// it return a (mostly) normal object -- it just means Medieval Overhaul's own
    /// "Medieval Overhaul v X.X.X" version-logging line doesn't print. Everything downstream
    /// of that (Utility's cctor, and Medieval Overhaul's actual gameplay content) then runs
    /// normally.
    ///
    /// Patched via reflection (AccessTools.TypeByName), not a direct type reference, so this
    /// mod has no hard compile-time or load-time dependency on Medieval Overhaul and does
    /// nothing at all if it isn't installed or if a future update fixes the bug and this
    /// patch simply finds nothing to do.
    /// </summary>
    /// <summary>
    /// Called directly from LanternVillageMod's constructor -- NOT [StaticConstructorOnStartup],
    /// which runs after every mod's Mod-class constructor has already executed (too late: by
    /// then Medieval Overhaul's own constructor has already thrown). This mod is declared
    /// loadBefore Medieval Overhaul in About.xml specifically so this patch is in place first.
    /// </summary>
    public static class CompatPatch_MedievalOverhaul
    {
        public static void Apply()
        {
            try
            {
                Type settingsType = AccessTools.TypeByName("MedievalOverhaul.MedievalOverhaulSettings");
                if (settingsType == null)
                {
                    return; // Medieval Overhaul not installed, or this type was renamed/removed -- nothing to patch.
                }

                ConstructorInfo ctor = settingsType.GetConstructors(AccessTools.all)
                    .FirstOrDefault(c => c.GetParameters().Length == 1 &&
                                          c.GetParameters()[0].ParameterType == typeof(ModContentPack));
                if (ctor == null)
                {
                    Log.Warning("[Lantern Village] Medieval Overhaul compat patch: constructor not found (mod may have been updated); skipping.");
                    return;
                }

                HarmonyMethod finalizer = new HarmonyMethod(typeof(CompatPatch_MedievalOverhaul), nameof(SwallowException));
                LanternVillageMod.HarmonyInstance.Patch(ctor, finalizer: finalizer);
                Log.Message("[Lantern Village] Applied Medieval Overhaul compatibility patch (guards its MedievalOverhaulSettings constructor against a known NullReferenceException in its own self-version-lookup code).");
            }
            catch (Exception ex)
            {
                Log.Warning($"[Lantern Village] Medieval Overhaul compat patch failed to apply (non-fatal, Medieval Overhaul will behave as before): {ex}");
            }
        }

        public static Exception SwallowException(Exception __exception)
        {
            if (__exception != null)
            {
                Log.Warning("[Lantern Village] Suppressed a NullReferenceException from Medieval Overhaul's MedievalOverhaulSettings constructor (its own ModLister.GetActiveModWithIdentifier/.ModVersion self-version-logging code) so mod loading can continue.");
            }
            return null; // null = exception swallowed; the original method returns normally.
        }
    }
}
