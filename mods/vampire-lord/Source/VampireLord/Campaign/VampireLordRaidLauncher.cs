using RimWorld;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Builds <see cref="IncidentParms"/> and executes vanilla <see cref="IncidentDefOf.RaidEnemy"/>.
    /// </summary>
    public static class VampireLordRaidLauncher
    {
        public static bool TryLaunchRaid(VampireLordWaveType waveType, float points, out string failReason)
        {
            Map map = ResolveTargetMap();
            if (map == null)
            {
                failReason = "no player home map";
                return false;
            }

            Faction faction = Find.FactionManager.RandomRaidableEnemyFaction(
                allowHidden: false,
                allowDefeated: false,
                allowNonHumanlike: true,
                minTechLevel: TechLevel.Undefined);
            if (faction == null)
            {
                failReason = "no raidable enemy faction";
                return false;
            }

            RaidStrategyDef strategy = ResolveStrategy(waveType);
            if (strategy == null)
            {
                failReason = $"raid strategy missing for {waveType}";
                return false;
            }

            var parms = new IncidentParms
            {
                target = map,
                points = points,
                faction = faction,
                forced = true,
                raidStrategy = strategy,
                bypassStorytellerSettings = true
            };

            ApplyArchetypeTweaks(waveType, parms);

            IncidentDef raidDef = IncidentDefOf.RaidEnemy;
            if (raidDef?.Worker == null)
            {
                failReason = "IncidentDefOf.RaidEnemy unavailable";
                return false;
            }

            bool executed = raidDef.Worker.TryExecute(parms);
            if (!executed)
            {
                failReason = "RaidEnemy.TryExecute returned false";
                return false;
            }

            failReason = null;
            return true;
        }

        public static Map ResolveTargetMap()
        {
            Map current = Find.CurrentMap;
            if (current != null && current.IsPlayerHome)
            {
                return current;
            }

            return Find.AnyPlayerHomeMap;
        }

        public static RaidStrategyDef ResolveStrategy(VampireLordWaveType waveType)
        {
            switch (waveType)
            {
                case VampireLordWaveType.Breachers:
                    return GetStrategy("ImmediateAttackBreaching")
                           ?? GetStrategy("ImmediateAttackSappers")
                           ?? RaidStrategyDefOf.ImmediateAttack;

                case VampireLordWaveType.Siege:
                    return GetStrategy("Siege") ?? RaidStrategyDefOf.ImmediateAttack;

                case VampireLordWaveType.Hunters:
                    return GetStrategy("ImmediateAttackSmart")
                           ?? RaidStrategyDefOf.ImmediateAttack;

                case VampireLordWaveType.Fire:
                    // V0: no reliable vanilla "force incendiary loadouts" knob without custom pawn gen.
                    // Closest tactical pressure: immediate assault (see RIMWORLD_API_NOTES.md).
                    return RaidStrategyDefOf.ImmediateAttack;

                case VampireLordWaveType.Mob:
                default:
                    return RaidStrategyDefOf.ImmediateAttack;
            }
        }

        private static void ApplyArchetypeTweaks(VampireLordWaveType waveType, IncidentParms parms)
        {
            switch (waveType)
            {
                case VampireLordWaveType.Mob:
                    // Favour numbers via points already set; keep vanilla immediate assault.
                    parms.generateFightersOnly = false;
                    break;

                case VampireLordWaveType.Hunters:
                    // Prefer fighters; quality still comes from vanilla raid generation at these points.
                    parms.generateFightersOnly = true;
                    break;

                case VampireLordWaveType.Fire:
                    // Documented limitation: cannot force Molotov/incendiary kits via IncidentParms alone.
                    parms.generateFightersOnly = false;
                    break;

                default:
                    break;
            }
        }

        private static RaidStrategyDef GetStrategy(string defName)
        {
            return DefDatabase<RaidStrategyDef>.GetNamedSilentFail(defName);
        }
    }
}
