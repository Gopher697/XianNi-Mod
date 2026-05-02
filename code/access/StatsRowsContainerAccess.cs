using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class StatsRowsContainerAccess
    {
        private static readonly MethodInfo GetStatRowMethod = AccessTools.Method(typeof(StatsRowsContainer), "getStatRow", new[] { typeof(string) });
        private static bool _warnedGetStatRow;

        public static KeyValueField GetStatRow(StatsRowsContainer container, string key)
        {
            if (container == null || string.IsNullOrEmpty(key)) return null;
            if (GetStatRowMethod == null)
            {
                WarnOnce(ref _warnedGetStatRow, "[XN] StatsRowsContainer.getStatRow method not found; stat row lookup failed.");
                return null;
            }
            return GetStatRowMethod.Invoke(container, new object[] { key }) as KeyValueField;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
