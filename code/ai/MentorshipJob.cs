using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ai;
namespace xn.world
{
    public static class MentorshipJob
    {
        const string KEY_MASTER_ID = "xn_men_master_id";           
        const string KEY_DISCIPLES_IDS = "xn_men_disciples_ids";   
        const string KEY_SCAN_NEXT_YEAR = "xn_men_scan_next_year"; 
        const string KEY_SCAN_COOLDOWN_UNTIL = "xn_men_scan_cd";   
        const string KEY_REVENGE_TARGET = "xn_men_revenge_id";     
        const string KEY_TRANS_NEXT_YEAR = "xn_men_trans_next";    
        const string KEY_CONSUME_NEXT_YEAR = "xn_men_consume_next"; 
        const string KEY_XP = "xn.stat.xiuwei";                    
        const string KEY_LINGLI = "xn.stat.lingli";                
        const float PROB_TAKE_DISCIPLE = 0.5f;        
        const int SCAN_COOLDOWN_MIN = 5;              
        const int SCAN_COOLDOWN_MAX = 20;              
        const int TRANS_COOLDOWN_MIN = 10;            
        const int TRANS_COOLDOWN_MAX = 30;            
        const int TRANS_MIN_LINGLI = 1000;            
        const float TRANS_MULTIPLIER_MIN = 1f;        
        const float TRANS_MULTIPLIER_MAX = 10f;       
        const float PROB_MASTER_REVENGE = 1.0f;       
        const float PROB_DISCIPLE_REVENGE = 0.5f;     
        const float PROB_CONSUME = 0.3f;              
        const float PROB_REBELLION = 0.15f;           
        const int CONSUME_INTERVAL_YEARS = 20;        
        const float CONSUME_LIFE_BONUS = 0.2f;        
        const float REBELLION_XP_GAIN = 0.5f;         
        static readonly string[] REALM_IDS = {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        static readonly Dictionary<string, int> REALM_INDEX_MAP = new Dictionary<string, int>();
        static readonly HashSet<long> s_mentorshipUnits = new HashSet<long>();
        static float s_nextCacheRefresh = 0f;
        const float CACHE_REFRESH_INTERVAL = 10f; 
        static readonly string[] ROOT_IDS = {
            "root_01_mortal", "root_02_low", "root_03_mid", 
            "root_04_high", "root_05_supreme", "root_06_tiandi"
        };
        static readonly string[] KEY_TRAITS = {
            "genius", "lucky", "ambitious"
        };
        static readonly string[] DEMONIC_TRAITS = {
            "evil", "bloodthirsty", "path_01_demonic"
        };
        public static void Init(Harmony harmony)
        {
            if (REALM_INDEX_MAP.Count == 0)
            {
                for (int i = 0; i < REALM_IDS.Length; i++)
                    REALM_INDEX_MAP[REALM_IDS[i]] = i;
            }
            harmony.PatchAll(typeof(Hook_MapBox_updateSimulation));
            harmony.PatchAll(typeof(Hook_Actor_die));
        }
        static int GetRealmIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            foreach (var tr in a.traits)
            {
                if (tr == null || string.IsNullOrEmpty(tr.id)) continue;
                int i;
                if (REALM_INDEX_MAP.TryGetValue(tr.id, out i) && i > idx)
                    idx = i;
            }
            return idx;
        }
        static bool IsCultivatorRealmOnly(Actor a)
        {
            bool hasRealm = a.hasAnyTraitWithPrefix("realm_");
            if (!hasRealm) return false;
            if (a.hasAnyTraitWithPrefix("ancient_")) return false;
            if (a.hasAnyTraitWithPrefix("beast_")) return false;
            return true;
        }
        static int GetRootQualityIndex(Actor a)
        {
            if (a == null) return -1;
            int bestIdx = -1;
            foreach (var tr in a.traits)
            {
                if (tr == null || string.IsNullOrEmpty(tr.id)) continue;
                for (int i = 0; i < ROOT_IDS.Length; i++)
                    if (tr.id == ROOT_IDS[i] && i > bestIdx) bestIdx = i;
            }
            return bestIdx;
        }
        static bool HasKeyTrait(Actor a)
        {
            if (a == null) return false;
            foreach (var tr in a.traits)
            {
                if (tr == null || string.IsNullOrEmpty(tr.id)) continue;
                foreach (var key in KEY_TRAITS)
                    if (tr.id == key) return true;
            }
            return false;
        }
        static bool IsDemonic(Actor a)
        {
            if (a == null) return false;
            foreach (var tr in a.traits)
            {
                if (tr == null || string.IsNullOrEmpty(tr.id)) continue;
                foreach (var dem in DEMONIC_TRAITS)
                    if (tr.id == dem) return true;
            }
            return false;
        }
        static bool HasCruelTrait(Actor a)
        {
            return a != null && a.hasTrait("cruel");
        }
        static bool HasPureTrait(Actor a)
        {
            return a != null && a.hasTrait("pure");
        }
        static int GetMaxDisciples(Actor master)
        {
            int realm = GetRealmIndex(master);
            if (realm < 1) return 0; 
            return realm;
        }
        static int GetDisciplesCount(Actor master)
        {
            string idsStr;
            master.data.get(KEY_DISCIPLES_IDS, out idsStr, "");
            if (string.IsNullOrEmpty(idsStr)) return 0;
            string[] parts = idsStr.Split(',');
            int count = 0;
            foreach (var part in parts)
            {
                if (long.TryParse(part.Trim(), out long id) && id > 0)
                {
                    var d = World.world.units.get(id);
                    if (d != null && !d.isRekt()) count++;
                }
            }
            return count;
        }
        static List<long> GetDisciplesList(Actor master)
        {
            List<long> list = new List<long>();
            string idsStr;
            master.data.get(KEY_DISCIPLES_IDS, out idsStr, "");
            if (string.IsNullOrEmpty(idsStr)) return list;
            string[] parts = idsStr.Split(',');
            foreach (var part in parts)
            {
                if (long.TryParse(part.Trim(), out long id) && id > 0)
                {
                    var d = World.world.units.get(id);
                    if (d != null && !d.isRekt()) list.Add(id);
                }
            }
            return list;
        }
        static void AddDisciple(Actor master, Actor disciple)
        {
            List<long> list = GetDisciplesList(master);
            if (!list.Contains(disciple.data.id))
            {
                list.Add(disciple.data.id);
                master.data.set(KEY_DISCIPLES_IDS, string.Join(",", list));
            }
            disciple.data.set(KEY_MASTER_ID, master.data.id);
            s_mentorshipUnits.Add(master.data.id);
            s_mentorshipUnits.Add(disciple.data.id);
        }
        static void RemoveDisciple(Actor master, long discipleId)
        {
            List<long> list = GetDisciplesList(master);
            list.Remove(discipleId);
            master.data.set(KEY_DISCIPLES_IDS, list.Count > 0 ? string.Join(",", list) : "");
            if (list.Count == 0)
            {
                long masterId;
                master.data.get(KEY_MASTER_ID, out masterId, 0L);
                if (masterId <= 0) s_mentorshipUnits.Remove(master.data.id);
            }
        }
        static int s_tick_index = 0; 
        const int UNITS_PER_FRAME = 10; 
        static void RefreshMentorshipCache()
        {
            s_mentorshipUnits.Clear();
            var allUnits = World.world.units.getSimpleList();
            for (int i = 0; i < allUnits.Count; i++)
            {
                var a = allUnits[i];
                if (a == null || !a.isAlive()) continue;
                long masterId;
                a.data.get(KEY_MASTER_ID, out masterId, 0L);
                if (masterId > 0)
                {
                    s_mentorshipUnits.Add(a.data.id);
                    continue;
                }
                string idsStr;
                a.data.get(KEY_DISCIPLES_IDS, out idsStr, "");
                if (!string.IsNullOrEmpty(idsStr))
                {
                    s_mentorshipUnits.Add(a.data.id);
                }
            }
        }
        static void Tick(float pElapsed)
        {
            if (!xn.config.ModConfigHooks.EnableMentorship) return;
            int curYear = Date.getCurrentYear();
            float now = UnityEngine.Time.time;
            if (now >= s_nextCacheRefresh)
            {
                s_nextCacheRefresh = now + CACHE_REFRESH_INTERVAL;
                RefreshMentorshipCache();
            }
            List<Actor> allUnits = World.world.units.getSimpleList();
            int len = allUnits.Count;
            if (len == 0)
            {
                s_tick_index = 0;
                return;
            }
            if (s_tick_index >= len)
            {
                s_tick_index = 0;
            }
            int processed = 0;
            int startIndex = s_tick_index;
            while (processed < UNITS_PER_FRAME && processed < len)
            {
                Actor a = allUnits[s_tick_index];
                if (a != null && a.isAlive() && !a.isInsideSomething() && a.isSapient() && IsCultivatorRealmOnly(a))
                {
                    int nextYear;
                    a.data.get(KEY_SCAN_NEXT_YEAR, out nextYear, 0);
                    bool isInCooldown = nextYear > curYear;
                    if (!isInCooldown || GetDisciplesCount(a) < GetMaxDisciples(a))
                    {
                        TryTakeDisciple(a, curYear);
                    }
                    if (!isInCooldown)
                    {
                        TryTransmitPower(a, curYear);
                    }
                    if (!isInCooldown && IsDemonic(a))
                    {
                        TryConsumeDisciple(a, curYear);
                    }
                }
                s_tick_index = (s_tick_index + 1) % len;
                processed++;
                if (s_tick_index == startIndex && processed > 0)
                    break;
            }
        }
        static void TryTakeDisciple(Actor master, int curYear)
        {
            if (!IsCultivatorRealmOnly(master)) return;
            int masterRealm = GetRealmIndex(master);
            if (masterRealm < 1) return; 
            int nextYear;
            master.data.get(KEY_SCAN_NEXT_YEAR, out nextYear, 0);
            if (nextYear > curYear) return;
            int cooldownUntil;
            master.data.get(KEY_SCAN_COOLDOWN_UNTIL, out cooldownUntil, 0);
            if (cooldownUntil > curYear) return;
            int maxDisc = GetMaxDisciples(master);
            int curDisc = GetDisciplesCount(master);
            if (curDisc >= maxDisc)
            {
                master.data.set(KEY_SCAN_NEXT_YEAR, curYear + 10);
                return;
            }
            if (master.has_attack_target) return;
            Actor candidate = FindDiscipleCandidate(master, masterRealm);
            if (candidate == null)
            {
                master.data.set(KEY_SCAN_NEXT_YEAR, curYear + 10);
                return;
            }
            int cooldown = Randy.randomInt(SCAN_COOLDOWN_MIN, SCAN_COOLDOWN_MAX + 1);
            if (!Randy.randomChance(PROB_TAKE_DISCIPLE))
            {
                master.data.set(KEY_SCAN_COOLDOWN_UNTIL, curYear + cooldown);
                master.data.set(KEY_SCAN_NEXT_YEAR, curYear + cooldown);
                return;
            }
            AddDisciple(master, candidate);
            BroadcastSystem.MentorshipTake(master, candidate);
            master.data.set(KEY_SCAN_NEXT_YEAR, curYear + cooldown);
            candidate.data.set(KEY_SCAN_NEXT_YEAR, curYear + cooldown);
        }
        static Actor FindDiscipleCandidate(Actor master, int masterRealm)
        {
            if (master.current_tile == null) return null;
            List<Actor> candidates = new List<Actor>();
            const int SEARCH_RADIUS_TILES = 25; 
            const int SEARCH_RADIUS_CHUNKS = 2; 
            foreach (var candidate in Finder.getUnitsFromChunk(master.current_tile, SEARCH_RADIUS_CHUNKS, SEARCH_RADIUS_TILES))
            {
                if (IsValidCandidate(master, candidate, masterRealm))
                {
                    candidates.Add(candidate);
                }
            }
            if (candidates.Count == 0) return null;
            candidates.Sort((a, b) => CompareCandidates(master, a, b));
            return candidates[0];
        }
        static bool IsValidCandidate(Actor master, Actor candidate, int masterRealm)
        {
            if (candidate == null || !candidate.isAlive() || candidate == master) return false;
            if (candidate.isInsideSomething() || !candidate.isSapient()) return false;
            if (!IsCultivatorRealmOnly(candidate)) return false;
            int candRealm = GetRealmIndex(candidate);
            if (candRealm < 0) return false; 
            if (masterRealm - candRealm < 1) return false; 
            long masterId;
            candidate.data.get(KEY_MASTER_ID, out masterId, 0L);
            if (masterId > 0) return false;
            List<long> discList = GetDisciplesList(candidate);
            if (discList.Count > 0) return false;
            return true;
        }
        static int CompareCandidates(Actor master, Actor a, Actor b)
        {
            bool isDemonic = IsDemonic(master);
            int rootA = GetRootQualityIndex(a);
            int rootB = GetRootQualityIndex(b);
            if (rootA != rootB) return rootB - rootA; 
            bool keyA = HasKeyTrait(a);
            bool keyB = HasKeyTrait(b);
            if (keyA != keyB) return keyB ? 1 : -1;
            if (isDemonic)
            {
                bool cruelA = HasCruelTrait(a);
                bool cruelB = HasCruelTrait(b);
                if (cruelA != cruelB) return cruelB ? 1 : -1;
                bool pureA = HasPureTrait(a);
                bool pureB = HasPureTrait(b);
                if (pureA != pureB) return pureB ? 1 : -1; 
            }
            return 0; 
        }
        static void TryTransmitPower(Actor master, int curYear)
        {
            List<long> discList = GetDisciplesList(master);
            if (discList.Count == 0) return;
            int nextYear;
            master.data.get(KEY_TRANS_NEXT_YEAR, out nextYear, 0);
            if (nextYear > curYear) return;
            int lingli;
            master.data.get(KEY_LINGLI, out lingli, 0);
            if (lingli < TRANS_MIN_LINGLI) return;
            int masterRealm = GetRealmIndex(master);
            if (masterRealm < 0) return; 
            List<long> validDiscList = new List<long>();
            foreach (var discId in discList)
            {
                var disc = World.world.units.get(discId);
                if (disc == null || disc.isRekt()) continue;
                int discRealm = GetRealmIndex(disc);
                if (discRealm >= 0 && discRealm < masterRealm)
                {
                    validDiscList.Add(discId);
                }
            }
            if (validDiscList.Count == 0) return;
            int count = Randy.randomInt(1, validDiscList.Count + 1);
            List<long> selected = new List<long>();
            List<long> tempList = new List<long>(validDiscList);
            for (int i = 0; i < count; i++)
            {
                if (tempList.Count == 0) break;
                int idx = Randy.randomInt(0, tempList.Count);
                selected.Add(tempList[idx]);
                tempList.RemoveAt(idx);
            }
            float multiplier = Randy.randomFloat(TRANS_MULTIPLIER_MIN, TRANS_MULTIPLIER_MAX);
            long xpPerDisciple = (long)(lingli * multiplier / selected.Count);
            master.data.set(KEY_LINGLI, 0);
            foreach (var discId in selected)
            {
                var disc = World.world.units.get(discId);
                if (disc != null && !disc.isRekt())
                {
                    long curXp;
                    disc.data.get(KEY_XP, out curXp, 0L);
                    disc.data.set(KEY_XP, curXp + xpPerDisciple);
                    BroadcastSystem.MentorshipTrans(master, disc, xpPerDisciple);
                }
            }
            int cooldown = Randy.randomInt(TRANS_COOLDOWN_MIN, TRANS_COOLDOWN_MAX + 1);
            master.data.set(KEY_TRANS_NEXT_YEAR, curYear + cooldown);
        }
        static void TryConsumeDisciple(Actor master, int curYear)
        {
            List<long> discList = GetDisciplesList(master);
            if (discList.Count == 0) return;
            int nextYear;
            master.data.get(KEY_CONSUME_NEXT_YEAR, out nextYear, 0);
            if (nextYear > curYear) return;
            int age = master.getAge();
            float lifespan = master.stats["lifespan"];
            if (lifespan <= 0f || age < lifespan * 0.8f) return; 
            if (!Randy.randomChance(PROB_CONSUME)) return;
            int idx = Randy.randomInt(0, discList.Count);
            long discId = discList[idx];
            var disc = World.world.units.get(discId);
            if (disc == null || disc.isRekt()) return;
            if (Randy.randomChance(PROB_REBELLION))
            {
                long masterXp;
                master.data.get(KEY_XP, out masterXp, 0L);
                long discXp;
                disc.data.get(KEY_XP, out discXp, 0L);
                disc.data.set(KEY_XP, discXp + (long)(masterXp * REBELLION_XP_GAIN));
                BroadcastSystem.Custom(disc.getName() + " 弑师证道，获得 " + master.getName() + " 一半修为");
                RemoveDisciple(master, discId);
                disc.data.set(KEY_MASTER_ID, 0L);
                master.die(pDestroy: false, AttackType.Other, pCountDeath: true, pLogFavorite: true);
                return;
            }
            long masterXp2;
            master.data.get(KEY_XP, out masterXp2, 0L);
            long discXp2;
            disc.data.get(KEY_XP, out discXp2, 0L);
            master.data.set(KEY_XP, masterXp2 + discXp2);
            IncreaseLifespan(master, CONSUME_LIFE_BONUS);
            BroadcastSystem.MentorshipConsume(master);
            RemoveDisciple(master, discId);
            disc.die();
            master.data.set(KEY_CONSUME_NEXT_YEAR, curYear + CONSUME_INTERVAL_YEARS);
        }
        static void IncreaseLifespan(Actor actor, float bonusPercent)
        {
            if (actor == null || actor.isRekt()) return;
            float currentLifespan = actor.stats["lifespan"];
            if (currentLifespan <= 0f) return;
            float lifespanBonus = currentLifespan * bonusPercent;
            ActorTrait lifespanTrait = new ActorTrait
            {
                id = "xn_temp_lifespan_bonus_" + actor.data.id, 
                path_icon = "zhanwei", 
                base_stats = new BaseStats()
            };
            lifespanTrait.base_stats["lifespan"] = lifespanBonus;
            actor.addTrait(lifespanTrait, pRemoveOpposites: false);
            actor.setStatsDirty();
        }
        [HarmonyPatch(typeof(MapBox), "updateSimulation")]
        static class Hook_MapBox_updateSimulation
        {
            static void Postfix(float pElapsed)
            {
                Tick(pElapsed);
            }
        }
        [HarmonyPatch(typeof(Actor), "die")]
        static class Hook_Actor_die
        {
            static void Postfix(Actor __instance)
            {
                if (__instance == null || !__instance.isRekt()) return;
                long masterId;
                __instance.data.get(KEY_MASTER_ID, out masterId, 0L);
                if (masterId > 0)
                {
                    Actor master = World.world.units.get(masterId);
                    if (master != null && !master.isRekt() && master.current_tile != null && __instance.current_tile != null && master.current_tile.isSameIsland(__instance.current_tile))
                    {
                        long killerId = __instance.attackedBy?.a?.data.id ?? 0L;
                        if (killerId > 0)
                        {
                            var killer = World.world.units.get(killerId);
                            if (killer != null && !killer.isRekt())
                            {
                                int killerRealm = GetRealmIndex(killer);
                                int masterRealm = GetRealmIndex(master);
                                if (killerRealm <= masterRealm)
                                {
                                    master.data.set(KEY_REVENGE_TARGET, killerId);
                                    master.setAttackTarget(killer);
                                    BroadcastSystem.MentorshipVow(master);
                                }
                            }
                        }
                    }
                }
                List<long> discList = GetDisciplesList(__instance);
                if (discList.Count > 0)
                {
                    long killerId2 = __instance.attackedBy?.a?.data.id ?? 0L;
                    Actor inheritor = null;
                    long maxXp = -1;
                    foreach (var discId in discList)
                    {
                        var disc = World.world.units.get(discId);
                        if (disc == null || disc.isRekt()) continue;
                        if (killerId2 > 0 && Randy.randomChance(PROB_DISCIPLE_REVENGE))
                        {
                            var killer = World.world.units.get(killerId2);
                            if (killer != null && !killer.isRekt())
                            {
                                int killerRealm = GetRealmIndex(killer);
                                int discRealm = GetRealmIndex(disc);
                                if (killerRealm <= discRealm)
                                {
                                    disc.data.set(KEY_REVENGE_TARGET, killerId2);
                                    disc.setAttackTarget(killer);
                                }
                            }
                        }
                        if (IsDemonic(__instance) && killerId2 > 0 && Randy.randomChance(0.3f))
                        {
                            var killer = World.world.units.get(killerId2);
                            if (killer != null && killer.city != null)
                            {
                            }
                        }
                        long discXp;
                        disc.data.get(KEY_XP, out discXp, 0L);
                        if (discXp > maxXp)
                        {
                            maxXp = discXp;
                            inheritor = disc;
                        }
                        RemoveDisciple(__instance, discId);
                        disc.data.set(KEY_MASTER_ID, 0L);
                    }
                    if (inheritor != null)
                    {
                        int shiLing;
                        __instance.data.get("xn.stat.shiling", out shiLing, 0);
                        int jiPinShiLing;
                        __instance.data.get("xn.stat.jipinshiling", out jiPinShiLing, 0);
                        int inheritorShiLing;
                        inheritor.data.get("xn.stat.shiling", out inheritorShiLing, 0);
                        int inheritorJiPin;
                        inheritor.data.get("xn.stat.jipinshiling", out inheritorJiPin, 0);
                        inheritor.data.set("xn.stat.shiling", inheritorShiLing + shiLing);
                        inheritor.data.set("xn.stat.jipinshiling", inheritorJiPin + jiPinShiLing);
                        BroadcastSystem.Custom(inheritor.getName() + " 继承了 " + __instance.getName() + " 的所有灵石");
                    }
                    BroadcastSystem.Custom(__instance.getName() + " 陨落，其徒弟发誓将来复仇");
                }
            }
        }
    }
    static class MentorExt
    {
        public static bool hasAnyTraitWithPrefix(this Actor a, string prefix)
        {
            if (a == null) return false;
            foreach (var tr in a.traits)
            {
                if (tr == null) continue;
                if (string.IsNullOrEmpty(tr.id)) continue;
                if (tr.id.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }
}