using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace xn.world
{
    public static class XiuzhenguoSystem
    {
        public const string KEY_LEVEL = "xn.xiuzhenguo.level";  
        public static bool Visible { get; private set; } = false;
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
        public struct LevelConfig
        {
            public int level;               
            public string name;             
            public int requiredRealmIndex;  
            public int requiredCount;       
            public int secondaryRealmIndex; 
            public int secondaryCount;      
            public int maxAura;             
            public float speedBonus;        
            public bool autoKing;           
        }
        private static readonly LevelConfig[] LEVEL_CONFIGS = new LevelConfig[]
        {
            new LevelConfig { level=0, name="凡人国度", requiredRealmIndex=-1, requiredCount=0, secondaryRealmIndex=-1, secondaryCount=0, maxAura=40000, speedBonus=0f, autoKing=false },
            new LevelConfig { level=1, name="一级修真国", requiredRealmIndex=1, requiredCount=5, secondaryRealmIndex=-1, secondaryCount=0, maxAura=100000, speedBonus=0.01f, autoKing=false },  
            new LevelConfig { level=2, name="二级修真国", requiredRealmIndex=2, requiredCount=3, secondaryRealmIndex=-1, secondaryCount=0, maxAura=300000, speedBonus=0.05f, autoKing=false },  
            new LevelConfig { level=3, name="三级修真国", requiredRealmIndex=3, requiredCount=2, secondaryRealmIndex=-1, secondaryCount=0, maxAura=500000, speedBonus=0.08f, autoKing=false },  
            new LevelConfig { level=4, name="四级修真国", requiredRealmIndex=4, requiredCount=2, secondaryRealmIndex=-1, secondaryCount=0, maxAura=800000, speedBonus=0.10f, autoKing=false },  
            new LevelConfig { level=5, name="五级修真国", requiredRealmIndex=5, requiredCount=1, secondaryRealmIndex=-1, secondaryCount=0, maxAura=1000000, speedBonus=0.12f, autoKing=false },      
            new LevelConfig { level=6, name="六级修真国", requiredRealmIndex=6, requiredCount=1, secondaryRealmIndex=5, secondaryCount=5, maxAura=-1, speedBonus=0.15f, autoKing=true },       
            new LevelConfig { level=7, name="七级修真星", requiredRealmIndex=9, requiredCount=1, secondaryRealmIndex=7, secondaryCount=5, maxAura=-1, speedBonus=0.20f, autoKing=true },       
            new LevelConfig { level=8, name="八级修真星", requiredRealmIndex=13, requiredCount=1, secondaryRealmIndex=10, secondaryCount=10, maxAura=-1, speedBonus=0.25f, autoKing=true },    
            new LevelConfig { level=9, name="九级修真星", requiredRealmIndex=14, requiredCount=1, secondaryRealmIndex=13, secondaryCount=5, maxAura=-1, speedBonus=0.30f, autoKing=true },     
            new LevelConfig { level=10, name="顶级修真星", requiredRealmIndex=15, requiredCount=1, secondaryRealmIndex=14, secondaryCount=5, maxAura=-1, speedBonus=0.40f, autoKing=true }     
        };
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _h = new Harmony("xn.worldbox.xiuzhenguo");
            var mSetText = AccessTools.Method(typeof(NameplateText), "setText", new Type[] { typeof(string), typeof(Vector3), typeof(int) });
            if (mSetText != null)
                _h.Patch(mSetText, prefix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Pre_Nameplate_SetText)));
            var mUpdate = AccessTools.Method(typeof(MapBox), "Update");
            if (mUpdate != null)
                _h.Patch(mUpdate, postfix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Post_MapBox_Update)));
            var mCultTick = AccessTools.Method(typeof(xn.world.CultivationPracticeSystem), "Tick_Yearly");
            if (mCultTick != null)
            {
                _h.Patch(mCultTick, prefix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Pre_CultivationPractice_Tick)));
                _h.Patch(mCultTick, postfix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Post_CultivationPractice_Tick)));
            }
            var mCityInit = AccessTools.Method(typeof(xn.world.CityAuraSystem), "Post_City_Init");
            if (mCityInit != null)
                _h.Patch(mCityInit, postfix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Post_CityAura_Init)));
            var mRefreshAura = AccessTools.Method(typeof(xn.world.CityAuraSystem), "RefreshAllCityAura");
            if (mRefreshAura != null)
                _h.Patch(mRefreshAura, postfix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Post_RefreshAllCityAura)));
            var mFinishCapture = AccessTools.Method(typeof(City), "finishCapture", new Type[] { typeof(Kingdom) });
            if (mFinishCapture != null)
                _h.Patch(mFinishCapture, prefix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Pre_City_FinishCapture)));
        }
        public static void Toggle()
        {
            Visible = !Visible;
            WorldTip.showNowTop(Visible ? "tip_xiuzhenguo_on" : "tip_xiuzhenguo_off", pTranslate: true);
        }
        public static int GetLevel(Kingdom k)
        {
            if (k == null || k.isRekt()) return 0;
            int level;
            k.data.get(KEY_LEVEL, out level, 0);
            return level;
        }
        public static LevelConfig GetConfig(int level)
        {
            if (level < 0 || level >= LEVEL_CONFIGS.Length) return LEVEL_CONFIGS[0];
            return LEVEL_CONFIGS[level];
        }
        public static int CalculateLevel(Kingdom k)
        {
            if (k == null || k.isRekt() || k.units == null) return 0;
            int[] realmCounts = new int[REALM_IDS.Length];
            foreach (var unit in k.units)
            {
                if (unit == null || !unit.isAlive()) continue;
                int realmIndex = GetRealmIndex(unit);
                if (realmIndex >= 0 && realmIndex < realmCounts.Length)
                {
                    realmCounts[realmIndex]++;
                }
            }
            int specialLevel = 0;
            if (realmCounts[15] >= 1)  
            {
                specialLevel = 9;
            }
            else if (realmCounts[14] >= 1)  
            {
                specialLevel = 8;
            }
            else if (realmCounts[13] >= 1)  
            {
                specialLevel = 7;
            }
            int normalLevel = 0;
            for (int i = LEVEL_CONFIGS.Length - 1; i >= 1; i--)
            {
                var cfg = LEVEL_CONFIGS[i];
                bool primaryOk = false;
                if (cfg.requiredRealmIndex >= 0 && cfg.requiredRealmIndex < realmCounts.Length)
                {
                    int totalCount = 0;
                    for (int j = cfg.requiredRealmIndex; j < realmCounts.Length; j++)
                    {
                        totalCount += realmCounts[j];
                    }
                    primaryOk = totalCount >= cfg.requiredCount;
                }
                else
                {
                    primaryOk = true; 
                }
                bool secondaryOk = false;
                if (cfg.secondaryRealmIndex >= 0 && cfg.secondaryRealmIndex < realmCounts.Length)
                {
                    int totalCount = 0;
                    for (int j = cfg.secondaryRealmIndex; j < realmCounts.Length; j++)
                    {
                        totalCount += realmCounts[j];
                    }
                    secondaryOk = totalCount >= cfg.secondaryCount;
                }
                else
                {
                    secondaryOk = true; 
                }
                if (primaryOk && secondaryOk)
                {
                    normalLevel = i;
                    break;
                }
            }
            return Mathf.Max(specialLevel, normalLevel);
        }
        private static int GetRealmIndex(Actor a)
        {
            if (a == null || !a.isAlive()) return -1;
            for (int i = REALM_IDS.Length - 1; i >= 0; i--)
            {
                if (a.hasTrait(REALM_IDS[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        public static void Pre_Nameplate_SetText(NameplateText __instance, ref string pNewText, Vector3 pPos, ref int pAdditionalWidth)
        {
            if (!Visible || __instance == null) return;
            var meta = __instance.nano_object;
            if (meta == null) return;
            var kingdom = meta as Kingdom;
            if (kingdom != null && !kingdom.isRekt())
            {
                int level = GetLevel(kingdom);
                var cfg = GetConfig(level);
                pNewText = pNewText + $" · {cfg.name}";
                pAdditionalWidth += 100;
            }
        }
        private static int _lastCheckYear = -1;
        public static void Post_MapBox_Update(MapBox __instance)
        {
            if (__instance == null) return;
            int curYear = Date.getCurrentYear();
            if (curYear <= 0 || curYear == _lastCheckYear) return;
            _lastCheckYear = curYear;
            UpdateAllKingdomLevels();
        }
        private static void UpdateAllKingdomLevels()
        {
            if (World.world == null || World.world.kingdoms == null) return;
            foreach (var kingdom in World.world.kingdoms)
            {
                if (kingdom == null || kingdom.isRekt()) continue;
                int oldLevel = GetLevel(kingdom);
                int newLevel = CalculateLevel(kingdom);
                if (oldLevel != newLevel)
                {
                    kingdom.data.set(KEY_LEVEL, newLevel);
                    var cfg = GetConfig(newLevel);
                    if (cfg.autoKing && newLevel > oldLevel)
                    {
                        TryAutoSetKing(kingdom, cfg);
                    }
                    ApplyKingdomAuraLimit(kingdom);
                }
            }
        }
        private static void TryAutoSetKing(Kingdom k, LevelConfig cfg)
        {
            if (k == null || k.isRekt() || k.units == null) return;
            if (cfg.requiredRealmIndex < 0 || cfg.requiredRealmIndex >= REALM_IDS.Length) return;
            if (k.hasKing() && k.king != null && k.king.isAlive())
            {
                int kingRealmIndex = GetRealmIndex(k.king);
                if (kingRealmIndex >= cfg.requiredRealmIndex)
                {
                    return;
                }
            }
            foreach (var unit in k.units)
            {
                if (unit == null || !unit.isAlive()) continue;
                int unitRealmIndex = GetRealmIndex(unit);
                if (unitRealmIndex >= cfg.requiredRealmIndex)
                {
                    if (k.king != unit)
                    {
                        k.setKing(unit);
                        BroadcastSystem.Custom($"{unit.getName()} 成为 {k.name} 的国王！");
                    }
                    break; 
                }
            }
        }
        private static string GetRealmName(int realmIndex)
        {
            var names = new string[] {
                "凝气", "筑基", "结丹", "元婴", "化神", "婴变", "问鼎", "窥涅",
                "净涅", "碎涅", "空涅", "空灵", "空玄", "天尊", "半步踏天", "踏天"
            };
            if (realmIndex >= 0 && realmIndex < names.Length)
                return names[realmIndex];
            return "未知";
        }
        public static float GetSpeedBonus(Kingdom k)
        {
            if (k == null || k.isRekt()) return 0f;
            int level = GetLevel(k);
            var cfg = GetConfig(level);
            return cfg.speedBonus;
        }
        public static int GetMaxAura(Kingdom k)
        {
            if (k == null || k.isRekt()) return 40000;
            int level = GetLevel(k);
            var cfg = GetConfig(level);
            return cfg.maxAura < 0 ? int.MaxValue : cfg.maxAura;
        }
        public static int GetMaxKingdomAura(Kingdom k)
        {
            if (k == null || k.isRekt()) return 40000;
            int level = GetLevel(k);
            var cfg = GetConfig(level);
            if (cfg.maxAura < 0) return int.MaxValue;
            int cityCount = (k.cities != null) ? k.cities.Count : 1;
            if (cityCount <= 0) cityCount = 1;
            int maxTotal = cfg.maxAura * cityCount;
            return maxTotal >= cfg.maxAura ? maxTotal : cfg.maxAura;
        }
        [ThreadStatic]
        private static long _oldXP = 0;
        private const string KEY_XP = "xn.stat.xiuwei";
        private const string KEY_BREAK_SUCCESS_YEAR = "xn.break.success_year";  
        public static void Pre_CultivationPractice_Tick(Actor a)
        {
            if (a == null || !a.isAlive())
            {
                _oldXP = 0;
                return;
            }
            a.data.get(KEY_XP, out _oldXP, 0L);
        }
        public static void Post_CultivationPractice_Tick(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (a.kingdom == null || a.kingdom.isRekt()) return;
            int successYear; a.data.get(KEY_BREAK_SUCCESS_YEAR, out successYear, 0);
            if (successYear > 0)
            {
                int curYear = Date.getCurrentYear();
                if (curYear - successYear < 3)
                {
                    return; 
                }
            }
            float bonus = GetSpeedBonus(a.kingdom);
            if (bonus <= 0f) return;
            long newXP;
            a.data.get(KEY_XP, out newXP, 0L);
            long gainThisYear = newXP - _oldXP;
            if (gainThisYear <= 0) return; 
            long extra = (long)(gainThisYear * bonus);
            if (extra > 0)
            {
                a.data.set(KEY_XP, newXP + extra);
            }
        }
        public static void Post_CityAura_Init(City __instance)
        {
            if (__instance == null) return;
            if (!xn.config.ModConfigHooks.EnableXiuzhenguoAuraLimit)
            {
                return;
            }
            int currentAura;
            __instance.data.get(CityAuraSystem.KeyAura, out currentAura, 0);
            int maxAura = 40000; 
            if (__instance.kingdom != null && !__instance.kingdom.isRekt())
            {
                maxAura = GetMaxAura(__instance.kingdom);
            }
            if (maxAura < int.MaxValue && currentAura > maxAura)
            {
                __instance.data.set(CityAuraSystem.KeyAura, maxAura);
                currentAura = maxAura;
            }
            if (__instance.kingdom != null && !__instance.kingdom.isRekt())
            {
                ApplyKingdomAuraLimit(__instance.kingdom);
            }
        }
        private static void ApplyKingdomAuraLimit(Kingdom k)
        {
            if (k == null || k.isRekt() || k.cities == null) return;
            if (!xn.config.ModConfigHooks.EnableXiuzhenguoAuraLimit)
            {
                return;
            }
            int maxKingdomAura = GetMaxKingdomAura(k);
            if (maxKingdomAura >= int.MaxValue) return;
            int totalAura = 0;
            foreach (var city in k.cities)
            {
                if (city == null || city.isRekt()) continue;
                int aura;
                city.data.get(CityAuraSystem.KeyAura, out aura, 0);
                totalAura += aura;
            }
            if (totalAura > maxKingdomAura)
            {
                float ratio = (float)maxKingdomAura / (float)totalAura;
                foreach (var city in k.cities)
                {
                    if (city == null || city.isRekt()) continue;
                    int aura;
                    city.data.get(CityAuraSystem.KeyAura, out aura, 0);
                    int newAura = (int)(aura * ratio);
                    city.data.set(CityAuraSystem.KeyAura, newAura);
                }
            }
        }
        public static bool Pre_City_FinishCapture(City __instance, Kingdom pNewKingdom)
        {
            if (__instance == null || pNewKingdom == null) return true;
            if (__instance.kingdom == null || __instance.kingdom.isRekt()) return true;
            if (!xn.config.ModConfigHooks.EnableXiuzhenguoSuppress)
            {
                return true;
            }
            int defenderLevel = GetLevel(__instance.kingdom);   
            int attackerLevel = GetLevel(pNewKingdom);          
            if (attackerLevel < defenderLevel)
            {
                if (UnityEngine.Random.value < 0.30f)
                {
                    BroadcastSystem.Custom($"{pNewKingdom.name}（{GetConfig(attackerLevel).name}）无法占领 {__instance.kingdom.name}（{GetConfig(defenderLevel).name}）的城市：修真国等级不足！");
                }
                __instance.clearCapture(); 
                return false; 
            }
            else if (attackerLevel == defenderLevel && defenderLevel > 0)
            {
                if (UnityEngine.Random.value < 0.30f)
                {
                    BroadcastSystem.Custom($"{pNewKingdom.name} 与 {__instance.kingdom.name} 同为{GetConfig(defenderLevel).name}，无法占领其城市！");
                }
                __instance.clearCapture();
                return false;
            }
            return true;
        }
        public static void Post_RefreshAllCityAura()
        {
            if (World.world == null || World.world.kingdoms == null) return;
            foreach (var kingdom in World.world.kingdoms)
            {
                if (kingdom == null || kingdom.isRekt()) continue;
                ApplyKingdomAuraLimit(kingdom);
            }
        }
    }
}