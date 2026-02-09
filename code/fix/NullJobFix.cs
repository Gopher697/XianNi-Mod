using HarmonyLib;
namespace xn.fix
{
    [HarmonyPatch(typeof(Actor), "getNextJob")]
    internal static class Patch_Actor_GetNextJob_NullSafety
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Actor __instance, ref string __result)
        {
            if (__result == null)
            {
                __result = "random_move";
            }
        }
    }
}