using System.Collections.Generic;
namespace xn.bloodline
{
    public static class BloodlineTypes
    {
        public const string NONE = "";
        public const string TAIGU = "taigu";           
        public const string CAOMU = "caomu";           
        public const string MEIHUO = "meihuo";         
        public const string HOUYI = "houyi";           
        public const string HUANGQUAN = "huangquan";   
        public const string ZUZHOU = "zuzhou";         
        public const string JIHAN = "jihan";           
        public const string JUMO = "jumo";             
        public const string KUANGZHANSHI = "kuangzhanshi"; 
        public const string NIEPAN = "niepan";             
        public const string JINFA = "jinfa";               
        public const string GUTI = "guti";             
        public const string SUIYUE = "suiyue";         
        public const string LEIFA = "leifa";           
        public const string XUANWU = "xuanwu";         
        public const string ENAN = "enan";             
        public const string TIANSHA = "tiansha";       
        public const string SHIBIAN = "shibian";       
        public const string ZAOSHUAI = "zaoshuai";     
        public const string JIBIAN = "jibian";         
        public static readonly string[] XIAN_MO_POOL = {
            TAIGU, CAOMU, MEIHUO, HOUYI, HUANGQUAN, ZUZHOU, JIHAN, JUMO
        };
        public static readonly string[] YAOSHOU_POOL = {
            KUANGZHANSHI, NIEPAN, HUANGQUAN, JINFA, JUMO
        };
        public static readonly string[] GUSHEN_POOL = {
            GUTI, SUIYUE, LEIFA, XUANWU, JINFA, JUMO
        };
        public static readonly string[] MUTATION_POOL = {
            ENAN, TIANSHA, SHIBIAN, ZAOSHUAI, JIBIAN
        };
        public static readonly string[] ALL_TYPES = {
            TAIGU, CAOMU, MEIHUO, HOUYI, HUANGQUAN, ZUZHOU, JIHAN, JUMO,
            KUANGZHANSHI, NIEPAN, JINFA,
            GUTI, SUIYUE, LEIFA, XUANWU,
            ENAN, TIANSHA, SHIBIAN, ZAOSHUAI, JIBIAN
        };
        public static readonly Dictionary<string, string> DISPLAY_NAMES = new Dictionary<string, string>
        {
            { TAIGU, "Primordial Bloodline" },
            { CAOMU, "Verdantwood Bloodline" },
            { MEIHUO, "Allure Bloodline" },
            { HOUYI, "Houyi Bloodline" },
            { HUANGQUAN, "Yellow Springs Bloodline" },
            { ZUZHOU, "Curse Bloodline" },
            { JIHAN, "Frostblood Bloodline" },
            { JUMO, "Giant-Demon Bloodline" },
            { KUANGZHANSHI, "Berserker Bloodline" },
            { NIEPAN, "Nirvana Bloodline" },
            { JINFA, "Spellbane Bloodline" },
            { GUTI, "Ancient Body Bloodline" },
            { SUIYUE, "Ageless Bloodline" },
            { LEIFA, "Thunder Punishment Bloodline" },
            { XUANWU, "Black Tortoise Bloodline" },
            { ENAN, "Calamity Venombody (Mutated)" },
            { TIANSHA, "Heavenbane Bloodline (Mutated)" },
            { SHIBIAN, "Corpseblight Bloodline (Mutated)" },
            { ZAOSHUAI, "Fleeting Bloom Bloodline (Mutated)" },
            { JIBIAN, "Aberrant Flesh Bloodline (Mutated)" }
        };
        public static string GetLocaleName(string bloodlineType)
        {
            if (string.IsNullOrEmpty(bloodlineType)) return T("bloodline_type_none", "No Bloodline");
            if (DISPLAY_NAMES.TryGetValue(bloodlineType, out string name))
            {
                return T("bloodline_type_" + bloodlineType, name);
            }
            return bloodlineType;
        }
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
        public static bool IsMutation(string bloodlineType)
        {
            if (string.IsNullOrEmpty(bloodlineType)) return false;
            foreach (var t in MUTATION_POOL)
            {
                if (t == bloodlineType) return true;
            }
            return false;
        }
    }
}
