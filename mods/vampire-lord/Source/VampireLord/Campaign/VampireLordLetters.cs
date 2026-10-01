using RimWorld;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Advance-warning letters for pending waves (gothic keep flavour).
    /// </summary>
    public static class VampireLordLetters
    {
        public static void SendAdvanceWarning(VampireLordCampaignGameComponent campaign)
        {
            string title = TitleFor(campaign.PendingWaveType);
            string body = BodyFor(campaign.PendingWaveType, campaign.ThreatLevel);
            Find.LetterStack.ReceiveLetter(title, body, LetterDefOf.ThreatBig);
        }

        public static string TitleFor(VampireLordWaveType waveType)
        {
            switch (waveType)
            {
                case VampireLordWaveType.Mob:
                    return "The Mob Gathers";
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

        public static string BodyFor(VampireLordWaveType waveType, int threatLevel)
        {
            string flavour = FlavourLine(waveType);
            return
                $"{flavour}\n\n" +
                "Estimated arrival: tomorrow.\n\n" +
                $"Threat: {waveType}\n" +
                $"Campaign threat level: {threatLevel}\n\n" +
                "Prepare the castle.";
        }

        private static string FlavourLine(VampireLordWaveType waveType)
        {
            switch (waveType)
            {
                case VampireLordWaveType.Mob:
                    return "Scouts report a ragged host gathering on the road to the keep.";
                case VampireLordWaveType.Hunters:
                    return "Torchlight on the road — a smaller, harder company marches on the keep.";
                case VampireLordWaveType.Breachers:
                    return "Scouts report an armed force moving toward the Black Keep with tools for the walls.";
                case VampireLordWaveType.Fire:
                    return "Smoke hangs on the horizon. An armed force is coming for the outer estate.";
                case VampireLordWaveType.Siege:
                    return "Siege engines have been sighted on the approach to the Black Keep.";
                default:
                    return "Scouts report an armed force moving toward the Black Keep.";
            }
        }
    }
}
