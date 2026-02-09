using HarmonyLib;
namespace xn.fix
{
    [HarmonyPatch(typeof(Army), "save")]
    internal static class ArmySaveFix
    {
        [HarmonyPrefix]
        private static bool Prefix(Army __instance)
        {
            if (__instance == null || __instance.data == null)
            {
                UnityEngine.Debug.LogWarning("[XN-Fix] ArmySaveFix: Skipping save for Army with null data");
                return false;
            }
            return true;
        }
    }
}