using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Tick-0 SettlementDefeatUtility.CheckDefeated fires CaravanAssaultSuccessful with a
    /// null pawn when a settlement has no faction members yet — including the player's
    /// brand-new home before colonists finish attaching. Block defeat checks for the
    /// player faction (and the first in-game day) under The Lantern Village scenario.
    /// </summary>
    [HarmonyPatch(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated))]
    public static class Patch_SettlementDefeatUtility_CheckDefeated
    {
        public static bool Prefix(Settlement factionBase)
        {
            if (!ScenarioGate.IsActive())
            {
                return true;
            }

            if (factionBase == null)
            {
                return false;
            }

            if (factionBase.Faction == null || factionBase.Faction.IsPlayer)
            {
                return false;
            }

            // Give world gen / pawn arrival a day before NPC empty-settlement defeat tales.
            if (Find.TickManager != null && Find.TickManager.TicksGame < GenDate.TicksPerDay)
            {
                return false;
            }

            return true;
        }
    }
}
