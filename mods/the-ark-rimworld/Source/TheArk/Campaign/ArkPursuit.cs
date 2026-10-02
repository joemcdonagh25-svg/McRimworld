using Verse;

namespace TheArk.Campaign
{
    /// <summary>
    /// M5/M6 Pursuit bands and V0 growth rate (time-on-ground only).
    /// Thresholds match GAME_DESIGN.md.
    /// </summary>
    public static class ArkPursuit
    {
        public const int Min = 0;
        public const int Max = 100;

        /// <summary>V0: +1 Pursuit per full RimWorld day while a landing session is active.</summary>
        public const int PursuitPerLandedDay = 1;

        public enum Band
        {
            Quiet = 0,
            Noticed = 1,
            Hunted = 2,
            Besieged = 3,
            Harbinger = 4
        }

        public static int Clamp(int value)
        {
            if (value < Min)
            {
                return Min;
            }

            if (value > Max)
            {
                return Max;
            }

            return value;
        }

        public static Band BandFor(int pursuit)
        {
            int p = Clamp(pursuit);
            if (p <= 20)
            {
                return Band.Quiet;
            }

            if (p <= 40)
            {
                return Band.Noticed;
            }

            if (p <= 60)
            {
                return Band.Hunted;
            }

            if (p <= 80)
            {
                return Band.Besieged;
            }

            return Band.Harbinger;
        }

        public static string BandLabel(Band band)
        {
            switch (band)
            {
                case Band.Quiet:
                    return "QUIET";
                case Band.Noticed:
                    return "NOTICED";
                case Band.Hunted:
                    return "HUNTED";
                case Band.Besieged:
                    return "BESIEGED";
                case Band.Harbinger:
                    return "HARBINGER";
                default:
                    return band.ToString().ToUpperInvariant();
            }
        }

        public static string BandRangeLabel(Band band)
        {
            switch (band)
            {
                case Band.Quiet:
                    return "0–20";
                case Band.Noticed:
                    return "21–40";
                case Band.Hunted:
                    return "41–60";
                case Band.Besieged:
                    return "61–80";
                case Band.Harbinger:
                    return "81–100";
                default:
                    return "?";
            }
        }

        public static string Format(int pursuit)
        {
            Band band = BandFor(pursuit);
            return $"Pursuit={Clamp(pursuit)} ({BandLabel(band)} {BandRangeLabel(band)})";
        }
    }
}
