using System;
using HarmonyLib;
using ai;
using ai.behaviours;
using UnityEngine;
using System.Collections.Generic;
using xn.bloodline;
namespace cultivation 
{
    internal static class StatsCombatPatches
    {
        static StatsCombatPatches()
        {
            TryRaiseArmorPercentCap();
        }
        private static void TryRaiseArmorPercentCap()
        {
            try
            {
                var lib = AssetManager.base_stats_library;
                if (lib == null) return;
                var armor = lib.get("armor");
                if (armor == null) return;
                armor.normalize = true;
                armor.show_as_percents = true;
                armor.normalize_max = 1000f;
            }
            catch {  }
        }
        private static Actor s_currentDelegateAttacker = null;
        private static bool s_debugDamage = false;   
        private const string LOGTAG = "[XN-DMG]";
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        private static void D(string msg)
        {
            if (!s_debugDamage) return;
            UnityEngine.Debug.Log(LOGTAG + " " + msg);
        }
        private const string KEY_TMP_DEF_PCT = "xn.intent.tmp_def_pct";
        private const string KEY_LD_ACTIVE = "xn.intent.life_death_active";
        private const string KEY_ACTIVE_KILLING = "xn.intent.active.intent_04_killing";
        private const string KEY_KILLING_LAYERS = "xn.intent.killing_layers";
        private const string KEY_ACTIVE_PREFIX = "xn.intent.active.";
        private const string KEY_LINGLI = "xn.stat.lingli";
        private const string KEY_QH_LAYERS = "xn.intent.qh_layers";
        private const string KEY_QH_UNTIL = "xn.intent.qh_until";
        private const string KEY_REVERSE_BOOST_UNTIL = "xn.intent.reverse_boost_until";
        private const string KEY_REBIRTH_CD_UNTIL_YEAR = "xn.intent.rebirth_cd_until_year";
        private const string KEY_REBIRTH_INVULN_UNTIL = "xn.intent.rebirth_invuln_until";
        private const string INTENT_EXTREME = "intent_01_extreme";
        private const string INTENT_ANGEL = "intent_02_angel";
        private const string INTENT_QIANHUAN = "intent_03_qianhuan";
        private const string INTENT_KILLING = "intent_04_killing";
        private const string INTENT_REVERSE = "intent_05_reverse";
        private const string INTENT_LIFE_DEATH = "intent_06_life_death";
        private const string INTENT_REINCARNATION = "intent_07_reincarnation";
        private const string INTENT_CHAOS = "intent_08_chaos";
        private const string INTENT_MADNESS = "intent_09_madness";
        private const string KEY_ART_SHIELD_ACTIVE = "xn.art.shield_on";         
        private const string KEY_ART_LINK_ACTIVE = "xn.art.link_on";           
        private const string KEY_ART_LINK_END = "xn.art.link_end";          
        private const string KEY_ART_LINK_TID = "xn.art.link_tid";          
        private const string KEY_BY_METAL = "xn.benyuan.metal_on";
        private const string KEY_BY_WOOD = "xn.benyuan.wood_on";
        private const string KEY_NOHEAL_END = "xn.benyuan.noheal_end";
        private const string KEY_BY_WATER = "xn.benyuan.water_on";
        private const string KEY_BY_FIRE = "xn.benyuan.fire_on";
        private const string KEY_BY_EARTH = "xn.benyuan.earth_on";
        private const string ATTR_METAL = "attr_01_metal";
        private const string ATTR_WOOD = "attr_02_wood";
        private const string ATTR_WATER = "attr_03_water";
        private const string ATTR_FIRE = "attr_04_fire";
        private const string ATTR_EARTH = "attr_05_earth";
        private const string KEY_FIRE_DOT_STACKS = "xn.benyuan.fire_dot_stacks";
        private const string KEY_FIRE_DOT_UNTIL = "xn.benyuan.fire_dot_until";
        private const int COST_EVENT_EXTREME = 10;    
        private const int COST_EVENT_REBIRTH = 1500;  
        private const int MAX_SAFE_DAMAGE = 2_100_000_000;
        private const int MAX_MP_SP_REFLECT = 1_800_000_000;
        private static readonly string[] REALM_IDS = new[]{
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        private static readonly string[] ANC_STAR_IDS = {
            "ancient_01_star","ancient_02_star","ancient_03_star","ancient_04_star","ancient_05_star",
            "ancient_06_star","ancient_07_star","ancient_08_star","ancient_09_star","ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = {
            "beast_01_stage","beast_02_stage","beast_03_stage","beast_04_stage","beast_05_stage",
            "beast_06_stage","beast_07_stage","beast_08_stage","beast_09_stage","beast_10_stage"
        };
        private static int GetAncIndex(Actor a) { if (a == null) return -1; int idx = -1; for (int i = 0; i < ANC_STAR_IDS.Length; i++) if (a.hasTrait(ANC_STAR_IDS[i])) idx = i; return idx; }
        private static int GetBeastIndex(Actor a) { if (a == null) return -1; int idx = -1; for (int i = 0; i < BEAST_STAGE_IDS.Length; i++) if (a.hasTrait(BEAST_STAGE_IDS[i])) idx = i; return idx; }
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
        private static bool IsAncient(Actor a)
        {
            if (a == null) return false;
            var traits = a.getTraits();
            if (traits == null) return false;
            foreach (var t in traits)
            {
                if (t != null && t.id != null && t.id.StartsWith("ancient_"))
                    return true;
            }
            return false;
        }
        private static bool IsBeast(Actor a)
        {
            if (a == null) return false;
            var traits = a.getTraits();
            if (traits == null) return false;
            foreach (var t in traits)
            {
                if (t != null && t.id != null && (t.id.StartsWith("beast_") || t.id == "path_03_beast"))
                    return true;
            }
            return false;
        }
        private static bool s_annualPatched = false;
        private static int s_lastAppliedYear = -1;
        private static bool IsIntentActive(Actor a, string id)
        {
            if (a == null) return false;
            if (!a.hasTrait(id)) return false; 
            int v; xn.access.ActorAccess.GetData(a).get(KEY_ACTIVE_PREFIX + id, out v, 0);
            return v == 1;
        }
    private static bool SpendLingli(Actor a, int amount)
        { if (a == null || amount <= 0) return true; int cur; xn.access.ActorAccess.GetData(a).get(KEY_LINGLI, out cur, 0); if (cur < amount) return false; xn.access.ActorAccess.GetData(a).set(KEY_LINGLI, cur - amount); return true; }
        private static int NowTs() => (int)World.world.getCurWorldTime();
        private static int CurYear() => Date.getCurrentYear();
        private static int GetRealmIndex(Actor a)
        { if (a == null) return -1; int idx = -1; for (int i = 0; i < REALM_IDS.Length; i++) if (a.hasTrait(REALM_IDS[i])) idx = i; return idx; }
        private static void DoAnnualRegen(Actor a, int years)
        {
            if (a == null || !a.isAlive()) return;
            if (years <= 0) years = 1;
            float perYear = xn.access.BaseSimObjectAccess.GetStats(a)["Healback"];
            if (perYear > 0f)
            {
                int heal = (int)(perYear * years);
                if (heal > 0) a.changeHealth(heal);
            }
            AddResourceAnnual(a, years, "xn.stat.lingli", "LingliMax", "LingliRegenPerYear");
            AddResourceAnnual(a, years, "xn.stat.yuanli", "YuanliMax", "YuanliRegenPerYear");
            AddResourceAnnual(a, years, "xn.stat.nieli", "NieliMax", "NieliRegenPerYear");
        }
        private static void AddResourceAnnual(Actor a, int years, string dataKey, string maxStatKey, string regenStatKey)
        {
            if (a == null || !a.isAlive()) return;
            if (years <= 0) years = 1;
            int add = (int)(xn.access.BaseSimObjectAccess.GetStats(a)[regenStatKey] * years);
            if (add <= 0) return;
            int cap = (int)xn.access.BaseSimObjectAccess.GetStats(a)[maxStatKey];
            int cur; xn.access.ActorAccess.GetData(a).get(dataKey, out cur, 0);
            long next = (long)cur + (long)add;
            if (cap > 0 && next > cap) next = cap;
            xn.access.ActorAccess.GetData(a).set(dataKey, (int)next);
        }
        [HarmonyPatch(typeof(Actor), "getHit", new System.Type[] { typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool) })]
        private static class GetHitPatch
        {
            [HarmonyPrefix]
            [HarmonyPriority(int.MaxValue)] 
            private static bool Prefix(Actor __instance, ref float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker, bool pSkipIfShake, bool pMetallicWeapon, bool pCheckDamageReduction)
            {
                if (__instance == null) return true;
                if (!__instance.hasHealth()) return false;
                Actor attacker = null;
                if (pAttacker != null && xn.access.BaseSimObjectAccess.IsActor(pAttacker))
                {
                    attacker = xn.access.BaseSimObjectAccess.GetActor(pAttacker);
                }
                Actor realmCheckAttacker = attacker ?? s_currentDelegateAttacker;
                AttackType atkType = pAttackType;
                if (attacker != null) xn.access.ActorAccess.SetAttackedBy(__instance, attacker);
                xn.access.ActorAccess.SetLastAttackType(__instance, atkType);
                float atkDmg = attacker != null ? xn.access.BaseSimObjectAccess.GetStats(attacker)["damage"] : 0f;
                float defArmor0 = xn.access.BaseSimObjectAccess.GetStats(__instance)["armor"];
                D($"Enter atk={(attacker != null ? attacker.getName() : "<null>")}#{(attacker != null ? (int)xn.access.ActorAccess.GetData(attacker).id : 0)} " +
                  $"-> def={__instance.getName()}#{(int)xn.access.ActorAccess.GetData(__instance).id} base={pDamage:F2} atkDmg={atkDmg:F1} defArmor={defArmor0:F1}");
                int onMetal = 0, onWood = 0;
                if (attacker != null) {
                    xn.access.ActorAccess.GetData(attacker).get(KEY_BY_METAL, out onMetal, 0);
                    xn.access.ActorAccess.GetData(attacker).get(KEY_BY_WOOD,  out onWood,  0);
                }
                float originalDamage = pDamage;
                {
                    int invuln; xn.access.ActorAccess.GetData(__instance).get(KEY_REBIRTH_INVULN_UNTIL, out invuln, 0);
                    if (invuln > 0 && NowTs() < invuln) { D("Invuln(life-guard) SKIP"); return false; }
                }
                if (attacker != null && __instance.isAlive())
                {
                    float acc = xn.access.BaseSimObjectAccess.GetStats(attacker)["Accuracy"];
                    if (IsIntentActive(attacker, INTENT_QIANHUAN)) acc += 15f;  
                    float dodge = xn.access.BaseSimObjectAccess.GetStats(__instance)["Dodge"];
                    int defWater; xn.access.ActorAccess.GetData(__instance).get(KEY_BY_WATER, out defWater, 0);
                    if (defWater == 1 && __instance.hasTrait(ATTR_WATER)) dodge += 15f;
                    float missChance = Mathf.Clamp(dodge - acc, 0f, 100f) / 100f;
                    if (Randy.randomChance(missChance))
                    {
                        __instance.startColorEffect(ActorColorEffect.White);
                        D($"MISS acc={acc:F1} dodge={dodge:F1} miss={missChance:P0}");
                        return false; 
                    }
                    else { D($"Hit acc={acc:F1} dodge={dodge:F1} miss={missChance:P0}"); }
                }
                {
                    float armorPen = attacker != null ? xn.access.BaseSimObjectAccess.GetStats(attacker)["ArmorPenPercent"] : 0f;
                    float effArmor = Mathf.Max(xn.access.BaseSimObjectAccess.GetStats(__instance)["armor"] - armorPen, 0f);
                    int defEarth; xn.access.ActorAccess.GetData(__instance).get(KEY_BY_EARTH, out defEarth, 0);
                    if (defEarth == 1 && __instance.hasTrait(ATTR_EARTH)) effArmor += 30f;
                    if (onMetal == 1) 
                    {
                        effArmor = 0f;        
                        pDamage *= 1.20f;     
                    }
                    if (defEarth == 1 && __instance.hasTrait(ATTR_EARTH)) effArmor *= 1.5f;
                    if (attacker != null && BloodlineSystem.HasBloodline(attacker))
                    {
                        string atkBloodline = BloodlineSystem.GetBloodlineType(attacker);
                        if (atkBloodline == BloodlineTypes.HOUYI)
                        {
                            float atkConc = BloodlineSystem.GetConcentration(attacker);
                            if (atkConc >= 50f)
                            {
                                int atkRealm = GetUnifiedRealmIndex(attacker);
                                int defRealm = GetUnifiedRealmIndex(__instance);
                                if (atkRealm >= 0 && defRealm >= 0 && defRealm <= atkRealm)
                                {
                                    effArmor *= 0.5f; 
                                    D($"Houyi armor pen -50% -> effArmor={effArmor:F1}");
                                }
                            }
                        }
                    }
                    float factor = 100f / (100f + effArmor);
                    if (factor < 0.001f) factor = 0.001f;
                    pDamage *= factor;
                    D($"Armor effArmor={effArmor:F1} factor={factor:F2} -> {pDamage:F2}");
                }
                if (attacker != null)
                {
                    if (onMetal == 1 && attacker.hasTrait(ATTR_METAL) && __instance.hasTrait(ATTR_WOOD)) pDamage *= 1.5f;
                    if (onWood  == 1 && attacker.hasTrait(ATTR_WOOD)  && __instance.hasTrait(ATTR_EARTH)) pDamage *= 1.5f;
                    int onWater; xn.access.ActorAccess.GetData(attacker).get(KEY_BY_WATER, out onWater, 0);
                    int onFire;  xn.access.ActorAccess.GetData(attacker).get(KEY_BY_FIRE,  out onFire,  0);
                    int onEarth; xn.access.ActorAccess.GetData(attacker).get(KEY_BY_EARTH, out onEarth, 0);
                    if (onWater == 1 && attacker.hasTrait(ATTR_WATER) && __instance.hasTrait(ATTR_FIRE))  pDamage *= 1.5f;
                    if (onFire  == 1 && attacker.hasTrait(ATTR_FIRE)  && __instance.hasTrait(ATTR_METAL)) pDamage *= 1.5f;
                    if (onEarth == 1 && attacker.hasTrait(ATTR_EARTH) && __instance.hasTrait(ATTR_WATER)) pDamage *= 1.5f;
                    D($"Elements onM={onMetal} onW={onWood} onWa={onWater} onF={onFire} onE={onEarth} -> {pDamage:F2}");
                }
                {
                    int onShield; xn.access.ActorAccess.GetData(__instance).get(KEY_ART_SHIELD_ACTIVE, out onShield, 0);
                    if (onShield == 1)
                    {
                        pDamage *= 0.6f; 
                        if (attacker != null && attacker.isAlive())
                        {
                            int reflect = Mathf.FloorToInt(xn.access.BaseSimObjectAccess.GetStats(__instance)["damage"] * 0.8f);
                            if (reflect > MAX_SAFE_DAMAGE) reflect = MAX_SAFE_DAMAGE;
                            if (reflect > 0) attacker.changeHealth(-reflect);
                            if (!attacker.hasHealth()) attacker.batch.c_check_deaths.Add(attacker);
                        }
                        D($"ArtShield on=1 -> {pDamage:F2}");
                    }
                }
                {
                    int linkOn; xn.access.ActorAccess.GetData(__instance).get(KEY_ART_LINK_ACTIVE, out linkOn, 0);
                    if (linkOn == 1)
                    {
                        float end; xn.access.ActorAccess.GetData(__instance).get(KEY_ART_LINK_END, out end, 0f);
                        int tid;   xn.access.ActorAccess.GetData(__instance).get(KEY_ART_LINK_TID, out tid, 0);
                        if (end > 0f && Time.time < end && tid != 0)
                        {
                            var tile = __instance.current_tile;
                            if (tile != null)
                            {
                                Actor partner = null;
                                foreach (var u in Finder.getUnitsFromChunk(tile, 2, 8f))
                                { if (u != null && u.isAlive() && (int)xn.access.ActorAccess.GetData(u).id == tid) { partner = u; break; } }
                                if (partner != null && partner.isAlive())
                                {
                                    int copy = Mathf.CeilToInt(pDamage);
                                    if (copy > 0) partner.changeHealth(-copy);
                                    if (!partner.hasHealth()) partner.batch.c_check_deaths.Add(partner);
                                    D($"Link copy={copy} to {partner.getName()}#{(int)xn.access.ActorAccess.GetData(partner).id}");
                                }
                                else
                                {
                                    xn.access.ActorAccess.GetData(__instance).set(KEY_ART_LINK_ACTIVE, 0);
                                    xn.access.ActorAccess.GetData(__instance).set(KEY_ART_LINK_TID, 0);
                                    xn.access.ActorAccess.GetData(__instance).set(KEY_ART_LINK_END, 0f);
                                    D("Link end (partner lost)");
                                }
                            }
                        }
                        else
                        {
                            xn.access.ActorAccess.GetData(__instance).set(KEY_ART_LINK_ACTIVE, 0);
                            xn.access.ActorAccess.GetData(__instance).set(KEY_ART_LINK_TID, 0);
                            xn.access.ActorAccess.GetData(__instance).set(KEY_ART_LINK_END, 0f);
                            D("Link end (timeout)");
                        }
                    }
                }
                if (realmCheckAttacker != null)
                {
                    int attackerRealm = GetUnifiedRealmIndex(realmCheckAttacker);
                    int defenderRealm = GetUnifiedRealmIndex(__instance);
                    if (attackerRealm < 0 && defenderRealm >= 0)
                    {
                        pDamage = 0f;
                        D($"NoRealm -> HasRealm: damage=0");
                        if (realmCheckAttacker != null && realmCheckAttacker.isAlive())
                        {
                            int realmStages = defenderRealm + 1; 
                            float reflectMult = Mathf.Pow(2, realmStages);
                            int reflectDmg = Mathf.FloorToInt(originalDamage * reflectMult);
                            if (reflectDmg > MAX_SAFE_DAMAGE) reflectDmg = MAX_SAFE_DAMAGE;
                            if (reflectDmg > 0)
                            {
                                realmCheckAttacker.changeHealth(-reflectDmg);
                                long mpSpMultL = (realmStages >= 3) ? (long)Math.Pow(10, realmStages - 3) : 1L;
                                long mpSpCostL = (long)reflectDmg * mpSpMultL;
                                int mpSpCost = (mpSpCostL > MAX_MP_SP_REFLECT) ? MAX_MP_SP_REFLECT : (int)mpSpCostL;
                                realmCheckAttacker.changeMana(-mpSpCost);
                                realmCheckAttacker.changeStamina(-mpSpCost);
                                realmCheckAttacker.startColorEffect(ActorColorEffect.Red);
                                D($"RealmReflect: NoRealm attacker takes {reflectDmg} HP, {mpSpCost} MP/SP (stages={realmStages}, hpMult={reflectMult}x, mpMult={mpSpMultL}x)");
                                if (!realmCheckAttacker.hasHealth()) realmCheckAttacker.batch.c_check_deaths.Add(realmCheckAttacker);
                            }
                        }
                        return false; 
                    }
                    else if (attackerRealm >= 0 && defenderRealm >= 0)
                    {
                        int diff = attackerRealm - defenderRealm;
                        if (diff < 0)
                        {
                            int stages = -diff; 
                            if (stages >= 3)
                            {
                                pDamage = 0f;
                                D($"RealmDiff A({attackerRealm}) < D({defenderRealm}), diff={stages}, immune (damage=0)");
                                if (realmCheckAttacker != null && realmCheckAttacker.isAlive())
                                {
                                    float reflectMult = Mathf.Pow(2, stages);
                                    int reflectDmg = Mathf.FloorToInt(originalDamage * reflectMult);
                                    if (reflectDmg > MAX_SAFE_DAMAGE) reflectDmg = MAX_SAFE_DAMAGE;
                                    if (reflectDmg > 0)
                                    {
                                        realmCheckAttacker.changeHealth(-reflectDmg);
                                        long mpSpMultL = (stages >= 3) ? (long)Math.Pow(10, stages - 3) : 1L;
                                        long mpSpCostL = (long)reflectDmg * mpSpMultL;
                                        int mpSpCost = (mpSpCostL > MAX_MP_SP_REFLECT) ? MAX_MP_SP_REFLECT : (int)mpSpCostL;
                                        realmCheckAttacker.changeMana(-mpSpCost);
                                        realmCheckAttacker.changeStamina(-mpSpCost);
                                        realmCheckAttacker.startColorEffect(ActorColorEffect.Red);
                                        D($"RealmReflect: A({attackerRealm}) takes {reflectDmg} HP, {mpSpCost} MP/SP from D({defenderRealm}) (stages={stages}, hpMult={reflectMult}x, mpMult={mpSpMultL}x)");
                                        if (!realmCheckAttacker.hasHealth()) realmCheckAttacker.batch.c_check_deaths.Add(realmCheckAttacker);
                                    }
                                }
                                return false; 
                            }
                            else
                            {
                                float multiplier = 1.0f;
                                for (int i = 0; i < stages; i++)
                                {
                                    multiplier *= 0.5f; 
                                }
                                pDamage *= multiplier;
                                D($"RealmDiff A({attackerRealm}) < D({defenderRealm}), diff={stages}, mult={multiplier:F3} -> {pDamage:F2}");
                            }
                        }
                        else if (diff > 0)
                        {
                            int stages = diff; 
                            float multiplier = 1.0f;
                            for (int i = 0; i < stages; i++)
                            {
                                multiplier *= 1.3f; 
                            }
                            pDamage *= multiplier;
                            D($"RealmDiff A({attackerRealm}) > D({defenderRealm}), diff={stages}, mult={multiplier:F3} -> {pDamage:F2}");
                            float trueChance = stages >= 3 ? 1f : (stages == 2 ? 0.6f : (stages == 1 ? 0.2f : 0.01f));
                            if (Randy.randomChance(trueChance))
                            {
                                int trueDmg = Mathf.FloorToInt(__instance.getMaxHealth() * 0.2f);
                                if (trueDmg > 0)
                                {
                                    __instance.changeHealth(-trueDmg);
                                    D($"RealmTrueDmg chance={trueChance:P0} dealt={trueDmg}");
                                }
                            }
                        }
                        else if (diff == 0)
                        {
                            if (Randy.randomChance(0.01f))
                            {
                                int trueDmg = Mathf.FloorToInt(__instance.getMaxHealth() * 0.2f);
                                if (trueDmg > 0)
                                {
                                    __instance.changeHealth(-trueDmg);
                                    D($"RealmTrueDmg(same) dealt={trueDmg}");
                                }
                            }
                        }
                    }
                    else if (attackerRealm == 15 && defenderRealm < 0) 
                    {
                        if (Randy.randomChance(0.00001f))
                        {
                            if (attacker != null) xn.access.ActorAccess.GetData(attacker).kills++;
                            string attackerName = attacker?.getName() ?? T("broadcast_heavenly_tribulation_unknown_tatian", "Heaven Trampling cultivator");
                            xn.world.BroadcastSystem.PostActor(attacker, T("broadcast_heavenly_tribulation_purge_beast", "{0} used heavenly tribulation power to purge the beast", attackerName));
                            World.world.units.destroyObject(__instance);
                            D($"Tatian instant kill: NoRealm target erased from world (0.1% chance)");
                            return false; 
                        }
                        int trueDmg = Mathf.FloorToInt(__instance.getMaxHealth() * 0.2f);
                        if (trueDmg > 0)
                        {
                            __instance.changeHealth(-trueDmg);
                            D($"RealmTrueDmg(Tatian->NoRealm) dealt={trueDmg}");
                        }
                    }
                    else if (attackerRealm >= 0 && defenderRealm < 0)
                    {
                        int trueDmg = Mathf.FloorToInt(__instance.getMaxHealth() * 0.2f);
                        if (trueDmg > 0)
                        {
                            __instance.changeHealth(-trueDmg);
                            D($"RealmTrueDmg(HasRealm->NoRealm) dealt={trueDmg}");
                        }
                    }
                }
                if (attacker != null && IsIntentActive(attacker, INTENT_KILLING))
                    pDamage *= 1.10f;
                if (attacker != null && IsIntentActive(attacker, INTENT_KILLING)) D($"Killing +10% -> {pDamage:F2}");
                if (attacker != null)
                {
                    int until; xn.access.ActorAccess.GetData(attacker).get(KEY_QH_UNTIL, out until, 0);
                    if (until > 0 && NowTs() > until) { xn.access.ActorAccess.GetData(attacker).set(KEY_QH_LAYERS, 0); xn.access.ActorAccess.GetData(attacker).set(KEY_QH_UNTIL, 0); }
                    int layers; xn.access.ActorAccess.GetData(attacker).get(KEY_QH_LAYERS, out layers, 0);
                    if (layers > 0) pDamage *= (1f + 0.05f * Mathf.Min(layers, 5));
                    if (layers > 0) D($"Qianhuan layers={layers} -> {pDamage:F2}");
                }
                if (attacker != null && attacker.isAlive())
                {
                    if (onMetal == 1)
                    {
                        xn.access.ActorAccess.GetData(__instance).set(KEY_NOHEAL_END, Time.time + 3f);
                        D("Metal hit: no-heal 3s");
                    }
                    if (onWood == 1 && attacker.hasTrait(ATTR_WOOD))
                    {
                        int targetMetal; xn.access.ActorAccess.GetData(__instance).get(KEY_BY_METAL, out targetMetal, 0);
                        if (targetMetal != 1) 
                        {
                            if (Randy.randomChance(0.30f)) __instance.makeStunned(5);
                            D("Wood hit: try stun 5s (30%)");
                        }
                    }
                }
                if (attacker != null)
                {
                    int until; xn.access.ActorAccess.GetData(attacker).get(KEY_REVERSE_BOOST_UNTIL, out until, 0);
                    if (until > NowTs()) pDamage *= 1.20f;
                    if (until > NowTs()) D($"Reverse buff +20% -> {pDamage:F2}");
                }
                if (attacker != null && IsIntentActive(attacker, INTENT_CHAOS) && Randy.randomChance(0.15f))
                {
                    if (Randy.randomBool()) pDamage *= 3f; 
                    else attacker.changeHealth(Mathf.FloorToInt(attacker.getMaxHealth() * 0.10f)); 
                    D($"Chaos roll -> {pDamage:F2} (or self-heal)");
                }
                {
                    if (IsIntentActive(__instance, INTENT_ANGEL))
                    {
                        float defPct = 0f;
                        xn.access.ActorAccess.GetData(__instance).get(KEY_TMP_DEF_PCT, out defPct, 0f);
                        if (defPct > 0f)
                        {
                            defPct = Mathf.Clamp(defPct, 0f, 0.8f); 
                            pDamage *= (1f - defPct);
                            D($"Angel guard -{defPct:P0} -> {pDamage:F2}");
                        }
                    }
                }
                {
                    int ldOn; xn.access.ActorAccess.GetData(__instance).get(KEY_LD_ACTIVE, out ldOn, 0);
                    if (ldOn == 1 && __instance.hasTrait(INTENT_LIFE_DEATH))
                        pDamage *= 0.6f;
                    if (ldOn == 1 && __instance.hasTrait(INTENT_LIFE_DEATH)) D($"LifeDeath -40% -> {pDamage:F2}");
                }
                if (attacker != null)
                {
                    bool attackerInCombat = xn.access.ActorAccess.HasAttackTarget(attacker);
                    if (!attackerInCombat)
                    {
                        var task = xn.access.AiSystemAccess.GetTask(xn.access.ActorAccess.GetAI(attacker));
                        if (task != null)
                        {
                            attackerInCombat = task.in_combat || task.id == "fighting";
                        }
                    }
                    if (attacker.hasTrait(INTENT_EXTREME))
                    {
                        bool attackerIsCultivator = !IsAncient(attacker) && !IsBeast(attacker);
                        if (attackerIsCultivator && attackerInCombat)
                        {
                            int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_EXTREME, out active, 0);
                            if (active == 0)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_EXTREME, 1);
                            }
                        }
                        else if (!attackerInCombat)
                        {
                            xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_EXTREME, 0);
                        }
                    }
                    if (!attackerInCombat)
                    {
                        if (attacker.hasTrait(INTENT_QIANHUAN))
                        {
                            int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_QIANHUAN, out active, 0);
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_QIANHUAN, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                        if (attacker.hasTrait(INTENT_KILLING))
                        {
                            int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_KILLING, out active, 0);
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_KILLING, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                        if (attacker.hasTrait(INTENT_REVERSE))
                        {
                            int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_REVERSE, out active, 0);
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_REVERSE, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                        if (attacker.hasTrait(INTENT_CHAOS))
                        {
                            int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_CHAOS, out active, 0);
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_CHAOS, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                        if (attacker.hasTrait(INTENT_MADNESS))
                        {
                            int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_MADNESS, out active, 0);
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_MADNESS, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                        goto SkipIntentActivation;
                    }
                    if (attacker.hasTrait(INTENT_QIANHUAN))
                    {
                        int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_QIANHUAN, out active, 0);
                        if (SpendLingli(attacker, 60)) 
                        {
                            if (active == 0)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_QIANHUAN, 1);
                                YijingFX.StartLoop(attacker);
                            }
                            else
                            {
                                YijingFX.StartLoop(attacker);
                            }
                        }
                        else
                        {
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_QIANHUAN, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                    }
                    if (attacker.hasTrait(INTENT_KILLING))
                    {
                        int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_KILLING, out active, 0);
                        if (SpendLingli(attacker, 50)) 
                        {
                            if (active == 0)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_KILLING, 1);
                                YijingFX.StartLoop(attacker);
                            }
                            else
                            {
                                YijingFX.StartLoop(attacker);
                            }
                        }
                        else
                        {
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_KILLING, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                    }
                    if (attacker.hasTrait(INTENT_REVERSE))
                    {
                        int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_REVERSE, out active, 0);
                        if (SpendLingli(attacker, 70)) 
                        {
                            if (active == 0)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_REVERSE, 1);
                                YijingFX.StartLoop(attacker);
                            }
                            else
                            {
                                YijingFX.StartLoop(attacker);
                            }
                        }
                        else
                        {
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_REVERSE, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                    }
                    if (attacker.hasTrait(INTENT_CHAOS))
                    {
                        int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_CHAOS, out active, 0);
                        if (SpendLingli(attacker, 100)) 
                        {
                            if (active == 0)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_CHAOS, 1);
                                YijingFX.StartLoop(attacker);
                            }
                            else
                            {
                                YijingFX.StartLoop(attacker);
                            }
                        }
                        else
                        {
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_CHAOS, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                    }
                    if (attacker.hasTrait(INTENT_MADNESS))
                    {
                        int active; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_PREFIX + INTENT_MADNESS, out active, 0);
                        if (SpendLingli(attacker, 50)) 
                        {
                            if (active == 0)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_MADNESS, 1);
                                YijingFX.StartLoop(attacker);
                            }
                            else
                            {
                                YijingFX.StartLoop(attacker);
                            }
                        }
                        else
                        {
                            if (active == 1)
                            {
                                xn.access.ActorAccess.GetData(attacker).set(KEY_ACTIVE_PREFIX + INTENT_MADNESS, 0);
                                YijingFX.StopLoop(attacker);
                            }
                        }
                    }
                }
                SkipIntentActivation: 
                bool extremeExecuted = false;
                if (attacker != null && IsIntentActive(attacker, INTENT_EXTREME))
                {
                    bool attackerIsCultivator = !IsAncient(attacker) && !IsBeast(attacker);
                    bool targetIsCultivator = !IsAncient(__instance) && !IsBeast(__instance);
                    if (attackerIsCultivator && targetIsCultivator)
                    {
                        int ar = GetRealmIndex(attacker);
                        int dr = GetRealmIndex(__instance);
                        if (ar >= 0 && dr >= 0 && dr <= ar)
                        {
                            if (SpendLingli(attacker, COST_EVENT_EXTREME))
                            {
                                extremeExecuted = true; 
                                YijingFX.PlayExtremeOnce(__instance); 
                            }
                        }
                    }
                }
                if (__instance != null && __instance.isAlive() && __instance.hasTrait(INTENT_REINCARNATION) && !extremeExecuted)
                {
                    int cd; xn.access.ActorAccess.GetData(__instance).get(KEY_REBIRTH_CD_UNTIL_YEAR, out cd, 0);
                    if (CurYear() >= cd)
                    {
                        float hp = __instance.getHealth();
                        if (hp - pDamage <= 0f && SpendLingli(__instance, COST_EVENT_REBIRTH))
                        {
                            int heal = Mathf.FloorToInt(__instance.getMaxHealth() * 0.5f);
                            if (heal > 0) __instance.changeHealth(heal);
                            pDamage = Mathf.Max(0f, hp - 1f);
                            xn.access.ActorAccess.GetData(__instance).set(KEY_REBIRTH_INVULN_UNTIL, NowTs() + 20);
                            xn.access.ActorAccess.GetData(__instance).set(KEY_REBIRTH_CD_UNTIL_YEAR, CurYear() + 300);
                            D("Rebirth: save to 1 HP");
                        }
                    }
                }
                {
                    int defWater; xn.access.ActorAccess.GetData(__instance).get(KEY_BY_WATER, out defWater, 0);
                    if (defWater == 1 && __instance.hasTrait(ATTR_WATER))
                    {
                        pDamage *= 0.6f; 
                        __instance.finishStatusEffect("burning");
                        D($"Water DEF -40% -> {pDamage:F2} (clear burning)");
                    }
                }
                {
                    int defFire; xn.access.ActorAccess.GetData(__instance).get(KEY_BY_FIRE, out defFire, 0);
                    if (defFire == 1 && __instance.hasTrait(ATTR_FIRE) && attacker != null && attacker.hasTrait(ATTR_METAL))
                    {
                        int atkMetal; xn.access.ActorAccess.GetData(attacker).get(KEY_BY_METAL, out atkMetal, 0);
                        if (atkMetal == 1) pDamage *= 0.5f;
                        if (defFire == 1 && attacker != null && attacker.hasTrait(ATTR_METAL)) D($"Fire DEF vs Metal ATK -50% -> {pDamage:F2}");
                    }
                }
                {
                    bool isProjectileAttack = pAttackType == AttackType.Weapon ||
                                              pAttackType == AttackType.Explosion;
                    if (isProjectileAttack)
                    {
                        pDamage = BloodlineEffects.ApplyJinfaProjectileDamageReduction(__instance, pDamage, pAttackType, true);
                        D($"Jinfa projectile reduction -> {pDamage:F2}");
                    }
                }
                {
                    pDamage = BloodlineEffects.ApplyGutiDamageReduction(__instance, pDamage);
                    D($"Guti damage reduction -> {pDamage:F2}");
                }
                {
                    pDamage = BloodlineEffects.ApplyGutiManaShield(__instance, pDamage);
                    D($"Guti mana shield -> {pDamage:F2}");
                }
                if (extremeExecuted)
                {
                    pDamage = __instance.getHealth(); 
                    D("Extreme: EXECUTE (after all reductions)");
                }
                if (pDamage > MAX_SAFE_DAMAGE) pDamage = MAX_SAFE_DAMAGE;
                int dealt = Mathf.FloorToInt(pDamage);
                if (dealt <= 0 && pDamage > 0f) dealt = 1; 
                if (dealt > 0)
                    __instance.changeHealth(-dealt);
                D($"DEALT {dealt}  -> defHP={__instance.getHealth():F0}");
                if (attacker != null && attacker.isAlive())
                {
                    int atkWater; xn.access.ActorAccess.GetData(attacker).get(KEY_BY_WATER, out atkWater, 0);
                    int atkFire;  xn.access.ActorAccess.GetData(attacker).get(KEY_BY_FIRE,  out atkFire,  0);
                    int atkEarth; xn.access.ActorAccess.GetData(attacker).get(KEY_BY_EARTH, out atkEarth, 0);
                    if (atkWater == 1)
                    {
                        xn.access.BaseSimObjectAccess.AddStatusEffect(__instance, "slowness", 3f);
                        D("Water hit: slowness 3s");
                    }
                    if (atkFire == 1)
                    {
                        xn.access.BaseSimObjectAccess.AddStatusEffect(__instance, "burning", 5f);
                        int stacks; xn.access.ActorAccess.GetData(__instance).get(KEY_FIRE_DOT_STACKS, out stacks, 0);
                        xn.access.ActorAccess.GetData(__instance).set(KEY_FIRE_DOT_STACKS, stacks + 1);
                        xn.access.ActorAccess.GetData(__instance).set(KEY_FIRE_DOT_UNTIL, Time.time + 5f);
                        D($"Fire hit: burning + dot stacks={(stacks + 1)}");
                    }
                    if (atkEarth == 1)
                    {
                        if (Randy.randomChance(0.25f)) __instance.makeStunned(1);
                        D("Earth hit: stun 1s (25%) try");
                    }
                }
                {
                    int defEarth2; xn.access.ActorAccess.GetData(__instance).get(KEY_BY_EARTH, out defEarth2, 0);
                    if (defEarth2 == 1 && __instance.hasTrait(ATTR_EARTH) && attacker != null && attacker.isAlive())
                    {
                        int rebound = Mathf.CeilToInt(dealt * 0.3f);
                        if (rebound > MAX_SAFE_DAMAGE) rebound = MAX_SAFE_DAMAGE;
                        if (rebound > 0) attacker.changeHealth(-rebound);
                        if (!attacker.hasHealth()) attacker.batch.c_check_deaths.Add(attacker);
                        D($"Earth reflect {rebound}");
                    }
                }
                if (attacker != null && !__instance.hasHealth())
                {
                    int on; xn.access.ActorAccess.GetData(attacker).get(KEY_ACTIVE_KILLING, out on, 0);
                    if (on == 1)
                    {
                        int layers; xn.access.ActorAccess.GetData(attacker).get(KEY_KILLING_LAYERS, out layers, 0);
                        if (layers < 20) xn.access.ActorAccess.GetData(attacker).set(KEY_KILLING_LAYERS, layers + 1);
                        D($"Killing add layer -> {(on == 1 ? (int)xn.access.ActorAccess.GetData(attacker).id : 0)} layers+1");
                    }
                }
                if (attacker != null && IsIntentActive(attacker, INTENT_QIANHUAN))
                {
                    int layers; xn.access.ActorAccess.GetData(attacker).get(KEY_QH_LAYERS, out layers, 0);
                    if (layers < 5) layers++;
                    xn.access.ActorAccess.GetData(attacker).set(KEY_QH_LAYERS, layers);
                    xn.access.ActorAccess.GetData(attacker).set(KEY_QH_UNTIL, NowTs() + 5);
                }
                if (attacker != null && attacker.isAlive() && dealt > 0)
                {
                    int woodOn = 0; xn.access.ActorAccess.GetData(attacker).get(KEY_BY_WOOD, out woodOn, 0);
                    if (woodOn == 1 && attacker.hasTrait(ATTR_WOOD))
                    {
                        int healWood = Mathf.CeilToInt(dealt * 0.5f);
                        if (healWood > 0) attacker.changeHealth(healWood);
                        D($"Wood leech heal={healWood} (based on dealt={dealt})");
                    }
                    float vamp = xn.access.BaseSimObjectAccess.GetStats(attacker)["Vampire"]; 
                    if (vamp > 0f)
                    {
                        int heal = Mathf.FloorToInt(dealt * vamp / 100f);
                        if (heal > 0) attacker.changeHealth(heal);
                    }
                    int ldOnAtk; xn.access.ActorAccess.GetData(attacker).get(KEY_LD_ACTIVE, out ldOnAtk, 0);
                    if (ldOnAtk == 1)
                    {
                        int heal2 = Mathf.FloorToInt(dealt * 0.30f);
                        if (heal2 > 0) attacker.changeHealth(heal2);
                    }
                }
                __instance.startColorEffect(ActorColorEffect.Red);
                xn.access.ActorAccess.SetTimerAction(__instance, 0.002f);
                if (!__instance.hasHealth()) __instance.batch.c_check_deaths.Add(__instance);
                return false;
            }
        }
        [HarmonyPatch(typeof(ActorTool), "applyForceToUnit", new System.Type[] { typeof(AttackData), typeof(BaseSimObject), typeof(float), typeof(bool) })]
        private static class ApplyForceToUnitPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(AttackData pData, BaseSimObject pTargetToCheck, float pMod = 1f, bool pCheckCancelJobOnLand = false)
            {
                float force = pData.knockback * pMod;
                if (force <= 0f || pTargetToCheck == null || !xn.access.BaseSimObjectAccess.IsActor(pTargetToCheck)) return true; 
                var target = xn.access.BaseSimObjectAccess.GetActor(pTargetToCheck);
                var attacker = (pData.initiator != null && xn.access.BaseSimObjectAccess.IsActor(pData.initiator)) ? xn.access.BaseSimObjectAccess.GetActor(pData.initiator) : null;
                if (attacker != null && target != null)
                {
                    int ar = GetUnifiedRealmIndex(attacker);
                    int dr = GetUnifiedRealmIndex(target);
                    if (ar < 0 && dr >= 0) return false;
                    if (ar >= 0 && dr >= 0 && ar <= dr - 2) return false;
                }
                BaseStats targetStats = xn.access.BaseSimObjectAccess.GetStats(target);
                if (targetStats == null) return true;
                float resist = targetStats["Resist"];   
                force = Mathf.Max(force - resist, 0f);
                float kbReduce = targetStats[strings.S.knockback_reduction]; 
                if (kbReduce > 0f)
                    force *= Mathf.Max(0f, 1f - kbReduce / 100f);
                Vector2 pos = xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(target);
                Vector2 hit = pos + new Vector2(0.1f, 0f);                
                xn.access.ActorAccess.CalculateForce(target, pos.x, pos.y, hit.x, hit.y, force, 0f, pCheckCancelJobOnLand);
                return false; 
            }
        }
        [HarmonyPatch(typeof(MapBox), "updateMetaHistory")]
        private static class YearlyRegen_MapBoxPatch
        {
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (__instance == null) return;
                int curYear = Date.getCurrentYear();
                if (curYear <= 0) return;
                if (curYear == s_lastAppliedYear) return;
                s_lastAppliedYear = curYear;
                var list = __instance.units != null ? __instance.units.getSimpleList() : null;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var a = list[i];
                        if (a != null && a.isAlive())
                            DoAnnualRegen(a, 1); 
                    }
                }
            }
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class YearlyRegen_MapBoxUpdatePatch
        {
            [HarmonyPostfix]
            private static void Postfix(MapBox __instance)
            {
                if (__instance == null) return;
                int curYear = Date.getCurrentYear();
                if (curYear <= 0) return;
                if (curYear == s_lastAppliedYear) return;
                s_lastAppliedYear = curYear;
                var list = __instance.units != null ? __instance.units.getSimpleList() : null;
                int count = list != null ? list.Count : -1;
                if (list == null || count <= 0) return;
                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (a != null && a.isAlive())
                        DoAnnualRegen(a, 1); 
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "attackTargetActions")]
        private static class AttackTargetActionsPatch
        {
            [HarmonyPrefix]
            private static void Prefix(Actor __instance, BaseSimObject pTarget, WorldTile pTile)
            {
                s_currentDelegateAttacker = __instance;
            }
            [HarmonyPostfix]
            private static void Postfix()
            {
                s_currentDelegateAttacker = null;
            }
        }
        [HarmonyPatch(typeof(BaseSimObject), "changeHealth")]
        private static class ChangeHealthRealmCheckPatch
        {
            [HarmonyPrefix]
            [HarmonyPriority(int.MaxValue)] 
            private static void Prefix(BaseSimObject __instance, ref int pValue)
            {
                if (pValue >= 0) return;
                if (!xn.access.BaseSimObjectAccess.IsActor(__instance)) return;
                Actor target = xn.access.BaseSimObjectAccess.GetActor(__instance);
                if (target == null || !target.isAlive()) return;
                Actor attacker = null;
                if (xn.access.ActorAccess.GetAttackedBy(target) != null && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetAttackedBy(target)))
                {
                    attacker = xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(target));
                }
                if (attacker == null)
                {
                    attacker = s_currentDelegateAttacker;
                }
                if (attacker == null) return;
                int attackerRealm = GetUnifiedRealmIndex(attacker);
                int defenderRealm = GetUnifiedRealmIndex(target);
                if (attackerRealm < 0 && defenderRealm >= 0)
                {
                    pValue = 0;
                    D($"[changeHealth] blocked: NoRealm -> HasRealm");
                    return;
                }
                if (attackerRealm >= 0 && defenderRealm >= 0)
                {
                    int diff = attackerRealm - defenderRealm;
                    if (diff < 0 && -diff >= 3)
                    {
                        pValue = 0;
                        D($"[changeHealth] blocked: RealmDiff A({attackerRealm}) < D({defenderRealm})");
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "addForce", new Type[] { typeof(float), typeof(float), typeof(float), typeof(bool), typeof(bool) })]
        private static class AddForceRealmCheckPatch
        {
            [HarmonyPrefix]
            [HarmonyPriority(int.MaxValue)] 
            private static bool Prefix(Actor __instance, ref float pX, ref float pY, ref float pHeight)
            {
                Actor attacker = null;
                if (xn.access.ActorAccess.GetAttackedBy(__instance) != null && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetAttackedBy(__instance)))
                {
                    attacker = xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(__instance));
                }
                if (attacker == null)
                {
                    attacker = s_currentDelegateAttacker;
                }
                if (attacker == null) return true;
                int attackerRealm = GetUnifiedRealmIndex(attacker);
                int defenderRealm = GetUnifiedRealmIndex(__instance);
                if (attackerRealm < 0 && defenderRealm >= 0)
                {
                    D($"[addForce] blocked: NoRealm -> HasRealm");
                    return false;
                }
                if (attackerRealm >= 0 && defenderRealm >= 0)
                {
                    int diff = attackerRealm - defenderRealm;
                    if (diff < 0 && -diff >= 3)
                    {
                        D($"[addForce] blocked: RealmDiff A({attackerRealm}) < D({defenderRealm})");
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
