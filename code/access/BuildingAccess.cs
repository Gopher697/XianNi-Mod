using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class BuildingAccess
    {
        private static readonly FieldInfo DataField = AccessTools.Field(typeof(Building), "data");
        private static readonly FieldInfo AssetField = AccessTools.Field(typeof(Building), "asset");
        private static readonly FieldInfo StateOwnershipField = AccessTools.Field(typeof(Building), "state_ownership");
        private static readonly MethodInfo RemoveBuildingFinalMethod = AccessTools.Method(typeof(Building), "removeBuildingFinal");
        private static bool _warnedData;
        private static bool _warnedAsset;
        private static bool _warnedStateOwnership;
        private static bool _warnedRemoveBuildingFinal;

        public static BuildingData GetData(Building building)
        {
            if (building == null) return null;
            if (DataField == null)
            {
                WarnOnce(ref _warnedData, "[XN] Building.data field not found; building data lookup failed.");
                return null;
            }
            return DataField.GetValue(building) as BuildingData;
        }

        public static BuildingAsset GetAsset(Building building)
        {
            if (building == null) return null;
            if (AssetField == null)
            {
                WarnOnce(ref _warnedAsset, "[XN] Building.asset field not found; building asset lookup failed.");
                return null;
            }
            return AssetField.GetValue(building) as BuildingAsset;
        }

        public static BuildingOwnershipState GetStateOwnership(Building building)
        {
            if (building == null) return default(BuildingOwnershipState);
            if (StateOwnershipField == null)
            {
                WarnOnce(ref _warnedStateOwnership, "[XN] Building.state_ownership field not found; using default ownership state.");
                return default(BuildingOwnershipState);
            }
            return StateOwnershipField.GetValue(building) is BuildingOwnershipState value ? value : default(BuildingOwnershipState);
        }

        public static void RemoveBuildingFinal(Building building)
        {
            if (building == null) return;
            if (RemoveBuildingFinalMethod == null)
            {
                WarnOnce(ref _warnedRemoveBuildingFinal, "[XN] Building.removeBuildingFinal method not found; building was not removed.");
                return;
            }
            RemoveBuildingFinalMethod.Invoke(building, null);
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
