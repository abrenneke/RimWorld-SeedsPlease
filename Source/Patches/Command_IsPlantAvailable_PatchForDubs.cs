﻿using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace SeedsPlease
{
    [HarmonyPatch]
    static class Command_IsPlantAvailable_PatchForDubs
    {
        static MethodBase target;

        static bool Prepare()
        {
            var mod = LoadedModManager.RunningMods.FirstOrDefault(m => m.Name == "Dubs Mint Menus");
            if (mod == null) {
                return false;
            }

            var type = mod.assemblies.loadedAssemblies
                .FirstOrDefault(a => a.GetName().Name == "DubsMintMenus")?
                .GetType("DubsMintMenus.Dialog_FancyDanPlantSetterBob");

            if (type == null) {
                Log.Warning("SeedsPlease :: Can't patch DubsMintMenu. No Dialog_FancyDanPlantSetterBob");

                return false;
            }

            target = AccessTools.DeclaredMethod(type, "IsPlantAvailable");

            if (target == null) {
                Log.Warning("SeedsPlease :: Can't patch DubsMintMenu. No IsPlantAvailable");

                return false;
            }

            return true;
        }

        static MethodBase TargetMethod()
        {
            return target;
        }

        static void Postfix(ThingDef plantDef, Map map, ref bool __result)
        {
            Command_IsPlantAvailable_Patch.Postfix(plantDef, map, ref __result);
        }
    }
}
