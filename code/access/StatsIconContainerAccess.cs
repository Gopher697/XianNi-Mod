using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class StatsIconContainerAccess
    {
        private static readonly FieldInfo StatsIconsField = AccessTools.Field(typeof(StatsIconContainer), "_stats_icons");

        public static Dictionary<string, StatsIcon> GetStatsIcons(StatsIconContainer container)
        {
            if (container == null) return null;
            if (StatsIconsField == null)
            {
                Debug.LogWarning("[XN] StatsIconContainer._stats_icons field not found.");
                return null;
            }
            return StatsIconsField.GetValue(container) as Dictionary<string, StatsIcon>;
        }
    }
}
