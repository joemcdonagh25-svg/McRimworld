using RimWorld;
using VampireLord.Scenario;
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
        private int lastBloodScanTick = -1;

        // M3 Blood Tithe
        private int bloodReserve;
        private int bloodGainedSinceWave;
        private bool lastWaveBloodStarved;
        private int lastWaveBloodSpent;
        private int lastWaveBloodCost;

        // M4 Keep Fortification
        private int fortifyPurchasesThisWindow;
        private int lastFortifyWaveNumber = -1;
        private int lastFortifyPlacedCount;
        private int fortifyOfferDueTick = -1;
        private bool fortifyOfferSentThisWindow;
        // Playtest defaults ON — less letter clicking / waiting while iterating.
        private bool autoFortifyPlaytest = true;
        private bool playtestPace = true;
        private bool playtestQuietLetters = true;

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

        public int BloodReserve
        {
            get => bloodReserve;
            set => bloodReserve = value < 0 ? 0 : value;
        }

        public int BloodGainedSinceWave
        {
            get => bloodGainedSinceWave;
            set => bloodGainedSinceWave = value < 0 ? 0 : value;
        }

        public bool LastWaveBloodStarved
        {
            get => lastWaveBloodStarved;
            set => lastWaveBloodStarved = value;
        }

        public int LastWaveBloodSpent
        {
            get => lastWaveBloodSpent;
            set => lastWaveBloodSpent = value;
        }

        public int LastWaveBloodCost
        {
            get => lastWaveBloodCost;
            set => lastWaveBloodCost = value;
        }

        public int FortifyPurchasesThisWindow
        {
            get => fortifyPurchasesThisWindow;
            set => fortifyPurchasesThisWindow = value < 0 ? 0 : value;
        }

        public int LastFortifyWaveNumber
        {
            get => lastFortifyWaveNumber;
            set => lastFortifyWaveNumber = value;
        }

        public int LastFortifyPlacedCount
        {
            get => lastFortifyPlacedCount;
            set => lastFortifyPlacedCount = value < 0 ? 0 : value;
        }

        public int FortifyOfferDueTick
        {
            get => fortifyOfferDueTick;
            set => fortifyOfferDueTick = value;
        }

        public bool FortifyOfferSentThisWindow
        {
            get => fortifyOfferSentThisWindow;
            set => fortifyOfferSentThisWindow = value;
        }

        /// <summary>
        /// Playtest QoL: when true, prep windows auto-place gate sandbags instead of sending the Accept letter.
        /// Default ON for Vampire Lord campaign.
        /// </summary>
        public bool AutoFortifyPlaytest
        {
            get => autoFortifyPlaytest;
            set => autoFortifyPlaytest = value;
        }

        /// <summary>Faster wave schedule for iteration. Default ON.</summary>
        public bool PlaytestPace
        {
            get => playtestPace;
            set => playtestPace = value;
        }

        /// <summary>Skip non-essential letters (harvest / fortify complete). Warnings + tithe stay. Default ON.</summary>
        public bool PlaytestQuietLetters
        {
            get => playtestQuietLetters;
            set => playtestQuietLetters = value;
        }

        public float EffectiveDaysBetweenWaves =>
            playtestPace ? VampireLordTuning.PlaytestDaysBetweenWaves : VampireLordTuning.DaysBetweenWaves;

        public float EffectiveWarningLeadDays =>
            playtestPace ? VampireLordTuning.PlaytestWarningLeadDays : VampireLordTuning.WarningLeadDays;

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
            Scribe_Values.Look(ref bloodReserve, "vlBloodReserve", 0);
            Scribe_Values.Look(ref bloodGainedSinceWave, "vlBloodGainedSinceWave", 0);
            Scribe_Values.Look(ref lastWaveBloodStarved, "vlLastWaveBloodStarved", false);
            Scribe_Values.Look(ref lastWaveBloodSpent, "vlLastWaveBloodSpent", 0);
            Scribe_Values.Look(ref lastWaveBloodCost, "vlLastWaveBloodCost", 0);
            Scribe_Values.Look(ref fortifyPurchasesThisWindow, "vlFortifyPurchasesThisWindow", 0);
            Scribe_Values.Look(ref lastFortifyWaveNumber, "vlLastFortifyWaveNumber", -1);
            Scribe_Values.Look(ref lastFortifyPlacedCount, "vlLastFortifyPlacedCount", 0);
            Scribe_Values.Look(ref fortifyOfferDueTick, "vlFortifyOfferDueTick", -1);
            Scribe_Values.Look(ref fortifyOfferSentThisWindow, "vlFortifyOfferSentThisWindow", false);
            Scribe_Values.Look(ref autoFortifyPlaytest, "vlAutoFortifyPlaytest", true);
            Scribe_Values.Look(ref playtestPace, "vlPlaytestPace", true);
            Scribe_Values.Look(ref playtestQuietLetters, "vlPlaytestQuietLetters", true);
        }

        public override void StartedNewGame()
        {
            VampireLordPlayerHome.TryEnsure("StartedNewGame");
            LogCampaignState("StartedNewGame");
        }

        public override void LoadedGame()
        {
            VampireLordBloodTithe.ResetSessionCredits();
            // Existing playtest saves may still be Camp/non-home — repair on load.
            VampireLordPlayerHome.TryEnsure("LoadedGame");
            LogCampaignState("LoadedGame");
        }

        public override void GameComponentTick()
        {
            if (!campaignActive)
            {
                return;
            }

            int ticks = Find.TickManager.TicksGame;

            if (lastBloodScanTick < 0 ||
                ticks - lastBloodScanTick >= VampireLordTuning.BloodScanIntervalTicks)
            {
                lastBloodScanTick = ticks;
                VampireLordBloodTithe.ScanForFreshKills(this);
            }

            // Fortify Accept letter is delayed so it is not lost during PostGameStart settle.
            VampireLordFortify.EvaluatePendingOffer(this);

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
                $"WarningTick={warningTick}, NextWaveTick={nextWaveTick}, " +
                $"Blood={bloodReserve}, GainedSinceWave={bloodGainedSinceWave}, LastStarved={lastWaveBloodStarved}, " +
                $"FortifyPurchases={fortifyPurchasesThisWindow}, LastFortifyWave={lastFortifyWaveNumber}");
        }
    }
}
