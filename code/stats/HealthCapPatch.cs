using HarmonyLib;
namespace xn.stats
{
    internal static class HealthCapConfig
    {
        public const int MAX_HP_CAP = 1_800_000_000;
        public const float MAX_STAT_CAP = 1_800_000_000f; 
    }
    [HarmonyPatch(typeof(BaseSimObject), "getMaxHealth")]
    internal static class Patch_MaxHP_Cap
    {
        static void Postfix(BaseSimObject __instance, ref int __result)
        {
            float raw = __instance.stats["health"];
            if (raw > HealthCapConfig.MAX_HP_CAP)
            {
                __result = HealthCapConfig.MAX_HP_CAP;
                return;
            }
            if (raw < 1f)
            {
                __result = 1; 
                return;
            }
            __result = (int)raw;
        }
    }
    [HarmonyPatch(typeof(BaseSimObject), "setHealth")]
    internal static class Patch_SetHP_Cap
    {
        static void Prefix(ref int pValue)
        {
            if (pValue > HealthCapConfig.MAX_HP_CAP)
                pValue = HealthCapConfig.MAX_HP_CAP;
        }
    }
    [HarmonyPatch(typeof(BaseSimObject), "changeHealth")]
    internal static class Patch_ChangeHP_OverflowProtection
    {
        static void Prefix(BaseSimObject __instance, ref int pValue)
        {
            int currentHealth = __instance.getHealth();
            if (currentHealth > HealthCapConfig.MAX_HP_CAP)
            {
                __instance.setHealth(HealthCapConfig.MAX_HP_CAP);
                currentHealth = HealthCapConfig.MAX_HP_CAP;
            }
            long newHealth = (long)currentHealth + (long)pValue;
            if (newHealth > HealthCapConfig.MAX_HP_CAP)
            {
                pValue = HealthCapConfig.MAX_HP_CAP - currentHealth;
            }
            else if (newHealth < 0)
            {
                pValue = -currentHealth; 
            }
            else if (newHealth > int.MaxValue)
            {
                pValue = int.MaxValue - currentHealth;
            }
        }
    }
    [HarmonyPatch(typeof(BaseStats), "set")]
    internal static class Patch_StatsCap_OverflowProtection
    {
        private static readonly string[] PROTECTED_STATS = new[]
        {
            "mana",           
            "stamina",        
            "diplomacy",      
            "warfare",        
            "stewardship",    
            "intelligence",   
            "damage",         
            "critical_chance", 
            "lifespan"        
        };
        static void Prefix(string pID, ref float pAmount)
        {
            bool isProtected = false;
            for (int i = 0; i < PROTECTED_STATS.Length; i++)
            {
                if (pID == PROTECTED_STATS[i])
                {
                    isProtected = true;
                    break;
                }
            }
            if (!isProtected) return;
            if (pAmount > HealthCapConfig.MAX_STAT_CAP)
            {
                pAmount = HealthCapConfig.MAX_STAT_CAP;
            }
            else if (pAmount < -HealthCapConfig.MAX_STAT_CAP)
            {
                pAmount = -HealthCapConfig.MAX_STAT_CAP;
            }
        }
    }
}