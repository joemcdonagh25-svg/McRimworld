using System;

namespace ColonyTime
{
    /// <summary>
    /// Formats playtime seconds and colony ticks for Load Game UI. No I/O.
    /// </summary>
    public static class SaveTimeFormatter
    {
        public const int TicksPerDay = 60000;
        public const int DaysPerYear = 60;

        public static string FormatPlayTime(double? seconds)
        {
            if (seconds == null || seconds.Value < 0)
            {
                return "—";
            }

            long totalSeconds = (long)Math.Floor(seconds.Value);
            long totalMinutes = totalSeconds / 60;
            long hours = totalMinutes / 60;
            long minutes = totalMinutes % 60;

            if (hours <= 0)
            {
                return minutes + "m";
            }

            return hours + "h " + minutes + "m";
        }

        public static string FormatColonyAge(long? ticksGame)
        {
            if (ticksGame == null || ticksGame.Value < 0)
            {
                return "—";
            }

            long totalDays = ticksGame.Value / TicksPerDay;
            long years = totalDays / DaysPerYear;
            long days = totalDays % DaysPerYear;

            if (years <= 0)
            {
                return days + "d";
            }

            return years + "y " + days + "d";
        }

        public static string FormatPlayedLine(double? seconds)
        {
            return "Played: " + FormatPlayTime(seconds);
        }

        public static string FormatColonyLine(long? ticksGame)
        {
            return "Colony: " + FormatColonyAge(ticksGame);
        }
    }
}
