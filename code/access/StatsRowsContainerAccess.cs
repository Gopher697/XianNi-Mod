using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class StatsRowsContainerAccess
    {
        private static readonly MethodInfo StatsRowsContainerGetter = AccessTools.PropertyGetter(typeof(StatsWindow), "stats_rows_container");
        private static readonly FieldInfo StatsRowsContainerField = AccessTools.Field(typeof(StatsWindow), "_stats_rows_container");
        private static readonly MethodInfo GetStatRowMethod = AccessTools.Method(typeof(StatsRowsContainer), "getStatRow", new[] { typeof(string) });
        private static bool _warnedStatsRowsContainer;
        private static bool _warnedGetStatRow;

        public static StatsRowsContainer GetStatsRowsContainer(StatsWindow window)
        {
            if (window == null) return null;
            if (StatsRowsContainerGetter != null)
            {
                return StatsRowsContainerGetter.Invoke(window, null) as StatsRowsContainer;
            }
            if (StatsRowsContainerField != null)
            {
                return StatsRowsContainerField.GetValue(window) as StatsRowsContainer;
            }
            WarnOnce(ref _warnedStatsRowsContainer, "[XN] StatsWindow.stats_rows_container member not found; stats rows container lookup failed.");
            return null;
        }

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
