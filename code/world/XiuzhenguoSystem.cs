using System;
using System.Collections.Generic;
using System.Reflection;
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
            public string localeKey;
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
            new LevelConfig { level=0, name="Mortal Kingdom", localeKey="xiuzhenguo_level_0", requiredRealmIndex=-1, requiredCount=0, secondaryRealmIndex=-1, secondaryCount=0, maxAura=40000, speedBonus=0f, autoKing=false },
            new LevelConfig { level=1, name="Rank 1 Cultivation Kingdom", localeKey="xiuzhenguo_level_1", requiredRealmIndex=1, requiredCount=5, secondaryRealmIndex=-1, secondaryCount=0, maxAura=100000, speedBonus=0.01f, autoKing=false },  
            new LevelConfig { level=2, name="Rank 2 Cultivation Kingdom", localeKey="xiuzhenguo_level_2", requiredRealmIndex=2, requiredCount=3, secondaryRealmIndex=-1, secondaryCount=0, maxAura=300000, speedBonus=0.05f, autoKing=false },  
            new LevelConfig { level=3, name="Rank 3 Cultivation Kingdom", localeKey="xiuzhenguo_level_3", requiredRealmIndex=3, requiredCount=2, secondaryRealmIndex=-1, secondaryCount=0, maxAura=500000, speedBonus=0.08f, autoKing=false },  
            new LevelConfig { level=4, name="Rank 4 Cultivation Kingdom", localeKey="xiuzhenguo_level_4", requiredRealmIndex=4, requiredCount=2, secondaryRealmIndex=-1, secondaryCount=0, maxAura=800000, speedBonus=0.10f, autoKing=false },  
            new LevelConfig { level=5, name="Rank 5 Cultivation Kingdom", localeKey="xiuzhenguo_level_5", requiredRealmIndex=5, requiredCount=1, secondaryRealmIndex=-1, secondaryCount=0, maxAura=1000000, speedBonus=0.12f, autoKing=false },      
            new LevelConfig { level=6, name="Rank 6 Cultivation Kingdom", localeKey="xiuzhenguo_level_6", requiredRealmIndex=6, requiredCount=1, secondaryRealmIndex=5, secondaryCount=5, maxAura=-1, speedBonus=0.15f, autoKing=true },       
            new LevelConfig { level=7, name="Rank 7 Cultivation Planet", localeKey="xiuzhenguo_level_7", requiredRealmIndex=9, requiredCount=1, secondaryRealmIndex=7, secondaryCount=5, maxAura=-1, speedBonus=0.20f, autoKing=true },       
            new LevelConfig { level=8, name="Rank 8 Cultivation Planet", localeKey="xiuzhenguo_level_8", requiredRealmIndex=13, requiredCount=1, secondaryRealmIndex=10, secondaryCount=10, maxAura=-1, speedBonus=0.25f, autoKing=true },    
            new LevelConfig { level=9, name="Rank 9 Cultivation Planet", localeKey="xiuzhenguo_level_9", requiredRealmIndex=14, requiredCount=1, secondaryRealmIndex=13, secondaryCount=5, maxAura=-1, speedBonus=0.30f, autoKing=true },     
            new LevelConfig { level=10, name="Peak Cultivation Planet", localeKey="xiuzhenguo_level_10", requiredRealmIndex=15, requiredCount=1, secondaryRealmIndex=14, secondaryCount=5, maxAura=-1, speedBonus=0.40f, autoKing=true }     
        };
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _h = new Harmony("xn.worldbox.xiuzhenguo");
            var mSetText = AccessTools.Method(typeof(NameplateText), "setText", new Type[] { typeof(string), typeof(Vector3), typeof(int) });
            TryPatch(mSetText, "NameplateText.setText(string, Vector3, int)", prefix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Pre_Nameplate_SetText)));
            var mUpdate = AccessTools.Method(typeof(MapBox), "Update");
            TryPatch(mUpdate, "MapBox.Update", postfix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Post_MapBox_Update)));
            var mCultTick = AccessTools.Method(typeof(xn.world.CultivationPracticeSystem), "GainCultivationAnnual", new Type[] { typeof(Actor) });
            TryPatch(mCultTick, "CultivationPracticeSystem.GainCultivationAnnual(Actor)", prefix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Pre_CultivationPractice_Tick)), postfix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Post_CultivationPractice_Tick)));
            var mFinishCapture = AccessTools.Method(typeof(City), "finishCapture", new Type[] { typeof(Kingdom) });
            TryPatch(mFinishCapture, "City.finishCapture(Kingdom)", prefix: new HarmonyMethod(typeof(XiuzhenguoSystem), nameof(Pre_City_FinishCapture)));
        }
        private static bool TryPatch(MethodBase target, string targetName, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            if (target == null)
            {
                Debug.LogWarning("[XN] XiuzhenguoSystem patch target not found, skipped: " + targetName);
                return false;
            }
            try
            {
                _h.Patch(target, prefix: prefix, postfix: postfix);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[XN] XiuzhenguoSystem patch failed, skipped: " + targetName + " - " + e.Message);
                return false;
            }
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
            if (level < 0 || level >= LEVEL_CONFIGS.Length) level = 0;
            return LEVEL_CONFIGS[level];
        }
        private static string GetConfigName(LevelConfig cfg)
        {
            return T(cfg.localeKey, cfg.name);
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
                pNewText = pNewText + $" · {GetConfigName(cfg)}";
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
                        BroadcastSystem.Custom(T("broadcast_xiuzhenguo_auto_king", "{0} became the king of {1}!", unit.getName(), k.name));
                    }
                    break; 
                }
            }
        }
        private static string GetRealmName(int realmIndex)
        {
            var names = new string[] {
                "Qi Condensation", "Foundation Establishment", "Core Formation", "Nascent Soul", "Soul Formation", "Soul Transformation", "Ascendant", "Nirvana Scryer",
                "Nirvana Cleanser", "Nirvana Shatterer", "Void Nirvana", "Void Spirit", "Void Arcanum", "Grand Empyrean", "Half-Step Heaven Trampling", "Heaven Trampling"
            };
            if (realmIndex >= 0 && realmIndex < names.Length)
                return T("trait_" + REALM_IDS[realmIndex], names[realmIndex]);
            return "Unknown";
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
            xn.access.ActorAccess.GetData(a).get(KEY_XP, out _oldXP, 0L);
        }
        public static void Post_CultivationPractice_Tick(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (a.kingdom == null || a.kingdom.isRekt()) return;
            int successYear; xn.access.ActorAccess.GetData(a).get(KEY_BREAK_SUCCESS_YEAR, out successYear, 0);
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
            xn.access.ActorAccess.GetData(a).get(KEY_XP, out newXP, 0L);
            long gainThisYear = newXP - _oldXP;
            if (gainThisYear <= 0) return; 
            long extra = (long)(gainThisYear * bonus);
            if (extra > 0)
            {
                xn.access.ActorAccess.GetData(a).set(KEY_XP, newXP + extra);
            }
        }
        public static bool Pre_City_FinishCapture(City __instance, Kingdom pNewKingdom)
        {
            if (__instance == null || pNewKingdom == null) return true;
            Kingdom defenderKingdom = xn.access.CityAccess.GetKingdom(__instance);
            if (defenderKingdom == null || defenderKingdom.isRekt()) return true;
            if (!xn.config.ModConfigHooks.EnableXiuzhenguoSuppress)
            {
                return true;
            }
            int defenderLevel = GetLevel(defenderKingdom);   
            int attackerLevel = GetLevel(pNewKingdom);          
            if (attackerLevel < defenderLevel)
            {
                if (UnityEngine.Random.value < 0.30f)
                {
                    BroadcastSystem.Custom(T("broadcast_xiuzhenguo_capture_level_low", "{0} ({1}) cannot occupy {2}'s city ({3}): cultivation kingdom rank is too low!", pNewKingdom.name, GetConfigName(GetConfig(attackerLevel)), defenderKingdom.name, GetConfigName(GetConfig(defenderLevel))));
                }
                xn.access.CityAccess.ClearCapture(__instance); 
                return false; 
            }
            else if (attackerLevel == defenderLevel && defenderLevel > 0)
            {
                if (UnityEngine.Random.value < 0.30f)
                {
                    BroadcastSystem.Custom(T("broadcast_xiuzhenguo_capture_same_level", "{0} and {1} are both {2}; it cannot occupy that city!", pNewKingdom.name, defenderKingdom.name, GetConfigName(GetConfig(defenderLevel))));
                }
                xn.access.CityAccess.ClearCapture(__instance);
                return false;
            }
            return true;
        }
    }
}
