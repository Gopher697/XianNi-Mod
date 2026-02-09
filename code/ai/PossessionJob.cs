using System;
using System.Collections.Generic;
using HarmonyLib;
using ai;
using UnityEngine;
using xn.Traits;
using xn.world;
namespace cultivation.ai
{
    internal static class PossessionJob
    {
        private const string KEY_POS_ACTIVE   = "xn.possession.active";        
        private const string KEY_POS_DEADLINE = "xn.possession.deadline_time"; 
        private const string KEY_POS_TARGET   = "xn.possession.target_id";     
        private const string KEY_POS_RESOLVE  = "xn.possession.resolve_t";     
        private const string KEY_POS_TAKEN    = "xn.possession.taken";         
        private const string KEY_POS_BEING_POSSESSED = "xn.possession.being_possessed"; 
        private const string KEY_POS_RESOLVE_DEATH = "xn.possession.resolve_death"; 
        private const string KEY_REINC        = "xn.reincarnation.count";      
        private const string KEY_POS_PREV_INFO = "xn.possession.prev_info";    
        private const string KEY_REINC_BRUSH  = "xn.reincarnation.brush";      
        private const string KEY_XP    = "xn.stat.xiuwei";
        private const string KEY_WUXIN = "xn.stat.wuxin";
        private const string KEY_LUCK  = "xn.stat.qiyun";
        private const string KEY_STOP  = "xn.cultivation.stop";
        private const string KEY_SEAL_UNTIL_YEAR = "xn.seal_until_year";
        private static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
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
        private static readonly float[] REALM_TRIGGER_PROB = new float[] {
            0f,    
            0f,    
            0f,    
            0.05f, 
            0.10f, 
            0.20f, 
            0.35f, 
            0.45f, 
            0.55f, 
            0.65f, 
            0.70f, 
            0.75f, 
            0.80f, 
            0.90f, 
            0.95f, 
            1.00f  
        };
        private static float GetTriggerProbByRealmIndex(int idx)
        {
            if (idx < 0 || idx >= REALM_TRIGGER_PROB.Length) return 0f;
            return REALM_TRIGGER_PROB[idx];
        }
        public static void Init(Harmony h)
        {
            RegisterJob();
            h.Patch(AccessTools.Method(typeof(Actor), "die",
                new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(PossessionJob), nameof(Prefix_Actor_die)) { priority = Priority.Normal });
            h.Patch(AccessTools.Method(typeof(MapBox), "Update"),
                postfix: new HarmonyMethod(typeof(PossessionJob), nameof(Post_MapBox_Update)));
        }
        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib.get("job_xn_possession") != null) return;
            var job = new ActorJob { id = "job_xn_possession" };
            lib.add(job);
            job.addTask("stay_in_own_home");
            job.addTask("end_job");
        }
        private static bool Prefix_Actor_die(Actor __instance, bool pDestroy, AttackType pType, bool pCountDeath, bool pLogFavorite)
        {
            if (xn.world.SpaceRiftSystem.IsSpaceActiveFor(__instance)) return true;
            int mark;
            __instance.data.get(AmbitionSystem.KEY_AMB_DRAGON, out mark, 0);
            if (mark == 1) return true;
            __instance.data.get(AmbitionSystem.KEY_AMB_DEMON, out mark, 0);
            if (mark == 1) return true;
            __instance.data.get(KEY_REINC_BRUSH, out mark, 0);
            if (mark == 1) return true;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_REMOVED, out mark, 0);
            if (mark == 1) return true;
            __instance.data.get(KEY_POS_RESOLVE_DEATH, out mark, 0);
            if (mark == 1) return true;
            int isMainChar;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar == 1)
            {
                int lives;
                __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_LIVES, out lives, 0);
                if (lives > 0) return true; 
            }
            if (!pDestroy)
            {
                int alreadyActive; __instance.data.get(KEY_POS_ACTIVE, out alreadyActive, 0);
                if (alreadyActive == 1) return false;
                int idx = GetRealmIndex(__instance);
                if (idx >= 3) 
                {
                    float prob = GetTriggerProbByRealmIndex(idx);
                    if (prob > 0f && Randy.randomChance(prob))
                    {
                        __instance.data.set(KEY_POS_ACTIVE, 1);
                        __instance.data.set(KEY_POS_DEADLINE, Time.time + 15f); 
                        __instance.ai.setJob("job_xn_possession");
                        return false; 
                    }
                }
            }
            return true;
        }
        private static void Post_MapBox_Update(MapBox __instance)
        {
            var units = World.world.units; if (units == null) return;
            float now = Time.time;
            foreach (var a in units)
            {
                if (a == null) continue;
                int active; a.data.get(KEY_POS_ACTIVE, out active, 0);
                if (active != 1) continue; 
                float deadline; a.data.get(KEY_POS_DEADLINE, out deadline, 0f);
                if (deadline > 0f && now > deadline)
                {
                    long tidTimeout; a.data.get(KEY_POS_TARGET, out tidTimeout, 0L);
                    if (tidTimeout != 0L)
                    {
                        var targetTimeout = World.world.units.get(tidTimeout);
                        if (targetTimeout != null && targetTimeout.isAlive())
                        {
                            targetTimeout.data.set(KEY_POS_BEING_POSSESSED, 0);
                        }
                    }
                    ReincarnationSystem.OnEligibleDeath(a); 
                    ForceDestroySoul(a);
                    continue;
                }
                long tid; a.data.get(KEY_POS_TARGET, out tid, 0L);
                if (tid == 0)
                {
                    var t = PickTarget(a);
                    if (t == null) continue;
                    int beingPossessed; t.data.get(KEY_POS_BEING_POSSESSED, out beingPossessed, 0);
                    if (beingPossessed != 0) continue; 
                    t.data.set(KEY_POS_BEING_POSSESSED, 1);
                    FreezeDuringFX(a, t);
                    DuoSheFX.PlayOnce(t);
                    a.data.set(KEY_POS_TARGET, t.getID());
                    a.data.set(KEY_POS_RESOLVE, now + DuoSheFX.GetDuration());
                    continue;
                }
                float resolveAt; a.data.get(KEY_POS_RESOLVE, out resolveAt, 0f);
                if (now >= resolveAt && resolveAt > 0f)
                {
                    var t = World.world.units.get(tid);
                    ResolvePossession(a, t);
                }
            }
        }
        private static void ResolvePossession(Actor soul, Actor target)
        {
            soul.data.set(KEY_POS_ACTIVE, 0);
            long tid = 0L; soul.data.get(KEY_POS_TARGET, out tid, 0L);
            soul.data.set(KEY_POS_TARGET, 0L);
            soul.data.set(KEY_POS_RESOLVE, 0f);
            if (target != null && target.isAlive())
            {
                target.data.set(KEY_POS_BEING_POSSESSED, 0);
            }
            else if (tid != 0L)
            {
                var targetById = World.world.units.get(tid);
                if (targetById != null && targetById.isAlive())
                {
                    targetById.data.set(KEY_POS_BEING_POSSESSED, 0);
                }
            }
            if (target == null || !target.isAlive())
            {
                ReincarnationSystem.OnEligibleDeath(soul); 
                ForceDestroySoul(soul);
                return;
            }
            int sw; soul.data.get(KEY_WUXIN, out sw, 0);
            int sl; soul.data.get(KEY_LUCK,  out sl, 0);
            int tw; target.data.get(KEY_WUXIN, out tw, 0);
            int tl; target.data.get(KEY_LUCK,  out tl, 0);
            int realmIdx = GetRealmIndex(soul);
            float floor = 0.10f; 
            if (realmIdx >= 3)
            {
                float stepped = 0.10f + 0.10f * (realmIdx - 3);
                floor = Mathf.Min(0.70f, stepped);
            }
            float prob = Mathf.Max(floor, (sw + sl - (tw + tl)) * 0.10f);
            if (Randy.randomChance(prob))
            {
                SavePreviousLifeSnapshot(soul, target);
                ApplyPossessionSuccess(soul, target);
                int rc; soul.data.get(KEY_REINC, out rc, 0);
                target.data.set(KEY_REINC, rc);
                target.data.set(KEY_POS_TAKEN, 1);
                target.data.set(KEY_STOP, 1);
                target.data.set(KEY_SEAL_UNTIL_YEAR, Date.getCurrentYear() + 10);
                BroadcastSystem.PossessionSuccess(soul, target);
                soul.data.set("xn.reinc.enq", 1); 
                ForceDestroySoul(soul);
            }
            else
            {
                BroadcastSystem.PossessionFail(soul, target);
                ReincarnationSystem.OnEligibleDeath(soul); 
                ForceDestroySoul(soul);
            }
        }
        private static void FreezeDuringFX(Actor a, Actor t)
        {
            float d = DuoSheFX.GetDuration();
            if (a.isAlive()) a.makeStunned(d);
            if (t != null && t.isAlive()) t.makeStunned(d);
        }
        private static void ForceDestroySoul(Actor soul)
        {
            if (soul == null) return;
            soul.data.set(KEY_POS_RESOLVE_DEATH, 1);
            soul.dieAndDestroy(AttackType.Other);
            if (soul.isAlive())
            {
                soul.setAlive(false);
                World.world.units.scheduleDestroyOnPlay(soul);
            }
        }
        private static void SavePreviousLifeSnapshot(Actor soul, Actor target)
        {
            if (soul == null || target == null) return;
            long soulId = soul.getID();
            string soulName = soul.getName();
            int realmIdx = GetRealmIndex(soul);
            string realmName = (realmIdx >= 0 && realmIdx < REALM_IDS.Length) ? REALM_IDS[realmIdx] : "";
            long xp; soul.data.get(KEY_XP, out xp, 0L);
            int wuxin; soul.data.get(KEY_WUXIN, out wuxin, 0);
            int luck; soul.data.get(KEY_LUCK, out luck, 0);
            string kingdomName = soul.hasKingdom() ? soul.kingdom.name : "";
            string speciesId = "";
            if (soul.asset != null && !string.IsNullOrEmpty(soul.asset.id))
            {
                speciesId = soul.asset.id;
            }
            else if (soul.data != null && !string.IsNullOrEmpty(soul.data.asset_id))
            {
                speciesId = soul.data.asset_id;
            }
            int year = Date.getCurrentYear();
            string snapshot = $"{soulId}|{soulName}|{realmName}|{xp}|{wuxin}|{luck}|{kingdomName}|{speciesId}|{year}";
            target.data.set(KEY_POS_PREV_INFO, snapshot);
        }
        private static void ApplyPossessionSuccess(Actor src, Actor dst)
        {
            bool dstFavorite = dst.data.favorite;
            TransferBloodlineData(src, dst);
            TitleSystem.ClearTitleData(dst);
            string srcBase = ExtractBaseNameOnly(src);
            dst.setName(srcBase); 
            City srcCity = src.hasCity() ? src.city : null;
            City dstCity = dst.hasCity() ? dst.city : null;
            Kingdom srcKingdom = src.hasKingdom() ? src.kingdom : null;
            Kingdom dstKingdom = dst.hasKingdom() ? dst.kingdom : null;
            long xp; src.data.get(KEY_XP, out xp, 0L);
            dst.data.set(KEY_XP, xp);
            if (!HasAnyCultivationRealm(dst))
            {
                var qi = AssetManager.traits.get("realm_01_qi") as ActorTrait;
                if (qi != null) dst.addTrait(qi);
                dst.data.set(KEY_XP, 0L);
            }
            var removeBuf = new List<ActorTrait>(16);
            var tsDst = dst.getTraits();
            if (tsDst != null)
            {
                foreach (var t in tsDst) if (t != null) removeBuf.Add(t);
                for (int i = 0; i < removeBuf.Count; i++) dst.removeTrait(removeBuf[i]);
            }
            var tsSrc = src.getTraits();
            if (tsSrc != null)
            {
                foreach (var t in tsSrc) if (t != null && !dst.hasTrait(t)) dst.addTrait(t);
            }
            int currentRealmIdx = GetRealmIndex(dst);
            if (currentRealmIdx >= 1) 
            {
                int newRealmIdx = currentRealmIdx - 1; 
                if (newRealmIdx >= 0 && newRealmIdx < REALM_IDS.Length)
                {
                    string currentRealmId = REALM_IDS[currentRealmIdx];
                    var currentRealmTrait = AssetManager.traits.get(currentRealmId) as ActorTrait;
                    if (currentRealmTrait != null && dst.hasTrait(currentRealmTrait))
                    {
                        dst.removeTrait(currentRealmTrait);
                    }
                    string newRealmId = REALM_IDS[newRealmIdx];
                    var newRealmTrait = AssetManager.traits.get(newRealmId) as ActorTrait;
                    if (newRealmTrait != null)
                    {
                        dst.addTrait(newRealmTrait);
                    }
                    if (newRealmIdx < REALM_THRESHOLDS.Length)
                    {
                        long newXP = REALM_THRESHOLDS[newRealmIdx];
                        dst.data.set(KEY_XP, newXP);
                    }
                }
            }
            if (srcCity != null && !srcCity.isRekt())
            {
                dst.setCity(srcCity);
            }
            else if (dstCity != null && !dstCity.isRekt())
            {
                dst.setCity(null); 
                dst.setCity(dstCity); 
            }
            else if (srcKingdom != null && !srcKingdom.isRekt())
            {
                dst.setKingdom(srcKingdom);
            }
            else if (dstKingdom != null && !dstKingdom.isRekt())
            {
                dst.setKingdom(dstKingdom);
            }
            if (dstFavorite)
            {
                dst.data.favorite = true;
            }
            TransferMainCharacterStatus(src, dst);
        }
        private static void TransferMainCharacterStatus(Actor src, Actor dst)
        {
            if (src == null || dst == null) return;
            int isMainChar;
            src.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return; 
            dst.data.set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, 1);
            dst.data.set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_LIVES, 3); 
            dst.data.set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_REMOVED, 0); 
            if (World.world.map_stats.custom_data == null)
            {
                World.world.map_stats.custom_data = new SaveCustomData();
            }
            World.world.map_stats.custom_data.set("xn.world.main_char_id", dst.getID());
            if (!dst.isFavorite())
            {
                dst.switchFavorite();
            }
            string name = dst.getName() ?? "未知";
            BroadcastSystem.Custom($"主角{name}夺舍成功，继续拥有主角光环");
        }
        private static Actor PickTarget(Actor soul)
        {
            int myRealm = GetRealmIndex(soul);
            Actor bestCult = null; int bestScore = int.MinValue;
            Actor bestHuman = null; int bestScoreH = int.MinValue;
            foreach (var u in World.world.units)
            {
                if (u == null || !u.isAlive() || u == soul) continue;
                if (HasAnyAncientOrBeast(u)) continue;
                int taken; u.data.get(KEY_POS_TAKEN, out taken, 0);
                if (taken != 0) continue;
                int beingPossessed; u.data.get(KEY_POS_BEING_POSSESSED, out beingPossessed, 0);
                if (beingPossessed != 0) continue;
                int age = u.data.getAge(); 
                if (age < 18) continue;
                int score = -Mathf.Abs(age - 24);
                if (HasAnyCultivationRealm(u))
                {
                    int r = GetRealmIndex(u);
                    if (r >= myRealm) continue; 
                    score += (10 - r);
                    if (score > bestScore) { bestScore = score; bestCult = u; }
                }
                else
                {
                    if (score > bestScoreH) { bestScoreH = score; bestHuman = u; }
                }
            }
            return bestCult ?? bestHuman;
        }
        private static bool HasAnyCultivationRealm(Actor a)
        {
            var list = a.getTraits(); if (list == null) return false;
            foreach (var t in list) if (t != null && t.group_id == RealmTraitGroup.GroupRealm) return true;
            return false;
        }
        private static bool HasAnyAncientOrBeast(Actor a)
        {
            var list = a.getTraits(); if (list == null) return false;
            foreach (var t in list)
            {
                if (t == null) continue;
                if (t.group_id == RealmTraitGroup.GroupAncientRealm) return true;
                if (t.group_id == RealmTraitGroup.GroupBeastStage)   return true;
            }
            return false;
        }
        private static int GetRealmIndex(Actor a)
        {
            int idx = -1;
            var ts = a.getTraits();
            if (ts == null) return -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
                foreach (var t in ts) if (t != null && t.id == REALM_IDS[i]) { if (i > idx) idx = i; }
            return idx;
        }
        private static bool IsNascentOrAbove(Actor a)
        {
            int idx = GetRealmIndex(a);
            return idx >= 3; 
        }
        private static string ExtractBaseNameOnly(Actor a)
        {
            if (a != null)
            {
                a.data.get("xn.title.base_name", out string storedBase, "");
                if (!string.IsNullOrEmpty(storedBase))
                {
                    return storedBase;
                }
            }
            string name = a != null ? a.getName() : "";
            if (string.IsNullOrEmpty(name)) return name;
            string rest = name.Trim();
            int lastBracket = rest.LastIndexOf(']');
            if (lastBracket >= 0 && lastBracket + 1 < rest.Length)
            {
                rest = rest.Substring(lastBracket + 1).Trim();
            }
            else
            {
                while (true)
                {
                    int startBracket = rest.IndexOf('[');
                    if (startBracket < 0) break;
                    int endBracket = rest.IndexOf(']', startBracket);
                    if (endBracket <= startBracket) break;
                    rest = rest.Substring(0, startBracket) + rest.Substring(endBracket + 1);
                    rest = rest.Trim();
                }
            }
            int dash = rest.IndexOf('-');
            if (dash >= 0) rest = rest.Substring(0, dash).Trim();
            return string.IsNullOrEmpty(rest) ? name.Trim() : rest;
        }
        private static void TransferBloodlineData(Actor src, Actor dst)
        {
            if (src == null || dst == null) return;
            src.data.get("xn.bloodline.type", out string bloodlineType, "");
            if (string.IsNullOrEmpty(bloodlineType))
            {
                return;
            }
            string[] stringKeys = new string[]
            {
                "xn.bloodline.type",
                "xn.bloodline.founder_name",
                "xn.bloodline.mutation_type"
            };
            foreach (var key in stringKeys)
            {
                src.data.get(key, out string value, "");
                if (!string.IsNullOrEmpty(value))
                {
                    dst.data.set(key, value);
                }
            }
            src.data.get("xn.bloodline.concentration", out float concentration, 0f);
            if (concentration > 0f)
            {
                dst.data.set("xn.bloodline.concentration", concentration);
            }
            string[] intKeys = new string[]
            {
                "xn.bloodline.generation",
                "xn.bloodline.awakened",
                "xn.bloodline.awakened_year",
                "xn.bloodline.is_founder",
                "xn.bloodline.is_atavism",
                "xn.bloodline.position",
                "xn.bloodline.last_election_year",
                "xn.bloodline.family_created_year"
            };
            foreach (var key in intKeys)
            {
                src.data.get(key, out int value, 0);
                dst.data.set(key, value);
            }
            string[] longKeys = new string[]
            {
                "xn.bloodline.founder_id"
            };
            foreach (var key in longKeys)
            {
                src.data.get(key, out long value, -1L);
                dst.data.set(key, value);
            }
            if (src.hasClan())
            {
                dst.data.set("xn.bloodline.clan_id", src.clan.getID());
                if (src.clan.isAlive())
                {
                    dst.setClan(src.clan);
                }
            }
            else if (dst.hasClan())
            {
                dst.data.set("xn.bloodline.clan_id", dst.clan.getID());
            }
        }
    }
}