using HarmonyLib;
namespace xn.tournament
{
    [HarmonyPatch(typeof(MapBox), "Update")]
    internal static class TournamentUpdatePatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (MapBox.instance == null) return;
            if (World.world == null) return;
            if (xn.access.MapBoxAccess.IsPaused(World.world)) return;
            TournamentManager.Update();
        }
    }
}
