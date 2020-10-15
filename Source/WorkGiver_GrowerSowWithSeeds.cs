using RimWorld;
using Verse;
using Verse.AI;

namespace SeedsPlease
{
    public class WorkGiver_GrowerSowWithSeeds : WorkGiver_GrowerSow
    {
        const int SEEDS_TO_CARRY = 25;

    	public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            var job = base.JobOnCell (pawn, c, forced);

            // plant has seeds, if there is a seed return a job, otherwise prevent it. Seeds with no category are forbidden.
            var seed = job?.plantDefToSow?.blueprintDef;
            if (seed == null || seed.thingCategories.NullOrEmpty())
                return job;
            
            // Clear the area some...
            var zone = c.GetZone (pawn.Map);
            if (zone != null) {
                foreach (var corner in GenAdj.AdjacentCells8WayRandomized ()) {
                    var cell = c + corner;
                    if (!zone.ContainsCell(cell))
                        continue;
                        
                    foreach (var thing in pawn.Map.thingGrid.ThingsAt (cell))
                    {
                        if (thing.def == job.plantDefToSow || !thing.def.BlocksPlanting(true) || !pawn.CanReserve(thing) || thing.IsForbidden(pawn))
                            continue;
                            
                        if (thing.def.category == ThingCategory.Plant) {
                            return new Job(JobDefOf.CutPlant, thing);
                        }
                        if (thing.def.EverHaulable) {
                            return HaulAIUtility.HaulAsideJobFor(pawn, thing);
                        }
                    }
                }
            }

            bool Predicate(Thing tempThing) => 
                !tempThing.IsForbidden(pawn.Faction) 
                && tempThing.Position.InAllowedArea(pawn) 
                && pawn.AnimalAwareOf(tempThing) 
                && pawn.CanReserve(tempThing);

            Thing bestSeedThingForSowing = GenClosest.ClosestThingReachable (
                c, pawn.Map, ThingRequest.ForDef (job.plantDefToSow.blueprintDef),
                PathEndMode.ClosestTouch, TraverseParms.For (pawn), 9999,
                Predicate);

            if (bestSeedThingForSowing != null) {
                return new Job (ResourceBank.JobDefOf.SowWithSeeds, c, bestSeedThingForSowing) {
                    plantDefToSow = job.plantDefToSow,
                    count = SEEDS_TO_CARRY
                };
            }
            return null;
        }
    }
}
