using HarmonyLib;
using xn.world;
namespace xn.expand
{
    public static class BuildingProtectionSystem
    {
        public static void Init(Harmony harmony)
        {
            harmony.PatchAll(typeof(BuildingProtectionSystem));
        }
        private static bool ShouldProtectBuilding(Building building)
        {
            if (building == null || !building.isAlive()) return false;
            if (building.asset == null) return false;
            if (!xn.config.ModConfigHooks.EnableBuildingProtection) return false;
            if (building.asset.id == RuinBuildingAssets.ID) return false;
            if (building.asset.tower) return false;
            return building.state_ownership == BuildingOwnershipState.Civilization;
        }
        private static bool IsRuinBuilding(Building building)
        {
            return building != null 
                && building.asset != null 
                && building.asset.id == RuinBuildingAssets.ID;
        }
        #region Harmony Patches
        [HarmonyPatch(typeof(Building), "getHit")]
        [HarmonyPrefix]
        static bool Prefix_getHit(Building __instance, float pDamage)
        {
            if (ShouldProtectBuilding(__instance))
            {
                return false; 
            }
            return true; 
        }
        [HarmonyPatch(typeof(Building), "startDestroyBuilding")]
        [HarmonyPrefix]
        static bool Prefix_startDestroyBuilding(Building __instance)
        {
            if (ShouldProtectBuilding(__instance))
            {
                return false; 
            }
            return true; 
        }
        [HarmonyPatch(typeof(MapAction), "terraformTile")]
        [HarmonyPrefix]
        static void Prefix_terraformTile(WorldTile pTile, TerraformOptions pOptions)
        {
            if (pOptions == null || pTile == null) return;
            if (!pOptions.destroy_buildings || !pTile.hasBuilding()) return;
            var building = pTile.building;
            if (building == null) return;
            if (ShouldProtectBuilding(building))
            {
                if (pOptions.ignore_buildings == null)
                {
                    pOptions.ignore_buildings = new System.Collections.Generic.List<string>();
                }
                if (!pOptions.ignore_buildings.Contains(building.asset.id))
                {
                    pOptions.ignore_buildings.Add(building.asset.id);
                }
            }
        }
        #endregion
    }
}