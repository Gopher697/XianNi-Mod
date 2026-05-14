using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ai;
using ai.behaviours;
using UnityEngine;
using xn.Traits;
using xn.world;

namespace cultivation.ai
{
    /*
     * Stage 1 native decision bridge:
     * - Init hook: CultivationDecisions.Init(h) installs postfixes on DecisionsLibrary.init(),
     *   DecisionsLibrary.linkAssets(), and MapBox.Update(); it also runs immediate registration
     *   because NeoModLoader may load Xianni after vanilla decision linking.
     * - decision_index handling: vanilla linkAssets() assigns indexes when possible. The link
     *   postfix/immediate path recalculates decision_index, priority_int_cached, and
     *   has_weight_custom from AssetManager.decisions_library.list for all decisions when Xianni
     *   decisions are late-added.
     * - actor refresh: the first MapBox.Update after registration resizes existing actor decision
     *   cooldown/disabled/decision arrays without dropping existing values, then calls
     *   actor.setStatsDirty() so trait/status decision caches are rebuilt.
     */
    internal static class CultivationDecisions
    {
        private const int CooldownOneYear = 60;

        private const string DecisionBreakthrough = "xn_decision_breakthrough";
        private const string DecisionCondenseRoot = "xn_decision_condense_root";
        private const string DecisionIntentComprehend = "xn_decision_intent_comprehend";
        private const string DecisionDemonicHunt = "xn_decision_demonic_hunt";
        private const string DecisionTianyunzi = "xn_decision_tianyunzi";

        private const string TaskBreakthroughEntry = "task_xn_breakthrough_entry";
        private const string TaskCondenseRootEntry = "task_xn_condense_root_entry";
        private const string TaskIntentComprehendEntry = "task_xn_intent_comprehend_entry";
        private const string TaskDemonicHuntEntry = "task_xn_demonic_hunt_entry";
        private const string TaskTianyunziEntry = "task_xn_tianyunzi_entry";

        private const string JobBreakthrough = "job_xn_breakthrough";
        private const string JobCondenseRoot = "job_xn_condense_root";
        private const string JobIntentComprehend = "job_xn_intent_comprehend";
        private const string JobDemonicHunt = "job_xn_demonic_hunt";
        private const string JobTianyunzi = "job_xn_tianyunzi";

        private const string KeyStop = "xn.cultivation.stop";
        private const string KeyXp = "xn.stat.xiuwei";
        private const string KeyXinmo = "xn.stat.xinmo";
        private const string KeyWuxin = "xn.stat.wuxin";
        private const string KeyTrialActive = "xn.trial.active";
        private const string KeyTrialCooldownUntil = "xn.trial.cooldown_until";
        private const string KeyHalfTatianLocked = "xn.half_tatian.locked";
        private const string KeyDaoBaseDamagedUntil = "xn.daobase.damaged_until";
        private const string KeyBreakTriedYear = "xn.break.tried_year";
        private const string KeyBreakSuccessYear = "xn.break.success_year";
        private const string KeyAncientStop = "xn.ancient.stop";
        private const string KeyBeastStop = "xn.beast.stop";
        private const string KeyCondenseReady = "xn.root.condense_ready";
        private const string KeyCondenseYear = "xn.root.condense_year";
        private const string KeyNextRootTryYear = "xn.root.next_try_year";
        private const string KeyCityAura = "xn.city.aura";
        private const string KeyCityRootYear = "xn.city.root.try_year";
        private const string KeyCityRootUsed = "xn.city.root.try_used";
        private const string KeyCityRootQuota = "xn.city.root.try_quota";
        private const string KeyIntentActive = "xn.intent.lv_active";
        private const string KeyIntentCooldownUntil = "xn.intent.lv_cd_until_year";
        private const string KeyDemonicHuntTarget = "xn.demonic_hunt.target_id";
        private const string KeyDemonicHuntActive = "xn.demonic_hunt.active";
        private const string KeyTianyunziFlag = "xn_is_tianyunzi";

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

        private static readonly string[] IntentRealmAttachIds = new[]
        {
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

        private static readonly string[] SapientActorAssetIds = new[]
        {
            "human",
            "elf",
            "dwarf",
            "orc",
            "nvpu",
            "dashou"
        };

        private static readonly string[] XianniDecisionIds = new[]
        {
            DecisionBreakthrough,
            DecisionCondenseRoot,
            DecisionIntentComprehend,
            DecisionDemonicHunt,
            DecisionTianyunzi
        };

        private static readonly FieldInfo DecisionCooldownsField = AccessTools.Field(typeof(Actor), "_decision_cooldowns");
        private static readonly FieldInfo DecisionDisabledField = AccessTools.Field(typeof(Actor), "_decision_disabled");
        private static readonly FieldInfo ActorAssetDecisionCacheField = AccessTools.Field(typeof(ActorAsset), "_cached_assets_decisions");
        private static readonly FieldInfo ActorAssetDecisionCacheCounterField = AccessTools.Field(typeof(ActorAsset), "_cached_assets_decisions_counter");

        private static bool _inited;
        private static bool _registered;
        private static bool _actorRefreshDone;
        private static bool _warnedActorArrayReflection;
        private static bool _warnedActorAssetCacheReflection;

        public static void Init(Harmony harmony)
        {
            if (_inited)
            {
                RegisterAll();
                return;
            }

            _inited = true;
            if (harmony != null)
            {
                Patch(harmony, typeof(DecisionsLibrary), "init", nameof(PostDecisionsLibraryInit));
                Patch(harmony, typeof(DecisionsLibrary), "linkAssets", nameof(PostDecisionsLibraryLinkAssets));
                Patch(harmony, typeof(MapBox), "Update", nameof(PostMapBoxUpdate));
            }

            RegisterAll();
        }

        private static void Patch(Harmony harmony, Type type, string methodName, string postfixName)
        {
            MethodInfo original = AccessTools.Method(type, methodName);
            MethodInfo postfix = AccessTools.Method(typeof(CultivationDecisions), postfixName);
            if (original == null || postfix == null)
            {
                Debug.LogWarning("[XN] CultivationDecisions could not patch " + type.Name + "." + methodName);
                return;
            }

            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }

        private static void PostDecisionsLibraryInit()
        {
            RegisterAll();
        }

        private static void PostDecisionsLibraryLinkAssets()
        {
            EnsureDecisionIndexes();
            AttachDecisions();
        }

        private static void PostMapBoxUpdate(MapBox __instance)
        {
            if (_actorRefreshDone)
            {
                return;
            }

            RegisterAll();
            RefreshExistingActorDecisionArrays(__instance);
            _actorRefreshDone = true;
        }

        private static void RegisterAll()
        {
            EnsureLegacyJobs();
            RegisterEntryTasks();
            RegisterDecisionAssets();
            EnsureDecisionIndexes();
            AttachDecisions();
            _registered = true;
        }

        private static void EnsureLegacyJobs()
        {
            BreakthroughJob.Init();
            CondenseRootJob.Init();
            IntentComprehendJob.Init();
            DemonicHuntJob.Init();
            TianyunziJob.Init();
        }

        private static void RegisterEntryTasks()
        {
            RegisterEntryTask(TaskBreakthroughEntry, DecisionBreakthrough, "stats/xiuwei", JobBreakthrough);
            RegisterEntryTask(TaskCondenseRootEntry, DecisionCondenseRoot, "ui/icon/lingqiadd", JobCondenseRoot);
            RegisterEntryTask(TaskIntentComprehendEntry, DecisionIntentComprehend, "trair/intent_01_extreme", JobIntentComprehend);
            RegisterEntryTask(TaskDemonicHuntEntry, DecisionDemonicHunt, "trair/path_01_demonic", JobDemonicHunt);
            RegisterEntryTask(TaskTianyunziEntry, DecisionTianyunzi, "acots/skin/tianyunzi", JobTianyunzi);
        }

        private static void RegisterEntryTask(string taskId, string localeKey, string iconPath, string expectedJobId)
        {
            BehaviourTaskActorLibrary taskLibrary = AssetManager.tasks_actor;
            if (taskLibrary == null || taskLibrary.get(taskId) != null)
            {
                return;
            }

            var task = new BehaviourTaskActor
            {
                id = taskId,
                locale_key = localeKey,
                path_icon = iconPath,
                show_icon = true
            };

            taskLibrary.add(task);
            task.addBeh(new BehEnterLegacyJob(localeKey, expectedJobId));
        }

        private static void RegisterDecisionAssets()
        {
            RegisterDecision(new DecisionAsset
            {
                id = DecisionBreakthrough,
                task_id = TaskBreakthroughEntry,
                path_icon = "stats/xiuwei",
                priority = NeuroLayer.Layer_3_High,
                cooldown = CooldownOneYear,
                unique = true,
                action_check_launch = CanLaunchBreakthrough,
                weight_calculate_custom = WeightBreakthrough
            });

            RegisterDecision(new DecisionAsset
            {
                id = DecisionCondenseRoot,
                task_id = TaskCondenseRootEntry,
                path_icon = "ui/icon/lingqiadd",
                priority = NeuroLayer.Layer_2_Moderate,
                cooldown = CooldownOneYear,
                unique = true,
                only_sapient = true,
                action_check_launch = CanLaunchCondenseRoot,
                weight_calculate_custom = WeightCondenseRoot
            });

            RegisterDecision(new DecisionAsset
            {
                id = DecisionIntentComprehend,
                task_id = TaskIntentComprehendEntry,
                path_icon = "trair/intent_01_extreme",
                priority = NeuroLayer.Layer_1_Low,
                cooldown = CooldownOneYear,
                unique = true,
                action_check_launch = CanLaunchIntentComprehend,
                weight_calculate_custom = WeightIntentComprehend
            });

            RegisterDecision(new DecisionAsset
            {
                id = DecisionDemonicHunt,
                task_id = TaskDemonicHuntEntry,
                path_icon = "trair/path_01_demonic",
                priority = NeuroLayer.Layer_2_Moderate,
                cooldown = CooldownOneYear,
                unique = true,
                action_check_launch = CanLaunchDemonicHunt,
                weight_calculate_custom = WeightDemonicHunt
            });

            RegisterDecision(new DecisionAsset
            {
                id = DecisionTianyunzi,
                task_id = TaskTianyunziEntry,
                path_icon = "acots/skin/tianyunzi",
                priority = NeuroLayer.Layer_2_Moderate,
                cooldown = CooldownOneYear,
                unique = true,
                action_check_launch = CanLaunchTianyunzi,
                weight_calculate_custom = WeightTianyunzi
            });
        }

        private static void RegisterDecision(DecisionAsset decision)
        {
            DecisionsLibrary library = AssetManager.decisions_library;
            if (library == null || decision == null || library.get(decision.id) != null)
            {
                return;
            }

            library.add(decision);
        }

        private static void EnsureDecisionIndexes()
        {
            DecisionsLibrary library = AssetManager.decisions_library;
            if (library == null || library.list == null)
            {
                return;
            }

            for (int i = 0; i < library.list.Count; i++)
            {
                DecisionAsset decision = library.list[i];
                if (decision == null)
                {
                    continue;
                }

                decision.decision_index = i;
                decision.priority_int_cached = (int)decision.priority;
                decision.has_weight_custom = decision.weight_calculate_custom != null;
            }

            ValidateXianniDecisionIndexes();
        }

        private static void ValidateXianniDecisionIndexes()
        {
            DecisionsLibrary library = AssetManager.decisions_library;
            if (library == null)
            {
                return;
            }

            var seen = new HashSet<int>();
            for (int i = 0; i < XianniDecisionIds.Length; i++)
            {
                string id = XianniDecisionIds[i];
                DecisionAsset decision = library.get(id);
                if (decision == null)
                {
                    Debug.LogWarning("[XN] Cultivation decision missing after registration: " + id);
                    continue;
                }

                if (decision.decision_index < 0 || !seen.Add(decision.decision_index))
                {
                    Debug.LogWarning("[XN] Cultivation decision has invalid index: " + id + " index=" + decision.decision_index);
                }
            }
        }

        private static void AttachDecisions()
        {
            for (int i = 0; i < RealmIds.Length; i++)
            {
                AttachDecisionToTrait(RealmIds[i], DecisionBreakthrough);
            }

            for (int i = 0; i < IntentRealmAttachIds.Length; i++)
            {
                AttachDecisionToTrait(IntentRealmAttachIds[i], DecisionIntentComprehend);
            }

            AttachDecisionToTrait("path_01_demonic", DecisionDemonicHunt);

            for (int i = 0; i < SapientActorAssetIds.Length; i++)
            {
                AttachDecisionToActorAsset(SapientActorAssetIds[i], DecisionCondenseRoot);
            }

            AttachDecisionToActorAsset("dragon", DecisionTianyunzi);
        }

        private static void AttachDecisionToTrait(string traitId, string decisionId)
        {
            ActorTrait trait = AssetManager.traits != null ? AssetManager.traits.get(traitId) as ActorTrait : null;
            DecisionAsset decision = AssetManager.decisions_library != null ? AssetManager.decisions_library.get(decisionId) : null;
            if (trait == null || decision == null)
            {
                return;
            }

            if (trait.decision_ids == null)
            {
                trait.decision_ids = new List<string>();
            }

            if (!trait.decision_ids.Contains(decisionId))
            {
                trait.decision_ids.Add(decisionId);
            }

            AddDecisionAsset(trait, decision);
        }

        private static void AddDecisionAsset(BaseAugmentationAsset asset, DecisionAsset decision)
        {
            if (asset == null || decision == null)
            {
                return;
            }

            DecisionAsset[] existing = asset.decisions_assets;
            if (existing != null)
            {
                for (int i = 0; i < existing.Length; i++)
                {
                    if (existing[i] == decision || (existing[i] != null && existing[i].id == decision.id))
                    {
                        return;
                    }
                }
            }

            int oldLength = existing != null ? existing.Length : 0;
            var next = new DecisionAsset[oldLength + 1];
            if (existing != null && oldLength > 0)
            {
                Array.Copy(existing, next, oldLength);
            }

            next[oldLength] = decision;
            asset.decisions_assets = next;
        }

        private static void AttachDecisionToActorAsset(string actorAssetId, string decisionId)
        {
            ActorAsset actorAsset = AssetManager.actor_library != null ? AssetManager.actor_library.get(actorAssetId) : null;
            DecisionAsset decision = AssetManager.decisions_library != null ? AssetManager.decisions_library.get(decisionId) : null;
            if (actorAsset == null || decision == null)
            {
                return;
            }

            actorAsset.addDecision(decisionId);
            ClearActorAssetDecisionCache(actorAsset);
        }

        private static void ClearActorAssetDecisionCache(ActorAsset actorAsset)
        {
            if (actorAsset == null)
            {
                return;
            }

            if (ActorAssetDecisionCacheField == null || ActorAssetDecisionCacheCounterField == null)
            {
                WarnOnce(ref _warnedActorAssetCacheReflection, "[XN] ActorAsset decision cache fields not found; late actor asset decisions may need a game restart.");
                return;
            }

            ActorAssetDecisionCacheField.SetValue(actorAsset, null);
            ActorAssetDecisionCacheCounterField.SetValue(actorAsset, 0);
        }

        private static void RefreshExistingActorDecisionArrays(MapBox map)
        {
            if (!_registered)
            {
                RegisterAll();
            }

            int targetSize = GetDecisionCount();
            if (targetSize <= 0)
            {
                return;
            }

            ActorManager units = null;
            if (map != null)
            {
                units = map.units;
            }
            if (units == null && World.world != null)
            {
                units = World.world.units;
            }
            if (units == null)
            {
                return;
            }

            List<Actor> actors = units.getSimpleList();
            if (actors == null)
            {
                return;
            }

            int refreshed = 0;
            for (int i = 0; i < actors.Count; i++)
            {
                Actor actor = actors[i];
                if (actor == null)
                {
                    continue;
                }

                if (ResizeActorDecisionArrays(actor, targetSize))
                {
                    refreshed++;
                }
            }

            Debug.Log("[XN] Cultivation decision actor refresh completed. actors=" + actors.Count + " resized=" + refreshed + " decisions=" + targetSize);
        }

        private static bool ResizeActorDecisionArrays(Actor actor, int targetSize)
        {
            if (actor == null)
            {
                return false;
            }

            bool changed = false;
            if (DecisionCooldownsField == null || DecisionDisabledField == null)
            {
                WarnOnce(ref _warnedActorArrayReflection, "[XN] Actor decision array fields not found; existing actors may need a game restart for cultivation neurons.");
                return false;
            }

            var cooldowns = DecisionCooldownsField.GetValue(actor) as double[];
            var cooldownsNext = EnsureArraySize(cooldowns, targetSize);
            if (!ReferenceEquals(cooldowns, cooldownsNext))
            {
                DecisionCooldownsField.SetValue(actor, cooldownsNext);
                changed = true;
            }

            var disabled = DecisionDisabledField.GetValue(actor) as bool[];
            var disabledNext = EnsureArraySize(disabled, targetSize);
            if (!ReferenceEquals(disabled, disabledNext))
            {
                DecisionDisabledField.SetValue(actor, disabledNext);
                changed = true;
            }

            DecisionAsset[] decisions = actor.decisions;
            DecisionAsset[] decisionsNext = EnsureArraySize(decisions, targetSize);
            if (!ReferenceEquals(decisions, decisionsNext))
            {
                actor.decisions = decisionsNext;
                changed = true;
            }

            if (changed)
            {
                actor.setStatsDirty();
            }

            return changed;
        }

        private static T[] EnsureArraySize<T>(T[] current, int targetSize)
        {
            if (targetSize <= 0)
            {
                return current ?? new T[0];
            }

            if (current != null && current.Length >= targetSize)
            {
                return current;
            }

            int newSize = NextPowerOfTwo(targetSize);
            var next = new T[newSize];
            if (current != null && current.Length > 0)
            {
                Array.Copy(current, next, current.Length);
            }

            return next;
        }

        private static int NextPowerOfTwo(int value)
        {
            int result = 1;
            while (result < value)
            {
                result <<= 1;
            }
            return result;
        }

        private static int GetDecisionCount()
        {
            DecisionsLibrary library = AssetManager.decisions_library;
            return library != null && library.list != null ? library.list.Count : 0;
        }

        private static bool CanLaunchBreakthrough(Actor actor)
        {
            if (!IsAlive(actor) || !HasCityAndKingdom(actor) || xn.access.ActorAccess.IsInsideBoat(actor))
            {
                return false;
            }

            int currentYear = Date.getCurrentYear();
            if (GetInt(actor, KeyTrialCooldownUntil, 0) > currentYear)
            {
                return false;
            }
            if (GetInt(actor, KeyTrialActive, 0) == 1)
            {
                return false;
            }
            // Stage 2 parity: BreakthroughJob.Patch_Actor_GetNextJob starts ancient/beast trials here and returns true.
            if (GetInt(actor, KeyAncientStop, 0) == 1 || GetInt(actor, KeyBeastStop, 0) == 1)
            {
                return false;
            }
            if (GetInt(actor, KeyStop, 0) != 1)
            {
                return false;
            }

            int nextIndex = GetNextRealmIndex(actor);
            if (nextIndex < 0 || nextIndex >= RealmThresholds.Length)
            {
                return false;
            }

            long xp = GetLong(actor, KeyXp, 0L);
            long cap = RealmThresholds[nextIndex];
            if (xp < cap)
            {
                return false;
            }

            int currentRealm = GetCurrentRealmIndex(actor);
            if (GetInt(actor, KeyHalfTatianLocked, 0) == 1 && currentRealm >= 14)
            {
                return false;
            }

            // Stage 2 parity: BreakthroughJob.Patch_Actor_GetNextJob starts heaven-gate trials here and does not redirect.
            if (IsHeavenGateRealm(currentRealm))
            {
                return false;
            }

            if (HasDaoBaseDamage(actor, currentYear))
            {
                return false;
            }

            if (currentRealm >= 1)
            {
                int successYear = GetInt(actor, KeyBreakSuccessYear, 0);
                if (successYear > 0)
                {
                    string daoCode = GetDaoBaseCode(actor);
                    int cooldownYears = GetDaoBaseCooldownYears(daoCode);
                    if (currentYear - successYear < cooldownYears)
                    {
                        return false;
                    }
                }
            }

            // Stage 2 parity: BreakthroughJob.Patch_Actor_GetNextJob skips actors that already tried this year.
            if (GetInt(actor, KeyBreakTriedYear, -1) == currentYear)
            {
                return false;
            }

            Debug.Log("[XN S2] " + DecisionBreakthrough + " launch check PASS actor=" +
                GetActorDataName(actor) + " realm=" + GetCurrentRealmIndex(actor));
            return true;
        }

        private static bool CanLaunchCondenseRoot(Actor actor)
        {
            if (!IsAlive(actor) || !HasCityAndKingdom(actor) || xn.access.ActorAccess.IsInsideBoat(actor))
            {
                return false;
            }
            if (xn.expand.FanjieKingdomTrait.ActorHasFanjieTrait(actor))
            {
                return false;
            }
            // Stage 2 parity: CondenseRootJob.Patch_Actor_GetNextJob redirects from ready/year only;
            // aura/root/next-try checks live in the condense setup and task body.
            if (GetInt(actor, KeyCondenseReady, 0) != 1)
            {
                return false;
            }

            int currentYear = Date.getCurrentYear();
            if (GetInt(actor, KeyCondenseYear, -1) == currentYear)
            {
                return false;
            }

            Debug.Log("[XN S2] " + DecisionCondenseRoot + " launch check PASS actor=" +
                GetActorDataName(actor) + " realm=" + GetCurrentRealmIndex(actor));
            return true;
        }

        private static bool CanLaunchIntentComprehend(Actor actor)
        {
            if (!IsAlive(actor) || !HasCityAndKingdom(actor) || xn.access.ActorAccess.IsInsideBoat(actor))
            {
                return false;
            }
            if (HasAnyIntent(actor))
            {
                return false;
            }
            if (GetCurrentRealmIndex(actor) < 4)
            {
                return false;
            }
            if (GetInt(actor, KeyIntentActive, 0) == 1)
            {
                return false;
            }
            if (Date.getCurrentYear() < GetInt(actor, KeyIntentCooldownUntil, 0))
            {
                return false;
            }

            Debug.Log("[XN S2] " + DecisionIntentComprehend + " launch check PASS actor=" +
                GetActorDataName(actor) + " realm=" + GetCurrentRealmIndex(actor));
            return true;
        }

        private static bool CanLaunchDemonicHunt(Actor actor)
        {
            if (!IsAlive(actor) || !HasCityAndKingdom(actor))
            {
                return false;
            }
            if (!HasTrait(actor, "path_01_demonic"))
            {
                return false;
            }
            if (GetInt(actor, KeyDemonicHuntActive, 0) != 1)
            {
                return false;
            }
            if (GetInt(actor, KeyStop, 0) == 1 || GetInt(actor, KeyTrialActive, 0) == 1)
            {
                return false;
            }

            // Stage 2 parity: DemonicHuntJob.Patch_Actor_GetNextJob does not validate target liveness before redirecting.
            Debug.Log("[XN S2] " + DecisionDemonicHunt + " launch check PASS actor=" +
                GetActorDataName(actor) + " realm=" + GetCurrentRealmIndex(actor));
            return true;
        }

        private static bool CanLaunchTianyunzi(Actor actor)
        {
            if (!IsAlive(actor))
            {
                return false;
            }
            if (GetInt(actor, KeyTianyunziFlag, 0) != 1)
            {
                return false;
            }
            if (GetInt(actor, KeyStop, 0) == 1 || GetInt(actor, KeyTrialActive, 0) == 1)
            {
                return false;
            }

            Debug.Log("[XN S2] " + DecisionTianyunzi + " launch check PASS actor=" +
                GetActorDataName(actor) + " realm=" + GetCurrentRealmIndex(actor));
            return true;
        }

        private static float WeightBreakthrough(Actor actor)
        {
            float weight = 2f;
            int aura = GetCityAura(actor);
            weight += Mathf.Clamp(aura / 2000f, 0f, 5f);

            int xinmo = GetInt(actor, KeyXinmo, 0);
            if (xinmo > 100)
            {
                weight *= Mathf.Clamp01(1f - ((xinmo - 100) / 300f));
            }

            int currentRealm = GetCurrentRealmIndex(actor);
            if (IsHeavenGateRealm(currentRealm))
            {
                weight += 3f;
            }

            return Mathf.Max(0.1f, weight);
        }

        private static float WeightCondenseRoot(Actor actor)
        {
            int aura = GetCityAura(actor);
            int wuxin = GetInt(actor, KeyWuxin, 0);
            float auraFactor = Mathf.Clamp((aura - 600) / 2000f, 0f, 5f);
            float wuxinFactor = Mathf.Clamp(wuxin / 100f, 0.1f, 1.5f);
            return Mathf.Max(0.1f, 0.5f + auraFactor + wuxinFactor);
        }

        private static float WeightIntentComprehend(Actor actor)
        {
            int realm = GetCurrentRealmIndex(actor);
            if (realm < 4 || HasAnyIntent(actor))
            {
                return 0.1f;
            }

            int wuxin = GetInt(actor, KeyWuxin, 0);
            return Mathf.Max(0.1f, 1f + Mathf.Clamp(wuxin / 100f, 0f, 1.5f));
        }

        private static float WeightDemonicHunt(Actor actor)
        {
            int xinmo = GetInt(actor, KeyXinmo, 0);
            return Mathf.Max(0.1f, 1f + Mathf.Clamp(xinmo / 100f, 0f, 4f));
        }

        private static float WeightTianyunzi(Actor actor)
        {
            return GetInt(actor, KeyTianyunziFlag, 0) == 1 ? 4f : 0.1f;
        }

        private static bool IsAlive(Actor actor)
        {
            return actor != null && actor.isAlive();
        }

        private static bool HasCityAndKingdom(Actor actor)
        {
            return actor != null && actor.kingdom != null && actor.city != null;
        }

        private static string GetActorDataName(Actor actor)
        {
            ActorData data = xn.access.ActorAccess.GetData(actor);
            return data != null ? data.name : "";
        }

        private static int GetCurrentRealmIndex(Actor actor)
        {
            if (actor == null)
            {
                return -1;
            }

            int current = -1;
            var traits = actor.getTraits();
            if (traits == null)
            {
                return current;
            }

            for (int i = 0; i < RealmIds.Length; i++)
            {
                foreach (ActorTrait trait in traits)
                {
                    if (trait != null && trait.id == RealmIds[i] && i > current)
                    {
                        current = i;
                    }
                }
            }

            return current;
        }

        private static int GetNextRealmIndex(Actor actor)
        {
            int current = GetCurrentRealmIndex(actor);
            int next = current + 1;
            return next >= RealmIds.Length ? -1 : next;
        }

        private static bool IsHeavenGateRealm(int index)
        {
            return index == 6 || index == 9 || index == 12 || index == 13 || index == 14;
        }

        private static bool HasDaoBaseDamage(Actor actor, int currentYear)
        {
            return GetInt(actor, KeyDaoBaseDamagedUntil, 0) > currentYear;
        }

        private static string GetDaoBaseCode(Actor actor)
        {
            var traits = actor != null ? actor.getTraits() : null;
            if (traits == null)
            {
                return "";
            }

            foreach (ActorTrait trait in traits)
            {
                if (trait == null || trait.group_id != RealmTraitGroup.GroupDaoBase)
                {
                    continue;
                }

                if (trait.id == "dao_07_damaged")
                {
                    continue;
                }
                if (trait.id.StartsWith("dao_01")) return "01";
                if (trait.id.StartsWith("dao_02")) return "02";
                if (trait.id.StartsWith("dao_03")) return "03";
                if (trait.id.StartsWith("dao_04")) return "04";
                if (trait.id.StartsWith("dao_05")) return "05";
                if (trait.id.StartsWith("dao_06")) return "06";
            }

            return "";
        }

        private static int GetDaoBaseCooldownYears(string daoCode)
        {
            switch (daoCode)
            {
                case "01": return 30;
                case "02": return 20;
                case "03": return 10;
                case "04": return 5;
                case "05": return 3;
                case "06": return 1;
                default: return 30;
            }
        }

        private static bool HasAnySpiritRoot(Actor actor)
        {
            var traits = actor != null ? actor.getTraits() : null;
            if (traits == null)
            {
                return false;
            }

            foreach (ActorTrait trait in traits)
            {
                if (trait != null && trait.group_id == RealmTraitGroup.GroupSpiritRoot)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAnyAncientInheritance(Actor actor)
        {
            var traits = actor != null ? actor.getTraits() : null;
            if (traits == null)
            {
                return false;
            }

            foreach (ActorTrait trait in traits)
            {
                if (trait != null && trait.id != null && trait.id.StartsWith("inherit_"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAnyIntent(Actor actor)
        {
            var traits = actor != null ? actor.getTraits() : null;
            if (traits == null)
            {
                return false;
            }

            foreach (ActorTrait trait in traits)
            {
                if (trait != null && trait.group_id == RealmTraitGroup.GroupIntent)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasTrait(Actor actor, string traitId)
        {
            if (actor == null || string.IsNullOrEmpty(traitId))
            {
                return false;
            }

            var traits = actor.getTraits();
            if (traits == null)
            {
                return false;
            }

            foreach (ActorTrait trait in traits)
            {
                if (trait != null && trait.id == traitId)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetCityAura(Actor actor)
        {
            City city = actor != null ? actor.city : null;
            if (city == null || city.data == null)
            {
                return 0;
            }

            int aura;
            city.data.get(KeyCityAura, out aura, 0);
            return aura;
        }

        private static bool CityHasRootQuota(Actor actor)
        {
            City city = actor != null ? actor.city : null;
            if (city == null || city.data == null)
            {
                return false;
            }

            int currentYear = Date.getCurrentYear();
            int year;
            city.data.get(KeyCityRootYear, out year, -1);
            if (year != currentYear)
            {
                return true;
            }

            int used;
            city.data.get(KeyCityRootUsed, out used, 0);
            int quota;
            city.data.get(KeyCityRootQuota, out quota, 0);
            if (quota <= 0)
            {
                quota = 1;
            }

            return used < quota;
        }

        private static int GetInt(Actor actor, string key, int fallback)
        {
            ActorData data = xn.access.ActorAccess.GetData(actor);
            if (data == null)
            {
                return fallback;
            }

            int value;
            data.get(key, out value, fallback);
            return value;
        }

        private static long GetLong(Actor actor, string key, long fallback)
        {
            ActorData data = xn.access.ActorAccess.GetData(actor);
            if (data == null)
            {
                return fallback;
            }

            long value;
            data.get(key, out value, fallback);
            return value;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned)
            {
                return;
            }

            warned = true;
            Debug.LogWarning(message);
        }

        private sealed class BehEnterLegacyJob : BehaviourActionActor
        {
            private readonly string _decisionId;
            private readonly string _expectedJobId;

            public BehEnterLegacyJob(string decisionId, string expectedJobId)
            {
                _decisionId = decisionId;
                _expectedJobId = expectedJobId;
            }

            public override BehResult execute(Actor actor)
            {
                if (actor == null || !actor.isAlive())
                {
                    return BehResult.Stop;
                }

                Debug.Log("[XN] Native cultivation decision fired: " + _decisionId);

                string nextJob = actor.getNextJob();
                if (nextJob == _expectedJobId)
                {
                    actor.endJob();
                    var ai = xn.access.ActorAccess.GetAI(actor);
                    if (ai != null)
                    {
                        ai.setJob(_expectedJobId);
                    }
                }

                return BehResult.Stop;
            }
        }
    }
}
