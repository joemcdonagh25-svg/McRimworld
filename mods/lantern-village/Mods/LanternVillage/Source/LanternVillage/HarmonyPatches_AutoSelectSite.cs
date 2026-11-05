using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Auto-picks a Boreal landing site and advances past the globe page.
    /// Combined with Patch_SettlementProximityGoodwillUtility_CheckConfirmSettle so
    /// DoNext does not stall on the proximity-confirm dialog.
    /// </summary>
    [HarmonyPatch(typeof(Page_SelectStartingSite), nameof(Page_SelectStartingSite.PostOpen))]
    public static class Patch_AutoSelectLandingSite
    {
        private const int ScanLimit = 8000;
        private static bool s_advanceScheduled;

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

                TryAdvance(__instance, "immediate");

                // Also defer — PostOpen can run before the page is fully interactive.
                if (!s_advanceScheduled)
                {
                    s_advanceScheduled = true;
                    LongEventHandler.ExecuteWhenFinished(() =>
                    {
                        s_advanceScheduled = false;
                        TryAdvance(__instance, "deferred");
                    });
                }
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[Lantern Village] Auto-select landing site failed (non-fatal): {ex}");
            }
        }

        private static void TryAdvance(Page_SelectStartingSite page, string phase)
        {
            try
            {
                if (page == null || Find.WindowStack == null)
                {
                    return;
                }

                // Already left the page (e.g. immediate DoNext worked).
                if (!Find.WindowStack.IsOpen(page))
                {
                    return;
                }

                AccessTools.Method(typeof(Page), "DoNext")?.Invoke(page, null);
                Log.Message($"[Lantern Village] Landing-site DoNext invoked ({phase}).");
            }
            catch (System.Exception e)
            {
                Log.Warning($"[Lantern Village] Auto-advance from starting site failed ({phase}): {e.Message}");
            }
        }
    }
}
