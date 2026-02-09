using HarmonyLib;
namespace xn.world
{
    [HarmonyPatch(typeof(Actor), "isStarving")]
    internal static class Patch_Actor_IsStarving_RealmBypass
    {
        static bool Prefix(Actor __instance, ref bool __result)
        {
            if (HungerBypassUtil.HasAnyRealmTrait(__instance))
            {
                __result = false; 
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Actor), "updateNutritionDecay")]
    internal static class Patch_Actor_UpdateNutritionDecay_RealmBypass
    {
        static bool Prefix(Actor __instance, bool pDoStarvationDamage = true)
        {
            if (HungerBypassUtil.HasAnyRealmTrait(__instance))
            {
                int metabolicRate = __instance.subspecies.getMetabolicRate();
                __instance.decreaseNutrition(metabolicRate);
                int currentNutrition = __instance.getNutrition();
                if (currentNutrition <= 0)
                {
                    __instance.setNutrition(1, pClamp: false);
                }
                __instance.updateStamina();
                __instance.updateMana();
                return false; 
            }
            return true;
        }
    }
    internal static class HungerBypassUtil
    {
        public static bool HasAnyRealmTrait(Actor a)
        {
            if (a == null) return false;
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t == null) continue;
                string gid = t.group_id;
                if (gid == xn.Traits.RealmTraitGroup.GroupRealm
                 || gid == xn.Traits.RealmTraitGroup.GroupAncientRealm
                 || gid == xn.Traits.RealmTraitGroup.GroupBeastStage)
                    return true;
            }
            return false;
        }
    }
}