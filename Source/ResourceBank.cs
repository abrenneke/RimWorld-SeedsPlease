﻿using RimWorld;
using Verse;

namespace SeedsPlease
{
    public static class ResourceBank
    {
        public static readonly string StringPlantMinFertlity = "FertilityRequirement".Translate ();
        public static readonly string StringPlantFertilitySensitivity = "FertilitySensitivity".Translate();
        public static readonly string StringPlantGrowDays = "GrowDays".Translate();
        public static readonly string StringHarvestMultiplier = "HarvestMultiplier".Translate ();
        public static readonly string StringSeedMultiplier = "SeedMultiplier".Translate ();
        public static readonly string StringSeedBaseChance = "SeedBaseChance".Translate ();
        public static readonly string StringSeedExtraChance = "SeedExtraChance".Translate ();

        public static readonly string StringTextMoteHarvestFailed = "TextMote_HarvestFailed".Translate();

        public static readonly string StringSettingsCategory = "SeedsPlease.SettingsCategory".Translate();
        public static readonly string StringSettingsPlaceSeedsInInventory = "SeedsPlease.Settings.PlaceSeedsInInventory".Translate();
        public static readonly string StringSettingsPlaceSeedsInInventoryTooltip = "SeedsPlease.Settings.PlaceSeedsInInventory.Tooltip".Translate();

        [DefOf]
        public static class JobDefOf
        {
            public static JobDef SowWithSeeds;
            public static JobDef UnloadSeeds;
        }

        [DefOf]
        public static class ThingDefOf
        {
            public static ThingDef PlantProcessingTable;
            public static ThingDef Seed_Psychoid;
        }
    }
}