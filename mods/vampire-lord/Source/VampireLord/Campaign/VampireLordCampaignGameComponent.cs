using RimWorld;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Authoritative persistent Vampire Lord campaign state for the current game save.
    /// Owns schedule ticking; delegates wave logic to <see cref="VampireLordWaveDirector"/>.
    /// </summary>
    public class VampireLordCampaignGameComponent : GameComponent
    {
        // Backing fields — Scribe labels are stable save-format IDs; do not rename lightly.
        private bool campaignActive;
        private int campaignDay;
        private int waveNumber;
        private int threatLevel = VampireLordTuning.InitialThreatLevel;
        private int nextWaveTick = -1;
        private int warningTick = -1;
        private VampireLordWaveType pendingWaveType = VampireLordWaveType.Mob;
        private bool warningIssued;
        private bool wavePending;
        private VampireLordWaveType lastWaveType = VampireLordWaveType.Mob;
        private int sameArchetypeStreak;
        private int lastScheduleCheckTick = -1;

        public bool CampaignActive
        {
            get => campaignActive;
            set => campaignActive = value;
        }

        public int CampaignDay
        {
            get => campaignDay;
            set => campaignDay = value;
        }

        /// <summary>Number of waves successfully dispatched (completed count).</summary>
        public int WaveNumber
        {
            get => waveNumber;
            set => waveNumber = value;
        }

        public int ThreatLevel
        {
            get => threatLevel;
            set => threatLevel = value;
        }

        public int NextWaveTick
        {
            get => nextWaveTick;
            set => nextWaveTick = value;
        }

        public int WarningTick
        {
            get => warningTick;
            set => warningTick = value;
        }

        public VampireLordWaveType PendingWaveType
        {
            get => pendingWaveType;
            set => pendingWaveType = value;
        }

        public bool WarningIssued
        {
            get => warningIssued;
            set => warningIssued = value;
        }

        public bool WavePending
        {
            get => wavePending;
            set => wavePending = value;
        }

        public VampireLordWaveType LastWaveType
        {
            get => lastWaveType;
            set => lastWaveType = value;
        }

        public int SameArchetypeStreak
        {
            get => sameArchetypeStreak;
            set => sameArchetypeStreak = value;
        }

        /// <summary>1-based number of the currently pending / next wave.</summary>
        public int UpcomingWaveNumber => waveNumber + 1;

        public int TicksUntilWarning
        {
            get
            {
                if (!wavePending || warningIssued || warningTick < 0)
                {
                    return -1;
                }

                return warningTick - Find.TickManager.TicksGame;
            }
        }

        public int TicksUntilWave
        {
            get
            {
                if (!wavePending || nextWaveTick < 0)
                {
                    return -1;
                }

                return nextWaveTick - Find.TickManager.TicksGame;
            }
        }

        /// <summary>
        /// Required by <see cref="Game.FillComponents"/> — Activator passes the current <see cref="Game"/>.
        /// </summary>
        public VampireLordCampaignGameComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref campaignActive, "vlCampaignActive", false);
            Scribe_Values.Look(ref campaignDay, "vlCampaignDay", 0);
            Scribe_Values.Look(ref waveNumber, "vlWaveNumber", 0);
            Scribe_Values.Look(ref threatLevel, "vlThreatLevel", VampireLordTuning.InitialThreatLevel);
            Scribe_Values.Look(ref nextWaveTick, "vlNextWaveTick", -1);
            Scribe_Values.Look(ref warningTick, "vlWarningTick", -1);
            Scribe_Values.Look(ref pendingWaveType, "vlPendingWaveType", VampireLordWaveType.Mob);
            Scribe_Values.Look(ref warningIssued, "vlWarningIssued", false);
            Scribe_Values.Look(ref wavePending, "vlWavePending", false);
            Scribe_Values.Look(ref lastWaveType, "vlLastWaveType", VampireLordWaveType.Mob);
            Scribe_Values.Look(ref sameArchetypeStreak, "vlSameArchetypeStreak", 0);
        }

        public override void StartedNewGame()
        {
            LogCampaignState("StartedNewGame");
        }

        public override void LoadedGame()
        {
            LogCampaignState("LoadedGame");
        }

        public override void GameComponentTick()
        {
            if (!campaignActive)
            {
                return;
            }

            int ticks = Find.TickManager.TicksGame;
            if (lastScheduleCheckTick >= 0 &&
                ticks - lastScheduleCheckTick < VampireLordTuning.ScheduleCheckIntervalTicks)
            {
                return;
            }

            lastScheduleCheckTick = ticks;
            campaignDay = GenDate.DaysPassed;
            VampireLordWaveDirector.EvaluateSchedule(this);
        }

        private void LogCampaignState(string context)
        {
            Log.Message(
                $"[VampireLord] Campaign state ({context}): " +
                $"Active={campaignActive}, Day={campaignDay}, Wave={waveNumber}, Threat={threatLevel}, " +
                $"Pending={wavePending}, WarningIssued={warningIssued}, Type={pendingWaveType}, " +
                $"WarningTick={warningTick}, NextWaveTick={nextWaveTick}");
        }
    }
}
