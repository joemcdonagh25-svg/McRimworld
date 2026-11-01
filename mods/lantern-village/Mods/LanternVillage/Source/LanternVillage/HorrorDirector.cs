using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Modular campaign/phase controller for the Lantern Village horror progression.
    /// Tracks which of the five phases the colony is in by days passed since settling,
    /// and exposes configurable event weights per phase. Supernatural/darkness events
    /// are gated behind EnableSupernatural, which defaults to false -- per the spec,
    /// the installer and scenario must not be blocked on this system being complete,
    /// and nothing here claims an Anomaly-style Unnatural Darkness integration that has
    /// not been verified against this exact mod list.
    /// </summary>
    public class HorrorDirector : GameComponent
    {
        public static bool EnableSupernatural = false;

        public struct Phase
        {
            public string Name;
            public int StartDay;
            public int EndDay;
            public Phase(string name, int startDay, int endDay) { Name = name; StartDay = startDay; EndDay = endDay; }
        }

        public static readonly List<Phase> Phases = new List<Phase>
        {
            new Phase("Foundation", 1, 30),
            new Phase("Folklore threats", 31, 60),
            new Phase("The strange forest", 61, 90),
            new Phase("The darkness awakens", 91, 120),
            new Phase("The last light", 121, 180),
        };

        /// <summary>Per-phase relative weight for "ordinary forest danger" incidents. Configurable, not final content.</summary>
        public Dictionary<string, float> OrdinaryThreatWeight = new Dictionary<string, float>
        {
            { "Foundation", 0.6f },
            { "Folklore threats", 1.0f },
            { "The strange forest", 1.0f },
            { "The darkness awakens", 0.8f },
            { "The last light", 0.6f },
        };

        /// <summary>Per-phase relative weight for folklore/supernatural incidents. Inert while EnableSupernatural is false.</summary>
        public Dictionary<string, float> SupernaturalThreatWeight = new Dictionary<string, float>
        {
            { "Foundation", 0f },
            { "Folklore threats", 0.3f },
            { "The strange forest", 0.7f },
            { "The darkness awakens", 1.2f },
            { "The last light", 1.5f },
        };

        private string lastPhaseName;

        public HorrorDirector(Game game) { }

        public override void GameComponentTick()
        {
            if (!ScenarioGate.IsActive())
            {
                return;
            }

            if (Find.TickManager.TicksGame % 1000 != 0)
            {
                return;
            }

            Phase current = GetCurrentPhase();
            if (current.Name != lastPhaseName)
            {
                lastPhaseName = current.Name;
                Log.Message($"[Lantern Village] Horror director entering phase '{current.Name}' (days {current.StartDay}-{current.EndDay}).");
            }
        }

        public Phase GetCurrentPhase()
        {
            int day = DaysPassedSinceSettle();
            foreach (Phase p in Phases)
            {
                if (day >= p.StartDay && day <= p.EndDay)
                {
                    return p;
                }
            }
            return day > Phases[Phases.Count - 1].EndDay ? Phases[Phases.Count - 1] : Phases[0];
        }

        private static int DaysPassedSinceSettle()
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return 0;
            }
            return GenDate.DaysPassedSinceSettle;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastPhaseName, "lastPhaseName");
        }

        [DebugAction("Lantern Village", "Log current horror phase", requiresIdeology: true, requiresBiotech: true)]
        public static void DebugLogPhase()
        {
            HorrorDirector director = Current.Game?.GetComponent<HorrorDirector>();
            if (director == null)
            {
                Log.Message("[Lantern Village] No HorrorDirector on this game.");
                return;
            }
            Phase p = director.GetCurrentPhase();
            Log.Message($"[Lantern Village] Current phase: {p.Name} (day {DaysPassedSinceSettle()}, supernatural enabled: {EnableSupernatural})");
        }

        [DebugAction("Lantern Village", "Toggle supernatural events", requiresIdeology: true, requiresBiotech: true)]
        public static void DebugToggleSupernatural()
        {
            EnableSupernatural = !EnableSupernatural;
            Log.Message($"[Lantern Village] Supernatural events now {(EnableSupernatural ? "ENABLED" : "disabled")}.");
        }
    }
}
