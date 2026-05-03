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
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
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
            xn.access.ActorAccess.GetData(__instance).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return true; 
            int removed;
            xn.access.ActorAccess.GetData(__instance).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_REMOVED, out removed, 0);
            if (removed == 1) return true; 
            if (xn.tournament.TournamentManager.IsRunning && xn.tournament.TournamentManager.IsParticipant(__instance))
            {
                return true; 
            }
            int lives;
            xn.access.ActorAccess.GetData(__instance).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_LIVES, out lives, 3);
            if (lives > 0)
            {
                lives--;
                xn.access.ActorAccess.GetData(__instance).set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_LIVES, lives);
                RecordNearDeathEvent(__instance, pType);
                int maxHealth = __instance.getMaxHealth();
                __instance.changeHealth(maxHealth);
                TeleportToRandomLocation(__instance);
                string name = __instance.getName() ?? T("common_unknown", "Unknown");
                string message = T("broadcast_main_character_life_saved", "{0}'s protagonist halo saved them. Remaining saves: {1}", name, lives);
                xn.world.BroadcastSystem.PostActor(__instance, message);
                return false;
            }
            return true;
        }
        private static void RecordNearDeathEvent(Actor actor, AttackType deathType)
        {
            if (actor == null) return;
            if (!xn.access.ActorAccess.GetAttackedBy(actor).isRekt() && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetAttackedBy(actor)))
            {
                WorldLog.logFavMurder(actor, xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(actor)));
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
                var allTiles = xn.access.MapBoxAccess.GetTilesList(World.world);
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
                xn.access.ActorAccess.SpawnOn(actor, randomTile);
            }
        }
        private static void Postfix_Actor_tryToAttack(Actor __instance, BaseSimObject pTarget, bool pDoChecks, Action pKillAction, Vector3 pAttackPosition, Kingdom pForceKingdom, WorldTile pTileTarget, float pBonusAreOfEffect, bool __result)
        {
            if (__instance == null || !__instance.isAlive()) return;
            if (!__result) return; 
            int isMainChar;
            xn.access.ActorAccess.GetData(__instance).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return; 
            WorldTile targetTile = null;
            if (pTarget != null && !pTarget.isRekt())
            {
                if (xn.access.BaseSimObjectAccess.IsActor(pTarget))
                {
                    targetTile = xn.access.BaseSimObjectAccess.GetActor(pTarget).current_tile;
                }
                else if (xn.access.BaseSimObjectAccess.IsBuilding(pTarget))
                {
                    Building targetBuilding = xn.access.BaseSimObjectAccess.GetBuilding(pTarget);
                    if (targetBuilding != null) targetTile = targetBuilding.current_tile;
                }
            }
            if (targetTile == null)
            {
                targetTile = __instance.current_tile;
            }
            if (targetTile != null)
            {
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_IMMUNE, 1);
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_TIME, Time.time);
                MapBox.spawnLightningSmall(targetTile, 0.25f, __instance);
            }
        }
        private static bool Prefix_Actor_getHit(Actor __instance, float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker, bool pMetallicWeapon, bool pSkipIfShake, bool pCheckDamageReduction)
        {
            if (__instance == null) return true;
            int isMainChar;
            xn.access.ActorAccess.GetData(__instance).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return true; 
            if (pAttackType == AttackType.Fire)
            {
                return false; 
            }
            int ownLightningImmune;
            xn.access.ActorAccess.GetData(__instance).get(KEY_OWN_LIGHTNING_IMMUNE, out ownLightningImmune, 0);
            if (ownLightningImmune != 1) return true; 
            float lightningTime;
            xn.access.ActorAccess.GetData(__instance).get(KEY_OWN_LIGHTNING_TIME, out lightningTime, 0f);
            float timeSinceLightning = Time.time - lightningTime;
            if (timeSinceLightning > IMMUNE_DURATION)
            {
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_IMMUNE, 0);
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_TIME, 0f);
                return true;
            }
            bool isLightningDamage = (pAttackType == AttackType.Other);
            if (isLightningDamage && pAttacker != null && xn.access.BaseSimObjectAccess.IsActor(pAttacker) && xn.access.BaseSimObjectAccess.GetActor(pAttacker) == __instance)
            {
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_IMMUNE, 0);
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_TIME, 0f);
                return false; 
            }
            xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_IMMUNE, 0);
            xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_LIGHTNING_TIME, 0f);
            return true;
        }
        private static bool Prefix_Actor_addStatusEffect(Actor __instance, StatusAsset pStatusAsset, float pOverrideTimer, bool pColorEffect)
        {
            if (__instance == null || pStatusAsset == null) return true;
            int isMainChar;
            xn.access.ActorAccess.GetData(__instance).get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar != 1) return true; 
            if (pStatusAsset.id == "burning")
            {
                return false; 
            }
            return true; 
        }
    }
}
