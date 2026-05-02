using System;
using HarmonyLib;
using UnityEngine;
namespace xn.expand
{
    public static class RealmAttackSkillSystem
    {
        private const string KEY_OWN_EFFECT_IMMUNE = "xn.realm_skill.own_effect_immune";
        private const string KEY_OWN_EFFECT_TIME = "xn.realm_skill.own_effect_time";
        private const string KEY_SKILL_COOLDOWN = "xn.realm_skill.cooldown"; 
        private const float IMMUNE_DURATION = 5f; 
        private const string KEY_LINGLI = "xn.stat.lingli"; 
        private static System.Collections.Generic.HashSet<long> s_immuneActors = new System.Collections.Generic.HashSet<long>();
        private static readonly string[] REALM_IDS = new[]
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
        private static readonly string[] ANC_STAR_IDS = new[]
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
        private static readonly string[] BEAST_STAGE_IDS = new[]
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
        public static void Init(Harmony h)
        {
            h.Patch(AccessTools.Method(typeof(Actor), "tryToAttack",
                new Type[] { typeof(BaseSimObject), typeof(bool), typeof(Action), typeof(Vector3), typeof(Kingdom), typeof(WorldTile), typeof(float) }),
                postfix: new HarmonyMethod(typeof(RealmAttackSkillSystem), nameof(Postfix_Actor_tryToAttack)));
            h.Patch(AccessTools.Method(typeof(Projectile), "targetReached"),
                postfix: new HarmonyMethod(typeof(RealmAttackSkillSystem), nameof(Postfix_Projectile_targetReached)));
            h.Patch(AccessTools.Method(typeof(Actor), "getHit",
                new Type[] { typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(RealmAttackSkillSystem), nameof(Prefix_Actor_getHit)));
            h.Patch(AccessTools.Method(typeof(Actor), "addStatusEffect",
                new Type[] { typeof(StatusAsset), typeof(float), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(RealmAttackSkillSystem), nameof(Prefix_Actor_addStatusEffect)));
        }
        private static void Postfix_Actor_tryToAttack(Actor __instance, BaseSimObject pTarget, bool pDoChecks, Action pKillAction, Vector3 pAttackPosition, Kingdom pForceKingdom, WorldTile pTileTarget, float pBonusAreOfEffect, bool __result)
        {
            if (__instance == null || !__instance.isAlive()) return;
            if (!__result) return; 
            if (pTarget == null || pTarget.isRekt()) return;
            WorldTile targetTile = null;
            if (xn.access.BaseSimObjectAccess.IsActor(pTarget))
            {
                targetTile = xn.access.BaseSimObjectAccess.GetActor(pTarget).current_tile;
            }
            else if (xn.access.BaseSimObjectAccess.IsBuilding(pTarget))
            {
                targetTile = pTarget.b.current_tile;
            }
            if (targetTile == null)
            {
                targetTile = __instance.current_tile;
            }
            int realmIndex = GetCurrentRealmIndex(__instance);
            int ancientIndex = GetCurrentAncientIndex(__instance);
            int beastIndex = GetCurrentBeastIndex(__instance);
            if (realmIndex < 0 && ancientIndex < 0 && beastIndex < 0) return;
            xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_EFFECT_IMMUNE, 1);
            xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_EFFECT_TIME, Time.time);
            s_immuneActors.Add(__instance.id);
            if (realmIndex >= 0)
            {
                int maxRolls = GetMaxRollsForRealm(realmIndex);
                for (int roll = 0; roll < maxRolls; roll++)
                {
                    TriggerRealmSkills(__instance, realmIndex, targetTile);
                }
            }
            if (ancientIndex >= 0)
            {
                int mappedIndex = MapAncientBeastIndexToRealmIndex(ancientIndex);
                int maxRolls = GetMaxRollsForRealm(mappedIndex);
                for (int roll = 0; roll < maxRolls; roll++)
                {
                    TriggerRealmSkills(__instance, mappedIndex, targetTile);
                }
            }
            if (beastIndex >= 0)
            {
                int mappedIndex = MapAncientBeastIndexToRealmIndex(beastIndex);
                int maxRolls = GetMaxRollsForRealm(mappedIndex);
                for (int roll = 0; roll < maxRolls; roll++)
                {
                    TriggerRealmSkills(__instance, mappedIndex, targetTile);
                }
            }
        }
        private static void Postfix_Projectile_targetReached(Projectile __instance)
        {
            if (__instance == null) return;
            if (xn.access.ProjectileAccess.GetByWho(__instance) == null || !xn.access.BaseSimObjectAccess.IsActor(xn.access.ProjectileAccess.GetByWho(__instance))) return;
            Actor attacker = xn.access.BaseSimObjectAccess.GetActor(xn.access.ProjectileAccess.GetByWho(__instance));
            if (attacker == null || !attacker.isAlive()) return;
            WorldTile targetTile = xn.access.ProjectileAccess.GetCurrentTilePosition(__instance) ?? attacker.current_tile;
            int realmIndex = GetCurrentRealmIndex(attacker);
            int ancientIndex = GetCurrentAncientIndex(attacker);
            int beastIndex = GetCurrentBeastIndex(attacker);
            if (realmIndex < 0 && ancientIndex < 0 && beastIndex < 0) return;
            xn.access.ActorAccess.GetData(attacker).set(KEY_OWN_EFFECT_IMMUNE, 1);
            xn.access.ActorAccess.GetData(attacker).set(KEY_OWN_EFFECT_TIME, Time.time);
            s_immuneActors.Add(xn.access.ActorAccess.GetData(attacker).id);
            if (realmIndex >= 0)
            {
                int maxRolls = GetMaxRollsForRealm(realmIndex);
                for (int roll = 0; roll < maxRolls; roll++)
                {
                    TriggerRealmSkills(attacker, realmIndex, targetTile);
                }
            }
            if (ancientIndex >= 0)
            {
                int mappedIndex = MapAncientBeastIndexToRealmIndex(ancientIndex);
                int maxRolls = GetMaxRollsForRealm(mappedIndex);
                for (int roll = 0; roll < maxRolls; roll++)
                {
                    TriggerRealmSkills(attacker, mappedIndex, targetTile);
                }
            }
            if (beastIndex >= 0)
            {
                int mappedIndex = MapAncientBeastIndexToRealmIndex(beastIndex);
                int maxRolls = GetMaxRollsForRealm(mappedIndex);
                for (int roll = 0; roll < maxRolls; roll++)
                {
                    TriggerRealmSkills(attacker, mappedIndex, targetTile);
                }
            }
        }
        private static int GetCurrentRealmIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
            {
                foreach (var t in list)
                {
                    if (t != null && t.id == REALM_IDS[i]) { if (i > cur) cur = i; }
                }
            }
            return cur;
        }
        private static int GetCurrentAncientIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < ANC_STAR_IDS.Length; i++)
            {
                foreach (var t in list)
                {
                    if (t != null && t.id == ANC_STAR_IDS[i]) { if (i > cur) cur = i; }
                }
            }
            return cur;
        }
        private static int GetCurrentBeastIndex(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return -1;
            int cur = -1;
            for (int i = 0; i < BEAST_STAGE_IDS.Length; i++)
            {
                foreach (var t in list)
                {
                    if (t != null && t.id == BEAST_STAGE_IDS[i]) { if (i > cur) cur = i; }
                }
            }
            return cur;
        }
        private static int GetMaxRollsForRealm(int realmIndex)
        {
            if (realmIndex == 11 || realmIndex == 12 || realmIndex == 13) return 2;
            if (realmIndex == 14) return 3;
            if (realmIndex == 15) return 5;
            return 1;
        }
        private static int MapAncientBeastIndexToRealmIndex(int ancientBeastIndex)
        {
            int starOrStage = ancientBeastIndex + 1;
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
        private static void TriggerRealmSkills(Actor actor, int realmIndex, WorldTile targetTile)
        {
            if (actor == null || targetTile == null) return;
            if (!CheckAndSetCooldown(actor, realmIndex)) return;
            switch (realmIndex)
            {
                case 2: 
                    if (Randy.randomChance(0.15f)) SpawnLightning(actor, targetTile, realmIndex);
                    break;
                case 3: 
                    if (Randy.randomChance(0.22f)) SpawnLightning(actor, targetTile, realmIndex);
                    break;
                case 4: 
                    if (Randy.randomChance(0.29f)) SpawnLightning(actor, targetTile, realmIndex);
                    break;
                case 5: 
                    if (Randy.randomChance(0.36f)) SpawnLightning(actor, targetTile, realmIndex);
                    break;
                case 6: 
                    if (Randy.randomChance(0.43f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.12f)) SpawnMeteorite(actor, targetTile);
                    break;
                case 7: 
                    if (Randy.randomChance(0.50f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.12f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.22f)) SpawnMeteorite(actor, targetTile);
                    break;
                case 8: 
                    if (Randy.randomChance(0.57f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.20f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.32f)) SpawnMeteorite(actor, targetTile);
                    break;
                case 9: 
                    if (Randy.randomChance(0.64f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.25f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.42f)) SpawnMeteorite(actor, targetTile);
                    break;
                case 10: 
                    if (Randy.randomChance(0.71f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.30f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.50f)) SpawnMeteorite(actor, targetTile);
                    if (Randy.randomChance(0.10f)) SpawnBomb(actor, targetTile);
                    break;
                case 11: 
                    if (Randy.randomChance(0.78f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.35f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.58f)) SpawnMeteorite(actor, targetTile);
                    if (Randy.randomChance(0.10f)) SpawnBomb(actor, targetTile);
                    if (Randy.randomChance(0.01f)) SpawnBoulder(actor, targetTile);
                    break;
                case 12: 
                    if (Randy.randomChance(0.85f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.35f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.65f)) SpawnMeteorite(actor, targetTile);
                    if (Randy.randomChance(0.15f)) SpawnBomb(actor, targetTile);
                    if (Randy.randomChance(0.10f)) SpawnBoulder(actor, targetTile);
                    break;
                case 13: 
                    if (Randy.randomChance(0.90f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.40f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.72f)) SpawnMeteorite(actor, targetTile);
                    if (Randy.randomChance(0.20f)) SpawnBomb(actor, targetTile);
                    if (Randy.randomChance(0.15f)) SpawnBoulder(actor, targetTile);
                    if (Randy.randomChance(0.15f)) SpawnHeatRay(actor, targetTile, 5f);
                    if (Randy.randomChance(0.10f)) SpawnAntimatterBomb(actor, targetTile);
                    break;
                case 14: 
                    if (Randy.randomChance(0.95f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.40f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.80f)) SpawnMeteorite(actor, targetTile);
                    if (Randy.randomChance(0.25f)) SpawnBomb(actor, targetTile);
                    if (Randy.randomChance(0.20f)) SpawnBoulder(actor, targetTile);
                    if (Randy.randomChance(0.25f)) SpawnHeatRay(actor, targetTile, 5f);
                    if (Randy.randomChance(0.20f)) SpawnAntimatterBomb(actor, targetTile);
                    break;
                case 15: 
                    if (Randy.randomChance(1.0f)) SpawnLightning(actor, targetTile, realmIndex);
                    if (Randy.randomChance(0.60f)) SpawnGrenade(actor, targetTile);
                    if (Randy.randomChance(0.95f)) SpawnMeteorite(actor, targetTile);
                    if (Randy.randomChance(0.50f)) SpawnBomb(actor, targetTile);
                    if (Randy.randomChance(0.50f)) SpawnBoulder(actor, targetTile);
                    if (Randy.randomChance(0.30f)) SpawnHeatRay(actor, targetTile, 20f);
                    if (Randy.randomChance(0.50f)) SpawnAntimatterBomb(actor, targetTile);
                    break;
            }
        }
        private static bool CheckAndSetCooldown(Actor actor, int realmIndex)
        {
            if (actor == null) return false;
            if (realmIndex >= 15) return true;
            float baseCooldown = GetCooldownForRealm(realmIndex);
            float lastCooldown;
            xn.access.ActorAccess.GetData(actor).get(KEY_SKILL_COOLDOWN, out lastCooldown, 0f);
            float elapsed = Time.time - lastCooldown;
            if (elapsed >= baseCooldown)
            {
                xn.access.ActorAccess.GetData(actor).set(KEY_SKILL_COOLDOWN, Time.time);
                return true;
            }
            float remainingCD = baseCooldown - elapsed;
            float reducedCD = TryReduceCooldownWithLingli(actor, realmIndex, baseCooldown, remainingCD);
            if (reducedCD <= 0f)
            {
                xn.access.ActorAccess.GetData(actor).set(KEY_SKILL_COOLDOWN, Time.time);
                return true;
            }
            return false; 
        }
        private static float TryReduceCooldownWithLingli(Actor actor, int realmIndex, float baseCooldown, float remainingCD)
        {
            if (actor == null || realmIndex < 2) return remainingCD;
            int lingliMax = GetLingliMax(actor, realmIndex);
            if (lingliMax <= 0) return remainingCD;
            xn.access.ActorAccess.GetData(actor).get(KEY_LINGLI, out int currentLingli, 0);
            if (currentLingli <= 0) return remainingCD;
            float costPercent;      
            float cdReduction;      
            float maxReductionPercent; 
            if (realmIndex <= 6) 
            {
                costPercent = 0.20f;
                cdReduction = 1f;
                maxReductionPercent = 0.50f;
            }
            else if (realmIndex <= 9) 
            {
                costPercent = 0.15f;
                cdReduction = 2f;
                maxReductionPercent = 0.75f;
            }
            else if (realmIndex <= 12) 
            {
                costPercent = 0.10f;
                cdReduction = 2f;
                maxReductionPercent = 0.90f;
            }
            else 
            {
                costPercent = 0.05f;
                cdReduction = 1f;
                maxReductionPercent = 1.00f; 
            }
            int costPerReduction = (int)(lingliMax * costPercent);
            if (costPerReduction <= 0) costPerReduction = 1;
            float maxReduction = baseCooldown * maxReductionPercent;
            float targetReduction = Mathf.Min(remainingCD, maxReduction);
            int timesNeeded = Mathf.CeilToInt(targetReduction / cdReduction);
            int timesCanAfford = currentLingli / costPerReduction;
            int actualTimes = Mathf.Min(timesNeeded, timesCanAfford);
            if (actualTimes > 0)
            {
                int totalCost = actualTimes * costPerReduction;
                xn.access.ActorAccess.GetData(actor).set(KEY_LINGLI, currentLingli - totalCost);
                float actualReduction = actualTimes * cdReduction;
                remainingCD -= actualReduction;
            }
            return Mathf.Max(0f, remainingCD);
        }
        private static int GetLingliMax(Actor actor, int realmIndex)
        {
            string realmId = realmIndex >= 0 && realmIndex < REALM_IDS.Length ? REALM_IDS[realmIndex] : null;
            if (!string.IsNullOrEmpty(realmId))
            {
                if (xn.world.CultivationStatsConfigTable.TryGetRealmConfig(realmId, out var config))
                {
                    return config.lingliMax;
                }
            }
            switch (realmIndex)
            {
                case 2: return 500;      
                case 3: return 800;      
                case 4: return 1500;     
                case 5: return 2500;     
                case 6: return 5000;     
                case 7: return 10000;    
                case 8: return 20000;    
                case 9: return 50000;    
                case 10: return 100000;  
                case 11: return 500000;  
                case 12: return 2000000; 
                case 13: return 10000000; 
                case 14: return 100000000; 
                default: return 500;
            }
        }
        private static float GetCooldownForRealm(int realmIndex)
        {
            switch (realmIndex)
            {
                case 2:  return 22f;  
                case 3:  return 19f;  
                case 4:  return 16f;  
                case 5:  return 14f;  
                case 6:  return 12f;  
                case 7:  return 10f;  
                case 8:  return 8f;   
                case 9:  return 7f;   
                case 10: return 6f;   
                case 11: return 5f;   
                case 12: return 4f;   
                case 13: return 3f;   
                case 14: return 2f;   
                default: return 0f;   
            }
        }
        private static void SpawnLightning(Actor actor, WorldTile tile, int realmIndex)
        {
            if (actor == null || tile == null) return;
            int baseRadius = 7;
            int bonusRadius = realmIndex > 2 ? realmIndex - 2 : 0;
            int radius = baseRadius + bonusRadius;
            float pScale = radius / 15f;
            var effect = EffectsLibrary.spawnAtTile("fx_lightning_medium", tile, pScale);
            if (effect != null)
            {
                effect.sprite_renderer.flipX = Randy.randomBool();
            }
            int normalDamage = 47 + (realmIndex - 2) * 5;
            float trueDamagePercent = 0.01f + (realmIndex - 2) * 0.03f;
            int radiusSquared = radius * radius;
            foreach (Actor target in Finder.getUnitsFromChunk(tile, 1))
            {
                if (target == actor) continue; 
                if (Toolbox.SquaredDistTile(target.current_tile, tile) > radiusSquared) continue;
                if (target.asset.can_be_hurt_by_powers)
                {
                    target.getHit(normalDamage, true, AttackType.Other, actor);
                    int attackerRealmIndex = GetCurrentRealmIndex(actor);
                    int attackerAncientIndex = GetCurrentAncientIndex(actor);
                    int attackerBeastIndex = GetCurrentBeastIndex(actor);
                    int targetRealmIndex = GetCurrentRealmIndex(target);
                    int targetAncientIndex = GetCurrentAncientIndex(target);
                    int targetBeastIndex = GetCurrentBeastIndex(target);
                    int attackerUnifiedRealm = attackerRealmIndex;
                    if (attackerUnifiedRealm < 0 && attackerAncientIndex >= 0)
                        attackerUnifiedRealm = MapAncientBeastIndexToRealmIndex(attackerAncientIndex);
                    if (attackerUnifiedRealm < 0 && attackerBeastIndex >= 0)
                        attackerUnifiedRealm = MapAncientBeastIndexToRealmIndex(attackerBeastIndex);
                    int targetUnifiedRealm = targetRealmIndex;
                    if (targetUnifiedRealm < 0 && targetAncientIndex >= 0)
                        targetUnifiedRealm = MapAncientBeastIndexToRealmIndex(targetAncientIndex);
                    if (targetUnifiedRealm < 0 && targetBeastIndex >= 0)
                        targetUnifiedRealm = MapAncientBeastIndexToRealmIndex(targetBeastIndex);
                    if (attackerUnifiedRealm >= 0 && (targetUnifiedRealm < 0 || attackerUnifiedRealm >= targetUnifiedRealm))
                    {
                        int maxHealth = target.getMaxHealth();
                        int trueDamage = (int)(maxHealth * trueDamagePercent);
                        xn.access.ActorAccess.GetData(target).health = Mathf.Max(0, xn.access.ActorAccess.GetData(target).health - trueDamage);
                        target.startColorEffect(ActorColorEffect.Red);
                        if (!target.hasHealth())
                        {
                            target.batch.c_check_deaths.Add(target);
                        }
                    }
                }
                target.calculateForce(target.current_tile.x, target.current_tile.y, tile.x, tile.y, 2.5f, 0f, true);
            }
            MapAction.applyTileDamage(tile, radius, AssetManager.terraform.get("lightning_normal"));
        }
        private static void SpawnGrenade(Actor actor, WorldTile tile)
        {
            if (actor == null || tile == null) return;
            World.world.drop_manager.spawnParabolicDrop(tile, "grenade", 0f, 0.62f, 104f, 0.7f, 23.5f);
        }
        private static void SpawnMeteorite(Actor actor, WorldTile tile)
        {
            if (actor == null || tile == null) return;
            Meteorite.spawnMeteorite(tile, actor);
        }
        private static void SpawnBomb(Actor actor, WorldTile tile)
        {
            if (actor == null || tile == null) return;
            World.world.drop_manager.spawn(tile, "bomb", -1f, -1f, -1L);
        }
        private static void SpawnBoulder(Actor actor, WorldTile tile)
        {
            if (actor == null || tile == null) return;
            var boulder = EffectsLibrary.spawn("fx_boulder", tile);
            if (boulder != null && boulder is Boulder)
            {
                Vector2 pos = new Vector2(tile.x, tile.y);
                ((Boulder)boulder).spawnOn(pos);
            }
        }
        private static System.Collections.Generic.Dictionary<WorldTile, float> s_heatRayTiles = new System.Collections.Generic.Dictionary<WorldTile, float>();
        private static System.Collections.Generic.Dictionary<WorldTile, float> s_heatRayLastTick = new System.Collections.Generic.Dictionary<WorldTile, float>();
        private const float HEAT_RAY_TICK_INTERVAL = 1f; 
        private static void SpawnHeatRay(Actor actor, WorldTile tile, float duration)
        {
            if (actor == null || tile == null) return;
            HeatRayEffect heatRayFx = xn.access.MapBoxAccess.GetHeatRayFx(World.world);
            if (heatRayFx == null) return;
            Vector2Int pos = tile.pos;
            xn.access.HeatRayEffectAccess.Play(heatRayFx, new Vector2(pos.x, pos.y), 10);
            s_heatRayTiles[tile] = Time.time + duration;
            s_heatRayLastTick[tile] = Time.time;
        }
        private static void SpawnAntimatterBomb(Actor actor, WorldTile tile)
        {
            if (actor == null || tile == null) return;
            World.world.startShake(0.3f, 0.01f, 0.03f);
            EffectsLibrary.spawn("fx_antimatter_effect", tile);
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class Patch_MapBox_Update_HeatRay
        {
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (s_heatRayTiles == null || s_heatRayTiles.Count == 0) return;
                float now = Time.time;
                var tilesToRemove = new System.Collections.Generic.List<WorldTile>();
                foreach (var kvp in s_heatRayTiles)
                {
                    var tile = kvp.Key;
                    float endTime = kvp.Value;
                    if (tile == null || now >= endTime)
                    {
                        tilesToRemove.Add(tile);
                        continue;
                    }
                    float lastTick;
                    if (!s_heatRayLastTick.TryGetValue(tile, out lastTick))
                    {
                        lastTick = now;
                        s_heatRayLastTick[tile] = now;
                    }
                    if (now - lastTick >= HEAT_RAY_TICK_INTERVAL)
                    {
                        HeatRayEffect heatRayFx = xn.access.MapBoxAccess.GetHeatRayFx(World.world);
                        Heat heat = xn.access.MapBoxAccess.GetHeat(World.world);
                        if (heatRayFx != null && xn.access.HeatRayEffectAccess.IsReady(heatRayFx) && heat != null)
                        {
                            xn.access.HeatAccess.AddTile(heat, tile, Randy.randomInt(1, 3));
                        }
                        s_heatRayLastTick[tile] = now;
                    }
                }
                foreach (var tile in tilesToRemove)
                {
                    s_heatRayTiles.Remove(tile);
                    s_heatRayLastTick.Remove(tile);
                }
            }
        }
        private static bool Prefix_Actor_getHit(Actor __instance, float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker, bool pMetallicWeapon, bool pSkipIfShake, bool pCheckDamageReduction)
        {
            if (__instance == null) return true;
            if (!s_immuneActors.Contains(__instance.id)) return true;
            int ownEffectImmune;
            xn.access.ActorAccess.GetData(__instance).get(KEY_OWN_EFFECT_IMMUNE, out ownEffectImmune, 0);
            if (ownEffectImmune != 1)
            {
                s_immuneActors.Remove(__instance.id);
                return true;
            }
            float effectTime;
            xn.access.ActorAccess.GetData(__instance).get(KEY_OWN_EFFECT_TIME, out effectTime, 0f);
            float timeSinceEffect = Time.time - effectTime;
            if (timeSinceEffect > IMMUNE_DURATION)
            {
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_EFFECT_IMMUNE, 0);
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_EFFECT_TIME, 0f);
                s_immuneActors.Remove(__instance.id);
                return true;
            }
            if (pAttacker != null && xn.access.BaseSimObjectAccess.IsActor(pAttacker) && xn.access.BaseSimObjectAccess.GetActor(pAttacker) == __instance)
            {
                return false; 
            }
            if (pAttacker == null && timeSinceEffect >= 0f)
            {
                if (pAttackType == AttackType.Explosion || pAttackType == AttackType.Fire)
                {
                    return false; 
                }
            }
            return true;
        }
        private static bool Prefix_Actor_addStatusEffect(Actor __instance, StatusAsset pStatusAsset, float pOverrideTimer, bool pColorEffect)
        {
            if (__instance == null || pStatusAsset == null) return true;
            if (pStatusAsset.id != "burning" && pStatusAsset.id != "stunned") return true;
            if (!s_immuneActors.Contains(__instance.id)) return true;
            int ownEffectImmune;
            xn.access.ActorAccess.GetData(__instance).get(KEY_OWN_EFFECT_IMMUNE, out ownEffectImmune, 0);
            if (ownEffectImmune != 1)
            {
                s_immuneActors.Remove(__instance.id);
                return true;
            }
            float effectTime;
            xn.access.ActorAccess.GetData(__instance).get(KEY_OWN_EFFECT_TIME, out effectTime, 0f);
            float timeSinceEffect = Time.time - effectTime;
            if (timeSinceEffect > IMMUNE_DURATION)
            {
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_EFFECT_IMMUNE, 0);
                xn.access.ActorAccess.GetData(__instance).set(KEY_OWN_EFFECT_TIME, 0f);
                s_immuneActors.Remove(__instance.id);
                return true;
            }
            return false;
        }
    }
}
