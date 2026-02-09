using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.world;
using ai.behaviours;
using AiBehaviours = ai.behaviours;
namespace cultivation.ai
{
    internal static class TianyunziJob
    {
        private const string KEY_TYZ_FLAG = "xn_is_tianyunzi";
        public static void Init()
        {
            RegisterJob();
        }
        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib.get("job_xn_tianyunzi") != null)
                return;
            RegisterHuntTask();
            var job = new ActorJob { id = "job_xn_tianyunzi" };
            lib.add(job);
            job.addTask("task_xn_tianyunzi_hunt");
        }
        private static void RegisterHuntTask()
        {
            var taskLib = AssetManager.tasks_actor;
            if (taskLib.get("task_xn_tianyunzi_hunt") != null)
                return;
            var task = new BehaviourTaskActor
            {
                id = "task_xn_tianyunzi_hunt",
                locale_key = "task_xn_tianyunzi_hunt"
            };
            taskLib.add(task);
            task.addBeh(new BehFindAndAttackTarget());
            task.addBeh(new AiBehaviours.BehRestartTask());
        }
        private class BehFindAndAttackTarget : BehaviourActionActor
        {
            public override BehResult execute(Actor pActor)
            {
                if (pActor == null || !pActor.isAlive()) return BehResult.Stop;
                BaseSimObject targetObj = pActor.beh_actor_target;
                Actor currentTarget = (targetObj != null && targetObj.isActor()) ? targetObj.a : null;
                if (currentTarget != null && currentTarget.isAlive())
                {
                    pActor.setAttackTarget(currentTarget);
                    if (pActor.isInAttackRange(currentTarget))
                    {
                        pActor.tryToAttack(currentTarget);
                    }
                    else
                    {
                        if (!pActor.isUsingPath() || pActor.beh_actor_target != targetObj)
                        {
                            pActor.beh_actor_target = targetObj;
                            pActor.goTo(currentTarget.current_tile);
                        }
                    }
                    return BehResult.Continue;
                }
                Actor newTarget = FindNearestVisibleTarget(pActor);
                if (newTarget != null)
                {
                    pActor.beh_actor_target = newTarget;
                    pActor.setAttackTarget(newTarget);
                    if (pActor.isInAttackRange(newTarget))
                    {
                        pActor.tryToAttack(newTarget);
                    }
                    else
                    {
                        pActor.goTo(newTarget.current_tile);
                    }
                    return BehResult.Continue;
                }
                if (!pActor.isUsingPath())
                {
                    WorldTile randomTile = GetRandomNearbyTile(pActor);
                    if (randomTile != null)
                    {
                        pActor.goTo(randomTile);
                    }
                }
                return BehResult.Continue;
            }
            private Actor FindNearestVisibleTarget(Actor tianyunzi)
            {
                if (tianyunzi == null) return null;
                float minDist = float.MaxValue;
                Actor nearest = null;
                foreach (var u in World.world.units)
                {
                    if (u == null || !u.isAlive()) continue;
                    if (u == tianyunzi) continue;
                    float dist = Toolbox.DistVec2Float(tianyunzi.current_position, u.current_position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = u;
                    }
                }
                return nearest;
            }
            private WorldTile GetRandomNearbyTile(Actor a)
            {
                if (a == null || a.current_tile == null) return null;
                int offsetX = UnityEngine.Random.Range(-10, 11);
                int offsetY = UnityEngine.Random.Range(-10, 11);
                int targetX = a.current_tile.x + offsetX;
                int targetY = a.current_tile.y + offsetY;
                return World.world.GetTile(targetX, targetY);
            }
        }
        [HarmonyPatch(typeof(Actor), "getNextJob")]
        private static class Patch_Actor_GetNextJob
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, ref string __result)
            {
                if (__instance == null || !__instance.isAlive()) return true;
                int isTianyunzi;
                __instance.data.get(KEY_TYZ_FLAG, out isTianyunzi, 0);
                if (isTianyunzi != 1) return true;
                int stop;
                __instance.data.get("xn.cultivation.stop", out stop, 0);
                if (stop == 1) return true;
                int trialActive;
                __instance.data.get("xn.trial.active", out trialActive, 0);
                if (trialActive == 1) return true;
                RegisterJob();
                __result = "job_xn_tianyunzi";
                return false;
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
                int isTianyunzi;
                __instance.data.get(KEY_TYZ_FLAG, out isTianyunzi, 0);
                if (isTianyunzi != 1) return;
                if (__instance.ai.job != null && __instance.ai.job.id == "job_xn_tianyunzi")
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
                if (!__instance.isActor() || pTarget == null || !pTarget.isActor()) return;
                Actor actor = __instance.a;
                if (actor == null) return;
                int isTianyunzi;
                actor.data.get(KEY_TYZ_FLAG, out isTianyunzi, 0);
                if (isTianyunzi != 1) return;
                Actor target = pTarget.a;
                if (target != null && target.isAlive())
                {
                    if (target != actor)
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}