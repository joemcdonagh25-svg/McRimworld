using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Auto-picks a Boreal landing site and advances past the globe page so the player
    /// is not stuck selecting a tile. Safe to DoNext now that ChooseIdeoPreset is
    /// Harmony-guarded for this scenario.
    /// </summary>
    [HarmonyPatch(typeof(Page_SelectStartingSite), nameof(Page_SelectStartingSite.PostOpen))]
    public static class Patch_AutoSelectLandingSite
    {
        private const int ScanLimit = 8000;

        public static void Postfix(Page_SelectStartingSite __instance)
        {
            if (!ScenarioGate.IsActive())
            {
                return;
            }

            try
            {
                MapValidator.Candidate? best = MapValidator.FindBestTiles(ScanLimit, 1).Cast<MapValidator.Candidate?>().FirstOrDefault();
                string fallbackNote = "";
                if (best == null)
                {
                    best = MapValidator.FindBestTilesRelaxed(ScanLimit, 1).Cast<MapValidator.Candidate?>().FirstOrDefault();
                    fallbackNote = " (no Boreal Forest tile found within scan limit; used a relaxed match instead)";
                }

                if (best == null)
                {
                    Log.Warning("[Lantern Village] Auto-select landing site: no candidate tile found; leaving manual selection.");
                    return;
                }

                Find.WorldInterface.SelectedTile = best.Value.Tile.tile;
                Log.Message(
                    $"[Lantern Village] Auto-selected Boreal landing site (score {best.Value.Score}){fallbackNote}; advancing.");

                // Defer DoNext so PostOpen finishes and the window is fully on the stack.
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    try
                    {
                        if (__instance != null && Find.WindowStack != null && Find.WindowStack.WindowOfType<Page_SelectStartingSite>() == __instance)
                        {
                            // DoNext is protected on Page — invoke via AccessTools.
                            AccessTools.Method(typeof(Page), "DoNext")?.Invoke(__instance, null);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Log.Warning($"[Lantern Village] Auto-advance from starting site failed (press Next manually): {e.Message}");
                    }
                });
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[Lantern Village] Auto-select landing site failed (non-fatal): {ex}");
            }
        }
    }
}
