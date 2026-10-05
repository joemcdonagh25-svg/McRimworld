using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ColonyTime
{
    public sealed class ColonyTimeMod : Mod
    {
        public const string HarmonyId = "joemcdonagh.colonytime";

        public ColonyTimeMod(ModContentPack content) : base(content)
        {
            try
            {
                var harmony = new Harmony(HarmonyId);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                ColonyTimeLog.Message("Initialized.");
            }
            catch (Exception ex)
            {
                ColonyTimeLog.Error("Harmony patch failure — Load Game extras disabled. " + ex);
            }
        }
    }
}
