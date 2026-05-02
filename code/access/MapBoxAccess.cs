using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class MapBoxAccess
    {
        private static readonly FieldInfo MapStatsField = AccessTools.Field(typeof(MapBox), "map_stats");
        private static readonly FieldInfo SelectedButtonsField = AccessTools.Field(typeof(MapBox), "selected_buttons");
        private static readonly FieldInfo SelectedButtonField = AccessTools.Field(typeof(PowerButtonSelector), "selectedButton");
        private static readonly FieldInfo OnWorldLoadedField = AccessTools.Field(typeof(MapBox), "on_world_loaded");
        private static readonly FieldInfo HeatRayFxField = AccessTools.Field(typeof(MapBox), "heat_ray_fx");
        private static readonly FieldInfo HeatField = AccessTools.Field(typeof(MapBox), "heat");
        private static readonly FieldInfo TilesListField = AccessTools.Field(typeof(MapBox), "tiles_list");
        private static readonly MethodInfo IsPausedMethod = AccessTools.Method(typeof(MapBox), "isPaused");
        private static bool _warnedMapStats;
        private static bool _warnedSelectedButtons;
        private static bool _warnedSelectedButton;
        private static bool _warnedOnWorldLoaded;
        private static bool _warnedHeatRayFx;
        private static bool _warnedHeat;
        private static bool _warnedTilesList;
        private static bool _warnedIsPaused;

        public static MapStats GetMapStats(MapBox mapBox)
        {
            if (mapBox == null) return null;
            if (MapStatsField == null)
            {
                WarnOnce(ref _warnedMapStats, "[XN] MapBox.map_stats field not found; map stats lookup failed.");
                return null;
            }
            return MapStatsField.GetValue(mapBox) as MapStats;
        }

        public static SaveCustomData GetCustomData(MapBox mapBox)
        {
            return GetMapStats(mapBox)?.custom_data;
        }

        public static SaveCustomData EnsureCustomData(MapBox mapBox)
        {
            MapStats mapStats = GetMapStats(mapBox);
            if (mapStats == null) return null;
            if (mapStats.custom_data == null)
            {
                mapStats.custom_data = new SaveCustomData();
            }
            return mapStats.custom_data;
        }

        public static bool IsPaused(MapBox mapBox)
        {
            if (mapBox == null) return true;
            if (IsPausedMethod == null)
            {
                WarnOnce(ref _warnedIsPaused, "[XN] MapBox.isPaused method not found; treating world as not paused.");
                return false;
            }
            return IsPausedMethod.Invoke(mapBox, null) is bool value && value;
        }

        public static PowerButton GetSelectedButton(MapBox mapBox)
        {
            if (mapBox == null) return null;
            if (SelectedButtonsField == null)
            {
                WarnOnce(ref _warnedSelectedButtons, "[XN] MapBox.selected_buttons field not found; selected power lookup failed.");
                return null;
            }
            if (SelectedButtonField == null)
            {
                WarnOnce(ref _warnedSelectedButton, "[XN] PowerButtonSelector.selectedButton field not found; selected power lookup failed.");
                return null;
            }
            object selector = SelectedButtonsField.GetValue(mapBox);
            if (selector == null) return null;
            return SelectedButtonField.GetValue(selector) as PowerButton;
        }

        public static HeatRayEffect GetHeatRayFx(MapBox mapBox)
        {
            if (mapBox == null) return null;
            if (HeatRayFxField == null)
            {
                WarnOnce(ref _warnedHeatRayFx, "[XN] MapBox.heat_ray_fx field not found; heat ray effect lookup failed.");
                return null;
            }
            return HeatRayFxField.GetValue(mapBox) as HeatRayEffect;
        }

        public static Heat GetHeat(MapBox mapBox)
        {
            if (mapBox == null) return null;
            if (HeatField == null)
            {
                WarnOnce(ref _warnedHeat, "[XN] MapBox.heat field not found; heat lookup failed.");
                return null;
            }
            return HeatField.GetValue(mapBox) as Heat;
        }

        public static WorldTile[] GetTilesList(MapBox mapBox)
        {
            if (mapBox == null) return null;
            if (TilesListField == null)
            {
                WarnOnce(ref _warnedTilesList, "[XN] MapBox.tiles_list field not found; tile list lookup failed.");
                return null;
            }
            return TilesListField.GetValue(mapBox) as WorldTile[];
        }

        public static void AddWorldLoadedHandler(Action handler)
        {
            if (handler == null) return;
            if (OnWorldLoadedField == null)
            {
                WarnOnce(ref _warnedOnWorldLoaded, "[XN] MapBox.on_world_loaded field not found; world-loaded handler was not registered.");
                return;
            }
            var current = OnWorldLoadedField.GetValue(null) as Action;
            OnWorldLoadedField.SetValue(null, (Action)Delegate.Combine(current, handler));
        }

        public static void RemoveWorldLoadedHandler(Action handler)
        {
            if (handler == null) return;
            if (OnWorldLoadedField == null)
            {
                WarnOnce(ref _warnedOnWorldLoaded, "[XN] MapBox.on_world_loaded field not found; world-loaded handler was not removed.");
                return;
            }
            var current = OnWorldLoadedField.GetValue(null) as Action;
            OnWorldLoadedField.SetValue(null, (Action)Delegate.Remove(current, handler));
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
