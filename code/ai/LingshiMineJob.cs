using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ai;
using ai.behaviours;
using xn.Traits;
using xn.world;

namespace cultivation.ai
{
    internal static class LingshiMineJob
    {
        private const string JobId = "job_xn_lingshi_mine";
        private const string TaskId = "task_xn_lingshi_mine";
        private const string KeyActive = "xn.lingshi_mine.active";
        private const string KeyTargetId = "xn.lingshi_mine.target_id";
        private const string KeyReturning = "xn.lingshi_mine.returning";
        private const string KeyNextMineTime = "xn.lingshi_mine.next_time";
        private const string KeyVeinRemaining = "xn.lingshi_vein.remaining";
        private const string KeyPendingXp = "xn.stat.lingshi_pending_xp";
        private const string KeyXiuwei = "xn.stat.xiuwei";

        private const int InitialVeinUnits = 100;
        private const float MiningIntervalSeconds = 1.5f;
        private const float MiningDistanceSqr = 9f;
        private const float DepositDistanceSqr = 16f;

        private static readonly string[] RealmIds = new[]
        {
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

        private static readonly long[] RealmThresholds = new long[]
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

        public static void Init()
        {
            RegisterJob();
        }

        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib == null || lib.get(JobId) != null)
            {
                return;
            }

            RegisterTask();
            ActorJob job = new ActorJob { id = JobId };
            lib.add(job);
            job.addTask(TaskId);
            job.addTask("end_job");
        }

        private static void RegisterTask()
        {
            var taskLib = AssetManager.tasks_actor;
            if (taskLib == null || taskLib.get(TaskId) != null)
            {
                return;
            }

            BehaviourTaskActor task = new BehaviourTaskActor
            {
                id = TaskId,
                locale_key = TaskId
            };
            taskLib.add(task);
            task.addBeh(new BehMineLingshiVein());
            task.addBeh(new BehRestartTask());
        }

        private sealed class BehMineLingshiVein : BehaviourActionActor
        {
            public override BehResult execute(Actor actor)
            {
                if (actor == null || !actor.isAlive())
                {
                    return BehResult.Stop;
                }

                int returning;
                xn.access.ActorAccess.GetData(actor).get(KeyReturning, out returning, 0);
                if (returning == 1)
                {
                    return ReturnHomeAndDeposit(actor);
                }

                Building vein = GetTargetVein(actor);
                if (vein == null || !vein.isAlive())
                {
                    return ReturnHomeAndDeposit(actor);
                }

                if (actor.current_tile == null || vein.current_tile == null || !actor.current_tile.isSameIsland(vein.current_tile))
                {
                    Clear(actor);
                    return BehResult.Stop;
                }

                float dist = Toolbox.SquaredDist(actor.current_tile.x, actor.current_tile.y, vein.current_tile.x, vein.current_tile.y);
                if (dist > MiningDistanceSqr)
                {
                    if (!xn.access.ActorAccess.IsUsingPath(actor))
                    {
                        actor.goTo(vein.current_tile);
                    }
                    return BehResult.Continue;
                }

                actor.stopMovement();
                float nextMineTime;
                xn.access.ActorAccess.GetData(actor).get(KeyNextMineTime, out nextMineTime, 0f);
                if (Time.time < nextMineTime)
                {
                    return BehResult.Continue;
                }

                xn.access.ActorAccess.GetData(actor).set(KeyNextMineTime, Time.time + MiningIntervalSeconds);
                MineOnce(actor, vein);
                return BehResult.Continue;
            }
        }

        private static void MineOnce(Actor actor, Building vein)
        {
            BuildingData veinData = xn.access.BuildingAccess.GetData(vein);
            if (veinData == null)
            {
                Clear(actor);
                return;
            }

            int remaining;
            veinData.get(KeyVeinRemaining, out remaining, InitialVeinUnits);
            if (remaining <= 0)
            {
                DepleteVein(actor, vein);
                return;
            }

            int yield = Mathf.Min(remaining, GetLingshiYield(actor));
            if (yield <= 0)
            {
                yield = 1;
            }

            remaining -= yield;
            veinData.set(KeyVeinRemaining, remaining);
            actor.addToInventory(xn.assets.XNResourceRegistry.LingshiResourceId, yield);

            long pending;
            xn.access.ActorAccess.GetData(actor).get(KeyPendingXp, out pending, 0L);
            xn.access.ActorAccess.GetData(actor).set(KeyPendingXp, pending + yield * 1000L);

            if (remaining <= 0)
            {
                DepleteVein(actor, vein);
            }
        }

        private static void DepleteVein(Actor actor, Building vein)
        {
            if (vein != null)
            {
                xn.access.BuildingAccess.RemoveBuildingFinal(vein);
            }

            xn.access.ActorAccess.GetData(actor).set(KeyReturning, 1);
        }

        private static BehResult ReturnHomeAndDeposit(Actor actor)
        {
            if (actor == null || !actor.isAlive())
            {
                return BehResult.Stop;
            }

            City city = actor.city;
            if (city == null || city.isRekt())
            {
                Clear(actor);
                return BehResult.Stop;
            }

            WorldTile cityTile = city.getTile(false);
            if (cityTile == null || actor.current_tile == null)
            {
                Clear(actor);
                return BehResult.Stop;
            }

            float dist = Toolbox.SquaredDist(actor.current_tile.x, actor.current_tile.y, cityTile.x, cityTile.y);
            if (dist > DepositDistanceSqr)
            {
                if (!xn.access.ActorAccess.IsUsingPath(actor))
                {
                    actor.goTo(cityTile);
                }
                return BehResult.Continue;
            }

            actor.giveInventoryResourcesToCity();
            Clear(actor);
            return BehResult.Stop;
        }

        private static Building GetTargetVein(Actor actor)
        {
            long targetId;
            xn.access.ActorAccess.GetData(actor).get(KeyTargetId, out targetId, 0L);
            if (targetId <= 0L || World.world == null)
            {
                return null;
            }

            Building building = World.world.buildings.get(targetId);
            return IsLingshiVein(building) ? building : null;
        }

        private static bool TryAssignTarget(Actor actor)
        {
            if (actor == null || !actor.isAlive() || World.world == null)
            {
                return false;
            }
            if (actor.current_tile == null || xn.access.ActorAccess.IsInsideBoat(actor))
            {
                return false;
            }
            if (!HasCultivationTrait(actor))
            {
                return false;
            }

            float chance = IsAtNormalRealmCap(actor) ? 0.005f : 0.03f;
            if (!Randy.randomChance(chance))
            {
                return false;
            }

            Building target = FindBestReachableVein(actor);
            if (target == null)
            {
                return false;
            }

            BuildingData targetData = xn.access.BuildingAccess.GetData(target);
            if (targetData == null)
            {
                return false;
            }

            RegisterJob();
            xn.access.ActorAccess.GetData(actor).set(KeyTargetId, targetData.id);
            xn.access.ActorAccess.GetData(actor).set(KeyActive, 1);
            xn.access.ActorAccess.GetData(actor).set(KeyReturning, 0);
            return true;
        }

        private static Building FindBestReachableVein(Actor actor)
        {
            List<Building> buildings = World.world.buildings.getSimpleList();
            if (buildings == null || buildings.Count == 0)
            {
                return null;
            }

            Building best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building building = buildings[i];
                if (!IsLingshiVein(building))
                {
                    continue;
                }
                if (building.current_tile == null || actor.current_tile == null || !actor.current_tile.isSameIsland(building.current_tile))
                {
                    continue;
                }

                float dist = Toolbox.SquaredDist(actor.current_tile.x, actor.current_tile.y, building.current_tile.x, building.current_tile.y);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = building;
                }
            }

            return best;
        }

        private static bool IsLingshiVein(Building building)
        {
            BuildingAsset asset = xn.access.BuildingAccess.GetAsset(building);
            return building != null && building.isAlive() && asset != null && asset.id == LingshiVeinAssets.ID;
        }

        private static int GetLingshiYield(Actor actor)
        {
            int realm = GetHighestRealmLikeIndex(actor);
            if (realm < 0) return 1;
            if (realm <= 2) return 2;
            if (realm <= 4) return 4;
            if (realm <= 6) return 8;
            if (realm <= 8) return 14;
            return 25;
        }

        private static bool HasCultivationTrait(Actor actor)
        {
            var traits = actor.getTraits();
            if (traits == null)
            {
                return false;
            }

            foreach (ActorTrait trait in traits)
            {
                if (trait == null)
                {
                    continue;
                }
                if (trait.group_id == RealmTraitGroup.GroupRealm ||
                    trait.group_id == RealmTraitGroup.GroupAncientRealm ||
                    trait.group_id == RealmTraitGroup.GroupBeastStage)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetHighestRealmLikeIndex(Actor actor)
        {
            int normalRealm = GetCurrentNormalRealmIndex(actor);
            int best = normalRealm;
            var traits = actor.getTraits();
            if (traits == null)
            {
                return best;
            }

            foreach (ActorTrait trait in traits)
            {
                if (trait == null || string.IsNullOrEmpty(trait.id))
                {
                    continue;
                }
                if (trait.group_id == RealmTraitGroup.GroupAncientRealm || trait.group_id == RealmTraitGroup.GroupBeastStage)
                {
                    int underscore = trait.id.LastIndexOf('_');
                    string numericPart = underscore > 0 ? trait.id.Substring(underscore - 2, 2) : "";
                    int parsed;
                    if (int.TryParse(numericPart, out parsed))
                    {
                        best = Math.Max(best, parsed - 1);
                    }
                }
            }

            return best;
        }

        private static bool IsAtNormalRealmCap(Actor actor)
        {
            int current = GetCurrentNormalRealmIndex(actor);
            int next = current + 1;
            if (next < 0 || next >= RealmThresholds.Length)
            {
                return false;
            }

            long xp;
            xn.access.ActorAccess.GetData(actor).get(KeyXiuwei, out xp, 0L);
            return xp >= RealmThresholds[next];
        }

        private static int GetCurrentNormalRealmIndex(Actor actor)
        {
            var traits = actor.getTraits();
            if (traits == null)
            {
                return -1;
            }

            int current = -1;
            for (int i = 0; i < RealmIds.Length; i++)
            {
                foreach (ActorTrait trait in traits)
                {
                    if (trait != null && trait.id == RealmIds[i])
                    {
                        current = Math.Max(current, i);
                    }
                }
            }

            return current;
        }

        private static void Clear(Actor actor)
        {
            if (actor == null)
            {
                return;
            }

            xn.access.ActorAccess.GetData(actor).set(KeyActive, 0);
            xn.access.ActorAccess.GetData(actor).set(KeyTargetId, 0L);
            xn.access.ActorAccess.GetData(actor).set(KeyReturning, 0);
            xn.access.ActorAccess.GetData(actor).set(KeyNextMineTime, 0f);
        }

        [HarmonyPatch(typeof(Actor), "getNextJob")]
        private static class PatchActorGetNextJob
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, ref string __result)
            {
                if (__instance == null || !__instance.isAlive())
                {
                    return true;
                }

                int active;
                xn.access.ActorAccess.GetData(__instance).get(KeyActive, out active, 0);
                if (active == 1)
                {
                    __result = JobId;
                    return false;
                }

                if (!TryAssignTarget(__instance))
                {
                    return true;
                }

                __result = JobId;
                return false;
            }
        }

        [HarmonyPatch(typeof(Actor), "setTask", new Type[] {
            typeof(string), typeof(bool), typeof(bool), typeof(bool)
        })]
        private static class PatchActorSetTask
        {
            [HarmonyPrefix]
            private static void Prefix(Actor __instance, string pTaskId, bool pClean, ref bool pCleanJob, bool pForceAction)
            {
                if (__instance == null || !__instance.isAlive())
                {
                    return;
                }

                int active;
                xn.access.ActorAccess.GetData(__instance).get(KeyActive, out active, 0);
                if (active != 1)
                {
                    return;
                }

                var ai = xn.access.ActorAccess.GetAI(__instance);
                if (ai != null && ai.job != null && ai.job.id == JobId && pCleanJob)
                {
                    pCleanJob = false;
                }
            }
        }
    }
}
