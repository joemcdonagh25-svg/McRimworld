using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ColonyTime
{
    /// <summary>
    /// Isolates private Dialog_FileList member access in one place.
    /// </summary>
    internal static class DialogFileListAccess
    {
        private static readonly FieldInfo FilesField =
            AccessTools.Field(typeof(Dialog_FileList), "files");

        public static List<SaveFileInfo> GetFiles(Dialog_FileList dialog)
        {
            if (dialog == null || FilesField == null)
            {
                return null;
            }

            return FilesField.GetValue(dialog) as List<SaveFileInfo>;
        }
    }
}
