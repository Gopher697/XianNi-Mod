using HarmonyLib;
using UnityEngine;

namespace xn.world
{
    internal static class LingshiVeinSpawnBehaviour
    {
        private const string BehaviourId = "xn_spawn_lingshi_veins";
        private const int MinWorldYear = 50;
        private const int MinIslandTiles = 20;
        private const int MinChunkAura = 3000;

        private static bool _patchRegistered;
        private static bool _registered;

        public static void Init(Harmony harmony)
        {
            if (!_patchRegistered && harmony != null)
            {
                var method = AccessTools.Method(typeof(WorldBehaviourLibrary), "init");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(LingshiVeinSpawnBehaviour), nameof(PostWorldBehaviourLibraryInit)));
                    _patchRegistered = true;
                }
                else
                {
                    Debug.LogWarning("[XN] WorldBehaviourLibrary.init not found; lingshi vein spawn behaviour patch was not registered.");
                }
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
            {
                return;
            }

            library = library ?? AssetManager.world_behaviours;
            if (library == null)
            {
                return;
            }
            if (library.get(BehaviourId) != null)
            {
                _registered = true;
                return;
            }

            WorldBehaviourAsset asset = new WorldBehaviourAsset
            {
                id = BehaviourId,
                interval = 8f,
                interval_random = 4f,
                enabled = true,
                stop_when_world_on_pause = true,
                action = TrySpawnVeins
            };
            library.add(asset);
            asset.manager = new WorldBehaviour(asset);
            _registered = true;
        }

        private static void TrySpawnVeins()
        {
            if (World.world == null || World.world.islands_calculator == null)
            {
                return;
            }
            if (Date.getCurrentYear() < MinWorldYear)
            {
                return;
            }

            LingshiVeinAssets.RegisterIfNeeded();
            BuildingAsset veinAsset = AssetManager.buildings.get(LingshiVeinAssets.ID);
            if (veinAsset == null)
            {
                return;
            }

            var islands = World.world.islands_calculator.islands_ground;
            if (islands == null || islands.Count == 0)
            {
                return;
            }

            for (int i = 0; i < islands.Count; i++)
            {
                TileIsland island = islands[i];
                if (island == null || island.getTileCount() < MinIslandTiles || island.regions == null || island.regions.Count == 0)
                {
                    continue;
                }

                MapRegion region = island.regions.GetRandom();
                if (region == null || region.tiles == null || region.tiles.Count == 0)
                {
                    continue;
                }

                WorldTile centerTile = GetRegionCenterTile(region);
                if (centerTile == null)
                {
                    continue;
                }

                int chunkAura = AuraChunkSystem.GetAuraForTile(centerTile);
                if (chunkAura < MinChunkAura)
                {
                    continue;
                }
                if (RegionHasVein(region))
                {
                    continue;
                }

                float spawnChance = Mathf.Clamp(chunkAura / 100000f, 0.01f, 0.10f);
                if (!Randy.randomChance(spawnChance))
                {
                    continue;
                }

                WorldTile targetTile = PickBuildableTile(region, veinAsset);
                if (targetTile == null)
                {
                    continue;
                }

                xn.access.BuildingManagerAccess.AddBuilding(
                    World.world.buildings,
                    LingshiVeinAssets.ID,
                    targetTile,
                    checkForBuild: true,
                    sfx: false,
                    type: BuildPlacingType.New);
            }
        }

        private static WorldTile GetRegionCenterTile(MapRegion region)
        {
            if (region == null || region.tiles == null || region.tiles.Count == 0 || World.world == null)
            {
                return null;
            }

            long x = 0L;
            long y = 0L;
            for (int i = 0; i < region.tiles.Count; i++)
            {
                WorldTile tile = region.tiles[i];
                if (tile == null)
                {
                    continue;
                }
                x += tile.x;
                y += tile.y;
            }

            WorldTile center = World.world.GetTile((int)(x / region.tiles.Count), (int)(y / region.tiles.Count));
            if (center != null && center.region == region)
            {
                return center;
            }

            return region.tiles.GetRandom();
        }

        private static bool RegionHasVein(MapRegion region)
        {
            if (region == null || region.tiles == null)
            {
                return false;
            }

            for (int i = 0; i < region.tiles.Count; i++)
            {
                BuildingAsset asset = xn.access.BuildingAccess.GetAsset(region.tiles[i] != null ? region.tiles[i].building : null);
                if (asset != null && asset.id == LingshiVeinAssets.ID)
                {
                    return true;
                }
            }

            return false;
        }

        private static WorldTile PickBuildableTile(MapRegion region, BuildingAsset veinAsset)
        {
            if (region == null || region.tiles == null || veinAsset == null)
            {
                return null;
            }

            int attempts = Mathf.Min(region.tiles.Count, 24);
            for (int i = 0; i < attempts; i++)
            {
                WorldTile tile = region.tiles[UnityEngine.Random.Range(0, region.tiles.Count)];
                if (tile == null || tile.isOnFire())
                {
                    continue;
                }
                if (xn.access.BuildingManagerAccess.CanBuildFrom(World.world.buildings, tile, veinAsset, null, BuildPlacingType.New))
                {
                    return tile;
                }
            }

            return null;
        }
    }
}
