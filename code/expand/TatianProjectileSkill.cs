using System;
using HarmonyLib;
using UnityEngine;
namespace xn.expand
{
    public static class TatianProjectileSkill
    {
        private const string REALM_TATIAN_ID = "realm_16_tatian";
        private const int PROJECTILE_COUNT = 8; 
        private const float PROJECTILE_RANGE = 10f; 
        private static readonly string[] DANGEROUS_PROJECTILES = new[]
        {
            "madness_ball"  
        };
        private const string KEY_OWN_EFFECT_IMMUNE = "xn.realm_skill.own_effect_immune";
        private const string KEY_OWN_EFFECT_TIME = "xn.realm_skill.own_effect_time";
        private const float IMMUNE_DURATION = 5f; 
        public static void Init(Harmony h)
        {
            h.Patch(AccessTools.Method(typeof(Actor), "tryToAttack",
                new Type[] { typeof(BaseSimObject), typeof(bool), typeof(Action), typeof(Vector3), typeof(Kingdom), typeof(WorldTile), typeof(float) }),
                postfix: new HarmonyMethod(typeof(TatianProjectileSkill), nameof(Postfix_Actor_tryToAttack)));
        }
        private static void Postfix_Actor_tryToAttack(Actor __instance, BaseSimObject pTarget, bool pDoChecks, Action pKillAction, Vector3 pAttackPosition, Kingdom pForceKingdom, WorldTile pTileTarget, float pBonusAreOfEffect, bool __result)
        {
            if (__instance == null || !__instance.isAlive()) return;
            if (!__result) return; 
            if (!HasTatianRealm(__instance)) return;
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
            __instance.data.set(KEY_OWN_EFFECT_IMMUNE, 1);
            __instance.data.set(KEY_OWN_EFFECT_TIME, Time.time);
            TriggerTatianProjectiles(__instance, pTarget, targetTile);
        }
        private static bool HasTatianRealm(Actor actor)
        {
            if (actor == null) return false;
            return actor.hasTrait(REALM_TATIAN_ID);
        }
        private static void TriggerTatianProjectiles(Actor actor, BaseSimObject target, WorldTile targetTile)
        {
            if (actor == null || actor.current_tile == null) return;
            var enemyData = EnemiesFinder.findEnemiesFrom(actor.current_tile, actor.kingdom);
            bool hasEnemies = enemyData != null &&
                             enemyData.list != null &&
                             enemyData.list.Count > 0;
            if (hasEnemies)
            {
                SpawnProjectilesToEnemies(actor, enemyData.list);
            }
            else
            {
                SpawnProjectilesInArea(actor, null);
            }
        }
        private static void SpawnProjectilesToEnemies(Actor actor, System.Collections.Generic.List<BaseSimObject> enemies)
        {
            if (actor == null || enemies == null) return;
            Vector3 launchPosition = CalculateLaunchPosition(actor);
            int validEnemyCount = 0;
            for (int i = 0; i < enemies.Count && validEnemyCount < PROJECTILE_COUNT; i++)
            {
                BaseSimObject enemy = enemies[i];
                if (enemy == null || enemy.isRekt() || !enemy.isAlive()) continue;
                if (enemy.isActor() && enemy.a == actor) continue; 
                string projectileType = GetRandomProjectileType();
                Vector3 targetPosition = GetEnemyPosition(enemy);
                float targetZ = 0f;
                if (enemy.isActor())
                {
                    targetZ = enemy.a.getHeight();
                }
                SpawnProjectile(actor, enemy, projectileType, launchPosition, targetPosition, targetZ);
                validEnemyCount++;
            }
            if (validEnemyCount < PROJECTILE_COUNT)
            {
                FillRemainingProjectiles(actor, launchPosition, validEnemyCount);
            }
        }
        private static void FillRemainingProjectiles(Actor actor, Vector3 launchPosition, int alreadyFired)
        {
            if (actor == null || alreadyFired >= PROJECTILE_COUNT) return;
            Vector2 actorPos = actor.current_position;
            int remainingCount = PROJECTILE_COUNT - alreadyFired;
            float angleStep = 360f / remainingCount;
            float startAngle = Randy.randomFloat(0f, 360f); 
            for (int i = 0; i < remainingCount; i++)
            {
                float angle = startAngle + angleStep * i;
                float radian = angle * Mathf.Deg2Rad;
                float targetX = actorPos.x + Mathf.Cos(radian) * PROJECTILE_RANGE;
                float targetY = actorPos.y + Mathf.Sin(radian) * PROJECTILE_RANGE;
                Vector3 targetPosition = new Vector3(targetX, targetY, 0f);
                string projectileType = GetRandomProjectileType();
                SpawnProjectile(actor, null, projectileType, launchPosition, targetPosition, 0f);
            }
        }
        private static void SpawnProjectilesInArea(Actor actor, System.Collections.Generic.List<BaseSimObject> enemies)
        {
            if (actor == null) return;
            Vector2 actorPos = actor.current_position;
            Vector3 launchPosition = CalculateLaunchPosition(actor);
            float angleStep = 360f / PROJECTILE_COUNT;
            for (int i = 0; i < PROJECTILE_COUNT; i++)
            {
                float angle = angleStep * i;
                float radian = angle * Mathf.Deg2Rad;
                float targetX = actorPos.x + Mathf.Cos(radian) * PROJECTILE_RANGE;
                float targetY = actorPos.y + Mathf.Sin(radian) * PROJECTILE_RANGE;
                Vector3 targetPosition = new Vector3(targetX, targetY, 0f);
                string projectileType = GetRandomProjectileType();
                SpawnProjectile(actor, null, projectileType, launchPosition, targetPosition, 0f);
            }
        }
        private static string GetRandomProjectileType()
        {
            if (AssetManager.projectiles == null || AssetManager.projectiles.list == null || AssetManager.projectiles.list.Count == 0)
            {
                return "arrow"; 
            }
            var safeProjectiles = new System.Collections.Generic.List<ProjectileAsset>();
            foreach (var projectile in AssetManager.projectiles.list)
            {
                if (projectile != null && !IsDangerousProjectile(projectile.id))
                {
                    safeProjectiles.Add(projectile);
                }
            }
            if (safeProjectiles.Count == 0)
            {
                return "arrow";
            }
            int randomIndex = Randy.randomInt(0, safeProjectiles.Count);
            return safeProjectiles[randomIndex].id;
        }
        private static bool IsDangerousProjectile(string projectileId)
        {
            if (string.IsNullOrEmpty(projectileId)) return true;
            foreach (var dangerousId in DANGEROUS_PROJECTILES)
            {
                if (projectileId == dangerousId)
                {
                    return true;
                }
            }
            return false;
        }
        private static Vector3 GetEnemyPosition(BaseSimObject enemy)
        {
            if (enemy.isActor())
            {
                Vector2 pos2D = enemy.a.current_position;
                return new Vector3(pos2D.x, pos2D.y, 0f);
            }
            else if (enemy.isBuilding())
            {
                Vector2Int pos = enemy.b.current_tile.pos;
                return new Vector3(pos.x, pos.y, 0f);
            }
            else
            {
                Vector2 pos2D = enemy.current_position;
                return new Vector3(pos2D.x, pos2D.y, 0f);
            }
        }
        private static Vector3 CalculateLaunchPosition(Actor actor)
        {
            Vector2 pos2D = actor.current_position;
            Vector3 position = new Vector3(pos2D.x, pos2D.y, 0f);
            float size = actor.stats["size"];
            position.y += size + 0.5f; 
            return position;
        }
        private static void SpawnProjectile(Actor actor, BaseSimObject target, string projectileType, Vector3 launchPosition, Vector3 targetPosition, float targetZ)
        {
            if (actor == null || World.world == null || World.world.projectiles == null) return;
            float startZ = actor.getHeight() + 0.5f;
            World.world.projectiles.spawn(
                actor,              
                target,             
                projectileType,     
                launchPosition,     
                targetPosition,     
                targetZ,            
                startZ,             
                null,               
                actor.kingdom       
            );
        }
    }
}