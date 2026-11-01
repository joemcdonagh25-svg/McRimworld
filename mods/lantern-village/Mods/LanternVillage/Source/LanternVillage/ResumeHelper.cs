using System;
using System.IO;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Lets "Launch The Lantern Village.bat" resume the existing save without the
    /// player having to click Continue themselves. The launcher writes a one-line
    /// marker file naming the save; this reads it once, at main-menu startup, and
    /// calls the real, verified RimWorld.SavedGameLoaderNow.LoadGameFromSaveFileNow
    /// API -- it does not touch save files directly.
    ///
    /// This has been implemented against RimWorld's public API surface but has not
    /// been runtime-verified in this session (that requires actually launching the
    /// game). See IMPLEMENTATION_REPORT.md.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ResumeHelper
    {
        public static string MarkerFilePath =>
            Path.Combine(GenFilePaths.SaveDataFolderPath, "LanternVillageResume.txt");

        static ResumeHelper()
        {
            LongEventHandler.ExecuteWhenFinished(TryResume);
        }

        private static void TryResume()
        {
            try
            {
                string path = MarkerFilePath;
                if (!File.Exists(path))
                {
                    return;
                }

                string saveName = File.ReadAllText(path).Trim();
                File.Delete(path);

                if (string.IsNullOrEmpty(saveName))
                {
                    return;
                }

                Log.Message($"[Lantern Village] Resume marker found; loading save '{saveName}'.");
                SavedGameLoaderNow.LoadGameFromSaveFileNow(saveName);
            }
            catch (Exception ex)
            {
                Log.Error($"[Lantern Village] Failed to auto-resume: {ex}");
            }
        }
    }
}
