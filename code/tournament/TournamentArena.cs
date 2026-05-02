using System.Collections.Generic;
using UnityEngine;
namespace xn.tournament
{
    public static class TournamentArena
    {
        private const int ARENA_RADIUS = 7;
        private static HashSet<WorldTile> _arenaTiles = new HashSet<WorldTile>();
        private static bool _allowTerrainChange = false; 
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        internal static bool IsTerrainChangeAllowed()
        {
            return _allowTerrainChange;
        }
        public static WorldTile FindOceanTileForArena()
        {
            if (World.world == null) return null;
            var deepOcean = TileLibrary.deep_ocean;
            if (deepOcean != null && deepOcean.hashset != null && deepOcean.hashset.Count > 0)
            {
                var candidates = new List<WorldTile>(deepOcean.hashset);
                ShuffleList(candidates);
                foreach (var tile in candidates)
                {
                    if (tile == null) continue;
                    if (IsValidArenaLocation(tile)) return tile;
                }
            }
            var closeOcean = TileLibrary.close_ocean;
            if (closeOcean != null && closeOcean.hashset != null && closeOcean.hashset.Count > 0)
            {
                var candidates = new List<WorldTile>(closeOcean.hashset);
                ShuffleList(candidates);
                foreach (var tile in candidates)
                {
                    if (tile == null) continue;
                    if (IsValidArenaLocation(tile)) return tile;
                }
            }
            return null;
        }
        private static bool IsValidArenaLocation(WorldTile center)
        {
            if (center == null) return false;
            int validCount = 0;
            for (int dx = -ARENA_RADIUS; dx <= ARENA_RADIUS; dx++)
            {
                for (int dy = -ARENA_RADIUS; dy <= ARENA_RADIUS; dy++)
                {
                    var tile = World.world.GetTile(center.x + dx, center.y + dy);
                    if (tile != null && tile.Type != null && tile.Type.ocean)
                        validCount++;
                }
            }
            int totalTiles = (ARENA_RADIUS * 2 + 1) * (ARENA_RADIUS * 2 + 1);
            return validCount >= totalTiles * 0.8f;
        }
        public static void BuildArena(WorldTile center)
        {
            if (center == null) return;
            _arenaTiles.Clear();
            _allowTerrainChange = true; 
            for (int dx = -ARENA_RADIUS; dx <= ARENA_RADIUS; dx++)
            {
                for (int dy = -ARENA_RADIUS; dy <= ARENA_RADIUS; dy++)
                {
                    if (dx * dx + dy * dy > ARENA_RADIUS * ARENA_RADIUS) continue;
                    var tile = World.world.GetTile(center.x + dx, center.y + dy);
                    if (tile == null) continue;
                    MapAction.terraformMain(tile, TileLibrary.soil_high, TerraformLibrary.flash);
                    _arenaTiles.Add(tile);
                }
            }
            _allowTerrainChange = false; 
            xn.world.BroadcastSystem.PostAtTile(center, T("broadcast_tournament_arena_built", "Tournament arena constructed!"));
        }
        public static bool IsArenaReady(WorldTile center)
        {
            if (center == null) return false;
            return _arenaTiles.Count > 0;
        }
        public static List<WorldTile> GetArenaTiles(WorldTile center)
        {
            if (center == null) return null;
            var result = new List<WorldTile>(_arenaTiles.Count);
            foreach (var tile in _arenaTiles)
            {
                if (tile != null) result.Add(tile);
            }
            return result;
        }
        public static bool IsInArena(WorldTile tile)
        {
            if (tile == null) return false;
            return _arenaTiles.Contains(tile);
        }
        public static void CleanupArena(WorldTile center)
        {
            if (center == null) return;
            _allowTerrainChange = true; 
            foreach (var tile in _arenaTiles)
            {
                if (tile == null) continue;
                MapAction.terraformMain(tile, TileLibrary.deep_ocean, TerraformLibrary.flash);
            }
            _allowTerrainChange = false; 
            _arenaTiles.Clear();
        }
        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
