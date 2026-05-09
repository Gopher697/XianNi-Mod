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
        private static readonly MethodInfo AddStatusEffectMethod = AccessTools.Method(typeof(BaseSimObject), "addStatusEffect", new[] { typeof(string), typeof(float), typeof(bool) });
        private static readonly MethodInfo GetHitMethod = AccessTools.Method(typeof(BaseSimObject), "getHit", new[] { typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool) });
        private static readonly MethodInfo IsInLiquidMethod = AccessTools.Method(typeof(BaseSimObject), "isInLiquid");
        private static readonly MethodInfo CanAttackTargetMethod = AccessTools.Method(typeof(BaseSimObject), "canAttackTarget", new[] { typeof(BaseSimObject), typeof(bool), typeof(bool) });
        private static readonly MethodInfo IgnoreTargetMethod = AccessTools.Method(typeof(BaseSimObject), "ignoreTarget", new[] { typeof(BaseSimObject) });
        private static bool _warnedActor;
        private static bool _warnedBuilding;
        private static bool _warnedStats;
        private static bool _warnedCurrentTransformPosition;
        private static bool _warnedIsActor;
        private static bool _warnedIsBuilding;
        private static bool _warnedGetHeight;
        private static bool _warnedHasStatus;
        private static bool _warnedAddStatusEffect;
        private static bool _warnedGetHit;
        private static bool _warnedIsInLiquid;
        private static bool _warnedCanAttackTarget;
        private static bool _warnedIgnoreTarget;

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

        public static bool AddStatusEffect(BaseSimObject obj, string effectId, float duration = 0f, bool pColorEffect = false)
        {
            if (obj == null || string.IsNullOrEmpty(effectId)) return false;
            if (AddStatusEffectMethod == null)
            {
                WarnOnce(ref _warnedAddStatusEffect, "[XN] BaseSimObject.addStatusEffect method not found; status effect was not applied.");
                return false;
            }
            return AddStatusEffectMethod.Invoke(obj, new object[] { effectId, duration, pColorEffect }) is bool value && value;
        }

        public static void GetHit(BaseSimObject obj, float pDamage, bool pFlash = true, AttackType pAttackType = AttackType.Other, BaseSimObject pAttacker = null, bool pMetallicWeapon = false, bool pSkipIfShake = false, bool pCheckDamageReduction = true)
        {
            if (obj == null) return;
            if (GetHitMethod == null)
            {
                WarnOnce(ref _warnedGetHit, "[XN] BaseSimObject.getHit method not found; damage was not applied.");
                return;
            }
            GetHitMethod.Invoke(obj, new object[] { pDamage, pFlash, pAttackType, pAttacker, pMetallicWeapon, pSkipIfShake, pCheckDamageReduction });
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

        public static bool CanAttackTarget(BaseSimObject source, BaseSimObject target, bool pCheckForFactions = true, bool pAttackBuildings = true)
        {
            if (source == null || target == null) return false;
            if (CanAttackTargetMethod == null)
            {
                WarnOnce(ref _warnedCanAttackTarget, "[XN] BaseSimObject.canAttackTarget method not found; treating attack target as invalid.");
                return false;
            }
            return CanAttackTargetMethod.Invoke(source, new object[] { target, pCheckForFactions, pAttackBuildings }) is bool value && value;
        }

        public static void IgnoreTarget(BaseSimObject source, BaseSimObject target)
        {
            if (source == null || target == null) return;
            if (IgnoreTargetMethod == null)
            {
                WarnOnce(ref _warnedIgnoreTarget, "[XN] BaseSimObject.ignoreTarget method not found; target was not ignored.");
                return;
            }
            IgnoreTargetMethod.Invoke(source, new object[] { target });
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
