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
                    xn.access.ActorAccess.GetData(pActor).set(KEY_CONDENSE_YEAR, Date.getCurrentYear());
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
                    xn.access.ActorAccess.SetTimerAction(pActor, duration);
                    return BehResult.Continue;
                }
                float timer;
                xn.access.ActorAccess.GetData(pActor).get(getTimerKey(), out timer, 0f);
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
                xn.access.ActorAccess.GetData(pActor).get(getTimerKey(), out timer, 0f);
                return timer > 0f;
            }
            private void setTimer(Actor pActor, float endTime)
            {
                xn.access.ActorAccess.GetData(pActor).set(getTimerKey(), endTime);
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
                xn.access.ActorAccess.GetData(pActor).get(KEY_CONDENSE_YEAR, out condenseYear, -1);
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
            xn.access.ActorAccess.GetData(a).set(KEY_CONDENSE_READY, 0);
            if (a.kingdom == null || a.city == null || xn.access.ActorAccess.IsInsideBoat(a)) return;
            if (HasAnySpiritRoot(a) || HasAnyAncientInheritance(a) || HasTraitId(a, "path_03_beast")) return;
            City c = a.city;
            if (c == null || c.data == null) return;
            int aura;
            c.data.get(KEY_CITY_AURA, out aura, 0);
            if (aura < 0) return;

            int age = GetActorAge(a);
            float ageSuccessModifier = GetAgeSuccessModifier(age);
            int ageCooldown = GetAgeRetryCooldown(age);

            float traitSuccessDelta = GetTraitSuccessDelta(a);

            int isMainChar;
            xn.access.ActorAccess.GetData(a).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);

            bool success;
            if (isMainChar == 1)
            {
                success = true;
            }
            else
            {
                float baseRate = 0.60f * ageSuccessModifier;
                float finalRate = Mathf.Clamp(baseRate + traitSuccessDelta, 0.05f, 0.95f);
                success = UnityEngine.Random.value < finalRate;
            }

            if (!success)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_NEXT_TRY_YEAR, curYear + ageCooldown);
                return;
            }

            float inheritChance = 0.12f + GetTraitInheritBoost(a);
            if (!HasTraitId(a, "path_03_beast") && !HasAnySpiritRoot(a) &&
                !HasAnyAncientInheritance(a) && UnityEngine.Random.value < inheritChance)
            {
                string inheritId = PickWeightedInheritId();
                var inh = AssetManager.traits.get(inheritId) as ActorTrait;
                if (inh != null)
                {
                    a.addTrait(inh);
                    xn.access.ActorAccess.GetData(a).set(KEY_NEXT_TRY_YEAR, 0);
                    return;
                }
            }

            float beastChance = 0.08f + GetTraitBeastBoost(a);
            if (UnityEngine.Random.value < beastChance)
            {
                if (!IsHuman(a))
                {
                    var beast = AssetManager.traits.get("path_03_beast") as ActorTrait;
                    if (beast != null)
                    {
                        a.addTrait(beast);
                        RemoveAllSpiritRoots(a);
                        RemoveAllAncientInheritance(a);
                        xn.access.ActorAccess.GetData(a).set(KEY_NEXT_TRY_YEAR, 0);
                        return;
                    }
                }
            }

            string rootId = PickWeightedRootIdWithTraits(a);
            var trait = AssetManager.traits.get(rootId) as ActorTrait;
            if (trait != null) a.addTrait(trait);
            int idx = Array.IndexOf(ROOT_IDS, rootId);
            if (idx >= 0 && idx < ROOT_COEFF_RANGE.Length)
            {
                var range = ROOT_COEFF_RANGE[idx];
                float coeff = UnityEngine.Random.Range(range.x, range.y);
                xn.access.ActorAccess.GetData(a).set(KEY_COEFF, coeff);
            }
            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_TRY_YEAR, 0);
        }
        private static int GetActorAge(Actor a)
        {
            if (a == null) return 0;
            try { return a.getAge(); }
            catch
            {
                ActorData data = xn.access.ActorAccess.GetData(a);
                return data != null ? data.getAge() : 0;
            }
        }

        private static float GetAgeSuccessModifier(int age)
        {
            if (age < 15) return 0.50f;
            if (age < 26) return 0.70f;
            if (age < 51) return 1.00f;
            if (age < 81) return 0.85f;
            if (age < 121) return 0.70f;
            return 0.55f;
        }

        private static int GetAgeRetryCooldown(int age)
        {
            if (age < 20) return 10;
            if (age < 51) return 15;
            if (age < 81) return 20;
            if (age < 121) return 25;
            return 35;
        }

        /// <summary>
        /// Returns an additive delta applied to the base 0.60 success rate.
        /// Native WorldBox trait IDs were confirmed by assembly audit (2026-05-13).
        /// </summary>
        private static float GetTraitSuccessDelta(Actor a)
        {
            float delta = 0f;

            if (HasTraitId(a, "genius")) delta += 0.15f;
            if (HasTraitId(a, "strong_minded")) delta += 0.10f;
            if (HasTraitId(a, "heart_of_wizard")) delta += 0.08f;
            if (HasTraitId(a, "immortal")) delta += 0.07f;
            if (HasTraitId(a, "ambitious")) delta += 0.06f;
            if (HasTraitId(a, "titan_lungs")) delta += 0.06f;
            if (HasTraitId(a, "boosted_vitality")) delta += 0.05f;
            if (HasTraitId(a, "regeneration")) delta += 0.05f;
            if (HasTraitId(a, "healing_aura")) delta += 0.05f;
            if (HasTraitId(a, "peaceful")) delta += 0.04f;
            if (HasTraitId(a, "pacifist")) delta += 0.04f;
            if (HasTraitId(a, "immune")) delta += 0.04f;
            if (HasTraitId(a, "content")) delta += 0.04f;
            if (HasTraitId(a, "tough")) delta += 0.04f;
            if (HasTraitId(a, "strong")) delta += 0.03f;
            if (HasTraitId(a, "long_liver")) delta += 0.03f;
            if (HasTraitId(a, "veteran")) delta += 0.03f;
            if (HasTraitId(a, "honest")) delta += 0.03f;
            if (HasTraitId(a, "agile")) delta += 0.03f;
            if (HasTraitId(a, "fertile")) delta += 0.02f;
            if (HasTraitId(a, "flower_prints")) delta += 0.02f;

            if (HasTraitId(a, "plague")) delta -= 0.12f;
            if (HasTraitId(a, "tumor_infection")) delta -= 0.12f;
            if (HasTraitId(a, "crippled")) delta -= 0.08f;
            if (HasTraitId(a, "hotheaded")) delta -= 0.08f;
            if (HasTraitId(a, "paranoid")) delta -= 0.07f;
            if (HasTraitId(a, "weak")) delta -= 0.06f;
            if (HasTraitId(a, "fragile_health")) delta -= 0.06f;
            if (HasTraitId(a, "gluttonous")) delta -= 0.06f;
            if (HasTraitId(a, "infected")) delta -= 0.06f;
            if (HasTraitId(a, "mush_spores")) delta -= 0.06f;
            if (HasTraitId(a, "pyromaniac")) delta -= 0.05f;
            if (HasTraitId(a, "clumsy")) delta -= 0.04f;
            if (HasTraitId(a, "fat")) delta -= 0.04f;
            if (HasTraitId(a, "tiny")) delta -= 0.03f;
            if (HasTraitId(a, "slow")) delta -= 0.03f;
            if (HasTraitId(a, "soft_skin")) delta -= 0.03f;
            if (HasTraitId(a, "heliophobia")) delta -= 0.03f;
            if (HasTraitId(a, "infertile")) delta -= 0.03f;
            if (HasTraitId(a, "eyepatch")) delta -= 0.02f;
            if (HasTraitId(a, "skin_burns")) delta -= 0.02f;

            return delta;
        }

        private static float GetQualityBias(Actor a)
        {
            float bias = 0f;

            int qiyun;
            xn.access.ActorAccess.GetData(a).get("xn.stat.qiyun", out qiyun, 50);
            bias += (qiyun - 50) * 0.30f;

            if (HasTraitId(a, "lucky")) bias += 12f;
            if (HasTraitId(a, "heart_of_wizard")) bias += 10f;
            if (HasTraitId(a, "blessed")) bias += 8f;
            if (HasTraitId(a, "wise")) bias += 8f;
            if (HasTraitId(a, "arcane_reflexes")) bias += 6f;
            if (HasTraitId(a, "sunblessed")) bias += 5f;
            if (HasTraitId(a, "eagle_eyed")) bias += 4f;
            if (HasTraitId(a, "moonchild")) bias += 4f;
            if (HasTraitId(a, "nightchild")) bias += 4f;
            if (HasTraitId(a, "shiny")) bias += 2f;

            if (HasTraitId(a, "unlucky")) bias -= 12f;
            if (HasTraitId(a, "stupid")) bias -= 10f;
            if (HasTraitId(a, "deceitful")) bias -= 7f;
            if (HasTraitId(a, "evil")) bias -= 7f;
            if (HasTraitId(a, "greedy")) bias -= 5f;
            if (HasTraitId(a, "lustful")) bias -= 5f;
            if (HasTraitId(a, "thief")) bias -= 4f;
            if (HasTraitId(a, "short_sighted")) bias -= 3f;

            return Mathf.Clamp(bias, -30f, 30f);
        }

        private static string PickWeightedRootIdWithTraits(Actor a)
        {
            float bias = GetQualityBias(a);
            float shiftFactor = 1f + bias * 0.01f;

            var weights = new float[ROOT_IDS.Length];
            for (int i = 0; i < ROOT_IDS.Length; i++)
            {
                float weight = ROOT_WEIGHTS[i] * Mathf.Pow(shiftFactor, i);
                weights[i] = Mathf.Max(0.01f, weight);
            }

            if (HasTraitId(a, "chosen_one"))
            {
                weights[5] *= 5f;
            }

            float total = 0f;
            for (int i = 0; i < weights.Length; i++) total += weights[i];
            float r = UnityEngine.Random.Range(0f, total);
            for (int i = 0; i < weights.Length; i++)
            {
                if (r < weights[i]) return ROOT_IDS[i];
                r -= weights[i];
            }
            return ROOT_IDS[0];
        }

        private static float GetTraitInheritBoost(Actor a)
        {
            float boost = 0f;
            if (HasTraitId(a, "chosen_one")) boost += 0.10f;
            if (HasTraitId(a, "scar_of_divinity")) boost += 0.06f;
            if (HasTraitId(a, "miracle_born")) boost += 0.05f;
            if (HasTraitId(a, "miracle_bearer")) boost += 0.03f;
            return boost;
        }

        private static float GetTraitBeastBoost(Actor a)
        {
            float boost = 0f;
            if (HasTraitId(a, "bloodlust")) boost += 0.06f;
            if (HasTraitId(a, "flesh_eater")) boost += 0.05f;
            if (HasTraitId(a, "psychopath")) boost += 0.05f;
            if (HasTraitId(a, "savage")) boost += 0.04f;
            return boost;
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
            xn.access.ActorAccess.GetData(a).set(KEY_COEFF, 0f);
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
