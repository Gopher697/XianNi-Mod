using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class AnimationFrameDataAccess
    {
        private static readonly FieldInfo PosHeadField = AccessTools.Field(typeof(AnimationFrameData), "pos_head");
        private static bool _warnedPosHead;

        public static Vector2 GetPosHead(AnimationFrameData frameData)
        {
            if (frameData == null) return Vector2.zero;
            if (PosHeadField == null)
            {
                WarnOnce(ref _warnedPosHead, "[XN] AnimationFrameData.pos_head field not found; using zero head offset.");
                return Vector2.zero;
            }
            return PosHeadField.GetValue(frameData) is Vector2 value ? value : Vector2.zero;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
