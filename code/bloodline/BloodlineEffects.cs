using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ai;
namespace xn.bloodline
{
    public static class BloodlineEffects
    {
        private static bool _inited;
        private static Harmony _h;
        private static readonly string[] REALM_IDS = {
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
        private static readonly string[] ANC_STAR_IDS = {
            "ancient_01_star","ancient_02_star","ancient_03_star","ancient_04_star","ancient_05_star",
            "ancient_06_star","ancient_07_star","ancient_08_star","ancient_09_star","ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = {
            "beast_01_stage","beast_02_stage","beast_03_stage","beast_04_stage","beast_05_stage",
            "beast_06_stage","beast_07_stage","beast_08_stage","beast_09_stage","beast_10_stage"
        };
        private const string KEY_PARASITE_CASTER_ID = "xn.bloodline.parasite_caster";
        private const string KEY_PARASITE_END_TIME = "xn.bloodline.parasite_end";
        private const string KEY_PARASITE_DAMAGE = "xn.bloodline.parasite_dmg";
        private const string KEY_FORCE_DEATH = "xn.bloodline.force_death";
        private const string KEY_TREE_REALM_CD = "xn.bloodline.tree_realm_cd";
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _h = new Harmony("xn.bloodline.effects");
            _h.PatchAll(typeof(Patch_Actor_UpdateStats_BloodlineEffects));
            _h.PatchAll(typeof(Patch_MapBox_ApplyAttack_BloodlineEffects));
            _h.PatchAll(typeof(Patch_Actor_GetHit_BloodlineEffects));
        }
        #region 境界转换
        private static int ConvertAncientStarToRealmIndex(int star)
        {
            if (star <= 0) return -1;
            if (star == 1) return 2;   
            if (star == 2) return 4;   
            if (star == 3) return 6;   
            if (star == 4) return 7;   
            if (star == 5) return 8;   
            if (star == 6) return 9;   
            if (star == 7) return 10;  
            if (star == 8) return 11;  
            if (star == 9) return 13;  
            if (star == 10) return 14; 
            return -1;
        }
        private static int ConvertBeastStageToRealmIndex(int stage)
        {
            if (stage <= 0) return -1;
            if (stage == 1) return 2;   
            if (stage == 2) return 4;   
            if (stage == 3) return 6;   
            if (stage == 4) return 7;   
            if (stage == 5) return 8;   
            if (stage == 6) return 9;   
            if (stage == 7) return 10;  
            if (stage == 8) return 11;  
            if (stage == 9) return 13;  
            if (stage == 10) return 14; 
            return -1;
        }
        private static int GetUnifiedRealmIndex(Actor a)
        {
            if (a == null) return -1;
            int realmIdx = BloodlineSystem.GetRealmIndex(a);
            if (realmIdx >= 0) return realmIdx;
            int star = BloodlineSystem.GetAncientStar(a);
            if (star > 0)
            {
                int converted = ConvertAncientStarToRealmIndex(star);
                if (converted >= 0) return converted;
            }
            int stage = BloodlineSystem.GetBeastStage(a);
            if (stage > 0)
            {
                int converted = ConvertBeastStageToRealmIndex(stage);
                if (converted >= 0) return converted;
            }
            return -1;
        }
        private static bool IsTargetRealmLowerOrEqual(Actor caster, Actor target)
        {
            if (caster == null || target == null) return false;
            int casterRealm = GetUnifiedRealmIndex(caster);
            int targetRealm = GetUnifiedRealmIndex(target);
            if (casterRealm < 0 || targetRealm < 0) return false;
            return targetRealm <= casterRealm;
        }
        #endregion
        #region 血脉效果应用
        public static void ApplyPassiveEffects(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!BloodlineSystem.HasBloodline(a)) return;
            float concentration = BloodlineSystem.GetConcentration(a);
            if (concentration < 20f) return; 
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                ApplyTaiguPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.CAOMU)
            {
                ApplyCaomuPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO)
            {
                ApplyMeihuoPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.HOUYI)
            {
                ApplyHouyiPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.HUANGQUAN)
            {
                ApplyHuangquanPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                ApplyJihanPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.JUMO)
            {
                ApplyJumoPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.KUANGZHANSHI)
            {
                ApplyKuangzhanshiPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.GUTI)
            {
                ApplyGutiPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                ApplySuiyuePassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.LEIFA)
            {
                ApplyLeifaPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.XUANWU)
            {
                ApplyXuanwuPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                ApplyEnanPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                ApplyTianshaPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.SHIBIAN)
            {
                ApplyShibianPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.ZAOSHUAI)
            {
                ApplyZaoshuaiPassive(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.JIBIAN)
            {
                ApplyJibianPassive(a, concentration);
            }
        }
        public static void ApplyAuraEffects(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!BloodlineSystem.HasBloodline(a)) return;
            float concentration = BloodlineSystem.GetConcentration(a);
            if (concentration < 50f) return; 
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                ApplyTaiguAura(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU)
            {
                ApplyZuzhouAura(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.JINFA)
            {
                ApplyJinfaAura(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                ApplyEnanAura(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                ApplyTianshaAura(a, concentration);
            }
            else if (bloodlineType == BloodlineTypes.SHIBIAN)
            {
                ApplyShibianAura(a, concentration);
            }
        }
        public static void ApplyAttackTriggerEffects(Actor attacker, Actor target, float damage)
        {
            if (attacker == null || !attacker.isAlive()) return;
            if (target == null) return;
            if (!BloodlineSystem.HasBloodline(attacker)) return;
            float concentration = BloodlineSystem.GetConcentration(attacker);
            string bloodlineType = BloodlineSystem.GetBloodlineType(attacker);
            if (bloodlineType == BloodlineTypes.HOUYI)
            {
                if (concentration >= 50f && target.isAlive())
                {
                    ApplyHouyiAttackTrigger(attacker, target, concentration);
                }
                if (concentration >= 80f)
                {
                    ApplyHouyiSunsetTrigger(attacker, target, concentration);
                }
                return;
            }
            if (!target.isAlive()) return;
            if (bloodlineType == BloodlineTypes.TAIGU && concentration >= 80f)
            {
                ApplyTaiguAttackTrigger(attacker, target, concentration);
            }
            else if (bloodlineType == BloodlineTypes.CAOMU && concentration >= 50f)
            {
                ApplyCaomuAttackTrigger(attacker, target, concentration, damage);
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                if (concentration >= 50f)
                {
                    ApplyJihanFreezeAttackTrigger(attacker, target, concentration);
                }
                if (concentration >= 80f)
                {
                    ApplyJihanShatterAttackTrigger(attacker, target, concentration, damage);
                }
            }
            else if (bloodlineType == BloodlineTypes.NIEPAN)
            {
                if (concentration >= 20f)
                {
                    ApplyNiepanAttackTrigger(attacker, target, concentration);
                }
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                if (concentration >= 50f)
                {
                    ApplySuiyueAttackTrigger(attacker, target, concentration);
                }
            }
        }
        public static void ApplyGetHitTriggerEffects(Actor victim, float damage, BaseSimObject attacker)
        {
            if (victim == null) return;
            if (!BloodlineSystem.HasBloodline(victim)) return;
            float concentration = BloodlineSystem.GetConcentration(victim);
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType == BloodlineTypes.CAOMU && concentration >= 80f && victim.isAlive())
            {
                ApplyCaomuGetHitTrigger(victim, concentration);
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO && concentration >= 50f)
            {
                ApplyMeihuoGetHitTrigger(victim, concentration, attacker);
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU && concentration >= 20f)
            {
                ApplyZuzhouGetHitTrigger(victim, concentration, damage, attacker);
            }
            else if (bloodlineType == BloodlineTypes.JUMO && concentration >= 80f && victim.isAlive())
            {
                ApplyJumoTeleportTrigger(victim, concentration);
            }
            else if (bloodlineType == BloodlineTypes.LEIFA && concentration >= 50f)
            {
                ApplyLeifaGetHitTrigger(victim, concentration, attacker);
            }
            else if (bloodlineType == BloodlineTypes.XUANWU && concentration >= 50f)
            {
                ApplyXuanwuGetHitTrigger(victim, concentration, damage, attacker);
            }
        }
        public static void ProcessParasiteDOT(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            xn.access.ActorAccess.GetData(a).get(KEY_PARASITE_END_TIME, out int endTimeTick, 0);
            if (endTimeTick <= 0) return;
            int currentTimeTick = GetCurrentTimeTick();
            if (currentTimeTick >= endTimeTick)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_PARASITE_END_TIME, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_PARASITE_CASTER_ID, 0L);
                xn.access.ActorAccess.GetData(a).set(KEY_PARASITE_DAMAGE, 0);
                return;
            }
            xn.access.ActorAccess.GetData(a).get(KEY_PARASITE_CASTER_ID, out long casterId, 0L);
            xn.access.ActorAccess.GetData(a).get(KEY_PARASITE_DAMAGE, out int dotDamageInt, 0);
            float dotDamage = dotDamageInt / 10f; 
            if (casterId <= 0 || dotDamage <= 0) return;
            var caster = World.world.units.get(casterId);
            float tickDamage = dotDamage * 0.1f;
            if (tickDamage > 0 && a.isAlive())
            {
                a.getHit(tickDamage, true, AttackType.Other, caster);
                if (caster != null && caster.isAlive())
                {
                    caster.restoreHealth((int)tickDamage);
                }
            }
        }
        private static int GetCurrentTimeTick()
        {
            return (int)World.world.getCurWorldTime();
        }
        private static int GetFutureTimeTick(int seconds)
        {
            return GetCurrentTimeTick() + seconds;
        }
        #endregion
        #region 太古血脉效果实现
        private static void ApplyTaiguPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["multiplier_damage"] += 0.1f;
            xn.access.BaseSimObjectAccess.GetStats(a)["armor"] += 10f;
        }
        private static void ApplyTaiguAura(Actor a, float concentration)
        {
            if (concentration < 50f) return;
            var tile = a.current_tile;
            if (tile == null) return;
            int attackerRealm = GetUnifiedRealmIndex(a);
            if (attackerRealm < 0) return; 
            foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 10f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue; 
                if (a.kingdom == null || unit.kingdom == null) continue;
                if (!a.kingdom.isEnemy(unit.kingdom)) continue;
                if (BloodlineSystem.HasBloodline(unit))
                {
                    string unitType = BloodlineSystem.GetBloodlineType(unit);
                    if (unitType == BloodlineTypes.TAIGU) continue; 
                }
                if (!IsTargetRealmLowerOrEqual(a, unit)) continue;
                BaseStats unitStats = xn.access.BaseSimObjectAccess.GetStats(unit);
                if (unitStats == null) continue;
                unitStats["armor"] -= 25f;
            }
        }
        private static void ApplyTaiguAttackTrigger(Actor attacker, Actor target, float concentration)
        {
            if (concentration < 80f) return;
            int attackerRealm = GetUnifiedRealmIndex(attacker);
            int targetRealm = GetUnifiedRealmIndex(target);
            if (attackerRealm < 0 || targetRealm < 0) return;
            if (targetRealm >= attackerRealm) return; 
            if (UnityEngine.Random.value < 0.3f)
            {
                target.makeStunned(2f);
            }
        }
        private const string KEY_MINDSLAVE_COUNT = "xn.bloodline.mindslave_count";
        private const string KEY_MINDSLAVE_IDS = "xn.bloodline.mindslave_ids";
        private const string KEY_SUNSET_CD = "xn.bloodline.sunset_cd";
        #endregion
        #region 草木血脉效果实现
        private static void ApplyCaomuPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            var tile = a.current_tile;
            if (tile == null) return;
            bool isOnGrassOrForest = false;
            if (tile.Type != null)
            {
                if (tile.Type.grass) isOnGrassOrForest = true;
                if (tile.Type.is_biome && tile.Type.biome_id == "biome_forest") isOnGrassOrForest = true;
            }
            if (tile.building != null && tile.building.asset != null)
            {
                if (tile.building.asset.building_type == BuildingType.Building_Tree)
                {
                    isOnGrassOrForest = true;
                }
            }
            if (isOnGrassOrForest)
            {
                xn.access.BaseSimObjectAccess.GetStats(a)["health_regen"] += xn.access.BaseSimObjectAccess.GetStats(a)["health_regen"] * 0.5f;
            }
        }
        private static void ApplyCaomuAttackTrigger(Actor attacker, Actor target, float concentration, float damage)
        {
            if (concentration < 50f) return;
            if (!IsTargetRealmLowerOrEqual(attacker, target)) return;
            float dotDamage = damage * 0.2f;
            if (dotDamage < 1f) dotDamage = 1f;
            int endTimeTick = GetFutureTimeTick(5);
            xn.access.ActorAccess.GetData(target).set(KEY_PARASITE_CASTER_ID, attacker.getID());
            xn.access.ActorAccess.GetData(target).set(KEY_PARASITE_END_TIME, endTimeTick);
            xn.access.ActorAccess.GetData(target).set(KEY_PARASITE_DAMAGE, (int)(dotDamage * 10)); 
        }
        private static void ApplyCaomuGetHitTrigger(Actor victim, float concentration)
        {
            if (concentration < 80f) return;
            float healthPercent = (float)victim.getHealth() / (float)victim.getMaxHealth();
            if (healthPercent >= 0.3f) return;
            xn.access.ActorAccess.GetData(victim).get(KEY_TREE_REALM_CD, out int cdEndTimeTick, 0);
            int currentTimeTick = GetCurrentTimeTick();
            if (currentTimeTick < cdEndTimeTick) return;
            xn.access.ActorAccess.GetData(victim).set(KEY_TREE_REALM_CD, GetFutureTimeTick(180));
            var tile = victim.current_tile;
            if (tile == null) return;
            int treesConverted = 0;
            int maxTrees = 5; 
            foreach (var building in Finder.getBuildingsFromChunk(tile, 2, 10))
            {
                if (building == null || !building.isAlive()) continue;
                if (building.asset == null) continue;
                if (building.asset.building_type != BuildingType.Building_Tree) continue;
                var treeTile = building.current_tile;
                if (treeTile == null) continue;
                building.startDestroyBuilding();
                var treant = World.world.units.spawnNewUnit("living_plants", treeTile, false, true);
                if (treant != null)
                {
                    if (victim.kingdom != null)
                    {
                        xn.access.ActorAccess.SetKingdom(treant, victim.kingdom);
                    }
                    treant.setName("树人", false);
                    treesConverted++;
                    if (treesConverted >= maxTrees) break;
                }
            }
        }
        #endregion
        #region 魅惑血脉效果实现
        private static void ApplyMeihuoPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["Dodge"] += 15f;
        }
        private static void ApplyMeihuoGetHitTrigger(Actor victim, float concentration, BaseSimObject attacker)
        {
            if (concentration < 50f) return;
            if (attacker == null || !xn.access.BaseSimObjectAccess.IsActor(attacker)) return;
            Actor attackerActor = xn.access.BaseSimObjectAccess.GetActor(attacker);
            if (attackerActor == null || !attackerActor.isAlive()) return;
            if (!IsTargetRealmLowerOrEqual(victim, attackerActor)) return;
            if (UnityEngine.Random.value < 0.2f)
            {
                attackerActor.cancelAllBeh();
                attackerActor.makeWait(3f);
            }
        }
        public static void ApplyMeihuoKillTrigger(Actor killer, Actor victim)
        {
            if (killer == null || !killer.isAlive()) return;
            if (victim == null) return;
            if (!BloodlineSystem.HasBloodline(killer)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(killer);
            if (bloodlineType != BloodlineTypes.MEIHUO) return;
            float concentration = BloodlineSystem.GetConcentration(killer);
            if (concentration < 80f) return;
            if (victim.hasTrait("leader") || victim.isKing()) return;
            if (!IsTargetRealmLowerOrEqual(killer, victim)) return;
            xn.access.ActorAccess.GetData(killer).get(KEY_MINDSLAVE_COUNT, out int slaveCount, 0);
            if (slaveCount >= 3) return;
            if (UnityEngine.Random.value >= 0.3f) return;
            var tile = victim.current_tile;
            if (tile == null) return;
            string assetId = victim.asset.id;
            var mindslave = World.world.units.spawnNewUnit(assetId, tile, false, true);
            if (mindslave != null)
            {
                if (killer.kingdom != null)
                {
                    xn.access.ActorAccess.SetKingdom(mindslave, killer.kingdom);
                }
                mindslave.setName($"{victim.getName()}(心奴)", false);
                xn.access.ActorAccess.GetData(killer).set(KEY_MINDSLAVE_COUNT, slaveCount + 1);
            }
        }
        #endregion
        #region 后羿血脉效果实现
        private static void ApplyHouyiPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["range"] += 2f;
            xn.access.BaseSimObjectAccess.GetStats(a)["accuracy"] += 20f;
        }
        private static void ApplyHouyiAttackTrigger(Actor attacker, Actor target, float concentration)
        {
            if (concentration < 50f) return;
            if (!IsTargetRealmLowerOrEqual(attacker, target)) return;
        }
        private static void ApplyHouyiSunsetTrigger(Actor attacker, Actor target, float concentration)
        {
            if (concentration < 80f) return;
            if (!IsTargetRealmLowerOrEqual(attacker, target)) return;
            if (UnityEngine.Random.value > 0.2f) return;
            Vector2 launchPos = attacker.current_position;
            Vector2 targetPos = target.current_position;
            for (int i = 0; i < 5; i++)
            {
                Vector3 launchPos3D = new Vector3(
                    targetPos.x + UnityEngine.Random.Range(-2f, 2f),
                    targetPos.y + 10f + UnityEngine.Random.Range(0f, 5f),
                    0f
                );
                Vector3 targetPos3D = new Vector3(
                    targetPos.x + UnityEngine.Random.Range(-1f, 1f),
                    targetPos.y + UnityEngine.Random.Range(-1f, 1f),
                    0f
                );
                World.world.projectiles.spawn(
                    attacker,
                    target,
                    "arrow", 
                    launchPos3D,
                    targetPos3D,
                    0f,      
                    8f,      
                    null,    
                    attacker.kingdom
                );
            }
        }
        #endregion
        #region 黄泉血脉效果实现
        private const string KEY_HUANGQUAN_SKELETON_COUNT = "xn.bloodline.hq_skeleton_count";
        private const string KEY_MINGHE_ACTIVE = "xn.bloodline.minghe_active";
        private const string KEY_MINGHE_END_TIME = "xn.bloodline.minghe_end";
        private static void ApplyHuangquanPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            bool isNightEra = false;
            var era = World.world_era;
            if (era != null)
            {
                isNightEra = era.flag_night || era.flag_moon;
            }
            if (isNightEra)
            {
                xn.access.BaseSimObjectAccess.GetStats(a)["damage"] += xn.access.BaseSimObjectAccess.GetStats(a)["damage"] * 0.15f;
                xn.access.BaseSimObjectAccess.GetStats(a)["armor"] += 15f;
                xn.access.BaseSimObjectAccess.GetStats(a)["speed"] += xn.access.BaseSimObjectAccess.GetStats(a)["speed"] * 0.15f;
                xn.access.BaseSimObjectAccess.GetStats(a)["health"] += xn.access.BaseSimObjectAccess.GetStats(a)["health"] * 0.15f;
            }
        }
        public static void ApplyHuangquanKillTrigger(Actor killer, Actor victim)
        {
            if (killer == null || !killer.isAlive()) return;
            if (victim == null) return;
            if (!BloodlineSystem.HasBloodline(killer)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(killer);
            if (bloodlineType != BloodlineTypes.HUANGQUAN) return;
            float concentration = BloodlineSystem.GetConcentration(killer);
            if (concentration < 50f) return;
            if (!IsTargetRealmLowerOrEqual(killer, victim)) return;
            var tile = victim.current_tile;
            if (tile == null) return;
            var skeleton = World.world.units.createNewUnit("skeleton", tile, false, 0f, null, null, true, true);
            if (skeleton != null)
            {
                if (killer.kingdom != null)
                {
                    xn.access.ActorAccess.SetKingdom(skeleton, killer.kingdom);
                }
                skeleton.setName("拘魂骷髅", false);
                xn.access.ActorAccess.GetData(skeleton).set("xn.bloodline.skeleton_expire", GetFutureTimeTick(30));
            }
        }
        public static bool ApplyHuangquanDeathTrigger(Actor victim)
        {
            if (victim == null) return false;
            if (!BloodlineSystem.HasBloodline(victim)) return false;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.HUANGQUAN) return false;
            float concentration = BloodlineSystem.GetConcentration(victim);
            if (concentration < 80f) return false;
            xn.access.ActorAccess.GetData(victim).get(KEY_MINGHE_ACTIVE, out int active, 0);
            if (active == 1) return false; 
            xn.access.ActorAccess.GetData(victim).set(KEY_MINGHE_ACTIVE, 1);
            xn.access.ActorAccess.GetData(victim).set(KEY_MINGHE_END_TIME, GetFutureTimeTick(10));
            victim.restoreHealth(1);
            return true;
        }
        public static void ProcessMingheState(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            xn.access.ActorAccess.GetData(a).get(KEY_MINGHE_ACTIVE, out int active, 0);
            if (active != 1) return;
            xn.access.ActorAccess.GetData(a).get(KEY_MINGHE_END_TIME, out int endTime, 0);
            int currentTime = GetCurrentTimeTick();
            if (currentTime >= endTime)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_MINGHE_ACTIVE, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_MINGHE_END_TIME, 0);
                a.die(true, AttackType.Other, true, true);
            }
            else
            {
                if (a.getHealth() <= 0)
                {
                    a.restoreHealth(1);
                }
            }
        }
        #endregion
        #region 诅咒血脉效果实现
        private const string KEY_SOUL_DESTROYED = "xn.bloodline.soul_destroyed";
        private static void ApplyZuzhouPassive(Actor a, float concentration)
        {
        }
        public static void ApplyZuzhouGetHitTrigger(Actor victim, float conc, float damage, BaseSimObject attacker)
        {
            if (victim == null || !victim.isAlive()) return;
            if (!BloodlineSystem.HasBloodline(victim)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.ZUZHOU) return;
            if (conc < 20f) return;
            if (attacker == null || !xn.access.BaseSimObjectAccess.IsActor(attacker)) return;
            Actor attackerActor = xn.access.BaseSimObjectAccess.GetActor(attacker);
            if (attackerActor == null || !attackerActor.isAlive()) return;
            int reflectDamage = (int)(damage * 0.05f);
            if (reflectDamage > 0)
            {
                attackerActor.getHit(reflectDamage, true, AttackType.Other, victim);
            }
        }
        private static void ApplyZuzhouAura(Actor a, float concentration)
        {
            if (concentration < 50f) return;
            var tile = a.current_tile;
            if (tile == null) return;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 10f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue; 
                if (a.kingdom == null || unit.kingdom == null) continue;
                if (!a.kingdom.isEnemy(unit.kingdom)) continue;
                if (!IsTargetRealmLowerOrEqual(a, unit)) continue;
                BaseStats unitStats = xn.access.BaseSimObjectAccess.GetStats(unit);
                if (unitStats == null) continue;
                unitStats["damage"] -= unitStats["damage"] * 0.2f;
                unitStats["speed"] -= unitStats["speed"] * 0.2f;
            }
        }
        public static void ApplyZuzhouKillTrigger(Actor killer, Actor victim)
        {
            if (killer == null || !killer.isAlive()) return;
            if (victim == null) return;
            if (!BloodlineSystem.HasBloodline(killer)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(killer);
            if (bloodlineType != BloodlineTypes.ZUZHOU) return;
            float concentration = BloodlineSystem.GetConcentration(killer);
            if (concentration < 80f) return;
            if (!IsTargetRealmLowerOrEqual(killer, victim)) return;
            xn.access.ActorAccess.GetData(victim).set(KEY_SOUL_DESTROYED, 1);
        }
        public static bool IsSoulDestroyed(Actor a)
        {
            if (a == null) return false;
            xn.access.ActorAccess.GetData(a).get(KEY_SOUL_DESTROYED, out int destroyed, 0);
            return destroyed == 1;
        }
        #endregion
        #region 极寒血脉效果实现
        private const string KEY_FROZEN_END_TIME = "xn.bloodline.frozen_end";
        private static void ApplyJihanPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["fire_resistance"] += 50f;
        }
        private static void ApplyJihanFreezeAttackTrigger(Actor attacker, Actor target, float concentration)
        {
            if (concentration < 50f) return;
            if (!IsTargetRealmLowerOrEqual(attacker, target)) return;
            if (UnityEngine.Random.value < 0.2f)
            {
                int endTimeTick = GetFutureTimeTick(5);
                xn.access.ActorAccess.GetData(target).set(KEY_FROZEN_END_TIME, endTimeTick);
                target.makeWait(5f);
                target.addStatusEffect("frozen", 5f);
                target.startColorEffect(ActorColorEffect.White);
                if (target.current_tile != null)
                {
                    EffectsLibrary.spawn("fx_cast_ground_blue", target.current_tile, null, null, 0f,
                        target.current_position.x, target.current_position.y);
                }
            }
        }
        private static void ApplyJihanShatterAttackTrigger(Actor attacker, Actor target, float concentration, float damage)
        {
            if (concentration < 80f) return;
            if (!IsTargetRealmLowerOrEqual(attacker, target)) return;
            xn.access.ActorAccess.GetData(target).get(KEY_FROZEN_END_TIME, out int frozenEndTime, 0);
            int currentTime = GetCurrentTimeTick();
            if (frozenEndTime > 0 && currentTime < frozenEndTime)
            {
                target.getHit(damage, true, AttackType.Other, attacker);
                xn.access.ActorAccess.GetData(target).set(KEY_FROZEN_END_TIME, 0);
            }
        }
        public static bool IsFrozen(Actor a)
        {
            if (a == null) return false;
            xn.access.ActorAccess.GetData(a).get(KEY_FROZEN_END_TIME, out int frozenEndTime, 0);
            int currentTime = GetCurrentTimeTick();
            return frozenEndTime > 0 && currentTime < frozenEndTime;
        }
        #endregion
        #region 巨魔血脉效果实现
        private const string KEY_JUMO_TELEPORT_CD = "xn.bloodline.jumo_teleport_cd";
        private static void ApplyJumoPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["health"] += xn.access.BaseSimObjectAccess.GetStats(a)["health"] * 0.2f;
            xn.access.BaseSimObjectAccess.GetStats(a)["scale"] += 0.2f;
            if (concentration >= 50f)
            {
                bool inCombat = xn.access.ActorAccess.HasAttackTarget(a) || xn.access.ActorAccess.GetAttackedBy(a) != null;
                if (inCombat)
                {
                    xn.access.BaseSimObjectAccess.GetStats(a)["armor"] += 20f;
                }
            }
        }
        private static void ApplyJumoTeleportTrigger(Actor victim, float concentration)
        {
            if (concentration < 80f) return;
            float healthPercent = (float)victim.getHealth() / (float)victim.getMaxHealth();
            if (healthPercent >= 0.15f) return;
            xn.access.ActorAccess.GetData(victim).get(KEY_JUMO_TELEPORT_CD, out int cdEndTimeTick, 0);
            int currentTimeTick = GetCurrentTimeTick();
            if (currentTimeTick < cdEndTimeTick) return;
            xn.access.ActorAccess.GetData(victim).set(KEY_JUMO_TELEPORT_CD, GetFutureTimeTick(500));
            var tile = victim.current_tile;
            if (tile == null) return;
            int teleportedCount = 0;
            int maxTeleport = 5;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 3, 15f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == victim.getID()) continue;
                if (victim.kingdom == null || unit.kingdom == null) continue;
                if (!victim.kingdom.isEnemy(unit.kingdom)) continue;
                if (!IsTargetRealmLowerOrEqual(victim, unit)) continue;
                var randomTile = Toolbox.getRandomTileWithinDistance(tile, 100);
                if (randomTile != null && !randomTile.Type.liquid)
                {
                    unit.cancelAllBeh();
                    unit.setCurrentTile(randomTile);
                    teleportedCount++;
                    if (teleportedCount >= maxTeleport) break;
                }
            }
        }
        #endregion
        #region 涅槃血脉效果实现
        private const string KEY_NIEPAN_EGG_ACTIVE = "xn.bloodline.niepan_egg_active";
        private const string KEY_NIEPAN_EGG_END_TIME = "xn.bloodline.niepan_egg_end";
        private const string KEY_NIEPAN_EGG_MAX_HEALTH = "xn.bloodline.niepan_egg_maxhp";
        private const string KEY_NIEPAN_JUST_REVIVED = "xn.bloodline.niepan_just_revived";
        private static void ApplyNiepanAttackTrigger(Actor attacker, Actor target, float concentration)
        {
            if (concentration < 20f) return;
            if (!IsTargetRealmLowerOrEqual(attacker, target)) return;
            target.addStatusEffect("burning", 5f);
        }
        public static bool ApplyNiepanDeathTrigger(Actor victim)
        {
            if (victim == null) return false;
            if (!BloodlineSystem.HasBloodline(victim)) return false;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.NIEPAN) return false;
            float concentration = BloodlineSystem.GetConcentration(victim);
            if (concentration < 50f) return false;
            xn.access.ActorAccess.GetData(victim).get(KEY_NIEPAN_EGG_ACTIVE, out int active, 0);
            if (active == 1) return false; 
            xn.access.ActorAccess.GetData(victim).set(KEY_NIEPAN_EGG_ACTIVE, 1);
            xn.access.ActorAccess.GetData(victim).set(KEY_NIEPAN_EGG_END_TIME, GetFutureTimeTick(10)); 
            xn.access.ActorAccess.GetData(victim).set(KEY_NIEPAN_EGG_MAX_HEALTH, victim.getMaxHealth());
            victim.restoreHealth(1);
            victim.addStatusEffect("frozen", 10f);
            return true;
        }
        public static void ProcessNiepanEggState(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            xn.access.ActorAccess.GetData(a).get(KEY_NIEPAN_EGG_ACTIVE, out int active, 0);
            if (active != 1) return;
            xn.access.ActorAccess.GetData(a).get(KEY_NIEPAN_EGG_END_TIME, out int endTime, 0);
            int currentTime = GetCurrentTimeTick();
            if (currentTime >= endTime)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_NIEPAN_EGG_ACTIVE, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_NIEPAN_EGG_END_TIME, 0);
                xn.access.ActorAccess.GetData(a).get(KEY_NIEPAN_EGG_MAX_HEALTH, out int maxHealth, 100);
                int reviveHealth = maxHealth / 2;
                a.restoreHealth(reviveHealth);
                a.finishStatusEffect("frozen");
                xn.access.ActorAccess.GetData(a).set(KEY_NIEPAN_JUST_REVIVED, 1);
                float concentration = BloodlineSystem.GetConcentration(a);
                if (concentration >= 80f)
                {
                    ApplyNiepanFireburstTrigger(a, concentration);
                }
                xn.access.ActorAccess.GetData(a).set(KEY_NIEPAN_JUST_REVIVED, 0);
            }
            else
            {
                if (a.getHealth() <= 0)
                {
                    xn.access.ActorAccess.GetData(a).set(KEY_NIEPAN_EGG_ACTIVE, 0);
                    xn.access.ActorAccess.GetData(a).set(KEY_NIEPAN_EGG_END_TIME, 0);
                    a.finishStatusEffect("frozen");
                    a.die(true, AttackType.Other, true, true);
                }
            }
        }
        private static void ApplyNiepanFireburstTrigger(Actor a, float concentration)
        {
            if (concentration < 80f) return;
            var tile = a.current_tile;
            if (tile == null) return;
            float fireDamage = a.getMaxHealth() * 0.3f;
            if (fireDamage < 10f) fireDamage = 10f;
            int hitCount = 0;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 8f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue; 
                if (a.kingdom == null || unit.kingdom == null) continue;
                if (!a.kingdom.isEnemy(unit.kingdom)) continue;
                if (!IsTargetRealmLowerOrEqual(a, unit)) continue;
                unit.getHit(fireDamage, true, AttackType.Fire, a);
                unit.addStatusEffect("burning", 5f);
                hitCount++;
            }
            EffectsLibrary.spawn("fx_fireball_explosion", tile, null, null, 0f, a.current_position.x, a.current_position.y);
        }
        public static bool IsInNiepanEggState(Actor a)
        {
            if (a == null) return false;
            xn.access.ActorAccess.GetData(a).get(KEY_NIEPAN_EGG_ACTIVE, out int active, 0);
            return active == 1;
        }
        #endregion
        #region 禁法血脉效果实现
        private const string KEY_JINFA_SILENCED = "xn.bloodline.jinfa_silenced";
        public static float ApplyJinfaProjectileDamageReduction(Actor victim, float damage, AttackType attackType, bool isProjectile)
        {
            if (victim == null || !victim.isAlive()) return damage;
            if (!BloodlineSystem.HasBloodline(victim)) return damage;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.JINFA) return damage;
            float concentration = BloodlineSystem.GetConcentration(victim);
            if (concentration < 20f) return damage;
            if (isProjectile || attackType == AttackType.Explosion || attackType == AttackType.Other)
            {
                return damage * 0.5f;
            }
            return damage;
        }
        private static void ApplyJinfaAura(Actor a, float concentration)
        {
            if (concentration < 50f) return;
            var tile = a.current_tile;
            if (tile == null) return;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 10f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue; 
                if (a.kingdom == null || unit.kingdom == null) continue;
                if (!a.kingdom.isEnemy(unit.kingdom)) continue;
                if (!IsTargetRealmLowerOrEqual(a, unit)) continue;
                BaseStats unitStats = xn.access.BaseSimObjectAccess.GetStats(unit);
                if (unitStats == null) continue;
                bool isMage = unitStats["mana"] > 0 ||
                              unit.hasTrait("mage") ||
                              unit.hasTrait("wizard") ||
                              (unit.asset.spells != null && unit.asset.spells.hasAny());
                if (isMage)
                {
                    if (!xn.access.BaseSimObjectAccess.HasStatus(unit, "slowness"))
                    {
                        unit.addStatusEffect("slowness", 1f);
                    }
                    unitStats["speed"] = 0.1f;
                    xn.access.ActorAccess.GetData(unit).set(KEY_JINFA_SILENCED, 1);
                }
            }
        }
        public static bool IsInJinfaAntimagicZone(Projectile projectile, Actor jinfaOwner)
        {
            if (projectile == null || jinfaOwner == null) return false;
            if (!jinfaOwner.isAlive()) return false;
            if (!BloodlineSystem.HasBloodline(jinfaOwner)) return false;
            string bloodlineType = BloodlineSystem.GetBloodlineType(jinfaOwner);
            if (bloodlineType != BloodlineTypes.JINFA) return false;
            float concentration = BloodlineSystem.GetConcentration(jinfaOwner);
            if (concentration < 80f) return false;
            Vector3 projectilePosition = xn.access.ProjectileAccess.GetCurrentPosition3D(projectile);
            float dist = UnityEngine.Vector2.Distance(
                new UnityEngine.Vector2(projectilePosition.x, projectilePosition.y),
                jinfaOwner.current_position
            );
            if (dist > 10f) return false;
            if (xn.access.ProjectileAccess.GetKingdom(projectile) == null || jinfaOwner.kingdom == null) return false;
            if (!jinfaOwner.kingdom.isEnemy(xn.access.ProjectileAccess.GetKingdom(projectile))) return false;
            return true;
        }
        public static float GetJinfaProjectileSpeedMultiplier()
        {
            return 0.1f; 
        }
        #endregion
        #region 古体血脉效果实现
        private const string KEY_GUTI_MANA_SHIELD_ACTIVE = "xn.bloodline.guti_mana_shield";
        private static void ApplyGutiPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["armor"] += 40f;
            xn.access.BaseSimObjectAccess.GetStats(a)["speed"] -= xn.access.BaseSimObjectAccess.GetStats(a)["speed"] * 0.6f;
            xn.access.BaseSimObjectAccess.GetStats(a)["mass"] += 1000f;
            if (concentration >= 50f)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_GUTI_MANA_SHIELD_ACTIVE, 1);
            }
        }
        public static float ApplyGutiManaShield(Actor victim, float damage)
        {
            if (victim == null || !victim.isAlive()) return damage;
            if (!BloodlineSystem.HasBloodline(victim)) return damage;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.GUTI) return damage;
            float concentration = BloodlineSystem.GetConcentration(victim);
            if (concentration < 50f) return damage;
            float currentMana = xn.access.BaseSimObjectAccess.GetStats(victim)["mana"];
            if (currentMana <= 0) return damage; 
            if (currentMana >= damage)
            {
                xn.access.BaseSimObjectAccess.GetStats(victim)["mana"] -= damage;
                return 0f; 
            }
            else
            {
                float remainingDamage = damage - currentMana;
                xn.access.BaseSimObjectAccess.GetStats(victim)["mana"] = 0;
                return remainingDamage;
            }
        }
        public static float ApplyGutiDamageReduction(Actor victim, float damage)
        {
            if (victim == null || !victim.isAlive()) return damage;
            if (!BloodlineSystem.HasBloodline(victim)) return damage;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.GUTI) return damage;
            float concentration = BloodlineSystem.GetConcentration(victim);
            if (concentration < 80f) return damage;
            float threshold = victim.getMaxHealth() * 0.05f;
            if (damage <= threshold && damage > 0)
            {
                return 1f;
            }
            return damage;
        }
        #endregion
        #region 岁月血脉效果实现
        private const string KEY_SUIYUE_AGE_LOCKED = "xn.bloodline.suiyue_age_locked";
        private const string KEY_SUIYUE_LOCKED_AGE = "xn.bloodline.suiyue_locked_age_val";
        private static void ApplySuiyuePassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["lifespan"] += xn.access.BaseSimObjectAccess.GetStats(a)["lifespan"] * 0.2f;
            if (concentration >= 80f)
            {
                int currentAge = a.getAge();
                if (currentAge >= 1000)
                {
                    xn.access.ActorAccess.GetData(a).get(KEY_SUIYUE_AGE_LOCKED, out int locked, 0);
                    if (locked == 0)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_SUIYUE_AGE_LOCKED, 1);
                        xn.access.ActorAccess.GetData(a).set(KEY_SUIYUE_LOCKED_AGE, currentAge);
                    }
                }
            }
        }
        private static void ApplySuiyueAttackTrigger(Actor attacker, Actor target, float concentration)
        {
            if (concentration < 50f) return;
            if (!IsTargetRealmLowerOrEqual(attacker, target)) return;
            if (UnityEngine.Random.value < 0.05f)
            {
                int currentAge = target.getAge();
                int maxAge = (int)xn.access.BaseSimObjectAccess.GetStats(target)["lifespan"];
                xn.access.BaseSimObjectAccess.GetStats(target)["lifespan"] -= 10f;
                if (xn.access.BaseSimObjectAccess.GetStats(target)["lifespan"] <= currentAge)
                {
                    target.die(true, AttackType.Age, true, true);
                }
                else
                {
                }
            }
        }
        public static void ProcessSuiyueImmortalState(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!BloodlineSystem.HasBloodline(a)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            if (bloodlineType != BloodlineTypes.SUIYUE) return;
            float concentration = BloodlineSystem.GetConcentration(a);
            if (concentration < 80f) return;
            xn.access.ActorAccess.GetData(a).get(KEY_SUIYUE_AGE_LOCKED, out int locked, 0);
            if (locked != 1) return;
            xn.access.ActorAccess.GetData(a).get(KEY_SUIYUE_LOCKED_AGE, out int lockedAge, 1000);
            int currentAge = a.getAge();
            if (currentAge > lockedAge)
            {
                int currentYear = Date.getCurrentYear();
                int targetBirthYear = currentYear - lockedAge;
                xn.access.ActorAccess.GetData(a).set("born_year", targetBirthYear);
            }
        }
        public static bool IsImmortal(Actor a)
        {
            if (a == null) return false;
            xn.access.ActorAccess.GetData(a).get(KEY_SUIYUE_AGE_LOCKED, out int locked, 0);
            return locked == 1;
        }
        #endregion
        #region 狂战士血脉效果实现
        private const string KEY_KUANGZHANSHI_BUQU_CD = "xn.bloodline.kuangzhanshi_buqu_cd";
        private const string KEY_KUANGZHANSHI_BUQU_ACTIVE = "xn.bloodline.kuangzhanshi_buqu_active";
        private const string KEY_KUANGZHANSHI_BUQU_END = "xn.bloodline.kuangzhanshi_buqu_end";
        private static void ApplyKuangzhanshiPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["courage"] = 100f; 
            if (concentration >= 50f)
            {
                float healthPercent = (float)a.getHealth() / (float)a.getMaxHealth() * 100f;
                float lostHealthPercent = 100f - healthPercent;
                float attackSpeedBonus = (lostHealthPercent / 5f) * 0.01f;
                xn.access.BaseSimObjectAccess.GetStats(a)["attack_speed"] += xn.access.BaseSimObjectAccess.GetStats(a)["attack_speed"] * attackSpeedBonus;
            }
            ProcessKuangzhanshiBuquState(a, concentration);
        }
        private static void ProcessKuangzhanshiBuquState(Actor a, float concentration)
        {
            if (concentration < 80f) return;
            xn.access.ActorAccess.GetData(a).get(KEY_KUANGZHANSHI_BUQU_ACTIVE, out int active, 0);
            if (active == 1)
            {
                xn.access.ActorAccess.GetData(a).get(KEY_KUANGZHANSHI_BUQU_END, out int endTime, 0);
                int currentTime = GetCurrentTimeTick();
                if (currentTime >= endTime)
                {
                    xn.access.ActorAccess.GetData(a).set(KEY_KUANGZHANSHI_BUQU_ACTIVE, 0);
                    xn.access.ActorAccess.GetData(a).set(KEY_KUANGZHANSHI_BUQU_END, 0);
                }
                else
                {
                    if (a.getHealth() <= 0)
                    {
                        a.restoreHealth(1);
                    }
                }
            }
        }
        public static bool ApplyKuangzhanshiDeathTrigger(Actor victim)
        {
            if (victim == null) return false;
            if (!BloodlineSystem.HasBloodline(victim)) return false;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.KUANGZHANSHI) return false;
            float concentration = BloodlineSystem.GetConcentration(victim);
            if (concentration < 80f) return false;
            xn.access.ActorAccess.GetData(victim).get(KEY_KUANGZHANSHI_BUQU_ACTIVE, out int active, 0);
            if (active == 1) return false; 
            xn.access.ActorAccess.GetData(victim).get(KEY_KUANGZHANSHI_BUQU_CD, out int cdEndTime, 0);
            int currentTime = GetCurrentTimeTick();
            if (currentTime < cdEndTime) return false;
            xn.access.ActorAccess.GetData(victim).set(KEY_KUANGZHANSHI_BUQU_ACTIVE, 1);
            xn.access.ActorAccess.GetData(victim).set(KEY_KUANGZHANSHI_BUQU_END, GetFutureTimeTick(5));
            xn.access.ActorAccess.GetData(victim).set(KEY_KUANGZHANSHI_BUQU_CD, GetFutureTimeTick(300));
            victim.restoreHealth(1);
            return true;
        }
        public static bool IsInBuquState(Actor a)
        {
            if (a == null) return false;
            xn.access.ActorAccess.GetData(a).get(KEY_KUANGZHANSHI_BUQU_ACTIVE, out int active, 0);
            return active == 1;
        }
        #endregion
        #region 雷罚血脉效果实现
        private const string KEY_LEIFA_LEICHI_ACTIVE = "xn.bloodline.leifa_leichi_active";
        private const string KEY_LEIFA_LEICHI_TICK = "xn.bloodline.leifa_leichi_tick";
        private static void ApplyLeifaPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.BaseSimObjectAccess.GetStats(a)["lightning_resistance"] += 100f;
            xn.access.BaseSimObjectAccess.GetStats(a)["speed"] += xn.access.BaseSimObjectAccess.GetStats(a)["speed"] * 0.1f;
        }
        public static void ApplyLeifaGetHitTrigger(Actor victim, float concentration, BaseSimObject attacker)
        {
            if (concentration < 50f) return;
            if (attacker == null || !xn.access.BaseSimObjectAccess.IsActor(attacker)) return;
            Actor attackerActor = xn.access.BaseSimObjectAccess.GetActor(attacker);
            if (attackerActor == null || !attackerActor.isAlive()) return;
            if (!IsTargetRealmLowerOrEqual(victim, attackerActor)) return;
            if (UnityEngine.Random.value < 0.3f)
            {
                float lightningDamage = xn.access.BaseSimObjectAccess.GetStats(victim)["damage"] * 1.5f;
                if (lightningDamage < 10f) lightningDamage = 10f;
                attackerActor.getHit(lightningDamage, true, AttackType.Other, victim);
                if (attackerActor.current_tile != null)
                {
                    EffectsLibrary.spawn("fx_lightning_small", attackerActor.current_tile, null, null, 0f,
                        attackerActor.current_position.x, attackerActor.current_position.y);
                }
            }
        }
        public static void ProcessLeifaLeichiState(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!BloodlineSystem.HasBloodline(a)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            if (bloodlineType != BloodlineTypes.LEIFA) return;
            float concentration = BloodlineSystem.GetConcentration(a);
            if (concentration < 80f) return;
            float healthPercent = (float)a.getHealth() / (float)a.getMaxHealth();
            if (healthPercent < 0.15f)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_LEIFA_LEICHI_ACTIVE, 1);
            }
            else
            {
                xn.access.ActorAccess.GetData(a).set(KEY_LEIFA_LEICHI_ACTIVE, 0);
                return;
            }
            xn.access.ActorAccess.GetData(a).get(KEY_LEIFA_LEICHI_ACTIVE, out int active, 0);
            if (active != 1) return;
            xn.access.ActorAccess.GetData(a).get(KEY_LEIFA_LEICHI_TICK, out int lastTick, 0);
            int currentTick = GetCurrentTimeTick();
            if (currentTick - lastTick < 1) return; 
            xn.access.ActorAccess.GetData(a).set(KEY_LEIFA_LEICHI_TICK, currentTick);
            var tile = a.current_tile;
            if (tile == null) return;
            float lightningDamage = xn.access.BaseSimObjectAccess.GetStats(a)["damage"] * 0.8f;
            if (lightningDamage < 5f) lightningDamage = 5f;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 8f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue; 
                unit.getHit(lightningDamage, true, AttackType.Other, a);
                if (UnityEngine.Random.value < 0.3f && unit.current_tile != null)
                {
                    EffectsLibrary.spawn("fx_lightning_small", unit.current_tile, null, null, 0f,
                        unit.current_position.x, unit.current_position.y);
                }
            }
        }
        #endregion
        #region 玄武血脉效果实现
        private const string KEY_XUANWU_DEFENSE_ACTIVE = "xn.bloodline.xuanwu_defense_active";
        private const string KEY_XUANWU_DEFENSE_END = "xn.bloodline.xuanwu_defense_end";
        private const string KEY_XUANWU_DEFENSE_CD = "xn.bloodline.xuanwu_defense_cd";
        private const string KEY_XUANWU_LAST_POS_X = "xn.bloodline.xuanwu_last_x";
        private const string KEY_XUANWU_LAST_POS_Y = "xn.bloodline.xuanwu_last_y";
        private const string KEY_XUANWU_STILL_TICKS = "xn.bloodline.xuanwu_still_ticks";
        private static void ApplyXuanwuPassive(Actor a, float concentration)
        {
            if (concentration < 20f) return;
            xn.access.ActorAccess.GetData(a).get(KEY_XUANWU_LAST_POS_X, out int lastX, -99999);
            xn.access.ActorAccess.GetData(a).get(KEY_XUANWU_LAST_POS_Y, out int lastY, -99999);
            int currentX = (int)(a.current_position.x * 100);
            int currentY = (int)(a.current_position.y * 100);
            if (lastX == currentX && lastY == currentY)
            {
                xn.access.ActorAccess.GetData(a).get(KEY_XUANWU_STILL_TICKS, out int stillTicks, 0);
                stillTicks++;
                xn.access.ActorAccess.GetData(a).set(KEY_XUANWU_STILL_TICKS, stillTicks);
                if (stillTicks > 10)
                {
                    xn.access.BaseSimObjectAccess.GetStats(a)["health_regen"] += xn.access.BaseSimObjectAccess.GetStats(a)["health_regen"] * 3f;
                }
            }
            else
            {
                xn.access.ActorAccess.GetData(a).set(KEY_XUANWU_STILL_TICKS, 0);
            }
            xn.access.ActorAccess.GetData(a).set(KEY_XUANWU_LAST_POS_X, currentX);
            xn.access.ActorAccess.GetData(a).set(KEY_XUANWU_LAST_POS_Y, currentY);
            ProcessXuanwuDefenseState(a, concentration);
        }
        public static void ApplyXuanwuGetHitTrigger(Actor victim, float concentration, float damage, BaseSimObject attacker)
        {
            if (concentration < 50f) return;
            if (attacker == null || !xn.access.BaseSimObjectAccess.IsActor(attacker)) return;
            Actor attackerActor = xn.access.BaseSimObjectAccess.GetActor(attacker);
            if (attackerActor == null || !attackerActor.isAlive()) return;
            float dist = UnityEngine.Vector2.Distance(victim.current_position, attackerActor.current_position);
            if (dist > 3f) return; 
            if (!IsTargetRealmLowerOrEqual(victim, attackerActor)) return;
            float reflectDamage = damage * 0.5f;
            if (reflectDamage > 0)
            {
                attackerActor.getHit(reflectDamage, true, AttackType.Other, victim);
            }
        }
        public static bool ApplyXuanwuDeathTrigger(Actor victim)
        {
            if (victim == null) return false;
            if (!BloodlineSystem.HasBloodline(victim)) return false;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.XUANWU) return false;
            float concentration = BloodlineSystem.GetConcentration(victim);
            if (concentration < 80f) return false;
            xn.access.ActorAccess.GetData(victim).get(KEY_XUANWU_DEFENSE_ACTIVE, out int active, 0);
            if (active == 1) return false; 
            xn.access.ActorAccess.GetData(victim).get(KEY_XUANWU_DEFENSE_CD, out int cdEndTime, 0);
            int currentTime = GetCurrentTimeTick();
            if (currentTime < cdEndTime) return false;
            xn.access.ActorAccess.GetData(victim).set(KEY_XUANWU_DEFENSE_ACTIVE, 1);
            xn.access.ActorAccess.GetData(victim).set(KEY_XUANWU_DEFENSE_END, GetFutureTimeTick(10));
            xn.access.ActorAccess.GetData(victim).set(KEY_XUANWU_DEFENSE_CD, GetFutureTimeTick(300));
            victim.restoreHealth(1);
            return true;
        }
        private static void ProcessXuanwuDefenseState(Actor a, float concentration)
        {
            if (concentration < 80f) return;
            xn.access.ActorAccess.GetData(a).get(KEY_XUANWU_DEFENSE_ACTIVE, out int active, 0);
            if (active != 1) return;
            xn.access.ActorAccess.GetData(a).get(KEY_XUANWU_DEFENSE_END, out int endTime, 0);
            int currentTime = GetCurrentTimeTick();
            if (currentTime >= endTime)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_XUANWU_DEFENSE_ACTIVE, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_XUANWU_DEFENSE_END, 0);
            }
            else
            {
                if (a.getHealth() <= 0)
                {
                    a.restoreHealth(1);
                }
                xn.access.BaseSimObjectAccess.GetStats(a)["speed"] = 0f;
            }
        }
        public static bool IsInXuanwuDefenseState(Actor a)
        {
            if (a == null) return false;
            xn.access.ActorAccess.GetData(a).get(KEY_XUANWU_DEFENSE_ACTIVE, out int active, 0);
            return active == 1;
        }
        #endregion
        #region 变异血脉 - 厄难毒体效果实现
        private const string KEY_ENAN_POISON_TICK = "xn.bloodline.enan_poison_tick";
        private static void ApplyEnanPassive(Actor a, float concentration)
        {
            xn.access.BaseSimObjectAccess.GetStats(a)["diplomacy"] = 0f;
            int maxHealth = a.getMaxHealth();
            int currentHealth = a.getHealth();
            int healthCap = (int)(maxHealth * 0.99f); 
            if (currentHealth > healthCap)
            {
                xn.access.ActorAccess.GetData(a).set("health", healthCap);
            }
        }
        private static void ApplyEnanAura(Actor a, float concentration)
        {
            bool inCombat = xn.access.ActorAccess.HasAttackTarget(a) || xn.access.ActorAccess.GetAttackedBy(a) != null;
            if (!inCombat) return;
            xn.access.ActorAccess.GetData(a).get(KEY_ENAN_POISON_TICK, out int lastTick, 0);
            int currentTick = GetCurrentTimeTick();
            if (currentTick - lastTick < 1) return;
            xn.access.ActorAccess.GetData(a).set(KEY_ENAN_POISON_TICK, currentTick);
            var tile = a.current_tile;
            if (tile == null) return;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 3, 15f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue;
                if (a.kingdom == null || unit.kingdom == null) continue;
                if (!a.kingdom.isEnemy(unit.kingdom)) continue;
                if (!xn.access.BaseSimObjectAccess.HasStatus(unit, "poisoned"))
                {
                    unit.addStatusEffect("poisoned", 3f);
                }
            }
        }
        #endregion
        #region 变异血脉 - 天煞血脉效果实现
        private const string KEY_TIANSHA_KILL_STACK = "xn.bloodline.tiansha_kill_stack";
        private const string KEY_TIANSHA_COMBAT_ID = "xn.bloodline.tiansha_combat_id";
        private static void ApplyTianshaPassive(Actor a, float concentration)
        {
            xn.access.BaseSimObjectAccess.GetStats(a)["luck"] -= xn.access.BaseSimObjectAccess.GetStats(a)["luck"] * 0.5f;
            xn.access.ActorAccess.GetData(a).get(KEY_TIANSHA_KILL_STACK, out int killStack, 0);
            if (killStack > 0)
            {
                float damageBonus = killStack * 0.1f;
                xn.access.BaseSimObjectAccess.GetStats(a)["damage"] += xn.access.BaseSimObjectAccess.GetStats(a)["damage"] * damageBonus;
            }
            bool inCombat = xn.access.ActorAccess.HasAttackTarget(a) || xn.access.ActorAccess.GetAttackedBy(a) != null;
            if (!inCombat && killStack > 0)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_TIANSHA_KILL_STACK, 0);
            }
        }
        private static void ApplyTianshaAura(Actor a, float concentration)
        {
            var tile = a.current_tile;
            if (tile == null) return;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 10f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue;
                if (a.kingdom == null || unit.kingdom == null) continue;
                if (a.kingdom.isEnemy(unit.kingdom)) continue; 
                if (a.kingdom.getID() != unit.kingdom.getID()) continue; 
                BaseStats unitStats = xn.access.BaseSimObjectAccess.GetStats(unit);
                if (unitStats == null) continue;
                unitStats["armor"] -= 40f;
            }
        }
        public static void OnAllyDeathForTiansha(Actor tianshaOwner, Actor deadAlly)
        {
            if (tianshaOwner == null || !tianshaOwner.isAlive()) return;
            if (deadAlly == null) return;
            if (!BloodlineSystem.HasBloodline(tianshaOwner)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(tianshaOwner);
            if (bloodlineType != BloodlineTypes.TIANSHA) return;
            if (tianshaOwner.kingdom == null || deadAlly.kingdom == null) return;
            if (tianshaOwner.kingdom.getID() != deadAlly.kingdom.getID()) return;
            float dist = UnityEngine.Vector2.Distance(tianshaOwner.current_position, deadAlly.current_position);
            if (dist > 15f) return;
            xn.access.ActorAccess.GetData(tianshaOwner).get(KEY_TIANSHA_KILL_STACK, out int killStack, 0);
            killStack++;
            xn.access.ActorAccess.GetData(tianshaOwner).set(KEY_TIANSHA_KILL_STACK, killStack);
        }
        public static int GetTianshaKillStack(Actor a)
        {
            if (a == null) return 0;
            xn.access.ActorAccess.GetData(a).get(KEY_TIANSHA_KILL_STACK, out int stack, 0);
            return stack;
        }
        #endregion
        #region 变异血脉 - 尸变血脉效果实现
        private const string KEY_SHIBIAN_POISON_TICK = "xn.bloodline.shibian_poison_tick";
        private const string KEY_SHIBIAN_SKELETON_COUNT = "xn.bloodline.shibian_skeleton_count";
        private const string KEY_SHIBIAN_POISONED_BY = "xn.bloodline.shibian_poisoned_by";
        private const string KEY_SHIBIAN_POISON_END = "xn.bloodline.shibian_poison_end";
        private static void ApplyShibianPassive(Actor a, float concentration)
        {
            xn.access.BaseSimObjectAccess.GetStats(a)["health_regen"] = 0f;
            var era = World.world_era;
            if (era != null)
            {
                bool isDebuffEra = era.flag_light_age ||
                                   (era.id != null && (era.id.Contains("hope") || era.id.Contains("miracle")));
                if (isDebuffEra)
                {
                    xn.access.BaseSimObjectAccess.GetStats(a)["damage"] *= 0.5f;
                    xn.access.BaseSimObjectAccess.GetStats(a)["armor"] *= 0.5f;
                    xn.access.BaseSimObjectAccess.GetStats(a)["speed"] *= 0.5f;
                    xn.access.BaseSimObjectAccess.GetStats(a)["health"] *= 0.5f;
                    xn.access.BaseSimObjectAccess.GetStats(a)["attack_speed"] *= 0.5f;
                }
            }
        }
        private static void ApplyShibianAura(Actor a, float concentration)
        {
            xn.access.ActorAccess.GetData(a).get(KEY_SHIBIAN_POISON_TICK, out int lastTick, 0);
            int currentTick = GetCurrentTimeTick();
            if (currentTick - lastTick < 1) return;
            xn.access.ActorAccess.GetData(a).set(KEY_SHIBIAN_POISON_TICK, currentTick);
            var tile = a.current_tile;
            if (tile == null) return;
            foreach (var unit in Finder.getUnitsFromChunk(tile, 1, 5f))
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.getID() == a.getID()) continue;
                if (a.kingdom == null || unit.kingdom == null) continue;
                if (!a.kingdom.isEnemy(unit.kingdom)) continue;
                xn.access.ActorAccess.GetData(unit).set(KEY_SHIBIAN_POISONED_BY, a.getID());
                xn.access.ActorAccess.GetData(unit).set(KEY_SHIBIAN_POISON_END, GetFutureTimeTick(3)); 
            }
        }
        public static void ProcessShibianPoisonDOT(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            xn.access.ActorAccess.GetData(a).get(KEY_SHIBIAN_POISON_END, out int endTimeTick, 0);
            if (endTimeTick <= 0) return;
            int currentTimeTick = GetCurrentTimeTick();
            if (currentTimeTick >= endTimeTick)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_SHIBIAN_POISON_END, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_SHIBIAN_POISONED_BY, 0L);
                return;
            }
            int maxHealth = a.getMaxHealth();
            float dotDamage = maxHealth * 0.01f * 0.1f; 
            if (dotDamage < 1f) dotDamage = 1f;
            xn.access.ActorAccess.GetData(a).get(KEY_SHIBIAN_POISONED_BY, out long casterId, 0L);
            var caster = casterId > 0 ? World.world.units.get(casterId) : null;
            a.getHit(dotDamage, true, AttackType.Other, caster);
        }
        public static void ApplyShibianKillTrigger(Actor killer, Actor victim)
        {
            if (killer == null || !killer.isAlive()) return;
            if (victim == null) return;
            if (!BloodlineSystem.HasBloodline(killer)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(killer);
            if (bloodlineType != BloodlineTypes.SHIBIAN) return;
            xn.access.ActorAccess.GetData(killer).get(KEY_SHIBIAN_SKELETON_COUNT, out int skeletonCount, 0);
            if (skeletonCount >= 20) return; 
            var tile = victim.current_tile;
            if (tile == null) return;
            var skeleton = World.world.units.createNewUnit("skeleton", tile, false, 0f, null, null, true, true);
            if (skeleton != null)
            {
                if (killer.kingdom != null)
                {
                    xn.access.ActorAccess.SetKingdom(skeleton, killer.kingdom);
                }
                skeleton.setName("尸毒骷髅", false);
                xn.access.ActorAccess.GetData(killer).set(KEY_SHIBIAN_SKELETON_COUNT, skeletonCount + 1);
            }
        }
        public static bool HasShibianPoison(Actor a)
        {
            if (a == null) return false;
            xn.access.ActorAccess.GetData(a).get(KEY_SHIBIAN_POISON_END, out int endTime, 0);
            int currentTime = GetCurrentTimeTick();
            return endTime > 0 && currentTime < endTime;
        }
        #endregion
        #region 变异血脉 - 早衰血脉效果实现
        private const string KEY_ZAOSHUAI_DEATH_CHECKED = "xn.bloodline.zaoshuai_death_checked";
        private static void ApplyZaoshuaiPassive(Actor a, float concentration)
        {
            xn.access.BaseSimObjectAccess.GetStats(a)["cultivation_speed"] += xn.access.BaseSimObjectAccess.GetStats(a)["cultivation_speed"] * 5f;
            xn.access.BaseSimObjectAccess.GetStats(a)["intelligence"] = 100f;
            xn.access.BaseSimObjectAccess.GetStats(a)["luck"] = 100f;
            xn.access.BaseSimObjectAccess.GetStats(a)["lifespan"] = 100f;
        }
        public static void ProcessZaoshuaiDeathCheck(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!BloodlineSystem.HasBloodline(a)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            if (bloodlineType != BloodlineTypes.ZAOSHUAI) return;
            int currentAge = a.getAge();
            if (currentAge >= 100)
            {
                xn.access.ActorAccess.GetData(a).get(KEY_ZAOSHUAI_DEATH_CHECKED, out int checked_, 0);
                if (checked_ == 1) return;
                xn.access.ActorAccess.GetData(a).set(KEY_ZAOSHUAI_DEATH_CHECKED, 1);
                a.die(true, AttackType.Age, true, true);
            }
        }
        #endregion
        #region 变异血脉 - 畸变血脉效果实现
        private const string KEY_JIBIAN_SUMMON_COUNT = "xn.bloodline.jibian_summon_count";
        private const string KEY_JIBIAN_CONFUSION_TICK = "xn.bloodline.jibian_confusion_tick";
        private const string KEY_JIBIAN_LAST_HIT_TICK = "xn.bloodline.jibian_last_hit_tick";
        private static readonly string[] WILD_CREATURES = {
            "wolf", "bear", "boar", "snake", "spider", "scorpion", "rat", "bat", "crab"
        };
        private static void ApplyJibianPassive(Actor a, float concentration)
        {
            xn.access.BaseSimObjectAccess.GetStats(a)["intelligence"] = 1f;
            ProcessJibianConfusion(a);
        }
        private static void ProcessJibianConfusion(Actor a)
        {
            bool inCombat = xn.access.ActorAccess.HasAttackTarget(a) || xn.access.ActorAccess.GetAttackedBy(a) != null;
            if (!inCombat) return;
            xn.access.ActorAccess.GetData(a).get(KEY_JIBIAN_CONFUSION_TICK, out int lastTick, 0);
            int currentTick = GetCurrentTimeTick();
            if (currentTick - lastTick < 10) return;
            xn.access.ActorAccess.GetData(a).set(KEY_JIBIAN_CONFUSION_TICK, currentTick);
            if (UnityEngine.Random.value < 0.05f)
            {
                var tile = a.current_tile;
                if (tile == null) return;
                Actor nearestAlly = null;
                float nearestDist = float.MaxValue;
                foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 10f))
                {
                    if (unit == null || !unit.isAlive()) continue;
                    if (unit.getID() == a.getID()) continue;
                    if (a.kingdom == null || unit.kingdom == null) continue;
                    if (a.kingdom.isEnemy(unit.kingdom)) continue;
                    if (a.kingdom.getID() != unit.kingdom.getID()) continue;
                    float dist = UnityEngine.Vector2.Distance(a.current_position, unit.current_position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestAlly = unit;
                    }
                }
                if (nearestAlly != null)
                {
                    float damage = xn.access.BaseSimObjectAccess.GetStats(a)["damage"] * 0.5f; 
                    nearestAlly.getHit(damage, true, AttackType.Other, a);
                }
            }
        }
        public static void ApplyJibianGetHitTrigger(Actor victim, float damage)
        {
            if (victim == null || !victim.isAlive()) return;
            if (!BloodlineSystem.HasBloodline(victim)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(victim);
            if (bloodlineType != BloodlineTypes.JIBIAN) return;
            xn.access.ActorAccess.GetData(victim).get(KEY_JIBIAN_LAST_HIT_TICK, out int lastTick, 0);
            int currentTick = GetCurrentTimeTick();
            if (currentTick - lastTick < 5) return; 
            xn.access.ActorAccess.GetData(victim).set(KEY_JIBIAN_LAST_HIT_TICK, currentTick);
            xn.access.ActorAccess.GetData(victim).get(KEY_JIBIAN_SUMMON_COUNT, out int summonCount, 0);
            if (summonCount >= 10) return; 
            if (UnityEngine.Random.value >= 0.3f) return;
            var tile = victim.current_tile;
            if (tile == null) return;
            string creatureType = WILD_CREATURES[UnityEngine.Random.Range(0, WILD_CREATURES.Length)];
            var spawnTile = Toolbox.getRandomTileWithinDistance(tile, 3);
            if (spawnTile == null || spawnTile.Type.liquid) spawnTile = tile;
            var creature = World.world.units.createNewUnit(creatureType, spawnTile, false, 0f, null, null, true, true);
            if (creature != null)
            {
                if (victim.kingdom != null)
                {
                    xn.access.ActorAccess.SetKingdom(creature, victim.kingdom);
                }
                creature.setName("血肉分身", false);
                xn.access.ActorAccess.GetData(victim).set(KEY_JIBIAN_SUMMON_COUNT, summonCount + 1);
            }
        }
        public static void ApplyJibianKillTrigger(Actor killer, Actor victim)
        {
            if (killer == null || !killer.isAlive()) return;
            if (victim == null) return;
            if (!BloodlineSystem.HasBloodline(killer)) return;
            string bloodlineType = BloodlineSystem.GetBloodlineType(killer);
            if (bloodlineType != BloodlineTypes.JIBIAN) return;
            if (UnityEngine.Random.value < 0.5f)
            {
                int maxHealth = killer.getMaxHealth();
                killer.restoreHealth(maxHealth);
            }
        }
        public static bool CanJibianBecomeLeader(Actor a)
        {
            if (a == null) return true;
            if (!BloodlineSystem.HasBloodline(a)) return true;
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            if (bloodlineType == BloodlineTypes.JIBIAN)
            {
                return false; 
            }
            return true;
        }
        #endregion
        #region 血脉天赋解锁状态查询
        public static string GetBloodlineTalentStatus(Actor a)
        {
            if (a == null || !BloodlineSystem.HasBloodline(a)) return "";
            float concentration = BloodlineSystem.GetConcentration(a);
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("血脉天赋：");
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                sb.AppendLine(concentration >= 20f ? "  [太古威严] 已领悟" : "  [太古威严] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [血脉压制] 已领悟" : "  [血脉压制] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [神震] 已领悟" : "  [神震] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.CAOMU)
            {
                sb.AppendLine(concentration >= 20f ? "  [自然亲和] 已领悟" : "  [自然亲和] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [寄生孢子] 已领悟" : "  [寄生孢子] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [树界降临] 已领悟" : "  [树界降临] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO)
            {
                sb.AppendLine(concentration >= 20f ? "  [幻形] 已领悟" : "  [幻形] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [乱心] 已领悟" : "  [乱心] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [心奴] 已领悟" : "  [心奴] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.HOUYI)
            {
                sb.AppendLine(concentration >= 20f ? "  [鹰眼] 已领悟" : "  [鹰眼] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [穿云] 已领悟" : "  [穿云] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [落日] 已领悟" : "  [落日] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.HUANGQUAN)
            {
                sb.AppendLine(concentration >= 20f ? "  [阴体] 已领悟" : "  [阴体] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [拘魂] 已领悟" : "  [拘魂] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [冥河渡] 已领悟" : "  [冥河渡] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU)
            {
                sb.AppendLine(concentration >= 20f ? "  [厄运] 已领悟" : "  [厄运] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [虚弱力场] 已领悟" : "  [虚弱力场] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [灭魂咒] 已领悟" : "  [灭魂咒] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                sb.AppendLine(concentration >= 20f ? "  [寒躯] 已领悟" : "  [寒躯] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [冰封] 已领悟" : "  [冰封] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [碎冰] 已领悟" : "  [碎冰] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.JUMO)
            {
                sb.AppendLine(concentration >= 20f ? "  [巨体] 已领悟" : "  [巨体] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [活血] 已领悟" : "  [活血] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [传送之术] 已领悟" : "  [传送之术] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.KUANGZHANSHI)
            {
                sb.AppendLine(concentration >= 20f ? "  [怒意] 已领悟" : "  [怒意] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [血怒] 已领悟" : "  [血怒] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [不屈] 已领悟" : "  [不屈] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.NIEPAN)
            {
                sb.AppendLine(concentration >= 20f ? "  [灵火] 已领悟" : "  [灵火] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [余烬] 已领悟" : "  [余烬] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [真火爆裂] 已领悟" : "  [真火爆裂] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.JINFA)
            {
                sb.AppendLine(concentration >= 20f ? "  [绝缘] 已领悟" : "  [绝缘] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [破法] 已领悟" : "  [破法] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [禁魔领域] 已领悟" : "  [禁魔领域] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.GUTI)
            {
                sb.AppendLine(concentration >= 20f ? "  [神皮] 已领悟" : "  [神皮] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [神力] 已领悟" : "  [神力] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [不灭体] 已领悟" : "  [不灭体] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                sb.AppendLine(concentration >= 20f ? "  [长生] 已领悟" : "  [长生] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [枯荣] 已领悟" : "  [枯荣] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [永生] 已领悟" : "  [永生] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.LEIFA)
            {
                sb.AppendLine(concentration >= 20f ? "  [雷体] 已领悟" : "  [雷体] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [引雷] 已领悟" : "  [引雷] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [雷池] 已领悟" : "  [雷池] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.XUANWU)
            {
                sb.AppendLine(concentration >= 20f ? "  [龟息] 已领悟" : "  [龟息] 未领悟 (需20%浓度)");
                sb.AppendLine(concentration >= 50f ? "  [反震] 已领悟" : "  [反震] 未领悟 (需50%浓度)");
                sb.AppendLine(concentration >= 80f ? "  [绝对防御] 已领悟" : "  [绝对防御] 未领悟 (需80%浓度)");
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                sb.AppendLine("  [万毒疆域] 已激活");
                sb.AppendLine("  [天煞孤星] 代价生效中");
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                sb.AppendLine("  [献祭光环] 已激活");
                sb.AppendLine("  [克死队友] 代价生效中");
            }
            else if (bloodlineType == BloodlineTypes.SHIBIAN)
            {
                sb.AppendLine("  [黄泉尸毒] 已激活");
                sb.AppendLine("  [生机断绝] 代价生效中");
            }
            else if (bloodlineType == BloodlineTypes.ZAOSHUAI)
            {
                sb.AppendLine("  [天道宠儿] 已激活");
                sb.AppendLine("  [昙花一现] 代价生效中");
            }
            else if (bloodlineType == BloodlineTypes.JIBIAN)
            {
                sb.AppendLine("  [血肉增殖] 已激活");
                sb.AppendLine("  [智力崩坏] 代价生效中");
            }
            else
            {
                sb.AppendLine("  (效果开发中...)");
            }
            return sb.ToString();
        }
        public static string GetBloodlineTalentDescription(string bloodlineType)
        {
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                return "太古血脉天赋：\n" +
                       "20% [太古威严]：基础攻击力与防御力提升10%\n" +
                       "50% [血脉压制]：周围10格内非太古血脉敌人护甲降低25%\n" +
                       "80% [神震]：攻击低境界敌人时30%概率眩晕2秒";
            }
            else if (bloodlineType == BloodlineTypes.CAOMU)
            {
                return "草木血脉天赋：\n" +
                       "20% [自然亲和]：在草地或森林上生命恢复速度提升50%\n" +
                       "50% [寄生孢子]：攻击附带寄生种子，持续5秒吸血\n" +
                       "80% [树界降临]：生命低于30%时召唤树人，冷却180秒";
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO)
            {
                return "魅惑血脉天赋：\n" +
                       "20% [幻形]：闪避率提升15%\n" +
                       "50% [乱心]：受击时20%概率使攻击者停顿3秒\n" +
                       "80% [心奴]：击杀非首领敌人时30%概率复活为死士(最多3名)";
            }
            else if (bloodlineType == BloodlineTypes.HOUYI)
            {
                return "后羿血脉天赋：\n" +
                       "20% [鹰眼]：攻击距离+2格，命中率+20%\n" +
                       "50% [穿云]：远程攻击无视50%护甲\n" +
                       "80% [落日]：攻击时20%概率召唤箭雨打击目标，无冷却";
            }
            else if (bloodlineType == BloodlineTypes.HUANGQUAN)
            {
                return "黄泉血脉天赋：\n" +
                       "20% [阴体]：夜晚时全属性提升15%\n" +
                       "50% [拘魂]：击杀敌人后召唤骷髅助战30秒\n" +
                       "80% [冥河渡]：死亡时以灵魂形态继续战斗10秒，期间无敌";
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU)
            {
                return "诅咒血脉天赋：\n" +
                       "20% [厄运]：受击时反弹5%伤害给攻击者\n" +
                       "50% [虚弱力场]：周围10格敌人攻击力和移速降低20%\n" +
                       "80% [灭魂咒]：击杀的敌人无法轮回和夺舍";
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                return "极寒血脉天赋：\n" +
                       "20% [寒躯]：受到的火焰伤害降低50%\n" +
                       "50% [冰封]：攻击有20%概率施加冻结状态，持续5秒\n" +
                       "80% [碎冰]：攻击处于冻结状态的敌人时，伤害翻倍";
            }
            else if (bloodlineType == BloodlineTypes.JUMO)
            {
                return "巨魔血脉天赋：\n" +
                       "20% [巨体]：生命上限提升20%，体型+20%\n" +
                       "50% [活血]：战斗时护甲提升20%\n" +
                       "80% [传送之术]：生命值低于15%时将至多5名敌人传送至地图随机位置，冷却500秒";
            }
            else if (bloodlineType == BloodlineTypes.KUANGZHANSHI)
            {
                return "狂战士血脉天赋：\n" +
                       "20% [怒意]：完全免疫恐惧状态，不会逃跑\n" +
                       "50% [血怒]：生命值每降低5%，攻击速度提升1%\n" +
                       "80% [不屈]：濒死时强制锁血1点，获得5秒无敌状态，冷却300秒";
            }
            else if (bloodlineType == BloodlineTypes.NIEPAN)
            {
                return "涅槃血脉天赋：\n" +
                       "20% [灵火]：普通攻击附带燃烧效果\n" +
                       "50% [余烬]：死亡后变为一颗蛋，若10秒内蛋未被摧毁，则以50%生命值复活\n" +
                       "80% [真火爆裂]：涅槃重生瞬间，对周围8格造成火焰伤害并附带燃烧";
            }
            else if (bloodlineType == BloodlineTypes.JINFA)
            {
                return "禁法血脉天赋：\n" +
                       "20% [绝缘]：投掷物和陨石造成伤害降低50%\n" +
                       "50% [破法]：范围10格内的法师无法施法且移动速度降低90%\n" +
                       "80% [禁魔领域]：周围10格内，所有敌方投射物飞行速度降低90%";
            }
            else if (bloodlineType == BloodlineTypes.GUTI)
            {
                return "古体血脉天赋：\n" +
                       "20% [神皮]：护甲提升40%，免疫击退效果，移速-60%\n" +
                       "50% [神力]：法力变为生命护盾，受伤时先消耗法力\n" +
                       "80% [不灭体]：单次伤害未超过生命上限5%时，强制判定为1伤害";
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                return "岁月血脉天赋：\n" +
                       "20% [长生]：寿命上限+20%\n" +
                       "50% [枯荣]：每次攻击5%概率强制减少敌人10年寿命\n" +
                       "80% [永生]：年龄超过1000岁时锁定年龄不再增加";
            }
            else if (bloodlineType == BloodlineTypes.LEIFA)
            {
                return "雷罚血脉天赋：\n" +
                       "20% [雷体]：免疫雷电伤害，移动速度提升10%\n" +
                       "50% [引雷]：受击时30%概率召唤闪电劈向攻击者\n" +
                       "80% [雷池]：生命值低于15%时，周围持续落下高频雷暴，无差别攻击";
            }
            else if (bloodlineType == BloodlineTypes.XUANWU)
            {
                return "玄武血脉天赋：\n" +
                       "20% [龟息]：静止不动时，生命恢复速度提升300%\n" +
                       "50% [反震]：受到的近战伤害50%直接反弹给攻击者\n" +
                       "80% [绝对防御]：濒死时获得10秒无敌，期间无法移动，冷却300秒";
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                return "厄难毒体（变异血脉）：\n" +
                       "[万毒疆域]：战斗时周围15格内敌方生物持续中毒\n" +
                       "[天煞孤星]（代价）：外交值固定为最低，生命值永远锁定1%无法恢复满";
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                return "天煞血脉（变异血脉）：\n" +
                       "[献祭光环]：视野范围内每有一个友军死亡，自身攻击力提升10%，可无限叠加直至战斗结束\n" +
                       "[克死队友]（代价）：周围友军防御力降低40%，自身气运值降低50%";
            }
            else if (bloodlineType == BloodlineTypes.SHIBIAN)
            {
                return "尸变血脉（变异血脉）：\n" +
                       "[黄泉尸毒]：周围5格内的敌人会自动染上尸毒，每秒扣除生命值1%，且死后100%转化为骷髅兵(至多20个)\n" +
                       "[生机断绝]（代价）：自然生命恢复速度为0，奇迹纪元和希望纪元全属性减半";
            }
            else if (bloodlineType == BloodlineTypes.ZAOSHUAI)
            {
                return "早衰血脉（变异血脉）：\n" +
                       "[天道宠儿]：修炼速度提升500%，悟性为100点气运为100点\n" +
                       "[昙花一现]（代价）：最大寿命强制锁定为100，若100岁直接死亡";
            }
            else if (bloodlineType == BloodlineTypes.JIBIAN)
            {
                return "畸变血脉（变异血脉）：\n" +
                       "[血肉增殖]：受到伤害时分裂出随机野生生物助战(至多10只)，击杀敌人后50%瞬间回满生命\n" +
                       "[智力崩坏]（代价）：智力强制锁定为1，无法成为领主或国王，战斗中有概率陷入混乱攻击友军";
            }
            return "";
        }
        #endregion
        #region Harmony Patches
        [HarmonyPatch(typeof(Actor), "updateStats")]
        private static class Patch_Actor_UpdateStats_BloodlineEffects
        {
            [HarmonyPostfix]
            private static void Postfix(Actor __instance)
            {
                if (__instance == null || !__instance.isAlive()) return;
                ApplyPassiveEffects(__instance);
                ApplyAuraEffects(__instance);
                ProcessParasiteDOT(__instance);
                ProcessMingheState(__instance);
                ProcessNiepanEggState(__instance);
                ProcessSuiyueImmortalState(__instance);
                ProcessLeifaLeichiState(__instance);
                ProcessShibianPoisonDOT(__instance);
                ProcessZaoshuaiDeathCheck(__instance);
            }
        }
        [HarmonyPatch(typeof(Actor), "die")]
        private static class Patch_Actor_Die_BloodlineEffects
        {
            [HarmonyPrefix]
            private static bool Prefix(Actor __instance, bool pDestroy)
            {
                if (__instance == null || !__instance.isAlive()) return true;
                xn.access.ActorAccess.GetData(__instance).get(KEY_FORCE_DEATH, out int forceDeath, 0);
                if (forceDeath == 1 || pDestroy)
                {
                    xn.access.ActorAccess.GetData(__instance).set(KEY_FORCE_DEATH, 0);
                    return true;
                }
                if (ApplyNiepanDeathTrigger(__instance))
                {
                    return false; 
                }
                if (ApplyHuangquanDeathTrigger(__instance))
                {
                    return false; 
                }
                if (ApplyKuangzhanshiDeathTrigger(__instance))
                {
                    return false; 
                }
                if (ApplyXuanwuDeathTrigger(__instance))
                {
                    return false; 
                }
                var attacker = xn.access.ActorAccess.GetAttackedBy(__instance);
                if (attacker != null && xn.access.BaseSimObjectAccess.IsActor(attacker))
                {
                    Actor killer = xn.access.BaseSimObjectAccess.GetActor(attacker);
                    if (killer != null && killer.isAlive())
                    {
                        ApplyMeihuoKillTrigger(killer, __instance);
                        ApplyHuangquanKillTrigger(killer, __instance);
                        ApplyZuzhouKillTrigger(killer, __instance);
                        ApplyShibianKillTrigger(killer, __instance);
                        ApplyJibianKillTrigger(killer, __instance);
                    }
                }
                return true; 
            }
        }
        [HarmonyPatch(typeof(MapBox), "applyAttack")]
        private static class Patch_MapBox_ApplyAttack_BloodlineEffects
        {
            [HarmonyPostfix]
            private static void Postfix(AttackData pData, BaseSimObject pTargetToCheck)
            {
                if (pTargetToCheck == null) return;
                if (pData.initiator == null) return;
                if (!xn.access.BaseSimObjectAccess.IsActor(pData.initiator)) return;
                if (!xn.access.BaseSimObjectAccess.IsActor(pTargetToCheck)) return;
                Actor attacker = xn.access.BaseSimObjectAccess.GetActor(pData.initiator);
                Actor target = xn.access.BaseSimObjectAccess.GetActor(pTargetToCheck);
                if (attacker == null || !attacker.isAlive()) return;
                if (target == null) return;
                ApplyAttackTriggerEffects(attacker, target, pData.damage);
            }
        }
        [HarmonyPatch(typeof(Actor), "getHit")]
        private static class Patch_Actor_GetHit_BloodlineEffects
        {
            [HarmonyPostfix]
            private static void Postfix(Actor __instance, float pDamage, BaseSimObject pAttacker)
            {
                if (__instance == null) return;
                ApplyGetHitTriggerEffects(__instance, pDamage, pAttacker);
                if (__instance.isAlive())
                {
                    ApplyJibianGetHitTrigger(__instance, pDamage);
                }
            }
        }
        [HarmonyPatch(typeof(Projectile), "update")]
        private static class Patch_Projectile_Update_JinfaAntimagicZone
        {
            [HarmonyPrefix]
            private static void Prefix(Projectile __instance)
            {
                if (__instance == null) return;
                if (xn.access.ProjectileAccess.GetKingdom(__instance) == null) return;
                if (xn.access.ProjectileAccess.GetKingdom(__instance).asset == null) return;
                Vector3 projectilePosition = xn.access.ProjectileAccess.GetCurrentPosition3D(__instance);
                Vector2 projectilePos = new Vector2(projectilePosition.x, projectilePosition.y);
                var tile = World.world.GetTile((int)projectilePos.x, (int)projectilePos.y);
                if (tile == null) return;
                foreach (var unit in Finder.getUnitsFromChunk(tile, 2, 10f))
                {
                    if (unit == null || !unit.isAlive()) continue;
                    if (unit.kingdom == null) continue;
                    if (unit.kingdom.asset == null) continue;
                    if (!unit.kingdom.isEnemy(xn.access.ProjectileAccess.GetKingdom(__instance))) continue;
                    if (!BloodlineSystem.HasBloodline(unit)) continue;
                    string bloodlineType = BloodlineSystem.GetBloodlineType(unit);
                    if (bloodlineType != BloodlineTypes.JINFA) continue;
                    float concentration = BloodlineSystem.GetConcentration(unit);
                    if (concentration < 80f) continue;
                    float dist = Vector2.Distance(projectilePos, unit.current_position);
                    if (dist > 10f) continue;
                    xn.access.ProjectileAccess.MultiplySpeed(__instance, GetJinfaProjectileSpeedMultiplier());
                    break; 
                }
            }
        }
        #endregion
    }
}
