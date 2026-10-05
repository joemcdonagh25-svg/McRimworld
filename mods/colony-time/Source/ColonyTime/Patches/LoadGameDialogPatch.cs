using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonyTime.Patches
{
    /// <summary>
    /// Narrow Load Game UI integration for Colony Time V0.
    /// Targets (RimWorld 1.6.4871):
    /// - RimWorld.Dialog_FileList.DoWindowContents(Rect)
    /// - RimWorld.Dialog_FileList.DrawDateAndVersion(SaveFileInfo, Rect)
    /// - RimWorld.Dialog_SaveFileList.ReloadFiles()
    /// </summary>
    [HarmonyPatch(typeof(Dialog_FileList), nameof(Dialog_FileList.DoWindowContents))]
    internal static class Dialog_FileList_DoWindowContents_Patch
    {
        internal static Dialog_FileList Current;
        internal static SaveSortMode SortMode = SaveSortMode.LastPlayed;
        internal static bool EntryHeightPatched;

        private static SaveSortMode lastAppliedSort = (SaveSortMode)(-1);
        private static int lastFileCount = -1;
        private static int lastResortGeneration = -1;

        private const float SortBarHeight = 28f;
        private const float EnhancedRowHeight = 72f;
        private const float VanillaRowHeight = 40f;

        /// <summary>
        /// Used by the transpiler. Must stay parameterless (call replaces ldc.r4).
        /// </summary>
        public static float GetEntryHeight()
        {
            return IsSaveFileDialog(Current) ? EnhancedRowHeight : VanillaRowHeight;
        }

        public static bool IsSaveFileDialog(Dialog_FileList dialog)
        {
            return dialog is Dialog_SaveFileList;
        }

        public static bool IsLoadGameDialog(Dialog_FileList dialog)
        {
            return dialog is Dialog_SaveFileList_Load;
        }

        static void Prefix(Dialog_FileList __instance, ref Rect inRect)
        {
            Current = __instance;

            if (!IsLoadGameDialog(__instance))
            {
                return;
            }

            DrawSortBar(ref inRect);
            ApplySortIfNeeded(__instance);
        }

        static void Postfix(Dialog_FileList __instance)
        {
            if (Current == __instance)
            {
                Current = null;
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var list = instructions.ToList();
            var getter = AccessTools.Method(typeof(Dialog_FileList_DoWindowContents_Patch), nameof(GetEntryHeight));
            int replaced = 0;

            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction ins = list[i];
                if (ins.opcode == OpCodes.Ldc_R4 && ins.operand is float f && Math.Abs(f - VanillaRowHeight) < 0.01f)
                {
                    list[i] = new CodeInstruction(OpCodes.Call, getter);
                    replaced++;
                }
            }

            EntryHeightPatched = replaced > 0;
            if (!EntryHeightPatched)
            {
                ColonyTimeLog.Warning(
                    "DoWindowContents transpiler found no " + VanillaRowHeight +
                    "f entry-height constants - using side labels instead of taller rows.");
            }

            return list;
        }

        private static void DrawSortBar(ref Rect inRect)
        {
            Rect bar = new Rect(inRect.x, inRect.y, inRect.width, SortBarHeight);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            float labelWidth = 70f;
            Widgets.Label(new Rect(bar.x, bar.y, labelWidth, bar.height), "Sort:");

            Rect buttonRect = new Rect(bar.x + labelWidth, bar.y, 180f, bar.height);
            if (Widgets.ButtonText(buttonRect, SortModeLabel(SortMode)))
            {
                var options = new List<FloatMenuOption>
                {
                    new FloatMenuOption(SortModeLabel(SaveSortMode.LastPlayed), () => SortMode = SaveSortMode.LastPlayed),
                    new FloatMenuOption(SortModeLabel(SaveSortMode.HoursPlayed), () => SortMode = SaveSortMode.HoursPlayed),
                    new FloatMenuOption(SortModeLabel(SaveSortMode.ColonyAge), () => SortMode = SaveSortMode.ColonyAge),
                    new FloatMenuOption(SortModeLabel(SaveSortMode.Name), () => SortMode = SaveSortMode.Name)
                };
                Find.WindowStack.Add(new FloatMenu(options));
            }

            Text.Anchor = TextAnchor.UpperLeft;
            inRect.yMin += SortBarHeight + 4f;
        }

        private static string SortModeLabel(SaveSortMode mode)
        {
            switch (mode)
            {
                case SaveSortMode.LastPlayed:
                    return "Last played";
                case SaveSortMode.HoursPlayed:
                    return "Hours played";
                case SaveSortMode.ColonyAge:
                    return "Colony age";
                case SaveSortMode.Name:
                    return "Name";
                default:
                    return mode.ToString();
            }
        }

        private static void ApplySortIfNeeded(Dialog_FileList dialog)
        {
            List<SaveFileInfo> files = DialogFileListAccess.GetFiles(dialog);
            if (files == null || files.Count == 0)
            {
                return;
            }

            if (SortMode == lastAppliedSort
                && files.Count == lastFileCount
                && lastResortGeneration == ResortMarker.Generation)
            {
                return;
            }

            switch (SortMode)
            {
                case SaveSortMode.LastPlayed:
                    files.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
                    break;
                case SaveSortMode.HoursPlayed:
                    files.Sort((a, b) =>
                    {
                        double av = SaveMetadataCache.Instance.GetOrRead(SafePath(a)).RealPlayTimeSeconds ?? -1;
                        double bv = SaveMetadataCache.Instance.GetOrRead(SafePath(b)).RealPlayTimeSeconds ?? -1;
                        int cmp = bv.CompareTo(av);
                        return cmp != 0 ? cmp : b.LastWriteTime.CompareTo(a.LastWriteTime);
                    });
                    break;
                case SaveSortMode.ColonyAge:
                    files.Sort((a, b) =>
                    {
                        long av = SaveMetadataCache.Instance.GetOrRead(SafePath(a)).TicksGame ?? -1;
                        long bv = SaveMetadataCache.Instance.GetOrRead(SafePath(b)).TicksGame ?? -1;
                        int cmp = bv.CompareTo(av);
                        return cmp != 0 ? cmp : b.LastWriteTime.CompareTo(a.LastWriteTime);
                    });
                    break;
                case SaveSortMode.Name:
                    files.Sort((a, b) =>
                    {
                        string an = SafeName(a);
                        string bn = SafeName(b);
                        int cmp = string.Compare(an, bn, StringComparison.OrdinalIgnoreCase);
                        return cmp != 0 ? cmp : b.LastWriteTime.CompareTo(a.LastWriteTime);
                    });
                    break;
            }

            lastAppliedSort = SortMode;
            lastFileCount = files.Count;
            lastResortGeneration = ResortMarker.Generation;
        }

        private static string SafePath(SaveFileInfo sfi)
        {
            try
            {
                return sfi?.FileInfo?.FullName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string SafeName(SaveFileInfo sfi)
        {
            try
            {
                if (!string.IsNullOrEmpty(sfi.FileName))
                {
                    return Path.GetFileNameWithoutExtension(sfi.FileName);
                }

                return Path.GetFileNameWithoutExtension(sfi.FileInfo.Name);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_FileList), nameof(Dialog_FileList.DrawDateAndVersion))]
    internal static class Dialog_FileList_DrawDateAndVersion_Patch
    {
        private const float ExtraInfoWidth = 150f;

        // Brighter than vanilla UnimportantTextColor (50% alpha) so Played/Colony stay readable.
        private static readonly Color MetaLabelColor = new Color(0.92f, 0.92f, 0.78f, 1f);
        private static readonly Color DateColor = new Color(1f, 1f, 1f, 0.7f);

        /// <summary>
        /// When row height was successfully increased, replace the date/version block
        /// with date, version, played, colony. Otherwise draw played/colony to the left
        /// and let vanilla draw date/version unchanged.
        /// </summary>
        static bool Prefix(SaveFileInfo sfi, Rect rect)
        {
            if (!Dialog_FileList_DoWindowContents_Patch.IsSaveFileDialog(Dialog_FileList_DoWindowContents_Patch.Current))
            {
                return true;
            }

            SaveMetadata meta = SaveMetadataCache.Instance.GetOrRead(SafePath(sfi));

            if (Dialog_FileList_DoWindowContents_Patch.EntryHeightPatched)
            {
                DrawEnhancedBlock(sfi, rect, meta);
                return false;
            }

            DrawSideLabels(rect, meta);
            return true;
        }

        private static void DrawEnhancedBlock(SaveFileInfo sfi, Rect rect, SaveMetadata meta)
        {
            Rect drawRect = rect;
            drawRect.xMin -= ExtraInfoWidth;
            drawRect.width += ExtraInfoWidth;

            Widgets.BeginGroup(drawRect);

            // Top: date + version (vanilla-style Tiny).
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;

            float topLine = 16f;
            GUI.color = DateColor;
            Widgets.Label(new Rect(0f, 1f, drawRect.width, topLine), sfi.LastWriteTime.ToString("g"));

            GUI.color = sfi.VersionColor;
            Rect versionRect = new Rect(0f, topLine, drawRect.width, topLine);
            Widgets.Label(versionRect, sfi.GameVersion);
            TooltipHandler.TipRegion(versionRect, sfi.CompatibilityTip);

            // Bottom: Played / Colony in Small + brighter color for readability.
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = MetaLabelColor;

            float metaY = topLine * 2f + 2f;
            float metaLine = (drawRect.height - metaY) / 2f;
            Widgets.Label(new Rect(0f, metaY, drawRect.width, metaLine), SaveTimeFormatter.FormatPlayedLine(meta.RealPlayTimeSeconds));
            Widgets.Label(new Rect(0f, metaY + metaLine, drawRect.width, metaLine), SaveTimeFormatter.FormatColonyLine(meta.TicksGame));

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndGroup();
        }

        private static void DrawSideLabels(Rect rect, SaveMetadata meta)
        {
            Rect side = new Rect(rect.x - ExtraInfoWidth, rect.y, ExtraInfoWidth - 4f, rect.height);
            Widgets.BeginGroup(side);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = MetaLabelColor;

            float half = side.height / 2f;
            Widgets.Label(new Rect(0f, 0f, side.width, half), SaveTimeFormatter.FormatPlayedLine(meta.RealPlayTimeSeconds));
            Widgets.Label(new Rect(0f, half, side.width, half), SaveTimeFormatter.FormatColonyLine(meta.TicksGame));

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndGroup();
        }

        private static string SafePath(SaveFileInfo sfi)
        {
            try
            {
                return sfi?.FileInfo?.FullName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_SaveFileList), "ReloadFiles")]
    internal static class Dialog_SaveFileList_ReloadFiles_Patch
    {
        static void Postfix(Dialog_SaveFileList __instance)
        {
            List<SaveFileInfo> files = DialogFileListAccess.GetFiles(__instance);
            if (files == null)
            {
                ResortMarker.Mark();
                return;
            }

            var paths = new List<string>(files.Count);
            for (int i = 0; i < files.Count; i++)
            {
                try
                {
                    string path = files[i]?.FileInfo?.FullName;
                    if (!string.IsNullOrEmpty(path))
                    {
                        paths.Add(path);
                    }
                }
                catch
                {
                    // ignore individual bad entries
                }
            }

            SaveMetadataCache.Instance.PruneMissing(paths);
            ResortMarker.Mark();
        }
    }

    /// <summary>
    /// Lets ReloadFiles invalidate the sort cache without exposing mutable internals widely.
    /// </summary>
    internal static class ResortMarker
    {
        public static int Generation;

        public static void Mark()
        {
            Generation++;
        }
    }
}
