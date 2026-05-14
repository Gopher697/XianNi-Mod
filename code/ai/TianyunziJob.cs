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
                BaseSimObject targetObj = xn.access.ActorAccess.GetBehActorTarget(pActor);
                Actor currentTarget = (targetObj != null && xn.access.BaseSimObjectAccess.IsActor(targetObj)) ? xn.access.BaseSimObjectAccess.GetActor(targetObj) : null;
                if (currentTarget != null && currentTarget.isAlive())
                {
                    pActor.setAttackTarget(currentTarget);
                    if (xn.access.ActorAccess.IsInAttackRange(pActor, currentTarget))
                    {
                        xn.access.ActorAccess.TryToAttack(pActor, currentTarget);
                    }
                    else
                    {
                        if (!xn.access.ActorAccess.IsUsingPath(pActor) || xn.access.ActorAccess.GetBehActorTarget(pActor) != targetObj)
                        {
                            xn.access.ActorAccess.SetBehActorTarget(pActor, targetObj);
                            pActor.goTo(currentTarget.current_tile);
                        }
                    }
                    return BehResult.Continue;
                }
                Actor newTarget = FindNearestVisibleTarget(pActor);
                if (newTarget != null)
                {
                    xn.access.ActorAccess.SetBehActorTarget(pActor, newTarget);
                    pActor.setAttackTarget(newTarget);
                    if (xn.access.ActorAccess.IsInAttackRange(pActor, newTarget))
                    {
                        xn.access.ActorAccess.TryToAttack(pActor, newTarget);
                    }
                    else
                    {
                        pActor.goTo(newTarget.current_tile);
                    }
                    return BehResult.Continue;
                }
                if (!xn.access.ActorAccess.IsUsingPath(pActor))
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
                xn.access.ActorAccess.GetData(__instance).get(KEY_TYZ_FLAG, out isTianyunzi, 0);
                if (isTianyunzi != 1) return true;
                int stop;
                xn.access.ActorAccess.GetData(__instance).get("xn.cultivation.stop", out stop, 0);
                if (stop == 1) return true;
                int trialActive;
                xn.access.ActorAccess.GetData(__instance).get("xn.trial.active", out trialActive, 0);
                if (trialActive == 1) return true;
                RegisterJob();
                Debug.Log("[XN S2] TianyunziJob prefix FIRE actor=" + xn.access.ActorAccess.GetData(__instance)?.name);
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
                xn.access.ActorAccess.GetData(__instance).get(KEY_TYZ_FLAG, out isTianyunzi, 0);
                if (isTianyunzi != 1) return;
                var actorAI = xn.access.ActorAccess.GetAI(__instance);
                if (actorAI != null && actorAI.job != null && actorAI.job.id == "job_xn_tianyunzi")
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
                if (actor == null) return;
                int isTianyunzi;
                xn.access.ActorAccess.GetData(actor).get(KEY_TYZ_FLAG, out isTianyunzi, 0);
                if (isTianyunzi != 1) return;
                Actor target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
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
