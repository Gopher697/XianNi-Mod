using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.race
{
    internal static class PowerLibraryAccess
    {
        private static readonly MethodInfo SpawnUnitMethod = AccessTools.Method(
            typeof(PowerLibrary),
            "spawnUnit",
            new[] { typeof(WorldTile), typeof(string) });

        public static bool SpawnUnit(WorldTile tile, string unitId)
        {
            if (tile == null || string.IsNullOrEmpty(unitId)) return false;

            if (SpawnUnitMethod == null)
            {
                Debug.LogWarning("[XN] PowerLibrary.spawnUnit method not found; unit spawn power cannot run.");
                return false;
            }

            object result = SpawnUnitMethod.Invoke(AssetManager.powers, new object[] { tile, unitId });
            return result is bool success ? success : true;
        }
    }
}
