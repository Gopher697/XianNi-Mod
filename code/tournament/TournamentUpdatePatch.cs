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
            if (World.world.isPaused()) return;
            TournamentManager.Update();
        }
    }
}