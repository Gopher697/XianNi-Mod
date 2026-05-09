using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class BuildingManagerAccess
    {
        private static readonly MethodInfo AddBuildingMethod = AccessTools.Method(
            typeof(BuildingManager),
            "addBuilding",
            new[] { typeof(string), typeof(WorldTile), typeof(bool), typeof(bool), typeof(BuildPlacingType) });

        private static bool _warnedAddBuilding;

        public static Building AddBuilding(BuildingManager manager, string id, WorldTile tile, bool checkForBuild, bool sfx, BuildPlacingType type)
        {
            if (manager == null || string.IsNullOrEmpty(id) || tile == null) return null;
            if (AddBuildingMethod == null)
            {
                WarnOnce(ref _warnedAddBuilding, "[XN] BuildingManager.addBuilding method not found; building was not placed.");
                return null;
            }
            return AddBuildingMethod.Invoke(manager, new object[] { id, tile, checkForBuild, sfx, type }) as Building;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
