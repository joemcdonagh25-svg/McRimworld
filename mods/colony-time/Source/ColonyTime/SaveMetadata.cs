using System;

namespace ColonyTime
{
    /// <summary>
    /// Cached read-only metadata extracted from a single .rws file.
    /// </summary>
    public sealed class SaveMetadata
    {
        public string FilePath;
        public double? RealPlayTimeSeconds;
        public long? TicksGame;
        public DateTime LastWriteTimeUtc;
        public long FileSize;
        public bool ParseFailed;
    }
}
