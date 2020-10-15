using Verse.AI;

namespace SeedsPlease
{
    public class JobDriver_PlantCutWithSeeds : JobDriver_PlantWorkWithSeeds
    {
        protected override void Init ()
        {
            xpPerTick = Plant.def.plant.harvestedThingDef != null && Plant.YieldNow() > 0 ? 0.17f : 0f;
        }

        protected override Toil PlantWorkDoneToil ()
        {
            return new Toil
            {
                initAction = () =>
                {
                    var thing = job.GetTarget(TargetIndex.A).Thing;
                    if (!thing.Destroyed)
                    {
                        thing.Destroy();
                    }
                }
            };
        }
    }
}
