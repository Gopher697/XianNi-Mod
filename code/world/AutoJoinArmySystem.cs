using HarmonyLib;
namespace xn.world
{
    [HarmonyPatch(typeof(City), "tryToMakeWarrior")]
    internal static class Patch_City_TryToMakeWarrior_PrioritizeHighRealm
    {
        private static bool _isProcessing = false;
        [HarmonyPrefix]
        private static bool Prefix(City __instance, Actor pActor, ref bool __result)
        {
            if (_isProcessing) return true;
            if (!xn.config.ModConfigHooks.EnableAutoJoinArmy) return true;
            if (!__instance.checkCanMakeWarrior(pActor)) return true;
            Actor bestActor = FindHighestRealmUnit(__instance);
            if (bestActor == null || bestActor == pActor) return true;
            if (!__instance.checkCanMakeWarrior(bestActor)) return true;
            _isProcessing = true;
            try
            {
                __result = __instance.tryToMakeWarrior(bestActor);
            }
            finally
            {
                _isProcessing = false;
            }
            return false; 
        }
        private static Actor FindHighestRealmUnit(City city)
        {
            if (city == null || city.units == null) return null;
            Actor bestActor = null;
            int bestLevel = -1;
            foreach (var unit in city.units)
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.isKing() || unit.isCityLeader()) continue;
                if (unit.isBaby()) continue;
                if (!unit.isProfession(UnitProfession.Unit)) continue; 
                int level = GetRealmLevel(unit);
                if (level > bestLevel)
                {
                    bestLevel = level;
                    bestActor = unit;
                }
            }
            return bestActor;
        }
        private static int GetRealmLevel(Actor actor)
        {
            if (actor == null) return -1;
            var traits = actor.getTraits();
            if (traits == null) return -1;
            int maxLevel = -1;
            foreach (var trait in traits)
            {
                if (trait == null) continue;
                if (trait.group_id == xn.Traits.RealmTraitGroup.GroupRealm)
                {
                    int idx = GetRealmIndex(trait.id);
                    if (idx > maxLevel) maxLevel = idx;
                }
                else if (trait.group_id == xn.Traits.RealmTraitGroup.GroupAncientRealm)
                {
                    int star = GetAncientStarIndex(trait.id);
                    int converted = ConvertToRealmLevel(star);
                    if (converted > maxLevel) maxLevel = converted;
                }
                else if (trait.group_id == xn.Traits.RealmTraitGroup.GroupBeastStage)
                {
                    int stage = GetBeastStageIndex(trait.id);
                    int converted = ConvertToRealmLevel(stage);
                    if (converted > maxLevel) maxLevel = converted;
                }
            }
            return maxLevel;
        }
        private static int GetRealmIndex(string id)
        {
            switch (id)
            {
                case "realm_01_qi": return 0;
                case "realm_02_foundation": return 1;
                case "realm_03_core": return 2;
                case "realm_04_nascent": return 3;
                case "realm_05_deity": return 4;
                case "realm_06_infantchg": return 5;
                case "realm_07_wending": return 6;
                case "realm_08_kuinie": return 7;
                case "realm_09_jingnie": return 8;
                case "realm_10_suinie": return 9;
                case "realm_11_kongnie": return 10;
                case "realm_12_kongling": return 11;
                case "realm_13_kongxuan": return 12;
                case "realm_14_gtianzun": return 13;
                case "realm_15_half_tatian": return 14;
                case "realm_16_tatian": return 15;
                default: return -1;
            }
        }
        private static int GetAncientStarIndex(string id)
        {
            switch (id)
            {
                case "ancient_01_star": return 1;
                case "ancient_02_star": return 2;
                case "ancient_03_star": return 3;
                case "ancient_04_star": return 4;
                case "ancient_05_star": return 5;
                case "ancient_06_star": return 6;
                case "ancient_07_star": return 7;
                case "ancient_08_star": return 8;
                case "ancient_09_star": return 9;
                case "ancient_10_star": return 10;
                default: return 0;
            }
        }
        private static int GetBeastStageIndex(string id)
        {
            switch (id)
            {
                case "beast_01_stage": return 1;
                case "beast_02_stage": return 2;
                case "beast_03_stage": return 3;
                case "beast_04_stage": return 4;
                case "beast_05_stage": return 5;
                case "beast_06_stage": return 6;
                case "beast_07_stage": return 7;
                case "beast_08_stage": return 8;
                case "beast_09_stage": return 9;
                case "beast_10_stage": return 10;
                default: return 0;
            }
        }
        private static int ConvertToRealmLevel(int level)
        {
            switch (level)
            {
                case 1: return 2;
                case 2: return 4;
                case 3: return 6;
                case 4: return 7;
                case 5: return 8;
                case 6: return 9;
                case 7: return 10;
                case 8: return 11;
                case 9: return 13;
                case 10: return 14;
                default: return -1;
            }
        }
    }
}