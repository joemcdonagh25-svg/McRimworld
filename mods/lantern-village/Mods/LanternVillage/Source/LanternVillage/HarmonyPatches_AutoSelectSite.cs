using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Automatically picks a landing site when The Lantern Village scenario is active,
    /// instead of leaving the player to scroll the globe and guess. Scores a bounded
    /// scan of the generated world's tiles with MapValidator (temperate forest, large
    /// hills, mild growing-season temperature, coastal a bonus), sets the real,
    /// confirmed-settable Verse.WorldInterface.SelectedTile to the best one, and calls
    /// the page's own DoNext() to advance -- the same thing clicking "Next" does.
    ///
    /// Falls back to MapValidator's relaxed scorer (no strict biome requirement) if no
    /// temperate forest tile turns up within the scan limit, so this never leaves the
    /// player stuck with nothing selected. If even that finds nothing (scan limit set
    /// too low, or a world with no land at all), it does nothing and leaves the normal
    /// manual-selection flow in place.
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
                    fallbackNote = " (no temperate forest tile found within scan limit; used a relaxed match instead)";
                }

                if (best == null)
                {
                    Log.Warning("[Lantern Village] Auto-select landing site: no candidate tile found at all; leaving manual selection in place.");
                    return;
                }

                Find.WorldInterface.SelectedTile = best.Value.Tile.tile;
                Log.Message($"[Lantern Village] Auto-selected landing site (score {best.Value.Score}){fallbackNote}.");

                // DoNext() is protected; this is the exact method Page's own "Next" button calls.
                AccessTools.Method(typeof(Page_SelectStartingSite), "DoNext").Invoke(__instance, null);
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[Lantern Village] Auto-select landing site failed (non-fatal, falling back to manual selection): {ex}");
            }
        }
    }
}
