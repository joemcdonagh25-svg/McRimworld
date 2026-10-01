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
    }
}
