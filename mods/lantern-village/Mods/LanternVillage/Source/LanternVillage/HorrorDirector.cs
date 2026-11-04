using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Mist / Darkness Director for The Lantern Village.
    /// Escalates forest mist (ForceWeather Fog / FoggyRain) and short Eclipse windows
    /// across five campaign phases. Deliberately avoids Anomaly UnnaturalDarkness.
    /// Schedule pattern mirrors Vampire Lord's absolute-tick Wave Director.
    /// </summary>
    public class HorrorDirector : GameComponent
    {
        public const string LogPrefix = "[Lantern Village]";

        /// <summary>Master switch. Default ON so mist/darkness is real gameplay; Dev Mode can toggle.</summary>
        public bool supernaturalEnabled = true;

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

        private string lastPhaseName;
        private int nextPulseTick = -1;
        private int pulsesFiredInPhase;
        private bool announcedCampaign;

        public HorrorDirector(Game game) { }

        public override void GameComponentTick()
        {
            if (!ScenarioGate.IsActive())
            {
                return;
            }

            if (Find.TickManager.TicksGame % 250 != 0)
            {
                return;
            }

            if (!announcedCampaign)
            {
                announcedCampaign = true;
                Log.Message($"{LogPrefix} Mist/Darkness Director active (supernatural={(supernaturalEnabled ? "ON" : "off")}).");
            }

            Phase current = GetCurrentPhase();
            if (current.Name != lastPhaseName)
            {
                OnPhaseEntered(current);
            }

            if (!supernaturalEnabled)
            {
                return;
            }

            EnsurePulseScheduled(current);

            int now = Find.TickManager.TicksGame;
            if (nextPulseTick > 0 && now >= nextPulseTick)
            {
                FirePulse(current);
                ScheduleNextPulse(current, afterPulse: true);
            }
        }

        private void OnPhaseEntered(Phase phase)
        {
            lastPhaseName = phase.Name;
            pulsesFiredInPhase = 0;
            Log.Message($"{LogPrefix} Horror director entering phase '{phase.Name}' (days {phase.StartDay}-{phase.EndDay}).");
            SendLetter(
                PhaseLetterTitle(phase),
                PhaseLetterBody(phase),
                phase.Name == "Foundation" || phase.Name == "Folklore threats"
                    ? LetterDefOf.NeutralEvent
                    : LetterDefOf.NegativeEvent);

            if (supernaturalEnabled)
            {
                // First pulse of a new phase lands soon so the player feels the shift.
                nextPulseTick = Find.TickManager.TicksGame + DaysToTicks(PhaseFirstPulseDelayDays(phase));
            }
        }

        private void EnsurePulseScheduled(Phase phase)
        {
            if (nextPulseTick < 0)
            {
                ScheduleNextPulse(phase, afterPulse: false);
            }
        }

        private void ScheduleNextPulse(Phase phase, bool afterPulse)
        {
            float minDays = PhasePulseMinDays(phase);
            float maxDays = PhasePulseMaxDays(phase);
            if (afterPulse)
            {
                // Slight jitter so pulses don't feel metronomic.
                float span = Math.Max(0.1f, maxDays - minDays);
                float delay = minDays + (float)Rand.Value * span;
                nextPulseTick = Find.TickManager.TicksGame + DaysToTicks(delay);
            }
            else if (nextPulseTick < 0)
            {
                nextPulseTick = Find.TickManager.TicksGame + DaysToTicks(PhaseFirstPulseDelayDays(phase));
            }

            Log.Message($"{LogPrefix} Next mist/darkness pulse in ~{TicksToDays(nextPulseTick - Find.TickManager.TicksGame):0.0} days (phase '{phase.Name}').");
        }

        private void FirePulse(Phase phase)
        {
            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map == null)
            {
                Log.Warning($"{LogPrefix} Pulse skipped: no player map.");
                return;
            }

            pulsesFiredInPhase++;
            PulseKind kind = ChoosePulseKind(phase);
            Log.Message($"{LogPrefix} Firing pulse '{kind}' in phase '{phase.Name}' (#{pulsesFiredInPhase}).");

            switch (kind)
            {
                case PulseKind.LightMist:
                    StartMist(map, "LanternVillage_ForestMist", DaysToTicks(0.6f + Rand.Value * 0.6f));
                    SendLetter(
                        "Mist in the trees",
                        "A cold mist slides between the trunks and gathers around the hamlet. The lanterns look smaller than they did this morning.",
                        LetterDefOf.NeutralEvent);
                    break;

                case PulseKind.ThickMist:
                    StartMist(map, "LanternVillage_ThickMist", DaysToTicks(0.8f + Rand.Value * 0.8f));
                    SendLetter(
                        "The forest closes in",
                        "Rain and fog braid until the palisade is only a grey suggestion. Something scrapes beyond the light — or the mind invents it.",
                        LetterDefOf.NegativeEvent);
                    break;

                case PulseKind.ShortDarkness:
                    StartEclipse(map, DaysToTicks(0.25f + Rand.Value * 0.35f));
                    StartMist(map, "LanternVillage_ForestMist", DaysToTicks(0.4f + Rand.Value * 0.4f));
                    SendLetter(
                        "The day fails",
                        "The sun dies early. Shadows stretch from the forest as if they have been waiting. Light every torch you own.",
                        LetterDefOf.ThreatSmall);
                    break;

                case PulseKind.LongDarkness:
                    StartEclipse(map, DaysToTicks(0.55f + Rand.Value * 0.55f));
                    StartMist(map, "LanternVillage_ThickMist", DaysToTicks(0.9f + Rand.Value * 0.7f));
                    SendLetter(
                        "The last light falters",
                        "Day becomes a rumour. Mist presses the walls. Keep the hearth burning — outside the light, the forest is learning your names.",
                        LetterDefOf.ThreatBig);
                    break;
            }
        }

        private enum PulseKind
        {
            LightMist,
            ThickMist,
            ShortDarkness,
            LongDarkness,
        }

        private static PulseKind ChoosePulseKind(Phase phase)
        {
            float roll = Rand.Value;
            switch (phase.Name)
            {
                case "Foundation":
                    return PulseKind.LightMist;

                case "Folklore threats":
                    return roll < 0.7f ? PulseKind.LightMist : PulseKind.ThickMist;

                case "The strange forest":
                    if (roll < 0.45f) return PulseKind.ThickMist;
                    if (roll < 0.75f) return PulseKind.LightMist;
                    return PulseKind.ShortDarkness;

                case "The darkness awakens":
                    if (roll < 0.35f) return PulseKind.ThickMist;
                    if (roll < 0.7f) return PulseKind.ShortDarkness;
                    return PulseKind.LongDarkness;

                case "The last light":
                default:
                    if (roll < 0.25f) return PulseKind.ThickMist;
                    if (roll < 0.55f) return PulseKind.ShortDarkness;
                    return PulseKind.LongDarkness;
            }
        }

        private static void StartMist(Map map, string conditionDefName, int durationTicks)
        {
            GameConditionDef def = DefDatabase<GameConditionDef>.GetNamedSilentFail(conditionDefName);
            if (def == null)
            {
                // Fallback: direct weather transition if defs failed to load.
                WeatherDef weather = conditionDefName.Contains("Thick") ? WeatherDefOf.FoggyRain : WeatherDefOf.Fog;
                map.weatherManager.TransitionTo(weather);
                Log.Warning($"{LogPrefix} Mist GameConditionDef '{conditionDefName}' missing; forced weather {weather.defName}.");
                return;
            }

            if (map.gameConditionManager.ConditionIsActive(def))
            {
                GameCondition existing = map.gameConditionManager.GetActiveCondition(def);
                if (existing != null)
                {
                    // Keep at least durationTicks remaining.
                    existing.Duration = existing.TicksPassed + Math.Max(existing.TicksLeft, durationTicks);
                    return;
                }
            }

            GameCondition condition = GameConditionMaker.MakeCondition(def, durationTicks);
            map.gameConditionManager.RegisterCondition(condition);
        }

        private static void StartEclipse(Map map, int durationTicks)
        {
            GameConditionDef eclipse = GameConditionDefOf.Eclipse;
            if (eclipse == null)
            {
                Log.Warning($"{LogPrefix} Eclipse GameConditionDef missing; skipping darkness pulse.");
                return;
            }

            if (map.gameConditionManager.ConditionIsActive(eclipse))
            {
                GameCondition existing = map.gameConditionManager.GetActiveCondition(eclipse);
                if (existing != null)
                {
                    existing.Duration = existing.TicksPassed + Math.Max(existing.TicksLeft, durationTicks);
                    return;
                }
            }

            GameCondition condition = GameConditionMaker.MakeCondition(eclipse, durationTicks);
            map.gameConditionManager.RegisterCondition(condition);
        }

        private static void SendLetter(string title, string body, LetterDef letterDef)
        {
            try
            {
                Find.LetterStack.ReceiveLetter(
                    LetterMaker.MakeLetter(title, body, letterDef ?? LetterDefOf.NeutralEvent));
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogPrefix} Failed to send letter '{title}': {ex.Message}");
            }
        }

        private static string PhaseLetterTitle(Phase phase)
        {
            switch (phase.Name)
            {
                case "Foundation": return "The clearing holds";
                case "Folklore threats": return "Old stories wake";
                case "The strange forest": return "The forest watches";
                case "The darkness awakens": return "The darkness awakens";
                case "The last light": return "The last light";
                default: return phase.Name;
            }
        }

        private static string PhaseLetterBody(Phase phase)
        {
            switch (phase.Name)
            {
                case "Foundation":
                    return "The hamlet is young. Build walls, plant gardens, and keep a light at every door. The forest is quiet — for now.";
                case "Folklore threats":
                    return "The woods have begun to whisper. Mist comes without rain. Check the lanterns before dusk.";
                case "The strange forest":
                    return "Paths you knew feel wrong. Fog hangs at noon. Something in the treeline does not love your fire.";
                case "The darkness awakens":
                    return "Day fails early. Mist presses the palisade. The light must not go out.";
                case "The last light":
                    return "This is the trial the stories warned of. Hold the hearth. Hold each other. Outside the light is the unknown.";
                default:
                    return $"The Lantern Village enters phase '{phase.Name}'.";
            }
        }

        private static float PhaseFirstPulseDelayDays(Phase phase)
        {
            switch (phase.Name)
            {
                case "Foundation": return 1.5f + Rand.Value * 1.5f; // first mist within ~1.5–3 days
                case "Folklore threats": return 0.8f + Rand.Value;
                case "The strange forest": return 0.5f + Rand.Value * 0.8f;
                case "The darkness awakens": return 0.35f + Rand.Value * 0.5f;
                case "The last light": return 0.25f + Rand.Value * 0.4f;
                default: return 1f;
            }
        }

        private static float PhasePulseMinDays(Phase phase)
        {
            switch (phase.Name)
            {
                case "Foundation": return 4f;
                case "Folklore threats": return 2.5f;
                case "The strange forest": return 1.8f;
                case "The darkness awakens": return 1.2f;
                case "The last light": return 0.8f;
                default: return 3f;
            }
        }

        private static float PhasePulseMaxDays(Phase phase)
        {
            switch (phase.Name)
            {
                case "Foundation": return 7f;
                case "Folklore threats": return 4.5f;
                case "The strange forest": return 3.2f;
                case "The darkness awakens": return 2.2f;
                case "The last light": return 1.6f;
                default: return 5f;
            }
        }

        public Phase GetCurrentPhase()
        {
            int day = DaysPassedSinceSettle();
            // Day 0 (settling day) counts as Foundation.
            if (day < 1)
            {
                day = 1;
            }

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
            return GenDate.DaysPassedSinceSettle;
        }

        private static int DaysToTicks(float days)
        {
            return Math.Max(250, (int)(days * GenDate.TicksPerDay));
        }

        private static float TicksToDays(int ticks)
        {
            return ticks / (float)GenDate.TicksPerDay;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastPhaseName, "lastPhaseName");
            Scribe_Values.Look(ref nextPulseTick, "nextPulseTick", -1);
            Scribe_Values.Look(ref pulsesFiredInPhase, "pulsesFiredInPhase", 0);
            Scribe_Values.Look(ref supernaturalEnabled, "supernaturalEnabled", true);
            Scribe_Values.Look(ref announcedCampaign, "announcedCampaign", false);
        }

        [DebugAction("Lantern Village", "Log current horror phase", requiresIdeology: true, requiresBiotech: true)]
        public static void DebugLogPhase()
        {
            HorrorDirector director = Current.Game?.GetComponent<HorrorDirector>();
            if (director == null)
            {
                Log.Message($"{LogPrefix} No HorrorDirector on this game.");
                return;
            }

            Phase p = director.GetCurrentPhase();
            Log.Message(
                $"{LogPrefix} Current phase: {p.Name} (day {DaysPassedSinceSettle()}, " +
                $"supernatural={(director.supernaturalEnabled ? "ON" : "off")}, " +
                $"nextPulseTick={director.nextPulseTick}, pulsesInPhase={director.pulsesFiredInPhase})");
        }

        [DebugAction("Lantern Village", "Toggle supernatural events", requiresIdeology: true, requiresBiotech: true)]
        public static void DebugToggleSupernatural()
        {
            HorrorDirector director = Current.Game?.GetComponent<HorrorDirector>();
            if (director == null)
            {
                Log.Message($"{LogPrefix} No HorrorDirector on this game.");
                return;
            }

            director.supernaturalEnabled = !director.supernaturalEnabled;
            Log.Message($"{LogPrefix} Supernatural events now {(director.supernaturalEnabled ? "ENABLED" : "disabled")}.");
        }

        [DebugAction("Lantern Village", "Fire mist/darkness pulse now", requiresIdeology: true, requiresBiotech: true)]
        public static void DebugFirePulseNow()
        {
            HorrorDirector director = Current.Game?.GetComponent<HorrorDirector>();
            if (director == null)
            {
                Log.Message($"{LogPrefix} No HorrorDirector on this game.");
                return;
            }

            director.supernaturalEnabled = true;
            director.FirePulse(director.GetCurrentPhase());
            director.ScheduleNextPulse(director.GetCurrentPhase(), afterPulse: true);
        }
    }
}
