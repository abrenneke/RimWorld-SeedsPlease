using HarmonyLib;
using Verse;
using Verse.AI;

namespace SeedsPlease.Patches
{
    [HarmonyPatch(typeof(JobGiver_Idle), "TryGiveJob")]
    public static class JobGiver_Idle_PatchUnloadSeeds
    {
        // If idle, try to unload any seeds in inventory
        public static void Postfix(Pawn pawn)
        {
            JobDriver_PlantWorkWithSeeds.TryUnloadSeeds(pawn);
        }
    }
}
