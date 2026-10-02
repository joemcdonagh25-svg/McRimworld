using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// Clears illegal faction relations that make vanilla
    /// <c>GoodwillSituationManager.GetSituations(PlayerColony)</c> NRE during
    /// <c>RecalculateAll</c> (player goodwill caps only accept non-player <c>other</c>).
    /// </summary>
    public static class VampireLordFactionHygiene
    {
        private const string LogPrefix = "[VampireLord]";

        private static readonly FieldInfo RelationsField = typeof(Faction).GetField(
            "relations",
            BindingFlags.Instance | BindingFlags.NonPublic);

        public static void TryRepair(string phase)
        {
            Faction player = Faction.OfPlayer;
            if (player == null || Find.FactionManager == null)
            {
                Log.Warning($"{LogPrefix} Faction hygiene ({phase}): Faction.OfPlayer or FactionManager missing.");
                return;
            }

            if (RelationsField == null)
            {
                Log.Warning($"{LogPrefix} Faction hygiene ({phase}): Faction.relations field missing.");
                return;
            }

            int playerLike = CountPlayerLike();
            if (playerLike != 1)
            {
                Log.Warning(
                    $"{LogPrefix} Faction hygiene ({phase}): IsPlayer count={playerLike} (expected 1). " +
                    $"OfPlayer={DescribeFaction(player)}. {DumpPlayerLike()}");
            }

            int removed = StripInvalidRelations(player);
            List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
            for (int i = 0; i < factions.Count; i++)
            {
                Faction faction = factions[i];
                if (faction == null || faction == player)
                {
                    continue;
                }

                removed += StripInvalidRelations(faction);
                EnsureRelationWithPlayer(faction, player);
            }

            // Second pass on player after NPC cleanup (mutual junk can reappear only via Ensure).
            removed += StripInvalidRelations(player);

            Log.Message(
                $"{LogPrefix} Faction hygiene ({phase}): removed {removed} bad relation(s); " +
                $"IsPlayer={playerLike}, factions={factions.Count}.");
        }

        private static int CountPlayerLike()
        {
            int count = 0;
            List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
            for (int i = 0; i < factions.Count; i++)
            {
                if (factions[i] != null && factions[i].IsPlayer)
                {
                    count++;
                }
            }

            return count;
        }

        private static string DumpPlayerLike()
        {
            var sb = new StringBuilder();
            List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
            for (int i = 0; i < factions.Count; i++)
            {
                Faction f = factions[i];
                if (f == null || !f.IsPlayer)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }

                sb.Append(DescribeFaction(f));
            }

            return sb.Length == 0 ? "(none)" : sb.ToString();
        }

        private static string DescribeFaction(Faction f)
        {
            if (f == null)
            {
                return "null";
            }

            return $"{f.Name}/{f.def?.defName}/{f.GetUniqueLoadID()}/IsPlayer={f.IsPlayer}";
        }

        private static List<FactionRelation> GetRelations(Faction faction)
        {
            return RelationsField?.GetValue(faction) as List<FactionRelation>;
        }

        private static int StripInvalidRelations(Faction faction)
        {
            List<FactionRelation> relations = GetRelations(faction);
            if (relations == null)
            {
                return 0;
            }

            int removed = 0;
            for (int i = relations.Count - 1; i >= 0; i--)
            {
                FactionRelation rel = relations[i];
                if (rel == null)
                {
                    relations.RemoveAt(i);
                    removed++;
                    continue;
                }

                Faction other = rel.other;
                // Player goodwill situations only accept non-player `other`. A player→player
                // (or self) relation makes CheckKindThresholds call GetMaxGoodwill(PlayerColony).
                bool bad =
                    other == null
                    || other == faction
                    || (faction.IsPlayer && other.IsPlayer);

                if (!bad)
                {
                    continue;
                }

                relations.RemoveAt(i);
                removed++;
                Log.Message(
                    $"{LogPrefix} Faction hygiene: stripped relation {DescribeFaction(faction)} → " +
                    $"{DescribeFaction(other)}.");
            }

            return removed;
        }

        private static void EnsureRelationWithPlayer(Faction faction, Faction player)
        {
            if (faction.IsPlayer || faction.def == null)
            {
                return;
            }

            FactionRelation existing = faction.RelationWith(player, allowNull: true);
            if (existing != null && existing.other == player)
            {
                return;
            }

            try
            {
                faction.TryMakeInitialRelationsWith(player);
            }
            catch (System.Exception e)
            {
                Log.Warning(
                    $"{LogPrefix} Faction hygiene: TryMakeInitialRelationsWith failed for " +
                    $"{DescribeFaction(faction)}: {e.Message}");
            }
        }
    }
}
