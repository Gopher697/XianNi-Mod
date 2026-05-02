using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class ProjectileAccess
    {
        private static readonly FieldInfo CurrentPosition3DField = AccessTools.Field(typeof(Projectile), "_current_position_3d");
        private static readonly FieldInfo SpeedField = AccessTools.Field(typeof(Projectile), "_speed");
        private static readonly FieldInfo ByWhoField = AccessTools.Field(typeof(Projectile), "by_who");
        private static readonly FieldInfo KingdomField = AccessTools.Field(typeof(Projectile), "kingdom");
        private static readonly MethodInfo GetCurrentTilePositionMethod = AccessTools.Method(typeof(Projectile), "getCurrentTilePosition");
        private static bool _warnedByWho;
        private static bool _warnedKingdom;
        private static bool _warnedCurrentTilePosition;

        public static Vector3 GetCurrentPosition3D(Projectile projectile)
        {
            if (projectile == null) return Vector3.zero;
            if (CurrentPosition3DField == null)
            {
                Debug.LogWarning("[XN] Projectile._current_position_3d field not found; using current tile position.");
                WorldTile tile = GetCurrentTilePosition(projectile);
                return tile != null ? new Vector3(tile.x, tile.y, 0f) : Vector3.zero;
            }
            return CurrentPosition3DField.GetValue(projectile) is Vector3 value ? value : Vector3.zero;
        }

        public static void MultiplySpeed(Projectile projectile, float multiplier)
        {
            if (projectile == null) return;
            if (SpeedField == null)
            {
                Debug.LogWarning("[XN] Projectile._speed field not found; speed was not changed.");
                return;
            }
            object raw = SpeedField.GetValue(projectile);
            if (raw is float speed)
            {
                SpeedField.SetValue(projectile, speed * multiplier);
            }
        }

        public static BaseSimObject GetByWho(Projectile projectile)
        {
            if (projectile == null) return null;
            if (ByWhoField == null)
            {
                WarnOnce(ref _warnedByWho, "[XN] Projectile.by_who field not found; projectile owner lookup failed.");
                return null;
            }
            return ByWhoField.GetValue(projectile) as BaseSimObject;
        }

        public static Kingdom GetKingdom(Projectile projectile)
        {
            if (projectile == null) return null;
            if (KingdomField == null)
            {
                WarnOnce(ref _warnedKingdom, "[XN] Projectile.kingdom field not found; projectile kingdom lookup failed.");
                return null;
            }
            return KingdomField.GetValue(projectile) as Kingdom;
        }

        public static WorldTile GetCurrentTilePosition(Projectile projectile)
        {
            if (projectile == null) return null;
            if (GetCurrentTilePositionMethod == null)
            {
                WarnOnce(ref _warnedCurrentTilePosition, "[XN] Projectile.getCurrentTilePosition method not found; projectile tile lookup failed.");
                return null;
            }
            return GetCurrentTilePositionMethod.Invoke(projectile, null) as WorldTile;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
