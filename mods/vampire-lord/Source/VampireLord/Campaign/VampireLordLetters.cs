using RimWorld;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Advance-warning letters for pending waves (Black Keep / gothic keep flavour).
    /// Product name remains Vampire Lord.
    /// </summary>
    public static class VampireLordLetters
    {
        public static void SendAdvanceWarning(VampireLordCampaignGameComponent campaign)
        {
            string title = TitleFor(campaign.PendingWaveType);
            string body = BodyFor(campaign.PendingWaveType, campaign.ThreatLevel, campaign.UpcomingWaveNumber);
            Find.LetterStack.ReceiveLetter(title, body, LetterDefOf.ThreatBig);
        }

        public static string TitleFor(VampireLordWaveType waveType)
        {
            switch (waveType)
            {
                case VampireLordWaveType.Mob:
                    return "The Mob Gathers at the Black Keep";
                case VampireLordWaveType.Hunters:
                    return "Hunters on the Road";
                case VampireLordWaveType.Breachers:
                    return "The Walls Will Be Tested";
                case VampireLordWaveType.Fire:
                    return "Smoke on the Horizon";
                case VampireLordWaveType.Siege:
                    return "Siege Engines Approach";
                default:
                    return "Torchlight on the Road";
            }
        }

        public static string BodyFor(VampireLordWaveType waveType, int threatLevel, int waveNumber)
        {
            string flavour = FlavourLine(waveType);
            return
                $"{flavour}\n\n" +
                $"Wave {waveNumber} marches on the Black Keep.\n" +
                "Estimated arrival: tomorrow.\n\n" +
                $"Threat: {waveType}\n" +
                $"Campaign threat level: {threatLevel}\n\n" +
                "Prepare the castle. Hold the gate.";
        }

        private static string FlavourLine(VampireLordWaveType waveType)
        {
            switch (waveType)
            {
                case VampireLordWaveType.Mob:
                    return "Scouts report a ragged host gathering on the road to the Black Keep — pitchforks, torches, and numbers.";
                case VampireLordWaveType.Hunters:
                    return "Torchlight on the road — a smaller, harder company marches for the Vampire Lord’s throat.";
                case VampireLordWaveType.Breachers:
                    return "An armed force moves on the Black Keep with rams, picks, and tools for your walls.";
                case VampireLordWaveType.Fire:
                    return "Smoke hangs on the horizon. They mean to burn the outer estate and smoke out the keep.";
                case VampireLordWaveType.Siege:
                    return "Siege engines have been sighted on the approach. The Black Keep will be ringed before dawn.";
                default:
                    return "Scouts report an armed force moving toward the Black Keep.";
            }
        }
    }
}
