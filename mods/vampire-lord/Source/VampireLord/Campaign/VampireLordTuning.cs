namespace VampireLord.Campaign
{
    /// <summary>
    /// Single place for V0 Wave Director tuning constants.
    /// </summary>
    public static class VampireLordTuning
    {
        /// <summary>In-game days between a completed wave and the next wave's arrival.</summary>
        public const float DaysBetweenWaves = 3f;

        /// <summary>In-game days between the advance warning letter and the raid.</summary>
        public const float WarningLeadDays = 1f;

        /// <summary>How often the campaign component evaluates schedule state.</summary>
        public const int ScheduleCheckIntervalTicks = 250;

        /// <summary>When a raid cannot fire (no home map), delay before retry.</summary>
        public const int RaidRetryDelayTicks = 2500;

        /// <summary>Base raid points before threat scaling.</summary>
        public const float BaseRaidPoints = 150f;

        /// <summary>Raid points added per threat level.</summary>
        public const float RaidPointsPerThreatLevel = 100f;

        /// <summary>Initial threat level when a campaign starts.</summary>
        public const int InitialThreatLevel = 1;

        /// <summary>Earliest wave number (1-based upcoming) that may roll Siege.</summary>
        public const int MinWaveForSiege = 5;

        /// <summary>Earliest wave number that may roll Breachers.</summary>
        public const int MinWaveForBreachers = 3;

        /// <summary>Earliest wave number that may roll Fire.</summary>
        public const int MinWaveForFire = 5;

        /// <summary>Do not pick the same archetype more than this many times in a row.</summary>
        public const int MaxSameArchetypeInARow = 2;

        // --- M3 Blood Tithe V0 ---

        /// <summary>Keep Blood Reserve when a campaign starts.</summary>
        public const int StartingBloodReserve = 40;

        /// <summary>Blood credited for a fresh hostile humanlike corpse.</summary>
        public const int BloodPerHumanlikeKill = 5;

        /// <summary>Max age (ticks) of a corpse that can still be tithed (avoids re-credit after load).</summary>
        public const int FreshCorpseMaxAgeTicks = 60000; // ~1 day

        /// <summary>How often to scan maps for fresh kills to tithe.</summary>
        public const int BloodScanIntervalTicks = 60;

        /// <summary>Base blood spent when a wave launches.</summary>
        public const int WaveBloodCostBase = 10;

        /// <summary>Extra blood cost per current threat level.</summary>
        public const int WaveBloodCostPerThreat = 2;

        /// <summary>Raid points multiplier when the keep cannot pay the full wave tithe.</summary>
        public const float StarvedRaidPointsMultiplier = 1.35f;

        // --- M4 Keep Fortification V0 ---

        /// <summary>Blood spent to place one gate sandbag package.</summary>
        public const int FortifyBloodCost = 15;

        /// <summary>Max fortify purchases allowed in one inter-wave prep window.</summary>
        public const int FortifyMaxPerPrepWindow = 1;
    }
}
