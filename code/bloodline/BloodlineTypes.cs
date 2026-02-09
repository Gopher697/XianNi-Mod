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
            { TAIGU, "太古血脉" },
            { CAOMU, "草木血脉" },
            { MEIHUO, "魅惑血脉" },
            { HOUYI, "后羿血脉" },
            { HUANGQUAN, "黄泉血脉" },
            { ZUZHOU, "诅咒血脉" },
            { JIHAN, "极寒血脉" },
            { JUMO, "巨魔血脉" },
            { KUANGZHANSHI, "狂战士血脉" },
            { NIEPAN, "涅槃血脉" },
            { JINFA, "禁法血脉" },
            { GUTI, "古体血脉" },
            { SUIYUE, "岁月血脉" },
            { LEIFA, "雷罚血脉" },
            { XUANWU, "玄武血脉" },
            { ENAN, "厄难毒体" },
            { TIANSHA, "天煞血脉" },
            { SHIBIAN, "尸变血脉" },
            { ZAOSHUAI, "早衰血脉" },
            { JIBIAN, "畸变血脉" }
        };
        public static string GetLocaleName(string bloodlineType)
        {
            if (string.IsNullOrEmpty(bloodlineType)) return "无血脉";
            if (DISPLAY_NAMES.TryGetValue(bloodlineType, out string name))
            {
                return name;
            }
            return bloodlineType;
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