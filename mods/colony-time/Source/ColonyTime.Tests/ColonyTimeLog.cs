using System.Collections.Generic;

namespace ColonyTime
{
    /// <summary>
    /// Test stub — production logging uses Verse.Log via the main assembly.
    /// </summary>
    internal static class ColonyTimeLog
    {
        private static readonly HashSet<string> OnceKeys = new HashSet<string>();

        public static readonly List<string> Messages = new List<string>();
        public static readonly List<string> Warnings = new List<string>();

        public static void Message(string text) => Messages.Add(text);

        public static void Warning(string text) => Warnings.Add(text);

        public static void Error(string text) => Warnings.Add(text);

        public static void WarningOnce(string key, string text)
        {
            if (!OnceKeys.Add(key))
            {
                return;
            }

            Warning(text);
        }

        public static void Reset()
        {
            OnceKeys.Clear();
            Messages.Clear();
            Warnings.Clear();
        }
    }
}
