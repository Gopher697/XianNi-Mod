using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class SelectedUnitAccess
    {
        private static readonly FieldInfo UnitMainField = AccessTools.Field(typeof(SelectedUnit), "_unit_main");

        public static Actor GetUnitMain()
        {
            if (UnitMainField == null)
            {
                Debug.LogWarning("[XN] SelectedUnit._unit_main field not found.");
                return null;
            }
            return UnitMainField.GetValue(null) as Actor;
        }

        public static void SetUnitMain(Actor actor)
        {
            if (UnitMainField == null)
            {
                Debug.LogWarning("[XN] SelectedUnit._unit_main field not found; selected unit was not updated.");
                return;
            }
            UnitMainField.SetValue(null, actor);
        }
    }
}
