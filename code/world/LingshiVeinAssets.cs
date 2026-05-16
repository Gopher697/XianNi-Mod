using HarmonyLib;
using UnityEngine;
using xn.assets;

namespace xn.world
{
    internal static class LingshiVeinAssets
    {
        public const string ID = "xn_building_lingshi_vein";

        private static bool _patchRegistered;
        private static bool _registered;

        public static void Init(Harmony harmony)
        {
            if (!_patchRegistered && harmony != null)
            {
                var method = AccessTools.Method(typeof(BuildingLibrary), "init");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(LingshiVeinAssets), nameof(PostBuildingLibraryInit)));
                    _patchRegistered = true;
                }
                else
                {
                    Debug.LogWarning("[XN] BuildingLibrary.init not found; lingshi vein patch was not registered.");
                }
            }

            RegisterIfNeeded();
        }

        private static void PostBuildingLibraryInit()
        {
            RegisterIfNeeded();
        }

        public static void RegisterIfNeeded()
        {
            if (_registered)
            {
                return;
            }
            if (AssetManager.buildings == null)
            {
                return;
            }
            if (AssetManager.buildings.get(ID) != null)
            {
                _registered = true;
                return;
            }

            XNResourceRegistry.RegisterIfNeeded();

            BuildingAsset asset = new BuildingAsset();
            asset.id = ID;
            asset.main_path = "buildings/minerals/";
            asset.sprite_path = "buildings/minerals/mineral_gems";
            asset.scale_base = new Vector3(0.25f, 0.25f, 0.25f);
            asset.fundament = new BuildingFundament(1, 1, 1, 0);
            asset.group = "nature";
            asset.kingdom = "nature";
            asset.type = "type_mineral";
            asset.material = "building";
            asset.building_type = BuildingType.Building_Mineral;
            asset.city_building = false;
            asset.has_resources_to_collect = true;
            asset.can_be_demolished = false;
            asset.remove_ruins = false;
            asset.remove_buildings_when_dropped = false;
            asset.spawn_units = false;
            asset.has_ruin_state = false;
            asset.has_ruins_graphics = false;
            asset.has_sprites_main = true;
            asset.destroy_on_liquid = true;
            asset.can_be_placed_on_liquid = false;
            asset.can_be_placed_on_blocks = false;
            asset.ignore_buildings = false;
            asset.ignore_same_building_id = true;
            asset.ignored_by_cities = true;
            asset.random_flip = true;
            asset.sparkle_effect = true;
            asset.nutrition_restore = 40;
            asset.setAtlasID("buildings");
            if (AssetManager.dynamic_sprites_library != null)
            {
                asset.atlas_asset = AssetManager.dynamic_sprites_library.get(asset.atlas_id);
            }
            asset.addResource(XNResourceRegistry.LingshiResourceId, 20);

            AssetManager.buildings.add(asset);
            asset.base_stats["health"] = 20f;
            asset.loadBuildingSprites();
            _registered = true;
        }
    }
}
