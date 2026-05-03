using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class TutorialAccess
    {
        private static readonly FieldInfo PagesField = AccessTools.Field(typeof(Tutorial), "pages");
        private static readonly MethodInfo CreateMethod = AccessTools.Method(typeof(Tutorial), "create");
        private static readonly MethodInfo IsActiveMethod = AccessTools.Method(typeof(Tutorial), "isActive");
        private static bool _warnedPages;
        private static bool _warnedCreate;
        private static bool _warnedIsActive;

        public static List<TutorialPage> GetPages(Tutorial tutorial)
        {
            if (tutorial == null) return null;
            if (PagesField == null)
            {
                WarnOnce(ref _warnedPages, "[XN] Tutorial.pages field not found; newbie guide pages lookup failed.");
                return null;
            }
            return PagesField.GetValue(tutorial) as List<TutorialPage>;
        }

        public static void Create(Tutorial tutorial)
        {
            if (tutorial == null) return;
            if (CreateMethod == null)
            {
                WarnOnce(ref _warnedCreate, "[XN] Tutorial.create method not found; newbie guide tutorial creation failed.");
                return;
            }
            CreateMethod.Invoke(tutorial, null);
        }

        public static bool IsActive(Tutorial tutorial)
        {
            if (tutorial == null) return false;
            if (IsActiveMethod == null)
            {
                WarnOnce(ref _warnedIsActive, "[XN] Tutorial.isActive method not found; treating tutorial as inactive.");
                return false;
            }
            return IsActiveMethod.Invoke(tutorial, null) is bool value && value;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
