using HarmonyLib;
using System.Collections.Generic;
namespace xn.bloodline
{
    [HarmonyPatch(typeof(Actor), "addTrait", new[] { typeof(string), typeof(bool) })]
    internal static class Patch_Actor_AddTrait_BloodlineRestriction
    {
        private static readonly HashSet<string> LINGGEN_TRAITS = new HashSet<string>
        {
            "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
            "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
            "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
            "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian",
            "linggen_jin", "linggen_mu", "linggen_shui", "linggen_huo", "linggen_tu",
            "linggen_feng", "linggen_lei", "linggen_bing", "linggen_an", "linggen_guang"
        };
        private static readonly HashSet<string> GUSHEN_TRAITS = new HashSet<string>
        {
            "ancient_01_star", "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star",
            "ancient_06_star", "ancient_07_star", "ancient_08_star", "ancient_09_star", "ancient_10_star"
        };
        private static readonly HashSet<string> YAOSHOU_TRAITS = new HashSet<string>
        {
            "beast_01_stage", "beast_02_stage", "beast_03_stage", "beast_04_stage", "beast_05_stage",
            "beast_06_stage", "beast_07_stage", "beast_08_stage", "beast_09_stage", "beast_10_stage"
        };
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance, string pTraitID)
        {
            if (__instance == null || string.IsNullOrEmpty(pTraitID)) return true;
            if (!BloodlineSystem.HasBloodline(__instance)) return true;
            string bloodlineType = BloodlineSystem.GetBloodlineType(__instance);
            if (string.IsNullOrEmpty(bloodlineType)) return true;
            int bloodlineSystem = GetBloodlineSystem(bloodlineType);
            if (bloodlineSystem == 0) return true; 
            int traitSystem = GetTraitSystem(pTraitID);
            if (traitSystem == 0) return true; 
            if (bloodlineSystem != traitSystem)
            {
                return false;
            }
            return true;
        }
        private static int GetBloodlineSystem(string bloodlineType)
        {
            if (BloodlineTypes.IsMutation(bloodlineType)) return 0;
            foreach (var t in BloodlineTypes.XIAN_MO_POOL)
            {
                if (t == bloodlineType) return 1;
            }
            foreach (var t in BloodlineTypes.YAOSHOU_POOL)
            {
                if (t == bloodlineType) return 2;
            }
            foreach (var t in BloodlineTypes.GUSHEN_POOL)
            {
                if (t == bloodlineType) return 3;
            }
            return 0;
        }
        private static int GetTraitSystem(string traitId)
        {
            if (LINGGEN_TRAITS.Contains(traitId)) return 1;
            if (YAOSHOU_TRAITS.Contains(traitId)) return 2;
            if (GUSHEN_TRAITS.Contains(traitId)) return 3;
            return 0;
        }
    }
}