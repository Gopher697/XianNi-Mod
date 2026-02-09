using HarmonyLib;
namespace xn.tournament
{
    [HarmonyPatch(typeof(MapAction), "terraformMain", typeof(WorldTile), typeof(TileType), typeof(bool))]
    internal static class TournamentArenaProtectionPatch1
    {
        [HarmonyPrefix]
        private static bool Prefix(WorldTile pTile)
        {
            if (TournamentArena.IsTerrainChangeAllowed())
                return true;
            if (pTile != null && TournamentArena.IsInArena(pTile))
            {
                return false; 
            }
            return true; 
        }
    }
    [HarmonyPatch(typeof(MapAction), "terraformMain", typeof(WorldTile), typeof(TileType), typeof(TerraformOptions), typeof(bool))]
    internal static class TournamentArenaProtectionPatch2
    {
        [HarmonyPrefix]
        private static bool Prefix(WorldTile pTile)
        {
            if (TournamentArena.IsTerrainChangeAllowed())
                return true;
            if (pTile != null && TournamentArena.IsInArena(pTile))
            {
                return false; 
            }
            return true; 
        }
    }
}