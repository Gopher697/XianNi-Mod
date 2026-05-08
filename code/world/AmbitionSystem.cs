using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace xn.world
{
    public static class AmbitionSystem
    {
        public const string KEY_AMB_DEMON  = "xn.amb.demon";   
        public const string KEY_AMB_DRAGON = "xn.amb.dragon";  
        private static int s_value = 0;               
        private static int s_nextDemonThreshold = 1000; 
        private const int DEMON_MAX_THRESHOLD = 9000; 
        private static bool s_dragonSpawned = false;  
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        public static int GetValue() { return s_value; }
        public static void Add(int delta)
        {
            if (delta <= 0) return;
            int old = s_value;
            s_value = ClampAdd(s_value, delta);
            CheckMilestones(old, s_value);
        }
        public static void DecPercent(int percent)
        {
            if (percent <= 0) return;
            int dec = (s_value * percent) / 100;
            if (dec <= 0) dec = 1;
            s_value = Math.Max(0, s_value - dec);
        }
        private static void CheckMilestones(int oldVal, int newVal)
        {
            while (s_nextDemonThreshold <= DEMON_MAX_THRESHOLD && newVal >= s_nextDemonThreshold)
            {
                TrySpawnDemonClone(); 
                s_nextDemonThreshold += 1000;
            }
            if (newVal >= 10000)
            {
                if (TrySpawnDragonAvatar())
                {
                    s_value = 0;
                    s_nextDemonThreshold = 1000;
                    s_dragonSpawned = false; 
                }
            }
        }
        private static bool TrySpawnDemonClone()
        {
            if (!xn.config.ModConfigHooks.EnableTianyunziSpawn) return false;
            var tile = PickRandomSpawnTile();
            if (tile == null) return false;
            Actor demon = World.world.units.createNewUnit("demon", tile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: false, pAdultAge: true);
            if (demon == null) return false;
            xn.access.ActorAccess.GetData(demon).age_overgrowth = 18;
            demon.setName("Tian Yunzi Clone");
            xn.access.ActorAccess.GetData(demon).set(KEY_AMB_DEMON, 1);
            Actor src = PickRandomFromTop15ByPower();
            if (src != null)
            {
                CopyRealmTraits(src, demon);
            }
            GiveRandomDivine(demon, 1);
            BroadcastSystem.PostActor(demon, T("broadcast_tianyunzi_avatar_descends", "Tian Yunzi's avatar has descended"));
            return true;
        }
        private static bool TrySpawnDragonAvatar()
        {
            if (!xn.config.ModConfigHooks.EnableTianyunziSpawn) return false;
            var tile = PickRandomSpawnTile();
            if (tile == null) return false;
            Actor dragon = World.world.units.createNewUnit("dragon", tile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: false);
            if (dragon == null) return false;
            dragon.setName("Tian Yunzi");
            xn.access.BaseSimObjectAccess.GetStats(dragon)["speed"] = 60f;
            dragon.precalcMovementSpeed(true);
            xn.access.ActorAccess.GetData(dragon).set(KEY_AMB_DRAGON, 1);
            xn.access.ActorAccess.GetData(dragon).set("xn_is_tianyunzi", 1);
            dragon.addTrait("realm_15_half_tatian");
            cultivation.ai.TianyunziJob.Init(); 
            var dragonAI = xn.access.ActorAccess.GetAI(dragon);
            if (dragonAI != null) dragonAI.setJob("job_xn_tianyunzi");
            GiveRandomDivine(dragon, 5);
            GiveRandomImmortalArts(dragon, 5);
            GiveRandomIntentExcludingExtreme(dragon, 1);
            GiveRandomAttrFive(dragon, 1);
            BroadcastSystem.PostActor(dragon, T("broadcast_tianyunzi_descends", "Tian Yunzi has descended. Humans, meet your death."));
            return true;
        }
        static readonly string[] DIVINE = new string[] {
            "divine_01_baonuzhibian","divine_02_weiya","divine_03_sanmeizhenhuo",
            "divine_04_wanjianguizong","divine_05_xuankongpo","divine_06_zhenkongquan",
            "divine_07_jiuyinbaiguzhao","divine_08_duqidan","divine_09_jianzhan"
        };
        static readonly string[] IMMORTAL_ARTS = new string[] {
            "art_01_missile","art_02_ascension","art_03_slash","art_04_quake","art_05_waves",
            "art_06_convert","art_07_palm","art_08_breaker","art_09_shield","art_10_link"
        };
        static readonly string[] INTENTS = new string[] {
            "intent_02_angel","intent_03_qianhuan","intent_04_killing","intent_05_reverse",
            "intent_06_life_death","intent_07_reincarnation","intent_08_chaos","intent_09_madness"
        };
        static readonly string[] ATTR_FIVE = new string[] {
            "attr_01_metal","attr_02_wood","attr_03_water","attr_04_fire","attr_05_earth"
        };
        static void GiveRandomDivine(Actor a, int count)
        {
            int given = 0, tries = 0;
            while (given < count && tries < 50)
            {
                tries++;
                int idx = Randy.randomInt(0, DIVINE.Length - 1);
                a.addTrait(DIVINE[idx]);
                given++;
            }
        }
        static void GiveRandomImmortalArts(Actor a, int count)
        {
            int given = 0, tries = 0;
            while (given < count && tries < 80)
            {
                tries++;
                int idx = Randy.randomInt(0, IMMORTAL_ARTS.Length - 1);
                a.addTrait(IMMORTAL_ARTS[idx]);
                given++;
            }
        }
        static void GiveRandomIntentExcludingExtreme(Actor a, int count)
        {
            int given = 0, tries = 0;
            while (given < count && tries < 40)
            {
                tries++;
                int idx = Randy.randomInt(0, INTENTS.Length - 1);
                a.addTrait(INTENTS[idx]);
                given++;
            }
        }
        static void GiveRandomAttrFive(Actor a, int count)
        {
            int given = 0, tries = 0;
            while (given < count && tries < 20)
            {
                tries++;
                int idx = Randy.randomInt(0, ATTR_FIVE.Length - 1);
                a.addTrait(ATTR_FIVE[idx]);
                given++;
            }
        }
        internal static void OnActorKilledBy(Actor killer)
        {
            if (killer == null || killer.isRekt()) return;
            int mark;
            xn.access.ActorAccess.GetData(killer).get(KEY_AMB_DEMON, out mark, 0);
            if (mark == 1)
            {
                int add = Randy.randomInt(10, 100);
                Add(add);
            }
        }
        private static WorldTile PickRandomSpawnTile()
        {
            WorldTile pick = null;
            int seen = 0;
            foreach (var a in World.world.units)
            {
                if (a == null || !a.isAlive() || a.current_tile == null) continue;
                seen++;
                if (Randy.randomInt(1, seen) == 1) pick = a.current_tile;
            }
            return pick;
        }
        private static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        private const int MAX_REALM_INDEX = 9; 
        private static void CopyRealmTraits(Actor src, Actor dst)
        {
            var list = src.getTraits();
            if (list == null) return;
            int maxSrcIndex = -1;
            foreach (var t in list)
            {
                if (t == null) continue;
                string id = t.id;
                if (string.IsNullOrEmpty(id)) continue;
                if (id.StartsWith("realm_"))
                {
                    for (int i = 0; i < REALM_IDS.Length; i++)
                    {
                        if (REALM_IDS[i] == id)
                        {
                            if (i > maxSrcIndex) maxSrcIndex = i;
                            break;
                        }
                    }
                }
            }
            int targetMaxIndex = maxSrcIndex;
            if (targetMaxIndex > MAX_REALM_INDEX)
            {
                targetMaxIndex = MAX_REALM_INDEX;
            }
            foreach (var t in list)
            {
                if (t == null) continue;
                string id = t.id;
                if (string.IsNullOrEmpty(id)) continue;
                if (id.StartsWith("realm_"))
                {
                    bool shouldCopy = false;
                    for (int i = 0; i <= targetMaxIndex; i++)
                    {
                        if (REALM_IDS[i] == id)
                        {
                            shouldCopy = true;
                            break;
                        }
                    }
                    if (shouldCopy)
                    {
                        dst.addTrait(id);
                    }
                }
            }
            if (targetMaxIndex >= 0 && targetMaxIndex <= MAX_REALM_INDEX)
            {
                bool hasMaxRealm = false;
                var dstTraits = dst.getTraits();
                if (dstTraits != null)
                {
                    foreach (var t in dstTraits)
                    {
                        if (t != null && t.id == REALM_IDS[targetMaxIndex])
                        {
                            hasMaxRealm = true;
                            break;
                        }
                    }
                }
                if (!hasMaxRealm && targetMaxIndex >= 0)
                {
                    dst.addTrait(REALM_IDS[targetMaxIndex]);
                }
            }
        }
        private static Actor PickRandomFromTop15ByPower()
        {
            var top = new List<(Actor a, long s)>(16);
            foreach (var a in World.world.units)
            {
                if (a == null || !a.isAlive() || !a.asset.can_be_favorited) continue;
                long s = CalcPowerLong(a);
                InsertTop(top, (a, s), 15);
            }
            if (top.Count == 0) return null;
            int pick = Randy.randomInt(0, top.Count - 1);
            return top[pick].a;
        }
        private static void InsertTop(List<(Actor a, long s)> list, (Actor a, long s) cur, int max)
        {
            int pos = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (cur.s > list[i].s || (cur.s == list[i].s && cur.a.getID() < list[i].a.getID())) { pos = i; break; }
            }
            if (pos == -1)
            {
                if (list.Count < max) list.Add(cur);
            }
            else
            {
                list.Insert(pos, cur);
                if (list.Count > max) list.RemoveAt(list.Count - 1);
            }
        }
        private static long CalcPowerLong(Actor u)
        {
            double dmg   = xn.access.BaseSimObjectAccess.GetStats(u)["damage"];          if (dmg   < 0) dmg = 0;   if (dmg   > 2_100_000_000d) dmg   = 2_100_000_000d;
            double aspd  = xn.access.BaseSimObjectAccess.GetStats(u)["attack_speed"];    if (aspd  < 0) aspd = 0;
            double cRate = xn.access.BaseSimObjectAccess.GetStats(u)["critical_chance"]; if (cRate < 0) cRate = 0; if (cRate > 1) cRate = 1;
            double cMult = xn.access.BaseSimObjectAccess.GetStats(u)["critical_damage_multiplier"]; if (cMult < 1) cMult = 1;
            double armor = xn.access.BaseSimObjectAccess.GetStats(u)["armor"];           if (armor < 0) armor = 0;
            double hpMax = u.getMaxHealth();           if (hpMax < 0) hpMax = 0; if (hpMax > 2_100_000_000d) hpMax = 2_100_000_000d;
            double dps  = dmg * aspd * (1.0 + cRate * cMult);
            double bulk = hpMax * 0.1 + armor * 1.5;
            double val  = dps + bulk;
            if (val <= 0) return 0;
            if (val >= long.MaxValue) return long.MaxValue;
            return (long)val;
        }
        private static int ClampAdd(int a, int b)
        {
            long v = (long)a + (long)b;
            if (v < 0) return 0;
            if (v > int.MaxValue) return int.MaxValue;
            return (int)v;
        }
        public static void ClearAll()
        {
            s_value = 0;
            s_nextDemonThreshold = 1000;
            s_dragonSpawned = false;
        }
    }
    [HarmonyPatch(typeof(Actor), "newKillAction")]
    internal static class AmbitionKillHook
    {
        static void Postfix(Actor __instance, Actor pDeadUnit, Kingdom pPrevKingdom, AttackType pAttackType)
        {
            AmbitionSystem.OnActorKilledBy(__instance);
        }
    }
}
