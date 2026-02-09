using System;
using HarmonyLib;
using ai;
using ai.behaviours;
using UnityEngine;
using xn.Traits;
using xn.world;
using xn.fx;
namespace cultivation.ai
{
    internal static class CondenseRootJob
    {
        private const string KEY_CONDENSE_READY = "xn.root.condense_ready"; 
        private const string KEY_CONDENSE_YEAR = "xn.root.condense_year";   
        private const string KEY_COEFF = "xn.root.coeff";                     
        private const string KEY_NEXT_TRY_YEAR = "xn.root.next_try_year";   
        private const string KEY_CITY_AURA = "xn.city.aura";                 
        private const string KEY_CITY_ROOT_USED = "xn.city.root.try_used";  
        private const string KEY_CONDENSE_RESULT = "xn.root.condense_result"; 
        private const string KEY_CONDENSE_ID = "xn.root.condense_id";         
        private static readonly string[] ROOT_IDS = new[]
        {
            "root_01_mortal", "root_02_low", "root_03_mid", 
            "root_04_high", "root_05_supreme", "root_06_tiandi"
        };
        private static readonly int[] ROOT_WEIGHTS = { 40, 30, 15, 8, 5, 1 };
        private static readonly Vector2[] ROOT_COEFF_RANGE = new[]
        {
            new Vector2(0.1f, 0.5f),   
            new Vector2(0.8f, 1.5f),   
            new Vector2(1.8f, 3.0f),   
            new Vector2(4.0f, 7.5f),   
            new Vector2(8.0f, 14.0f),  
            new Vector2(10.0f, 20.0f)  
        };
        private static readonly string[] INHERIT_IDS = new[]
        {
            "inherit_01_poor", "inherit_02_normal", "inherit_03_supreme",
            "inherit_04_tusi", "inherit_05_ancientblood"
        };
        private static readonly float[] INHERIT_WEIGHTS = { 50f, 30f, 15f, 4.5f, 0.5f };
        public static void Init()
        {
            RegisterJob();
            CondenseRootFX.Register();
        }
        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib.get("job_xn_condense_root") != null)
                return;
            RegisterCondenseTask();
            var job = new ActorJob { id = "job_xn_condense_root" };
            lib.add(job);
            job.addTask("task_xn_condense_root_stay");
            job.addTask("end_job");
        }
        private static void RegisterCondenseTask()
        {
            var taskLib = AssetManager.tasks_actor;
            if (taskLib.get("task_xn_condense_root_stay") != null)
                return;
            var task = new BehaviourTaskActor
            {
                id = "task_xn_condense_root_stay",
                locale_key = "task_xn_condense_root_stay"
            };
            taskLib.add(task);
            task.addBeh(new BehStartCondense());
            task.addBeh(new BehStayCondense(10f, 20f));
            task.addBeh(new BehDoCondense());
            task.addBeh(new BehStopCondense());
        }
        private class BehStartCondense : BehaviourActionActor
        {
            public override BehResult execute(Actor pActor)
            {
                if (pActor != null && pActor.isAlive())
                {
                    CondenseRootFX.StartFor(pActor);
                }
                return BehResult.Continue;
            }
        }
        private class BehStayCondense : BehaviourActionActor
        {
            private readonly float minSeconds;
            private readonly float maxSeconds;
            public BehStayCondense(float pMinSeconds, float pMaxSeconds)
            {
                minSeconds = pMinSeconds;
                maxSeconds = pMaxSeconds;
            }
            public override BehResult execute(Actor pActor)
            {
                if (pActor == null || !pActor.isAlive())
                    return BehResult.Stop;
                if (!hasStarted(pActor))
                {
                    float duration = UnityEngine.Random.Range(minSeconds, maxSeconds);
                    float endTime = Time.time + duration;
                    setTimer(pActor, endTime);
                    pActor.stopMovement();
                    pActor.timer_action = duration;
                    return BehResult.Continue;
                }
                float timer;
                pActor.data.get(getTimerKey(), out timer, 0f);
                if (Time.time >= timer)
                {
                    return BehResult.Continue; 
                }
                if (pActor.is_moving)
                {
                    pActor.stopMovement();
                }
                return BehResult.Continue; 
            }
            private bool hasStarted(Actor pActor)
            {
                float timer;
                pActor.data.get(getTimerKey(), out timer, 0f);
                return timer > 0f;
            }
            private void setTimer(Actor pActor, float endTime)
            {
                pActor.data.set(getTimerKey(), endTime);
            }
            private string getTimerKey()
            {
                return "xn.condense.stay_timer";
            }
        }
        private class BehDoCondense : BehaviourActionActor
        {
            public override BehResult execute(Actor pActor)
            {
                int curYear = Date.getCurrentYear();
                int condenseYear;
                pActor.data.get(KEY_CONDENSE_YEAR, out condenseYear, -1);
                if (condenseYear == curYear)
                {
                    TryDoCondense(pActor, curYear);
                }
                return BehResult.Continue;
            }
        }
        private class BehStopCondense : BehaviourActionActor
        {
            public override BehResult execute(Actor pActor)
            {
                if (pActor != null)
                {
                    CondenseRootFX.StopFor(pActor);
                }
                return BehResult.Continue;
            }
        }
        private static void TryDoCondense(Actor a, int curYear)
        {
            if (a == null || !a.isAlive()) return;
            a.data.set(KEY_CONDENSE_READY, 0);
            if (a.kingdom == null || a.city == null || a.is_inside_boat) return;
            if (HasAnySpiritRoot(a) || HasAnyAncientInheritance(a) || HasTraitId(a, "path_03_beast")) return;
            City c = a.city;
            if (c == null || c.data == null) return;
            int aura;
            c.data.get(KEY_CITY_AURA, out aura, 0);
            if (aura < 0) return; 
            int isMainChar;
            a.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            bool success;
            if (isMainChar == 1)
            {
                success = true;
            }
            else
            {
                success = UnityEngine.Random.value < 0.6f;
            }
            if (!success)
            {
                a.data.set(KEY_NEXT_TRY_YEAR, curYear + 30);
                return;
            }
            if (!HasTraitId(a, "path_03_beast") && !HasAnySpiritRoot(a) &&
                !HasAnyAncientInheritance(a) && UnityEngine.Random.value < 0.12f)
            {
                string inheritId = PickWeightedInheritId();
                var inh = AssetManager.traits.get(inheritId) as ActorTrait;
                if (inh != null)
                {
                    a.addTrait(inh);
                    a.data.set(KEY_NEXT_TRY_YEAR, 0);
                    return;
                }
            }
            if (UnityEngine.Random.value < 0.08f)
            {
                if (!IsHuman(a))
                {
                    var beast = AssetManager.traits.get("path_03_beast") as ActorTrait;
                    if (beast != null)
                    {
                        a.addTrait(beast);
                        RemoveAllSpiritRoots(a);
                        RemoveAllAncientInheritance(a);
                        a.data.set(KEY_NEXT_TRY_YEAR, 0);
                        return;
                    }
                }
            }
            string rootId = PickWeightedRootId();
            var trait = AssetManager.traits.get(rootId) as ActorTrait;
            if (trait != null) a.addTrait(trait);
            int idx = Array.IndexOf(ROOT_IDS, rootId);
            if (idx >= 0 && idx < ROOT_COEFF_RANGE.Length)
            {
                var range = ROOT_COEFF_RANGE[idx];
                float coeff = UnityEngine.Random.Range(range.x, range.y);
                a.data.set(KEY_COEFF, coeff);
            }
            a.data.set(KEY_NEXT_TRY_YEAR, 0);
        }
        [HarmonyPatch(typeof(Actor), "getNextJob")]
        private static class Patch_Actor_GetNextJob
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, ref string __result)
            {
                int curYear = Date.getCurrentYear();
                if (__instance == null || !__instance.isAlive()) return true;
                if (__instance.kingdom == null || __instance.city == null || __instance.is_inside_boat)
                    return true;
                if (xn.expand.FanjieKingdomTrait.ActorHasFanjieTrait(__instance))
                    return true;
                RegisterJob();
                int ready;
                __instance.data.get(KEY_CONDENSE_READY, out ready, 0);
                if (ready != 1) return true;
                int condenseYear;
                __instance.data.get(KEY_CONDENSE_YEAR, out condenseYear, -1);
                if (condenseYear == curYear) return true;
                __instance.data.set(KEY_CONDENSE_YEAR, curYear);
                __result = "job_xn_condense_root";
                return false;
            }
        }
        private static string PickWeightedRootId()
        {
            int sum = 0;
            for (int i = 0; i < ROOT_WEIGHTS.Length; i++) sum += ROOT_WEIGHTS[i];
            int r = UnityEngine.Random.Range(0, sum);
            for (int i = 0; i < ROOT_WEIGHTS.Length; i++)
            {
                if (r < ROOT_WEIGHTS[i]) return ROOT_IDS[i];
                r -= ROOT_WEIGHTS[i];
            }
            return ROOT_IDS[0];
        }
        private static string PickWeightedInheritId()
        {
            float sum = 0f;
            for (int i = 0; i < INHERIT_WEIGHTS.Length; i++) sum += INHERIT_WEIGHTS[i];
            float r = UnityEngine.Random.Range(0f, sum);
            for (int i = 0; i < INHERIT_WEIGHTS.Length; i++)
            {
                if (r < INHERIT_WEIGHTS[i]) return INHERIT_IDS[i];
                r -= INHERIT_WEIGHTS[i];
            }
            return INHERIT_IDS[0];
        }
        private static bool HasAnySpiritRoot(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t != null && t.group_id == RealmTraitGroup.GroupSpiritRoot)
                    return true;
            }
            return false;
        }
        private static bool HasAnyAncientInheritance(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list)
            {
                if (t != null && t.id != null && t.id.StartsWith("inherit_")) 
                    return true;
            }
            return false;
        }
        private static bool HasTraitId(Actor a, string id)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list) 
                if (t != null && t.id == id) return true;
            return false;
        }
        private static bool IsHuman(Actor a)
        {
            return a != null && a.asset != null && a.asset.id == "human";
        }
        private static void RemoveAllSpiritRoots(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return;
            System.Collections.Generic.List<ActorTrait> toRemove = 
                new System.Collections.Generic.List<ActorTrait>();
            foreach (var t in list)
            {
                if (t != null && t.group_id == RealmTraitGroup.GroupSpiritRoot)
                    toRemove.Add(t);
            }
            for (int i = 0; i < toRemove.Count; i++) 
                a.removeTrait(toRemove[i]);
            a.data.set(KEY_COEFF, 0f);
        }
        private static void RemoveAllAncientInheritance(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return;
            System.Collections.Generic.List<ActorTrait> toRemove = 
                new System.Collections.Generic.List<ActorTrait>();
            foreach (var t in list)
            {
                if (t != null && t.id != null && t.id.StartsWith("inherit_"))
                    toRemove.Add(t);
            }
            for (int i = 0; i < toRemove.Count; i++) 
                a.removeTrait(toRemove[i]);
        }
    }
}