using HarmonyLib;
namespace xn.world
{
    [HarmonyPatch(typeof(Actor), "addTrait",
        new System.Type[] { typeof(ActorTrait), typeof(bool) })]
    internal static class Patch_Actor_addTrait_AutoFavorite
    {
        private const string KEY_AUTO_FAV_DONE = "xn.auto_fav.done";
        private static readonly string[] REALM_IDS = {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        [HarmonyPostfix]
        private static void Postfix(Actor __instance, ActorTrait pTrait)
        {
            if (__instance == null || pTrait == null) return;
            if (__instance.asset != null && __instance.asset.id == "dashou")
                return;
            int done; xn.access.ActorAccess.GetData(__instance).get(KEY_AUTO_FAV_DONE, out done, 0);
            if (done == 1) return;
            if (__instance.isFavorite())
            {
                xn.access.ActorAccess.GetData(__instance).set(KEY_AUTO_FAV_DONE, 1);
                return;
            }
            string g = pTrait.group_id;
            if (g == xn.Traits.RealmTraitGroup.GroupRealm)
            {
                int gate = xn.config.ModConfigHooks.AutoFavRealmGate; 
                if (gate <= 0) return; 
                if (gate > 13) return; 
                int minIdx = gate + 2; 
                int addedIdx = -1;
                for (int i = 0; i < REALM_IDS.Length; i++)
                    if (pTrait.id == REALM_IDS[i]) { addedIdx = i; break; }
                if (addedIdx >= minIdx)
                    AutoFavOnce(__instance);
                return;
            }
            if (g == xn.Traits.RealmTraitGroup.GroupAncientRealm)
            {
                int star = GetAncientStar(__instance);
                int requiredGate = GetRequiredGateForAncientBeast(star);
                if (requiredGate < 0) return; 
                int gate = xn.config.ModConfigHooks.AutoFavRealmGate;
                if (gate <= 0) return; 
                if (gate > 13) return; 
                if (gate <= requiredGate) AutoFavOnce(__instance);
                return;
            }
            if (g == xn.Traits.RealmTraitGroup.GroupBeastStage)
            {
                int stage = GetBeastStage(__instance);
                int requiredGate = GetRequiredGateForAncientBeast(stage);
                if (requiredGate < 0) return; 
                int gate = xn.config.ModConfigHooks.AutoFavRealmGate;
                if (gate <= 0) return; 
                if (gate > 13) return; 
                if (gate <= requiredGate) AutoFavOnce(__instance);
                return;
            }
        }
        private static int GetRequiredGateForAncientBeast(int starOrStage)
        {
            switch (starOrStage)
            {
                case 3: return 4;
                case 4: return 5;
                case 5: return 8;
                case 6: return 9;
                case 7: return 10;
                case 8: return 11;
                case 9: return 12;
                case 10: return 13;
                default: return -1; 
            }
        }
        private static void AutoFavOnce(Actor a)
        {
            if (!a.isFavorite())
                a.switchFavorite(); 
            xn.access.ActorAccess.GetData(a).set(KEY_AUTO_FAV_DONE, 1); 
        }
        private static int GetAncientStar(Actor a)
        {
            var ts = a.getTraits(); if (ts == null) return 0;
            int star = 0;
            foreach (var t in ts)
            {
                if (t == null || t.group_id != xn.Traits.RealmTraitGroup.GroupAncientRealm) continue;
                if (t.id.Length >= 14)
                {
                    if (int.TryParse(t.id.Substring(8, 2), out int n) && n > star) star = n;
                }
            }
            return star;
        }
        private static int GetBeastStage(Actor a)
        {
            var ts = a.getTraits(); if (ts == null) return 0;
            int st = 0;
            foreach (var t in ts)
            {
                if (t == null || t.group_id != xn.Traits.RealmTraitGroup.GroupBeastStage) continue;
                if (t.id.Length >= 13)
                {
                    if (int.TryParse(t.id.Substring(6, 2), out int n) && n > st) st = n;
                }
            }
            return st;
        }
    }
}