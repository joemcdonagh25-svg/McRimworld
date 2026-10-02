using RimWorld;
using Verse;

namespace TheArk.Campaign
{
    /// <summary>
    /// M6 player-facing Pursuit band feedback (letters on threshold cross).
    /// </summary>
    public static class ArkPursuitLetters
    {
        public static void SendBandChanged(int pursuit, ArkPursuit.Band newBand, ArkPursuit.Band? previousBand)
        {
            string bandName = ArkPursuit.BandLabel(newBand);
            string range = ArkPursuit.BandRangeLabel(newBand);
            string title = $"Pursuit: {bandName}";
            string from = previousBand.HasValue
                ? $"Was {ArkPursuit.BandLabel(previousBand.Value)}. "
                : string.Empty;
            string body =
                $"{from}Pursuit is now {ArkPursuit.Clamp(pursuit)} — {bandName} ({range}).\n\n" +
                BodyFor(newBand);

            Find.LetterStack.ReceiveLetter(title, body, LetterDefFor(newBand));
            Log.Message($"[The Ark] Pursuit band → {bandName} (Pursuit={ArkPursuit.Clamp(pursuit)}).");
        }

        private static LetterDef LetterDefFor(ArkPursuit.Band band)
        {
            switch (band)
            {
                case ArkPursuit.Band.Quiet:
                    return LetterDefOf.NeutralEvent;
                case ArkPursuit.Band.Noticed:
                    return LetterDefOf.NeutralEvent;
                case ArkPursuit.Band.Hunted:
                    return LetterDefOf.NegativeEvent;
                case ArkPursuit.Band.Besieged:
                    return LetterDefOf.ThreatSmall;
                case ArkPursuit.Band.Harbinger:
                    return LetterDefOf.ThreatBig;
                default:
                    return LetterDefOf.NeutralEvent;
            }
        }

        private static string BodyFor(ArkPursuit.Band band)
        {
            switch (band)
            {
                case ArkPursuit.Band.Quiet:
                    return "The ground feels quiet. Take what you need — but do not linger forever.";
                case ArkPursuit.Band.Noticed:
                    return "Something has noticed the Ark. Stay sharp; the clock is running.";
                case ArkPursuit.Band.Hunted:
                    return "You are being hunted. Opportunity is shrinking. Prepare to leave.";
                case ArkPursuit.Band.Besieged:
                    return "Pressure closes in. Evacuation is becoming the wise call.";
                case ArkPursuit.Band.Harbinger:
                    return "HARBINGER. The intended answer is to leave — not to farm the storm.";
                default:
                    return "Pursuit has changed.";
            }
        }
    }
}
