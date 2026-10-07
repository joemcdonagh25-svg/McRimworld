using System.Linq;
using System.Text;
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
            LongEventHandler.ExecuteWhenFinished(LogInstallAndScenarioStatus);
        }

        private static void LogInstallAndScenarioStatus()
        {
            // Diagnostics only — never let a listing/ConfigErrors failure take down the entry screen.
            try
            {
                LogInstallAndScenarioStatusInner();
            }
            catch (System.Exception e)
            {
                Log.Warning("[The Ark] Startup diagnostics failed (ignored): " + e);
            }
        }

        private static void LogInstallAndScenarioStatusInner()
        {
            ModContentPack pack = LoadedModManager.RunningModsListForReading
                .FirstOrDefault(m => m?.PackageIdPlayerFacing == "joemcdonagh.theark"
                    || m?.PackageId == "joemcdonagh.theark"
                    || (m?.assemblies?.loadedAssemblies != null
                        && m.assemblies.loadedAssemblies.Any(a => a == typeof(TheArkBootstrap).Assembly)));

            if (pack != null)
            {
                Log.Message(
                    "[The Ark] Running mod root='" + pack.RootDir + "' packageId='" + pack.PackageId + "' name='" + pack.Name + "'.");
            }
            else
            {
                Log.Warning(
                    "[The Ark] Could not find running ModContentPack for packageId joemcdonagh.theark. " +
                    "Assembly loaded from '" + typeof(TheArkBootstrap).Assembly.Location + "'.");
            }

            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                if (mod == null)
                {
                    continue;
                }

                string id = mod.PackageId ?? string.Empty;
                string name = mod.Name ?? string.Empty;
                if (id.ToLowerInvariant().Contains("theark") || name.ToLowerInvariant().Contains("the ark"))
                {
                    Log.Message("[The Ark] Related running mod: name='" + name + "' id='" + id + "' root='" + mod.RootDir + "'.");
                }
            }

            ScenarioDef def = DefDatabase<ScenarioDef>.GetNamedSilentFail("TheArk_Playtest");
            if (def == null)
            {
                Log.Error(
                    "[The Ark] ScenarioDef TheArk_Playtest is MISSING from DefDatabase. " +
                    "New Game cannot list it. Fix: junction C:\\McRimworld\\mods\\the-ark-rimworld into RimWorld Mods, " +
                    "enable The Ark, remove any older The Ark folder with the same packageId, restart.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[The Ark] ScenarioDef TheArk_Playtest LOADED.");
            if (def.scenario != null)
            {
                sb.Append(" name='").Append(def.scenario.name).Append("'");
                sb.Append(" showInUI=").Append(def.scenario.showInUI);
                sb.Append(" enabled=").Append(def.scenario.enabled);
            }
            else
            {
                sb.Append(" scenario=NULL");
            }

            Log.Message(sb.ToString());

            foreach (string err in def.ConfigErrors())
            {
                Log.Error("[The Ark] ScenarioDef ConfigError: " + err);
            }

            if (def.scenario != null)
            {
                foreach (string err in def.scenario.ConfigErrors())
                {
                    Log.Error("[The Ark] Scenario ConfigError: " + err);
                }
            }

            bool listed = false;
            foreach (RimWorld.Scenario scen in ScenarioLister.AllScenarios())
            {
                if (scen != null && (scen.name == "The Ark" || scen == def.scenario))
                {
                    listed = true;
                    Log.Message("[The Ark] ScenarioLister CONTAINS The Ark (category=" + scen.Category + ").");
                    break;
                }
            }

            if (!listed)
            {
                Log.Error(
                    "[The Ark] ScenarioDef loaded but ScenarioLister does NOT contain The Ark. " +
                    "Check ConfigError lines above. Paste these [The Ark] log lines to the agent.");
            }
        }
    }
}
