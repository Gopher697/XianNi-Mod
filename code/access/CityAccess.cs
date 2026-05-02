using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class CityAccess
    {
        private static readonly FieldInfo KingdomField = AccessTools.Field(typeof(City), "kingdom");
        private static readonly MethodInfo ClearCaptureMethod = AccessTools.Method(typeof(City), "clearCapture");
        private static readonly MethodInfo TryToMakeWarriorMethod = AccessTools.Method(typeof(City), "tryToMakeWarrior", new[] { typeof(Actor) });
        private static bool _warnedKingdom;
        private static bool _warnedClearCapture;
        private static bool _warnedTryToMakeWarrior;

        public static Kingdom GetKingdom(City city)
        {
            if (city == null) return null;
            if (KingdomField == null)
            {
                WarnOnce(ref _warnedKingdom, "[XN] City.kingdom field not found; city kingdom lookup failed.");
                return null;
            }
            return KingdomField.GetValue(city) as Kingdom;
        }

        public static void ClearCapture(City city)
        {
            if (city == null) return;
            if (ClearCaptureMethod == null)
            {
                WarnOnce(ref _warnedClearCapture, "[XN] City.clearCapture method not found; capture state was not cleared.");
                return;
            }
            ClearCaptureMethod.Invoke(city, null);
        }

        public static bool TryToMakeWarrior(City city, Actor actor)
        {
            if (city == null || actor == null) return false;
            if (TryToMakeWarriorMethod == null)
            {
                WarnOnce(ref _warnedTryToMakeWarrior, "[XN] City.tryToMakeWarrior method not found; warrior conversion failed.");
                return false;
            }
            return TryToMakeWarriorMethod.Invoke(city, new object[] { actor }) is bool value && value;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
