using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class BaseSimObjectAccess
    {
        private static readonly FieldInfo ActorField = AccessTools.Field(typeof(BaseSimObject), "a");
        private static readonly FieldInfo BuildingField = AccessTools.Field(typeof(BaseSimObject), "b");
        private static readonly FieldInfo StatsField = AccessTools.Field(typeof(BaseSimObject), "stats");
        private static readonly FieldInfo CurrentTransformPositionField = AccessTools.Field(typeof(BaseSimObject), "cur_transform_position");
        private static readonly MethodInfo IsActorMethod = AccessTools.Method(typeof(BaseSimObject), "isActor");
        private static readonly MethodInfo IsBuildingMethod = AccessTools.Method(typeof(BaseSimObject), "isBuilding");
        private static readonly MethodInfo GetHeightMethod = AccessTools.Method(typeof(BaseSimObject), "getHeight");
        private static readonly MethodInfo HasStatusMethod = AccessTools.Method(typeof(BaseSimObject), "hasStatus", new[] { typeof(string) });
        private static readonly MethodInfo IsInLiquidMethod = AccessTools.Method(typeof(BaseSimObject), "isInLiquid");
        private static bool _warnedActor;
        private static bool _warnedBuilding;
        private static bool _warnedStats;
        private static bool _warnedCurrentTransformPosition;
        private static bool _warnedIsActor;
        private static bool _warnedIsBuilding;
        private static bool _warnedGetHeight;
        private static bool _warnedHasStatus;
        private static bool _warnedIsInLiquid;

        public static Actor GetActor(BaseSimObject obj)
        {
            if (obj == null) return null;
            if (ActorField == null)
            {
                WarnOnce(ref _warnedActor, "[XN] BaseSimObject.a field not found; actor lookup failed.");
                return null;
            }
            return ActorField.GetValue(obj) as Actor;
        }

        public static Building GetBuilding(BaseSimObject obj)
        {
            if (obj == null) return null;
            if (BuildingField == null)
            {
                WarnOnce(ref _warnedBuilding, "[XN] BaseSimObject.b field not found; building lookup failed.");
                return null;
            }
            return BuildingField.GetValue(obj) as Building;
        }

        public static BaseStats GetStats(BaseSimObject obj)
        {
            if (obj == null) return null;
            if (StatsField == null)
            {
                WarnOnce(ref _warnedStats, "[XN] BaseSimObject.stats field not found; stats lookup failed.");
                return null;
            }
            return StatsField.GetValue(obj) as BaseStats;
        }

        public static Vector3 GetCurrentTransformPosition(BaseSimObject obj)
        {
            if (obj == null) return Vector3.zero;
            if (CurrentTransformPositionField == null)
            {
                WarnOnce(ref _warnedCurrentTransformPosition, "[XN] BaseSimObject.cur_transform_position field not found; using current_position.");
                return obj.current_position;
            }
            return CurrentTransformPositionField.GetValue(obj) is Vector3 value ? value : Vector3.zero;
        }

        public static bool IsActor(BaseSimObject obj)
        {
            if (obj == null) return false;
            if (IsActorMethod == null)
            {
                WarnOnce(ref _warnedIsActor, "[XN] BaseSimObject.isActor method not found; treating object as non-actor.");
                return false;
            }
            return IsActorMethod.Invoke(obj, null) is bool value && value;
        }

        public static bool IsBuilding(BaseSimObject obj)
        {
            if (obj == null) return false;
            if (IsBuildingMethod == null)
            {
                WarnOnce(ref _warnedIsBuilding, "[XN] BaseSimObject.isBuilding method not found; treating object as non-building.");
                return false;
            }
            return IsBuildingMethod.Invoke(obj, null) is bool value && value;
        }

        public static float GetHeight(BaseSimObject obj)
        {
            if (obj == null) return 0f;
            if (GetHeightMethod == null)
            {
                WarnOnce(ref _warnedGetHeight, "[XN] BaseSimObject.getHeight method not found; using height 0.");
                return 0f;
            }
            return GetHeightMethod.Invoke(obj, null) is float value ? value : 0f;
        }

        public static bool HasStatus(BaseSimObject obj, string id)
        {
            if (obj == null || string.IsNullOrEmpty(id)) return false;
            if (HasStatusMethod == null)
            {
                WarnOnce(ref _warnedHasStatus, "[XN] BaseSimObject.hasStatus method not found; treating status as absent.");
                return false;
            }
            return HasStatusMethod.Invoke(obj, new object[] { id }) is bool value && value;
        }

        public static bool IsInLiquid(BaseSimObject obj)
        {
            if (obj == null) return false;
            if (IsInLiquidMethod == null)
            {
                WarnOnce(ref _warnedIsInLiquid, "[XN] BaseSimObject.isInLiquid method not found; treating object as not in liquid.");
                return false;
            }
            return IsInLiquidMethod.Invoke(obj, null) is bool value && value;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
