using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.Traits;
using xn.world;
namespace xn.bloodline
{
    public static class BloodlineSystem
    {
        private static bool _inited;
        private static Harmony _h;
        private static readonly string[] REALM_IDS = {
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
        private static readonly string[] ANC_STAR_IDS = {
            "ancient_01_star","ancient_02_star","ancient_03_star","ancient_04_star","ancient_05_star",
            "ancient_06_star","ancient_07_star","ancient_08_star","ancient_09_star","ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = {
            "beast_01_stage","beast_02_stage","beast_03_stage","beast_04_stage","beast_05_stage",
            "beast_06_stage","beast_07_stage","beast_08_stage","beast_09_stage","beast_10_stage"
        };
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _h = new Harmony("xn.bloodline");
            _h.PatchAll(typeof(BloodlineSystem));
            _h.PatchAll(typeof(Patch_UnitWindow_OnEnable_Bloodline));
            _h.PatchAll(typeof(Patch_Tooltip_ShowTooltip_Bloodline));
            BloodlineElectionSystem.Init();
            _h.PatchAll(typeof(Patch_MapBox_UpdateSimulation_Election));
            _h.PatchAll(typeof(Patch_Actor_AddTrait_BloodlineRestriction));
            BloodlineWindow.Init();
            BloodlineEffects.Init();
        }
        #region 血脉数据读写
        public static bool HasBloodline(Actor a)
        {
            if (a == null) return false;
            a.data.get(BloodlineDataKeys.KEY_TYPE, out string type, "");
            return !string.IsNullOrEmpty(type);
        }
        public static string GetBloodlineType(Actor a)
        {
            if (a == null) return "";
            a.data.get(BloodlineDataKeys.KEY_TYPE, out string type, "");
            return type;
        }
        public static float GetConcentration(Actor a)
        {
            if (a == null) return 0f;
            a.data.get(BloodlineDataKeys.KEY_CONCENTRATION, out float conc, 0f);
            return conc;
        }
        public static int GetGeneration(Actor a)
        {
            if (a == null) return 0;
            a.data.get(BloodlineDataKeys.KEY_GENERATION, out int gen, 0);
            return gen;
        }
        public static long GetFounderId(Actor a)
        {
            if (a == null) return -1;
            a.data.get(BloodlineDataKeys.KEY_FOUNDER_ID, out long id, -1);
            return id;
        }
        public static string GetFounderName(Actor a)
        {
            if (a == null) return "";
            a.data.get(BloodlineDataKeys.KEY_FOUNDER_NAME, out string name, "");
            return name;
        }
        public static bool IsFounder(Actor a)
        {
            if (a == null) return false;
            a.data.get(BloodlineDataKeys.KEY_IS_FOUNDER, out int isFounder, 0);
            return isFounder == 1;
        }
        public static bool IsAwakened(Actor a)
        {
            if (a == null) return false;
            a.data.get(BloodlineDataKeys.KEY_AWAKENED, out int awakened, 0);
            return awakened == 1;
        }
        public static bool IsAtavism(Actor a)
        {
            if (a == null) return false;
            a.data.get(BloodlineDataKeys.KEY_IS_ATAVISM, out int isAtavism, 0);
            return isAtavism == 1;
        }
        public static void SetBloodline(Actor a, string type, float concentration, int generation,
            long founderId, string founderName, bool isFounder = false, bool isAtavism = false)
        {
            if (a == null) return;
            a.data.set(BloodlineDataKeys.KEY_TYPE, type);
            a.data.set(BloodlineDataKeys.KEY_CONCENTRATION, concentration);
            a.data.set(BloodlineDataKeys.KEY_GENERATION, generation);
            a.data.set(BloodlineDataKeys.KEY_FOUNDER_ID, founderId);
            a.data.set(BloodlineDataKeys.KEY_FOUNDER_NAME, founderName);
            a.data.set(BloodlineDataKeys.KEY_IS_FOUNDER, isFounder ? 1 : 0);
            a.data.set(BloodlineDataKeys.KEY_IS_ATAVISM, isAtavism ? 1 : 0);
            a.data.set(BloodlineDataKeys.KEY_AWAKENED, 1);
            a.data.set(BloodlineDataKeys.KEY_AWAKENED_YEAR, Date.getCurrentYear());
            if (isFounder)
            {
                a.data.set(BloodlineDataKeys.KEY_FAMILY_CREATED_YEAR, Date.getCurrentYear());
            }
            if (a.hasClan())
            {
                a.data.set(BloodlineDataKeys.KEY_CLAN_ID, a.clan.getID());
            }
        }
        public static void UpdateConcentration(Actor a, float newConcentration)
        {
            if (a == null) return;
            a.data.set(BloodlineDataKeys.KEY_CONCENTRATION, Mathf.Clamp(newConcentration, 0f, 100f));
        }
        #endregion
        #region 境界检测
        public static int GetRealmIndex(Actor a)
        {
            if (a == null) return -1;
            var traits = a.getTraits();
            if (traits == null) return -1;
            int maxIdx = -1;
            foreach (var t in traits)
            {
                if (t == null) continue;
                for (int i = 0; i < REALM_IDS.Length; i++)
                {
                    if (t.id == REALM_IDS[i] && i > maxIdx)
                    {
                        maxIdx = i;
                    }
                }
            }
            return maxIdx;
        }
        public static int GetAncientStar(Actor a)
        {
            if (a == null) return 0;
            var traits = a.getTraits();
            if (traits == null) return 0;
            int maxStar = 0;
            foreach (var t in traits)
            {
                if (t == null || t.group_id != RealmTraitGroup.GroupAncientRealm) continue;
                for (int i = 0; i < ANC_STAR_IDS.Length; i++)
                {
                    if (t.id == ANC_STAR_IDS[i] && (i + 1) > maxStar)
                    {
                        maxStar = i + 1;
                    }
                }
            }
            return maxStar;
        }
        public static int GetBeastStage(Actor a)
        {
            if (a == null) return 0;
            var traits = a.getTraits();
            if (traits == null) return 0;
            int maxStage = 0;
            foreach (var t in traits)
            {
                if (t == null || t.group_id != RealmTraitGroup.GroupBeastStage) continue;
                for (int i = 0; i < BEAST_STAGE_IDS.Length; i++)
                {
                    if (t.id == BEAST_STAGE_IDS[i] && (i + 1) > maxStage)
                    {
                        maxStage = i + 1;
                    }
                }
            }
            return maxStage;
        }
        public static int GetCultivationType(Actor a)
        {
            if (a == null) return 0;
            if (GetAncientStar(a) > 0) return 3; 
            if (GetBeastStage(a) > 0) return 2;  
            if (GetRealmIndex(a) >= 0) return 1; 
            return 0; 
        }
        #endregion
        #region 血脉唯一性检查
        public static bool IsBloodlineOccupied(string bloodlineType)
        {
            if (string.IsNullOrEmpty(bloodlineType)) return false;
            if (BloodlineTypes.IsMutation(bloodlineType)) return false;
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || !a.isAlive()) continue;
                string type = GetBloodlineType(a);
                if (type == bloodlineType)
                {
                    return true; 
                }
            }
            return false; 
        }
        public static List<string> GetAvailableBloodlines(string[] bloodlinePool)
        {
            var available = new List<string>();
            if (bloodlinePool == null || bloodlinePool.Length == 0)
                return available;
            foreach (var type in bloodlinePool)
            {
                if (!IsBloodlineOccupied(type))
                {
                    available.Add(type);
                }
            }
            return available;
        }
        public static bool HasAvailableBloodline(string[] bloodlinePool)
        {
            return GetAvailableBloodlines(bloodlinePool).Count > 0;
        }
        #endregion
        #region 始祖诞生
        public static bool TryAwakeAsFounder(Actor a)
        {
            if (!xn.config.ModConfigHooks.EnableBloodlineAwaken) return false;
            if (a == null) return false;
            if (HasBloodline(a)) return false; 
            int isTianyunzi;
            a.data.get("xn_is_tianyunzi", out isTianyunzi, 0);
            if (isTianyunzi == 1) return false;
            int cultivationType = GetCultivationType(a);
            if (cultivationType == 0) return false; 
            float concentration = 0f;
            string[] bloodlinePool = null;
            bool shouldAwake = false;
            if (cultivationType == 1) 
            {
                int realmIdx = GetRealmIndex(a);
                if (realmIdx == 15) { concentration = 100f; shouldAwake = true; }
                else if (realmIdx == 14) { concentration = UnityEngine.Random.Range(85f, 95f); shouldAwake = true; }
                else if (realmIdx == 13) { concentration = UnityEngine.Random.Range(70f, 80f); shouldAwake = true; }
                else if (realmIdx == 12 && UnityEngine.Random.value < 0.5f) 
                { 
                    concentration = UnityEngine.Random.Range(55f, 65f); 
                    shouldAwake = true; 
                }
                else if (realmIdx == 11 && UnityEngine.Random.value < 0.2f) 
                { 
                    concentration = UnityEngine.Random.Range(30f, 50f); 
                    shouldAwake = true; 
                }
                bloodlinePool = BloodlineTypes.XIAN_MO_POOL;
            }
            else if (cultivationType == 2) 
            {
                int stage = GetBeastStage(a);
                if (stage == 10) { concentration = 100f; shouldAwake = true; }
                else if (stage == 9) { concentration = UnityEngine.Random.Range(85f, 95f); shouldAwake = true; }
                else if (stage == 8) { concentration = UnityEngine.Random.Range(70f, 80f); shouldAwake = true; }
                else if (stage == 7 && UnityEngine.Random.value < 0.5f) 
                { 
                    concentration = UnityEngine.Random.Range(55f, 65f); 
                    shouldAwake = true; 
                }
                bloodlinePool = BloodlineTypes.YAOSHOU_POOL;
            }
            else if (cultivationType == 3) 
            {
                int star = GetAncientStar(a);
                if (star == 10) { concentration = 100f; shouldAwake = true; }
                else if (star == 9) { concentration = UnityEngine.Random.Range(85f, 95f); shouldAwake = true; }
                else if (star == 8) { concentration = UnityEngine.Random.Range(70f, 80f); shouldAwake = true; }
                else if (star == 7 && UnityEngine.Random.value < 0.5f) 
                { 
                    concentration = UnityEngine.Random.Range(55f, 65f); 
                    shouldAwake = true; 
                }
                bloodlinePool = BloodlineTypes.GUSHEN_POOL;
            }
            if (!shouldAwake || bloodlinePool == null || bloodlinePool.Length == 0)
                return false;
            var availableBloodlines = GetAvailableBloodlines(bloodlinePool);
            if (availableBloodlines.Count == 0)
                return false;
            string bloodlineType = availableBloodlines[UnityEngine.Random.Range(0, availableBloodlines.Count)];
            SetBloodline(a, bloodlineType, concentration, 1, a.getID(), a.getName(), isFounder: true);
            int initialPurifyRealm = 0;
            if (cultivationType == 1) initialPurifyRealm = GetRealmIndex(a);
            else if (cultivationType == 2) initialPurifyRealm = GetBeastStage(a);
            else if (cultivationType == 3) initialPurifyRealm = GetAncientStar(a);
            a.data.set(BloodlineDataKeys.KEY_LAST_PURIFY_REALM, initialPurifyRealm);
            string typeName = BloodlineTypes.GetLocaleName(bloodlineType);
            XNHistoryRegistry.LogBroadcastForActor(a, $"{a.getName()} 证道成功，觉醒了 {typeName}，浓度 {concentration:F1}%！");
            TryGenerateChildAfterAwaken(a);
            return true;
        }
        private static void TryGenerateChildAfterAwaken(Actor founder)
        {
            if (founder == null || !founder.isAlive()) return;
            Actor baby = BabyMaker.makeBaby(founder, null, ActorSex.None, pCloneTraits: false,
                                            pMutationRate: 0, pTile: null, pAddToFamily: true);
            if (baby != null)
            {
                baby.addTrait("miracle_born");
                baby.data.set(BloodlineDataKeys.KEY_AWAKENED, 0);
                XNHistoryRegistry.LogBroadcastForActor(baby,
                    $"{founder.getName()} 证道后天赐麟儿，{baby.getName()} 诞生，继承了血脉！");
            }
        }
        public static void OnFounderBreakthrough(Actor founder, float oldConcentration, float newConcentration)
        {
            if (founder == null) return;
            if (!IsFounder(founder)) return;
            if (newConcentration <= oldConcentration) return;
            float boost = (newConcentration - oldConcentration) / 2f;
            long founderId = founder.getID();
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || !a.isAlive()) continue;
                if (a.getID() == founderId) continue; 
                if (GetFounderId(a) == founderId)
                {
                    float oldConc = GetConcentration(a);
                    float newConc = Mathf.Min(oldConc + boost, 100f);
                    UpdateConcentration(a, newConc);
                }
            }
        }
        #endregion
        #region 遗传与衰减
        public static void InheritBloodline(Actor baby, Actor parent1, Actor parent2)
        {
            if (baby == null) return;
            bool p1HasBlood = HasBloodline(parent1);
            bool p2HasBlood = parent2 != null && HasBloodline(parent2);
            if (!p1HasBlood && !p2HasBlood) return;
            string type1 = p1HasBlood ? GetBloodlineType(parent1) : "";
            string type2 = p2HasBlood ? GetBloodlineType(parent2) : "";
            float conc1 = p1HasBlood ? GetConcentration(parent1) : 0f;
            float conc2 = p2HasBlood ? GetConcentration(parent2) : 0f;
            string resultType;
            float resultConc;
            long founderId;
            string founderName;
            int generation;
            bool isAtavism = false;
            if (p1HasBlood && p2HasBlood && type1 == type2)
            {
                resultType = type1;
                resultConc = (conc1 + conc2) / 2f * 0.9f;
                founderId = GetFounderId(parent1);
                founderName = GetFounderName(parent1);
                generation = Mathf.Max(GetGeneration(parent1), GetGeneration(parent2)) + 1;
            }
            else if ((p1HasBlood && !p2HasBlood) || (!p1HasBlood && p2HasBlood))
            {
                Actor strongParent = p1HasBlood ? parent1 : parent2;
                float strongConc = p1HasBlood ? conc1 : conc2;
                resultType = GetBloodlineType(strongParent);
                if (strongConc > 80f)
                {
                    resultConc = strongConc * 0.8f;
                }
                else if (strongConc >= 50f)
                {
                    resultConc = strongConc * 0.8f;
                }
                else
                {
                    resultConc = strongConc / 10f;
                }
                founderId = GetFounderId(strongParent);
                founderName = GetFounderName(strongParent);
                generation = GetGeneration(strongParent) + 1;
            }
            else
            {
                if (UnityEngine.Random.value < 0.8f)
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        resultType = type1;
                        founderId = GetFounderId(parent1);
                        founderName = GetFounderName(parent1);
                        generation = GetGeneration(parent1) + 1;
                    }
                    else
                    {
                        resultType = type2;
                        founderId = GetFounderId(parent2);
                        founderName = GetFounderName(parent2);
                        generation = GetGeneration(parent2) + 1;
                    }
                    resultConc = (conc1 + conc2) / 8f;
                }
                else
                {
                    resultType = BloodlineTypes.MUTATION_POOL[
                        UnityEngine.Random.Range(0, BloodlineTypes.MUTATION_POOL.Length)];
                    resultConc = (conc1 + conc2) / 2f;
                    founderId = baby.getID(); 
                    founderName = baby.getName();
                    generation = 1;
                }
            }
            if (resultConc < 3f && UnityEngine.Random.value < 0.00001f)
            {
                resultConc = UnityEngine.Random.Range(90f, 100f);
                isAtavism = true;
                XNHistoryRegistry.LogBroadcastForActor(baby, $"奇迹！{baby.getName()} 发生返祖现象，血脉浓度重置为 {resultConc:F1}%！");
            }
            if (resultConc < 3f && !isAtavism)
            {
                return;
            }
            SetBloodline(baby, resultType, resultConc, generation, founderId, founderName, 
                isFounder: false, isAtavism: isAtavism);
        }
        #endregion
        #region 获取血脉数据列表
        public static List<Actor> GetAllBloodlineActors()
        {
            var result = new List<Actor>();
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || !a.isAlive()) continue;
                if (!HasBloodline(a)) continue;
                result.Add(a);
            }
            result.Sort((a, b) => GetConcentration(b).CompareTo(GetConcentration(a)));
            return result;
        }
        public static List<Actor> GetBloodlineDescendants(long founderId)
        {
            var result = new List<Actor>();
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || !a.isAlive()) continue;
                if (!HasBloodline(a)) continue;
                if (GetFounderId(a) != founderId) continue;
                result.Add(a);
            }
            result.Sort((a, b) => GetConcentration(b).CompareTo(GetConcentration(a)));
            return result;
        }
        public static List<Clan> GetBloodlineClans()
        {
            var result = new List<Clan>();
            var checkedClans = new HashSet<long>();
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || !a.isAlive()) continue;
                if (!HasBloodline(a)) continue;
                if (!a.hasClan()) continue;
                long clanId = a.clan.getID();
                if (checkedClans.Contains(clanId)) continue;
                checkedClans.Add(clanId);
                result.Add(a.clan);
            }
            return result;
        }
        public static List<Actor> GetClanBloodlineMembers(Clan clan)
        {
            var result = new List<Actor>();
            if (clan == null) return result;
            long clanId = clan.getID();
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || !a.isAlive()) continue;
                if (!a.hasClan() || a.clan.getID() != clanId) continue;
                if (!HasBloodline(a)) continue;
                result.Add(a);
            }
            return result;
        }
        public static string GetClanBloodlineSummary(Clan clan)
        {
            if (clan == null) return "";
            var members = GetClanBloodlineMembers(clan);
            if (members.Count == 0) return "无血脉成员";
            var typeCount = new Dictionary<string, int>();
            float maxConc = 0f;
            Actor founder = null;
            foreach (var a in members)
            {
                string type = GetBloodlineType(a);
                if (!typeCount.ContainsKey(type)) typeCount[type] = 0;
                typeCount[type]++;
                float conc = GetConcentration(a);
                if (conc > maxConc) maxConc = conc;
                if (IsFounder(a)) founder = a;
            }
            string mainType = "";
            int maxCount = 0;
            foreach (var kv in typeCount)
            {
                if (kv.Value > maxCount)
                {
                    maxCount = kv.Value;
                    mainType = kv.Key;
                }
            }
            string typeName = BloodlineTypes.GetLocaleName(mainType);
            string founderInfo = founder != null ? $"始祖：{founder.getName()}" : "";
            return $"{typeName} | 成员：{members.Count} | 最高浓度：{maxConc:F1}% | {founderInfo}";
        }
        #endregion
        #region Harmony Patch - 婴儿出生时继承血脉
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BabyMaker), nameof(BabyMaker.makeBaby))]
        private static void Patch_MakeBaby(Actor __result, Actor pParent1, Actor pParent2)
        {
            if (__result == null) return;
            InheritBloodline(__result, pParent1, pParent2);
        }
        #endregion
        #region Harmony Patch - 始祖觉醒（境界变化时检测）
        private static ConcurrentDictionary<long, int> _lastRealmChecked = new ConcurrentDictionary<long, int>();
        private static ConcurrentDictionary<long, int> _lastStarChecked = new ConcurrentDictionary<long, int>();
        private static ConcurrentDictionary<long, int> _lastStageChecked = new ConcurrentDictionary<long, int>();
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), nameof(Actor.updateStats))]
        private static void Patch_UpdateStats(Actor __instance)
        {
            if (__instance == null || !__instance.isAlive()) return;
            long actorId = __instance.getID();
            if (HasBloodline(__instance) && IsFounder(__instance))
            {
                CheckFounderBreakthrough(__instance, actorId);
                return;
            }
            if (!HasBloodline(__instance))
            {
                CheckAndTryAwake(__instance, actorId);
            }
        }
        private static void CheckFounderBreakthrough(Actor a, long actorId)
        {
            int cultivationType = GetCultivationType(a);
            a.data.get(BloodlineDataKeys.KEY_LAST_PURIFY_REALM, out int lastPurifyRealm, 0);
            if (cultivationType == 1) 
            {
                int currentRealm = GetRealmIndex(a);
                if (currentRealm > lastPurifyRealm && currentRealm >= 11) 
                {
                    float newConc = CalculateFounderConcentration(currentRealm, cultivationType);
                    float oldConc = GetConcentration(a);
                    if (newConc > oldConc)
                    {
                        UpdateConcentration(a, newConc);
                        a.data.set(BloodlineDataKeys.KEY_LAST_PURIFY_REALM, currentRealm);
                        OnFounderBreakthrough(a, oldConc, newConc);
                        string typeName = BloodlineTypes.GetLocaleName(GetBloodlineType(a));
                        XNHistoryRegistry.LogBroadcastForActor(a, $"{a.getName()} 境界突破，{typeName}浓度提升至 {newConc:F1}%！后代血脉得到提纯。");
                    }
                }
            }
            else if (cultivationType == 2) 
            {
                int currentStage = GetBeastStage(a);
                if (currentStage > lastPurifyRealm && currentStage >= 7)
                {
                    float newConc = CalculateFounderConcentration(currentStage, cultivationType);
                    float oldConc = GetConcentration(a);
                    if (newConc > oldConc)
                    {
                        UpdateConcentration(a, newConc);
                        a.data.set(BloodlineDataKeys.KEY_LAST_PURIFY_REALM, currentStage);
                        OnFounderBreakthrough(a, oldConc, newConc);
                    }
                }
            }
            else if (cultivationType == 3) 
            {
                int currentStar = GetAncientStar(a);
                if (currentStar > lastPurifyRealm && currentStar >= 7)
                {
                    float newConc = CalculateFounderConcentration(currentStar, cultivationType);
                    float oldConc = GetConcentration(a);
                    if (newConc > oldConc)
                    {
                        UpdateConcentration(a, newConc);
                        a.data.set(BloodlineDataKeys.KEY_LAST_PURIFY_REALM, currentStar);
                        OnFounderBreakthrough(a, oldConc, newConc);
                    }
                }
            }
        }
        private static float CalculateFounderConcentration(int level, int cultivationType)
        {
            if (cultivationType == 1) 
            {
                if (level >= 15) return 100f; 
                if (level == 14) return 92f; 
                if (level == 13) return 78f; 
                if (level == 12) return 62f; 
                if (level == 11) return 42f; 
            }
            else 
            {
                if (level >= 10) return 100f;
                if (level == 9) return 92f;
                if (level == 8) return 78f;
                if (level == 7) return 62f;
            }
            return 0f;
        }
        private static void CheckAndTryAwake(Actor a, long actorId)
        {
            int cultivationType = GetCultivationType(a);
            if (cultivationType == 0) return; 
            bool shouldCheck = false;
            if (cultivationType == 1)
            {
                int currentRealm = GetRealmIndex(a);
                _lastRealmChecked.TryGetValue(actorId, out int lastRealm);
                if (currentRealm != lastRealm && currentRealm >= 11)
                {
                    _lastRealmChecked[actorId] = currentRealm;
                    shouldCheck = true;
                }
            }
            else if (cultivationType == 2)
            {
                int currentStage = GetBeastStage(a);
                _lastStageChecked.TryGetValue(actorId, out int lastStage);
                if (currentStage != lastStage && currentStage >= 7)
                {
                    _lastStageChecked[actorId] = currentStage;
                    shouldCheck = true;
                }
            }
            else if (cultivationType == 3)
            {
                int currentStar = GetAncientStar(a);
                _lastStarChecked.TryGetValue(actorId, out int lastStar);
                if (currentStar != lastStar && currentStar >= 7)
                {
                    _lastStarChecked[actorId] = currentStar;
                    shouldCheck = true;
                }
            }
            if (shouldCheck)
            {
                TryAwakeAsFounder(a);
            }
        }
        public static void CleanupDeadActors()
        {
            var deadIds = new List<long>();
            foreach (var kv in _lastRealmChecked)
            {
                var actor = World.world.units.get(kv.Key);
                if (actor == null || !actor.isAlive())
                {
                    deadIds.Add(kv.Key);
                }
            }
            foreach (var id in deadIds)
            {
                _lastRealmChecked.TryRemove(id, out _);
                _lastStarChecked.TryRemove(id, out _);
                _lastStageChecked.TryRemove(id, out _);
            }
        }
        #endregion
    }
}