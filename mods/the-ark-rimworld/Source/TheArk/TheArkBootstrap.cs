using RimWorld;
using Verse;

namespace TheArk
{
    [StaticConstructorOnStartup]
    public static class TheArkBootstrap
    {
        static TheArkBootstrap()
        {
            Log.Message("[The Ark] Initialised successfully.");
            // Defs finish loading after static ctors; log once the queue drains so Joe can verify in Player.log.
            LongEventHandler.ExecuteWhenFinished(LogScenarioDefStatus);
        }

        private static void LogScenarioDefStatus()
        {
            ScenarioDef def = DefDatabase<ScenarioDef>.GetNamedSilentFail("TheArk_Playtest");
            if (def == null)
            {
                Log.Warning(
                    "[The Ark] ScenarioDef TheArk_Playtest is MISSING. " +
                    "New Game will not list The Ark. Check: Mods list has The Ark enabled; " +
                    "mod folder is C:\\McRimworld\\mods\\the-ark-rimworld (must contain About\\ and Defs\\); " +
                    "no second old The Ark mod with the same packageId.");
                return;
            }

            Log.Message(
                "[The Ark] ScenarioDef TheArk_Playtest LOADED (label='" + def.label + "'). " +
                "It should appear under New Game scenarios.");
        }
    }
}
