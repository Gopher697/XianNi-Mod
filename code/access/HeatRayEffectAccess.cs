using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class HeatRayEffectAccess
    {
        private static readonly MethodInfo IsReadyMethod = AccessTools.Method(typeof(HeatRayEffect), "isReady");
        private static readonly MethodInfo PlayMethod = AccessTools.Method(typeof(HeatRayEffect), "play", new[] { typeof(Vector2), typeof(int) });
        private static bool _warnedIsReady;
        private static bool _warnedPlay;

        public static bool IsReady(HeatRayEffect effect)
        {
            if (effect == null) return false;
            if (IsReadyMethod == null)
            {
                WarnOnce(ref _warnedIsReady, "[XN] HeatRayEffect.isReady method not found; treating effect as not ready.");
                return false;
            }
            return IsReadyMethod.Invoke(effect, null) is bool value && value;
        }

        public static void Play(HeatRayEffect effect, Vector2 position, int size)
        {
            if (effect == null) return;
            if (PlayMethod == null)
            {
                WarnOnce(ref _warnedPlay, "[XN] HeatRayEffect.play method not found; effect was not played.");
                return;
            }
            PlayMethod.Invoke(effect, new object[] { position, size });
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
