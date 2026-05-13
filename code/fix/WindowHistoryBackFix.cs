using System;
using HarmonyLib;
using UnityEngine;

namespace xn.fix
{
    [HarmonyPatch(typeof(WindowHistory), nameof(WindowHistory.clickBack))]
    internal static class WindowHistoryBackFix
    {
        private static bool s_warned;

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (!(__exception is NullReferenceException))
            {
                return __exception;
            }
            if (!s_warned)
            {
                s_warned = true;
                Debug.LogWarning("[XN-Fix] Suppressed null window history back action.");
            }
            return null;
        }
    }
}
