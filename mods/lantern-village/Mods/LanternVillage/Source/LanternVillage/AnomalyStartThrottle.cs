using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Persists how many Anomaly storyteller incidents have already been allowed
    /// during The Lantern Village early window. Owned here so save/load keeps the
    /// "one early fleshbeast-style attack" budget honest.
    /// </summary>
    public class AnomalyStartThrottle : GameComponent
    {
        /// <summary>Foundation window — same horizon as HorrorDirector phase 1.</summary>
        public const int EarlyDaysInclusive = 30;

        /// <summary>Joe's ask: one early Anomaly attack is fine; a flood is not.</summary>
        public const int MaxEarlyAnomalyIncidents = 1;

        /// <summary>
        /// Cap the first allowed early Anomaly incident so it is a small fight
        /// (roughly one fleshbeast-scale threat), not a points-scaled swarm.
        /// </summary>
        public const float EarlyAnomalyPointsCap = 90f;

        public int earlyAnomalyIncidentsFired;

        public AnomalyStartThrottle(Game game)
        {
        }

        public static AnomalyStartThrottle Get()
        {
            return Current.Game?.GetComponent<AnomalyStartThrottle>();
        }

        public static bool IsInEarlyWindow()
        {
            int day = GenDate.DaysPassedSinceSettle;
            if (day < 1)
            {
                day = 1;
            }

            return day <= EarlyDaysInclusive;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref earlyAnomalyIncidentsFired, "lvEarlyAnomalyIncidentsFired", 0);
        }
    }
}
