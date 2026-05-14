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
        private const string DecisionAncientBreakthrough = "xn_decision_ancient_breakthrough";
        private const string DecisionBeastBreakthrough = "xn_decision_beast_breakthrough";
        private const string DecisionPathChoice = "xn_decision_path_choice";

        private const string IconAncientBreakthrough = "stats/gushen";
        private const string IconBeastBreakthrough = "stats/yaoli";

        private const string TaskBreakthroughEntry = "task_xn_breakthrough_entry";
        private const string TaskCondenseRootEntry = "task_xn_condense_root_entry";
        private const string TaskIntentComprehendEntry = "task_xn_intent_comprehend_entry";
        private const string TaskDemonicHuntEntry = "task_xn_demonic_hunt_entry";
        private const string TaskAncientBreakthroughEntry = "task_xn_ancient_breakthrough_entry";
        private const string TaskBeastBreakthroughEntry = "task_xn_beast_breakthrough_entry";
        private const string TaskPathChoiceEntry = "task_xn_path_choice_entry";

        private const string JobBreakthrough = "job_xn_breakthrough";
        private const string JobCondenseRoot = "job_xn_condense_root";
        private const string JobIntentComprehend = "job_xn_intent_comprehend";

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
        private const string KeyDemonicHuntActive = "xn.demonic_hunt.active";
        private const string KeyKillCount = "xn.kill_count";
        private const string KeyKillPrev = "xn.kill.prev";

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

        private static readonly string[] AncientStarIds = new[]
        {
            "ancient_01_star",
            "ancient_02_star",
            "ancient_03_star",
            "ancient_04_star",
            "ancient_05_star",
            "ancient_06_star",
            "ancient_07_star",
            "ancient_08_star",
            "ancient_09_star",
            "ancient_10_star"
        };

        private static readonly string[] BeastStageIds = new[]
        {
            "beast_01_stage",
            "beast_02_stage",
            "beast_03_stage",
            "beast_04_stage",
            "beast_05_stage",
            "beast_06_stage",
            "beast_07_stage",
            "beast_08_stage",
            "beast_09_stage",
            "beast_10_stage"
        };

        private static readonly string[] InheritanceTraitIds = new[]
        {
            "inherit_01_poor",
            "inherit_02_normal",
            "inherit_03_supreme",
            "inherit_04_tusi",
            "inherit_05_ancientblood"
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
            DecisionAncientBreakthrough,
            DecisionBeastBreakthrough,
            DecisionPathChoice
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
        private static bool _warnedTaskBehaviourReflection;

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
                Patch(harmony, typeof(Actor), "updateStats", nameof(PostActorUpdateStats));
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

        private static void PostActorUpdateStats(Actor __instance)
        {
            Patch_Actor_UpdateStats_InjectSapientDecisions.Postfix(__instance);
        }

        private static class Patch_Actor_UpdateStats_InjectSapientDecisions
        {
            internal static void Postfix(Actor __instance)
            {
                if (__instance == null || !__instance.isAlive()) return;
                if (__instance.asset == null) return;

                bool isSapient = false;
                for (int i = 0; i < SapientActorAssetIds.Length; i++)
                {
                    if (__instance.asset.id == SapientActorAssetIds[i])
                    {
                        isSapient = true;
                        break;
                    }
                }
                if (!isSapient) return;

                InjectDecisionIfMissing(__instance, DecisionCondenseRoot);
                InjectDecisionIfMissing(__instance, DecisionPathChoice);
            }
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
            RegisterBreakthroughTask(TaskBreakthroughEntry, DecisionBreakthrough, "stats/xiuwei");
            RegisterDirectTask(TaskCondenseRootEntry, DecisionCondenseRoot, "ui/icon/lingqiadd", JobCondenseRoot);
            RegisterDirectTask(TaskIntentComprehendEntry, DecisionIntentComprehend, "trair/intent_01_extreme", JobIntentComprehend);
            RegisterDirectTask(TaskDemonicHuntEntry, DecisionDemonicHunt, "trair/path_01_demonic", "job_xn_demonic_hunt");
            RegisterTrialTask(TaskAncientBreakthroughEntry, DecisionAncientBreakthrough, IconAncientBreakthrough, 3);
            RegisterTrialTask(TaskBeastBreakthroughEntry, DecisionBeastBreakthrough, IconBeastBreakthrough, 4);
            RegisterPathChoiceTask(TaskPathChoiceEntry, DecisionPathChoice, "stats/xiuwei");
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

        private static void RegisterDirectTask(string taskId, string localeKey, string iconPath, string jobId)
        {
            BehaviourTaskActorLibrary taskLibrary = AssetManager.tasks_actor;
            if (taskLibrary == null)
            {
                return;
            }

            BehaviourTaskActor task = taskLibrary.get(taskId);
            if (task == null)
            {
                task = new BehaviourTaskActor
                {
                    id = taskId,
                    locale_key = localeKey,
                    path_icon = iconPath,
                    show_icon = true
                };

                taskLibrary.add(task);
            }
            else
            {
                // Stage 3 migration: hot-loaded Stage 1 task objects are modified in place.
                // BehaviourTaskActor exposes addBeh() but not a stable public remove API here,
                // so clear the existing behaviour IList by reflection before adding the direct entry.
                task.locale_key = localeKey;
                task.path_icon = iconPath;
                task.show_icon = true;
                ClearTaskBehaviours(task);
            }

            task.addBeh(new BehSetDirectJob(localeKey, jobId));
        }

        private static void RegisterTrialTask(string taskId, string localeKey, string iconPath, int trialType)
        {
            BehaviourTaskActorLibrary taskLibrary = AssetManager.tasks_actor;
            if (taskLibrary == null)
            {
                return;
            }

            BehaviourTaskActor task = taskLibrary.get(taskId);
            if (task == null)
            {
                task = new BehaviourTaskActor
                {
                    id = taskId,
                    locale_key = localeKey,
                    path_icon = iconPath,
                    show_icon = true
                };
                taskLibrary.add(task);
            }
            else
            {
                task.locale_key = localeKey;
                task.path_icon = iconPath;
                task.show_icon = true;
                ClearTaskBehaviours(task);
            }

            task.addBeh(new BehStartTrialDirectly(localeKey, trialType));
        }

        private static void RegisterBreakthroughTask(string taskId, string localeKey, string iconPath)
        {
            BehaviourTaskActorLibrary taskLibrary = AssetManager.tasks_actor;
            if (taskLibrary == null)
            {
                return;
            }

            BehaviourTaskActor task = taskLibrary.get(taskId);
            if (task == null)
            {
                task = new BehaviourTaskActor
                {
                    id = taskId,
                    locale_key = localeKey,
                    path_icon = iconPath,
                    show_icon = true
                };
                taskLibrary.add(task);
            }
            else
            {
                task.locale_key = localeKey;
                task.path_icon = iconPath;
                task.show_icon = true;
                ClearTaskBehaviours(task);
            }

            task.addBeh(new BehStartBreakthrough(localeKey));
        }

        private static void RegisterPathChoiceTask(string taskId, string localeKey, string iconPath)
        {
            BehaviourTaskActorLibrary taskLibrary = AssetManager.tasks_actor;
            if (taskLibrary == null) return;

            BehaviourTaskActor task = taskLibrary.get(taskId);
            if (task == null)
            {
                task = new BehaviourTaskActor
                {
                    id = taskId,
                    locale_key = localeKey,
                    path_icon = iconPath,
                    show_icon = true
                };
                taskLibrary.add(task);
            }
            else
            {
                task.locale_key = localeKey;
                task.path_icon = iconPath;
                task.show_icon = true;
                ClearTaskBehaviours(task);
            }

            task.addBeh(new BehChoosePath(localeKey));
        }

        private static void ClearTaskBehaviours(BehaviourTaskActor task)
        {
            if (task == null)
            {
                return;
            }

            bool cleared = false;
            Type type = task.GetType();
            while (type != null)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    var list = field.GetValue(task) as System.Collections.IList;
                    if (list == null || !LooksLikeBehaviourList(field, list))
                    {
                        continue;
                    }

                    list.Clear();
                    cleared = true;
                }

                type = type.BaseType;
            }

            if (!cleared)
            {
                WarnOnce(ref _warnedTaskBehaviourReflection, "[XN] Could not clear existing intent entry task behaviours; direct native behaviour was appended.");
            }
        }

        private static bool LooksLikeBehaviourList(FieldInfo field, System.Collections.IList list)
        {
            if (field == null)
            {
                return false;
            }

            string name = field.Name != null ? field.Name.ToLowerInvariant() : "";
            if (name.Contains("beh"))
            {
                return true;
            }

            Type fieldType = field.FieldType;
            if (fieldType != null && fieldType.IsGenericType)
            {
                Type[] args = fieldType.GetGenericArguments();
                if (args.Length == 1 && typeof(BehaviourActionActor).IsAssignableFrom(args[0]))
                {
                    return true;
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is BehaviourActionActor)
                {
                    return true;
                }
            }

            return false;
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
                id = DecisionAncientBreakthrough,
                task_id = TaskAncientBreakthroughEntry,
                path_icon = IconAncientBreakthrough,
                priority = NeuroLayer.Layer_3_High,
                cooldown = CooldownOneYear,
                unique = true,
                action_check_launch = CanLaunchAncientBreakthrough,
                weight_calculate_custom = WeightAncientBreakthrough
            });

            RegisterDecision(new DecisionAsset
            {
                id = DecisionBeastBreakthrough,
                task_id = TaskBeastBreakthroughEntry,
                path_icon = IconBeastBreakthrough,
                priority = NeuroLayer.Layer_3_High,
                cooldown = CooldownOneYear,
                unique = true,
                action_check_launch = CanLaunchBeastBreakthrough,
                weight_calculate_custom = WeightBeastBreakthrough
            });

            RegisterDecision(new DecisionAsset
            {
                id = DecisionPathChoice,
                task_id = TaskPathChoiceEntry,
                path_icon = "stats/xiuwei",
                priority = NeuroLayer.Layer_3_High,
                cooldown = CooldownOneYear,
                unique = true,
                action_check_launch = CanLaunchPathChoice,
                weight_calculate_custom = WeightPathChoice
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
            AttachDecisionToTrait("path_01_demonic", DecisionBreakthrough);
            AttachDecisionToTrait("path_02_immortal", DecisionBreakthrough);

            for (int i = 0; i < IntentRealmAttachIds.Length; i++)
            {
                AttachDecisionToTrait(IntentRealmAttachIds[i], DecisionIntentComprehend);
            }

            AttachDecisionToTrait("path_01_demonic", DecisionDemonicHunt);
            AttachDecisionToTrait("path_04_ancient", DecisionAncientBreakthrough);
            AttachDecisionToTrait("path_03_beast", DecisionBeastBreakthrough);

            for (int i = 0; i < SapientActorAssetIds.Length; i++)
            {
                AttachDecisionToActorAsset(SapientActorAssetIds[i], DecisionCondenseRoot);
                AttachDecisionToActorAsset(SapientActorAssetIds[i], DecisionPathChoice);
            }
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

        private static void InjectDecisionIfMissing(Actor actor, string decisionId)
        {
            if (actor == null || actor.decisions == null) return;
            DecisionAsset decision = AssetManager.decisions_library != null
                ? AssetManager.decisions_library.get(decisionId)
                : null;
            if (decision == null) return;

            int count = Mathf.Min(actor.decisions_counter, actor.decisions.Length);
            for (int i = 0; i < count; i++)
            {
                if (actor.decisions[i] != null && actor.decisions[i].id == decisionId)
                {
                    return;
                }
            }

            if (actor.decisions_counter >= actor.decisions.Length)
            {
                Debug.LogWarning("[XN] InjectDecisionIfMissing: decisions array full for actor=" +
                    GetActorDataName(actor) + " decision=" + decisionId);
                return;
            }

            actor.decisions[actor.decisions_counter++] = decision;
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

            // Stage 3g: path choice must resolve before first breakthrough fires
            if (GetCurrentRealmIndex(actor) < 0 &&
                !HasTrait(actor, "path_01_demonic") &&
                !HasTrait(actor, "path_02_immortal"))
            {
                return false;
            }

            int currentRealm = GetCurrentRealmIndex(actor);
            if (GetInt(actor, KeyHalfTatianLocked, 0) == 1 && currentRealm >= 14)
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

            Debug.Log("[XN S3] " + DecisionBreakthrough + " launch check PASS actor=" +
                GetActorDataName(actor) + " realm=" + GetCurrentRealmIndex(actor));
            return true;
        }

        private static bool CanLaunchPathChoice(Actor actor)
        {
            if (!IsAlive(actor) || !HasCityAndKingdom(actor) || xn.access.ActorAccess.IsInsideBoat(actor))
                return false;
            if (GetInt(actor, KeyStop, 0) != 1)
                return false;
            if (GetCurrentRealmIndex(actor) >= 0)
                return false;
            if (HasTrait(actor, "path_01_demonic") || HasTrait(actor, "path_02_immortal"))
                return false;
            long xp = GetLong(actor, KeyXp, 0L);
            if (xp < RealmThresholds[0])
                return false;

            Debug.Log("[XN S3] " + DecisionPathChoice + " launch check PASS actor=" + GetActorDataName(actor));
            return true;
        }

        private static bool CanLaunchAncientBreakthrough(Actor actor)
        {
            if (!IsAlive(actor) || !HasCityAndKingdom(actor) || xn.access.ActorAccess.IsInsideBoat(actor))
            {
                return false;
            }
            if (IsInActiveTournament(actor))
            {
                return false;
            }
            if (!HasTrait(actor, "path_04_ancient"))
            {
                return false;
            }
            if (GetInt(actor, KeyAncientStop, 0) != 1)
            {
                return false;
            }
            if (GetInt(actor, KeyTrialActive, 0) == 1)
            {
                return false;
            }
            if (Date.getCurrentYear() < GetInt(actor, KeyTrialCooldownUntil, 0))
            {
                return false;
            }
            if (GetCurrentAncientStarIndex(actor) >= GetMaxAncientBeastAllowedByKingdom(actor))
            {
                return false;
            }

            Debug.Log("[XN S2] " + DecisionAncientBreakthrough + " launch check PASS actor=" +
                GetActorDataName(actor) + " star=" + GetCurrentAncientStarIndex(actor));
            return true;
        }

        private static bool CanLaunchBeastBreakthrough(Actor actor)
        {
            if (!IsAlive(actor) || !HasCityAndKingdom(actor) || xn.access.ActorAccess.IsInsideBoat(actor))
            {
                return false;
            }
            if (IsInActiveTournament(actor))
            {
                return false;
            }
            if (!HasTrait(actor, "path_03_beast"))
            {
                return false;
            }
            if (GetInt(actor, KeyBeastStop, 0) != 1)
            {
                return false;
            }
            if (GetInt(actor, KeyTrialActive, 0) == 1)
            {
                return false;
            }
            if (Date.getCurrentYear() < GetInt(actor, KeyTrialCooldownUntil, 0))
            {
                return false;
            }
            if (GetCurrentBeastStageIndex(actor) >= GetMaxAncientBeastAllowedByKingdom(actor))
            {
                return false;
            }

            Debug.Log("[XN S2] " + DecisionBeastBreakthrough + " launch check PASS actor=" +
                GetActorDataName(actor) + " stage=" + GetCurrentBeastStageIndex(actor));
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
            if (GetInt(actor, KeyCondenseReady, 0) != 1)
            {
                return false;
            }

            Debug.Log("[XN S3] " + DecisionCondenseRoot + " launch check PASS actor=" +
                GetActorDataName(actor));
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

            Debug.Log("[XN S3] " + DecisionDemonicHunt + " launch check PASS actor=" +
                GetActorDataName(actor));
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

        private static float WeightPathChoice(Actor actor)
        {
            return 3f;
        }

        private static float WeightAncientBreakthrough(Actor actor)
        {
            int currentStar = GetCurrentAncientStarIndex(actor);
            int maxAllowed = GetMaxAncientBeastAllowedByKingdom(actor);
            if (currentStar >= maxAllowed)
            {
                return 0.1f;
            }

            float weight = 2f + Mathf.Clamp(GetCityAura(actor) / 2000f, 0f, 5f);
            weight *= GetInheritanceWeight(actor);
            return Mathf.Max(0.1f, weight);
        }

        private static float WeightBeastBreakthrough(Actor actor)
        {
            int currentStage = GetCurrentBeastStageIndex(actor);
            int maxAllowed = GetMaxAncientBeastAllowedByKingdom(actor);
            if (currentStage >= maxAllowed)
            {
                return 0.1f;
            }

            float weight = 1.8f + Mathf.Clamp(GetCityAura(actor) / 2000f, 0f, 5f);
            int kills = GetInt(actor, KeyKillCount, 0);
            int previousKills = GetInt(actor, KeyKillPrev, 0);
            int wuxin = GetInt(actor, KeyWuxin, 0);
            if (kills - previousKills > 0 && wuxin >= 50)
            {
                weight += 0.5f;
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

        private static int GetCurrentAncientStarIndex(Actor actor)
        {
            if (actor == null)
            {
                return -1;
            }

            int current = -1;
            for (int i = 0; i < AncientStarIds.Length; i++)
            {
                if (HasTrait(actor, AncientStarIds[i]) && i > current)
                {
                    current = i;
                }
            }

            return current;
        }

        private static int GetCurrentBeastStageIndex(Actor actor)
        {
            if (actor == null)
            {
                return -1;
            }

            int current = -1;
            for (int i = 0; i < BeastStageIds.Length; i++)
            {
                if (HasTrait(actor, BeastStageIds[i]) && i > current)
                {
                    current = i;
                }
            }

            return current;
        }

        private static int GetMaxAncientBeastAllowedByKingdom(Actor actor)
        {
            if (actor == null || actor.kingdom == null || actor.kingdom.isRekt())
            {
                return 0;
            }

            return GetMaxAncientBeastAllowedByKingdomLevel(xn.world.XiuzhenguoSystem.GetLevel(actor.kingdom));
        }

        private static int GetMaxAncientBeastAllowedByKingdomLevel(int kingdomLevel)
        {
            if (kingdomLevel <= 1)
            {
                return 0;
            }
            if (kingdomLevel <= 3)
            {
                return 1;
            }
            if (kingdomLevel <= 5)
            {
                return 2;
            }
            if (kingdomLevel == 6)
            {
                return 3;
            }
            if (kingdomLevel == 7)
            {
                return 5;
            }
            if (kingdomLevel == 8)
            {
                return 6;
            }
            if (kingdomLevel == 9)
            {
                return 8;
            }

            return 9;
        }

        private static float GetInheritanceWeight(Actor actor)
        {
            float result = 0.8f;
            for (int i = 0; i < InheritanceTraitIds.Length; i++)
            {
                if (!HasTrait(actor, InheritanceTraitIds[i]))
                {
                    continue;
                }

                switch (i)
                {
                    case 0:
                        result = 0.9f;
                        break;
                    case 1:
                        result = 1.0f;
                        break;
                    case 2:
                        result = 1.2f;
                        break;
                    case 3:
                        result = 1.5f;
                        break;
                    case 4:
                        result = 2.0f;
                        break;
                }
            }

            return result;
        }

        private static bool IsInActiveTournament(Actor actor)
        {
            return actor != null &&
                xn.tournament.TournamentManager.IsRunning &&
                xn.tournament.TournamentManager.IsParticipant(actor);
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

        private sealed class BehSetDirectJob : BehaviourActionActor
        {
            private readonly string _decisionId;
            private readonly string _jobId;

            public BehSetDirectJob(string decisionId, string jobId)
            {
                _decisionId = decisionId;
                _jobId = jobId;
            }

            public override BehResult execute(Actor actor)
            {
                if (actor == null || !actor.isAlive())
                {
                    return BehResult.Stop;
                }

                Debug.Log("[XN S3] Native decision authoritative: " +
                    _decisionId + " actor=" + GetActorDataName(actor));

                if (_jobId == JobIntentComprehend)
                {
                    IntentComprehendJob.BeginComprehension(actor);
                }

                actor.endJob();
                var ai = xn.access.ActorAccess.GetAI(actor);
                if (ai != null)
                {
                    ai.setJob(_jobId);
                }

                return BehResult.Stop;
            }
        }

        private sealed class BehStartTrialDirectly : BehaviourActionActor
        {
            private readonly string _decisionId;
            private readonly int _trialType;

            public BehStartTrialDirectly(string decisionId, int trialType)
            {
                _decisionId = decisionId;
                _trialType = trialType;
            }

            public override BehResult execute(Actor actor)
            {
                if (actor == null || !actor.isAlive())
                {
                    return BehResult.Stop;
                }

                Debug.Log("[XN S3] Native trial decision fired: " + _decisionId +
                    " type=" + _trialType + " actor=" + GetActorDataName(actor));

                if (_trialType == 3)
                {
                    BreakthroughJob.BeginAncientTrial(actor);
                }
                else if (_trialType == 4)
                {
                    BreakthroughJob.BeginBeastTrial(actor);
                }

                return BehResult.Stop;
            }
        }

        private sealed class BehStartBreakthrough : BehaviourActionActor
        {
            private readonly string _decisionId;

            public BehStartBreakthrough(string decisionId)
            {
                _decisionId = decisionId;
            }

            public override BehResult execute(Actor actor)
            {
                if (actor == null || !actor.isAlive())
                {
                    return BehResult.Stop;
                }

                int curRealm = GetCurrentRealmIndex(actor);
                int curYear = Date.getCurrentYear();

                // Stamp tried-year before acting so the fallback patch and
                // CanLaunchBreakthrough both see this attempt recorded.
                xn.access.ActorAccess.GetData(actor).set(KeyBreakTriedYear, curYear);

                if (IsHeavenGateRealm(curRealm))
                {
                    // Heaven-gate: stun in place and resolve via Patch_TickHeavenTrial.
                    // Do not set a job - mirrors the ancient/beast trial behavior.
                    Debug.Log("[XN S3] Native breakthrough decision fired (heaven gate): " +
                        _decisionId + " actor=" + GetActorDataName(actor) + " realm=" + curRealm);
                    BreakthroughJob.BeginHeavenTrial(actor, curRealm);
                    return BehResult.Stop;
                }

                // Normal realm: send actor to the breakthrough job.
                Debug.Log("[XN S3] Native breakthrough decision fired (realm): " +
                    _decisionId + " actor=" + GetActorDataName(actor) + " realm=" + curRealm);
                actor.endJob();
                var ai = xn.access.ActorAccess.GetAI(actor);
                if (ai != null)
                {
                    ai.setJob(JobBreakthrough);
                }

                return BehResult.Stop;
            }
        }

        private sealed class BehChoosePath : BehaviourActionActor
        {
            private readonly string _decisionId;

            public BehChoosePath(string decisionId)
            {
                _decisionId = decisionId;
            }

            public override BehResult execute(Actor actor)
            {
                if (actor == null || !actor.isAlive()) return BehResult.Stop;

                // Idempotent: if path already assigned (e.g. via IncreaseXinmoAndMaybeCorrupt
                // firing before this decision resolved), do nothing.
                if (HasTrait(actor, "path_01_demonic") || HasTrait(actor, "path_02_immortal"))
                    return BehResult.Stop;

                int xinmo = GetInt(actor, KeyXinmo, 0);
                int wuxin = GetInt(actor, KeyWuxin, 0);

                float demonicChance = 15f;

                // Continuous stats
                demonicChance += Mathf.Min(xinmo * 0.22f, 45f);
                demonicChance -= Mathf.Clamp(Mathf.Max(0f, (wuxin - 50) * 0.2f), 0f, 15f);

                // Demonic trait deltas
                if (HasTrait(actor, "psychopath")) demonicChance += 20f;
                if (HasTrait(actor, "evil")) demonicChance += 15f;
                if (HasTrait(actor, "bloodlust")) demonicChance += 15f;
                if (HasTrait(actor, "madness")) demonicChance += 10f;
                if (HasTrait(actor, "savage")) demonicChance += 10f;
                if (HasTrait(actor, "death_mark")) demonicChance += 8f;
                if (HasTrait(actor, "flesh_eater")) demonicChance += 8f;
                if (HasTrait(actor, "greedy")) demonicChance += 5f;
                if (HasTrait(actor, "deceitful")) demonicChance += 5f;
                if (HasTrait(actor, "pyromaniac")) demonicChance += 5f;
                if (HasTrait(actor, "hotheaded")) demonicChance += 3f;
                if (HasTrait(actor, "nightchild")) demonicChance += 3f;

                // Immortal trait deltas
                if (HasTrait(actor, "chosen_one")) demonicChance -= 20f;
                if (HasTrait(actor, "blessed")) demonicChance -= 12f;
                if (HasTrait(actor, "wise")) demonicChance -= 10f;
                if (HasTrait(actor, "strong_minded")) demonicChance -= 8f;
                if (HasTrait(actor, "peaceful")) demonicChance -= 7f;
                if (HasTrait(actor, "pacifist")) demonicChance -= 7f;
                if (HasTrait(actor, "content")) demonicChance -= 5f;
                if (HasTrait(actor, "honest")) demonicChance -= 5f;
                if (HasTrait(actor, "moonchild")) demonicChance -= 5f;
                if (HasTrait(actor, "sunblessed")) demonicChance -= 5f;

                // Clamp - preserve free will
                demonicChance = Mathf.Clamp(demonicChance, 5f, 80f);

                bool goesDemonic = UnityEngine.Random.value * 100f < demonicChance;
                string pathId = goesDemonic ? "path_01_demonic" : "path_02_immortal";
                var pathTrait = AssetManager.traits.get(pathId) as ActorTrait;
                if (pathTrait != null) actor.addTrait(pathTrait);

                // Immortal path: clear xinmo contamination
                if (!goesDemonic)
                    xn.access.ActorAccess.GetData(actor).set(KeyXinmo, 0);

                Debug.Log("[XN S3] " + _decisionId + " resolved: actor=" + GetActorDataName(actor) +
                    " demonicChance=" + demonicChance.ToString("F1") + "% result=" + pathId);

                return BehResult.Stop;
            }
        }
    }
}
