using System;
using System.IO;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Reads the Founding/Hamlet mode choice the launcher wrote before starting RimWorld.
    /// The marker lives next to saves (GenFilePaths.SaveDataFolderPath), same convention
    /// as ResumeHelper's resume marker.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HamletModeLoader
    {
        public static string MarkerFilePath =>
            Path.Combine(GenFilePaths.SaveDataFolderPath, "LanternVillageMode.txt");

        static HamletModeLoader()
        {
            try
            {
                string path = MarkerFilePath;
                if (File.Exists(path))
                {
                    string mode = File.ReadAllText(path).Trim();
                    HamletModeState.HamletModeRequested = mode.Equals("Hamlet", StringComparison.OrdinalIgnoreCase);
                    Log.Message($"[Lantern Village] Launch mode: {(HamletModeState.HamletModeRequested ? "Hamlet" : "Founding")}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Lantern Village] Failed to read launch-mode marker: {ex}");
            }
        }
    }
}
