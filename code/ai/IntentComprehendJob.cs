using System;
using HarmonyLib;
using ai;
using ai.behaviours;
using UnityEngine;
using xn.Traits;
using xn.world;
namespace cultivation.ai
{
    internal static class IntentComprehendJob
    {
        private const string DECISION_INTENT_COMPREHEND = "xn_decision_intent_comprehend";
        private const string KEY_LV_ACTIVE   = "xn.intent.lv_active";          
        private const string KEY_LV_END_T    = "xn.intent.lv_end_t";           
        private const string KEY_LV_CD_UNTIL = "xn.intent.lv_cd_until_year";   
        private static readonly string[] INTENT_IDS = {
            "intent_02_angel","intent_03_qianhuan","intent_04_killing","intent_05_reverse",
            "intent_06_life_death","intent_07_reincarnation","intent_08_chaos","intent_09_madness"
        }; 
        private static readonly string[] REALM_IDS = {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        public static void Init()
        {
            RegisterJob();
        }
        private static void RegisterJob()
        {
            var lib = AssetManager.job_actor;
            if (lib.get("job_xn_intent_comprehend") != null) return;
            RegisterIntentComprehendTask();
            var job = new ActorJob { id = "job_xn_intent_comprehend" };
            lib.add(job);
            job.addTask("task_xn_intent_comprehend_stay");
            job.addTask("end_job");
        }
        private static void RegisterIntentComprehendTask()
        {
            var taskLib = AssetManager.tasks_actor;
            if (taskLib.get("task_xn_intent_comprehend_stay") != null)
                return;
            var task = new BehaviourTaskActor 
            { 
                id = "task_xn_intent_comprehend_stay",
                locale_key = "task_xn_intent_comprehend_stay"
            };
            taskLib.add(task);
            task.addBeh(new BehBuildingTargetHome());
            task.addBeh(new BehGetTargetBuildingMainTile());
            task.addBeh(new BehGoToTileTarget());
            task.addBeh(new BehStayInBuildingTarget(30f, 60f));
            task.addBeh(new BehRestoreStats(0.1f, 0.2f));
            task.addBeh(new BehExitBuilding());
        }
        internal static void BeginComprehension(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;
            RegisterJob();
            ActorData data = xn.access.ActorAccess.GetData(actor);
            if (data == null) return;
            data.set(KEY_LV_ACTIVE, 1);
            float dur = UnityEngine.Random.Range(30f, 60f);
            data.set(KEY_LV_END_T, Time.time + dur);
        }
        private static bool HasAnyIntent(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list) { if (t != null && t.group_id == RealmTraitGroup.GroupIntent) return true; }
            return false;
        }
        private static int GetRealmIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
            {
                foreach (var t in list) if (t != null && t.id == REALM_IDS[i]) { if (i > cur) cur = i; }
            }
            return cur;
        }
        private static string GetActorDataName(Actor actor)
        {
            ActorData data = xn.access.ActorAccess.GetData(actor);
            return data != null ? data.name : "";
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class Patch_Tick_Comprehend
        {
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (__instance == null || __instance.units == null) return;
                var list = __instance.units.getSimpleList();
                if (list == null || list.Count == 0) return;
                float now = Time.time;
                int year = Date.getCurrentYear();
                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (a == null || !a.isAlive()) continue;
                    int active; xn.access.ActorAccess.GetData(a).get(KEY_LV_ACTIVE, out active, 0);
                    if (active != 1) continue;
                    if (!xn.access.ActorAccess.IsInsideBuilding(a) || xn.access.ActorAccess.GetInsideBuilding(a) == null) continue;
                    float endt; xn.access.ActorAccess.GetData(a).get(KEY_LV_END_T, out endt, 0f);
                    if (endt <= 0f || now < endt) continue;
                    xn.access.ActorAccess.GetData(a).set(KEY_LV_ACTIVE, 0);
                    xn.access.ActorAccess.GetData(a).set(KEY_LV_END_T, 0f);
                    bool ok = (UnityEngine.Random.value < 0.30f);
                    if (ok)
                    {
                        int pick = UnityEngine.Random.Range(0, INTENT_IDS.Length);
                        var t = AssetManager.traits.get(INTENT_IDS[pick]) as ActorTrait;
                        if (t != null) a.addTrait(t);
                        BroadcastSystem.IntentGain(a, INTENT_IDS[pick]);
                    }
                    else
                    {
                        int loss = Mathf.FloorToInt(a.getMaxHealth() * 0.30f);
                        if (loss > 0) a.changeHealth(-loss);
                        if (!a.hasHealth()) a.batch.c_check_deaths.Add(a);
                        xn.access.ActorAccess.GetData(a).set(KEY_LV_CD_UNTIL, year + 50);
                        BroadcastSystem.IntentComprehendFail(a);
                    }
                }
            }
        }
    }
}
