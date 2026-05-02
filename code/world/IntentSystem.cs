using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ai.behaviours;
namespace cultivation
{
    internal static class IntentSystem
    {
        private static bool s_patched = false;
        public const string INTENT_EXTREME       = "intent_01_extreme";
        public const string INTENT_ANGEL         = "intent_02_angel";
        public const string INTENT_QIANHUAN      = "intent_03_qianhuan";
        public const string INTENT_KILLING       = "intent_04_killing";
        public const string INTENT_REVERSE       = "intent_05_reverse";
        public const string INTENT_LIFE_DEATH    = "intent_06_life_death";
        public const string INTENT_REINCARNATION = "intent_07_reincarnation";
        public const string INTENT_CHAOS         = "intent_08_chaos";
        public const string INTENT_MADNESS       = "intent_09_madness";
        private static readonly string[] ALL_INTENT_IDS = {
            INTENT_EXTREME, INTENT_ANGEL, INTENT_QIANHUAN, INTENT_KILLING,
            INTENT_REVERSE, INTENT_LIFE_DEATH, INTENT_REINCARNATION, INTENT_CHAOS, INTENT_MADNESS
        };
        private const int COST_SEC_ANGEL     = 100;
        private const int COST_SEC_QIANHUAN  = 60;
        private const int COST_SEC_KILLING   = 50;
        private const int COST_SEC_REVERSE   = 70;
        private const int COST_SEC_LIFEDEATH = 200;
        private const int COST_SEC_CHAOS     = 100;
        private const int COST_SEC_MADNESS   = 50;
        public  const int COST_EVENT_EXTREME = 10;   
        public  const int COST_EVENT_REBIRTH = 1500; 
        private const float RADIUS_TILES = 10f;
        private const int   FRIENDS_MAX  = 10;
        private const float ANGEL_HEAL_PCT = 0.05f;      
        private const float ANGEL_DEF_PER_FRIEND = 0.02f;
        private const float MADNESS_SELF_LOSS_PCT   = 0.05f; 
        private const int   MADNESS_SELF_LOSS_EVERY = 2;
        private const int   REVERSE_CHECK_INTERVAL = 5; 
        private static readonly string[] NEGATIVE_STATUSES = {
            "poisoned","cursed","burning","slowness"
        };
        private const string KEY_LINGLI            = "xn.stat.lingli";
        private const string KEY_ACTIVE_PREFIX     = "xn.intent.active.";    
        private const string KEY_TMP_DEF_PCT       = "xn.intent.tmp_def_pct";
        private const string KEY_LAST_COMBAT_TS    = "xn.intent.last_combat_ts"; 
        private const string KEY_KILLING_LAYERS    = "xn.intent.killing_layers"; 
        private const string KEY_QH_LAYERS         = "xn.intent.qh_layers";      
        private const string KEY_REVERSE_NEXT      = "xn.intent.reverse_next_ts";
        private const string KEY_MADNESS_NEXT      = "xn.intent.madness_next_ts";
        private const string KEY_LD_ACTIVE         = "xn.intent.life_death_active";
        private const string KEY_MADNESS_ATKSPD_ON = "xn.intent.madness_atkspd_on";
        private static float s_nextTick = 0.0f;
        private static readonly HashSet<long> s_intentUnits = new HashSet<long>();
        private static float s_nextCacheRefresh = 0f;
        private const float CACHE_REFRESH_INTERVAL = 5f; 
        public static void Init(Harmony h)
        {
        if (s_patched) return;
        h.PatchAll(typeof(IntentSystem));
        s_patched = true;
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        [HarmonyPostfix]
        private static void Post_MapBox_Update()
        {
            float now = Time.time;
            if (now < s_nextTick) return;
            s_nextTick = now + 1.0f; 
            if (now >= s_nextCacheRefresh)
            {
                s_nextCacheRefresh = now + CACHE_REFRESH_INTERVAL;
                RefreshIntentUnitsCache();
            }
            TickIntents();
        }
        private static void RefreshIntentUnitsCache()
        {
            s_intentUnits.Clear();
            var actors = World.world.units.getSimpleList();
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.isAlive()) continue;
                if (a.kingdom == null || a.kingdom.wild) continue;
                if (HasAnyIntent(a))
                {
                    s_intentUnits.Add(xn.access.ActorAccess.GetData(a).id);
                }
            }
        }
        private static bool HasAnyIntent(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t == null) continue;
                for (int i = 0; i < ALL_INTENT_IDS.Length; i++)
                {
                    if (t.id == ALL_INTENT_IDS[i]) return true;
                }
            }
            return false;
        }
        private static void TickIntents()
        {
            var firstNegative = BuildFirstNegativeStatusIndex();
            List<long> toRemove = null;
            foreach (long id in s_intentUnits)
            {
                Actor a = World.world.units.get(id);
                if (a == null || !a.isAlive())
                {
                    YijingFX.StopLoop(a); 
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                    continue;
                }
                if (a.kingdom == null || a.kingdom.wild)
                {
                    YijingFX.StopLoop(a); 
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                    continue;
                }
                bool inCombat = IsInCombat(a);
                if (HasIntent(a, INTENT_ANGEL))
                {
                    int friendsCnt = 0;
                    bool hasWounded = HasWoundedAllyNearby(a, RADIUS_TILES, out friendsCnt);
                    bool shouldOpen = inCombat || hasWounded;
                    if (shouldOpen)
                    {
                        if (SpendLingli(a, COST_SEC_ANGEL))
                        {
                            SetActive(a, INTENT_ANGEL, true);
                            HealAllies(a, RADIUS_TILES, ANGEL_HEAL_PCT);
                            float defPct = Mathf.Min(friendsCnt, FRIENDS_MAX) * ANGEL_DEF_PER_FRIEND;
                            xn.access.ActorAccess.GetData(a).set(KEY_TMP_DEF_PCT, defPct);
                        }
                        else
                        {
                            SetActive(a, INTENT_ANGEL, false);
                            xn.access.ActorAccess.GetData(a).set(KEY_TMP_DEF_PCT, 0f);
                        }
                    }
                    else
                    {
                        SetActive(a, INTENT_ANGEL, false);
                        xn.access.ActorAccess.GetData(a).set(KEY_TMP_DEF_PCT, 0f);
                    }
                }
                if (HasIntent(a, INTENT_QIANHUAN))
                {
                    if (!inCombat)
                    {
                        SetActive(a, INTENT_QIANHUAN, false);
                    }
                }
                if (HasIntent(a, INTENT_KILLING))
                {
                    if (!inCombat)
                    {
                        int last, nowTs = (int)World.world.getCurWorldTime();
                        xn.access.ActorAccess.GetData(a).get(KEY_LAST_COMBAT_TS, out last, 0);
                        if (nowTs - last > 10)
                        {
                            int layers; xn.access.ActorAccess.GetData(a).get(KEY_KILLING_LAYERS, out layers, 0);
                            if (layers > 0) xn.access.ActorAccess.GetData(a).set(KEY_KILLING_LAYERS, layers - 1);
                        }
                        SetActive(a, INTENT_KILLING, false);
                    }
                }
                if (HasIntent(a, INTENT_REVERSE))
                {
                    bool hasNeg = firstNegative.ContainsKey(xn.access.ActorAccess.GetData(a).id);
                    if (!inCombat && !hasNeg)
                    {
                        SetActive(a, INTENT_REVERSE, false);
                    }
                    else if (hasNeg)
                    {
                        bool active = GetActive(a, INTENT_REVERSE);
                        if (active)
                        {
                            TryCleanOneNegative(a, firstNegative); 
                            MarkReverseBoost5s(a);                  
                        }
                    }
                }
                if (HasIntent(a, INTENT_LIFE_DEATH))
                {
                    float hp = a.getHealth(), mhp = a.getMaxHealth();
                    bool needOpen  = (mhp > 0 && hp / mhp <= 0.20f);
                    bool needClose = (mhp > 0 && hp / mhp >= 0.80f);
                    bool active = GetActive(a, INTENT_LIFE_DEATH);
                    if (needOpen)
                    {
                        if (SpendLingli(a, COST_SEC_LIFEDEATH))
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_LD_ACTIVE, 1);
                            SetActive(a, INTENT_LIFE_DEATH, true);
                        }
                        else
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_LD_ACTIVE, 0);
                            SetActive(a, INTENT_LIFE_DEATH, false);
                        }
                    }
                    else if (needClose)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_LD_ACTIVE, 0);
                        SetActive(a, INTENT_LIFE_DEATH, false);
                    }
                    else if (active)
                    {
                        if (!SpendLingli(a, COST_SEC_LIFEDEATH))
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_LD_ACTIVE, 0);
                            SetActive(a, INTENT_LIFE_DEATH, false);
                        }
                    }
                }
                if (HasIntent(a, INTENT_CHAOS))
                {
                    if (!inCombat)
                    {
                        SetActive(a, INTENT_CHAOS, false);
                    }
                }
                if (HasIntent(a, INTENT_MADNESS))
                {
                    if (!inCombat)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_MADNESS_ATKSPD_ON, 0);
                        SetActive(a, INTENT_MADNESS, false);
                    }
                    else
                    {
                        bool active = GetActive(a, INTENT_MADNESS);
                        if (active)
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_MADNESS_ATKSPD_ON, 1); 
                            TryMadnessSelfLose(a);                
                        }
                        else
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_MADNESS_ATKSPD_ON, 0);
                        }
                    }
                }
                if (HasIntent(a, INTENT_EXTREME))
                {
                    SetActive(a, INTENT_EXTREME, inCombat);
                }
            }
            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                    s_intentUnits.Remove(toRemove[i]);
            }
        }
        private static bool HasIntent(Actor a, string id)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list) { if (t != null && t.id == id) return true; }
            return false;
        }
        private static bool GetActive(Actor a, string id) { int v; xn.access.ActorAccess.GetData(a).get(KEY_ACTIVE_PREFIX + id, out v, 0); return v == 1; }
        private static void SetActive(Actor a, string id, bool on)
        {
            xn.access.ActorAccess.GetData(a).set(KEY_ACTIVE_PREFIX + id, on ? 1 : 0);
            if (id != INTENT_EXTREME && id != INTENT_REINCARNATION)
            {
                if (on) YijingFX.StartLoop(a);
                else YijingFX.StopLoop(a);
            }
        }
        private static bool IsInCombat(Actor a)
        {
            if (a == null || !a.isAlive()) return false;
            if (xn.access.ActorAccess.HasAttackTarget(a)) return true;
            var task = xn.access.ActorAccess.GetAI(a)?.task as BehaviourTaskActor;
            if (task != null)
            {
                if (task.in_combat) return true;
                if (task.id == "fighting") return true;
            }
            return false;
        }
        private static bool SpendLingli(Actor a, int amount)
        {
            if (amount <= 0) return true;
            int cur; xn.access.ActorAccess.GetData(a).get(KEY_LINGLI, out cur, 0);
            if (cur < amount) return false;
            xn.access.ActorAccess.GetData(a).set(KEY_LINGLI, cur - amount);
            return true;
        }
        private static bool HasWoundedAllyNearby(Actor a, float radiusTiles, out int friendsCount)
        {
            friendsCount = 0;
            if (a.current_tile == null || a.kingdom == null) return false;
            var list = Finder.getUnitsFromChunk(a.current_tile, 0, radiusTiles, false); 
            if (list == null) return false;
            bool hasWounded = false;
            foreach (var b in list)
            {
                if (b == null || b == a) continue;
                if (!b.isAlive()) continue;
                if (b.kingdom != a.kingdom) continue;
                friendsCount++;
                if (b.getHealth() < b.getMaxHealth()) hasWounded = true;
            }
            return hasWounded;
        }
        private static void HealAllies(Actor a, float radiusTiles, float pct)
        {
            var list = Finder.getUnitsFromChunk(a.current_tile, 0, radiusTiles, false);
            if (list != null)
            {
                int selfHeal = Mathf.FloorToInt(a.getMaxHealth() * pct);
                if (selfHeal > 0) a.changeHealth(selfHeal);
                foreach (var b in list)
                {
                    if (b == null || b == a) continue;
                    if (!b.isAlive()) continue;
                    if (b.kingdom != a.kingdom) continue;
                    int heal = Mathf.FloorToInt(b.getMaxHealth() * pct);
                    if (heal > 0) b.changeHealth(heal);
                }
            }
        }
        private static Dictionary<long, Status> BuildFirstNegativeStatusIndex()
        {
            var map = new Dictionary<long, Status>();
            var list = World.world.statuses.list; 
            foreach (var st in list)
            {
                if (st == null || st.is_finished) continue;
                var sim = st.sim_object;
                if (sim == null || !xn.access.BaseSimObjectAccess.IsActor(sim)) continue;
                string sid = st.asset.id;
                bool neg = false;
                for (int k = 0; k < NEGATIVE_STATUSES.Length; k++)
                    if (NEGATIVE_STATUSES[k] == sid) { neg = true; break; }
                if (!neg) continue;
                Actor actor = xn.access.BaseSimObjectAccess.GetActor(sim);
                ActorData actorData = xn.access.ActorAccess.GetData(actor);
                if (actorData == null) continue;
                long aid = actorData.id;
                if (!map.ContainsKey(aid)) map[aid] = st; 
            }
            return map;
        }
        private static void TryMadnessSelfLose(Actor a)
        {
            int nextTs; xn.access.ActorAccess.GetData(a).get(KEY_MADNESS_NEXT, out nextTs, 0);
            int now = (int)World.world.getCurWorldTime();
            if (now < nextTs) return;
            xn.access.ActorAccess.GetData(a).set(KEY_MADNESS_NEXT, now + MADNESS_SELF_LOSS_EVERY);
            int lose = Mathf.FloorToInt(a.getHealth() * MADNESS_SELF_LOSS_PCT);
            if (lose > 0) a.changeHealth(-lose);
            if (!a.hasHealth()) a.batch.c_check_deaths.Add(a);
        }
        private static void TryCleanOneNegative(Actor a, Dictionary<long, Status> firstNeg)
        {
            Status st;
            if (firstNeg.TryGetValue(xn.access.ActorAccess.GetData(a).id, out st))
            {
                int next; xn.access.ActorAccess.GetData(a).get(KEY_REVERSE_NEXT, out next, 0);
                int now = (int)World.world.getCurWorldTime();
                if (now >= next)
                {
                    st.finish(); 
                    xn.access.ActorAccess.GetData(a).set(KEY_REVERSE_NEXT, now + REVERSE_CHECK_INTERVAL);
                }
            }
        }
        private static void MarkReverseBoost5s(Actor a)
        {
            int now = (int)World.world.getCurWorldTime();
            xn.access.ActorAccess.GetData(a).set("xn.intent.reverse_boost_until", now + 5); 
        }
    }
}
