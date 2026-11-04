using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Scenario setup for The Lantern Village:
    /// 1) During map generation (<see cref="GenerateIntoMap"/>), place the prebuilt
    ///    medieval hamlet (Vampire Lord ScenPart courtyard pattern — real buildings,
    ///    once per new colony, never on save load).
    /// 2) After game start, apply the five founding colonists' roles (skill floors,
    ///    fitting traits, Elder–Healer spouse and Elder–Hunter parent relations).
    /// Skills/traits are only ever raised to a floor, never lowered.
    /// </summary>
    public class ScenPart_LanternVillageSetup : ScenPart
    {
        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            // Do not gate on Find.Scenario here — during map gen it can be unset.
            // This ScenPart only exists on The Lantern Village scenario / ScenarioDef.
            Log.Message("[Lantern Village] ScenPart.GenerateIntoMap — building hamlet.");
            LanternHamletBuilder.TryBuild(map);
        }

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            if (!LanternHamletBuilder.WasBuiltFor(map))
            {
                Log.Message("[Lantern Village] ScenPart.PostMapGenerate — hamlet missing; building now.");
                LanternHamletBuilder.TryBuild(map);
            }
        }

        private class RoleSpec
        {
            public string RoleName;
            public Dictionary<string, int> SkillFloors;
            public string TraitDefName;
        }

        private static readonly List<RoleSpec> Roles = new List<RoleSpec>
        {
            new RoleSpec { RoleName = "Elder",    SkillFloors = new Dictionary<string, int> { { "Social", 8 }, { "Intellectual", 5 }, { "Medicine", 3 } }, TraitDefName = "Kind" },
            new RoleSpec { RoleName = "Woodsman",  SkillFloors = new Dictionary<string, int> { { "Plants", 8 }, { "Construction", 6 } }, TraitDefName = "Industrious" },
            new RoleSpec { RoleName = "Hunter",    SkillFloors = new Dictionary<string, int> { { "Shooting", 9 }, { "Animals", 6 } }, TraitDefName = "Nimble" },
            new RoleSpec { RoleName = "Smith",     SkillFloors = new Dictionary<string, int> { { "Crafting", 8 }, { "Mining", 5 } }, TraitDefName = "Industrious" },
            new RoleSpec { RoleName = "Healer",    SkillFloors = new Dictionary<string, int> { { "Medicine", 8 }, { "Cooking", 6 } }, TraitDefName = "Kind" },
        };

        public override void PostGameStart()
        {
            base.PostGameStart();

            // Last-chance hamlet placement if map-gen hooks were skipped for any reason.
            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map != null && !LanternHamletBuilder.WasBuiltFor(map))
            {
                Log.Warning("[Lantern Village] PostGameStart — hamlet still missing; building now (pawns may already be on the map).");
                LanternHamletBuilder.TryBuild(map);
            }

            // Always stabilize settlement + colonist faction — even if ScenarioGate naming
            // fails — so tick-0 SettlementDefeat / Non-colonist apparel errors stop.
            TryEnsureStartingPawnsAreColonists();
            LanternVillagePlayerHome.TryEnsure("PostGameStart");
            // Second pass after Settle — some arrival paths attach pawns after home exists.
            TryEnsureStartingPawnsAreColonists();

            if (!ScenarioGate.IsActive())
            {
                return;
            }

            try
            {
                List<Pawn> pawns = GetStartingPawns();
                if (pawns == null || pawns.Count < Roles.Count)
                {
                    Log.Warning($"[Lantern Village] Expected {Roles.Count} starting pawns, found {pawns?.Count ?? 0}. Skipping role assignment.");
                }
                else
                {
                    for (int i = 0; i < Roles.Count; i++)
                    {
                        ApplyRole(pawns[i], Roles[i]);
                        // Role rename can run while Faction is still unset — force after rename.
                        if (pawns[i] != null && Faction.OfPlayer != null && pawns[i].Faction != Faction.OfPlayer)
                        {
                            pawns[i].SetFaction(Faction.OfPlayer);
                        }
                    }

                    ApplyRelationships(pawns);
                }

                // Guard dog softens the one allowed early Anomaly scrape.
                TryTrainStartingGuardDog(pawns);
            }
            catch (Exception ex)
            {
                Log.Error($"[Lantern Village] Failed while applying colonist roles: {ex}");
            }
        }

        private static void TryEnsureStartingPawnsAreColonists()
        {
            if (Faction.OfPlayer == null)
            {
                return;
            }

            int fixedCount = 0;

            // GameInitData pawns may not be spawned yet when apparel jobs race tick 0.
            if (Find.GameInitData?.startingAndOptionalPawns != null)
            {
                foreach (Pawn pawn in Find.GameInitData.startingAndOptionalPawns)
                {
                    if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike)
                    {
                        continue;
                    }

                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        pawn.SetFaction(Faction.OfPlayer);
                        fixedCount++;
                    }
                }
            }

            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map?.mapPawns != null)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.ToList())
                {
                    if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike)
                    {
                        continue;
                    }

                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        pawn.SetFaction(Faction.OfPlayer);
                        fixedCount++;
                    }
                }
            }

            if (fixedCount > 0)
            {
                Log.Message($"[Lantern Village] Reassigned {fixedCount} humanlike pawn(s) to player faction.");
            }
        }

        private static List<Pawn> GetStartingPawns()
        {
            if (Find.GameInitData?.startingAndOptionalPawns != null && Find.GameInitData.startingAndOptionalPawns.Count > 0)
            {
                return Find.GameInitData.startingAndOptionalPawns;
            }
            // Fallback once the map actually exists: whichever free colonists are on the current map.
            return Find.CurrentMap?.mapPawns?.FreeColonists?.ToList();
        }

        private static void ApplyRole(Pawn pawn, RoleSpec role)
        {
            if (pawn?.skills == null)
            {
                return;
            }

            foreach (KeyValuePair<string, int> floor in role.SkillFloors)
            {
                SkillDef skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(floor.Key);
                if (skillDef == null)
                {
                    Log.Warning($"[Lantern Village] SkillDef '{floor.Key}' not found; cannot apply {role.RoleName}'s floor.");
                    continue;
                }
                SkillRecord record = pawn.skills.GetSkill(skillDef);
                if (record != null && record.Level < floor.Value)
                {
                    record.Level = floor.Value;
                    record.passion = record.passion == Passion.None ? Passion.Minor : record.passion;
                }
            }

            if (!string.IsNullOrEmpty(role.TraitDefName) && pawn.story?.traits != null)
            {
                TraitDef traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(role.TraitDefName);
                if (traitDef != null && !pawn.story.traits.HasTrait(traitDef))
                {
                    pawn.story.traits.GainTrait(new Trait(traitDef, 0, false));
                }
            }

            pawn.Name = pawn.Name is NameTriple triple
                ? new NameTriple(triple.First, $"{role.RoleName}", triple.Last)
                : pawn.Name;
        }

        private static void ApplyRelationships(List<Pawn> pawns)
        {
            // Index 0 = Elder, 1 = Woodsman, 2 = Hunter, 3 = Smith, 4 = Healer.
            // Married couple: Elder & Healer. Parent/adult-child: Elder -> Hunter.
            Pawn elder = pawns[0];
            Pawn healer = pawns[4];
            Pawn hunter = pawns[2];

            if (elder.relations != null && healer.relations != null &&
                !elder.relations.DirectRelationExists(PawnRelationDefOf.Spouse, healer))
            {
                elder.relations.AddDirectRelation(PawnRelationDefOf.Spouse, healer);
            }

            if (elder.relations != null && hunter.relations != null &&
                !elder.relations.DirectRelationExists(PawnRelationDefOf.Parent, hunter))
            {
                elder.relations.AddDirectRelation(PawnRelationDefOf.Parent, hunter);
            }
        }

        /// <summary>
        /// Starting Dog from the scenario gets full Obedience + Release training and
        /// follows the Hunter (or first colonist) when drafted.
        /// </summary>
        private static void TryTrainStartingGuardDog(List<Pawn> startingHumans)
        {
            List<Pawn> dogs = FindStartingDogs();
            if (dogs.Count == 0)
            {
                Log.Warning("[Lantern Village] No starting Dog found to train as guard.");
                return;
            }

            Pawn master = PreferHunterMaster(startingHumans);
            foreach (Pawn dog in dogs)
            {
                TrainCombatReady(dog, master);
            }
        }

        private static List<Pawn> FindStartingDogs()
        {
            List<Pawn> dogs = new List<Pawn>();

            void Consider(Pawn pawn)
            {
                if (pawn?.RaceProps == null || !pawn.RaceProps.Animal)
                {
                    return;
                }

                if (pawn.kindDef == null || pawn.kindDef.defName != "Dog")
                {
                    return;
                }

                if (Faction.OfPlayer != null && pawn.Faction != Faction.OfPlayer)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                }

                if (!dogs.Contains(pawn))
                {
                    dogs.Add(pawn);
                }
            }

            if (Find.GameInitData?.startingAndOptionalPawns != null)
            {
                foreach (Pawn pawn in Find.GameInitData.startingAndOptionalPawns)
                {
                    Consider(pawn);
                }
            }

            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map?.mapPawns != null)
            {
                foreach (Pawn pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer).ToList())
                {
                    Consider(pawn);
                }
            }

            return dogs;
        }

        private static Pawn PreferHunterMaster(List<Pawn> startingHumans)
        {
            if (startingHumans != null && startingHumans.Count > 2 && startingHumans[2] != null)
            {
                // Index 2 = Hunter after role assignment.
                return startingHumans[2];
            }

            return Find.CurrentMap?.mapPawns?.FreeColonists?.FirstOrDefault();
        }

        private static void TrainCombatReady(Pawn dog, Pawn master)
        {
            if (dog.training == null)
            {
                Log.Warning($"[Lantern Village] Dog '{dog.LabelShort}' has no training tracker; cannot combat-train.");
                return;
            }

            // Obedience then Release — Release is the vanilla "fight when released / with drafted master" trainable.
            TrainFully(dog, TrainableDefOf.Obedience);
            TrainFully(dog, TrainableDefOf.Release);

            if (dog.playerSettings != null)
            {
                if (master != null && TrainableUtility.CanBeMaster(master, dog, checkSpawned: false))
                {
                    dog.playerSettings.Master = master;
                }

                dog.playerSettings.followDrafted = true;
                dog.playerSettings.followFieldwork = true;
            }

            string masterLabel = master != null ? master.LabelShort : "(none)";
            Log.Message(
                $"[Lantern Village] Guard dog '{dog.LabelShort}' combat-trained (Obedience+Release); master={masterLabel}.");
        }

        private static void TrainFully(Pawn animal, TrainableDef trainable)
        {
            if (trainable == null || animal?.training == null)
            {
                return;
            }

            AcceptanceReport canAssign = animal.training.CanAssignToTrain(trainable);
            if (!canAssign.Accepted && !animal.training.HasLearned(trainable))
            {
                // Still try Train — some animals report visibility quirks at tick 0.
                Log.Message(
                    $"[Lantern Village] Trainable '{trainable.defName}' for '{animal.LabelShort}' reported: {canAssign.Reason}");
            }

            animal.training.SetWantedRecursive(trainable, true);
            animal.training.Train(trainable, trainer: null, complete: true);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
