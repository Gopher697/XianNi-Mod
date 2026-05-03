using HarmonyLib;
using System.Reflection;
using UnityEngine;
using ai.behaviours;

namespace xn.access
{
    internal static class AiSystemAccess
    {
        private static readonly FieldInfo TaskField = AccessTools.Field(typeof(AiSystemActor), "task");
        private static bool _warnedTask;

        public static BehaviourTaskActor GetTask(AiSystemActor ai)
        {
            if (ai == null) return null;
            if (TaskField == null)
            {
                WarnOnce(ref _warnedTask, "[XN] AiSystem.task field not found; actor task lookup failed.");
                return null;
            }
            return TaskField.GetValue(ai) as BehaviourTaskActor;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
