using System.Collections.Generic;
using Verse;

namespace ColonyTime
{
    internal static class ColonyTimeLog
    {
        private static readonly HashSet<string> OnceKeys = new HashSet<string>();

        public static void Message(string text)
        {
            Log.Message("[Colony Time] " + text);
        }

        public static void Warning(string text)
        {
            Log.Warning("[Colony Time] " + text);
        }

        public static void Error(string text)
        {
            Log.Error("[Colony Time] " + text);
        }

        public static void WarningOnce(string key, string text)
        {
            if (!OnceKeys.Add(key))
            {
                return;
            }

            Warning(text);
        }
    }
}
