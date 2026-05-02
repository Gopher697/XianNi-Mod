using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class PowerButtonAccess
    {
        private static readonly FieldInfo GodPowerField = AccessTools.Field(typeof(PowerButton), "godPower");
        private static bool _warnedGodPower;

        public static GodPower GetGodPower(PowerButton button)
        {
            if (button == null) return null;
            if (GodPowerField == null)
            {
                if (!_warnedGodPower)
                {
                    _warnedGodPower = true;
                    Debug.LogWarning("[XN] PowerButton.godPower field not found; selected power lookup failed.");
                }
                return null;
            }
            return GodPowerField.GetValue(button) as GodPower;
        }
    }
}
