using System;
using System.IO;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Legacy Founding/Hamlet marker reader. The prebuilt hamlet always generates now;
    /// we force HamletModeRequested = true so older Founding marker files cannot flip it off.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HamletModeLoader
    {
        public static string MarkerFilePath =>
            Path.Combine(GenFilePaths.SaveDataFolderPath, "LanternVillageMode.txt");

        static HamletModeLoader()
        {
            HamletModeState.HamletModeRequested = true;
            try
            {
                string path = MarkerFilePath;
                if (File.Exists(path))
                {
                    string mode = File.ReadAllText(path).Trim();
                    Log.Message(
                        $"[Lantern Village] Launch marker was '{mode}' — prebuilt hamlet always enabled.");
                }
                else
                {
                    Log.Message("[Lantern Village] Launch mode: Hamlet (always on).");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Lantern Village] Failed to read launch-mode marker: {ex}");
            }
        }
    }
}
