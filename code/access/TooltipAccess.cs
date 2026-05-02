using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class TooltipAccess
    {
        private static readonly MethodInfo AddLineTextMethod = AccessTools.Method(typeof(Tooltip), "addLineText");
        private static readonly MethodInfo SetDescriptionMethod = AccessTools.Method(typeof(Tooltip), "setDescription");
        private static bool _warnedAddLineText;
        private static bool _warnedSetDescription;

        public static void AddLineText(Tooltip tooltip, string id, string value, string color, bool percent = false, bool localize = true, int limitValue = 0)
        {
            if (tooltip == null) return;
            if (AddLineTextMethod == null)
            {
                WarnOnce(ref _warnedAddLineText, "[XN] Tooltip.addLineText method not found; tooltip line was not added.");
                return;
            }
            AddLineTextMethod.Invoke(tooltip, new object[] { id, value, color, percent, localize, limitValue });
        }

        public static void SetDescription(Tooltip tooltip, string description, string color = "#FFFFFF")
        {
            if (tooltip == null) return;
            if (SetDescriptionMethod == null)
            {
                WarnOnce(ref _warnedSetDescription, "[XN] Tooltip.setDescription method not found; tooltip description was not changed.");
                return;
            }
            SetDescriptionMethod.Invoke(tooltip, new object[] { description, color });
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
