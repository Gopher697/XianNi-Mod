using System;
using HarmonyLib;
using ai;
using ai.behaviours;
using UnityEngine;
using System.Collections.Generic; 
using xn.Traits;
using xn.world;
using static xn.world.HeavenTrialFX; 
namespace cultivation.ai
{
    internal static class BreakthroughJob
    {
        public static void Init()
        {
            RegisterJob();
        }
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        private static string Ordinal(int value)
        {
            int mod100 = value % 100;
            if (mod100 >= 11 && mod100 <= 13) return value + "th";
            switch (value % 10)
            {
                case 1: return value + "st";
                case 2: return value + "nd";
                case 3: return value + "rd";
                default: return value + "th";
            }
        }

        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib.get("job_xn_breakthrough") != null)
                return;
            RegisterBreakthroughTask();
            var job = new ActorJob { id = "job_xn_breakthrough" };
            lib.add(job);
            job.addTask("task_xn_breakthrough_stay");
            job.addTask("end_job");
        }

        internal static void BeginAncientTrial(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            StartHeavenTrial(a, 3, GetCurrentAncientIndex(a));
        }

        internal static void BeginBeastTrial(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            StartHeavenTrial(a, 4, GetCurrentBeastIndex(a));
        }

        internal static void BeginHeavenTrial(Actor a, int curRealmIndex)
        {
            if (a == null || !a.isAlive()) return;
            int ttype = HasTrait(a, "path_01_demonic") ? 2 : 1;
            StartHeavenTrial(a, ttype, curRealmIndex);
        }

        private static void RegisterBreakthroughTask()
        {
            var taskLib = AssetManager.tasks_actor;
            if (taskLib.get("task_xn_breakthrough_stay") != null)
                return;
            var task = new BehaviourTaskActor 
            { 
                id = "task_xn_breakthrough_stay",
                locale_key = "task_xn_breakthrough_stay"
            };
            taskLib.add(task);
            task.addBeh(new BehBuildingTargetHome());
            task.addBeh(new BehGetTargetBuildingMainTile());
            task.addBeh(new BehGoToTileTarget());
            task.addBeh(new BehStayInBuildingTarget(20f, 60f));
            task.addBeh(new BehDoBreakthrough());
            task.addBeh(new BehRestoreStats(0.1f, 0.2f));
            task.addBeh(new BehExitBuilding());
        }
        private class BehDoBreakthrough : BehaviourActionActor
        {
            public override BehResult execute(Actor pActor)
            {
                int curYear = Date.getCurrentYear();
                TryDoBreakthrough(pActor, curYear);
                return BehResult.Continue;
            }
        }
        private const string KEY_STOP = "xn.cultivation.stop";   
        private const string KEY_XP = "xn.stat.xiuwei";        
        private const string KEY_COEFF = "xn.root.coeff";         
        private const string KEY_WUXIN = "xn.stat.wuxin";
        private const string KEY_QIYUN = "xn.stat.qiyun";
        private const string KEY_DAOB_ID = "xn.daobase.id";            
        private const string KEY_DAOB_DAMAGED_UNTIL = "xn.daobase.damaged_until"; 
        private const string KEY_BREAK_TRIED_YEAR = "xn.break.tried_year";      
        private const string KEY_BREAK_SUCCESS_YEAR = "xn.break.success_year";  
        private const string KEY_XINMO = "xn.stat.xinmo"; 
        private const string KEY_ANC_POWER = "xn.stat.gushen_power"; 
        private const string KEY_BEAST_POWER = "xn.stat.yaoli";        
        private static readonly string[] ATTR_IDS = {
            "attr_01_metal", "attr_02_wood", "attr_03_water", "attr_04_fire", "attr_05_earth"
        };
        private const string KEY_ATTR_TRY_11 = "xn.benyuan.try.11"; 
        private const string KEY_ATTR_TRY_12 = "xn.benyuan.try.12"; 
        private const string KEY_ATTR_TRY_13 = "xn.benyuan.try.13"; 
        private const string KEY_TRIAL_ACTIVE = "xn.trial.active";          
        private const string KEY_TRIAL_TYPE = "xn.trial.type";            
        private const string KEY_TRIAL_TARGET = "xn.trial.target";          
        private const string KEY_TRIAL_BRIDGE = "xn.trial.bridge";          
        private const string KEY_TRIAL_END_T = "xn.trial.end_t";           
        private const string KEY_TRIAL_COOLDOWN_UNTIL = "xn.trial.cooldown_until";  
        private const string KEY_TRIAL_NEXT_LIGHTNING = "xn.trial.next_lightning"; 
        private const string KEY_HALF_TATIAN_LOCKED = "xn.half_tatian.locked";    
        private const float TRIAL_DURATION_SECONDS = 120f;                   
        private const float TRIAL_STUN_LARGE = 9999f;                        
        private const float LIGHTNING_INTERVAL = 10f;                        
        private const int TRIAL_COOLDOWN_YEARS = 30;                         
        private const string KEY_ANC_STOP = "xn.ancient.stop";
        private const string KEY_BEAST_STOP = "xn.beast.stop";
        private static readonly string[] ANC_STAR_IDS = new[] {
            "ancient_01_star","ancient_02_star","ancient_03_star","ancient_04_star","ancient_05_star",
            "ancient_06_star","ancient_07_star","ancient_08_star","ancient_09_star","ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = new[] {
            "beast_01_stage","beast_02_stage","beast_03_stage","beast_04_stage","beast_05_stage",
            "beast_06_stage","beast_07_stage","beast_08_stage","beast_09_stage","beast_10_stage"
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
        private static int GetCurrentRealmIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
            {
                foreach (var t in list)
                {
                    if (t != null && t.id == REALM_IDS[i]) { if (i > cur) cur = i; }
                }
            }
            return cur;
        }
        private static int GetNextRealmIndex(Actor a)
        {
            int cur = GetCurrentRealmIndex(a);
            int next = cur + 1;
            if (next >= REALM_IDS.Length) return -1;
            return next;
        }
        private static bool IsHeavenGateRealm(int idx)
        {
            return idx == 6 || idx == 9 || idx == 12 || idx == 13 || idx == 14;
        }
        private static bool HasDaoBaseDamage(Actor a, int curYear)
        {
            int until;
            xn.access.ActorAccess.GetData(a).get(KEY_DAOB_DAMAGED_UNTIL, out until, 0);
            return until > curYear;
        }
        private static bool TryDoBreakthrough(Actor a, int curYear)
        {
            int cur = GetCurrentRealmIndex(a);
            int next = GetNextRealmIndex(a);
            if (next < 0) return false; 
            if (IsHeavenGateRealm(cur))
                return false;
            if (HasDaoBaseDamage(a, curYear))
                return false;
            long xp; xn.access.ActorAccess.GetData(a).get(KEY_XP, out xp, 0L);
            int luck; xn.access.ActorAccess.GetData(a).get(KEY_QIYUN, out luck, 0);
            if (luck < 20) luck = 20; 
            if (cur < 1) 
            {
                float prob = Mathf.Clamp01(luck / 100f);
                bool success = UnityEngine.Random.value < prob;
                if (!success)
                {
                    float r = UnityEngine.Random.Range(0.1f, 1f);
                    long lose = (long)(xp * r);
                    long left = Math.Max(0, xp - lose);
                    xn.access.ActorAccess.GetData(a).set(KEY_XP, left);
                    IncreaseXinmoAndMaybeCorrupt(a); 
                    {
                    int curIdx = GetCurrentRealmIndex(a);   
                        if (curIdx >= 0 && curIdx < 4 && !HasTrait(a, "intent_01_extreme"))
                        {
                            if (UnityEngine.Random.value < 0.01f)
                            {
                                var t = AssetManager.traits.get("intent_01_extreme") as ActorTrait;
                                if (t != null) a.addTrait(t);
                                BroadcastSystem.IntentGain(a, "intent_01_extreme");
                            }
                        }
                    }
                    xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0); 
                    return false;
                }
                var trait = AssetManager.traits.get(REALM_IDS[next]) as ActorTrait;
                if (trait != null)
                {
                    if (cur >= 0)
                    {
                        var curTrait = AssetManager.traits.get(REALM_IDS[cur]) as ActorTrait;
                        if (curTrait != null && a.hasTrait(REALM_IDS[cur]))
                        {
                            a.removeTrait(curTrait);
                        }
                    }
                    if (!a.hasTrait(REALM_IDS[next]))
                    {
                        a.addTrait(trait);
                    }
                }
                xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
                if (next >= 3)
                {
                    BroadcastSystem.RealmUp(a, REALM_IDS[next]);
                }
                if (HasTrait(a, "path_02_immortal") && !HasTrait(a, "path_01_demonic"))
                    xn.access.ActorAccess.GetData(a).set(KEY_XINMO, 0);
                MaybeGiveAttrForRealm(a, next);
                if (next == 1) EnsureDaoBaseAfterFoundation(a);
                GiveRealmBreakthroughRewards(a, next);
                return true;
            }
            string daoId = GetDaoBaseCode(a);
            if (string.IsNullOrEmpty(daoId))
                daoId = "01"; 
            float successProb;
            float damageLoseMin = 0.1f; 
            float damageLoseMax = 1.0f; 
            float chanceDamage;         
            bool canDowngrade;         
            float chanceDowngrade;      
            switch (daoId)
            {
                case "01":
                    successProb = 0.20f; chanceDamage = 0.50f; damageLoseMin = 0.1f; damageLoseMax = 1.0f; canDowngrade = true; chanceDowngrade = 0.50f; break;
                case "02":
                    successProb = 0.30f; chanceDamage = 0.65f; damageLoseMin = 0.1f; damageLoseMax = 0.8f; canDowngrade = true; chanceDowngrade = 0.35f; break;
                case "03":
                    successProb = 0.50f; chanceDamage = 0.70f; damageLoseMin = 0.1f; damageLoseMax = 0.7f; canDowngrade = true; chanceDowngrade = 0.30f; break;
                case "04":
                    successProb = 0.70f; chanceDamage = 0.75f; damageLoseMin = 0.1f; damageLoseMax = 0.6f; canDowngrade = true; chanceDowngrade = 0.25f; break;
                case "05":
                    successProb = 0.80f; chanceDamage = 0.90f; damageLoseMin = 0.1f; damageLoseMax = 0.5f; canDowngrade = true; chanceDowngrade = 0.10f; break;
                case "06":
                    successProb = 0.90f; chanceDamage = 1.00f; damageLoseMin = 0.1f; damageLoseMax = 0.4f; canDowngrade = false; chanceDowngrade = 0f; break;
                default:
                    successProb = 0.20f; chanceDamage = 0.50f; damageLoseMin = 0.1f; damageLoseMax = 0.3f; canDowngrade = true; chanceDowngrade = 0.50f; break;
            }
            bool ok = UnityEngine.Random.value < successProb;
            if (ok)
            {
                var trait = AssetManager.traits.get(REALM_IDS[next]) as ActorTrait;
                if (trait != null) a.addTrait(trait);
                xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_BREAK_SUCCESS_YEAR, curYear);
                if (next >= 3)
                {
                    BroadcastSystem.RealmUp(a, REALM_IDS[next]);
                }
                if (next == 1) EnsureDaoBaseAfterFoundation(a);
                GiveRealmBreakthroughRewards(a, next);
                return true;
            }
            float branch = UnityEngine.Random.value;
            if (branch < chanceDamage || !canDowngrade)
            {
                float r = UnityEngine.Random.Range(damageLoseMin, damageLoseMax);
                long lose = (long)(xp * r);
                long left = Math.Max(0, xp - lose);
                xn.access.ActorAccess.GetData(a).set(KEY_XP, left);
                int until = curYear + UnityEngine.Random.Range(1, 21);
                xn.access.ActorAccess.GetData(a).set(KEY_DAOB_DAMAGED_UNTIL, until);
                var damaged = AssetManager.traits.get("dao_07_damaged") as ActorTrait;
                if (damaged != null) a.addTrait(damaged);
                IncreaseXinmoAndMaybeCorrupt(a);
                xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
                {
                    int curIdx = GetCurrentRealmIndex(a);   
                    if (curIdx >= 0 && curIdx < 4 && !HasTrait(a, "intent_01_extreme"))
                    {
                        if (UnityEngine.Random.value < 0.01f)
                        {
                            var t = AssetManager.traits.get("intent_01_extreme") as ActorTrait;
                            if (t != null) a.addTrait(t);
                            BroadcastSystem.IntentGain(a, "intent_01_extreme");
                        }
                    }
                }
                return false;
            }
            else
            {
                if (cur >= 1)
                {
                    var curTrait = AssetManager.traits.get(REALM_IDS[cur]) as ActorTrait;
                    var prevTrait = AssetManager.traits.get(REALM_IDS[cur - 1]) as ActorTrait;
                    if (curTrait != null) a.removeTrait(curTrait);
                    if (prevTrait != null) a.addTrait(prevTrait);
                    long half = REALM_THRESHOLDS[cur] / 2;
                    xn.access.ActorAccess.GetData(a).set(KEY_XP, half);
                    xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
                    BroadcastSystem.RealmFailDemote(a, REALM_IDS[cur - 1]);
                }
                else
                {
                    float r = UnityEngine.Random.Range(0.1f, 1.0f);
                    long lose = (long)(xp * r);
                    xn.access.ActorAccess.GetData(a).set(KEY_XP, Math.Max(0, xp - lose));
                    xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
                }
                IncreaseXinmoAndMaybeCorrupt(a);
                {
                    int curIdx = GetCurrentRealmIndex(a);   
                    if (curIdx >= 0 && curIdx < 4 && !HasTrait(a, "intent_01_extreme"))
                    {
                        if (UnityEngine.Random.value < 0.01f)
                        {
                            var t = AssetManager.traits.get("intent_01_extreme") as ActorTrait;
                            if (t != null) a.addTrait(t);                           
                            BroadcastSystem.IntentGain(a, "intent_01_extreme");
                        }
                    }
                }
                return false;
            }
        }
        private static void StartHeavenTrial(Actor a, int ttype, int targetIndex)
        {
            if (a == null || !a.isAlive()) return;
            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_ACTIVE, 1);
            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_TYPE, ttype);     
            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_TARGET, targetIndex);
            if (ttype == 1 || ttype == 2)
            {
                if (targetIndex == 13) 
                {
                    bool hasHalfTatian = HasTrait(a, "realm_15_half_tatian");
                    int startBridge = hasHalfTatian ? 5 : 0;
                    long existingL; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_BRIDGE, out existingL, 0L);
                    int existing = (int)existingL;
                    if (existing > 8)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_BRIDGE, (long)startBridge);
                    }
                    else if (!hasHalfTatian && existing >= 5)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_BRIDGE, 0L);
                    }
                    else if (hasHalfTatian && existing < 5)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_BRIDGE, 5L);
                    }
                    else if (existing < startBridge)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_BRIDGE, (long)startBridge);
                    }
                }
            }
            float now = Time.time;
            a.makeStunned(TRIAL_STUN_LARGE);
            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_END_T, now + TRIAL_DURATION_SECONDS);
            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_NEXT_LIGHTNING, now);
            if (targetIndex == 13) {
                long bL; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_BRIDGE, out bL, 0L); int b = (int)bL; 
                BroadcastSystem.PostActor(a, T("broadcast_tatian_bridge_attempt", "{0} is attempting the {1} Heaven Trampling Bridge", a.getName(), Ordinal(b + 1)));
            }
            else
            {
                BroadcastSystem.HeavenStart(a);
            }
            HeavenTrialFX.StartFor(a);
        }
        private static string GetDaoBaseCode(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return "";
            foreach (var t in list)
            {
                if (t == null) continue;
                if (t.group_id == RealmTraitGroup.GroupDaoBase)
                {
                    if (t.id == "dao_07_damaged") continue;
                    if (t.id.StartsWith("dao_01")) return "01";
                    if (t.id.StartsWith("dao_02")) return "02";
                    if (t.id.StartsWith("dao_03")) return "03";
                    if (t.id.StartsWith("dao_04")) return "04";
                    if (t.id.StartsWith("dao_05")) return "05";
                    if (t.id.StartsWith("dao_06")) return "06";
                }
            }
            return "";
        }
        private static int GetDaoBaseCooldownYears(string daoCode)
        {
            switch (daoCode)
            {
                case "01": return 30;
                case "02": return 25;
                case "03": return 20;
                case "04": return 15;
                case "05": return 10;
                case "06": return 5;
                default: return 30; 
            }
        }
        private static void EnsureDaoBaseAfterFoundation(Actor a)
        {
            var list = a.getTraits();
            if (list != null)
            {
                foreach (var t in list)
                {
                    if (t != null && t.group_id == RealmTraitGroup.GroupDaoBase && t.id != "dao_07_damaged")
                        return;
                }
            }
            string[] ids = {
                "dao_01_mortal",
                "dao_02_low",
                "dao_03_mid",
                "dao_04_high",
                "dao_05_supreme",
                "dao_06_tiandi"
            };
            int[] wghts = { 35, 27, 18, 12, 6, 1 };
            int sum = 0; for (int i = 0; i < wghts.Length; i++) sum += wghts[i];
            int r = UnityEngine.Random.Range(0, sum);
            string pick = "dao_01_mortal";
            for (int i = 0; i < ids.Length; i++)
            {
                if (r < wghts[i]) { pick = ids[i]; break; }
                r -= wghts[i];
            }
            var trait = AssetManager.traits.get(pick) as ActorTrait;
            if (trait != null) a.addTrait(trait);
        }
        private static readonly string[] DIVINE_IDS = new string[] {
            "divine_01_baonuzhibian", "divine_02_weiya", "divine_03_sanmeizhenhuo",
            "divine_04_wanjianguizong", "divine_05_xuankongpo", "divine_06_zhenkongquan",
            "divine_07_jiuyinbaiguzhao", "divine_08_duqidan", "divine_09_jianzhan"
        };
        private static readonly string[] ART_IDS = new string[] {
            "art_01_missile", "art_02_ascension", "art_03_slash", "art_04_quake", "art_05_waves",
            "art_06_convert", "art_07_palm", "art_08_breaker", "art_09_shield", "art_10_link"
        };
        private static int CountDivine(Actor a)
        {
            if (a == null) return 0;
            var list = a.getTraits();
            if (list == null) return 0;
            int count = 0;
            foreach (var t in list)
            {
                if (t == null || t.id == null) continue;
                if (t.id.StartsWith("divine_")) count++;
            }
            return count;
        }
        private static void GiveRandomDivine(Actor a)
        {
            if (a == null) return;
            var list = a.getTraits();
            if (list == null) return;
            HashSet<string> owned = new HashSet<string>();
            foreach (var t in list)
            {
                if (t != null && t.id != null && t.id.StartsWith("divine_"))
                    owned.Add(t.id);
            }
            List<string> available = new List<string>();
            foreach (var id in DIVINE_IDS)
            {
                if (!owned.Contains(id))
                    available.Add(id);
            }
            if (available.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, available.Count);
                var trait = AssetManager.traits.get(available[idx]) as ActorTrait;
                if (trait != null) a.addTrait(trait);
            }
        }
        private static void GiveRandomArt(Actor a)
        {
            if (a == null) return;
            var list = a.getTraits();
            if (list == null) return;
            HashSet<string> owned = new HashSet<string>();
            foreach (var t in list)
            {
                if (t != null && t.id != null && t.id.StartsWith("art_"))
                    owned.Add(t.id);
            }
            List<string> available = new List<string>();
            foreach (var id in ART_IDS)
            {
                if (!owned.Contains(id))
                    available.Add(id);
            }
            if (available.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, available.Count);
                var trait = AssetManager.traits.get(available[idx]) as ActorTrait;
                if (trait != null) a.addTrait(trait);
            }
        }
        private static void GiveRealmBreakthroughRewards(Actor a, int realmIndex)
        {
            if (a == null || !a.isAlive()) return;
            if (realmIndex == 5)
            {
                if (CountDivine(a) == 0)
                {
                    GiveRandomDivine(a);
                }
            }
            else if (realmIndex == 7)
            {
                if (CountDivine(a) == 1)
                {
                    GiveRandomDivine(a);
                }
            }
            else if (realmIndex == 10)
            {
                GiveRandomDivine(a);
                GiveRandomArt(a);
            }
            else if (realmIndex == 13)
            {
                GiveRandomDivine(a);
                GiveRandomArt(a);
            }
        }
        private static bool HasTrait(Actor a, string traitId)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list) if (t != null && t.id == traitId) return true;
            return false;
        }
        private static bool HasAnyFiveAttr(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
                if (t != null && t.group_id == RealmTraitGroup.GroupAttrFive)
                    return true;
            return false;
        }
        private static void MaybeGiveAttrForRealm(Actor a, int realmIdx)
        {
            if (a == null) return;
            string key = null; float chance = 0f;
            switch (realmIdx)
            {
                case 10: key = KEY_ATTR_TRY_11; chance = 0.50f; break; 
                case 11: key = KEY_ATTR_TRY_12; chance = 0.80f; break; 
                case 12: key = KEY_ATTR_TRY_13; chance = 1.00f; break; 
                default: return;
            }
            int tried; xn.access.ActorAccess.GetData(a).get(key, out tried, 0);
            if (tried == 1) return;     
            xn.access.ActorAccess.GetData(a).set(key, 1);         
            if (HasAnyFiveAttr(a)) return;
            if (!Randy.randomChance(chance)) return;
            int pick = UnityEngine.Random.Range(0, ATTR_IDS.Length);
            var tr = AssetManager.traits.get(ATTR_IDS[pick]) as ActorTrait;
            if (tr != null) a.addTrait(tr);
        }
        private static void IncreaseXinmoAndMaybeCorrupt(Actor a)
        {
            int xinmo; xn.access.ActorAccess.GetData(a).get(KEY_XINMO, out xinmo, 0);
            int beforeBucket = xinmo / 100;
            int delta = UnityEngine.Random.Range(1, 21); 
            xinmo += delta;
            xn.access.ActorAccess.GetData(a).set(KEY_XINMO, xinmo);
            if (HasTrait(a, "path_01_demonic")) return;
            int afterBucket = xinmo / 100;
            if (afterBucket > beforeBucket)
            {
                int steps = afterBucket - beforeBucket;
                for (int i = 0; i < steps; i++)
                {
                    if (UnityEngine.Random.value < 0.6f) 
                    {
                        var t = AssetManager.traits.get("path_01_demonic") as ActorTrait;
                        if (t != null) a.addTrait(t);
                        break; 
                    }
                }
            }
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class Patch_TickHeavenTrial
        {
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (__instance == null || __instance.units == null) return;
                var list = __instance.units.getSimpleList();
                if (list == null || list.Count == 0) return;
                float now = Time.time;
                int year = Date.getCurrentYear();
                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (a == null || !a.isAlive()) continue;
                    int active; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_ACTIVE, out active, 0);
                    if (active != 1) continue;
                    float endT; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_END_T, out endT, 0f);
                    if (now < endT)
                    {
                        if (!xn.access.BaseSimObjectAccess.HasStatus(a, "stunned"))
                        {
                            a.makeStunned(TRIAL_STUN_LARGE);
                        }
                        float nextLightning; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_NEXT_LIGHTNING, out nextLightning, 0f);
                        if (now >= nextLightning)
                        {
                            if (Randy.randomChance(0.50f))
                            {
                                SpawnTrialLightning(a);
                            }
                            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_NEXT_LIGHTNING, now + LIGHTNING_INTERVAL);
                        }
                        continue;
                    }
                    FinishHeavenTrial(a, year);
                }
            }
        }
        private static float GetTrialLightningScale(Actor a)
        {
            int ttype; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_TYPE, out ttype, 1);
            int target; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_TARGET, out target, 0);
            if (ttype == 1 || ttype == 2)
            {
                if (target == 13)
                {
                    long bridgeL; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_BRIDGE, out bridgeL, 0L);
                    int bridge = (int)bridgeL;
                    return 0.15f + bridge * 0.05f;
                }
                switch (target)
                {
                    case 6:  return 0.10f;  
                    case 9:  return 0.15f;  
                    case 12: return 0.20f;  
                    case 14: return 0.25f;  
                    default: return 0.10f;
                }
            }
            if (ttype == 3 || ttype == 4)
            {
                int curIdx = (ttype == 3) ? GetCurrentAncientIndex(a) : GetCurrentBeastIndex(a);
                return 0.08f + curIdx * 0.02f;
            }
            return 0.10f; 
        }
        private static void SpawnTrialLightning(Actor a)
        {
            if (a == null || a.current_tile == null) return;
            float scale = GetTrialLightningScale(a);
            if (scale >= 0.25f)
            {
                MapBox.spawnLightningBig(a.current_tile, scale, null);
            }
            else if (scale >= 0.15f)
            {
                MapBox.spawnLightningMedium(a.current_tile, scale, null);
            }
            else
            {
                MapBox.spawnLightningSmall(a.current_tile, scale, null);
            }
        }
        private static void FinishHeavenTrial(Actor a, int curYear)
        {
            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_ACTIVE, 0); 
            a.finishStatusEffect("stunned");
            int ttype; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_TYPE, out ttype, 0);
            int target; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_TARGET, out target, -1);
            Action clearRealmStop = () => xn.access.ActorAccess.GetData(a).set(KEY_STOP, 0);
            Action clearAncStop = () => xn.access.ActorAccess.GetData(a).set(KEY_ANC_STOP, 0);
            Action clearBeaStop = () => xn.access.ActorAccess.GetData(a).set(KEY_BEAST_STOP, 0);
            bool pass = false;
            if (ttype == 1 || ttype == 2)
            {
                float baseRate = (ttype == 1) ? 40f : 25f;
                float rootAdd = GetRootHeavenAdd(a) * ((ttype == 1) ? 0.4f : 0.6f);
                float daoAdd = GetDaoHeavenAdd(a) * ((ttype == 1) ? 0.4f : 0.6f);
                int wx; xn.access.ActorAccess.GetData(a).get(KEY_WUXIN, out wx, 0);
                int qy; xn.access.ActorAccess.GetData(a).get(KEY_QIYUN, out qy, 0);
                float wxAdd;
                if (ttype == 1)
                {
                    wxAdd = (wx >= 130) ? 22f :
                            (wx >= 120) ? 18f :
                            (wx >= 110) ? 14f :
                            (wx >= 100) ? 8f :
                            (wx >= 90) ? 6f :
                            (wx >= 80) ? 4f :
                            (wx >= 70) ? 2.5f :
                            (wx >= 20) ? 1.2f : 0f;
                }
                else
                {
                    wxAdd = (wx >= 100) ? 40f :
                            (wx >= 90) ? 32f :
                            (wx >= 80) ? 24f :
                            (wx >= 70) ? 16f :
                            (wx >= 20) ? 8f : 0f;
                    wxAdd *= 0.6f;
                }
                float luckAdd;
                if (ttype == 1)
                {
                    luckAdd = Mathf.Min((qy / 10) * 1f, 10f);
                }
                else
                {
                    luckAdd = Mathf.Min((qy / 10) * 3f, 30f);
                    luckAdd *= 0.6f;
                }
                int xinmo; xn.access.ActorAccess.GetData(a).get(KEY_XINMO, out xinmo, 0);
                float xinmoPenalty = (ttype == 1) ? xinmo * 0.3f : Mathf.Min(xinmo, 100) * 0.4f;
                float extraPenalty = (ttype == 2) ? 15f : 0f;
                float diff = 0f;
                if (target == 6) diff = 5f;   
                if (target == 9) diff = 10f;  
                if (target == 12) diff = 15f;  
                long bridgeL; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_BRIDGE, out bridgeL, 0L); int bridge = (int)bridgeL;
                if (target == 13)
                {
                    if (bridge <= 2) diff = 20f;      
                    else if (bridge <= 4) diff = 25f; 
                    else if (bridge <= 5) diff = 30f; 
                    else diff = 35f;                  
                }
                if (target == 14) diff = 25f; 
                float final = baseRate + rootAdd + daoAdd + wxAdd + luckAdd - xinmoPenalty - diff - extraPenalty;
                float minCap = (ttype == 1) ? 5f : 3f;
                float maxCap = (ttype == 1) ? 85f : 65f;
                final = Mathf.Clamp(final, minCap, maxCap);
                int isMainChar;
                xn.access.ActorAccess.GetData(a).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
                if (isMainChar == 1)
                {
                    pass = true; 
                }
                else
                {
                    pass = Randy.randomChance(final / 100f);
                }
                if (pass)
                {
                    if (target != 13) { BroadcastSystem.HeavenSuccess(a); }
                    if (ttype == 1) xn.access.ActorAccess.GetData(a).set(KEY_XINMO, 0);
                    if (target == 13) 
                    {
                        bridge++;
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_BRIDGE, (long)bridge);
                        if (bridge == 5)
                        {
                            ReplaceRealmTo(a, 14); 
                            BroadcastSystem.HeavenSuccessRealm(a, REALM_IDS[14]); 
                            xn.access.ActorAccess.GetData(a).set(KEY_BREAK_SUCCESS_YEAR, curYear);
                            if (UnityEngine.Random.value < 0.25f)
                            {
                                xn.access.ActorAccess.GetData(a).set(KEY_HALF_TATIAN_LOCKED, 1);
                                BroadcastSystem.PostActor(a, T("broadcast_half_tatian_locked", "{0} comprehended the limit of Heaven after breaking through Half-Step Heaven Trampling, cultivation permanently locked at this realm", a.getName()));
                            }
                            else
                            {
                                BroadcastSystem.PostActor(a, T("broadcast_half_tatian_continue", "{0} gained insight after breaking through Half-Step Heaven Trampling, can continue to attempt Heaven Trampling", a.getName()));
                            }
                        }
                        else if (bridge == 9)
                        {
                            ReplaceRealmTo(a, 15); 
                            BroadcastSystem.HeavenSuccessRealm(a, REALM_IDS[15]); 
                            GiveTatianAllPowersAndArts(a);
                            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_BRIDGE, 0); 
                            xn.access.ActorAccess.GetData(a).set(KEY_BREAK_SUCCESS_YEAR, curYear);
                        }
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                    }
                    else
                    {
                        int newIdx = Math.Min(target + 1, REALM_IDS.Length - 1);
                        ReplaceRealmTo(a, newIdx);
                        MaybeGiveAttrForRealm(a, newIdx);
                        BroadcastSystem.HeavenSuccessRealm(a, REALM_IDS[Mathf.Min(target + 1, REALM_IDS.Length - 1)]);
                        clearRealmStop();
                        xn.access.ActorAccess.GetData(a).set(KEY_BREAK_SUCCESS_YEAR, curYear);
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                    }
                }
                else
                {
                    if (target == 13)
                    {
                        if (bridge < 5 && Randy.randomChance(0.10f))
                        {
                            BroadcastSystem.PostActor(a, T("broadcast_bridge_dao_insight", "{0} gained Dao insight, so this is cultivation, bridge progress preserved", a.getName()));
                            clearRealmStop();
                            xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                    }
                    else
                    {
                            BroadcastSystem.PostActor(a, T("broadcast_bridge_fail", "{0} failed the {1} Heaven Trampling Bridge", a.getName(), Ordinal(bridge + 1)));
                    ApplyImmortalDemonicFailPenalty(a, ttype, target, xinmo);
                    clearRealmStop();
                    xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                        int reset = HasTrait(a, "realm_15_half_tatian") ? 5 : 0;
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_BRIDGE, (long)reset);
                        }
                    }
                    else
                    {
                        BroadcastSystem.HeavenFail(a);
                        ApplyImmortalDemonicFailPenalty(a, ttype, target, xinmo);
                        clearRealmStop();
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                    }
                }
            }
            else if (ttype == 3 || ttype == 4)
            {
                int wx; xn.access.ActorAccess.GetData(a).get(KEY_WUXIN, out wx, 0);
                int qy; xn.access.ActorAccess.GetData(a).get(KEY_QIYUN, out qy, 0);
                float baseRate = 50f;
                float wxAdd = (wx >= 100) ? 40f :
                              (wx >= 90) ? 32f :
                              (wx >= 80) ? 24f :
                              (wx >= 70) ? 16f :
                              (wx >= 20) ? 8f : 0f;
                float luckAdd = Mathf.Min((qy / 10) * 3f, 30f);
                int curIdx = (ttype == 3) ? GetCurrentAncientIndex(a) : GetCurrentBeastIndex(a);
                float diff = 0f;
                switch (curIdx)
                {
                    case 1: diff = 8f; break; 
                    case 2: diff = 12f; break; 
                    case 3: diff = 16f; break; 
                    case 4: diff = 20f; break; 
                    case 5: diff = 25f; break; 
                    case 6: diff = 30f; break; 
                    case 7: diff = 35f; break; 
                    case 8: diff = 40f; break; 
                }
                float final = Mathf.Clamp(baseRate + wxAdd + luckAdd - diff, 10f, 90f);
                int isMainChar;
                xn.access.ActorAccess.GetData(a).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
                if (isMainChar == 1)
                {
                    pass = true; 
                }
                else
                {
                    pass = Randy.randomChance(final / 100f);
                }
                if (pass)
                {
                    int targetIdx = curIdx + 1;
                    int maxAllowed = GetMaxAllowedStarStageIndex(a);
                    if (targetIdx > maxAllowed)
                    {
                        BroadcastSystem.HeavenFail(a);
                        ApplyAncientBeastFailPenalty(a, ttype, curIdx);
                        if (ttype == 3) clearAncStop(); else clearBeaStop();
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                    }
                    else
                    {
                        BroadcastSystem.HeavenSuccess(a);
                        if (ttype == 3) { PromoteAncientTo(a, targetIdx); clearAncStop(); }
                        else { PromoteBeastTo(a, targetIdx); clearBeaStop(); }
                        int newIdx = targetIdx;
                        if (newIdx + 1 >= 3)
                        {
                            if (ttype == 3)
                                BroadcastSystem.AncientUp(a, newIdx + 1);
                            else
                                BroadcastSystem.BeastUp(a, newIdx + 1);
                        }
                        xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                    }
                }
                else
                {
                    BroadcastSystem.HeavenFail(a);
                    ApplyAncientBeastFailPenalty(a, ttype, curIdx);
                    if (ttype == 3) clearAncStop(); else clearBeaStop();
                    xn.access.ActorAccess.GetData(a).set(KEY_TRIAL_COOLDOWN_UNTIL, curYear + TRIAL_COOLDOWN_YEARS);
                }
            }
            HeavenTrialFX.StopFor(a);
        }
        private static void ApplyImmortalDemonicFailPenalty(Actor a, int ttype, int targetRealmIndex, int xinmo)
        {
            int roll = UnityEngine.Random.Range(1, 7);
            switch (roll)
            {
                case 1: 
                    DemoteRealmBy(a, 3, 0.5f);
                    {
                        int toIdx = GetCurrentRealmIndex(a);
                        if (toIdx >= 0) BroadcastSystem.RealmFailDemote(a, REALM_IDS[toIdx]);
                    }
                    break;
                case 2: 
                    xn.access.ActorAccess.GetData(a).set(KEY_XINMO, xinmo + 30);
                    xn.access.ActorAccess.GetData(a).set("xn.tdao.try_cooldown_until", Date.getCurrentYear() + 3);
                    break;
                case 3: 
                    DowngradeDaoBase(a);
                    break;
                case 4: 
                    xn.access.BaseSimObjectAccess.GetStats(a)[strings.S.lifespan] = Mathf.Max(0f, xn.access.BaseSimObjectAccess.GetStats(a)[strings.S.lifespan] - 300f);
                    break;
                case 5: 
                    GiveOrReplaceTrait(a, "root_07_broken", xn.Traits.RealmTraitGroup.GroupSpiritRoot);
                    break;
                case 6: 
                    if (Randy.randomChance(0.40f))
                        a.addTrait("madness");
                    break;
            }
            if (ttype == 2 && xinmo > 200 && Randy.randomChance(0.10f))
            {
                a.dieAndDestroy(AttackType.Divine);
            }
        }
        private static void ApplyAncientBeastFailPenalty(Actor a, int ttype , int curIdx )
        {
            DemoteAncientOrBeast(a, ttype, 1, 0.5f);
            int roll = UnityEngine.Random.Range(2, 7);
            switch (roll)
            {
                case 2: 
                    xn.access.ActorAccess.GetData(a).set("xn.stat.wuxin", 1);
                    break;
                case 3: 
                    if (Randy.randomChance(0.40f))
                        a.addTrait("madness");
                    break;
                case 4: 
                    xn.access.ActorAccess.GetData(a).set("xn.seal_until", Date.getCurrentYear() + 100);
                    break;
                case 5: 
                    xn.access.ActorAccess.GetData(a).set("xn.stat.qiyun", 1);
                    ResetAncientOrBeastTo2(a, ttype);
                    break;
                case 6: 
                    xn.access.BaseSimObjectAccess.GetStats(a)[strings.S.lifespan] = Mathf.Max(0f, xn.access.BaseSimObjectAccess.GetStats(a)[strings.S.lifespan] - 500f);
                    break;
            }
            if (curIdx >= 6 && Randy.randomChance(0.10f))
            {
                a.dieAndDestroy(AttackType.Divine);
            }
            if (ttype == 4 && Randy.randomChance(0.10f))
                a.addTrait("madness");
        }
        private static void ReplaceRealmTo(Actor a, int idx)
        {
            var list = a.getTraits();
            if (list != null)
            {
                var toRemove = new List<ActorTrait>();
                for (int i = 0; i < REALM_IDS.Length; i++)
                    foreach (var t in list)
                        if (t != null && t.id == REALM_IDS[i])
                        {
                            toRemove.Add(t);
                            break;
                        }
                foreach (var t in toRemove)
                    a.removeTrait(t);
            }
            idx = Mathf.Clamp(idx, 0, REALM_IDS.Length - 1);
            var tr = AssetManager.traits.get(REALM_IDS[idx]) as ActorTrait;
            if (tr != null) a.addTrait(tr);
        }
        private static void DemoteRealmBy(Actor a, int levels, float percentOfThreshold)
        {
            int cur = GetCurrentRealmIndex(a);
            if (cur < 0) return;
            int to = Mathf.Max(cur - Mathf.Max(levels, 1), 0);
            ReplaceRealmTo(a, to);
            long cap = (to >= 0 && to < REALM_THRESHOLDS.Length) ? REALM_THRESHOLDS[to] : 0;
            long val = (long)(cap * Mathf.Clamp01(percentOfThreshold));
            xn.access.ActorAccess.GetData(a).set("xn.stat.xiuwei", val);
            xn.access.ActorAccess.GetData(a).set("xn.cultivation.stop", 0);
        }
        private static void GiveOrReplaceTrait(Actor a, string id, string groupId)
        {
            var list = a.getTraits();
            if (list != null && groupId != null)
            {
                var toRemove = new List<ActorTrait>();
                foreach (var t in list)
                    if (t != null && t.group_id == groupId)
                        toRemove.Add(t);
                foreach (var t in toRemove)
                    a.removeTrait(t);
            }
            var tr = AssetManager.traits.get(id) as ActorTrait;
            if (tr != null) a.addTrait(tr);
        }
        private static void DowngradeDaoBase(Actor a)
        {
            string[] order = {
        "dao_06_tiandi","dao_05_supreme","dao_04_high","dao_03_mid","dao_02_low","dao_01_mortal","dao_07_damaged"
    };
            int cur = -1;
            var list = a.getTraits();
            if (list != null)
            {
                for (int i = 0; i < order.Length; i++)
                    foreach (var t in list)
                        if (t != null && t.id == order[i]) { cur = i; break; }
            }
            if (cur == -1)
            {
                var tr = AssetManager.traits.get("dao_07_damaged") as ActorTrait;
                if (tr != null) a.addTrait(tr);
                return;
            }
            int to = Mathf.Min(cur + 1, order.Length - 1); 
            GiveOrReplaceTrait(a, order[to], xn.Traits.RealmTraitGroup.GroupDaoBase);
        }
        private static void ReplaceTraitInSet(Actor a, string[] ids, int pickIndex)
        {
            var list = a.getTraits();
            if (list != null)
            {
                var toRemove = new List<ActorTrait>();
                for (int i = 0; i < ids.Length; i++)
                    foreach (var t in list)
                        if (t != null && t.id == ids[i])
                        {
                            toRemove.Add(t);
                            break;
                        }
                foreach (var t in toRemove)
                    a.removeTrait(t);
            }
            pickIndex = Mathf.Clamp(pickIndex, 0, ids.Length - 1);
            var tr = AssetManager.traits.get(ids[pickIndex]) as ActorTrait;
            if (tr != null) a.addTrait(tr);
        }
        private static int GetCurrentBeastIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < BEAST_STAGE_IDS.Length; i++)
                foreach (var t in list)
                    if (t != null && t.id == BEAST_STAGE_IDS[i]) { if (i > cur) cur = i; }
            return cur;
        }
        private static void DemoteAncientOrBeast(Actor a, int ttype , int steps, float percentOfCap)
        {
            if (ttype == 3)
            {
                int cur = GetCurrentAncientIndex(a);
                if (cur <= 0) return;
                int to = Mathf.Max(cur - Mathf.Max(steps, 1), 0);
                ReplaceTraitInSet(a, ANC_STAR_IDS, to);
                long curP; xn.access.ActorAccess.GetData(a).get(KEY_ANC_POWER, out curP, 0L);
                long nextP = (long)(curP * Mathf.Clamp01(percentOfCap));
                xn.access.ActorAccess.GetData(a).set(KEY_ANC_POWER, nextP);
                xn.access.ActorAccess.GetData(a).set(KEY_ANC_STOP, 0);
            }
            else 
            {
                int cur = GetCurrentBeastIndex(a);
                if (cur <= 0) return;
                int to = Mathf.Max(cur - Mathf.Max(steps, 1), 0);
                ReplaceTraitInSet(a, BEAST_STAGE_IDS, to);
                long curP; xn.access.ActorAccess.GetData(a).get(KEY_BEAST_POWER, out curP, 0L);
                long nextP = (long)(curP * Mathf.Clamp01(percentOfCap));
                xn.access.ActorAccess.GetData(a).set(KEY_BEAST_POWER, nextP);
                xn.access.ActorAccess.GetData(a).set(KEY_BEAST_STOP, 0);
            }
        }
        private static void ResetAncientOrBeastTo2(Actor a, int ttype )
        {
            if (ttype == 3)
            {
                ReplaceTraitInSet(a, ANC_STAR_IDS, 1); 
                long curP; xn.access.ActorAccess.GetData(a).get(KEY_ANC_POWER, out curP, 0L);
                xn.access.ActorAccess.GetData(a).set(KEY_ANC_POWER, curP / 2);
                xn.access.ActorAccess.GetData(a).set(KEY_ANC_STOP, 0);
            }
            else
            {
                ReplaceTraitInSet(a, BEAST_STAGE_IDS, 1); 
                long curP; xn.access.ActorAccess.GetData(a).get(KEY_BEAST_POWER, out curP, 0L);
                xn.access.ActorAccess.GetData(a).set(KEY_BEAST_POWER, curP / 2);
                xn.access.ActorAccess.GetData(a).set(KEY_BEAST_STOP, 0);
            }
        }
        private static int GetMaxAllowedStarStageIndex(Actor a)
        {
            if (!xn.config.ModConfigHooks.EnableAncientBeastLevelLimit)
            {
                return 9; 
            }
            if (a == null || a.kingdom == null || a.kingdom.isRekt())
                return 0; 
            int kingdomLevel = xn.world.XiuzhenguoSystem.GetLevel(a.kingdom);
            switch (kingdomLevel)
            {
                case 0: return 0;  
                case 1: return 1;  
                case 2:
                case 3: return 1;  
                case 4:
                case 5: return 2;  
                case 6: return 3;  
                case 7: return 5;  
                case 8: return 6;  
                case 9: return 8;  
                case 10: return 9; 
                default: return 9; 
            }
        }
        private static void PromoteAncientTo(Actor a, int idx)
        {
            int maxAllowed = GetMaxAllowedStarStageIndex(a);
            int targetIdx = Mathf.Clamp(idx, 0, Mathf.Min(maxAllowed, ANC_STAR_IDS.Length - 1));
            ReplaceTraitInSet(a, ANC_STAR_IDS, targetIdx);
        }
        private static void PromoteBeastTo(Actor a, int idx)
        {
            int maxAllowed = GetMaxAllowedStarStageIndex(a);
            int targetIdx = Mathf.Clamp(idx, 0, Mathf.Min(maxAllowed, BEAST_STAGE_IDS.Length - 1));
            ReplaceTraitInSet(a, BEAST_STAGE_IDS, targetIdx);
        }
        private static int GetCurrentAncientIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < ANC_STAR_IDS.Length; i++)
                foreach (var t in list)
                    if (t != null && t.id == ANC_STAR_IDS[i]) { if (i > cur) cur = i; }
            return cur;
        }
        private static float GetRootHeavenAdd(Actor a)
        {
            if (HasTrait(a, "root_06_tiandi")) return 35f;
            if (HasTrait(a, "root_05_supreme")) return 28f;
            if (HasTrait(a, "root_04_high")) return 20f;
            if (HasTrait(a, "root_03_mid")) return 12f;
            if (HasTrait(a, "root_02_low")) return 5f;
            return 0f; 
        }
        private static float GetDaoHeavenAdd(Actor a)
        {
            if (HasTrait(a, "dao_06_tiandi")) return 30f;
            if (HasTrait(a, "dao_05_supreme")) return 24f;
            if (HasTrait(a, "dao_04_high")) return 18f;
            if (HasTrait(a, "dao_03_mid")) return 10f;
            if (HasTrait(a, "dao_02_low")) return 3f;
            if (HasTrait(a, "dao_01_mortal")) return -5f;
            return 0f;
        }
        private static void GiveTatianAllPowersAndArts(Actor a)
        {
            if (a == null || !a.isAlive())
                return;
            if (!a.hasTrait("realm_16_tatian"))
                return;
            int rewardedFlag; xn.access.ActorAccess.GetData(a).get("xn.tatian_rewarded", out rewardedFlag, 0);
            if (rewardedFlag == 1)
                return;
            var allTraits = AssetManager.traits.list; 
            for (int i = 0; i < allTraits.Count; i++)
            {
                ActorTrait t = allTraits[i];
                if (t == null)
                    continue;
                string id = t.id;
                if (string.IsNullOrEmpty(id))
                    continue;
                if (id.Length > 7 && id[0] == 'd' && id[1] == 'i' && id[2] == 'v' && id[3] == 'i' && id[4] == 'n' && id[5] == 'e' && id[6] == '_')
                {
                    a.addTrait(t);
                    continue;
                }
                if (id.Length > 4 && id[0] == 'a' && id[1] == 'r' && id[2] == 't' && id[3] == '_')
                {
                    a.addTrait(t);
                    continue;
                }
            }
            bool hasAnyIntent = false;
            for (int i = 0; i < allTraits.Count; i++)
            {
                ActorTrait t = allTraits[i];
                if (t == null) continue;
                string id = t.id;
                if (string.IsNullOrEmpty(id)) continue;
                if (id.Length > 7 && id[0] == 'i' && id[1] == 'n' && id[2] == 't' && id[3] == 'e' && id[4] == 'n' && id[5] == 't' && id[6] == '_')
                {
                    if (a.hasTrait(t)) { hasAnyIntent = true; break; }
                }
            }
            if (!hasAnyIntent)
            {
                string[] INTENTS_NON_EXTREME = new string[] {
                "intent_02_angel","intent_03_qianhuan","intent_04_killing","intent_05_reverse",
                "intent_06_life_death","intent_07_reincarnation","intent_08_chaos","intent_09_madness"
                };
                int idx = Randy.randomInt(0, INTENTS_NON_EXTREME.Length - 1);
                a.addTrait(INTENTS_NON_EXTREME[idx]); 
            }
            xn.access.ActorAccess.GetData(a).set("xn.tatian_rewarded", 1);
        }
        [HarmonyPatch(typeof(Actor), "addStatusEffect", new Type[] { typeof(StatusAsset), typeof(float), typeof(bool) })]
        private static class Patch_Actor_addStatusEffect_TrialProtection
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, StatusAsset pStatusAsset)
            {
                if (__instance == null || pStatusAsset == null) return true;
                if (pStatusAsset.id != "burning") return true;
                int active;
                xn.access.ActorAccess.GetData(__instance).get(KEY_TRIAL_ACTIVE, out active, 0);
                if (active != 1) return true; 
                int curHealth = xn.access.ActorAccess.GetData(__instance).health;
                int maxHealth = __instance.getMaxHealth();
                if (maxHealth <= 0) return true;
                float healthPercent = (float)curHealth / maxHealth;
                if (healthPercent <= 0.20f)
                {
                    return false; 
                }
                return true; 
            }
        }
        [HarmonyPatch(typeof(Actor), "dieAndDestroy")]
        private static class Patch_Actor_dieAndDestroy_TrialDeath
        {
            [HarmonyPrefix]
            private static void Prefix(Actor __instance)
            {
                if (__instance == null) return;
                int active;
                xn.access.ActorAccess.GetData(__instance).get(KEY_TRIAL_ACTIVE, out active, 0);
                if (active != 1) return;
                if (xn.access.ActorAccess.GetAttackedBy(__instance) == null)
                {
                    BroadcastSystem.PostActor(__instance, T("broadcast_tribulation_death", "{0} was killed by heavenly tribulation during breakthrough", __instance.getName()));
                }
                xn.access.ActorAccess.GetData(__instance).set(KEY_TRIAL_ACTIVE, 0);
                HeavenTrialFX.StopFor(__instance);
            }
        }
    }
}
