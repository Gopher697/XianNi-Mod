using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class TooltipDataAccess
    {
        private static readonly FieldInfo TipNameField = AccessTools.Field(typeof(TooltipData), "_tip_name");
        private static readonly FieldInfo TipDescriptionField = AccessTools.Field(typeof(TooltipData), "_tip_description");

        public static TooltipData Create(string name, string description)
        {
            var data = new TooltipData();
            SetTipName(data, name);
            SetTipDescription(data, description);
            return data;
        }

        public static string GetTipName(TooltipData data)
        {
            if (data == null || TipNameField == null) return null;
            return TipNameField.GetValue(data) as string;
        }

        public static string GetTipDescription(TooltipData data)
        {
            if (data == null || TipDescriptionField == null) return null;
            return TipDescriptionField.GetValue(data) as string;
        }

        private static void SetTipName(TooltipData data, string value)
        {
            if (data == null) return;
            if (TipNameField == null)
            {
                Debug.LogWarning("[XN] TooltipData._tip_name field not found.");
                return;
            }
            TipNameField.SetValue(data, value);
        }

        private static void SetTipDescription(TooltipData data, string value)
        {
            if (data == null) return;
            if (TipDescriptionField == null)
            {
                Debug.LogWarning("[XN] TooltipData._tip_description field not found.");
                return;
            }
            TipDescriptionField.SetValue(data, value);
        }
    }
}
