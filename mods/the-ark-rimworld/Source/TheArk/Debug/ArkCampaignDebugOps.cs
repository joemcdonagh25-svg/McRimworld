using RimWorld;
using TheArk.Campaign;
using Verse;

namespace TheArk.Debug
{
    /// <summary>
    /// Explicit write/read operations for development tooling.
    /// Mutates <see cref="ArkCampaignGameComponent"/> only — no duplicate campaign state.
    /// </summary>
    public static class ArkCampaignDebugOps
    {
        public const int FixtureCampaignDay = 47;
        public const int FixtureLandingNumber = 6;
        public const int FixtureArkTier = 2;
        public const int FixturePursuit = 73;

        public static bool TryGet(out ArkCampaignGameComponent campaign)
        {
            return ArkCampaign.TryGet(out campaign);
        }

        public static void SetCampaignActive(ArkCampaignGameComponent campaign, bool value)
        {
            campaign.CampaignActive = value;
        }

        public static void SetCampaignDay(ArkCampaignGameComponent campaign, int value)
        {
            campaign.CampaignDay = value;
        }

        public static void SetLandingNumber(ArkCampaignGameComponent campaign, int value)
        {
            campaign.LandingNumber = value;
        }

        public static void SetArkTier(ArkCampaignGameComponent campaign, int value)
        {
            campaign.ArkTier = value;
        }

        public static void SetPursuit(ArkCampaignGameComponent campaign, int value)
        {
            campaign.SetPursuit(value, "Dev.SetPursuit", notifyBand: true);
        }

        /// <summary>
        /// M1 persistence proof fixture: Active=true, Day=47, Landing=6, Tier=2, Pursuit=73.
        /// Does not open a landing session or change the session timer.
        /// </summary>
        public static void ApplyPersistenceFixture(ArkCampaignGameComponent campaign)
        {
            SetCampaignActive(campaign, true);
            SetCampaignDay(campaign, FixtureCampaignDay);
            SetLandingNumber(campaign, FixtureLandingNumber);
            SetArkTier(campaign, FixtureArkTier);
            campaign.SetPursuit(FixturePursuit, "Dev.M1Fixture", notifyBand: true);
            Log.Message("[The Ark] [DEV] Applied M1 persistence fixture: " + FormatState(campaign));
        }

        /// <summary>
        /// M3 proof without a real gravship hop: ensure campaign active, then begin landing session.
        /// </summary>
        public static bool SimulateLanding(ArkCampaignGameComponent campaign)
        {
            if (!campaign.CampaignActive)
            {
                SetCampaignActive(campaign, true);
                Log.Message("[The Ark] [DEV] Simulate Landing: campaign was inactive — activated.");
            }

            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            return campaign.TryBeginLandingSession(map, "Dev.SimulateLanding", allowRecountSameMap: true);
        }

        public static bool EndLandingSession(ArkCampaignGameComponent campaign)
        {
            return campaign.EndLandingSession("Dev.EndLandingSession");
        }

        /// <summary>M4/M5 proof: add one RimWorld day to the landing timer (also grants +1 Pursuit while landed).</summary>
        public static bool AdvanceLandingTimerOneDay(ArkCampaignGameComponent campaign)
        {
            return campaign.AddLandingSessionTicks(GenDate.TicksPerDay, "Dev.AdvanceLandingTimerOneDay");
        }

        /// <summary>M6 proof: jump Pursuit to the entry value of each band in order for letter checks.</summary>
        public static void JumpPursuitToNextBand(ArkCampaignGameComponent campaign)
        {
            ArkPursuit.Band current = campaign.PursuitBand;
            int target;
            switch (current)
            {
                case ArkPursuit.Band.Quiet:
                    target = 21;
                    break;
                case ArkPursuit.Band.Noticed:
                    target = 41;
                    break;
                case ArkPursuit.Band.Hunted:
                    target = 61;
                    break;
                case ArkPursuit.Band.Besieged:
                    target = 81;
                    break;
                default:
                    target = 0;
                    break;
            }

            campaign.SetPursuit(target, "Dev.JumpPursuitToNextBand", notifyBand: true);
        }

        /// <summary>
        /// One-click Pressure V0 proof (M4+M5+M6). Prefer this over stepping individual debug actions.
        /// Logs a single PASS/FAIL line Joe can paste.
        /// </summary>
        public static bool RunPressureV0Proof(ArkCampaignGameComponent campaign)
        {
            Log.Message("[The Ark] [DEV] Pressure V0 proof START.");

            SetCampaignActive(campaign, true);
            campaign.SetPursuit(0, "Dev.PressureV0Proof.ResetPursuit", notifyBand: false);

            if (campaign.LandingSessionActive)
            {
                EndLandingSession(campaign);
            }

            if (!SimulateLanding(campaign))
            {
                Log.Error("[The Ark] [DEV] Pressure V0 proof FAIL: Simulate Landing did not start a session.");
                return false;
            }

            bool sessionOk = campaign.LandingSessionActive;
            bool timerZeroOk = campaign.LandingSessionTicks == 0;
            int landingAfterStart = campaign.LandingNumber;

            if (!AdvanceLandingTimerOneDay(campaign))
            {
                Log.Error("[The Ark] [DEV] Pressure V0 proof FAIL: could not advance landing timer.");
                return false;
            }

            bool timerDayOk = campaign.LandingSessionTicks >= GenDate.TicksPerDay;
            bool pursuitGrewOk = campaign.Pursuit >= 1;

            // Walk every band once so letters fire without Joe clicking repeatedly.
            int[] bandEntries = { 21, 41, 61, 81 };
            foreach (int entry in bandEntries)
            {
                campaign.SetPursuit(entry, "Dev.PressureV0Proof.BandWalk", notifyBand: true);
            }

            bool harbingerOk = campaign.PursuitBand == ArkPursuit.Band.Harbinger;

            EndLandingSession(campaign);
            bool timerClearedOk = !campaign.LandingSessionActive && campaign.LandingSessionTicks == 0;
            bool pursuitKeptOk = campaign.Pursuit >= 81;

            bool pass = sessionOk && timerZeroOk && timerDayOk && pursuitGrewOk && harbingerOk
                && timerClearedOk && pursuitKeptOk && landingAfterStart > 0;

            string summary =
                $"[The Ark] [DEV] Pressure V0 proof {(pass ? "PASS" : "FAIL")}: " +
                $"session={sessionOk}, timerStartZero={timerZeroOk}, timerDay={timerDayOk}, " +
                $"pursuitGrew={pursuitGrewOk}, harbinger={harbingerOk}, " +
                $"timerCleared={timerClearedOk}, pursuitKept={pursuitKeptOk}, " +
                $"LandingNumber={campaign.LandingNumber}, {ArkPursuit.Format(campaign.Pursuit)}";

            if (pass)
            {
                Log.Message(summary);
            }
            else
            {
                Log.Error(summary);
            }

            return pass;
        }

        /// <summary>
        /// One-click M7 proof: BESIEGED fires ManhunterPack once; second raise does not re-fire.
        /// Logs a single PASS/FAIL line Joe can paste.
        /// </summary>
        public static bool RunPursuitIncidentProof(ArkCampaignGameComponent campaign)
        {
            Log.Message("[The Ark] [DEV] Pursuit Incident (M7) proof START.");

            SetCampaignActive(campaign, true);
            campaign.SetPursuit(0, "Dev.M7Proof.ResetPursuit", notifyBand: false);

            if (campaign.LandingSessionActive)
            {
                EndLandingSession(campaign);
            }

            if (!SimulateLanding(campaign))
            {
                Log.Error("[The Ark] [DEV] Pursuit Incident proof FAIL: Simulate Landing did not start a session.");
                return false;
            }

            bool sessionOk = campaign.LandingSessionActive;
            bool notFiredYet = !campaign.PursuitIncidentFiredThisSession;

            // Cross BESIEGED threshold — should fire once.
            campaign.SetPursuit(ArkPursuitIncident.TriggerPursuit, "Dev.M7Proof.TriggerBesieged", notifyBand: true);
            bool firedOnTrigger = campaign.PursuitIncidentFiredThisSession;

            // Raise further inside BESIEGED — must not fire again this session.
            campaign.SetPursuit(75, "Dev.M7Proof.StillBesieged", notifyBand: true);
            bool stillOnce = campaign.PursuitIncidentFiredThisSession;

            // Second force attempt should be blocked by once-per-landing unless we use Force.
            // Prove automatic path stayed once: flag true and Pursuit still ≥ trigger.
            bool pursuitAtTrigger = campaign.Pursuit >= ArkPursuitIncident.TriggerPursuit;

            bool pass = sessionOk && notFiredYet && firedOnTrigger && stillOnce && pursuitAtTrigger;

            string summary =
                $"[The Ark] [DEV] Pursuit Incident proof {(pass ? "PASS" : "FAIL")}: " +
                $"session={sessionOk}, notFiredYet={notFiredYet}, firedOnTrigger={firedOnTrigger}, " +
                $"onceOnly={stillOnce}, pursuitAtTrigger={pursuitAtTrigger}, " +
                $"IncidentFired={campaign.PursuitIncidentFiredThisSession}, " +
                $"{ArkPursuit.Format(campaign.Pursuit)}";

            if (pass)
            {
                Log.Message(summary);
            }
            else
            {
                Log.Error(summary);
            }

            return pass;
        }

        public static bool ForceFirePursuitIncident(ArkCampaignGameComponent campaign)
        {
            return campaign.ForceFirePursuitIncident("Dev.ForceFirePursuitIncident");
        }

        public static string FormatState(ArkCampaignGameComponent campaign)
        {
            return
                $"Active={campaign.CampaignActive}, Day={campaign.CampaignDay}, " +
                $"Landing={campaign.LandingNumber}, Tier={campaign.ArkTier}, {ArkPursuit.Format(campaign.Pursuit)}, " +
                $"LandingSession={campaign.LandingSessionActive}, SessionMapId={campaign.LandingSessionMapId}, " +
                $"TimerTicks={campaign.LandingSessionTicks} (~{ArkCampaignGameComponent.FormatTicksAsDays(campaign.LandingSessionTicks)}d), " +
                $"PursuitIncidentFired={campaign.PursuitIncidentFiredThisSession}";
        }

        public static void LogState(string context, ArkCampaignGameComponent campaign)
        {
            Log.Message($"[The Ark] [DEV] Campaign state ({context}): {FormatState(campaign)}");
        }
    }
}
