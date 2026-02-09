using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.Traits;
using NeoModLoader.utils;
namespace xn.race
{
    internal static class DashouSystem
    {
        private const string ACTOR_ID = "dashou";
        private const string KEY_ANC_POWER = "xn.stat.gushen_power";
        private const string KEY_BEHAVIOR_MODE = "xn.dashou.behavior_mode";
        private static readonly string[] ANC_STAR_IDS = new[]
        {
            "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star", "ancient_06_star"
        };
        private static readonly int[] ANC_THRESHOLDS = new[]
        {
            30000,  
            50000,  
            100000, 
            200000, 
            500000  
        };
        private static readonly string[] DIVINE_IDS = new[]
        {
            "divine_01_baonuzhibian", "divine_02_weiya", "divine_03_sanmeizhenhuo",
            "divine_04_wanjianguizong", "divine_05_xuankongpo", "divine_06_zhenkongquan",
            "divine_07_jiuyinbaiguzhao", "divine_08_duqidan", "divine_09_jianzhan"
        };
        private static readonly string[] ART_IDS = new[]
        {
            "art_01_missile", "art_02_ascension", "art_03_slash", "art_04_quake", "art_05_waves",
            "art_06_convert", "art_07_palm", "art_08_breaker", "art_09_shield", "art_10_link"
        };
        public static void Init(Harmony harmony)
        {
            DashouKillerJob.Init();
            var mNewCreature = AccessTools.Method(typeof(Actor), "newCreature");
            if (mNewCreature != null)
            {
                harmony.Patch(mNewCreature, postfix: new HarmonyMethod(typeof(DashouSystem), nameof(Post_Actor_newCreature)));
            }
            var mDie = AccessTools.Method(typeof(Actor), "die", new[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) });
            if (mDie != null)
            {
                harmony.Patch(mDie, prefix: new HarmonyMethod(typeof(DashouSystem), nameof(Pre_Actor_die)));
            }
            var mUpdatePath = AccessTools.Method(typeof(Actor), "updatePathMovement");
            if (mUpdatePath != null)
            {
                harmony.Patch(mUpdatePath, prefix: new HarmonyMethod(typeof(DashouSystem), nameof(Pre_Actor_updatePathMovement)));
            }
        }
        [HarmonyPostfix]
        private static void Post_Actor_newCreature(Actor __instance)
        {
            if (__instance == null || __instance.asset == null || __instance.asset.id != ACTOR_ID)
                return;
            int starIdx = UnityEngine.Random.Range(0, ANC_STAR_IDS.Length);
            string starId = ANC_STAR_IDS[starIdx];
            int minPower = ANC_THRESHOLDS[starIdx];
            var starTrait = AssetManager.traits.get(starId) as ActorTrait;
            if (starTrait != null)
            {
                __instance.addTrait(starTrait);
            }
            __instance.data.set(KEY_ANC_POWER, minPower);
            __instance.data.set(KEY_BEHAVIOR_MODE, xn.config.ModConfigHooks.DashouBehaviorMode);
            __instance.data.set("xn.dashou.no_auto_fav", 1);
            __instance.data.set("xn.dashou.no_title", 1);
            ApplyBehaviorJob(__instance);
        }
        [HarmonyPrefix]
        private static void Pre_Actor_die(Actor __instance, bool pDestroy, AttackType pType, bool pCountDeath, bool pLogFavorite)
        {
            if (__instance == null || __instance.asset == null || __instance.asset.id != ACTOR_ID)
                return;
            Actor killer = null;
            if (!__instance.attackedBy.isRekt() && __instance.attackedBy.isActor())
            {
                killer = __instance.attackedBy.a;
            }
            if (killer == null || !killer.isAlive())
                return;
            float roll = UnityEngine.Random.value;
            if (roll < 0.6f)
            {
                GiveRandomDivine(killer);
            }
            else if (roll < 0.9f)
            {
                GiveRandomArt(killer);
            }
        }
        [HarmonyPrefix]
        private static bool Pre_Actor_updatePathMovement(Actor __instance)
        {
            if (__instance == null || __instance.asset == null || __instance.asset.id != ACTOR_ID)
                return true; 
            int mode;
            __instance.data.get(KEY_BEHAVIOR_MODE, out mode, 0);
            if (mode == 2)
            {
                __instance.stopMovement();
                return false; 
            }
            return true;
        }
        public static void ApplyBehaviorToAll()
        {
            int mode = xn.config.ModConfigHooks.DashouBehaviorMode;
            int count = 0;
            foreach (var actor in World.world.units)
            {
                if (actor == null || !actor.isAlive() || actor.asset == null || actor.asset.id != ACTOR_ID)
                    continue;
                actor.data.set(KEY_BEHAVIOR_MODE, mode);
                ApplyBehaviorJob(actor);
                count++;
            }
            xn.world.BroadcastSystem.Custom($"已应用行为模式 {mode} 到 {count} 个打手");
        }
        private static void ApplyBehaviorJob(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;
            int mode;
            actor.data.get(KEY_BEHAVIOR_MODE, out mode, 0);
            if (mode == 1)
            {
                actor.ai.setJob("job_dashou_killer");
            }
            else if (mode == 2)
            {
                actor.stopMovement();
                actor.clearAttackTarget();
                actor.endJob(); 
                if (!actor.hasTag("ignore_fights"))
                {
                    actor.stats.addTag("ignore_fights");
                    actor.setStatsDirty();
                }
            }
            else
            {
                actor.endJob(); 
                if (actor.hasTag("ignore_fights") && actor.stats._tags != null)
                {
                    actor.stats._tags.Remove("ignore_fights");
                    actor.setStatsDirty();
                }
            }
        }
        private static int GetRealmIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int idx = -1;
            string[] REALM_IDS = {
                "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent","realm_05_deity",
                "realm_06_infantchg","realm_07_wending","realm_08_kuinie","realm_09_jingnie","realm_10_suinie",
                "realm_11_kongnie","realm_12_kongling","realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
            };
            for (int i = 0; i < REALM_IDS.Length; i++)
                foreach (var t in list)
                    if (t != null && t.id == REALM_IDS[i]) { if (i > idx) idx = i; }
            return idx;
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
                if (trait != null)
                {
                    a.addTrait(trait);
                    xn.world.BroadcastSystem.Custom($"{a.getName()} 获得了神通：{trait.id}");
                }
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
                if (trait != null)
                {
                    a.addTrait(trait);
                    xn.world.BroadcastSystem.Custom($"{a.getName()} 获得了仙术：{trait.id}");
                }
            }
        }
    }
}