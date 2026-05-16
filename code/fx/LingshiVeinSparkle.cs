using HarmonyLib;
using UnityEngine;
using xn.access;
using xn.world;

namespace xn.fx
{
    public static class LingshiVeinSparkle
    {
        private const string BehaviourId = "xn_lingshi_vein_sparkle";
        private const string EffectId = "xn_lingshi_vein_sparkle_fx";
        private static bool _patched;
        private static bool _registered;

        public static void Init(Harmony harmony)
        {
            if (!_patched)
            {
                harmony.Patch(
                    AccessTools.Method(typeof(WorldBehaviourLibrary), "init"),
                    postfix: new HarmonyMethod(typeof(LingshiVeinSparkle), nameof(PostWorldBehaviourLibraryInit)));
                _patched = true;
            }

            RegisterIfNeeded();
        }

        private static void PostWorldBehaviourLibraryInit(WorldBehaviourLibrary __instance)
        {
            RegisterIfNeeded(__instance);
        }

        private static void RegisterIfNeeded(WorldBehaviourLibrary library = null)
        {
            if (_registered)
                return;

            WorldBehaviourLibrary lib = library ?? AssetManager.world_behaviours;
            if (lib == null)
                return;

            if (lib.get(BehaviourId) != null)
            {
                _registered = true;
                return;
            }

            WorldBehaviourAsset asset = new WorldBehaviourAsset
            {
                id = BehaviourId,
                interval = 6f,
                interval_random = 3f,
                enabled = true,
                stop_when_world_on_pause = true,
                action = SparkleVeins
            };

            lib.add(asset);
            asset.manager = new WorldBehaviour(asset);
            _registered = true;
        }

        private static void SparkleVeins()
        {
            if (World.world == null || World.world.buildings == null)
                return;

            var list = World.world.buildings.getSimpleList();
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                Building building = list[i];
                if (building == null || !building.isAlive())
                    continue;

                BuildingAsset asset = BuildingAccess.GetAsset(building);
                if (asset == null || asset.id != LingshiVeinAssets.ID)
                    continue;

                WorldTile tile = building.current_tile;
                if (tile == null)
                    continue;

                EffectsLibrary.spawnAt(EffectId, tile.posV3, 1f);
            }
        }
    }
}
