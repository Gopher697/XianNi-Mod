using System;
using HarmonyLib;
using UnityEngine;
namespace cultivation.ai
{
    internal static class MainCharacterSystem
    {
        private const string KEY_OWN_LIGHTNING_IMMUNE = "xn.main_char.own_lightning_immune";
        private const string KEY_OWN_LIGHTNING_TIME = "xn.main_char.own_lightning_time";
        private const float IMMUNE_DURATION = 2f;
        public static void Init(Harmony h)
        {
            h.Patch(AccessTools.Method(typeof(Actor), "die",
                new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(MainCharacterSystem), nameof(Prefix_Actor_die)) { priority = Priority.High });
            h.Patch(AccessTools.Method(typeof(Actor), "tryToAttack",
                new Type[] { typeof(BaseSimObject), typeof(bool), typeof(Action), typeof(Vector3), typeof(Kingdom), typeof(WorldTile), typeof(float) }),
                postfix: new HarmonyMethod(typeof(MainCharacterSystem), nameof(Postfix_Actor_tryToAttack)));
            h.Patch(AccessTools.Method(typeof(Actor), "getHit",
                new Type[] { typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(MainCharacterSystem), nameof(Prefix_Actor_getHit)));
            h.Patch(AccessTools.Method(typeof(Actor), "addStatusEffect",
                new Type[] { typeof(StatusAsset), typeof(float), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(MainCharacterSystem), nameof(Prefix_Actor_addStatusEffect)));
        }
        private static bool Prefix_Actor_die(Actor __instance, bool pDestroy, AttackType pType, bool pCountDeath, bool pLogFavorite)
        {
            if (__instance == null) return true;
            if (pDestroy) return true;
            int isMainChar;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return true; 
            int removed;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_REMOVED, out removed, 0);
            if (removed == 1) return true; 
            if (xn.tournament.TournamentManager.IsRunning && xn.tournament.TournamentManager.IsParticipant(__instance))
            {
                return true; 
            }
            int lives;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_LIVES, out lives, 3);
            if (lives > 0)
            {
                lives--;
                __instance.data.set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_LIVES, lives);
                RecordNearDeathEvent(__instance, pType);
                int maxHealth = __instance.getMaxHealth();
                __instance.changeHealth(maxHealth);
                TeleportToRandomLocation(__instance);
                string name = __instance.getName() ?? "未知";
                string message = $"{name}的主角光环触发保命，剩余保命次数：{lives}";
                xn.world.BroadcastSystem.PostActor(__instance, message);
                return false;
            }
            return true;
        }
        private static void RecordNearDeathEvent(Actor actor, AttackType deathType)
        {
            if (actor == null) return;
            if (!actor.attackedBy.isRekt() && actor.attackedBy.isActor())
            {
                WorldLog.logFavMurder(actor, actor.attackedBy.a);
            }
            else
            {
                WorldLog.logFavDead(actor);
            }
        }
        private static void TeleportToRandomLocation(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;
            WorldTile randomTile = World.world.islands_calculator.getRandomIslandGround()?.regions.GetRandom()?.tiles.GetRandom();
            if (randomTile == null || randomTile.Type.block || !randomTile.Type.ground)
            {
                var allTiles = World.world.tiles_list;
                if (allTiles != null && allTiles.Length > 0)
                {
                    int attempts = 0;
                    while (attempts < 100 && (randomTile == null || randomTile.Type.block || !randomTile.Type.ground))
                    {
                        randomTile = allTiles.GetRandom();
                        attempts++;
                    }
                }
            }
            if (randomTile != null && !randomTile.Type.block && randomTile.Type.ground)
            {
                ActionLibrary.teleportEffect(actor, randomTile);
                actor.cancelAllBeh();
                actor.spawnOn(randomTile);
            }
        }
        private static void Postfix_Actor_tryToAttack(Actor __instance, BaseSimObject pTarget, bool pDoChecks, Action pKillAction, Vector3 pAttackPosition, Kingdom pForceKingdom, WorldTile pTileTarget, float pBonusAreOfEffect, bool __result)
        {
            if (__instance == null || !__instance.isAlive()) return;
            if (!__result) return; 
            int isMainChar;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return; 
            WorldTile targetTile = null;
            if (pTarget != null && !pTarget.isRekt())
            {
                if (pTarget.isActor())
                {
                    targetTile = pTarget.a.current_tile;
                }
                else if (pTarget.isBuilding())
                {
                    targetTile = pTarget.b.current_tile;
                }
            }
            if (targetTile == null)
            {
                targetTile = __instance.current_tile;
            }
            if (targetTile != null)
            {
                __instance.data.set(KEY_OWN_LIGHTNING_IMMUNE, 1);
                __instance.data.set(KEY_OWN_LIGHTNING_TIME, Time.time);
                MapBox.spawnLightningSmall(targetTile, 0.25f, __instance);
            }
        }
        private static bool Prefix_Actor_getHit(Actor __instance, float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker, bool pMetallicWeapon, bool pSkipIfShake, bool pCheckDamageReduction)
        {
            if (__instance == null) return true;
            int isMainChar;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return true; 
            if (pAttackType == AttackType.Fire)
            {
                return false; 
            }
            int ownLightningImmune;
            __instance.data.get(KEY_OWN_LIGHTNING_IMMUNE, out ownLightningImmune, 0);
            if (ownLightningImmune != 1) return true; 
            float lightningTime;
            __instance.data.get(KEY_OWN_LIGHTNING_TIME, out lightningTime, 0f);
            float timeSinceLightning = Time.time - lightningTime;
            if (timeSinceLightning > IMMUNE_DURATION)
            {
                __instance.data.set(KEY_OWN_LIGHTNING_IMMUNE, 0);
                __instance.data.set(KEY_OWN_LIGHTNING_TIME, 0f);
                return true;
            }
            bool isLightningDamage = (pAttackType == AttackType.Other);
            if (isLightningDamage && pAttacker != null && pAttacker.isActor() && pAttacker.a == __instance)
            {
                __instance.data.set(KEY_OWN_LIGHTNING_IMMUNE, 0);
                __instance.data.set(KEY_OWN_LIGHTNING_TIME, 0f);
                return false; 
            }
            __instance.data.set(KEY_OWN_LIGHTNING_IMMUNE, 0);
            __instance.data.set(KEY_OWN_LIGHTNING_TIME, 0f);
            return true;
        }
        private static bool Prefix_Actor_addStatusEffect(Actor __instance, StatusAsset pStatusAsset, float pOverrideTimer, bool pColorEffect)
        {
            if (__instance == null || pStatusAsset == null) return true;
            int isMainChar;
            __instance.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return true; 
            if (pStatusAsset.id == "burning")
            {
                return false; 
            }
            return true; 
        }
    }
}