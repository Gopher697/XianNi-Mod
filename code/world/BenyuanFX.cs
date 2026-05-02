using System.Collections.Generic;
namespace xn.world
{
    internal static class BenyuanFX
    {
        private static readonly HashSet<long> s_played = new(256);
        public static void Init(HarmonyLib.Harmony _) => fx.XNStatusFX.Register();
        public static void PlayOnce(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            long id = xn.access.ActorAccess.GetData(a).id;
            if (s_played.Contains(id)) return;
            fx.XNStatusFX.PlayBenyuanOpen(a);
            s_played.Add(id);
        }
        public static void ResetPlayed(Actor a)
        {
            if (a == null) return;
            s_played.Remove(xn.access.ActorAccess.GetData(a).id);
        }
        public static void ClearAll() => s_played.Clear();
    }
}