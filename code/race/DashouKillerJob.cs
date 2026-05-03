using HarmonyLib;
using ai;
using ai.behaviours;
using UnityEngine;
using System.Collections.Generic;
namespace xn.race
{
    internal static class DashouKillerJob
    {
        private const string JOB_ID = "job_dashou_killer";
        private const string TASK_ID = "task_dashou_find_and_kill";
        public static void Init()
        {
            RegisterTask();
            RegisterJob();
        }
        private static void RegisterBehavior()
        {
        }
        private static void RegisterTask()
        {
            var lib = AssetManager.tasks_actor;
            if (lib.get(TASK_ID) != null) return;
            var task = new BehaviourTaskActor { id = TASK_ID };
            lib.add(task);
            task.addBeh(new BehDashouFindAnyTarget());
            task.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameTile, pPathOnWater: true, pCheckCanAttackTarget: false, pCalibrateTargetPosition: true));
            task.addBeh(new BehAttackActorHuntingTarget());
            task.addBeh(new BehRestartTask());
        }
        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib.get(JOB_ID) != null) return;
            var job = new ActorJob { id = JOB_ID };
            lib.add(job);
            job.addTask(TASK_ID);
        }
    }
    internal class BehDashouFindAnyTarget : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            if (xn.access.ActorAccess.GetBehActorTarget(pActor) != null && xn.access.ActorAccess.GetBehActorTarget(pActor).isAlive() && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetBehActorTarget(pActor)))
            {
                return BehResult.Continue;
            }
            Actor target = FindClosestAnyActor(pActor);
            if (target != null)
            {
                xn.access.ActorAccess.SetBehActorTarget(pActor, target);
                return BehResult.Continue;
            }
            pActor.makeWait(1f);
            return BehResult.Continue;
        }
        private Actor FindClosestAnyActor(Actor pActor)
        {
            float minDist = float.MaxValue;
            Actor closest = null;
            bool pRandom = Randy.randomBool();
            int pChunkRadius = 5; 
            foreach (Actor other in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, 0f, pRandom))
            {
                if (other == null || other == pActor || !other.isAlive())
                    continue;
                if (other.asset != null && other.asset.id == "dashou")
                    continue;
                if (!pActor.isSameIslandAs(other))
                    continue;
                if (!xn.access.BaseSimObjectAccess.CanAttackTarget(pActor, other))
                    continue;
                float dist = Toolbox.SquaredDistTile(other.current_tile, pActor.current_tile);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = other;
                    if (pRandom && Randy.randomBool())
                    {
                        return closest; 
                    }
                }
            }
            return closest;
        }
    }
}
