using UnityEngine;
using System;
using System.Collections.Generic;
using HarmonyLib;
namespace xn.world
{
    public static class XNAttackActions
    {
        private const string KEY_LINGLI = "xn.stat.lingli";
        private const string KEY_YUANLI = "xn.stat.yuanli";
        private const string KEY_SANMEI_CD = "xn.divine.sanmei_cd_until";
        private const string KEY_WANJIAN_CD = "xn.divine.wanjian_cd_until";
        private const string KEY_WEIYA_CD = "xn.divine.weiya_cd_until";
        private const string KEY_XUANKONG_CD = "xn.divine.xuankong_cd_until";
        private const string KEY_ZHENKONG_CD = "xn.divine.zhenkong_cd_until";
        private const string KEY_JIUYIN_CD = "xn.divine.jiuyin_cd_until";
        private const string KEY_DUQI_CD = "xn.divine.duqi_cd_until";
        private const string KEY_JIANZHAN_CD = "xn.divine.jianzhan_cd_until";
        private const string KEY_ART_MISSILE_CD = "xn.art.missile_cd_until";
        private const string KEY_ART_SLASH_CD = "xn.art.slash_cd_until";
        private const string KEY_ART_QUAKE_CD = "xn.art.quake_cd_until";
        private const string KEY_ART_WAVES_CD = "xn.art.waves_cd_until";
        private const string KEY_ART_CONVERT_CD = "xn.art.convert_cd_until";
        private const string KEY_ART_PALM_CD = "xn.art.palm_cd_until";
        private const string KEY_ART_BREAKER_CD = "xn.art.breaker_cd_until";
        private const string KEY_ART_LINK_CD = "xn.art.link_cd_until";
        private const string KEY_BAONU_CD = "xn.divine.baonu_cd_until";
        private const string KEY_BAONU_ACTIVE = "xn.divine.baonu_active";
        private const string KEY_BAONU_END_TS = "xn.divine.baonu_end_ts";
        private const string KEY_BAONU_DMG_ADD = "xn.divine.baonu_dmg_add";
        private const int COST_BAONU = 500;
        private const float CD_BAONU = 300f;
        private const string KEY_ART_ASC_CD = "xn.art.asc_cd_until";
        private const string KEY_ASC_ACTIVE = "xn.art.asc_on";
        private const string KEY_ASC_END_TS = "xn.art.asc_end";
        private const string KEY_ASC_OLD_REALM = "xn.art.asc_old_realm";
        private const string KEY_ASC_TMP_REALM = "xn.art.asc_tmp_realm";
        private const string KEY_ASC_ADD_DMG = "xn.art.asc_add_dmg";
        private const string KEY_ASC_ADD_ARMOR = "xn.art.asc_add_armor";
        private const float CD_ART_ASC = 500f;
        private const string KEY_ART_SHIELD_CD = "xn.art.shield_cd_until";
        private const string KEY_ART_SHIELD_ACTIVE = "xn.art.shield_on";
        private const string KEY_ART_SHIELD_NEXT_DRAIN = "xn.art.shield_next_drain";
        private const string KEY_ART_SHIELD_RESIST_ADD = "xn.art.shield_resist_add";
        private const int COST_ART_SHIELD_OPEN_REQ = 100;
        private const int COST_ART_SHIELD_PER_SEC = 5;
        private const float CD_ART_SHIELD = 20f;
        private const string KEY_ART_SLASH_ACTIVE = "xn.art.slash_on";
        private const string KEY_ART_SLASH_END = "xn.art.slash_end";
        private const string KEY_ART_SLASH_NEXT = "xn.art.slash_next";
        private const string KEY_ART_SLASH_LEFT = "xn.art.slash_left";
        private const string KEY_ART_SLASH_TID = "xn.art.slash_tid";
        private const string KEY_ART_WAVES_ACTIVE = "xn.art.waves_on";
        private const string KEY_ART_WAVES_NEXT = "xn.art.waves_next";
        private const string KEY_ART_WAVES_LEFT = "xn.art.waves_left";
        private const string KEY_ART_WAVES_TID = "xn.art.waves_tid";
        private const string KEY_JIANZHAN_WEAK_ACTIVE = "xn.divine.jianzhan_weak_on";
        private const string KEY_JIANZHAN_WEAK_END = "xn.divine.jianzhan_weak_end";
        private const string KEY_JIANZHAN_WEAK_SUB = "xn.divine.jianzhan_weak_sub";
        private const string KEY_NOHEAL_END_TS = "xn.art.noheal_end";
        private const string KEY_NOHEAL_LAST_HP = "xn.art.noheal_lasthp";
        private const int COST_SANMEI = 750;
        private const float CD_SANMEI = 120f;
        private const int COST_WANJIAN = 800;
        private const float CD_WANJIAN = 150f;
        private const int COST_WEIYA = 50;
        private const float CD_WEIYA = 20f;
        private const int COST_XUANKONG = 100;
        private const float CD_XUANKONG = 25f;
        private const int COST_ZHENKONG = 200;
        private const float CD_ZHENKONG = 40f;
        private const int COST_JIUYIN = 300;
        private const float CD_JIUYIN = 80f;
        private const int COST_DUQI = 120;
        private const float CD_DUQI = 60f;
        private const int COST_JIANZHAN_YUANLI = 100;
        private const int COST_JIANZHAN_LINGLI = 2500;
        private const float CD_JIANZHAN = 300f;
        private const int COST_ART_MISSILE = 150;
        private const float CD_ART_MISSILE = 200f;
        private const int COST_ART_SLASH = 95;
        private const float CD_ART_SLASH = 520f;
        private const int COST_ART_QUAKE = 250;
        private const float CD_ART_QUAKE = 300f;
        private const int COST_ART_WAVES = 80;
        private const float CD_ART_WAVES = 140f;
        private const int COST_ART_CONVERT = 550;
        private const float CD_ART_CONVERT = 1000f;
        private const int COST_ART_PALM = 2000;
        private const float CD_ART_PALM = 5000f;
        private const int COST_ART_BREAKER = 420;
        private const float CD_ART_BREAKER = 450f;
        private const int COST_ART_LINK = 30;
        private const float CD_ART_LINK = 75f;
        private const float CHANCE_SANMEI = 0.65f;      
        private const float CHANCE_WANJIAN = 0.60f;     
        private const float CHANCE_WEIYA = 0.75f;       
        private const float CHANCE_XUANKONG = 0.70f;    
        private const float CHANCE_ZHENKONG = 0.68f;    
        private const float CHANCE_JIUYIN = 0.65f;      
        private const float CHANCE_DUQI = 0.68f;        
        private const float CHANCE_JIANZHAN = 0.50f;    
        private const float CHANCE_ART_MISSILE = 0.55f; 
        private const float CHANCE_ART_SLASH = 0.45f;   
        private const float CHANCE_ART_QUAKE = 0.50f;   
        private const float CHANCE_ART_WAVES = 0.60f;   
        private const float CHANCE_ART_CONVERT = 0.35f; 
        private const float CHANCE_ART_PALM = 0.25f;    
        private const float CHANCE_ART_BREAKER = 0.45f; 
        private const float CHANCE_ART_LINK = 0.70f;    
        private static readonly string[] REALM_IDS = {
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
        private static readonly float[] REALM_CD_FACTORS = {
            1.00f, 
            1.00f, 
            1.00f, 
            0.90f, 
            0.80f, 
            0.70f, 
            0.65f, 
            0.55f, 
            0.45f, 
            0.35f, 
            0.30f, 
            0.25f, 
            0.20f, 
            0.15f, 
            0.10f, 
            0.02f  
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
                if (a.hasTrait(REALM_IDS[i])) idx = i;
            return idx;
        }
        private static int GetAncIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            for (int i = 0; i < ANC_STAR_IDS.Length; i++)
                if (a.hasTrait(ANC_STAR_IDS[i])) idx = i;
            return idx;
        }
        private static int GetBeastIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            for (int i = 0; i < BEAST_STAGE_IDS.Length; i++)
                if (a.hasTrait(BEAST_STAGE_IDS[i])) idx = i;
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
        private static float GetCooldownFactor(Actor a)
        {
            int idx = GetUnifiedRealmIndex(a);
            if (idx < 0 || idx >= REALM_CD_FACTORS.Length) return 1f;
            return REALM_CD_FACTORS[idx];
        }
        private static float AdjCD(Actor a, float baseCD) => baseCD * GetCooldownFactor(a);
        public static AttackAction GetActionFor(string traitId)
        {
            return traitId switch
            {
                "divine_03_sanmeizhenhuo" => Action_Sanmei,
                "divine_04_wanjianguizong" => Action_Wanjian,
                "divine_02_weiya" => Action_Weiya,
                "divine_05_xuankongpo" => Action_Xuankong,
                "divine_06_zhenkongquan" => Action_Zhenkong,
                "divine_07_jiuyinbaiguzhao" => Action_Jiuyin,
                "divine_08_duqidan" => Action_Duqi,
                "divine_09_jianzhan" => Action_Jianzhan,
                "art_01_missile" => Action_ArtMissile,
                "art_03_slash" => Action_ArtSlash,
                "art_04_quake" => Action_ArtQuake,
                "art_05_waves" => Action_ArtWaves,
                "art_06_convert" => Action_ArtConvert,
                "art_07_palm" => Action_ArtPalm,
                "art_08_breaker" => Action_ArtBreaker,
                "art_10_link" => Action_ArtLink,
                "divine_10_baonu" => Action_Baonu,
                "art_02_ascension" => Action_ArtAscension,
                "art_09_shield" => Action_ArtShield,
                _ => null
            };
        }
        private static bool IsEnemy(Actor a, Actor b)
        {
            if (a == null || b == null) return false;
            if (a.kingdom == null || b.kingdom == null) return false;
            return a.kingdom != b.kingdom;
        }
        private static bool IsLowerInAnyGroup(Actor caster, Actor target)
        {
            if (caster == null || target == null) return false;
            int ar = GetRealmIndex(caster), tr = GetRealmIndex(target);
            if (ar >= 0 && tr >= 0) return tr < ar;
            int aa = GetAncIndex(caster), ta = GetAncIndex(target);
            if (aa >= 0 && ta >= 0) return ta < aa;
            int ab = GetBeastIndex(caster), tb = GetBeastIndex(target);
            if (ab >= 0 && tb >= 0) return tb < ab;
            return false;
        }
        private static bool IsHigherInAnyGroup(Actor caster, Actor target)
        {
            if (caster == null || target == null) return false;
            int ar = GetRealmIndex(caster), tr = GetRealmIndex(target);
            if (ar >= 0 && tr >= 0) return tr > ar;
            int aa = GetAncIndex(caster), ta = GetAncIndex(target);
            if (aa >= 0 && ta >= 0) return ta > aa;
            int ab = GetBeastIndex(caster), tb = GetBeastIndex(target);
            if (ab >= 0 && tb >= 0) return tb > ab;
            return false;
        }
        private static bool CheckCDAndCost(Actor caster, string cdKey, float cdTime, string costKey, int cost)
        {
            float now = Time.time;
            xn.access.ActorAccess.GetData(caster).get(cdKey, out float cd, 0f);
            if (now < cd) return false;
            xn.access.ActorAccess.GetData(caster).get(costKey, out int resource, 0);
            if (resource < cost) return false;
            xn.access.ActorAccess.GetData(caster).set(costKey, resource - cost);
            xn.access.ActorAccess.GetData(caster).set(cdKey, now + AdjCD(caster, cdTime)); 
            return true;
        }
        private static void DealDamage(Actor caster, Actor target, float multiplier)
        {
            if (target == null || !target.isAlive()) return;
            int dmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] * multiplier);
            if (dmg <= 0) return;
            if (caster != null) xn.access.ActorAccess.SetAttackedBy(target, caster);
            xn.access.ActorAccess.SetLastAttackType(target, AttackType.Other);
            target.getHit(dmg, pFlash: true, AttackType.Other, caster);
        }
        public static bool Action_Sanmei(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_SANMEI) return false;
            if (!CheckCDAndCost(caster, KEY_SANMEI_CD, CD_SANMEI, KEY_LINGLI, COST_SANMEI)) return false;
            ShentongFX.PlayOnce_Sanmei(target);
            DealDamage(caster, target, 4f);
            target.addStatusEffect("burning", 8f);
            var tile = target.current_tile;
            if (tile != null)
            {
                foreach (var u in Finder.getUnitsFromChunk(tile, 2, 3f))
                {
                    if (u == null || !u.isAlive() || xn.access.ActorAccess.GetData(u).id == xn.access.ActorAccess.GetData(target).id) continue;
                    if (!IsEnemy(caster, u)) continue;
                    u.addStatusEffect("burning", 8f);
                }
            }
            return true;
        }
        public static bool Action_Wanjian(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_WANJIAN) return false;
            if (!CheckCDAndCost(caster, KEY_WANJIAN_CD, CD_WANJIAN, KEY_LINGLI, COST_WANJIAN)) return false;
            var tile = target.current_tile;
            if (tile == null) return false;
            var list = new List<Actor>(64);
            foreach (var u in Finder.getUnitsFromChunk(tile, 2, 10f))
            {
                if (u == null || !u.isAlive()) continue;
                if (xn.access.ActorAccess.GetData(u).id == xn.access.ActorAccess.GetData(caster).id) continue;
                if (!IsEnemy(caster, u)) continue;
                list.Add(u);
            }
            ShentongFX.PlayOnce_Wanjian(tile, xn.access.ActorAccess.GetActorScale(caster));
            int segDmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] * 1.6f);
            for (int seg = 0; seg < 3; seg++)
            {
                foreach (var u in list)
                {
                    if (u == null || !u.isAlive()) continue;
                    u.getHit(segDmg, pFlash: true, AttackType.Other, caster);
                    u.addStatusEffect("slowness");
                }
            }
            return true;
        }
        public static bool Action_Weiya(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_WEIYA) return false;
            if (!IsLowerInAnyGroup(caster, target)) return false;
            if (!CheckCDAndCost(caster, KEY_WEIYA_CD, CD_WEIYA, KEY_LINGLI, COST_WEIYA)) return false;
            int dur = UnityEngine.Random.Range(10, 61); 
            ShentongFX.StartLoop_Weiya(target);
            RegisterWeiyaTarget(target, dur); 
            target.makeStunned(dur);
            return true;
        }
        public static bool Action_Xuankong(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_XUANKONG) return false;
            if (!CheckCDAndCost(caster, KEY_XUANKONG_CD, CD_XUANKONG, KEY_LINGLI, COST_XUANKONG)) return false;
            ShentongFX.PlayOnce_Xuankongpo(target);
            DealDamage(caster, target, 1.5f);
            target.addForce(0, 0, 8f, false, false); 
            return true;
        }
        public static bool Action_Zhenkong(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ZHENKONG) return false;
            var tile = caster.current_tile;
            if (tile == null) return false;
            Vector2 posCaster = xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(caster);
            Vector2 forward = ((Vector2)xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(target) - posCaster);
            if (forward.sqrMagnitude < 0.0001f) return false;
            forward.Normalize();
            var victims = new List<Actor>(16);
            foreach (var u in Finder.getUnitsFromChunk(tile, 2, 5f))
            {
                if (u == null || !u.isAlive()) continue;
                if (xn.access.ActorAccess.GetData(u).id == xn.access.ActorAccess.GetData(caster).id) continue;
                if (!IsEnemy(caster, u)) continue;
                Vector2 d = (Vector2)xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(u) - posCaster;
                if (d.magnitude > 5f) continue;
                if (Vector2.Dot(d.normalized, forward) <= 0f) continue;
                victims.Add(u);
            }
            if (victims.Count == 0) return false;
            if (!CheckCDAndCost(caster, KEY_ZHENKONG_CD, CD_ZHENKONG, KEY_LINGLI, COST_ZHENKONG)) return false;
            ShentongFX.PlayOnce_Zhenkongquan(target);
            int dmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] * 1.8f);
            foreach (var v in victims)
            {
                if (v == null || !v.isAlive()) continue;
                v.getHit(dmg, pFlash: true, AttackType.Other, caster);
                v.makeStunned(3);
                Vector2 vp = xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(v);
                Vector2 dir = (vp - posCaster).normalized;
                Vector2 hit = vp + dir * 0.1f;
                xn.access.ActorAccess.CalculateForce(v, vp.x, vp.y, hit.x, hit.y, 18f, 0f, true);
            }
            return true;
        }
        public static bool Action_Jiuyin(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_JIUYIN) return false;
            if (!CheckCDAndCost(caster, KEY_JIUYIN_CD, CD_JIUYIN, KEY_LINGLI, COST_JIUYIN)) return false;
            ShentongFX.PlayOnce_Jiuyin(target);
            int dmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] * 2.8f);
            float maxHP = xn.access.BaseSimObjectAccess.GetStats(target)["health"];
            if (maxHP <= 0f) maxHP = target.getMaxHealth();
            float hpNow = target.getHealth();
            if (dmg > 0)
            {
                target.getHit(dmg, pFlash: true, AttackType.Other, caster);
                caster.changeHealth(dmg); 
            }
            if (maxHP > 0f && hpNow / maxHP <= 0.15f)
            {
                int executeDmg = target.getHealth() + 10;
                target.getHit(executeDmg, pFlash: true, AttackType.Other, caster);
            }
            return true;
        }
        public static bool Action_Duqi(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_DUQI) return false;
            if (!CheckCDAndCost(caster, KEY_DUQI_CD, CD_DUQI, KEY_LINGLI, COST_DUQI)) return false;
            var tile = target.current_tile;
            if (tile == null) return false;
            ShentongFX.PlayOnce_Duqi(tile, xn.access.ActorAccess.GetActorScale(caster));
            int extraDmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] * 0.5f);
            if (extraDmg > 0)
            {
                target.getHit(extraDmg, pFlash: true, AttackType.Other, caster);
            }
            foreach (var u in Finder.getUnitsFromChunk(tile, 2, 6f))
            {
                if (u == null || !u.isAlive()) continue;
                if (!IsEnemy(caster, u)) continue;
                u.addStatusEffect("poisoned", 10f);
            }
            return true;
        }
        public static bool Action_Jianzhan(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_JIANZHAN) return false;
            xn.access.ActorAccess.GetData(caster).get(KEY_YUANLI, out int yuanli, 0);
            xn.access.ActorAccess.GetData(caster).get(KEY_LINGLI, out int lingli, 0);
            float now = Time.time;
            xn.access.ActorAccess.GetData(caster).get(KEY_JIANZHAN_CD, out float cd, 0f);
            if (now < cd) return false;
            bool useYuanli = yuanli >= COST_JIANZHAN_YUANLI;
            bool useLingli = !useYuanli && lingli >= COST_JIANZHAN_LINGLI;
            if (!useYuanli && !useLingli) return false;
            xn.access.ActorAccess.GetData(caster).set(KEY_JIANZHAN_CD, now + AdjCD(caster, CD_JIANZHAN));
            if (useYuanli) xn.access.ActorAccess.GetData(caster).set(KEY_YUANLI, yuanli - COST_JIANZHAN_YUANLI);
            else xn.access.ActorAccess.GetData(caster).set(KEY_LINGLI, lingli - COST_JIANZHAN_LINGLI);
            ShentongFX.PlayOnce_Jianzhan(target);
            int baseAtk = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"]);
            if (useYuanli)
            {
                int dmg = Mathf.FloorToInt(baseAtk * 7.0f);
                target.getHit(dmg, pFlash: true, AttackType.Other, caster);
                int trueDmg = Mathf.FloorToInt(baseAtk * 0.2f);
                target.getHit(trueDmg, pFlash: false, AttackType.Other, caster, pCheckDamageReduction: false);
                target.makeStunned(3f);
            }
            else
            {
                int dmg = Mathf.FloorToInt(baseAtk * 6.5f);
                target.getHit(dmg, pFlash: true, AttackType.Other, caster);
                int sub = Mathf.FloorToInt(baseAtk * 0.2f);
                if (sub > 0)
                {
                    xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] -= sub;
                    xn.access.ActorAccess.GetData(caster).set(KEY_JIANZHAN_WEAK_SUB, sub);
                    xn.access.ActorAccess.GetData(caster).set(KEY_JIANZHAN_WEAK_ACTIVE, 1);
                    xn.access.ActorAccess.GetData(caster).set(KEY_JIANZHAN_WEAK_END, now + 20f);
                }
            }
            return true;
        }
        public static bool Action_ArtMissile(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_MISSILE) return false;
            if (!CheckCDAndCost(caster, KEY_ART_MISSILE_CD, CD_ART_MISSILE, KEY_YUANLI, COST_ART_MISSILE)) return false;
            ShentongFX.PlayOnce_XS_Missile(target);
            DealDamage(caster, target, 5f);
            xn.access.ActorAccess.GetData(target).set(KEY_NOHEAL_END_TS, Time.time + 20f);
            xn.access.ActorAccess.GetData(target).set(KEY_NOHEAL_LAST_HP, target.getHealth());
            return true;
        }
        public static bool Action_ArtSlash(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_SLASH) return false;
            if (!CheckCDAndCost(caster, KEY_ART_SLASH_CD, CD_ART_SLASH, KEY_YUANLI, COST_ART_SLASH)) return false;
            float now = Time.time;
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SLASH_ACTIVE, 1);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SLASH_END, now + 2f);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SLASH_NEXT, now + 0.0f); 
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SLASH_LEFT, 10);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SLASH_TID, (int)xn.access.ActorAccess.GetData(target).id);
            caster.addStatusEffect("invincible", 2f);
            ShentongFX.PlayOnce_XS_Slash(target);
            return true;
        }
        public static bool Action_ArtQuake(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_QUAKE) return false;
            if (!CheckCDAndCost(caster, KEY_ART_QUAKE_CD, CD_ART_QUAKE, KEY_YUANLI, COST_ART_QUAKE)) return false;
            var tile = target.current_tile;
            if (tile == null) return false;
            ShentongFX.PlayOnce_XS_Quake(tile, xn.access.ActorAccess.GetActorScale(caster));
            int dmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] * 8.88f);
            foreach (var u in Finder.getUnitsFromChunk(tile, 4, 6f))
            {
                if (u == null || !u.isAlive()) continue;
                if (!IsEnemy(caster, u)) continue;
                u.getHit(dmg, pFlash: true, AttackType.Other, caster);
                u.addStatusEffect("stunned", 4f);
            }
            return true;
        }
        public static bool Action_ArtWaves(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_WAVES) return false;
            if (!CheckCDAndCost(caster, KEY_ART_WAVES_CD, CD_ART_WAVES, KEY_YUANLI, COST_ART_WAVES)) return false;
            float now = Time.time;
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_WAVES_ACTIVE, 1);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_WAVES_NEXT, now + 0.0f); 
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_WAVES_LEFT, 3);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_WAVES_TID, (int)xn.access.ActorAccess.GetData(target).id);
            ShentongFX.PlayOnce_XS_Waves(caster);
            return true;
        }
        public static bool Action_ArtConvert(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_CONVERT) return false;
            if (!CheckCDAndCost(caster, KEY_ART_CONVERT_CD, CD_ART_CONVERT, KEY_YUANLI, COST_ART_CONVERT)) return false;
            var tile = caster.current_tile;
            if (tile == null) return false;
            var enemies = new List<Actor>();
            var allies = new List<Actor>();
            foreach (var u in Finder.getUnitsFromChunk(tile, 2, 6f))
            {
                if (u == null || !u.isAlive()) continue;
                if (IsEnemy(caster, u))
                    enemies.Add(u);
                else if (caster.kingdom != null && u.kingdom == caster.kingdom)
                    allies.Add(u);
            }
            if (allies.Count == 0) allies.Add(caster); 
            ShentongFX.PlayOnce_XS_Convert(tile, xn.access.ActorAccess.GetActorScale(caster));
            int burst = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] * 4.8f);
            int totalDamage = 0;
            foreach (var u in enemies)
            {
                if (u == null || !u.isAlive()) continue;
                int before = u.getHealth();
                u.getHit(burst, pFlash: true, AttackType.Other, caster);
                int after = u.getHealth();
                if (after < before) totalDamage += (before - after);
                if (IsLowerInAnyGroup(caster, u))
                {
                    if (UnityEngine.Random.value < 0.15f && !u.hasTrait("root_07_broken"))
                        u.addTrait("root_07_broken");
                }
            }
            int per = (allies.Count > 0) ? Mathf.FloorToInt(totalDamage / allies.Count) : 0;
            foreach (var f in allies)
            {
                if (f == null || !f.isAlive()) continue;
                if (per > 0) f.changeHealth(per);
                f.finishStatusEffect("poisoned");
                f.finishStatusEffect("cursed");
                f.finishStatusEffect("burning");
                f.finishStatusEffect("slowness");
            }
            return true;
        }
        public static bool Action_ArtPalm(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_PALM) return false;
            if (!CheckCDAndCost(caster, KEY_ART_PALM_CD, CD_ART_PALM, KEY_YUANLI, COST_ART_PALM)) return false;
            var tile = target.current_tile;
            if (tile == null) return false;
            ShentongFX.PlayOnce_XS_Palm(tile, xn.access.ActorAccess.GetActorScale(caster));
            foreach (var u in Finder.getUnitsFromChunk(tile, 4, 6f))
            {
                if (u == null || !u.isAlive()) continue;
                if (!IsEnemy(caster, u)) continue;
                u.addStatusEffect("freeze", 5f);
            }
            int baseHP = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(target)["health"]);
            if (baseHP > 0)
            {
                int dmg = Mathf.FloorToInt(baseHP * 0.9f);
                if (IsHigherInAnyGroup(caster, target))
                {
                    int capHP = target.getMaxHealth();
                    int cap = (int)(capHP * 0.30f);
                    if (dmg > cap) dmg = cap;
                }
                target.getHit(dmg, pFlash: true, AttackType.Other, caster);
            }
            return true;
        }
        public static bool Action_ArtBreaker(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_BREAKER) return false;
            if (!CheckCDAndCost(caster, KEY_ART_BREAKER_CD, CD_ART_BREAKER, KEY_YUANLI, COST_ART_BREAKER)) return false;
            ShentongFX.PlayOnce_XS_Breaker(target);
            int baseAtk = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"]);
            int raw = Mathf.FloorToInt(baseAtk * 10.0f);
            float armor = xn.access.BaseSimObjectAccess.GetStats(target)["armor"];
            float effArmor = Mathf.Max(armor * 0.5f, 0f);
            float factor = 1f - (effArmor / 100f);
            if (factor < 0.01f) factor = 0.01f;
            int finalDmg = Mathf.FloorToInt(raw * factor);
            target.getHit(finalDmg, pFlash: true, AttackType.Other, caster);
            return true;
        }
        public static bool Action_ArtLink(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null || xn.access.BaseSimObjectAccess.GetActor(pTarget) == null || !xn.access.BaseSimObjectAccess.GetActor(pTarget).isAlive()) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            var target = xn.access.BaseSimObjectAccess.GetActor(pTarget);
            if (!IsEnemy(caster, target)) return false;
            if (UnityEngine.Random.value > CHANCE_ART_LINK) return false;
            if (!CheckCDAndCost(caster, KEY_ART_LINK_CD, CD_ART_LINK, KEY_YUANLI, COST_ART_LINK)) return false;
            ShentongFX.PlayOnce_XS_Link(caster);
            ShentongFX.PlayOnce_XS_Link(target);
            xn.access.ActorAccess.GetData(caster).set(KEY_LINK_ON, 1);
            xn.access.ActorAccess.GetData(caster).set(KEY_LINK_END, Time.time + 20f);
            xn.access.ActorAccess.GetData(caster).set(KEY_LINK_TID, (int)xn.access.ActorAccess.GetData(target).id);
            return true;
        }
        public static bool Action_Baonu(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            if (!caster.isAlive()) return false;
            xn.access.ActorAccess.GetData(caster).get(KEY_BAONU_ACTIVE, out int active, 0);
            if (active == 1) return false;
            float now = Time.time;
            xn.access.ActorAccess.GetData(caster).get(KEY_BAONU_CD, out float cd, 0f);
            if (now < cd) return false;
            xn.access.ActorAccess.GetData(caster).get(KEY_LINGLI, out int lingli, 0);
            if (lingli < COST_BAONU) return false;
            xn.access.ActorAccess.GetData(caster).set(KEY_BAONU_ACTIVE, 1);
            xn.access.ActorAccess.GetData(caster).set(KEY_BAONU_END_TS, now + 20f);
            xn.access.ActorAccess.GetData(caster).set(KEY_BAONU_CD, now + AdjCD(caster, CD_BAONU));
            int baseAtk = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"]);
            int addDmg = Mathf.FloorToInt(baseAtk * 1.5f);
            xn.access.ActorAccess.GetData(caster).set(KEY_BAONU_DMG_ADD, addDmg);
            if (addDmg > 0) xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] += addDmg;
            ShentongFX.StartLoop_Baonu(caster);
            caster.finishStatusEffect("stunned");
            caster.finishStatusEffect("freeze");
            caster.finishStatusEffect("slowness");
            return true;
        }
        public static bool Action_ArtAscension(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            if (!caster.isAlive()) return false;
            xn.access.ActorAccess.GetData(caster).get(KEY_ASC_ACTIVE, out int active, 0);
            if (active == 1) return false;
            float now = Time.time;
            xn.access.ActorAccess.GetData(caster).get(KEY_ART_ASC_CD, out float cd, 0f);
            if (now < cd) return false;
            int curHP = caster.getHealth();
            int maxHP = caster.getMaxHealth();
            if (maxHP <= 0) return false;
            float hpRatio = (float)curHP / maxHP;
            if (hpRatio >= 0.2f) return false;
            xn.access.ActorAccess.GetData(caster).get(KEY_YUANLI, out int yuanli, 0);
            if (yuanli < 100) return false;
            int curRealmIdx = GetRealmIndex(caster);
            if (curRealmIdx >= 12) return false; 
            xn.access.ActorAccess.GetData(caster).get(KEY_LINGLI, out int lingli, 0);
            xn.access.ActorAccess.GetData(caster).set(KEY_LINGLI, 0);
            xn.access.ActorAccess.GetData(caster).set(KEY_YUANLI, 0);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_ASC_CD, now + AdjCD(caster, CD_ART_ASC));
            string oldRealm = (curRealmIdx >= 0 && curRealmIdx < REALM_IDS.Length) ? REALM_IDS[curRealmIdx] : "";
            xn.access.ActorAccess.GetData(caster).set(KEY_ASC_OLD_REALM, oldRealm);
            string tmpRealm = "";
            if (curRealmIdx >= 0)
            {
                int dstIdx = Mathf.Min(curRealmIdx + 1, 12); 
                if (dstIdx != curRealmIdx && dstIdx < REALM_IDS.Length)
                {
                    tmpRealm = REALM_IDS[dstIdx];
                    caster.addTrait(tmpRealm); 
                }
            }
            xn.access.ActorAccess.GetData(caster).set(KEY_ASC_TMP_REALM, tmpRealm);
            int baseAtk = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["damage"]);
            int baseArmor = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(caster)["armor"]);
            int addDmg = Mathf.FloorToInt(baseAtk * 0.3f);
            int addArmor = Mathf.FloorToInt(baseArmor * 0.3f);
            if (addDmg != 0) xn.access.BaseSimObjectAccess.GetStats(caster)["damage"] += addDmg;
            if (addArmor != 0) xn.access.BaseSimObjectAccess.GetStats(caster)["armor"] += addArmor;
            xn.access.ActorAccess.GetData(caster).set(KEY_ASC_ACTIVE, 1);
            xn.access.ActorAccess.GetData(caster).set(KEY_ASC_END_TS, now + 15f);
            xn.access.ActorAccess.GetData(caster).set(KEY_ASC_ADD_DMG, addDmg);
            xn.access.ActorAccess.GetData(caster).set(KEY_ASC_ADD_ARMOR, addArmor);
            ShentongFX.StartLoop_XS_Ascension(caster);
            var tile = caster.current_tile;
            if (tile != null)
            {
                foreach (var u in Finder.getUnitsFromChunk(tile, 2, 10f))
                {
                    if (u == null || !u.isAlive()) continue;
                    if (!IsEnemy(caster, u)) continue;
                    if (IsLowerInAnyGroup(caster, u))
                        u.setTask("run_away", true, true, true);
                }
            }
            return true;
        }
        public static bool Action_ArtShield(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (xn.access.BaseSimObjectAccess.GetActor(pSelf) == null) return false;
            var caster = xn.access.BaseSimObjectAccess.GetActor(pSelf);
            if (!caster.isAlive()) return false;
            float now = Time.time;
            xn.access.ActorAccess.GetData(caster).get(KEY_ART_SHIELD_CD, out float cd, 0f);
            xn.access.ActorAccess.GetData(caster).get(KEY_ART_SHIELD_ACTIVE, out int active, 0);
            if (active == 1)
            {
                xn.access.ActorAccess.GetData(caster).set(KEY_ART_SHIELD_ACTIVE, 0);
                xn.access.ActorAccess.GetData(caster).set(KEY_ART_SHIELD_CD, now + AdjCD(caster, CD_ART_SHIELD));
                xn.access.ActorAccess.GetData(caster).get(KEY_ART_SHIELD_RESIST_ADD, out int addResist, 0);
                if (addResist != 0) xn.access.BaseSimObjectAccess.GetStats(caster)["Resist"] -= addResist;
                xn.access.ActorAccess.GetData(caster).set(KEY_ART_SHIELD_RESIST_ADD, 0);
                ShentongFX.StopLoop_XS_Shield(caster);
                return true;
            }
            if (now < cd) return false;
            if (!HasEnemyNearby(caster, 6f)) return false;
            xn.access.ActorAccess.GetData(caster).get(KEY_YUANLI, out int yuanli, 0);
            if (yuanli < COST_ART_SHIELD_OPEN_REQ) return false;
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SHIELD_ACTIVE, 1);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SHIELD_NEXT_DRAIN, now + 1f);
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SHIELD_CD, now + AdjCD(caster, CD_ART_SHIELD));
            int resistAdd = 999;
            xn.access.BaseSimObjectAccess.GetStats(caster)["Resist"] += resistAdd;
            xn.access.ActorAccess.GetData(caster).set(KEY_ART_SHIELD_RESIST_ADD, resistAdd);
            ShentongFX.StartLoop_XS_Shield(caster);
            return true;
        }
        private static bool HasEnemyNearby(Actor a, float radius)
        {
            if (a == null || !a.isAlive()) return false;
            var tile = a.current_tile;
            if (tile == null) return false;
            foreach (var u in Finder.getUnitsFromChunk(tile, 2, radius))
            {
                if (u == null || !u.isAlive()) continue;
                if (IsEnemy(a, u)) return true;
            }
            return false;
        }
        private static void EndShieldState(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).get(KEY_ART_SHIELD_RESIST_ADD, out int addResist, 0);
            if (addResist != 0) xn.access.BaseSimObjectAccess.GetStats(a)["Resist"] -= addResist;
            xn.access.ActorAccess.GetData(a).set(KEY_ART_SHIELD_ACTIVE, 0);
            xn.access.ActorAccess.GetData(a).set(KEY_ART_SHIELD_NEXT_DRAIN, 0f);
            xn.access.ActorAccess.GetData(a).set(KEY_ART_SHIELD_RESIST_ADD, 0);
            ShentongFX.StopLoop_XS_Shield(a);
        }
        private const string KEY_WEIYA_END = "xn.weiya.end";
        private const string KEY_LINK_ON = "xn.art.link_on";
        private const string KEY_LINK_END = "xn.art.link_end";
        private const string KEY_LINK_TID = "xn.art.link_tid";
        private static readonly HashSet<int> s_weiyaTargets = new HashSet<int>();
        public static void InitStateSystem(Harmony h)
        {
            var updateMethod = AccessTools.Method(typeof(MapBox), "Update");
            var postfix = new HarmonyMethod(typeof(StateMaintenancePatch), nameof(StateMaintenancePatch.Postfix));
            h.Patch(updateMethod, postfix: postfix);
            var getHitMethod = AccessTools.Method(typeof(Actor), "getHit",
                new[] { typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool) });
            var linkPrefix = new HarmonyMethod(typeof(SoulLinkDamagePatch), nameof(SoulLinkDamagePatch.Prefix));
            h.Patch(getHitMethod, prefix: linkPrefix);
        }
        private static class StateMaintenancePatch
        {
            private static float s_lastUpdate = 0f;
            private const float UPDATE_INTERVAL = 0.1f; 
            public static void Postfix()
            {
                float now = Time.time;
                if (now - s_lastUpdate < UPDATE_INTERVAL) return;
                s_lastUpdate = now;
                var units = MapBox.instance?.units?.getSimpleList();
                if (units == null || units.Count == 0) return;
                CheckWeiyaStates(units, now);
                CheckLinkStates(units, now);
                CheckJianzhanWeakStates(units, now);
                CheckNoHealStates(units, now);
                CheckSlashSchedule(units, now);
                CheckWavesSchedule(units, now);
                CheckBaonuStates(units, now);
                CheckAscensionStates(units, now);
                CheckShieldStates(units, now);
            }
            private static void CheckWeiyaStates(List<Actor> units, float now)
            {
                var toRemove = new List<int>();
                foreach (int actorId in s_weiyaTargets)
                {
                    Actor target = null;
                    foreach (var u in units)
                    {
                        if (u != null && (int)xn.access.ActorAccess.GetData(u).id == actorId)
                        {
                            target = u;
                            break;
                        }
                    }
                    if (target == null || !target.isAlive())
                    {
                        toRemove.Add(actorId);
                        continue;
                    }
                    xn.access.ActorAccess.GetData(target).get(KEY_WEIYA_END, out float endTime, 0f);
                    if (now >= endTime)
                    {
                        ShentongFX.StopLoop_Weiya(target);
                        toRemove.Add(actorId);
                    }
                }
                foreach (int id in toRemove)
                {
                    s_weiyaTargets.Remove(id);
                }
            }
            private static void CheckLinkStates(List<Actor> units, float now)
            {
                foreach (var actor in units)
                {
                    if (actor == null || !actor.isAlive()) continue;
                    xn.access.ActorAccess.GetData(actor).get(KEY_LINK_ON, out int linkOn, 0);
                    if (linkOn != 1) continue;
                    xn.access.ActorAccess.GetData(actor).get(KEY_LINK_END, out float linkEnd, 0f);
                    if (now >= linkEnd)
                    {
                        xn.access.ActorAccess.GetData(actor).set(KEY_LINK_ON, 0);
                        xn.access.ActorAccess.GetData(actor).set(KEY_LINK_TID, 0);
                    }
                }
            }
            private static void CheckJianzhanWeakStates(List<Actor> units, float now)
            {
                foreach (var a in units)
                {
                    if (a == null || !a.isAlive()) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_JIANZHAN_WEAK_ACTIVE, out int on, 0);
                    if (on != 1) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_JIANZHAN_WEAK_END, out float endt, 0f);
                    if (endt > 0f && now >= endt)
                    {
                        xn.access.ActorAccess.GetData(a).get(KEY_JIANZHAN_WEAK_SUB, out int sub, 0);
                        if (sub > 0) xn.access.BaseSimObjectAccess.GetStats(a)["damage"] += sub;
                        xn.access.ActorAccess.GetData(a).set(KEY_JIANZHAN_WEAK_SUB, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_JIANZHAN_WEAK_ACTIVE, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_JIANZHAN_WEAK_END, 0f);
                    }
                }
            }
            private static void CheckNoHealStates(List<Actor> units, float now)
            {
                foreach (var a in units)
                {
                    if (a == null || !a.isAlive()) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_NOHEAL_END_TS, out float end, 0f);
                    if (end <= 0f) continue;
                    if (now >= end)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_NOHEAL_END_TS, 0f);
                        xn.access.ActorAccess.GetData(a).set(KEY_NOHEAL_LAST_HP, 0);
                        continue;
                    }
                    xn.access.ActorAccess.GetData(a).get(KEY_NOHEAL_LAST_HP, out int last, a.getHealth());
                    int cur = a.getHealth();
                    if (cur > last)
                    {
                        a.changeHealth(-(cur - last)); 
                        cur = last;
                    }
                    xn.access.ActorAccess.GetData(a).set(KEY_NOHEAL_LAST_HP, cur);
                }
            }
            private static void CheckSlashSchedule(List<Actor> units, float now)
            {
                foreach (var a in units)
                {
                    if (a == null || !a.isAlive()) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_SLASH_ACTIVE, out int on, 0);
                    if (on != 1) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_SLASH_END, out float end, 0f);
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_SLASH_NEXT, out float next, 0f);
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_SLASH_LEFT, out int left, 0);
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_SLASH_TID, out int tid, 0);
                    if (now >= end || left <= 0)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_ACTIVE, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_END, 0f);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_LEFT, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_TID, 0);
                        continue;
                    }
                    if (now < next) continue;
                    Actor target = FindActorById(units, tid);
                    if (target == null || !target.isAlive())
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_ACTIVE, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_END, 0f);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_LEFT, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_TID, 0);
                        continue;
                    }
                    int segDmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(a)["damage"] * 0.9f);
                    if (segDmg > 0)
                    {
                        target.getHit(segDmg, pFlash: true, AttackType.Other, a);
                        a.changeHealth(Mathf.FloorToInt(segDmg * 0.5f)); 
                    }
                    ShentongFX.PlayOnce_XS_Slash(target);
                    xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_NEXT, now + 0.2f);
                    xn.access.ActorAccess.GetData(a).set(KEY_ART_SLASH_LEFT, left - 1);
                }
            }
            private static void CheckWavesSchedule(List<Actor> units, float now)
            {
                foreach (var a in units)
                {
                    if (a == null || !a.isAlive()) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_WAVES_ACTIVE, out int on, 0);
                    if (on != 1) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_WAVES_NEXT, out float next, 0f);
                    if (now < next) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_WAVES_LEFT, out int left, 0);
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_WAVES_TID, out int tid, 0);
                    if (left <= 0)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_ACTIVE, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_LEFT, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_TID, 0);
                        continue;
                    }
                    Actor target = FindActorById(units, tid);
                    if (target == null || !target.isAlive())
                    {
                        target = FindClosestEnemy(a, units, 6f);
                        if (target == null)
                        {
                            xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_ACTIVE, 0);
                            xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_LEFT, 0);
                            xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_TID, 0);
                            continue;
                        }
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_TID, (int)xn.access.ActorAccess.GetData(target).id);
                    }
                    Vector2 posCaster = xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(a);
                    Vector2 forward = ((Vector2)xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(target) - posCaster);
                    if (forward.sqrMagnitude < 0.0001f) forward = Vector2.right;
                    forward.Normalize();
                    int waveIdx = 4 - left; 
                    float mul = 2.5f;
                    if (waveIdx == 2) mul *= 1.1f;
                    if (waveIdx == 3) mul *= 1.2f;
                    int dmg = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(a)["damage"] * mul);
                    var tile = a.current_tile;
                    if (tile != null)
                    {
                        foreach (var v in Finder.getUnitsFromChunk(tile, 2, 3f))
                        {
                            if (v == null || !v.isAlive()) continue;
                            if (!IsEnemy(a, v)) continue;
                            Vector2 d = (Vector2)xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(v) - posCaster;
                            if (d.magnitude > 3f) continue;
                            if (Vector2.Dot(d.normalized, forward) <= 0f) continue; 
                            v.getHit(dmg, pFlash: true, AttackType.Other, a);
                            Vector2 vp = xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(v);
                            Vector2 dir = d.normalized;
                            Vector2 hit = vp + dir * 0.08f;
                            xn.access.ActorAccess.CalculateForce(v, vp.x, vp.y, hit.x, hit.y, 12f, 0f, true);
                        }
                    }
                    ShentongFX.PlayOnce_XS_Waves(a);
                    xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_LEFT, left - 1);
                    xn.access.ActorAccess.GetData(a).set(KEY_ART_WAVES_NEXT, now + 0.4f);
                }
            }
            private static Actor FindActorById(List<Actor> units, int id)
            {
                if (id == 0) return null;
                foreach (var u in units)
                {
                    if (u != null && u.isAlive() && (int)xn.access.ActorAccess.GetData(u).id == id)
                        return u;
                }
                return null;
            }
            private static Actor FindClosestEnemy(Actor a, List<Actor> units, float radius)
            {
                if (a == null || !a.isAlive()) return null;
                Actor best = null;
                float bestD = radius * radius;
                Vector2 pa = xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(a);
                foreach (var u in units)
                {
                    if (u == null || !u.isAlive()) continue;
                    if (!IsEnemy(a, u)) continue;
                    float d = ((Vector2)xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(u) - pa).sqrMagnitude;
                    if (d < bestD) { bestD = d; best = u; }
                }
                return best;
            }
            private static void CheckBaonuStates(List<Actor> units, float now)
            {
                foreach (var a in units)
                {
                    if (a == null || !a.isAlive()) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_BAONU_ACTIVE, out int on, 0);
                    if (on != 1) continue;
                    a.finishStatusEffect("stunned");
                    a.finishStatusEffect("freeze");
                    a.finishStatusEffect("slowness");
                    xn.access.ActorAccess.GetData(a).get(KEY_BAONU_END_TS, out float end, 0f);
                    if (end > 0f && now >= end)
                    {
                        xn.access.ActorAccess.GetData(a).get(KEY_BAONU_DMG_ADD, out int addDmg, 0);
                        if (addDmg > 0) xn.access.BaseSimObjectAccess.GetStats(a)["damage"] -= addDmg;
                        xn.access.ActorAccess.GetData(a).set(KEY_BAONU_ACTIVE, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_BAONU_END_TS, 0f);
                        xn.access.ActorAccess.GetData(a).set(KEY_BAONU_DMG_ADD, 0);
                        ShentongFX.StopLoop_Baonu(a);
                    }
                }
            }
            private static void CheckAscensionStates(List<Actor> units, float now)
            {
                foreach (var a in units)
                {
                    if (a == null || !a.isAlive()) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ASC_ACTIVE, out int on, 0);
                    if (on != 1) continue;
                    a.restoreHealthPercent(0.05f);
                    xn.access.ActorAccess.GetData(a).get(KEY_ASC_END_TS, out float end, 0f);
                    if (end > 0f && now >= end)
                    {
                        xn.access.ActorAccess.GetData(a).get(KEY_ASC_ADD_DMG, out int addDmg, 0);
                        xn.access.ActorAccess.GetData(a).get(KEY_ASC_ADD_ARMOR, out int addArmor, 0);
                        if (addDmg != 0) xn.access.BaseSimObjectAccess.GetStats(a)["damage"] -= addDmg;
                        if (addArmor != 0) xn.access.BaseSimObjectAccess.GetStats(a)["armor"] -= addArmor;
                        xn.access.ActorAccess.GetData(a).get(KEY_ASC_OLD_REALM, out string oldR, "");
                        xn.access.ActorAccess.GetData(a).get(KEY_ASC_TMP_REALM, out string tmpR, "");
                        if (!string.IsNullOrEmpty(tmpR) && a.hasTrait(tmpR)) a.removeTrait(tmpR);
                        if (!string.IsNullOrEmpty(oldR) && !a.hasTrait(oldR)) a.addTrait(oldR);
                        xn.access.ActorAccess.GetData(a).set(KEY_ASC_ACTIVE, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ASC_END_TS, 0f);
                        xn.access.ActorAccess.GetData(a).set(KEY_ASC_ADD_DMG, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ASC_ADD_ARMOR, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_ASC_OLD_REALM, "");
                        xn.access.ActorAccess.GetData(a).set(KEY_ASC_TMP_REALM, "");
                        ShentongFX.StopLoop_XS_Ascension(a);
                    }
                }
            }
            private static void CheckShieldStates(List<Actor> units, float now)
            {
                foreach (var a in units)
                {
                    if (a == null || !a.isAlive()) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_SHIELD_ACTIVE, out int on, 0);
                    if (on != 1) continue;
                    xn.access.ActorAccess.GetData(a).get(KEY_ART_SHIELD_NEXT_DRAIN, out float nextDrain, 0f);
                    if (now >= nextDrain)
                    {
                        xn.access.ActorAccess.GetData(a).get(KEY_YUANLI, out int yuanli, 0);
                        if (yuanli < COST_ART_SHIELD_PER_SEC)
                        {
                            EndShieldState(a);
                            continue;
                        }
                        xn.access.ActorAccess.GetData(a).set(KEY_YUANLI, yuanli - COST_ART_SHIELD_PER_SEC);
                        xn.access.ActorAccess.GetData(a).set(KEY_ART_SHIELD_NEXT_DRAIN, now + 1f);
                    }
                }
            }
        }
        private static class SoulLinkDamagePatch
        {
            private static bool s_inLinkDamage = false;
            public static void Prefix(Actor __instance, float pDamage, BaseSimObject pAttacker)
            {
                if (s_inLinkDamage) return;
                if (__instance == null || !__instance.isAlive()) return;
                if (pDamage <= 0) return;
                xn.access.ActorAccess.GetData(__instance).get(KEY_LINK_ON, out int linkOn, 0);
                if (linkOn != 1) return;
                xn.access.ActorAccess.GetData(__instance).get(KEY_LINK_TID, out int linkTid, 0);
                if (linkTid == 0) return;
                var units = MapBox.instance?.units?.getSimpleList();
                if (units == null) return;
                Actor linkTarget = null;
                foreach (var u in units)
                {
                    if (u != null && u.isAlive() && (int)xn.access.ActorAccess.GetData(u).id == linkTid)
                    {
                        linkTarget = u;
                        break;
                    }
                }
                if (linkTarget == null || !linkTarget.isAlive())
                {
                    xn.access.ActorAccess.GetData(__instance).set(KEY_LINK_ON, 0);
                    return;
                }
                s_inLinkDamage = true;
                try
                {
                    int transferDmg = Mathf.FloorToInt(pDamage);
                    if (transferDmg > 0)
                    {
                        linkTarget.getHit(transferDmg, pFlash: true, AttackType.Other, __instance);
                    }
                }
                finally
                {
                    s_inLinkDamage = false;
                }
            }
        }
        private static void RegisterWeiyaTarget(Actor target, float duration)
        {
            int id = (int)xn.access.ActorAccess.GetData(target).id;
            s_weiyaTargets.Add(id);
            xn.access.ActorAccess.GetData(target).set(KEY_WEIYA_END, Time.time + duration);
        }
    }
}
