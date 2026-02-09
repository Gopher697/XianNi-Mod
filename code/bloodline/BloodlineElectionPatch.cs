using HarmonyLib;
namespace xn.bloodline
{
    [HarmonyPatch(typeof(MapBox), "updateSimulation")]
    internal static class Patch_MapBox_UpdateSimulation_Election
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            BloodlineElectionSystem.CheckElections();
        }
    }
}