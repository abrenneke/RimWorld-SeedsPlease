using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace SeedsPlease
{
    public class JobDriver_PlantSowWithSeeds : JobDriver
    {
        const TargetIndex targetCellIndex = TargetIndex.A;

        const TargetIndex seedsTargetIndex = TargetIndex.B;

        float sowWorkDone;

        public override string GetReport ()
        {
            string text = JobDefOf.Sow.reportString;
            text = text.Replace("TargetA", job.plantDefToSow != null ? GenLabel.ThingLabel (job.plantDefToSow, null) : "area");
            return text;
        }

        public override void ExposeData ()
        {
            base.ExposeData ();
            Scribe_Values.Look (ref sowWorkDone, "sowWorkDone");
        }

        protected override IEnumerable<Toil> MakeNewToils ()
        {
            this.FailOnDespawnedNullOrForbidden (targetCellIndex);

            yield return Toils_Reserve.Reserve (targetCellIndex);

            var reserveSeeds = ReserveSeedsIfWillPlantWholeStack ();
            yield return reserveSeeds;

            yield return Toils_Goto.GotoThing (seedsTargetIndex, PathEndMode.ClosestTouch)
                                   .FailOnDespawnedNullOrForbidden (seedsTargetIndex)
                                   .FailOnSomeonePhysicallyInteracting (seedsTargetIndex);

            yield return Toils_Haul.StartCarryThing (seedsTargetIndex)
                                   .FailOnDestroyedNullOrForbidden (seedsTargetIndex);

            Toils_Haul.CheckForGetOpportunityDuplicate (reserveSeeds, seedsTargetIndex, TargetIndex.None);

            var toil = Toils_Goto.GotoCell (targetCellIndex, PathEndMode.Touch);
            yield return toil;
            yield return SowSeedToil ();
            yield return Toils_Reserve.Release (targetCellIndex);
            yield return TryToSetAdditionalPlantingSite ();
            yield return Toils_Reserve.Reserve (targetCellIndex);
            yield return Toils_Jump.Jump (toil);
        }

        Toil ReserveSeedsIfWillPlantWholeStack ()
        {
            return new Toil {
                initAction = delegate
                {
                    if (pawn.Faction == null)
                    {
                        return;
                    }

                    var thing = job.GetTarget(seedsTargetIndex).Thing;
                    if (pawn.carryTracker.CarriedThing == thing)
                    {
                        return;
                    }

                    if (job.count >= thing.stackCount)
                    {
                        pawn.Reserve(thing, job);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant,
                atomicWithPrevious = true
            };
        }

        Toil SowSeedToil ()
        {
            var toil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never
            };
            toil.initAction = delegate
            {
                var actor = toil.actor;
                if (IsActorCarryingAppropriateSeed(actor, job.plantDefToSow))
                {
                    var plant = (Plant) GenSpawn.Spawn(job.plantDefToSow, TargetLocA, actor.Map);
                    plant.Growth = 0;
                    plant.sown = true;

                    job.targetC = plant;

                    actor.Reserve(job.targetC, job);

                    sowWorkDone = 0;
                }
                else
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            };
            toil.tickAction = delegate
            {
                var actor = toil.actor;

                var plant = (Plant) job.targetC.Thing;

                actor.skills?.Learn(SkillDefOf.Plants, 0.22f);

                if (plant.LifeStage != PlantLifeStage.Sowing)
                {
                    Log.Error(this + " getting sowing work while not in Sowing life stage.");
                }

                sowWorkDone += actor.GetStatValue(StatDefOf.PlantWorkSpeed);

                if (!(sowWorkDone >= plant.def.plant.sowWork))
                    return;

                if (!IsActorCarryingAppropriateSeed(actor, job.plantDefToSow))
                {
                    EndJobWith(JobCondition.Incompletable);

                    return;
                }

                if (actor.carryTracker.CarriedThing.stackCount <= 1)
                {
                    actor.carryTracker.CarriedThing.Destroy(DestroyMode.Cancel);
                }
                else
                {
                    actor.carryTracker.CarriedThing.stackCount--;
                }

                plant.Growth = 0.05f;

                plant.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);

                actor.records.Increment(RecordDefOf.PlantsSown);

                ReadyForNextToil();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOnDespawnedNullOrForbidden (targetCellIndex);
            toil.FailOnCannotTouch(targetCellIndex, PathEndMode.Touch);
            toil.WithEffect (EffecterDefOf.Sow, targetCellIndex);
            toil.WithProgressBar (targetCellIndex, () => sowWorkDone / job.plantDefToSow.plant.sowWork, true);
            toil.PlaySustainerOrSound (() => SoundDefOf.Interact_Sow);
            toil.AddFinishAction (delegate {
                var actor = toil.actor;

                var thing = job.targetC.Thing;
                if (thing != null) {
                    var plant = (Plant)thing;
                    if (sowWorkDone < plant.def.plant.sowWork && !thing.Destroyed) {
                        thing.Destroy ();
                    }

                    actor.Map.reservationManager.Release (job.targetC, actor, job);

                    job.targetC = null;
                }
            });
            toil.activeSkill = (() => SkillDefOf.Plants);
            return toil;
        }

        Toil TryToSetAdditionalPlantingSite ()
        {
            var toil = new Toil ();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = delegate {
                Pawn actor = toil.actor;

                if (IsActorCarryingAppropriateSeed (actor, job.plantDefToSow) && GetNearbyPlantingSite (job.GetTarget (targetCellIndex).Cell, actor.Map, out var intVec)) {
                    job.SetTarget (targetCellIndex, intVec);

                    return;
                }

                EndJobWith (JobCondition.Incompletable);
            };

            return toil;
        }

        bool GetNearbyPlantingSite (IntVec3 originPos, Map map, out IntVec3 newSite)
        {
            bool Validator(IntVec3 tempCell) => 
                IsCellOpenForSowingPlantOfType(tempCell, map, job.plantDefToSow) 
                && GetActor().CanReserveAndReach(tempCell, PathEndMode.Touch, GetActor().NormalMaxDanger());

            return CellFinder.TryFindRandomCellNear (originPos, map, 2, Validator, out newSite);
        }

        static bool IsCellOpenForSowingPlantOfType (IntVec3 cell, Map map, ThingDef plantDef)
        {
            var playerSetPlantForCell = GetPlayerSetPlantForCell (cell, map);
            if (playerSetPlantForCell == null || !playerSetPlantForCell.CanAcceptSowNow ()) {
                return false;
            }

            var plantDefToGrow = playerSetPlantForCell.GetPlantDefToGrow ();
            if (plantDefToGrow == null || plantDefToGrow != plantDef) {
                return false;
            }

            if (cell.GetPlant (map) != null) {
                return false;
            }

            if (PlantUtility.AdjacentSowBlocker (plantDefToGrow, cell, map) != null) {
                return false;
            }

            foreach (Thing current in map.thingGrid.ThingsListAt (cell)) {
                if (current.def.BlocksPlanting(true)) {
                    return false;
                }
            }
            return (plantDefToGrow.CanEverPlantAt (cell, map) && PlantUtility.GrowthSeasonNow (cell, map));
        }

        static IPlantToGrowSettable GetPlayerSetPlantForCell (IntVec3 cell, Map map)
        {
            var plantToGrowSettable = cell.GetEdifice (map) as IPlantToGrowSettable;
            if (plantToGrowSettable == null) {
                plantToGrowSettable = (map.zoneManager.ZoneAt (cell) as IPlantToGrowSettable);
            }
            return plantToGrowSettable;
        }

        static bool IsActorCarryingAppropriateSeed (Pawn pawn, ThingDef thingDef)
        {
            if (pawn.carryTracker == null) {
                return false;
            }

            var carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null || carriedThing.stackCount < 1) {
                return false;
            }

            if (thingDef.blueprintDef != carriedThing.def) {
                return false;
            }

            return true;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve (job.targetA, job, 1, -1, null, errorOnFailed);
        }
    }
}
