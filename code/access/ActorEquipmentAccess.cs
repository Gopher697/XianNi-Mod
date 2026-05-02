using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class ActorEquipmentAccess
    {
        private static readonly FieldInfo DictionaryField = AccessTools.Field(typeof(ActorEquipment), "_dictionary");
        private static readonly MethodInfo InitDictionaryMethod = AccessTools.Method(typeof(ActorEquipment), "initDictionary");

        public static Dictionary<EquipmentType, ActorEquipmentSlot> GetDictionary(ActorEquipment equipment)
        {
            if (equipment == null) return null;
            if (DictionaryField == null)
            {
                Debug.LogWarning("[XN] ActorEquipment._dictionary field not found.");
                return null;
            }
            return DictionaryField.GetValue(equipment) as Dictionary<EquipmentType, ActorEquipmentSlot>;
        }

        public static void InitDictionary(ActorEquipment equipment)
        {
            if (equipment == null) return;
            if (InitDictionaryMethod == null)
            {
                Debug.LogWarning("[XN] ActorEquipment.initDictionary method not found.");
                return;
            }
            InitDictionaryMethod.Invoke(equipment, null);
        }
    }
}
