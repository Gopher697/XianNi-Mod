using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.assets;

namespace xn.world
{
    internal static class LingshiVeinAssets
    {
        public const string ID = "xn_building_lingshi_vein";

        private static bool _patchRegistered;
        private static bool _auraPatchRegistered;
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

            RegisterAuraGenerationPatch(harmony);
            RegisterIfNeeded();
        }

        private static void RegisterAuraGenerationPatch(Harmony harmony)
        {
            if (_auraPatchRegistered || harmony == null)
            {
                return;
            }

            var method = AccessTools.Method(
                typeof(AuraChunkSystem),
                "ComputeChunkGeneration",
                new Type[] { typeof(int), typeof(int), typeof(int), typeof(HashSet<City>) });
            if (method == null)
            {
                Debug.LogWarning("[XN] AuraChunkSystem.ComputeChunkGeneration not found; lingshi vein aura bonus was not patched.");
                return;
            }

            harmony.Patch(method, postfix: new HarmonyMethod(typeof(LingshiVeinAssets), nameof(PostComputeChunkGeneration)));
            _auraPatchRegistered = true;
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
            asset.sparkle_effect = false;
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
            if (!asset.sprites_are_initiated)
            {
                Sprite[] fallback = SpriteTextureLoader.getSpriteList(asset.sprite_path);
                if (fallback != null && fallback.Length > 0)
                    Debug.Log($"[XN] Lingshi vein: loaded {fallback.Length} sprite(s) via path fallback.");
                else
                    Debug.LogWarning("[XN] Lingshi vein: sprite failed to load from path, building will be invisible.");
            }
            _registered = true;
        }

        private static void PostComputeChunkGeneration(int cx, int cy, ref int __result)
        {
            int count = CountVeinsInChunk(cx, cy);
            if (count <= 0)
            {
                return;
            }

            __result += count * 20;
        }

        private static int CountVeinsInChunk(int cx, int cy)
        {
            if (World.world == null)
            {
                return 0;
            }

            int startX = cx * AuraChunkSystem.CHUNK_SIZE;
            int startY = cy * AuraChunkSystem.CHUNK_SIZE;
            int endX = Mathf.Min(startX + AuraChunkSystem.CHUNK_SIZE, MapBox.width);
            int endY = Mathf.Min(startY + AuraChunkSystem.CHUNK_SIZE, MapBox.height);
            int count = 0;
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    WorldTile tile = World.world.GetTile(x, y);
                    BuildingAsset asset = xn.access.BuildingAccess.GetAsset(tile != null ? tile.building : null);
                    if (asset != null && asset.id == ID)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
