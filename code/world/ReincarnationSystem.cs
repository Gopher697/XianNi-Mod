using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace xn.world
{
    internal static class ReincarnationSystem
    {
        const string KEY_REINC = "xn.reincarnation.count";
        const string KEY_WUXIN = "xn.stat.wuxin";
        const string KEY_LUCK  = "xn.stat.qiyun";
        const string KEY_ENQUEUED = "xn.reinc.enq";             
        const string KEY_POS_ACTIVE = "xn.possession.active";   
        static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        struct Soul
        {
            public string baseName;
            public int reinc;
            public int wuxin;
            public int luck;
            public int realmIndex; 
            public string snapshot; 
            public string speciesId; 
            public bool favorite; 
            public bool isMainCharacter; 
        }
        static readonly List<Soul> s_pool = new List<Soul>(64);
        const string KEY_REINC_PREV_INFO = "xn.reincarnation.prev_info";
        public static void Init(Harmony h)
        {
            h.Patch(AccessTools.Method(typeof(BabyMaker), "makeBaby",
                new Type[] { typeof(Actor), typeof(Actor), typeof(ActorSex), typeof(bool), typeof(int), typeof(WorldTile), typeof(bool), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(ReincarnationSystem), nameof(Post_BabyMaker_makeBaby)));
        }
        public static void OnEligibleDeath(Actor a)
        {
            if (a == null) return;
            int demonMark; a.data.get(AmbitionSystem.KEY_AMB_DEMON, out demonMark, 0);
            int dragonMark; a.data.get(AmbitionSystem.KEY_AMB_DRAGON, out dragonMark, 0);
            if (demonMark == 1 || dragonMark == 1)
            {
                return; 
            }
            int removed;
            a.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_REMOVED, out removed, 0);
            if (removed == 1)
            {
                return; 
            }
            int isMainChar;
            a.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            bool isMainCharacter = (isMainChar == 1);
            int idx = GetRealmIndex(a);
            bool hasReincarnationIntent = a.hasTrait("intent_07_reincarnation");
            bool shouldEnterPool = false;
            if (isMainCharacter)
            {
                shouldEnterPool = true;
            }
            else if (idx >= 7) 
            {
                shouldEnterPool = true;
            }
            else if (hasReincarnationIntent && idx < 7)
            {
                if (Randy.randomChance(0.15f))
                {
                    shouldEnterPool = true;
                }
            }
            if (!shouldEnterPool) return;
            int enq; a.data.get(KEY_ENQUEUED, out enq, 0);
            if (enq == 1) return; 
            string baseName = ExtractBaseName(a);
            int wx; a.data.get(KEY_WUXIN, out wx, 0);
            int lk; a.data.get(KEY_LUCK,  out lk, 0);
            int rc; a.data.get(KEY_REINC, out rc, 0);
            string snapshot = BuildReincarnationSnapshot(a, idx);
            string speciesId = GetSpeciesId(a);
            bool fav = a.data.favorite;
            s_pool.Add(new Soul { baseName = baseName, reinc = rc, wuxin = wx, luck = lk, realmIndex = idx, snapshot = snapshot, speciesId = speciesId, favorite = fav, isMainCharacter = isMainCharacter });
            a.data.set(KEY_ENQUEUED, 1);
        }
        public static void ForceAddToPool(Actor a)
        {
            if (a == null) return;
            int enq; a.data.get(KEY_ENQUEUED, out enq, 0);
            if (enq == 1) return; 
            int idx = GetRealmIndex(a);
            string baseName = ExtractBaseName(a);
            int wx; a.data.get(KEY_WUXIN, out wx, 0);
            int lk; a.data.get(KEY_LUCK, out lk, 0);
            int rc; a.data.get(KEY_REINC, out rc, 0);
            string snapshot = BuildReincarnationSnapshot(a, idx);
            string speciesId = GetSpeciesId(a);
            bool fav = a.data.favorite;
            int isMainChar;
            a.data.get(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, out isMainChar, 0);
            bool isMainCharacter = (isMainChar == 1);
            s_pool.Add(new Soul { baseName = baseName, reinc = rc, wuxin = wx, luck = lk, realmIndex = idx, snapshot = snapshot, speciesId = speciesId, favorite = fav, isMainCharacter = isMainCharacter });
            a.data.set(KEY_ENQUEUED, 1);
        }
        static void Post_BabyMaker_makeBaby(Actor __result, Actor pParent1, Actor pParent2)
        {
            if (s_pool.Count == 0) return;
            if (__result == null || !__result.isAlive()) return;
            if (!__result.isSapient()) return;
            int matchIndex = -1;
            int mainCharIndex = -1;
            for (int i = 0; i < s_pool.Count; i++)
            {
                if (s_pool[i].isMainCharacter && mainCharIndex < 0)
                {
                    mainCharIndex = i;
                }
                if (matchIndex < 0)
                {
                    matchIndex = i;
                }
            }
            if (mainCharIndex >= 0)
            {
                matchIndex = mainCharIndex;
            }
            if (matchIndex < 0) return;
            var soul = s_pool[matchIndex];
            float chance = soul.isMainCharacter ? 1.0f : GetReincarnationChance(soul.realmIndex);
            if (!Randy.randomChance(chance)) return; 
            int newCount = soul.reinc + 1;
            string suffix = Build世Suffix(newCount);
            string newName = string.IsNullOrEmpty(soul.baseName) ? "无名" + suffix : soul.baseName + suffix;
            __result.setName(newName);
            int wx = soul.wuxin + Randy.randomInt(1, 51); 
            int lk = soul.luck + Randy.randomInt(1, 51);  
            if (wx < 0) wx = 0;
            if (lk < 0) lk = 0;
            __result.data.set(KEY_WUXIN, wx);
            __result.data.set(KEY_LUCK,  lk);
            __result.data.set(KEY_REINC, newCount);
            if (!string.IsNullOrEmpty(soul.snapshot))
            {
                __result.data.set(KEY_REINC_PREV_INFO, soul.snapshot);
            }
            if (soul.favorite)
            {
                __result.data.favorite = true;
            }
            if (soul.isMainCharacter)
            {
                RestoreMainCharacterStatus(__result);
            }
            int last = s_pool.Count - 1;
            s_pool[matchIndex] = s_pool[last];
            s_pool.RemoveAt(last);
        }
        private static void RestoreMainCharacterStatus(Actor newActor)
        {
            if (newActor == null || !newActor.isAlive()) return;
            newActor.data.set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHARACTER, 1);
            newActor.data.set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_LIVES, 3); 
            newActor.data.set(xn.ui.MainCharacterBrushTool.KEY_MAIN_CHAR_REMOVED, 0); 
            if (World.world.map_stats.custom_data == null)
            {
                World.world.map_stats.custom_data = new SaveCustomData();
            }
            World.world.map_stats.custom_data.set("xn.world.main_char_id", newActor.getID());
            if (!newActor.isFavorite())
            {
                newActor.switchFavorite();
            }
            string name = newActor.getName() ?? "未知";
            BroadcastSystem.Custom($"主角{name}轮回转世成功，继续拥有主角光环");
        }
        static float GetReincarnationChance(int realmIndex)
        {
            switch (realmIndex)
            {
                case 7:  return 0.20f; 
                case 8:  return 0.30f; 
                case 9:  return 0.40f; 
                case 10: return 0.50f; 
                case 11: return 0.60f; 
                case 12: return 0.70f; 
                case 13: return 0.80f; 
                case 14: return 0.90f; 
                case 15: return 1.00f; 
                default: return 0.20f; 
            }
        }
        static string ExtractBaseName(Actor a)
        {
            if (a != null)
            {
                a.data.get("xn.title.base_name", out string storedBase, "");
                if (!string.IsNullOrEmpty(storedBase))
                {
                    return RemoveShiSuffix(storedBase);
                }
            }
            string name = a != null ? a.getName() : "";
            if (string.IsNullOrEmpty(name)) return "无名";
            string rest = name.Trim();
            int lastBracket = rest.LastIndexOf(']');
            if (lastBracket >= 0 && lastBracket + 1 < rest.Length)
            {
                rest = rest.Substring(lastBracket + 1).Trim();
            }
            else
            {
                while (true)
                {
                    int startBracket = rest.IndexOf('[');
                    if (startBracket < 0) break;
                    int endBracket = rest.IndexOf(']', startBracket);
                    if (endBracket <= startBracket) break;
                    rest = rest.Substring(0, startBracket) + rest.Substring(endBracket + 1);
                    rest = rest.Trim();
                }
            }
            int dash = rest.IndexOf('-');
            string baseName = dash >= 0 ? rest.Substring(0, dash).Trim() : rest.Trim();
            baseName = RemoveShiSuffix(baseName);
            return string.IsNullOrEmpty(baseName) ? name.Trim() : baseName;
        }
        static string RemoveShiSuffix(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (!name.EndsWith("世")) return name;
            string[] shiPrefixes = new string[]
            {
                "二世", "三世", "四世", "五世", "六世", "七世", "八世", "九世", "十世",
                "十一世", "十二世", "十三世", "十四世", "十五世", "十六世", "十七世", "十八世", "十九世", "二十世",
                "二十一世", "二十二世", "二十三世", "二十四世", "二十五世", "二十六世", "二十七世", "二十八世", "二十九世", "三十世"
            };
            foreach (var prefix in shiPrefixes)
            {
                if (name.EndsWith(prefix))
                {
                    return name.Substring(0, name.Length - prefix.Length).Trim();
                }
            }
            int i = name.Length - 2; 
            while (i >= 0 && char.IsDigit(name[i]))
            {
                i--;
            }
            if (i < name.Length - 2)
            {
                return name.Substring(0, i + 1).Trim();
            }
            return name;
        }
        static string Build世Suffix(int count)
        {
            switch (count)
            {
                case 1: return "二世";  
                case 2: return "三世";  
                case 3: return "四世";  
                case 4: return "五世";
                case 5: return "六世";
                case 6: return "七世";
                case 7: return "八世";
                case 8: return "九世";
                case 9: return "十世";
                case 10: return "十一世";
                default: return count.ToString() + "世";
            }
        }
        static int GetRealmIndex(Actor a)
        {
            int idx = -1;
            var ts = a.getTraits();
            if (ts == null) return -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
                foreach (var t in ts)
                    if (t != null && t.id == REALM_IDS[i]) { if (i > idx) idx = i; }
            return idx;
        }
        static string GetSpeciesId(Actor a)
        {
            if (a == null) return "";
            if (a.asset != null && !string.IsNullOrEmpty(a.asset.id))
            {
                return a.asset.id;
            }
            if (a.data != null && !string.IsNullOrEmpty(a.data.asset_id))
            {
                return a.data.asset_id;
            }
            return "";
        }
        static string BuildReincarnationSnapshot(Actor a, int realmIndex)
        {
            if (a == null) return "";
            long actorId = a.getID();
            string actorName = a.getName();
            string realmName = (realmIndex >= 0 && realmIndex < REALM_IDS.Length) ? REALM_IDS[realmIndex] : "";
            const string KEY_XP = "xn.stat.xiuwei";
            long xp; a.data.get(KEY_XP, out xp, 0L);
            int wuxin; a.data.get(KEY_WUXIN, out wuxin, 0);
            int luck; a.data.get(KEY_LUCK, out luck, 0);
            string kingdomName = a.hasKingdom() ? a.kingdom.name : "";
            string speciesId = GetSpeciesId(a);
            int year = Date.getCurrentYear();
            return $"{actorId}|{actorName}|{realmName}|{xp}|{wuxin}|{luck}|{kingdomName}|{speciesId}|{year}";
        }
        [ThreadStatic]
        private static Actor s_pendingDeathActor = null;
        [HarmonyPatch(typeof(Actor), "die", new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) })]
        private static class Patch_Actor_die
        {
            [HarmonyPrefix]
            private static void Pre_Actor_die(Actor __instance, bool pDestroy, AttackType pType, bool pCountDeath, bool pLogFavorite)
            {
                if (__instance != null && __instance.isAlive())
                {
                    s_pendingDeathActor = __instance;
                }
                else
                {
                    s_pendingDeathActor = null;
                }
            }
            [HarmonyPostfix]
            private static void Post_Actor_die(Actor __instance, bool pDestroy, AttackType pType, bool pCountDeath, bool pLogFavorite)
            {
                if (s_pendingDeathActor != null && s_pendingDeathActor == __instance)
                {
                    if (__instance.isAlive())
                    {
                        s_pendingDeathActor = null;
                        return; 
                    }
                    int enq; __instance.data.get(KEY_ENQUEUED, out enq, 0);
                    if (enq == 1)
                    {
                        s_pendingDeathActor = null;
                        return; 
                    }
                    if (pDestroy && enq == 0)
                    {
                        s_pendingDeathActor = null;
                        return; 
                    }
                    OnEligibleDeath(s_pendingDeathActor);
                    s_pendingDeathActor = null;
                }
            }
        }
    }
}