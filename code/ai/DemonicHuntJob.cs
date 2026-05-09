using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.world;
using ai.behaviours;
using AiBehaviours = ai.behaviours;
namespace cultivation.ai
{
    internal static class DemonicHuntJob
    {
        private const string KEY_HUNT_TARGET = "xn.demonic_hunt.target_id";  
        private const string KEY_HUNT_YEAR = "xn.demonic_hunt.year";         
        private const string KEY_HUNT_ACTIVE = "xn.demonic_hunt.active";     
        private const string KEY_XINMO = "xn.stat.xinmo";                    
        private const string KEY_GLOBAL_NEXT_CHECK = "xn.demonic_hunt.next_check_year";
        private static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
            "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
            "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
            "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian"
        };
        private static readonly Dictionary<string, int> REALM_INDEX_MAP = new Dictionary<string, int>();
        private static readonly long[] REALM_THRESHOLDS = new long[]
        {
            100000, 1500000, 4000000, 9600000, 30000000, 80000000, 150000000, 250000000,
            400000000, 600000000, 700000000, 800000000, 900000000, 980000000, 1200000000, 1500000000
        };
        private static readonly HashSet<long> s_activeHunters = new HashSet<long>();
        public static void Init()
        {
            if (REALM_INDEX_MAP.Count == 0)
            {
                for (int i = 0; i < REALM_IDS.Length; i++)
                {
                    REALM_INDEX_MAP[REALM_IDS[i]] = i;
                }
            }
            RegisterJob();
        }
        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib.get("job_xn_demonic_hunt") != null)
                return;
            RegisterHuntTask();
            var job = new ActorJob { id = "job_xn_demonic_hunt" };
            lib.add(job);
            job.addTask("task_xn_demonic_hunt");
        }
        private static void RegisterHuntTask()
        {
            var taskLib = AssetManager.tasks_actor;
            if (taskLib.get("task_xn_demonic_hunt") != null)
                return;
            var task = new BehaviourTaskActor
            {
                id = "task_xn_demonic_hunt",
                locale_key = "task_xn_demonic_hunt"
            };
            taskLib.add(task);
            task.addBeh(new BehFindAndChaseTarget());
            task.addBeh(new AiBehaviours.BehRestartTask());
        }
        private class BehFindAndChaseTarget : BehaviourActionActor
        {
            public override BehResult execute(Actor pActor)
            {
                long targetId;
                xn.access.ActorAccess.GetData(pActor).get(KEY_HUNT_TARGET, out targetId, -1L);
                if (targetId <= 0)
                {
                    xn.access.ActorAccess.GetData(pActor).set(KEY_HUNT_ACTIVE, 0);
                    s_activeHunters.Remove(xn.access.ActorAccess.GetData(pActor).id);
                    pActor.cancelAllBeh();
                    return BehResult.Stop;
                }
                Actor target = World.world.units.get(targetId);
                if (target == null || !target.isAlive())
                {
                    xn.access.ActorAccess.GetData(pActor).set(KEY_HUNT_ACTIVE, 0);
                    s_activeHunters.Remove(xn.access.ActorAccess.GetData(pActor).id);
                    OnHuntSuccess(pActor);
                    pActor.cancelAllBeh();
                    return BehResult.Stop;
                }
                pActor.setAttackTarget(target);
                if (xn.access.ActorAccess.IsInAttackRange(pActor, target))
                {
                    xn.access.ActorAccess.TryToAttack(pActor, target);
                }
                else
                {
                    if (!xn.access.ActorAccess.IsUsingPath(pActor) || xn.access.ActorAccess.GetBehActorTarget(pActor) != target)
                    {
                        xn.access.ActorAccess.SetBehActorTarget(pActor, target);
                        pActor.goTo(target.current_tile);
                    }
                }
                return BehResult.Continue;
            }
        }
        private static void OnHuntSuccess(Actor hunter)
        {
            if (hunter == null || !hunter.isAlive()) return;
            xn.access.BaseSimObjectAccess.GetStats(hunter)["damage"] += 30f;
            int curRealmIdx = GetCurrentRealmIndex(hunter);
            if (curRealmIdx >= 0 && curRealmIdx < REALM_THRESHOLDS.Length)
            {
                long curXp;
                xn.access.ActorAccess.GetData(hunter).get("xn.stat.xiuwei", out curXp, 0L);
                long threshold = REALM_THRESHOLDS[curRealmIdx];
                long gain = (long)(threshold * 0.2f);
                xn.access.ActorAccess.GetData(hunter).set("xn.stat.xiuwei", curXp + gain);
            }
            int curXinmo;
            xn.access.ActorAccess.GetData(hunter).get(KEY_XINMO, out curXinmo, 0);
            int xinmoGain = UnityEngine.Random.Range(10, 31);
            xn.access.ActorAccess.GetData(hunter).set(KEY_XINMO, curXinmo + xinmoGain);
        }
        private static void OnHuntFailed(Actor hunter)
        {
            if (hunter == null || !hunter.isAlive()) return;
            hunter.dieAndDestroy(AttackType.Divine);
        }
        [HarmonyPatch(typeof(Actor), "getNextJob")]
        private static class Patch_Actor_GetNextJob
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, ref string __result)
            {
                if (__instance == null || !__instance.isAlive()) return true;
                if (__instance.kingdom == null || __instance.city == null) return true;
                if (!HasTrait(__instance, "path_01_demonic")) return true;
                int active;
                xn.access.ActorAccess.GetData(__instance).get(KEY_HUNT_ACTIVE, out active, 0);
                if (active == 1)
                {
                    int stop;
                    xn.access.ActorAccess.GetData(__instance).get("xn.cultivation.stop", out stop, 0);
                    if (stop == 1) return true;
                    int trialActive;
                    xn.access.ActorAccess.GetData(__instance).get("xn.trial.active", out trialActive, 0);
                    if (trialActive == 1) return true;
                    RegisterJob();
                    __result = "job_xn_demonic_hunt";
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Actor), "setTask", new Type[] {
            typeof(string), typeof(bool), typeof(bool), typeof(bool)
        })]
        private static class Patch_Actor_SetTask
        {
            [HarmonyPrefix]
            private static void Prefix(Actor __instance, string pTaskId, bool pClean, ref bool pCleanJob, bool pForceAction)
            {
                if (__instance == null || !__instance.isAlive()) return;
                if (__instance.kingdom == null || __instance.city == null) return;
                if (!HasTrait(__instance, "path_01_demonic")) return;
                int active;
                xn.access.ActorAccess.GetData(__instance).get(KEY_HUNT_ACTIVE, out active, 0);
                if (active != 1) return;
                var actorAI = xn.access.ActorAccess.GetAI(__instance);
                if (actorAI != null && actorAI.job != null && actorAI.job.id == "job_xn_demonic_hunt")
                {
                    if (pCleanJob)
                    {
                        pCleanJob = false;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(BaseSimObject), "canAttackTarget", new Type[] {
            typeof(BaseSimObject), typeof(bool), typeof(bool)
        })]
        private static class Patch_BaseSimObject_CanAttackTarget
        {
            [HarmonyPostfix]
            private static void Postfix(BaseSimObject __instance, BaseSimObject pTarget, bool pCheckForFactions, bool pAttackBuildings, ref bool __result)
            {
                if (__result) return;
                if (__instance == null || !__instance.isAlive()) return;
                if (!xn.access.BaseSimObjectAccess.IsActor(__instance) || pTarget == null || !xn.access.BaseSimObjectAccess.IsActor(pTarget)) return;
                Actor actor = xn.access.BaseSimObjectAccess.GetActor(__instance);
                if (actor == null || actor.kingdom == null || actor.city == null) return;
                if (!HasTrait(actor, "path_01_demonic")) return;
                int active;
                xn.access.ActorAccess.GetData(actor).get(KEY_HUNT_ACTIVE, out active, 0);
                if (active != 1) return;
                long targetId;
                xn.access.ActorAccess.GetData(actor).get(KEY_HUNT_TARGET, out targetId, -1L);
                Actor target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
                if (target != null && xn.access.ActorAccess.GetData(target).id == targetId)
                {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "getHit", new Type[] {
            typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject),
            typeof(bool), typeof(bool), typeof(bool)
        })]
        private static class Patch_Actor_GetHit
        {
            [HarmonyPostfix]
            private static void Postfix(Actor __instance, float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker,
                                        bool pSkipIfShake, bool pMetallicWeapon, bool pCheckDamageReduction)
            {
                if (__instance == null || !__instance.isAlive()) return; 
                if (pAttacker == null) return;
                Actor attacker = xn.access.BaseSimObjectAccess.GetActor(pAttacker);
                if (attacker == null || !attacker.isAlive()) return;
                int active;
                xn.access.ActorAccess.GetData(attacker).get(KEY_HUNT_ACTIVE, out active, 0);
                if (active != 1) return;
                long targetId;
                xn.access.ActorAccess.GetData(attacker).get(KEY_HUNT_TARGET, out targetId, -1L);
                if (targetId != xn.access.ActorAccess.GetData(__instance).id) return;
                xn.access.ActorAccess.GetData(attacker).set(KEY_HUNT_ACTIVE, 0);
                s_activeHunters.Remove(xn.access.ActorAccess.GetData(attacker).id);
                string hunterName = attacker.getName();
                string targetName = __instance.getName();
                string text;
                if (attacker.isSexFemale())
                {
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                    {
                        text = hunterName + " drained " + targetName + "'s Yang essence to death";
                    }
                    else
                    {
                        text = hunterName + " caught " + targetName + " spying and drained his Yang essence";
                    }
                }
                else
                {
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                    {
                        text = hunterName + " seized " + targetName + "'s Yin essence by force";
                    }
                    else
                    {
                        text = hunterName + " forced " + targetName + " to bear offspring and stripped her Yin essence";
                    }
                }
                BroadcastSystem.PostActor(attacker, text);
                OnHuntSuccess(attacker);
            }
        }
        private static int s_nextCheckYear = 0;
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class Patch_Map_Update_CheckHunt
        {
            private static int s_lastCheckYear = -1;
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (!xn.config.ModConfigHooks.EnableDemonicHunt) return;
                int curYear = Date.getCurrentYear();
                if (curYear <= 0) return;
                if (curYear == s_lastCheckYear) return;
                s_lastCheckYear = curYear;
                if (s_nextCheckYear == 0)
                {
                    s_nextCheckYear = curYear + UnityEngine.Random.Range(10, 21);
                    return;
                }
                if (curYear < s_nextCheckYear) 
                {
                    CheckTaskDeadline(curYear);
                    return;
                }
                s_nextCheckYear = curYear + UnityEngine.Random.Range(10, 21);
                CheckTaskDeadline(curYear);
                var units = __instance.units?.getSimpleList();
                if (units == null || units.Count == 0) return;
                List<Actor> demonicCandidates = new List<Actor>();
                for (int i = 0; i < units.Count; i++)
                {
                    Actor a = units[i];
                    if (a == null || !a.isAlive()) continue;
                    if (a.kingdom == null || a.city == null) continue;
                    if (!HasTrait(a, "path_01_demonic")) continue;
                    int active;
                    xn.access.ActorAccess.GetData(a).get(KEY_HUNT_ACTIVE, out active, 0);
                    if (active == 1) continue;
                    int stop;
                    xn.access.ActorAccess.GetData(a).get("xn.cultivation.stop", out stop, 0);
                    if (stop == 1) continue;
                    int trialActive;
                    xn.access.ActorAccess.GetData(a).get("xn.trial.active", out trialActive, 0);
                    if (trialActive == 1) continue;
                    demonicCandidates.Add(a);
                }
                if (demonicCandidates.Count == 0) return;
                int count = UnityEngine.Random.Range(1, Mathf.Min(11, demonicCandidates.Count + 1));
                for (int i = 0; i < demonicCandidates.Count; i++)
                {
                    Actor temp = demonicCandidates[i];
                    int randomIndex = UnityEngine.Random.Range(i, demonicCandidates.Count);
                    demonicCandidates[i] = demonicCandidates[randomIndex];
                    demonicCandidates[randomIndex] = temp;
                }
                for (int i = 0; i < count && i < demonicCandidates.Count; i++)
                {
                    Actor a = demonicCandidates[i];
                    Actor target = FindSuitableTarget(a);
                    if (target == null) continue;
                    xn.access.ActorAccess.GetData(a).set(KEY_HUNT_TARGET, xn.access.ActorAccess.GetData(target).id);
                    xn.access.ActorAccess.GetData(a).set(KEY_HUNT_YEAR, curYear);
                    xn.access.ActorAccess.GetData(a).set(KEY_HUNT_ACTIVE, 1);
                    s_activeHunters.Add(xn.access.ActorAccess.GetData(a).id);
                    RegisterJob();
                    a.endJob();
                    xn.access.ActorAccess.GetAI(a)?.setJob("job_xn_demonic_hunt");
                    string hunterName = a.getName();
                    string targetName = target.getName();
                    string broadcastText;
                    if (a.isSexFemale())
                    {
                        broadcastText = hunterName + " gives in to desire and begins draining " + targetName;
                    }
                    else
                    {
                        broadcastText = hunterName + " grows hungry for power and begins preying on " + targetName;
                    }
                    BroadcastSystem.PostActor(a, broadcastText);
                }
            }
        }
        private static Actor FindSuitableTarget(Actor hunter)
        {
            if (hunter == null) return null;
            int hunterRealmIdx = GetCurrentRealmIndex(hunter);
            if (hunterRealmIdx < 0) return null;
            bool hunterIsFemale = hunter.isSexFemale();
            var units = World.world.units.getSimpleList();
            List<Actor> candidates = new List<Actor>();
            foreach (Actor a in units)
            {
                if (a == null || !a.isAlive()) continue;
                if (xn.access.ActorAccess.GetData(a).id == xn.access.ActorAccess.GetData(hunter).id) continue;
                if (a.kingdom == null) continue;
                if (hunterIsFemale)
                {
                    if (!a.isSexMale()) continue;
                }
                else
                {
                    if (!a.isSexFemale()) continue;
                }
                int targetRealmIdx = GetCurrentRealmIndex(a);
                if (targetRealmIdx < 0) continue;
                if (targetRealmIdx > hunterRealmIdx) continue;
                candidates.Add(a);
            }
            if (candidates.Count == 0) return null;
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
        private static int GetCurrentRealmIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            foreach (var t in list)
            {
                if (t != null && t.id != null)
                {
                    int idx;
                    if (REALM_INDEX_MAP.TryGetValue(t.id, out idx))
                    {
                        if (idx > cur) cur = idx;
                    }
                }
            }
            return cur;
        }
        private static void CheckTaskDeadline(int curYear)
        {
            if (s_activeHunters.Count == 0) return;
            List<Actor> toFail = null;
            List<long> toRemove = null;
            foreach (long id in s_activeHunters)
            {
                Actor a = World.world.units.get(id);
                if (a == null || !a.isAlive())
                {
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                    continue;
                }
                int active;
                xn.access.ActorAccess.GetData(a).get(KEY_HUNT_ACTIVE, out active, 0);
                if (active != 1)
                {
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                    continue;
                }
                int startYear;
                xn.access.ActorAccess.GetData(a).get(KEY_HUNT_YEAR, out startYear, 0);
                if (startYear <= 0) continue;
                if (curYear >= startYear + 10)
                {
                    if (toFail == null) toFail = new List<Actor>();
                    toFail.Add(a);
                }
            }
            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                    s_activeHunters.Remove(toRemove[i]);
            }
            if (toFail != null)
            {
                for (int i = 0; i < toFail.Count; i++)
                {
                    Actor a = toFail[i];
                    xn.access.ActorAccess.GetData(a).set(KEY_HUNT_ACTIVE, 0);
                    s_activeHunters.Remove(xn.access.ActorAccess.GetData(a).id);
                    string failText;
                    if (a.isSexFemale())
                    {
                        failText = a.getName() + " failed to drain Yang essence and was executed by Tian Yunzi";
                    }
                    else
                    {
                        failText = a.getName() + " failed to seize Yin essence and was executed by Tian Yunzi";
                    }
                    BroadcastSystem.PostActor(a, failText);
                    OnHuntFailed(a);
                }
            }
        }
        private static bool HasTrait(Actor a, string traitId)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t != null && t.id == traitId) return true;
            }
            return false;
        }
    }
}
