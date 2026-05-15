using System;
namespace xn.config
{
    public static class ModConfigHooks
    {
        public static bool EnableLog = true;
        public static bool EnableAnimation = true;
        public static int AutoFavRealmGate = 1;
        public static int TianyunIntervalYears = 15;
        public static bool EnableTitles = true;
        public static bool EnableTitleGeneration = true;
        public static int BroadcastRealmGate = 1;
        public static bool EnableTatianSuppression = true;
        public static bool EnableTournamentDeathMatch = true;
        public static bool EnableBombSkills = true;
        public static string UnitSearchKeyword = "";
        public static int AmbitionAddValue = 1;
        public static bool EnableTianyunziSpawn = true;
        public static bool EnableAutoGC = false;
        public static int AutoGCThresholdMB = 512; 
        public static void OnLogSwitchChanged(bool pUpdatedValue)
        {
            EnableLog = pUpdatedValue;
        }
        public static void OnAutoFavRealmGateChanged(string val)
        {
            int v;
            if (!int.TryParse(val, out v)) { AutoFavRealmGate = 1; return; } 
            if (v < 0 || v > 13) { AutoFavRealmGate = 0; return; } 
            AutoFavRealmGate = v;
        }
         public static void OnTianyunYearsChanged(string val)
        {
            int years;
            if (!int.TryParse(val, out years)) years = 15;
            if (years < 1) years = 1;        
            if (years > 1000) years = 1000;  
            TianyunIntervalYears = years;
            xn.world.TianyunSystem.OnConfigIntervalChanged(years);
        }
        public static void OnTitlesSwitchChanged(bool pUpdatedValue)
        {
            EnableTitles = pUpdatedValue;
        }
        public static void OnTitleGenerationSwitchChanged(bool v)
        {
            EnableTitleGeneration = v;
        }
        public static void OnBroadcastRealmGateChanged(int v)
        {
            BroadcastRealmGate = v;
        }
        public static void OnTatianSuppressionSwitchChanged(bool v)
        {
            EnableTatianSuppression = v;
        }
        public static void OnTournamentDeathMatchSwitchChanged(bool v)
        {
            EnableTournamentDeathMatch = v;
        }
        public static void OnBombSkillsSwitchChanged(bool v)
        {
            EnableBombSkills = v;
        }
        public static void OnUnitSearchKeywordChanged(string val)
        {
            if (string.IsNullOrEmpty(val)) { UnitSearchKeyword = ""; return; }
            UnitSearchKeyword = val.Trim();
        }
        public static void OnAmbitionAddValueChanged(string val)
        {
            int v;
            if (!int.TryParse(val, out v)) v = 0;
            if (v < 0) v = 0;
            if (v > 1_000_000_000) v = 1_000_000_000; 
            AmbitionAddValue = v;
        }
        public static void OnTianyunziSpawnSwitchChanged(bool v)
        {
            EnableTianyunziSpawn = v;
        }
        public static void OnAutoGCSwitchChanged(bool v)
        {
            EnableAutoGC = v;
        }
        public static void OnAutoGCThresholdChanged(string val)
        {
            int mb;
            if (!int.TryParse(val, out mb)) mb = 512; 
            if (mb < 64) mb = 64;          
            if (mb > 1024 * 64) mb = 1024 * 64; 
            AutoGCThresholdMB = mb;
        }
        public static int DashouBehaviorMode = 0;
        public static void OnDashouBehaviorChanged(string val)
        {
            int mode;
            if (!int.TryParse(val, out mode)) mode = 0;
            if (mode < 0) mode = 0;
            if (mode > 2) mode = 2;
            DashouBehaviorMode = mode;
        }
        public static bool EnableXiuzhenguoSuppress = true;
        public static void OnXiuzhenguoSuppressSwitchChanged(bool v)
        {
            EnableXiuzhenguoSuppress = v;
        }
        public static bool EnableAncientBeastLevelLimit = true;
        public static void OnAncientBeastLevelLimitSwitchChanged(bool v)
        {
            EnableAncientBeastLevelLimit = v;
        }
        public static int AncientBeastMultiplier = 1;
        public static void OnAncientBeastMultiplierChanged(int val)
        {
            if (val < 1) val = 1;
            if (val > 10000) val = 10000;
            AncientBeastMultiplier = val;
        }
        public static int SkinToneColorIndex = 0;
        public static void OnSkinToneColorChanged(int val)
        {
            if (val < 0) val = 0;
            if (val > 15) val = 15;
            SkinToneColorIndex = val;
        }
        public static bool EnableAutoJoinArmy = false;
        public static void OnAutoJoinArmySwitchChanged(bool v)
        {
            EnableAutoJoinArmy = v;
        }
        public static bool EnableBuildingProtection = false;
        public static void OnBuildingProtectionSwitchChanged(bool v)
        {
            EnableBuildingProtection = v;
        }
        public static bool EnableDemonicHunt = true;
        public static void OnDemonicHuntSwitchChanged(bool v)
        {
            EnableDemonicHunt = v;
        }
        public static bool EnableBloodlineAwaken = true;
        public static void OnBloodlineAwakenSwitchChanged(bool v)
        {
            EnableBloodlineAwaken = v;
        }
        public static bool EnableMentorship = true;
        public static void OnMentorshipSwitchChanged(bool v)
        {
            EnableMentorship = v;
        }
        public static bool EnableXiuzhenguoAuraLimit = true;
        public static void OnXiuzhenguoAuraLimitSwitchChanged(bool v)
        {
            EnableXiuzhenguoAuraLimit = v;
        }
        public static int MaxKingdomAura = 500000;
        public static void OnMaxKingdomAuraChanged(string val)
        {
            int v;
            if (!int.TryParse(val, out v)) v = 500000;
            if (v < 0) v = 0;
            if (v > 1_000_000_000) v = 1_000_000_000;
            MaxKingdomAura = v;
        }
        // Legacy no-op callbacks for retired city aura config entries that may remain in user config files.
        public static void OnCityAuraMaxChanged(string val) { }
        public static void OnCityAuraRefreshYearsChanged(string val) { }
        public static void OnAnimationSwitchChanged(bool pUpdatedValue)
        {
            EnableAnimation = pUpdatedValue;
        }
        public static bool EnableBroadcastDisplay = true;
        public static void OnBroadcastDisplaySwitchChanged(bool v)
        {
            EnableBroadcastDisplay = v;
        }
        public static bool EnableBroadcastConsoleLog = true;
        public static void OnBroadcastConsoleLogSwitchChanged(bool v)
        {
            EnableBroadcastConsoleLog = v;
        }
        public static bool EnableMcSelectSfx = true;
        public static void OnMcSelectSfxSwitchChanged(bool v)
        {
            EnableMcSelectSfx = v;
        }
        public static bool EnableAIVoice = false;
        public static void OnAIVoiceSwitchChanged(bool v)
        {
            EnableAIVoice = v;
        }
        // AI text generation — works with any OpenAI-compatible endpoint
        public static bool EnableAITextGen = false;
        public static void OnAITextGenSwitchChanged(bool v)
        {
            EnableAITextGen = v;
        }
        // Backward-compat alias so existing save-configs still function
        public static void OnDeepSeekTextGenSwitchChanged(bool v) => OnAITextGenSwitchChanged(v);
        public static string CustomAIApiKey = "";
        public static void OnCustomAIApiKeyChanged(string val)
        {
            CustomAIApiKey = val?.Trim() ?? "";
        }
        public static string CustomAIUrl = "";
        public static void OnCustomAIUrlChanged(string val)
        {
            CustomAIUrl = val?.Trim() ?? "";
        }
        public static string CustomAIModel = "";
        public static void OnCustomAIModelChanged(string val)
        {
            CustomAIModel = val?.Trim() ?? "";
        }
        public static void InitializeFromConfig(NeoModLoader.api.ModConfig config)
        {
            if (config == null) return;
            try
            {
                try
                {
                    var apiKeyItem = config["xn_config_ai"]["xn_config_custom_ai_api_key"];
                    if (apiKeyItem != null && !string.IsNullOrEmpty(apiKeyItem.TextVal))
                    {
                        OnCustomAIApiKeyChanged(apiKeyItem.TextVal);
                    }
                }
                catch (System.Collections.Generic.KeyNotFoundException) { }
                try
                {
                    var urlItem = config["xn_config_ai"]["xn_config_custom_ai_url"];
                    if (urlItem != null && !string.IsNullOrEmpty(urlItem.TextVal))
                    {
                        OnCustomAIUrlChanged(urlItem.TextVal);
                    }
                }
                catch (System.Collections.Generic.KeyNotFoundException) { }
                try
                {
                    var modelItem = config["xn_config_ai"]["xn_config_custom_ai_model"];
                    if (modelItem != null && !string.IsNullOrEmpty(modelItem.TextVal))
                    {
                        OnCustomAIModelChanged(modelItem.TextVal);
                    }
                }
                catch (System.Collections.Generic.KeyNotFoundException) { }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[XN-Config] Config initialization failed: {e.Message}");
            }
        }
        public static bool DisableCondense = false;
        public static void OnCondenseSwitchChanged(bool v)
        {
            DisableCondense = v;
            xn.world.CultivationPracticeSystem.DisableCondense = v;
        }
        public static bool EnableXianniLaw = true;
        public static void OnXianniLawSwitchChanged(bool v)
        {
            EnableXianniLaw = v;
        }
        public static bool EnableDeathLaw = false;
        public static void OnDeathLawSwitchChanged(bool v)
        {
            EnableDeathLaw = v;
        }
    }
}
