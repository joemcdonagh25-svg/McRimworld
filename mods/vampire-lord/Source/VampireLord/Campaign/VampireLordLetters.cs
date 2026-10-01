using RimWorld;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Advance-warning and Blood Tithe letters (Black Keep / gothic keep flavour).
    /// Product name remains Vampire Lord.
    /// </summary>
    public static class VampireLordLetters
    {
        public static void SendAdvanceWarning(VampireLordCampaignGameComponent campaign)
        {
            string title = TitleFor(campaign.PendingWaveType);
            string body = BodyFor(campaign);
            Find.LetterStack.ReceiveLetter(title, body, LetterDefOf.ThreatBig);
        }

        public static void SendBloodTitheLetter(
            VampireLordCampaignGameComponent campaign,
            int spent,
            int cost,
            bool starved)
        {
            if (starved)
            {
                Find.LetterStack.ReceiveLetter(
                    "The Tithe Runs Dry",
                    "The Black Keep cannot pay the full blood tithe for the coming host.\n\n" +
                    $"Tithe due: {cost}\n" +
                    $"Paid: {spent}\n" +
                    $"Reserve: {campaign.BloodReserve}\n\n" +
                    "The attackers smell weakness. This wave will hit harder.",
                    LetterDefOf.NegativeEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter(
                    "The Blood Tithe Is Paid",
                    "The Keep drinks deep before the assault.\n\n" +
                    $"Tithe paid: {spent}\n" +
                    $"Reserve remaining: {campaign.BloodReserve}\n\n" +
                    "Hold the gate. Harvest what falls.",
                    LetterDefOf.NeutralEvent);
            }
        }

        public static void SendBloodHarvestLetter(VampireLordCampaignGameComponent campaign, int gained)
        {
            Find.LetterStack.ReceiveLetter(
                "Blood for the Keep",
                "The fallen feed the Black Keep.\n\n" +
                $"Blood harvested since last wave: {gained}\n" +
                $"Keep Blood Reserve: {campaign.BloodReserve}\n\n" +
                "Stock the crypts before the next host arrives.",
                LetterDefOf.PositiveEvent);
        }

        public static void SendFortifyOffer(VampireLordCampaignGameComponent campaign)
        {
            LetterDef def = DefDatabase<LetterDef>.GetNamedSilentFail("VampireLord_FortifyOffer");
            if (def == null)
            {
                Log.Warning("[VampireLord] Fortify LetterDef missing — sending plain letter.");
                Find.LetterStack.ReceiveLetter(
                    "Blood for the Walls",
                    FortifyOfferBody(campaign) +
                    "\n\n(Dev Mode → Vampire Lord → Offer Fortify / Force Fortify Now)",
                    LetterDefOf.PositiveEvent);
                return;
            }

            ChoiceLetter letter = LetterMaker.MakeLetter(
                "Blood for the Walls",
                FortifyOfferBody(campaign),
                def);
            if (letter == null)
            {
                return;
            }

            Find.LetterStack.ReceiveLetter(letter);
        }

        public static void SendFortifyComplete(VampireLordCampaignGameComponent campaign, int placed)
        {
            Find.LetterStack.ReceiveLetter(
                "The Gate Is Reinforced",
                "Blood buys stone patience.\n\n" +
                $"Sandbags placed: {placed}\n" +
                $"Keep Blood Reserve: {campaign.BloodReserve}\n\n" +
                "Hold the approach. The next host will find cover waiting.",
                LetterDefOf.PositiveEvent);
        }

        private static string FortifyOfferBody(VampireLordCampaignGameComponent campaign)
        {
            return
                "Between hosts, the Black Keep can drink deep and raise the gate works.\n\n" +
                $"Cost: {VampireLordTuning.FortifyBloodCost} blood\n" +
                $"Reserve: {campaign.BloodReserve}\n" +
                $"Limit: {VampireLordTuning.FortifyMaxPerPrepWindow} package this prep window\n\n" +
                "Accept to place sandbags south of the open gate.";
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

        public static string BodyFor(VampireLordCampaignGameComponent campaign)
        {
            VampireLordWaveType waveType = campaign.PendingWaveType;
            string flavour = FlavourLine(waveType);
            int cost = VampireLordBloodTithe.WaveBloodCost(campaign);
            return
                $"{flavour}\n\n" +
                $"Wave {campaign.UpcomingWaveNumber} marches on the Black Keep.\n" +
                "Estimated arrival: tomorrow.\n\n" +
                $"Threat: {waveType}\n" +
                $"Campaign threat level: {campaign.ThreatLevel}\n" +
                $"Keep Blood Reserve: {campaign.BloodReserve}\n" +
                $"Tithe due when they strike: {cost}\n\n" +
                "Prepare the castle. Hold the gate. Feed the Keep.";
        }

        private static string FlavourLine(VampireLordWaveType waveType)
        {
            switch (waveType)
            {
                case VampireLordWaveType.Mob:
                    return "Scouts report a ragged host gathering on the road to the Black Keep - pitchforks, torches, and numbers.";
                case VampireLordWaveType.Hunters:
                    return "Torchlight on the road - a smaller, harder company marches for the Vampire Lord's throat.";
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
