using System;
using System.Collections.Generic;
using HarmonyLib;
using ai;
using UnityEngine;
using xn.Traits; 
namespace xn.world
{
    internal static class CultivationPracticeSystem
    {
        public static bool DisableCondense { get; set; } = false;
        private static readonly System.Random s_independentRandom = new System.Random(
            unchecked((int)DateTime.Now.Ticks) ^ System.Environment.TickCount
        );
        private const string KEY_WUXIN = "xn.stat.wuxin";    
        private const string KEY_LUCK = "xn.stat.qiyun";    
        private const string KEY_XP = "xn.stat.xiuwei";   
        private const string KEY_STOP   = "xn.cultivation.stop";            
        private const string KEY_COEFF  = "xn.root.coeff";                  
        private const string KEY_NEXT_TRY_YEAR = "xn.root.next_try_year";   
        private const string KEY_CITY_AURA = "xn.city.aura";                
        private const string KEY_DAOB_DAMAGED_UNTIL = "xn.daobase.damaged_until";
        private const string KEY_HALF_TATIAN_LOCKED = "xn.half_tatian.locked";    
        private const string KEY_CITY_ROOT_YEAR = "xn.city.root.try_year";
        private const string KEY_CITY_ROOT_USED = "xn.city.root.try_used";
        private const string KEY_CITY_ROOT_QUOTA = "xn.city.root.try_quota";
        private const string KEY_CONDENSE_READY = "xn.root.condense_ready";
        private const string KEY_ANC_POWER = "xn.stat.gushen_power"; 
        private const string KEY_ANC_STOP = "xn.ancient.stop"; 
        private const string KEY_BEAST_POWER = "xn.stat.yaoli";        
        private const string KEY_BEAST_STOP = "xn.beast.stop";   
        private const string KEY_KILLS = "xn.kill_count";   
        private const string KEY_KILLS_PREV = "xn.kill.prev";    
        private static readonly int[] ANC_THRESHOLDS = new int[] {
            5000, 30000, 50000, 100000, 200000, 500000,
            1000000, 1500000, 3000000, 5000000
        };
        private static readonly string[] ANC_STAR_IDS = new[] {
            "ancient_01_star","ancient_02_star","ancient_03_star","ancient_04_star","ancient_05_star",
            "ancient_06_star","ancient_07_star","ancient_08_star","ancient_09_star","ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = new[] {
            "beast_01_stage","beast_02_stage","beast_03_stage","beast_04_stage","beast_05_stage",
            "beast_06_stage","beast_07_stage","beast_08_stage","beast_09_stage","beast_10_stage"
        };
        private static readonly string[] ROOT_IDS = new[]
        {
            "root_01_mortal",  
            "root_02_low",     
            "root_03_mid",     
            "root_04_high",    
            "root_05_supreme", 
            "root_06_tiandi"   
        };
        private static readonly Vector2[] ROOT_COEFF_RANGE = new[]
        {
            new Vector2(0.1f, 0.5f),   
            new Vector2(0.8f, 1.5f),   
            new Vector2(1.8f, 3.0f),   
            new Vector2(4.0f, 7.5f),   
            new Vector2(8.0f, 14.0f),  
            new Vector2(10.0f, 20.0f)  
        };
        private static readonly long[] REALM_THRESHOLDS = new long[]
        {
            100000,      
            1500000,    
            4000000,    
            9600000,    
            30000000,   
            80000000,   
            150000000,  
            250000000,  
            400000000,  
            600000000,  
            700000000,  
            800000000,  
            900000000,  
            980000000,  
            1200000000,
            1500000000 
        };
        private static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi",
            "realm_02_foundation",
            "realm_03_core",
            "realm_04_nascent",
            "realm_05_deity",
            "realm_06_infantchg",
            "realm_07_wending",
            "realm_08_kuinie",
            "realm_09_jingnie",
            "realm_10_suinie",
            "realm_11_kongnie",
            "realm_12_kongling",
            "realm_13_kongxuan",
            "realm_14_gtianzun",
            "realm_15_half_tatian",
            "realm_16_tatian"
        };
        private static int s_lastAppliedYearPractice = -1;
        [HarmonyPatch(typeof(ActorManager), "finalizeActor")]
        private static class FinalizeActorInitStatsPatch
        {
            [HarmonyPostfix]
            private static void Postfix(Actor pActor)
            {
                if (pActor == null || xn.access.ActorAccess.GetData(pActor) == null) return;
                int existingWuxin;
                xn.access.ActorAccess.GetData(pActor).get(KEY_WUXIN, out existingWuxin, -1);
                if (existingWuxin < 0)
                {
                    xn.access.ActorAccess.GetData(pActor).set(KEY_WUXIN, s_independentRandom.Next(0, 101));
                }
                int existingLuck;
                xn.access.ActorAccess.GetData(pActor).get(KEY_LUCK, out existingLuck, -1);
                if (existingLuck < 0)
                {
                    xn.access.ActorAccess.GetData(pActor).set(KEY_LUCK, s_independentRandom.Next(0, 101));
                }
            }
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class YearlyPracticePatch
        {
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (__instance == null) return;
                int curYear = Date.getCurrentYear();
                if (curYear <= 0) return;
                if (curYear == s_lastAppliedYearPractice) return;
                s_lastAppliedYearPractice = curYear;
                var list = __instance.units != null ? __instance.units.getSimpleList() : null;
                if (list == null || list.Count == 0) return;
                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (a == null || !a.isAlive()) continue;
                    TryCondenseSpiritRootAnnual(a); 
                    GainCultivationAnnual(a);       
                    GainAncientBeastAnnual(a);       
                    TryConvertXinmoEvery20Years(a);
                }
            }
        }
        private static void TryCondenseSpiritRootAnnual(Actor a)
        {
            if (DisableCondense) return;
            if (a.kingdom == null || a.city == null) return;
            if (xn.access.ActorAccess.IsInsideBoat(a)) return;
            if (a.asset != null && a.asset.is_boat) return;
            if (HasAnySpiritRoot(a)) return;
            if (HasAnyAncientInheritance(a)) return;
            if (HasTraitId(a, "path_03_beast")) return;
            City c = a.city;
            int curYear = Date.getCurrentYear();
            if (c == null || c.data == null) return;
            int y; c.data.get(KEY_CITY_ROOT_YEAR, out y, -1);
            if (y != curYear)
            {
                c.data.set(KEY_CITY_ROOT_YEAR, curYear);
                c.data.set(KEY_CITY_ROOT_USED, 0);
                c.data.set(KEY_CITY_ROOT_QUOTA, UnityEngine.Random.Range(1, 21)); 
            }
            int used; c.data.get(KEY_CITY_ROOT_USED, out used, 0);
            int quota; c.data.get(KEY_CITY_ROOT_QUOTA, out quota, 0);
            if (quota <= 0) quota = 1; 
            if (used >= quota) return;
            curYear = Date.getCurrentYear();
            int nextYear;
            xn.access.ActorAccess.GetData(a).get(KEY_NEXT_TRY_YEAR, out nextYear, 0);
            if (nextYear > curYear)
                return;
            int actorAge = GetActorAge(a);
            float attemptChance = GetCondenseAttemptChance(actorAge);
            if (UnityEngine.Random.value > attemptChance) return;
            c = a.city;
            if (c == null || c.data == null) return;
            int aura;
            c.data.get(KEY_CITY_AURA, out aura, 0);
            if (aura <= 600) return; 
            int cost = UnityEngine.Random.Range(1, 151);
            int newAura = Mathf.Max(0, aura - cost);
            c.data.set(KEY_CITY_AURA, newAura);
            c.data.set(KEY_CITY_ROOT_USED, used + 1);
            xn.access.ActorAccess.GetData(a).set(KEY_CONDENSE_READY, 1);
        }
        private static int GetActorAge(Actor a)
        {
            if (a == null) return 0;
            try { return a.getAge(); }
            catch
            {
                ActorData data = xn.access.ActorAccess.GetData(a);
                return data != null ? data.getAge() : 0;
            }
        }

        private static float GetCondenseAttemptChance(int age)
        {
            if (age < 10) return 0f;
            if (age < 21) return 0.25f;
            if (age < 41) return 0.50f;
            if (age < 71) return 0.80f;
            if (age < 101) return 0.50f;
            if (age < 131) return 0.30f;
            return 0.15f;
        }
        private static bool HasAnySpiritRoot(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t != null && t.group_id == RealmTraitGroup.GroupSpiritRoot)
                    return true;
            }
            return false;
        }
        private static bool HasAnyAncientInheritance(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t != null && t.id != null && t.id.StartsWith("inherit_")) return true;
            }
            return false;
        }
        private static bool HasTrait(Actor a, string id)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list) if (t != null && t.id == id) return true;
            return false;
        }
        private static void GainCultivationAnnual(Actor a)
        {
            RefreshRootCoeffAnnual(a);
            int curYear = Date.getCurrentYear();
            int damagedUntil;
            xn.access.ActorAccess.GetData(a).get("xn.daobase.damaged_until", out damagedUntil, 0);
            if (damagedUntil > 0) 
            {
                if (damagedUntil <= curYear)
                {
                    var list = a.getTraits();
                    if (list != null)
                    {
                        foreach (var t in list)
                        {
                            if (t != null && t.id == "dao_07_damaged")
                            {
                                a.removeTrait(t);
                                break;
                            }
                        }
                    }
                    xn.access.ActorAccess.GetData(a).set("xn.daobase.damaged_until", 0);
                    xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
                }
                else
                {
                    return;
                }
            }
            if (HasTraitId(a, "path_03_beast")) return;
            if (HasAnyAncientInheritance(a)) return;
            int halfTatianLocked; xn.access.ActorAccess.GetData(a).get(KEY_HALF_TATIAN_LOCKED, out halfTatianLocked, 0);
            if (halfTatianLocked == 1) return;
            if (a.city == null) return;
            {
                int nextRealmIdx = NextRealmIndex(a); 
                int curIndex = nextRealmIdx - 1;      
                if (curIndex >= 3 && HasTraitId(a, "intent_01_extreme"))
                    return;                           
            }
            int stop; xn.access.ActorAccess.GetData(a).get(KEY_STOP, out stop, 0);
            if (stop == 1)
            {
                long curXP; xn.access.ActorAccess.GetData(a).get(KEY_XP, out curXP, 0L);
                int idx = NextRealmIndex(a);
                if (idx >= 0)
                {
                    long capNow = REALM_THRESHOLDS[idx];
                    if (curXP >= capNow)
                        return; 
                }
                xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
            }
            int aura;
            a.city.data.get(KEY_CITY_AURA, out aura, 0);
            if (aura <= 0) return;
            int wx;
            xn.access.ActorAccess.GetData(a).get(KEY_WUXIN, out wx, 0);
            float coeff = 0f;
            xn.access.ActorAccess.GetData(a).get(KEY_COEFF, out coeff, 0f);
            if (coeff <= 0f && HasBrokenRoot(a)) coeff = 0f;
            if (coeff <= 0f) return; 
            double gain = (double)aura * ((double)wx / 100.0) * (double)coeff;
            if (gain <= 0) return;
            long cur;
            xn.access.ActorAccess.GetData(a).get(KEY_XP, out cur, 0L);
            long next = cur + (long)gain;
            int nextIndex = NextRealmIndex(a);
            if (nextIndex >= 0 && nextIndex < REALM_THRESHOLDS.Length)
            {
                long cap = REALM_THRESHOLDS[nextIndex];
                if (cur >= cap)
                {
                    xn.access.ActorAccess.GetData(a).set(KEY_STOP, 1);
                    return;
                }
                if (next >= cap)
                {
                    next = cap; 
                    xn.access.ActorAccess.GetData(a).set(KEY_STOP, 1); 
                }
            }
            xn.access.ActorAccess.GetData(a).set(KEY_XP, next);
            RefreshRootCoeffAnnual(a);
        }
        private static bool HasBrokenRoot(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t != null && t.id == "root_07_broken")
                    return true;
            }
            return false;
        }
        private static int NextRealmIndex(Actor a)
        {
            int curIndex = -1;
            var list = a.getTraits();
            if (list != null)
            {
                for (int i = 0; i < REALM_IDS.Length; i++)
                {
                    foreach (var t in list)
                    {
                        if (t != null && t.id == REALM_IDS[i])
                        {
                            if (i > curIndex) curIndex = i;
                            break;
                        }
                    }
                }
            }
            int next = curIndex + 1;
            if (next >= REALM_THRESHOLDS.Length) return -1; 
            return next;
        }
        private static void RefreshRootCoeffAnnual(Actor a)
        {
            if (HasBrokenRoot(a))
            {
                xn.access.ActorAccess.GetData(a).set(KEY_COEFF, 0f);
                return;
            }
            int idxFound = -1;
            var list = a.getTraits();
            if (list != null)
            {
                for (int i = 0; i < ROOT_IDS.Length; i++)
                {
                    foreach (var t in list)
                    {
                        if (t != null && t.id == ROOT_IDS[i])
                        {
                            idxFound = i;
                            break;
                        }
                    }
                    if (idxFound >= 0) break;
                }
            }
            if (idxFound < 0)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_COEFF, 0f);
                return;
            }
            var range = ROOT_COEFF_RANGE[idxFound];
            float coeff = UnityEngine.Random.Range(range.x, range.y);
            xn.access.ActorAccess.GetData(a).set(KEY_COEFF, coeff);
        }
        private static void GainAncientBeastAnnual(Actor a)
        {
            int wx; xn.access.ActorAccess.GetData(a).get(KEY_WUXIN, out wx, 0);
            int qy; xn.access.ActorAccess.GetData(a).get(KEY_LUCK, out qy, 0);
            int wq = wx + qy;
            if (wq < 0) wq = 0;
            int kills; xn.access.ActorAccess.GetData(a).get(KEY_KILLS, out kills, 0);
            int killsPrev; xn.access.ActorAccess.GetData(a).get(KEY_KILLS_PREV, out killsPrev, 0);
            if (kills < 0) kills = 0;
            if (killsPrev < 0) killsPrev = 0;
            if (HasAnyAncientInheritance(a))
            {
                long totalAncientGain = 0; 
                if (UnityEngine.Random.value < 0.65f)
                {
                    totalAncientGain += wq; 
                }
                if (kills > killsPrev)
                {
                    float coeff = GetAncientInheritCoeff(a); 
                    if (coeff > 0f && wq > 0)
                    {
                        for (int i = killsPrev + 1; i <= kills; i++)
                        {
                            double g = (double)i * (double)coeff * (double)wq;
                            if (g > 0) totalAncientGain += (long)g;
                        }
                    }
                }
                if (totalAncientGain > 0)
                {
                    totalAncientGain *= xn.config.ModConfigHooks.AncientBeastMultiplier;
                    int ap; xn.access.ActorAccess.GetData(a).get(KEY_ANC_POWER, out ap, 0);
                    long next = (long)ap + totalAncientGain;
                    if (next > int.MaxValue) next = int.MaxValue;
                    xn.access.ActorAccess.GetData(a).set(KEY_ANC_POWER, (int)next);
                }
                int curStar = GetCurrentAncientStarIndex(a);     
                int nextStar = curStar + 1;
                if (nextStar >= 0 && nextStar < ANC_THRESHOLDS.Length)
                {
                    int ap; xn.access.ActorAccess.GetData(a).get(KEY_ANC_POWER, out ap, 0);
                    if (nextStar >= 2)
                    {
                        int cap = ANC_THRESHOLDS[nextStar];
                        if (ap >= cap)
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_ANC_POWER, cap);
                            xn.access.ActorAccess.GetData(a).set(KEY_ANC_STOP, 1); 
                        }
                    }
                    else
                    {
                        for (int s = nextStar; s < 2 && s < ANC_THRESHOLDS.Length; s++)
                        {
                            int cap = ANC_THRESHOLDS[s];
                            xn.access.ActorAccess.GetData(a).get(KEY_ANC_POWER, out ap, 0);
                            if (ap >= cap)
                            {
                                ReplaceTraitInSet(a, ANC_STAR_IDS, ANC_STAR_IDS[s]);
                                if (s == 0 && !HasTraitId(a, "path_04_ancient"))
                                {
                                    var p = AssetManager.traits.get("path_04_ancient") as ActorTrait;
                                    if (p != null) a.addTrait(p);
                                }
                                curStar = s; nextStar = curStar + 1;
                            }
                            else break;
                        }
                    }
                }
            }
            if (HasTraitId(a, "path_03_beast"))
            {
                long totalBeastGain = 0; 
                if (UnityEngine.Random.value < 0.90f)
                {
                    totalBeastGain += wq;
                }
                if (kills > killsPrev && wq > 0)
                {
                    for (int i = killsPrev + 1; i <= kills; i++)
                    {
                        long g = (long)i * (long)wq;
                        if (g > 0) totalBeastGain += g;
                    }
                }
                if (totalBeastGain > 0)
                {
                    totalBeastGain *= xn.config.ModConfigHooks.AncientBeastMultiplier;
                    int bp; xn.access.ActorAccess.GetData(a).get(KEY_BEAST_POWER, out bp, 0);
                    long next = (long)bp + totalBeastGain;
                    if (next > int.MaxValue) next = int.MaxValue;
                    xn.access.ActorAccess.GetData(a).set(KEY_BEAST_POWER, (int)next);
                }
                int curStage = GetCurrentBeastStageIndex(a);
                int nextStage = curStage + 1;
                if (nextStage >= 0 && nextStage < ANC_THRESHOLDS.Length)
                {
                    int bp; xn.access.ActorAccess.GetData(a).get(KEY_BEAST_POWER, out bp, 0);
                    if (nextStage >= 2)
                    {
                        int cap = ANC_THRESHOLDS[nextStage];
                        if (bp >= cap)
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_BEAST_POWER, cap);
                            xn.access.ActorAccess.GetData(a).set(KEY_BEAST_STOP, 1); 
                        }
                    }
                    else
                    {
                        for (int s = nextStage; s < 2 && s < ANC_THRESHOLDS.Length; s++)
                        {
                            int cap = ANC_THRESHOLDS[s];
                            xn.access.ActorAccess.GetData(a).get(KEY_BEAST_POWER, out bp, 0);
                            if (bp >= cap)
                            {
                                ReplaceTraitInSet(a, BEAST_STAGE_IDS, BEAST_STAGE_IDS[s]);
                                curStage = s; nextStage = curStage + 1;
                            }
                            else break;
                        }
                    }
                }
            }
            xn.access.ActorAccess.GetData(a).set(KEY_KILLS_PREV, kills);
        }
        private static float GetAncientInheritCoeff(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return 0f;
            foreach (var t in list)
            {
                if (t == null || t.id == null) continue;
                if (!t.id.StartsWith("inherit_")) continue;
                if (t.id == "inherit_01_poor") return UnityEngine.Random.Range(0.1f, 0.2f);
                if (t.id == "inherit_02_normal") return UnityEngine.Random.Range(0.5f, 0.8f);
                if (t.id == "inherit_03_supreme") return UnityEngine.Random.Range(1.0f, 2.0f);
                if (t.id == "inherit_04_tusi") return UnityEngine.Random.Range(3.0f, 5.0f);
                if (t.id == "inherit_05_ancientblood") return UnityEngine.Random.Range(6.0f, 10.0f);
            }
            return 0f;
        }
        private static int GetCurrentAncientStarIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < ANC_STAR_IDS.Length; i++)
            {
                foreach (var t in list)
                {
                    if (t != null && t.id == ANC_STAR_IDS[i]) { if (i > cur) cur = i; }
                }
            }
            return cur;
        }
        private static int GetCurrentBeastStageIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < BEAST_STAGE_IDS.Length; i++)
            {
                foreach (var t in list)
                {
                    if (t != null && t.id == BEAST_STAGE_IDS[i]) { if (i > cur) cur = i; }
                }
            }
            return cur;
        }
        private static void ReplaceTraitInSet(Actor a, string[] idSet, string newId)
        {
            var list = a.getTraits();
            if (list != null)
            {
                for (int i = 0; i < idSet.Length; i++)
                {
                    foreach (var t in list)
                    {
                        if (t != null && t.id == idSet[i]) { a.removeTrait(t); break; }
                    }
                }
            }
            var tr = AssetManager.traits.get(newId) as ActorTrait;
            if (tr != null) a.addTrait(tr);
        }
        private static bool HasTraitId(Actor a, string id)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list) if (t != null && t.id == id) return true;
            return false;
        }
        private const string KEY_XINMO = "xn.stat.xinmo";
        private static void TryConvertXinmoEvery20Years(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!HasTrait(a, "path_01_demonic")) return;
            int year = Date.getCurrentYear();
            if (year <= 0 || (year % 20) != 0) return;
            int xinmo; xn.access.ActorAccess.GetData(a).get(KEY_XINMO, out xinmo, 0);
            if (xinmo <= 0) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["damage"] += xinmo;
            xn.access.BaseSimObjectAccess.GetStats(a)["health"] += xinmo;
        }
    }
}
