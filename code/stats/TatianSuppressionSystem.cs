using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using cultivation;
namespace cultivation
{
    internal static class TatianSuppressionSystem
    {
        private const string KEY_SUPPRESSED = "xn.tatian.suppressed";
        private const string KEY_SUPPRESSOR_ID = "xn.tatian.suppressor_id";
        private static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
            "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
            "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
            "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian"
        };
        private static readonly string[] ANC_STAR_IDS = {
            "ancient_01_star", "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star",
            "ancient_06_star", "ancient_07_star", "ancient_08_star", "ancient_09_star", "ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = {
            "beast_01_stage", "beast_02_stage", "beast_03_stage", "beast_04_stage", "beast_05_stage",
            "beast_06_stage", "beast_07_stage", "beast_08_stage", "beast_09_stage", "beast_10_stage"
        };
        private static int ConvertAncientBeastToRealmIndex(int starOrStage)
        {
            switch (starOrStage)
            {
                case 1: return 2;   
                case 2: return 4;   
                case 3: return 6;   
                case 4: return 7;   
                case 5: return 8;   
                case 6: return 9;   
                case 7: return 10;  
                case 8: return 11;  
                case 9: return 13;  
                case 10: return 14; 
                default: return -1;
            }
        }
        private static int GetRealmIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
            {
                if (a.hasTrait(REALM_IDS[i]))
                {
                    idx = i;
                }
            }
            return idx;
        }
        private static int GetAncIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            for (int i = 0; i < ANC_STAR_IDS.Length; i++)
            {
                if (a.hasTrait(ANC_STAR_IDS[i]))
                {
                    idx = i;
                }
            }
            return idx;
        }
        private static int GetBeastIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            for (int i = 0; i < BEAST_STAGE_IDS.Length; i++)
            {
                if (a.hasTrait(BEAST_STAGE_IDS[i]))
                {
                    idx = i;
                }
            }
            return idx;
        }
        private static int GetUnifiedRealmIndex(Actor a)
        {
            if (a == null) return -1;
            int realmIdx = GetRealmIndex(a);
            if (realmIdx >= 0) return realmIdx;
            int ancIdx = GetAncIndex(a);
            if (ancIdx >= 0)
            {
                int star = ancIdx + 1; 
                return ConvertAncientBeastToRealmIndex(star);
            }
            int beastIdx = GetBeastIndex(a);
            if (beastIdx >= 0)
            {
                int stage = beastIdx + 1; 
                return ConvertAncientBeastToRealmIndex(stage);
            }
            return -1;
        }
        private static bool AreEnemies(Actor a, Actor b)
        {
            if (a == null || b == null) return false;
            if (a == b) return false;
            return a.areFoes(b);
        }
        private static void ApplySuppression(Actor target, Actor suppressor)
        {
            if (target == null || !target.isAlive()) return;
            if (suppressor == null || xn.access.ActorAccess.GetData(suppressor) == null) return;
            xn.access.ActorAccess.GetData(target).set(KEY_SUPPRESSED, 1);
            xn.access.ActorAccess.GetData(target).set(KEY_SUPPRESSOR_ID, (int)xn.access.ActorAccess.GetData(suppressor).id);
            if (target.is_moving)
            {
                target.stopMovement();
            }
            if (xn.access.ActorAccess.HasAttackTarget(target))
            {
                target.clearAttackTarget();
            }
            target.cancelAllBeh();
        }
        private static void RemoveSuppression(Actor target)
        {
            if (target == null) return;
            xn.access.ActorAccess.GetData(target).set(KEY_SUPPRESSED, 0);
            xn.access.ActorAccess.GetData(target).set(KEY_SUPPRESSOR_ID, 0);
        }
        private static float GetSuppressionRange(Actor suppressor)
        {
            if (suppressor == null) return 0f;
            if (suppressor.hasTrait("realm_16_tatian")) return 50f; 
            if (suppressor.hasTrait("realm_15_half_tatian")) return 10f; 
            if (suppressor.hasTrait("realm_14_gtianzun")) return 5f; 
            if (suppressor.hasTrait("ancient_10_star")) return 10f; 
            if (suppressor.hasTrait("ancient_09_star")) return 5f; 
            if (suppressor.hasTrait("beast_10_stage")) return 10f; 
            if (suppressor.hasTrait("beast_09_stage")) return 5f; 
            return 0f;
        }
        private static bool IsInSuppressionRange(Actor suppressor, Actor target, float maxRange)
        {
            if (suppressor == null || target == null) return false;
            if (suppressor.current_tile == null || target.current_tile == null) return false;
            float distSq = Toolbox.SquaredDistTile(suppressor.current_tile, target.current_tile);
            float maxRangeSq = maxRange * maxRange;
            return distSq <= maxRangeSq;
        }
        private static void CheckAndUpdateSuppression(Actor suppressor, float range)
        {
            if (suppressor == null || !suppressor.isAlive()) return;
            if (suppressor.current_tile == null) return;
            int suppressorRealm = GetUnifiedRealmIndex(suppressor);
            if (suppressorRealm < 0) return; 
            List<Actor> suppressedTargets = null;
            int chunkRadius = Mathf.CeilToInt(range / 10f) + 1; 
            float tileRadius = range;
            foreach (var target in Finder.getUnitsFromChunk(suppressor.current_tile, chunkRadius, tileRadius))
            {
                if (target == null || !target.isAlive()) continue;
                if (target == suppressor) continue; 
                if (!AreEnemies(suppressor, target)) continue;
                int targetRealm = GetUnifiedRealmIndex(target);
                if (targetRealm >= 0 && targetRealm >= suppressorRealm) continue;
                if (IsInSuppressionRange(suppressor, target, range))
                {
                    ApplySuppression(target, suppressor);
                    if (suppressedTargets == null) suppressedTargets = new List<Actor>();
                    suppressedTargets.Add(target);
                }
                else
                {
                    int suppressed; xn.access.ActorAccess.GetData(target).get(KEY_SUPPRESSED, out suppressed, 0);
                    if (suppressed == 1)
                    {
                        int suppressorId; xn.access.ActorAccess.GetData(target).get(KEY_SUPPRESSOR_ID, out suppressorId, 0);
                        if (suppressorId == (int)xn.access.ActorAccess.GetData(suppressor).id)
                        {
                            RemoveSuppression(target);
                        }
                    }
                }
            }
            if (suppressedTargets != null && suppressedTargets.Count >= 5)
            {
                BlackHoleSkill.TryTrigger(suppressor, suppressedTargets);
            }
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class UpdateSuppressionPatch
        {
            private static float s_lastUpdateTime = 0f;
            private static readonly float UPDATE_INTERVAL = 0.5f; 
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (!xn.config.ModConfigHooks.EnableTatianSuppression) return;
                if (__instance == null || __instance.units == null) return;
                BlackHoleSkill.Update();
                float now = Time.time;
                if (now - s_lastUpdateTime < UPDATE_INTERVAL) return;
                s_lastUpdateTime = now;
                var list = __instance.units.getSimpleList();
                if (list == null || list.Count == 0) return;
                for (int i = 0; i < list.Count; i++)
                {
                    var actor = list[i];
                    if (actor == null || !actor.isAlive()) continue;
                    float range = GetSuppressionRange(actor);
                    if (range > 0f)
                    {
                        CheckAndUpdateSuppression(actor, range);
                    }
                }
                for (int i = 0; i < list.Count; i++)
                {
                    var actor = list[i];
                    if (actor == null) continue;
                    var actorData = xn.access.ActorAccess.GetData(actor);
                    if (actorData == null) continue;
                    int suppressed; actorData.get(KEY_SUPPRESSED, out suppressed, 0);
                    if (suppressed == 1)
                    {
                        if (!actor.isAlive())
                        {
                            RemoveSuppression(actor);
                            continue;
                        }
                        int suppressorId; actorData.get(KEY_SUPPRESSOR_ID, out suppressorId, 0);
                        if (suppressorId != 0)
                        {
                            Actor suppressor = null;
                            foreach (var candidate in list)
                            {
                                var candidateData = xn.access.ActorAccess.GetData(candidate);
                                if (candidate != null && candidate.isAlive() && candidateData != null && (int)candidateData.id == suppressorId)
                                {
                                    suppressor = candidate;
                                    break;
                                }
                            }
                            if (suppressor == null || !suppressor.isAlive())
                            {
                                RemoveSuppression(actor);
                            }
                            else
                            {
                                float range = GetSuppressionRange(suppressor);
                                if (range > 0f && !IsInSuppressionRange(suppressor, actor, range))
                                {
                                    RemoveSuppression(actor);
                                }
                            }
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "b3_findEnemyTarget")]
        private static class PreventEnemyTargetPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, float pElapsed)
            {
                if (BlackHoleSkill.IsCasting(__instance))
                {
                    return false;
                }
                int suppressed; xn.access.ActorAccess.GetData(__instance).get(KEY_SUPPRESSED, out suppressed, 0);
                if (suppressed == 1)
                {
                    return false; 
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Actor), "b5_checkPathMovement")]
        private static class PreventMovementPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, float pElapsed)
            {
                if (BlackHoleSkill.IsCasting(__instance))
                {
                    if (__instance.is_moving)
                    {
                        __instance.stopMovement();
                    }
                    return false;
                }
                int suppressed; xn.access.ActorAccess.GetData(__instance).get(KEY_SUPPRESSED, out suppressed, 0);
                if (suppressed == 1)
                {
                    if (__instance.is_moving)
                    {
                        __instance.stopMovement();
                    }
                    return false; 
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Actor), "attackTargetActions")]
        private static class BlockAttackActionsPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance)
            {
                xn.access.ActorAccess.GetData(__instance).get(KEY_SUPPRESSED, out int suppressed, 0);
                if (suppressed == 1) return false; 
                return true;
            }
        }
    }
}
