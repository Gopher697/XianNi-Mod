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
        private static readonly MethodInfo CanBuildFromMethod = AccessTools.Method(
            typeof(BuildingManager),
            "canBuildFrom",
            new[] { typeof(WorldTile), typeof(BuildingAsset), typeof(City), typeof(BuildPlacingType), typeof(bool) });

        private static bool _warnedAddBuilding;
        private static bool _warnedCanBuildFrom;

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

        public static bool CanBuildFrom(BuildingManager manager, WorldTile tile, BuildingAsset asset, City city, BuildPlacingType type, bool floraGrowth = false)
        {
            if (manager == null || tile == null || asset == null) return false;
            if (CanBuildFromMethod == null)
            {
                WarnOnce(ref _warnedCanBuildFrom, "[XN] BuildingManager.canBuildFrom method not found; treating tile as invalid for building placement.");
                return false;
            }
            return CanBuildFromMethod.Invoke(manager, new object[] { tile, asset, city, type, floraGrowth }) is bool value && value;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
