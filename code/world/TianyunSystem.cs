using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.config;
namespace xn.world
{
    internal static class TianyunSystem
    {
        const string KEY_WUXIN = "xn.stat.wuxin";         
        const string KEY_LUCK = "xn.stat.qiyun";         
        const string KEY_XP = "xn.stat.xiuwei";        
        const string KEY_STOP = "xn.cultivation.stop";   
        const string KEY_SEAL_UNTIL_YEAR = "xn.seal_until_year"; 
        const string KEY_BREAK_SUCCESS_YEAR = "xn.break.success_year";  
        const string KEY_ANC_POWER = "xn.stat.gushen_power"; 
        const string KEY_BEAST_POWER = "xn.stat.yaoli";        
        const string KEY_XINMO = "xn.stat.xinmo";           
        const string KEY_LINGSHI = "xn.stat.lingshi";
        const string KEY_LINGSHI_SUPREME = "xn.stat.lingshi_supreme";
        const string KEY_TY_COUNT = "xn.tianyun.count";
        static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        static readonly long[] REALM_THRESHOLDS = new long[]
        {
            100000,1500000,4000000,9600000,30000000,80000000,
            150000000,250000000,400000000,600000000,
            700000000,800000000,900000000,980000000,1200000000,1500000000
        };
        static ActorTrait[] s_poolDivine; 
        static ActorTrait[] s_poolArt;    
        static ActorTrait[] s_poolRoots;  
        static int s_nextYear = -1; 
        public static void OnConfigIntervalChanged(int years)
        {
            if (years < 1) years = 1;
            s_nextYear = Date.getCurrentYear() + years;
        }
        static System.Text.StringBuilder s_detailBuf;
        static string s_detailName;
        static string ExtractDetail(string raw, string actorName)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            string t = raw.Trim();
            int rb = t.IndexOf(']');
            if (rb >= 0 && rb + 1 < t.Length) t = t.Substring(rb + 1).Trim();
            if (!string.IsNullOrEmpty(actorName))
            {
                int p = t.IndexOf(actorName);
                if (p >= 0) t = t.Remove(p, actorName.Length).Trim();
            }
            if (t.EndsWith("。") || t.EndsWith(".")) t = t.Substring(0, t.Length - 1);
            return t;
        }
        public static void Init(Harmony h)
        {
            h.Patch(AccessTools.Method(typeof(MapBox), "updateMetaHistory"),
                postfix: new HarmonyMethod(typeof(TianyunSystem), nameof(Post_updateMetaHistory)));
        }
        static void Post_updateMetaHistory(MapBox __instance)
        {
            int year = Date.getCurrentYear();
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || !a.isAlive()) continue;
                int until = 0; a.data.get(KEY_SEAL_UNTIL_YEAR, out until, 0);
                if (until > 0 && year >= until)
                {
                    a.data.set(KEY_SEAL_UNTIL_YEAR, 0);
                    a.data.set(KEY_STOP, 0); 
                }
            }
            int interval = ModConfigHooks.TianyunIntervalYears; 
            if (interval <= 0) interval = 15;
            if (s_nextYear < 0) { s_nextYear = year + interval; return; }
            if (year < s_nextYear) return;
            s_nextYear = year + interval; 
            TriggerTianyun();             
        }
        static void TriggerTianyun()
        {
            BroadcastSystem.TianyunPrepare();
            if (Randy.randomChance(0.5f))
            {
                var tile = PickRandomUnitTile();
                if (tile != null)
                {
                    RuinBuildingAssets.PlaceAt(tile, playSfx: true);
                    BroadcastSystem.TianyunRuinBuilt();
                }
            }
            int cntWant = Randy.randomInt(1, 20);
            var picks = SampleRandomUnits(cntWant);
            if (picks == null || picks.Count == 0) return;
            int roll = Randy.randomInt(0, 100);
            bool reward = roll <= 65;
            bool ambRound = Randy.randomChance(0.60f);
            int ambLeft = ambRound ? 3 : 0;
            int affected = 0;
            for (int i = 0; i < picks.Count; i++)
            {
                var a = picks[i];
                if (a == null || !a.isAlive()) continue;
                if (reward)
                {
                    s_detailBuf = new System.Text.StringBuilder(64);
                    s_detailName = a.getName();
                    bool ok = DoReward(a);
                    string details = s_detailBuf.ToString();
                    s_detailBuf = null; s_detailName = null;
                    string extra = null;
                    if (ambLeft > 0 && ModConfigHooks.EnableTianyunziSpawn)
                    {
                        bool canS, beastMad; int amb;
                        GetAmbitionFor(a, out canS, out amb, out beastMad);
                        if (beastMad)
                        {
                            AddTrait(a, "madness"); 
                            extra = "天运子把妖兽" + NameOf(a) + " 当宠物狗一样玩弄并释放出来咬人了";
                            ambLeft--; 
                        }
                        else if (canS)
                        {
                            ambLeft--; 
                            if (Randy.randomChance(0.20f))
                            {
                                extra = "天运子阴谋被" + NameOf(a) + " 发现了，使用躲避术，天运子算计失误，野心值下降5%";
                                AmbitionSystem.DecPercent(5);
                            }
                            else
                            {
                                KillInstant(a); 
                                if (!a.hasHealth()) a.batch.c_check_deaths.Add(a);
                                AmbitionSystem.Add(amb);
                                extra = NameOf(a) + " 被天运子使用仙术传送到了仙界并收为亲传弟子";
                            }
                        }
                        else
                        {
                            extra = "天运子阴谋被" + NameOf(a) + " 发现了，使用躲避术，天运子算计失误，野心值下降5%";
                            AmbitionSystem.DecPercent(5);
                        }
                    }
                    if (!string.IsNullOrEmpty(details) || !string.IsNullOrEmpty(extra))
                    {
                        affected++; IncTianyunCount(a);
                        string body = string.IsNullOrEmpty(extra) ? details
                                    : string.IsNullOrEmpty(details) ? extra
                                    : details + "；" + extra;
                        BroadcastSystem.PostActor(a, "[天运·赏赐] " + NameOf(a) + " " + body);
                    }
                }
                else
                {
                    s_detailBuf = new System.Text.StringBuilder(64);
                    s_detailName = a.getName();
                    bool ok = DoBacklash(a);
                    string details = s_detailBuf.ToString();
                    s_detailBuf = null; s_detailName = null;
                    string extra = null;
                    if (ambLeft > 0 && ModConfigHooks.EnableTianyunziSpawn)
                    {
                        bool canS, beastMad; int amb;
                        GetAmbitionFor(a, out canS, out amb, out beastMad);
                        if (beastMad)
                        {
                            AddTrait(a, "madness");
                            extra = "妖兽" + NameOf(a) + " 被天运子施加咒语陷入了疯狂";
                            ambLeft--;
                        }
                        else if (canS)
                        {
                            ambLeft--;
                            if (Randy.randomChance(0.20f))
                            {
                                extra = "天运子阴谋被" + NameOf(a) + " 发现了，使用躲避术，天运子算计失误，野心值下降5%";
                                AmbitionSystem.DecPercent(5);
                            }
                            else
                            {
                                KillInstant(a); 
                                if (!a.hasHealth()) a.batch.c_check_deaths.Add(a);
                                AmbitionSystem.Add(amb);
                                extra = NameOf(a) + " 被天运子吞噬了，野心值增加了 " + amb;
                            }
                        }
                        else
                        {
                            extra = "天运子阴谋被" + NameOf(a) + " 发现了，使用躲避术，天运子算计失误，野心值下降5%";
                            AmbitionSystem.DecPercent(5);
                        }
                    }
                    if (!string.IsNullOrEmpty(details) || !string.IsNullOrEmpty(extra))
                    {
                        affected++; IncTianyunCount(a);
                        string body = string.IsNullOrEmpty(extra) ? details
                                    : string.IsNullOrEmpty(details) ? extra
                                    : details + "；" + extra;
                        BroadcastSystem.PostActor(a, "[天运·反噬] " + NameOf(a) + " " + body);
                    }
                }
            }
            if (reward) BroadcastSystem.TianyunRewardSummary(affected);
            else BroadcastSystem.TianyunBacklashSummary(affected);
        }
        static WorldTile PickRandomUnitTile()
        {
            var list = World.world.units.getSimpleList();
            if (list.Count == 0) return null;
            int start = Randy.randomInt(0, list.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                int idx = (start + i) % list.Count;
                var a = list[idx];
                if (a != null && a.isAlive()) return a.current_tile;
            }
            return null;
        }
        static List<Actor> SampleRandomUnits(int want)
        {
            var src = World.world.units.getSimpleList();
            var tmp = new List<Actor>(Math.Min(src.Count, 2048));
            for (int i = 0; i < src.Count; i++)
            {
                var a = src[i];
                if (a != null && a.isAlive()) tmp.Add(a);
            }
            if (tmp.Count == 0) return tmp;
            if (want > tmp.Count) want = tmp.Count;
            int n = tmp.Count;
            var res = new List<Actor>(want);
            for (int k = 0; k < want; k++)
            {
                int pick = Randy.randomInt(0, n - 1);
                res.Add(tmp[pick]);
                var t = tmp[pick]; tmp[pick] = tmp[n - 1]; tmp[n - 1] = t;
                n--;
            }
            return res;
        }
        static bool DoReward(Actor a)
        {
            int choice = Randy.randomInt(1, 8);
            switch (choice)
            {
                case 1:
                    { 
                        long beforeXP = 0, afterXP = 0; int beforeAP = 0, afterAP = 0, beforeBP = 0, afterBP = 0;
                        a.data.get(KEY_XP, out beforeXP, 0L); a.data.get(KEY_ANC_POWER, out beforeAP, 0); a.data.get(KEY_BEAST_POWER, out beforeBP, 0);
                        bool ok = Reward_XP_Or_Power(a);
                        a.data.get(KEY_XP, out afterXP, 0L); a.data.get(KEY_ANC_POWER, out afterAP, 0); a.data.get(KEY_BEAST_POWER, out afterBP, 0);
                        if (ok)
                        {
                            if (IsAncient(a)) LogHist($"[天运·赏赐] {NameOf(a)} 古神之力 +{afterAP - beforeAP}");
                            else if (IsBeast(a)) LogHist($"[天运·赏赐] {NameOf(a)} 妖力 +{afterBP - beforeBP}");
                            else LogHist($"[天运·赏赐] {NameOf(a)} 修为 +{afterXP - beforeXP}");
                        }
                        return ok;
                    }
                case 2:
                    { 
                        int before; a.data.get(KEY_WUXIN, out before, 0);
                        int add = Randy.randomInt(5, 10);
                        bool ok = AddIntClamped(a, KEY_WUXIN, add, 0, 100);
                        int after; a.data.get(KEY_WUXIN, out after, 0);
                        if (ok) LogHist($"[天运·赏赐] {NameOf(a)} 悟性 +{after - before}");
                        return ok;
                    }
                case 3:
                    { 
                        int before; a.data.get(KEY_LUCK, out before, 0);
                        int add = Randy.randomInt(5, 10);
                        bool ok = AddIntClamped(a, KEY_LUCK, add, 0, 100);
                        int after; a.data.get(KEY_LUCK, out after, 0);
                        if (ok) LogHist($"[天运·赏赐] {NameOf(a)} 气运 +{after - before}");
                        return ok;
                    }
                case 4:
                    { 
                        bool ok = RollGiveTrait(a, isDivine: true, p: 0.60f);
                        return ok;
                    }
                case 5:
                    { 
                        bool ok = RollGiveTrait(a, isDivine: false, p: 0.60f);
                        return ok;
                    }
                case 6:
                    { 
                        int before; a.data.get(KEY_LINGSHI, out before, 0);
                        int add = Randy.randomInt(1, 1000);
                        bool ok = AddStones(a, KEY_LINGSHI, add);
                        int after; a.data.get(KEY_LINGSHI, out after, 0);
                        if (ok) LogHist($"[天运·赏赐] {NameOf(a)} 灵石 +{after - before}");
                        return ok;
                    }
                case 7:
                    { 
                        int before; a.data.get(KEY_LINGSHI_SUPREME, out before, 0);
                        int add = Randy.randomInt(1, 100);
                        bool ok = AddStones(a, KEY_LINGSHI_SUPREME, add);
                        int after; a.data.get(KEY_LINGSHI_SUPREME, out after, 0);
                        if (ok) LogHist($"[天运·赏赐] {NameOf(a)} 至上灵石 +{after - before}");
                        return ok;
                    }
                case 8:
                    { 
                        bool ok = GiveRandomRoot(a);
                        return ok;
                    }
            }
            return false;
        }
        static bool DoBacklash(Actor a)
        {
            int choice = Randy.randomInt(1, 9);
            switch (choice)
            {
                case 1:
                    { 
                        long beforeXP = 0, afterXP = 0; int beforeAP = 0, afterAP = 0, beforeBP = 0, afterBP = 0;
                        a.data.get(KEY_XP, out beforeXP, 0L); a.data.get(KEY_ANC_POWER, out beforeAP, 0); a.data.get(KEY_BEAST_POWER, out beforeBP, 0);
                        bool ok = Backlash_XP_Or_Power(a);
                        a.data.get(KEY_XP, out afterXP, 0L); a.data.get(KEY_ANC_POWER, out afterAP, 0); a.data.get(KEY_BEAST_POWER, out afterBP, 0);
                        if (ok)
                        {
                            if (IsAncient(a)) LogHist($"[天运·反噬] {NameOf(a)} 古神之力 -{beforeAP - afterAP}");
                            else if (IsBeast(a)) LogHist($"[天运·反噬] {NameOf(a)} 妖力 -{beforeBP - afterBP}");
                            else LogHist($"[天运·反噬] {NameOf(a)} 修为 -{beforeXP - afterXP}");
                        }
                        return ok;
                    }
                case 2:
                    { 
                        bool ok = ReduceBaseStatHealth(a, 0.8f);
                        if (ok) LogHist($"[天运·反噬] {NameOf(a)} 生命上限降低 20%");
                        return ok;
                    }
                case 3:
                    { 
                        bool ok = SealForYears(a, 10);
                        if (ok) LogHist($"[天运·反噬] {NameOf(a)} 被封印 10 年");
                        return ok;
                    }
                case 4:
                    { 
                        bool ok = MindWipe(a);
                        if (ok) LogHist($"[天运·反噬] {NameOf(a)} 意识被抹除（忠诚清零，可能陷入疯狂）");
                        return ok;
                    }
                case 5:
                    { 
                        int before; a.data.get(KEY_LUCK, out before, 0);
                        bool ok = LuckStrip(a);
                        int after; a.data.get(KEY_LUCK, out after, 0);
                        if (ok)
                        {
                            if (after == 1 && before != 1) LogHist($"[天运·反噬] {NameOf(a)} 气运被剥离至 1");
                            else LogHist($"[天运·反噬] {NameOf(a)} 气运 -{before - after}");
                        }
                        return ok;
                    }
                case 6:
                    { 
                        int before; a.data.get(KEY_WUXIN, out before, 0);
                        bool ok = WuxinStrip(a);
                        int after; a.data.get(KEY_WUXIN, out after, 0);
                        if (ok)
                        {
                            if (after == 1 && before != 1) LogHist($"[天运·反噬] {NameOf(a)} 悟性被剥离至 1");
                            else LogHist($"[天运·反噬] {NameOf(a)} 悟性 -{before - after}");
                        }
                        return ok;
                    }
                case 7:
                    { 
                        int b1; a.data.get(KEY_LINGSHI, out b1, 0);
                        int b2; a.data.get(KEY_LINGSHI_SUPREME, out b2, 0);
                        bool ok = KillOrClearStonesOrNothing(a);
                        if (ok)
                        {
                            if (a.isRekt()) LogHist($"[天运·反噬] {NameOf(a)} 当场陨落");
                            else
                            {
                                int a1; a.data.get(KEY_LINGSHI, out a1, 0);
                                int a2; a.data.get(KEY_LINGSHI_SUPREME, out a2, 0);
                                if (a1 == 0 && a2 == 0 && (b1 > 0 || b2 > 0))
                                    LogHist($"[天运·反噬] {NameOf(a)} 灵石被清空");
                            }
                        }
                        return ok;
                    }
                case 8:
                    { 
                        int before; a.data.get(KEY_XINMO, out before, 0);
                        bool ok = AddInt(a, KEY_XINMO, 50);
                        int after; a.data.get(KEY_XINMO, out after, 0);
                        if (ok) LogHist($"[天运·反噬] {NameOf(a)} 心魔 +{after - before}");
                        return ok;
                    }
                case 9:
                    { 
                        bool ok = AddTrait(a, "root_07_broken");
                        if (ok) LogHist($"[天运·反噬] {NameOf(a)} 获得【断根】");
                        return ok;
                    }
            }
            return false;
        }
        static bool Reward_XP_Or_Power(Actor a)
        {
            if (IsAncient(a)) { int cur; a.data.get(KEY_ANC_POWER, out cur, 0); int add = (int)(GetRealmThreshold(a) * 0.30f); a.data.set(KEY_ANC_POWER, cur + add); LogHist($"[天运·赏赐] {NameOf(a)} 古神之力 +{add}"); return true; }
            if (IsBeast(a)) { int cur; a.data.get(KEY_BEAST_POWER, out cur, 0); int add = (int)(GetRealmThreshold(a) * 0.30f); a.data.set(KEY_BEAST_POWER, cur + add); LogHist($"[天运·赏赐] {NameOf(a)} 妖力 +{add}"); return true; }
            int successYear; a.data.get(KEY_BREAK_SUCCESS_YEAR, out successYear, 0);
            if (successYear > 0)
            {
                int curYear = Date.getCurrentYear();
                if (curYear - successYear < 3)
                {
                    return false; 
                }
            }
            long xp; a.data.get(KEY_XP, out xp, 0L); long addL = (long)(GetRealmThreshold(a) * 0.30f); a.data.set(KEY_XP, xp + addL); LogHist($"[天运·赏赐] {NameOf(a)} 修为 +{addL}"); return true;
        }
        static bool Backlash_XP_Or_Power(Actor a)
        {
            if (IsAncient(a)) { int cur; a.data.get(KEY_ANC_POWER, out cur, 0); int dec = cur / 2; a.data.set(KEY_ANC_POWER, Math.Max(0, cur - dec)); LogHist($"[天运·反噬] {NameOf(a)} 古神之力 -{dec}"); return true; }
            if (IsBeast(a)) { int cur; a.data.get(KEY_BEAST_POWER, out cur, 0); int dec = cur / 2; a.data.set(KEY_BEAST_POWER, Math.Max(0, cur - dec)); LogHist($"[天运·反噬] {NameOf(a)} 妖力 -{dec}"); return true; }
            long xp; a.data.get(KEY_XP, out xp, 0L); long decL = xp / 2; a.data.set(KEY_XP, Math.Max(0, xp - decL)); LogHist($"[天运·反噬] {NameOf(a)} 修为 -{decL}"); return true;
        }
        static long GetRealmThreshold(Actor a)
        {
            int idx = -1;
            var traits = a.getTraits(); if (traits == null) return 0;
            for (int i = 0; i < REALM_IDS.Length; i++)
                foreach (var t in traits) { if (t != null && t.id == REALM_IDS[i]) { if (i > idx) idx = i; } }
            if (idx < 0) return 0;
            if (idx >= 0 && idx < REALM_THRESHOLDS.Length) return REALM_THRESHOLDS[idx];
            return REALM_THRESHOLDS[REALM_THRESHOLDS.Length - 1];
        }
        static bool IsAncient(Actor a) { foreach (var t in a.getTraits()) { if (t != null && t.id.StartsWith("ancient_")) return true; } return false; }
        static bool IsBeast(Actor a) { foreach (var t in a.getTraits()) { if (t != null && t.id.StartsWith("beast_")) return true; } return false; }
        static bool AddIntClamped(Actor a, string key, int add, int min, int max)
        { int cur; a.data.get(key, out cur, 0); int v = cur + add; if (v < min) v = min; if (v > max) v = max; a.data.set(key, v); return true; }
        static bool AddInt(Actor a, string key, int add)
        { int cur; a.data.get(key, out cur, 0); a.data.set(key, cur + add); return true; }
        static bool LuckStrip(Actor a)
        {
            if (Randy.randomChance(0.5f)) { a.data.set(KEY_LUCK, 1); return true; }
            return AddIntClamped(a, KEY_LUCK, -Randy.randomInt(5, 10), 0, 100);
        }
        static bool WuxinStrip(Actor a)
        {
            if (Randy.randomChance(0.5f)) { a.data.set(KEY_WUXIN, 1); return true; }
            return AddIntClamped(a, KEY_WUXIN, -Randy.randomInt(5, 10), 0, 100);
        }
        static bool SealForYears(Actor a, int years)
        {
            a.data.set(KEY_STOP, 1);
            a.data.set(KEY_SEAL_UNTIL_YEAR, Date.getCurrentYear() + years);
            a.addStatusEffect("slowness", 3f);
            a.makeStunned(2f);
            return true;
        }
        static bool ReduceBaseStatHealth(Actor a, float ratio)
        {
            float hpF = a.stats["health"];
            int nhp = Mathf.Max(1, Mathf.FloorToInt(hpF * ratio));
            if (nhp < hpF) a.stats["health"] = nhp;
            return true;
        }
        static bool RollGiveTrait(Actor a, bool isDivine, float p)
        {
            if (!Randy.randomChance(p)) return false;
            BuildPoolsIfNeeded();
            var pool = isDivine ? s_poolDivine : s_poolArt;
            if (pool == null || pool.Length == 0) return false;
            for (int i = 0; i < 10; i++)
            {
                var t = pool[Randy.randomInt(0, pool.Length - 1)];
                if (t != null && !a.hasTrait(t))
                {
                    a.addTrait(t);
                    string traitName = GetTraitDisplayName(t);
                    string typeName = isDivine ? "神通" : "仙术";
                    LogHist($"[天运·赏赐] {NameOf(a)} 获得{typeName}【{traitName}】");
                    return true;
                }
            }
            return false;
        }
        static string GetTraitDisplayName(ActorTrait t)
        {
            if (t == null) return "未知";
            string id = t.id;
            if (string.IsNullOrEmpty(id)) return "未知";
            string localizedName = LocalizedTextManager.getText("trait_" + id);
            if (!string.IsNullOrEmpty(localizedName) && localizedName != ("trait_" + id))
            {
                return localizedName;
            }
            string translatedName = t.getTranslatedName();
            if (!string.IsNullOrEmpty(translatedName) && translatedName != id)
            {
                return translatedName;
            }
            if (id.StartsWith("divine_")) return id.Substring(7);
            if (id.StartsWith("art_")) return id.Substring(4);
            return id;
        }
        static bool GiveRandomRoot(Actor a)
        {
            BuildPoolsIfNeeded();
            if (s_poolRoots == null || s_poolRoots.Length == 0) return false;
            var t = s_poolRoots[Randy.randomInt(0, s_poolRoots.Length - 1)];
            if (t == null) return false;
            a.addTrait(t);
            LogHist($"[天运·赏赐] {NameOf(a)} 获得灵根");
            return true;
        }
        static bool KillOrClearStonesOrNothing(Actor a)
        {
            float r = Randy.randomFloat(0f, 1f);
            if (r < 0.10f)
            {
                a.changeHealth(-Mathf.FloorToInt(a.getHealth()));
                if (!a.hasHealth()) a.batch.c_check_deaths.Add(a);
                return true;
            }
            if (r < 0.60f)
            {
                a.data.set(KEY_LINGSHI, 0);
                a.data.set(KEY_LINGSHI_SUPREME, 0);
                return true;
            }
            return false; 
        }
        static bool AddStones(Actor a, string key, int add)
        { int cur; a.data.get(key, out cur, 0); a.data.set(key, cur + add); return true; }
        static bool AddTrait(Actor a, string traitId)
        {
            var t = AssetManager.traits.get(traitId) as ActorTrait;
            if (t == null) return false;
            if (a.hasTrait(t)) return false;
            a.addTrait(t);
            return true;
        }
        static bool MindWipe(Actor a)
        {
            if (a == null || !a.isAlive()) return false;
            a.stats["loyalty_traits"] = 0f;
            bool mad = false;
            if (Randy.randomChance(0.5f)) { AddTrait(a, "madness"); mad = true; }
            LogHist($"[天运·反噬] {NameOf(a)} 意识抹除，忠诚清零{(mad ? "，陷入疯狂" : "")}");
            return true;
        }
        static void LogHist(string text)
        {
            if (s_detailBuf == null) return; 
            string piece = ExtractDetail(text, s_detailName);
            if (!string.IsNullOrEmpty(piece))
            {
                if (s_detailBuf.Length > 0) s_detailBuf.Append('；'); 
                s_detailBuf.Append(piece);
            }
        }
        static string NameOf(Actor a)
        {
            var n = a.getName();
            return string.IsNullOrEmpty(n) ? "无名修士" : n;
        }
        static bool FalseSafe() { return false; }
        static void IncTianyunCount(Actor a)
        {
            int c; a.data.get(KEY_TY_COUNT, out c, 0);
            a.data.set(KEY_TY_COUNT, c + 1);
        }
        static void BuildPoolsIfNeeded()
        {
            if (s_poolDivine != null && s_poolArt != null && s_poolRoots != null) return;
            var list = AssetManager.traits.list;
            var div = new List<ActorTrait>(64);
            var art = new List<ActorTrait>(64);
            var rot = new List<ActorTrait>(16);
            foreach (var t in list)
            {
                if (t == null) continue;
                string id = t.id;
                if (id.StartsWith("divine_")) div.Add(t);
                else if (id.StartsWith("art_")) art.Add(t);
                else if (id.StartsWith("root_")) rot.Add(t);
            }
            s_poolDivine = div.ToArray();
            s_poolArt = art.ToArray();
            s_poolRoots = rot.ToArray();
        }
        static void KillInstant(Actor a)
        {
            if (a != null && a.isAlive())
            {
                int hp = a.getHealth();
                if (hp > 0)
                {
                    a.changeHealth(-hp);
                    if (!a.hasHealth() && a.batch != null) a.batch.c_check_deaths.Add(a);
                }
            }
        }
        static void GetAmbitionFor(Actor a, out bool canSwallow, out int ambAdd, out bool isBeastMadness)
        {
            canSwallow = false;
            ambAdd = 0;
            isBeastMadness = false;
            if (a == null || !a.isAlive()) return;
            if (IsBeast(a)) { isBeastMadness = true; return; }
            int topRealm = -1;
            var list = a.getTraits();
            if (list != null)
            {
                for (int i = 0; i < REALM_IDS.Length; i++)
                {
                    string rid = REALM_IDS[i];
                    foreach (var t in list)
                    {
                        if (t != null && t.id == rid) { if (i > topRealm) topRealm = i; }
                    }
                }
            }
            if (topRealm < 0)
            {
                int star = GetAncientStar(a);
                if (star > 0)
                {
                    if (star >= 4) { canSwallow = false; return; }
                    ambAdd = (star == 1 ? 100 : star == 2 ? 250 : 1000); 
                    canSwallow = true;
                    return;
                }
                canSwallow = true;
                ambAdd = 1;
                return;
            }
            if (topRealm >= 10) { canSwallow = false; return; }
            int[] INC = { 10, 30, 50, 100, 200, 500, 1000, 2000, 2500, 3500 };
            ambAdd = INC[Mathf.Clamp(topRealm, 0, 9)];
            canSwallow = true;
        }
        static int GetAncientStar(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return 0;
            foreach (var t in list)
            {
                if (t == null) continue;
                string id = t.id;
                if (id.StartsWith("ancient_") && id.EndsWith("_star"))
                {
                    int us = id.IndexOf('_');        
                    int us2 = id.IndexOf('_', us + 1); 
                    if (us >= 0 && us2 > us)
                    {
                        string mid = id.Substring(us + 1, us2 - us - 1);
                        int num; if (int.TryParse(mid, out num)) return num;
                    }
                }
            }
            return 0;
        }
    }
}