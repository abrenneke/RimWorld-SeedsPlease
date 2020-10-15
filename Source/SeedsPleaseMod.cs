using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace SeedsPlease
{
    public class SeedsPleaseMod : Mod
    {
        internal static readonly List<string> knownPrefixes = new List<string>
        {
            "VG_Plant",
            "VGP_",
            "RC2_", 
            "RC_Plant",
            "TKKN_Plant",
            "TKKN_",
            "TM_",
            "Plant_",
            "WildPlant",
            "Wild",
            "Plant",
            "tree",
            "Tree"
        };

        public SeedsPleaseMod(ModContentPack content) : base(content)
        {
            GetSettings<SeedsPleaseSettings>();
            new Harmony("rimworld.seedsplease").PatchAll();
        }

        public override string SettingsCategory()
        {
            return ResourceBank.StringSettingsCategory;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<SeedsPleaseSettings>().DoWindowContents(inRect);
        }
    }
}
