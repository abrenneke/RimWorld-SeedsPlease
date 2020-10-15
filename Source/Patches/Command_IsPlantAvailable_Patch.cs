using HarmonyLib;
using Verse;

namespace SeedsPlease
{
    [HarmonyPatch (typeof(Command_SetPlantToGrow), "IsPlantAvailable")]
    static class Command_IsPlantAvailable_Patch
    {
        public static void Postfix(ThingDef plantDef, Map map, ref bool __result)
        {
            if (__result && plantDef?.blueprintDef is SeedDef) {
                __result = map.listerThings.ThingsOfDef(plantDef.blueprintDef).Count > 0;
            }
        }
    }
}
