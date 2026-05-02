using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class ActorSimpleComponentAccess
    {
        private static readonly FieldInfo ActorField = AccessTools.Field(typeof(ActorSimpleComponent), "actor");
        private static bool _warnedActor;

        public static Actor GetActor(ActorSimpleComponent component)
        {
            if (component == null) return null;
            if (ActorField == null)
            {
                WarnOnce(ref _warnedActor, "[XN] ActorSimpleComponent.actor field not found; component actor lookup failed.");
                return null;
            }
            return ActorField.GetValue(component) as Actor;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
