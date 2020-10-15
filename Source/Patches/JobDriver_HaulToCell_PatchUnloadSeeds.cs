using HarmonyLib;
using Verse.AI;

namespace SeedsPlease
{
    [HarmonyPatch(typeof(JobDriver_HaulToCell), "MakeNewToils")]
    public static class JobDriver_HaulToCell_PatchUnloadSeeds
    {
        // If hauling something, try to also unload any seeds in inventory
        public static void Postfix(JobDriver_HaulToCell __instance)
        {
            if (__instance.job.haulMode == HaulMode.ToCellStorage)
                JobDriver_PlantWorkWithSeeds.TryUnloadSeeds(__instance.pawn);
        }
    }
}
