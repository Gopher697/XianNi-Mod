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
        #region Realm Conversion
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
        #region Bloodline Effect Application
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
        #region Primordial Bloodline Effect
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
        #region Verdantwood Bloodline Effect
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
                    treant.setName("Treant", false);
                    treesConverted++;
                    if (treesConverted >= maxTrees) break;
                }
            }
        }
        #endregion
        #region Allure Bloodline Effect
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
                mindslave.setName($"{victim.getName()} (Mindslave)", false);
                xn.access.ActorAccess.GetData(killer).set(KEY_MINDSLAVE_COUNT, slaveCount + 1);
            }
        }
        #endregion
        #region Houyi Bloodline Effect
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
        #region Yellow Springs Bloodline Effect
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
                skeleton.setName("Soul-Bound Skeleton", false);
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
        #region Curse Bloodline Effect
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
        #region Frostblood Bloodline Effect
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
        #region Giant-Demon Bloodline Effect
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
        #region Nirvana Bloodline Effect
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
        #region Spellbane Bloodline Effect
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
        #region Ancient Body Bloodline Effect
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
        #region Ageless Bloodline Effect
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
        #region Berserker Bloodline Effect
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
        #region Thunder Punishment Bloodline Effect
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
        #region Black Tortoise Bloodline Effect
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
        #region Mutated Bloodline - Calamity Venombody Effect
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
        #region Mutated Bloodline - Heavenbane Effect
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
        #region Mutated Bloodline - Corpseblight Effect
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
                skeleton.setName("Corpsevenom Skeleton", false);
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
        #region Mutated Bloodline - Fleeting Bloom Effect
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
        #region Mutated Bloodline - Aberrant Flesh Effect
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
                creature.setName("Flesh Avatar", false);
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
        #region Bloodline Talent Unlock State Query
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        private static string TalentName(string key, string fallback)
        {
            return T("bloodline_talent_name_" + key, fallback);
        }
        private static string TalentStatusLine(float concentration, int required, string key, string fallback)
        {
            string name = TalentName(key, fallback);
            return concentration >= required
                ? T("bloodline_talent_status_learned", "  [{0}] Learned", name)
                : T("bloodline_talent_status_not_learned", "  [{0}] Not learned (requires {1}% concentration)", name, required);
        }
        private static string TalentActiveLine(string key, string fallback)
        {
            return T("bloodline_talent_status_active", "  [{0}] Active", TalentName(key, fallback));
        }
        private static string TalentCostLine(string key, string fallback)
        {
            return T("bloodline_talent_status_cost", "  [{0}] Cost active", TalentName(key, fallback));
        }
        public static string GetBloodlineTalentStatus(Actor a)
        {
            if (a == null || !BloodlineSystem.HasBloodline(a)) return "";
            float concentration = BloodlineSystem.GetConcentration(a);
            string bloodlineType = BloodlineSystem.GetBloodlineType(a);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(T("bloodline_talent_status_header", "Bloodline Talents:"));
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "taigu_20", "Primordial Majesty"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "taigu_50", "Bloodline Suppression"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "taigu_80", "Divine Tremor"));
            }
            else if (bloodlineType == BloodlineTypes.CAOMU)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "caomu_20", "Natural Affinity"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "caomu_50", "Parasitic Spores"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "caomu_80", "Tree Realm Descent"));
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "meihuo_20", "Illusory Form"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "meihuo_50", "Mind Disturbance"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "meihuo_80", "Mind Slave"));
            }
            else if (bloodlineType == BloodlineTypes.HOUYI)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "houyi_20", "Hawk Eye"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "houyi_50", "Cloud Piercer"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "houyi_80", "Falling Sun"));
            }
            else if (bloodlineType == BloodlineTypes.HUANGQUAN)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "huangquan_20", "Yin Body"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "huangquan_50", "Soul Binding"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "huangquan_80", "Nether River Crossing"));
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "zuzhou_20", "Misfortune"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "zuzhou_50", "Weakening Field"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "zuzhou_80", "Soul-Destroying Curse"));
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "jihan_20", "Cold Body"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "jihan_50", "Ice Seal"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "jihan_80", "Ice Shatter"));
            }
            else if (bloodlineType == BloodlineTypes.JUMO)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "jumo_20", "Giant Body"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "jumo_50", "Blood Vitality"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "jumo_80", "Teleportation Art"));
            }
            else if (bloodlineType == BloodlineTypes.KUANGZHANSHI)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "kuangzhanshi_20", "Rage"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "kuangzhanshi_50", "Blood Fury"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "kuangzhanshi_80", "Unyielding"));
            }
            else if (bloodlineType == BloodlineTypes.NIEPAN)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "niepan_20", "Spirit Flame"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "niepan_50", "Embers"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "niepan_80", "True Fire Burst"));
            }
            else if (bloodlineType == BloodlineTypes.JINFA)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "jinfa_20", "Insulation"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "jinfa_50", "Spellbreaker"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "jinfa_80", "Anti-Magic Domain"));
            }
            else if (bloodlineType == BloodlineTypes.GUTI)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "guti_20", "Divine Skin"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "guti_50", "Divine Strength"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "guti_80", "Undying Body"));
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "suiyue_20", "Longevity"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "suiyue_50", "Wither and Flourish"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "suiyue_80", "Immortality"));
            }
            else if (bloodlineType == BloodlineTypes.LEIFA)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "leifa_20", "Thunder Body"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "leifa_50", "Lightning Call"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "leifa_80", "Thunder Pool"));
            }
            else if (bloodlineType == BloodlineTypes.XUANWU)
            {
                sb.AppendLine(TalentStatusLine(concentration, 20, "xuanwu_20", "Turtle Breath"));
                sb.AppendLine(TalentStatusLine(concentration, 50, "xuanwu_50", "Backlash"));
                sb.AppendLine(TalentStatusLine(concentration, 80, "xuanwu_80", "Absolute Defense"));
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                sb.AppendLine(TalentActiveLine("enan_active", "Ten Thousand Poisons Domain"));
                sb.AppendLine(TalentCostLine("enan_cost", "Solitary Ominous Star"));
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                sb.AppendLine(TalentActiveLine("tiansha_active", "Sacrifice Aura"));
                sb.AppendLine(TalentCostLine("tiansha_cost", "Doomed Companions"));
            }
            else if (bloodlineType == BloodlineTypes.SHIBIAN)
            {
                sb.AppendLine(TalentActiveLine("shibian_active", "Yellow Springs Corpse Poison"));
                sb.AppendLine(TalentCostLine("shibian_cost", "Severed Vitality"));
            }
            else if (bloodlineType == BloodlineTypes.ZAOSHUAI)
            {
                sb.AppendLine(TalentActiveLine("zaoshuai_active", "Heaven's Favored Child"));
                sb.AppendLine(TalentCostLine("zaoshuai_cost", "Fleeting Bloom"));
            }
            else if (bloodlineType == BloodlineTypes.JIBIAN)
            {
                sb.AppendLine(TalentActiveLine("jibian_active", "Flesh Proliferation"));
                sb.AppendLine(TalentCostLine("jibian_cost", "Shattered Mind"));
            }
            else
            {
                sb.AppendLine(T("bloodline_talent_status_developing", "  (Effects in development...)"));
            }
            return sb.ToString();
        }
        public static string GetBloodlineTalentDescription(string bloodlineType)
        {
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                return T("bloodline_talent_desc_taigu", "Primordial Bloodline Talents:\n20% [Primordial Majesty]: Base attack and defense +10%\n50% [Bloodline Suppression]: Non-Primordial enemies within 10 tiles lose 25 armor\n80% [Divine Tremor]: 30% chance to stun lower-realm enemies for 2 seconds when attacking");
            }
            else if (bloodlineType == BloodlineTypes.CAOMU)
            {
                return T("bloodline_talent_desc_caomu", "Woodland Bloodline Talents:\n20% [Natural Affinity]: Health regeneration +50% on grass or forest tiles\n50% [Parasitic Spores]: Attacks apply parasitic seeds that drain life for 5 seconds\n80% [Tree Realm Descent]: Summons a treant below 30% health, 180-second cooldown");
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO)
            {
                return T("bloodline_talent_desc_meihuo", "Charm Bloodline Talents:\n20% [Illusory Form]: Dodge +15%\n50% [Mind Disturbance]: 20% chance when hit to halt the attacker for 3 seconds\n80% [Mind Slave]: 30% chance to revive killed non-leader enemies as deathsworn servants (max 3)");
            }
            else if (bloodlineType == BloodlineTypes.HOUYI)
            {
                return T("bloodline_talent_desc_houyi", "Hou Yi Bloodline Talents:\n20% [Hawk Eye]: Attack range +2 tiles, accuracy +20%\n50% [Cloud Piercer]: Ranged attacks ignore 50% armor\n80% [Falling Sun]: 20% chance on attack to summon an arrow rain on the target, no cooldown");
            }
            else if (bloodlineType == BloodlineTypes.HUANGQUAN)
            {
                return T("bloodline_talent_desc_huangquan", "Yellow Springs Bloodline Talents:\n20% [Yin Body]: All stats +15% at night\n50% [Soul Binding]: Summons a skeleton for 30 seconds after killing an enemy\n80% [Nether River Crossing]: Continues fighting as an invincible soul for 10 seconds after death");
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU)
            {
                return T("bloodline_talent_desc_zuzhou", "Curse Bloodline Talents:\n20% [Misfortune]: Reflects 5% damage to attackers when hit\n50% [Weakening Field]: Enemies within 10 tiles lose 20% attack and movement speed\n80% [Soul-Destroying Curse]: Killed enemies cannot reincarnate or possess a new body");
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                return T("bloodline_talent_desc_jihan", "Extreme Cold Bloodline Talents:\n20% [Cold Body]: Fire damage taken -50%\n50% [Ice Seal]: Attacks have a 20% chance to freeze for 5 seconds\n80% [Ice Shatter]: Damage doubles against frozen enemies");
            }
            else if (bloodlineType == BloodlineTypes.JUMO)
            {
                return T("bloodline_talent_desc_jumo", "Troll Bloodline Talents:\n20% [Giant Body]: Max health +20%, size +20%\n50% [Blood Vitality]: Armor +20% while in combat\n80% [Teleportation Art]: Below 15% health, teleports up to 5 enemies to random map positions, 500-second cooldown");
            }
            else if (bloodlineType == BloodlineTypes.KUANGZHANSHI)
            {
                return T("bloodline_talent_desc_kuangzhanshi", "Berserker Bloodline Talents:\n20% [Rage]: Fully immune to fear and will not flee\n50% [Blood Fury]: Attack speed +1% for every 5% missing health\n80% [Unyielding]: At near death, locks health at 1 and gains 5 seconds of invincibility, 300-second cooldown");
            }
            else if (bloodlineType == BloodlineTypes.NIEPAN)
            {
                return T("bloodline_talent_desc_niepan", "Nirvana Bloodline Talents:\n20% [Spirit Flame]: Normal attacks apply burning\n50% [Embers]: After death, becomes an egg; if not destroyed within 10 seconds, revives with 50% health\n80% [True Fire Burst]: On Nirvana revival, deals fire damage within 8 tiles and applies burning");
            }
            else if (bloodlineType == BloodlineTypes.JINFA)
            {
                return T("bloodline_talent_desc_jinfa", "Forbidden Magic Bloodline Talents:\n20% [Insulation]: Projectile and meteor damage taken -50%\n50% [Spellbreaker]: Mages within 10 tiles cannot cast and lose 90% movement speed\n80% [Anti-Magic Domain]: Enemy projectiles within 10 tiles lose 90% flight speed");
            }
            else if (bloodlineType == BloodlineTypes.GUTI)
            {
                return T("bloodline_talent_desc_guti", "Ancient Body Bloodline Talents:\n20% [Divine Skin]: Armor +40%, immune to knockback, movement speed -60%\n50% [Divine Strength]: Mana becomes a health shield and is consumed before health when damaged\n80% [Undying Body]: Any single hit below 5% max health is forced to 1 damage");
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                return T("bloodline_talent_desc_suiyue", "Time Bloodline Talents:\n20% [Longevity]: Lifespan +20%\n50% [Wither and Flourish]: 5% chance on each attack to forcibly reduce enemy lifespan by 10 years\n80% [Immortality]: After age 1000, age is locked and no longer increases");
            }
            else if (bloodlineType == BloodlineTypes.LEIFA)
            {
                return T("bloodline_talent_desc_leifa", "Thunder Punishment Bloodline Talents:\n20% [Thunder Body]: Immune to lightning damage, movement speed +10%\n50% [Lightning Call]: 30% chance when hit to summon lightning against the attacker\n80% [Thunder Pool]: Below 15% health, high-frequency lightning storms strike nearby units indiscriminately");
            }
            else if (bloodlineType == BloodlineTypes.XUANWU)
            {
                return T("bloodline_talent_desc_xuanwu", "Xuanwu Bloodline Talents:\n20% [Turtle Breath]: While standing still, health regeneration +300%\n50% [Backlash]: Reflects 50% melee damage directly to the attacker\n80% [Absolute Defense]: At near death, gains 10 seconds of invincibility but cannot move, 300-second cooldown");
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                return T("bloodline_talent_desc_enan", "Calamity Poison Body (Mutated Bloodline):\n[Ten Thousand Poisons Domain]: During combat, enemy creatures within 15 tiles are continuously poisoned\n[Solitary Ominous Star] (Cost): Diplomacy is fixed at the minimum; health is permanently locked at 1% and cannot fully recover");
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                return T("bloodline_talent_desc_tiansha", "Heavenly Omen Bloodline (Mutated Bloodline):\n[Sacrifice Aura]: Each allied death within sight increases attack by 10%, stacking without limit until combat ends\n[Doomed Companions] (Cost): Nearby allies lose 40% defense; own luck is reduced by 50%");
            }
            else if (bloodlineType == BloodlineTypes.SHIBIAN)
            {
                return T("bloodline_talent_desc_shibian", "Corpse Transformation Bloodline (Mutated Bloodline):\n[Yellow Springs Corpse Poison]: Enemies within 5 tiles automatically contract corpse poison, losing 1% health per second; on death, 100% chance to become skeleton soldiers (max 20)\n[Severed Vitality] (Cost): Natural health regeneration is 0; all stats are halved during the Age of Miracles and Age of Hope");
            }
            else if (bloodlineType == BloodlineTypes.ZAOSHUAI)
            {
                return T("bloodline_talent_desc_zaoshuai", "Premature Decay Bloodline (Mutated Bloodline):\n[Heaven's Favored Child]: Cultivation speed +500%, comprehension set to 100, luck set to 100\n[Fleeting Bloom] (Cost): Maximum lifespan is forcibly locked at 100; death occurs directly at age 100");
            }
            else if (bloodlineType == BloodlineTypes.JIBIAN)
            {
                return T("bloodline_talent_desc_jibian", "Aberration Bloodline (Mutated Bloodline):\n[Flesh Proliferation]: When damaged, splits off random wild creatures to fight (max 10); after killing an enemy, 50% chance to instantly restore full health\n[Shattered Mind] (Cost): Intelligence is forced to 1, cannot become a lord or king, and may fall into confusion during combat and attack allies");
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
