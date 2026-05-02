namespace xn.ui
{
    public static class XNPowerRanking
    {
        private static bool _inited;
        private static readonly string[] REALM_IDS = {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            XNRankingWindow.Init();
        }
        public static void Open()
        {
            xn.expand.AudioManager.PlayPaihangbang();
            XNRankingWindow.Open();
        }
        internal static bool TryGetScore(Actor a, out long score)
        {
            score = 0;
            if (a == null) return false;
            return XNRankingWindow.TryGetScore(a, out score);
        }
        internal static long CalcPowerScoreLongInternal(Actor a)
        {
            double dmg   = xn.access.BaseSimObjectAccess.GetStats(a)["damage"];          if (dmg   < 0) dmg = 0;   if (dmg   > 2_100_000_000d) dmg   = 2_100_000_000d;
            double aspd  = xn.access.BaseSimObjectAccess.GetStats(a)["attack_speed"];    if (aspd  < 0) aspd = 0;
            double cRate = xn.access.BaseSimObjectAccess.GetStats(a)["critical_chance"]; if (cRate < 0) cRate = 0; if (cRate > 1) cRate = 1;
            double cMult = xn.access.BaseSimObjectAccess.GetStats(a)["critical_damage_multiplier"]; if (cMult < 1) cMult = 1;
            double armor = xn.access.BaseSimObjectAccess.GetStats(a)["armor"];           if (armor < 0) armor = 0;
            double hpMax = a.getMaxHealth();           if (hpMax < 0) hpMax = 0; if (hpMax > 2_100_000_000d) hpMax = 2_100_000_000d;
            double dps  = dmg * aspd * (1.0 + cRate * cMult);
            double bulk = hpMax * 0.1 + armor * 1.5;
            double basePower = dps + bulk;
            int kills = xn.access.ActorAccess.GetData(a).kills; if (kills < 0) kills = 0;
            int age = xn.access.ActorAccess.GetData(a).getAge(); if (age < 0) age = 0;
            if (age > 20) age = 20;
            double multiplier = (1.0 + kills * 0.0025) * (1.0 + age * 0.001);
            double powered = basePower * multiplier;
            if (powered <= 0) return 0;
            double result = System.Math.Pow(powered, 0.8);
            double realmCoeff = GetRealmCoefficient(a);
            result = result * realmCoeff;
            result = System.Math.Round(result, 1);
            if (result <= 0) return 0;
            if (result >= long.MaxValue) return long.MaxValue;
            return (long)result;
        }
        private static double GetRealmCoefficient(Actor a)
        {
            if (a == null) return 0.1;
            int ancientStar = GetAncientStar(a);
            if (ancientStar > 0)
            {
                switch (ancientStar)
                {
                    case 1: return GetRealmCoeffByIndex(2);  
                    case 2: return GetRealmCoeffByIndex(4);  
                    case 3: return GetRealmCoeffByIndex(6);  
                    case 4: return GetRealmCoeffByIndex(7);  
                    case 5: return GetRealmCoeffByIndex(8);  
                    case 6: return GetRealmCoeffByIndex(9);  
                    case 7: return GetRealmCoeffByIndex(10); 
                    case 8: return GetRealmCoeffByIndex(11); 
                    case 9: return GetRealmCoeffByIndex(13); 
                    case 10: return GetRealmCoeffByIndex(14); 
                    default: return 0.1;
                }
            }
            int beastStage = GetBeastStage(a);
            if (beastStage > 0)
            {
                switch (beastStage)
                {
                    case 1: return GetRealmCoeffByIndex(2);  
                    case 2: return GetRealmCoeffByIndex(4);  
                    case 3: return GetRealmCoeffByIndex(6);  
                    case 4: return GetRealmCoeffByIndex(7);  
                    case 5: return GetRealmCoeffByIndex(8);  
                    case 6: return GetRealmCoeffByIndex(9);  
                    case 7: return GetRealmCoeffByIndex(10); 
                    case 8: return GetRealmCoeffByIndex(11); 
                    case 9: return GetRealmCoeffByIndex(13); 
                    case 10: return GetRealmCoeffByIndex(14); 
                    default: return 0.1;
                }
            }
            int realmIndex = GetRealmIndex(a);
            if (realmIndex >= 0)
            {
                return GetRealmCoeffByIndex(realmIndex);
            }
            return 0.1;
        }
        private static double GetRealmCoeffByIndex(int index)
        {
            if (index < 0) return 0.1;
            return System.Math.Pow(1.3, index);
        }
        private static int GetRealmIndex(Actor a)
        {
            if (a == null) return -1;
            var ts = a.getTraits();
            if (ts == null) return -1;
            int idx = -1;
            foreach (var t in ts)
            {
                if (t == null) continue;
                for (int i = 0; i < REALM_IDS.Length; i++)
                {
                    if (t.id == REALM_IDS[i])
                    {
                        if (i > idx) idx = i;
                    }
                }
            }
            return idx;
        }
        private static int GetAncientStar(Actor a)
        {
            if (a == null) return 0;
            var ts = a.getTraits();
            if (ts == null) return 0;
            int star = 0;
            foreach (var t in ts)
            {
                if (t == null || t.group_id != xn.Traits.RealmTraitGroup.GroupAncientRealm) continue;
                if (t.id.Length >= 14 && int.TryParse(t.id.Substring(8, 2), out int n) && n > star)
                {
                    star = n;
                }
            }
            return star;
        }
        private static int GetBeastStage(Actor a)
        {
            if (a == null) return 0;
            var ts = a.getTraits();
            if (ts == null) return 0;
            int stage = 0;
            foreach (var t in ts)
            {
                if (t == null || t.group_id != xn.Traits.RealmTraitGroup.GroupBeastStage) continue;
                if (t.id.Length >= 13 && int.TryParse(t.id.Substring(6, 2), out int n) && n > stage)
                {
                    stage = n;
                }
            }
            return stage;
        }
    }
}