using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class HeatAccess
    {
        private static readonly MethodInfo AddTileMethod = AccessTools.Method(typeof(Heat), "addTile", new[] { typeof(WorldTile), typeof(int) });
        private static bool _warnedAddTile;

        public static void AddTile(Heat heat, WorldTile tile, int value)
        {
            if (heat == null || tile == null) return;
            if (AddTileMethod == null)
            {
                WarnOnce(ref _warnedAddTile, "[XN] Heat.addTile method not found; heat was not added.");
                return;
            }
            AddTileMethod.Invoke(heat, new object[] { tile, value });
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
