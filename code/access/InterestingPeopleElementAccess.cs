using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace xn.access
{
    internal static class InterestingPeopleElementAccess
    {
        private static readonly FieldInfo CounterField = AccessTools.Field(typeof(InterestingPeopleElement), "_counter");
        private static readonly FieldInfo GridField = AccessTools.Field(typeof(InterestingPeopleElement), "_grid");
        private static readonly FieldInfo ElementField = AccessTools.Field(typeof(InterestingPeopleElement), "_element");
        private static readonly MethodInfo ShowMemberMethod = AccessTools.Method(typeof(InterestingPeopleElement), "showMember", new[] { typeof(Actor) });
        private static bool _warnedShowMember;

        public static Text GetCounter(InterestingPeopleElement element)
        {
            if (element == null || CounterField == null) return null;
            return CounterField.GetValue(element) as Text;
        }

        public static Transform GetGrid(InterestingPeopleElement element)
        {
            if (element == null || GridField == null) return null;
            return FieldValueToTransform(GridField.GetValue(element));
        }

        public static Transform GetElementTransform(InterestingPeopleElement element)
        {
            if (element == null || ElementField == null) return null;
            return FieldValueToTransform(ElementField.GetValue(element));
        }

        public static void ShowMember(InterestingPeopleElement element, Actor actor)
        {
            if (element == null || actor == null) return;
            if (ShowMemberMethod == null)
            {
                if (!_warnedShowMember)
                {
                    _warnedShowMember = true;
                    Debug.LogWarning("[XN] InterestingPeopleElement.showMember method not found; top power display cannot show member.");
                }
                return;
            }
            ShowMemberMethod.Invoke(element, new object[] { actor });
        }

        private static Transform FieldValueToTransform(object value)
        {
            if (value is Transform transform) return transform;
            if (value is Component component) return component.transform;
            if (value is GameObject gameObject) return gameObject.transform;
            return null;
        }
    }
}
