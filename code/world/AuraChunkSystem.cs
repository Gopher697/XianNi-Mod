using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace xn.world
{
    public static class AuraChunkSystem
    {
        public const int CHUNK_SIZE = 8;
        public const string SAVE_KEY = "xn.aura.chunks.v2";
        public const string KEY_CITY_AURA = "xn.city.aura.pool";
        public const string KEY_KINGDOM_AURA = "xn.kingdom.aura";

        private const int CEILING_MOUNTAINS = 120000;
        private const int CEILING_FOREST = 80000;
        private const int CEILING_GRASS = 40000;
        private const int CEILING_GROUND = 20000;
        private const int CEILING_OCEAN = 0;

        private const int BASE_MOUNTAINS = 50;
        private const int BASE_FOREST = 30;
        private const int BASE_GRASS = 15;
        private const int BASE_GROUND = 8;
        private const int BASE_OCEAN = 0;

        private static readonly string[] RealmIds = new[]
        {
            "realm_01_qi",
            "realm_02_foundation",
            "realm_03_core",
            "realm_04_nascent",
            "realm_05_deity",
            "realm_06_infantchg",
            "realm_07_wending",
            "realm_08_kuinie",
            "realm_09_jingnie",
            "realm_10_suinie",
            "realm_11_kongnie",
            "realm_12_kongling",
            "realm_13_kongxuan",
            "realm_14_gtianzun",
            "realm_15_half_tatian",
            "realm_16_tatian"
        };

        private static int[] _chunks;
        private static int _gridW;
        private static int _gridH;
        private static int _lastGenYear = -1;

        public static int GridWidth
        {
            get
            {
                EnsureInitialized();
                return _gridW;
            }
        }

        public static int GridHeight
        {
            get
            {
                EnsureInitialized();
                return _gridH;
            }
        }

        public static void EnsureInitialized()
        {
            int width = MapBox.width;
            int height = MapBox.height;
            if (World.world == null || width <= 0 || height <= 0)
            {
                return;
            }

            int expectedW = Mathf.CeilToInt((float)width / CHUNK_SIZE);
            int expectedH = Mathf.CeilToInt((float)height / CHUNK_SIZE);
            if (_chunks != null && _gridW == expectedW && _gridH == expectedH)
            {
                return;
            }

            AllocateAndSeed();
        }

        private static void AllocateAndSeed()
        {
            int width = MapBox.width;
            int height = MapBox.height;
            if (World.world == null || width <= 0 || height <= 0)
            {
                _chunks = null;
                _gridW = 0;
                _gridH = 0;
                _lastGenYear = -1;
                return;
            }

            _gridW = Mathf.CeilToInt((float)width / CHUNK_SIZE);
            _gridH = Mathf.CeilToInt((float)height / CHUNK_SIZE);
            _chunks = new int[_gridW * _gridH];
            _lastGenYear = -1;

            for (int cy = 0; cy < _gridH; cy++)
            {
                for (int cx = 0; cx < _gridW; cx++)
                {
                    _chunks[GetIndex(cx, cy)] = Mathf.Clamp(SeedChunkFromTerrain(cx, cy), 0, GetChunkCeiling(cx, cy));
                }
            }
        }

        private static int SeedChunkFromTerrain(int cx, int cy)
        {
            return GetTerrainBase(cx, cy);
        }

        public static int GetChunkAura(int cx, int cy)
        {
            EnsureInitialized();
            if (!IsValidChunk(cx, cy))
            {
                return 0;
            }

            return _chunks[GetIndex(cx, cy)];
        }

        public static void SetChunkAura(int cx, int cy, int value)
        {
            EnsureInitialized();
            if (!IsValidChunk(cx, cy))
            {
                return;
            }

            _chunks[GetIndex(cx, cy)] = ClampAuraForChunk(cx, cy, value);
        }

        public static void AddChunkAura(int cx, int cy, int delta)
        {
            EnsureInitialized();
            if (!IsValidChunk(cx, cy) || delta == 0)
            {
                return;
            }

            SetChunkAura(cx, cy, _chunks[GetIndex(cx, cy)] + delta);
        }

        public static void DeductChunkAura(int cx, int cy, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            AddChunkAura(cx, cy, -amount);
        }

        public static (int cx, int cy) TileToChunk(int tx, int ty)
        {
            return (tx / CHUNK_SIZE, ty / CHUNK_SIZE);
        }

        public static WorldTile GetChunkCenterTile(int cx, int cy)
        {
            if (World.world == null || MapBox.width <= 0 || MapBox.height <= 0)
            {
                return null;
            }

            int x = Mathf.Clamp(cx * CHUNK_SIZE + CHUNK_SIZE / 2, 0, MapBox.width - 1);
            int y = Mathf.Clamp(cy * CHUNK_SIZE + CHUNK_SIZE / 2, 0, MapBox.height - 1);
            return World.world.GetTile(x, y);
        }

        public static int GetAuraForTile(WorldTile tile)
        {
            if (tile == null)
            {
                return 0;
            }

            var chunk = TileToChunk(tile.x, tile.y);
            return GetChunkAura(chunk.cx, chunk.cy);
        }

        public static int GetAuraForActor(Actor actor)
        {
            return actor != null ? GetAuraForTile(actor.current_tile) : 0;
        }

        public static int GetEffectiveAura(Actor actor)
        {
            if (actor == null)
            {
                return 0;
            }

            WorldTile tile = actor.current_tile;
            if (tile == null)
            {
                return 0;
            }

            int chunkAura = GetAuraForTile(tile);
            int kingdomBonus = 0;
            Kingdom kingdom = actor.kingdom;
            if (kingdom != null && !kingdom.isRekt())
            {
                int pool = GetKingdomPool(kingdom);
                if (pool > 0)
                {
                    int sapients = Math.Max(1, kingdom.getPopulationPeople());
                    kingdomBonus = Math.Min(pool / sapients, 2000);
                }
            }

            int cityBonus = 0;
            City city = actor.city;
            if (city != null && !city.isRekt())
            {
                int cityPool = GetCityPool(city);
                if (cityPool > 0)
                {
                    int cityPop = Math.Max(1, city.getPopulationPeople());
                    cityBonus = Math.Min(cityPool / cityPop, 1000);
                }
            }

            return chunkAura + kingdomBonus + cityBonus;
        }

        public static int GetCityPool(City city)
        {
            if (city == null || city.isRekt() || city.data == null)
            {
                return 0;
            }

            int value;
            city.data.get(KEY_CITY_AURA, out value, 0);
            return ClampCityPool(city, value);
        }

        public static void AddCityPool(City city, int delta)
        {
            if (city == null || city.isRekt() || city.data == null || delta == 0)
            {
                return;
            }

            long next = (long)GetCityPool(city) + delta;
            city.data.set(KEY_CITY_AURA, ClampCityPool(city, next));
        }

        public static void DeductCityPool(City city, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            AddCityPool(city, -amount);
        }

        public static int GetCityPoolCap(City city)
        {
            if (city == null || city.isRekt())
            {
                return 0;
            }

            return Mathf.Max(0, xn.config.ModConfigHooks.MaxCityAura);
        }

        public static int GetKingdomPool(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.isRekt() || kingdom.data == null)
            {
                return 0;
            }

            int value;
            kingdom.data.get(KEY_KINGDOM_AURA, out value, 0);
            return ClampKingdomPool(kingdom, value);
        }

        public static void AddKingdomPool(Kingdom kingdom, int delta)
        {
            if (kingdom == null || kingdom.isRekt() || kingdom.data == null || delta == 0)
            {
                return;
            }

            long next = (long)GetKingdomPool(kingdom) + delta;
            kingdom.data.set(KEY_KINGDOM_AURA, ClampKingdomPool(kingdom, next));
        }

        public static void DeductKingdomPool(Kingdom kingdom, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            AddKingdomPool(kingdom, -amount);
        }

        public static void DeductAuraAtTile(WorldTile tile, int amount)
        {
            if (tile == null || amount <= 0)
            {
                return;
            }

            var chunk = TileToChunk(tile.x, tile.y);
            DeductChunkAura(chunk.cx, chunk.cy, amount);
        }

        public static int GetChunkCeiling(int cx, int cy)
        {
            WorldTile tile = GetChunkCenterTile(cx, cy);
            TileTypeBase type = tile != null ? tile.Type : null;
            if (type == null)
            {
                return 0;
            }

            if (type.liquid || type.ocean)
            {
                return CEILING_OCEAN;
            }
            if (type.mountains)
            {
                return CEILING_MOUNTAINS;
            }
            if (IsForestOrJungle(type, tile.getBiome()))
            {
                return CEILING_FOREST;
            }
            if (type.grass)
            {
                return CEILING_GRASS;
            }
            if (type.ground)
            {
                return CEILING_GROUND;
            }

            return 0;
        }

        public static int GetChunkLimit(int cx, int cy)
        {
            int ceiling = GetChunkCeiling(cx, cy);
            int xiuzhenMax = GetXiuzhenAuraMax(cx, cy);
            if (xiuzhenMax < ceiling)
            {
                ceiling = xiuzhenMax;
            }

            return Mathf.Max(0, ceiling);
        }

        public static void TickAllChunks(int year)
        {
            EnsureInitialized();
            if (_chunks == null || year <= 0)
            {
                return;
            }

            HashSet<City> kingdomPoolCities = new HashSet<City>();
            for (int cy = 0; cy < _gridH; cy++)
            {
                for (int cx = 0; cx < _gridW; cx++)
                {
                    int gain = ComputeChunkGeneration(cx, cy, year, kingdomPoolCities);
                    AddChunkAura(cx, cy, gain);
                }
            }
        }

        public static int SumAuraForKingdom(Kingdom kingdom)
        {
            EnsureInitialized();
            if (kingdom == null || kingdom.isRekt() || _chunks == null)
            {
                return 0;
            }

            long total = 0L;
            for (int cy = 0; cy < _gridH; cy++)
            {
                for (int cx = 0; cx < _gridW; cx++)
                {
                    City city = GetChunkCity(cx, cy);
                    if (city == null || city.isRekt())
                    {
                        continue;
                    }

                    Kingdom chunkKingdom = xn.access.CityAccess.GetKingdom(city);
                    if (chunkKingdom == kingdom)
                    {
                        total += GetChunkAura(cx, cy);
                        if (total >= int.MaxValue)
                        {
                            return int.MaxValue;
                        }
                    }
                }
            }

            return (int)total;
        }

        private static int ComputeChunkGeneration(int cx, int cy, int year, HashSet<City> kingdomPoolCities)
        {
            int terrainBase = GetTerrainBase(cx, cy);
            if (GetChunkCeiling(cx, cy) <= 0)
            {
                return terrainBase;
            }

            float phase = (year % 60) / 60f;
            float cycleMultiplier = 0.15f + 0.85f * Mathf.Sin(phase * Mathf.PI);

            int vegetationScore = ComputeVegetationScore(cx, cy);
            int wildlifeScore = ComputeWildlifeScore(cx, cy);
            int livingScore = ComputeCityLivingScore(cx, cy, cycleMultiplier, kingdomPoolCities);
            int livingTotal = (int)((vegetationScore + wildlifeScore + livingScore) * cycleMultiplier);
            return terrainBase + livingTotal;
        }

        private static int GetTerrainBase(int cx, int cy)
        {
            WorldTile tile = GetChunkCenterTile(cx, cy);
            TileTypeBase type = tile != null ? tile.Type : null;
            if (type == null)
            {
                return 0;
            }

            if (type.liquid || type.ocean)
            {
                return BASE_OCEAN;
            }
            if (type.mountains)
            {
                return BASE_MOUNTAINS;
            }
            if (IsForestOrJungle(type, tile.getBiome()))
            {
                return BASE_FOREST;
            }
            if (type.grass)
            {
                return BASE_GRASS;
            }
            if (type.ground)
            {
                return BASE_GROUND;
            }

            return 0;
        }

        private static int ComputeVegetationScore(int cx, int cy)
        {
            if (World.world == null)
            {
                return 0;
            }

            int score = 0;
            int startX = cx * CHUNK_SIZE;
            int startY = cy * CHUNK_SIZE;
            int endX = Mathf.Min(startX + CHUNK_SIZE, MapBox.width);
            int endY = Mathf.Min(startY + CHUNK_SIZE, MapBox.height);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    WorldTile tile = World.world.GetTile(x, y);
                    if (tile == null || tile.Type == null)
                    {
                        continue;
                    }

                    BuildingAsset buildingAsset = xn.access.BuildingAccess.GetAsset(tile.building);
                    bool hasVegetationBuilding =
                        buildingAsset != null &&
                        (buildingAsset.is_vegetation ||
                         buildingAsset.flora ||
                         buildingAsset.building_type == BuildingType.Building_Tree ||
                         buildingAsset.building_type == BuildingType.Building_Plant ||
                         buildingAsset.building_type == BuildingType.Building_Fruits);

                    bool hasVegetatedSurface =
                        tile.top_type != null &&
                        (tile.Type.grass || tile.Type.life || tile.Type.is_biome);

                    bool isRecentlyBurned = tile.burned_stages > 0;
                    int tileScore = 0;
                    if (hasVegetationBuilding && hasVegetatedSurface && !isRecentlyBurned)
                    {
                        tileScore = 3;
                    }
                    else if (hasVegetatedSurface)
                    {
                        tileScore = 1;
                    }
                    if (isRecentlyBurned)
                    {
                        tileScore -= 2;
                    }

                    score += tileScore;
                }
            }

            return Mathf.Max(0, score);
        }

        private static int ComputeWildlifeScore(int cx, int cy)
        {
            WorldTile centerTile = GetChunkCenterTile(cx, cy);
            if (centerTile == null)
            {
                return 0;
            }

            int animalCount = 0;
            var actors = Finder.getUnitsFromChunk(centerTile, 1, CHUNK_SIZE / 2f, false);
            if (actors == null)
            {
                return 0;
            }

            foreach (Actor actor in actors)
            {
                if (actor != null && actor.isAlive() && actor.isAnimal())
                {
                    animalCount++;
                }
            }

            return animalCount * 3;
        }

        private static int ComputeCityLivingScore(int cx, int cy, float cycleMultiplier, HashSet<City> kingdomPoolCities)
        {
            City chunkCity = GetChunkCity(cx, cy);
            if (chunkCity == null || chunkCity.isRekt())
            {
                return 0;
            }

            Kingdom kingdom = xn.access.CityAccess.GetKingdom(chunkCity);
            if (kingdom == null || kingdom.isRekt() || kingdom.units == null)
            {
                return 0;
            }

            int livingScore = 0;
            int kingdomGain = 0;
            int cityGain = 0;
            bool creditKingdomPool = kingdomPoolCities != null && kingdomPoolCities.Add(chunkCity);
            foreach (Actor actor in kingdom.units)
            {
                if (actor == null || !actor.isAlive() || actor.city != chunkCity)
                {
                    continue;
                }

                ActorData data = xn.access.ActorAccess.GetData(actor);
                long xiuwei = 0L;
                if (data != null)
                {
                    data.get("xn.stat.xiuwei", out xiuwei, 0L);
                }

                bool sapient = actor.isSapient();
                int contribution = 0;
                if (xiuwei <= 0)
                {
                    contribution = sapient ? 2 : 0;
                }
                else
                {
                    contribution = GetCultivatorGeneration(actor);
                }

                livingScore += contribution;
                cityGain += contribution;
                if (creditKingdomPool && sapient)
                {
                    kingdomGain += contribution;
                }
            }

            if (creditKingdomPool && kingdomGain > 0)
            {
                int cycledGain = (int)(kingdomGain * cycleMultiplier);
                AddKingdomPool(kingdom, cycledGain);
            }
            if (cityGain > 0)
            {
                int cycledCityGain = (int)(cityGain * cycleMultiplier);
                AddCityPool(chunkCity, cycledCityGain);
            }

            return livingScore;
        }

        private static int GetCultivatorGeneration(Actor actor)
        {
            int realm = GetRealmIndex(actor);
            if (realm < 0) return 5;
            if (realm <= 2) return 20;
            if (realm <= 4) return 70;
            if (realm <= 6) return 250;
            if (realm <= 8) return 800;
            return 3000;
        }

        private static int GetRealmIndex(Actor actor)
        {
            if (actor == null)
            {
                return -1;
            }

            ActorData data = xn.access.ActorAccess.GetData(actor);
            int realm = -1;
            if (data != null)
            {
                data.get("xn.stat.realm", out realm, -1);
            }
            if (realm >= 0)
            {
                return realm;
            }

            int current = -1;
            var traits = actor.getTraits();
            if (traits == null)
            {
                return current;
            }

            for (int i = 0; i < RealmIds.Length; i++)
            {
                foreach (ActorTrait trait in traits)
                {
                    if (trait != null && trait.id == RealmIds[i] && i > current)
                    {
                        current = i;
                    }
                }
            }

            return current;
        }

        private static City GetChunkCity(int cx, int cy)
        {
            WorldTile centerTile = GetChunkCenterTile(cx, cy);
            return centerTile != null && centerTile.zone != null ? centerTile.zone.city : null;
        }

        private static int ClampAuraForChunk(int cx, int cy, int value)
        {
            return Mathf.Clamp(value, 0, GetChunkLimit(cx, cy));
        }

        private static int ClampKingdomPool(Kingdom kingdom, long value)
        {
            int cap = GetKingdomPoolCap(kingdom);
            if (cap <= 0 || value <= 0L)
            {
                return 0;
            }
            if (value >= cap)
            {
                return cap;
            }

            return (int)value;
        }

        private static int ClampCityPool(City city, long value)
        {
            int cap = GetCityPoolCap(city);
            if (cap <= 0 || value <= 0L)
            {
                return 0;
            }
            if (value >= cap)
            {
                return cap;
            }

            return (int)value;
        }

        private static int GetKingdomPoolCap(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.isRekt())
            {
                return 0;
            }

            int xiuzhenLevel = xn.world.XiuzhenguoSystem.GetLevel(kingdom);
            if (xiuzhenLevel > 0)
            {
                return xn.world.XiuzhenguoSystem.GetMaxAura(kingdom);
            }

            return Mathf.Max(0, xn.config.ModConfigHooks.MaxKingdomAura);
        }

        private static int GetXiuzhenAuraMax(int cx, int cy)
        {
            if (!xn.config.ModConfigHooks.EnableXiuzhenguoAuraLimit)
            {
                return int.MaxValue;
            }

            City zoneCity = GetChunkCity(cx, cy);
            if (zoneCity == null || zoneCity.isRekt())
            {
                return int.MaxValue;
            }

            Kingdom kingdom = xn.access.CityAccess.GetKingdom(zoneCity);
            if (kingdom == null || kingdom.isRekt())
            {
                return int.MaxValue;
            }

            return xn.world.XiuzhenguoSystem.GetMaxAura(kingdom);
        }

        private static bool IsForestOrJungle(TileTypeBase type, BiomeAsset biome)
        {
            if (type == null)
            {
                return false;
            }
            if (type.life)
            {
                return true;
            }

            string biomeId = biome != null ? biome.id : type.biome_id;
            if (string.IsNullOrEmpty(biomeId))
            {
                return false;
            }

            biomeId = biomeId.ToLowerInvariant();
            return biomeId.Contains("forest") || biomeId.Contains("jungle");
        }

        private static bool IsValidChunk(int cx, int cy)
        {
            return _chunks != null && cx >= 0 && cy >= 0 && cx < _gridW && cy < _gridH;
        }

        private static int GetIndex(int cx, int cy)
        {
            return cy * _gridW + cx;
        }

        private static string Serialize()
        {
            EnsureInitialized();
            if (_chunks == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("v2;");
            sb.Append(CHUNK_SIZE);
            sb.Append(';');
            sb.Append(_gridW);
            sb.Append(';');
            sb.Append(_gridH);
            sb.Append(';');
            for (int i = 0; i < _chunks.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }
                sb.Append(_chunks[i]);
            }

            return sb.ToString();
        }

        private static bool TryDeserialize(string saved)
        {
            if (string.IsNullOrEmpty(saved))
            {
                return false;
            }

            string[] parts = saved.Split(new[] { ';' }, 5);
            if (parts.Length != 5 || parts[0] != "v2")
            {
                return false;
            }

            int chunkSize;
            int savedW;
            int savedH;
            if (!int.TryParse(parts[1], out chunkSize) ||
                !int.TryParse(parts[2], out savedW) ||
                !int.TryParse(parts[3], out savedH) ||
                chunkSize != CHUNK_SIZE)
            {
                return false;
            }

            int expectedW = Mathf.CeilToInt((float)MapBox.width / CHUNK_SIZE);
            int expectedH = Mathf.CeilToInt((float)MapBox.height / CHUNK_SIZE);
            if (savedW != expectedW || savedH != expectedH || savedW <= 0 || savedH <= 0)
            {
                return false;
            }

            string[] values = parts[4].Split(',');
            if (values.Length != savedW * savedH)
            {
                return false;
            }

            int[] parsed = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                int value;
                if (!int.TryParse(values[i], out value))
                {
                    return false;
                }
                parsed[i] = value;
            }

            _gridW = savedW;
            _gridH = savedH;
            _chunks = parsed;
            _lastGenYear = -1;
            for (int cy = 0; cy < _gridH; cy++)
            {
                for (int cx = 0; cx < _gridW; cx++)
                {
                    int index = GetIndex(cx, cy);
                    _chunks[index] = ClampAuraForChunk(cx, cy, _chunks[index]);
                }
            }

            return true;
        }

        private static void SaveToWorldData()
        {
            var customData = xn.access.MapBoxAccess.EnsureCustomData(World.world);
            if (customData == null)
            {
                return;
            }

            customData.set(SAVE_KEY, Serialize());
        }

        private static void LoadFromWorldData()
        {
            var customData = xn.access.MapBoxAccess.EnsureCustomData(World.world);
            string saved = "";
            if (customData != null)
            {
                customData.get(SAVE_KEY, out saved, "");
            }

            if (!TryDeserialize(saved))
            {
                AllocateAndSeed();
            }
        }

        private static void OnSimulationYear()
        {
            int year = Date.getCurrentYear();
            if (year <= 0 || year == _lastGenYear)
            {
                return;
            }

            _lastGenYear = year;
            TickAllChunks(year);
        }

        [HarmonyPatch(typeof(MapBox), "generateNewMap")]
        private static class Patch_MapBox_GenerateNewMap
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                AllocateAndSeed();
            }
        }

        [HarmonyPatch(typeof(SaveManager), "loadWorld", typeof(string), typeof(bool))]
        private static class Patch_SaveManager_LoadWorld
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                LoadFromWorldData();
            }
        }

        [HarmonyPatch(typeof(SaveManager), "saveMapData", typeof(string), typeof(bool))]
        private static class Patch_SaveManager_SaveMapData
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                SaveToWorldData();
            }
        }

        [HarmonyPatch(typeof(MapBox), "updateSimulation")]
        private static class Patch_MapBox_UpdateSimulation
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                OnSimulationYear();
            }
        }
    }
}
