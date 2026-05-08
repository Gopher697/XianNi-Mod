using HarmonyLib;
namespace xn.expand
{
    public static class CultivationHistorySystem
    {
        private static bool _initialized = false;
        public static void Init(Harmony harmony)
        {
            if (_initialized)
                return;
            _initialized = true;
            harmony.PatchAll(typeof(UnitWindowCultivationHistoryPatch));
            UnityEngine.Debug.Log("[XN-CultivationHistory] Cultivation Chronicle system initialized");
        }
    }
}