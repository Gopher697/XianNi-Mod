using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WBConsole = WorldBoxConsole.Console;
using xn.Traits;
using xn.world;
namespace xn.world
{
    public static class BroadcastSystem
    {
        private static float _lastCleanupTime;
        private const float CLEANUP_INTERVAL = 300f; 
        public static void Init(Harmony h)
        {
            if (h != null)
            {
                var mUpdate = AccessTools.Method(typeof(MapBox), "Update");
                if (mUpdate != null)
                    h.Patch(mUpdate, postfix: new HarmonyMethod(typeof(BroadcastSystem), nameof(Post_MapBox_Update)));
            }
        }
        private static void Post_MapBox_Update()
        {
            if (!xn.config.ModConfigHooks.EnableBroadcastConsoleLog) return;
            float now = Time.unscaledTime;
            if (now - _lastCleanupTime < CLEANUP_INTERVAL) return;
            _lastCleanupTime = now;
            XNConsoleCleanup.ClearBroadcastLogs();
        }
        public static bool OnTraitAdded(NanoObject target, BaseAugmentationAsset traitAsset)
        {
            return false; 
        }
        public static void HeavenStart(Actor a)
        {
            if (a == null) return;
            PostActor(a, a.getName() + " 正在经历天道的考验");
        }
        public static void HeavenSuccess(Actor a)
        {
            if (a == null) return;
            PostActor(a, a.getName() + " 在天道的考验中成功");
        }
        public static void HeavenSuccessRealm(Actor a, string realmId)
        {
            if (a == null) return;
            string realm = getTraitDisplayName(realmId);
            PostActor(a, a.getName() + " 在天道考验中成功，晋升 " + realm);
        }
        public static void HeavenFail(Actor a)
        {
            if (a == null) return;
            PostActor(a, a.getName() + " 这个逼在天道的考验中失败哈哈哈");
        }
        public static void RealmUp(Actor a, string realmId)
        {
            if (a == null) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            string realm = getTraitDisplayName(realmId);
            PostActor(a, a.getName() + " 突破了 " + realm);
        }
        public static void RealmFailDemote(Actor a, string realmId)
        {
            if (a == null) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            string realm = getTraitDisplayName(realmId);
            PostActor(a, a.getName() + " 突破失败跌落至 " + realm + " 了哈哈哈");
        }
        public static void AncientUp(Actor a, int star)
        {
            if (a == null) return;
            if (star < 3) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            PostActor(a, a.getName() + " 的古神之躯突破到 " + star + " 星");
        }
        public static void BeastUp(Actor a, int stage)
        {
            if (a == null) return;
            if (stage < 3) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            PostActor(a, a.getName() + " 的妖兽道行突破到 " + stage + " 阶");
        }
        public static void IntentGain(Actor a, string intentId)
        {
            if (a == null) return;
            string n = getTraitDisplayName(intentId);
            if (string.IsNullOrEmpty(n)) n = intentId;
            PostActor(a, a.getName() + " 领悟了 " + n + " 意境");
        }
        public static void IntentComprehendFail(Actor a)
        {
            if (a == null) return;
            PostActor(a, a.getName() + " 尝试领悟意境失败了");
        }
        public static void RuinExploreReward(Actor a, string what)
        {
            if (a == null) return;
            string name = a.getName();
            string disp = what;
            int dot = what.IndexOf('·');
            if (dot >= 0 && dot + 1 < what.Length)
            {
                string head = what.Substring(0, dot + 1);
                string tid  = what.Substring(dot + 1);
                string tname = getTraitDisplayName(tid);
                if (!string.IsNullOrEmpty(tname) && tname != tid)
                    disp = head + tname;
            }
            else
            {
                if (what.StartsWith("divine_") || what.StartsWith("art_") ||
                what.StartsWith("realm_")  || what.StartsWith("intent_"))
                {
                    string tname = getTraitDisplayName(what);
                    if (!string.IsNullOrEmpty(tname) && tname != what)
                        disp = tname;
                }
            }
            PostActor(a, name + " 探索遗迹获得了 " + disp);
        }
        public static void PossessionSuccess(Actor src, Actor dst)
        {
            if (src == null || dst == null) return;
            PostActor(dst, src.getName() + " 夺舍成功");
        }
        public static void PossessionFail(Actor src, Actor dst)
        {
            if (src == null || dst == null) return;
            PostActor(src, src.getName() + " 夺舍 " + dst.getName() + " 失败，元婴死亡");
        }
        public static void MentorshipTake(Actor master, Actor appr)
        {
            if (master == null || appr == null) return;
            PostActor(master, master.getName() + " 收了 [" + appr.getName() + "] 为徒");
        }
        public static void MentorshipTrans(Actor master, Actor appr, long gain)
        {
            if (master == null || appr == null) return;
            PostActor(master, master.getName() + " 传功于 " + appr.getName() + "（+" + gain + " 修为）。");
        }
        public static void MentorshipVow(Actor master)
        {
            if (master == null) return;
            PostActor(master, master.getName() + "：你竟敢伤我徒儿！等待我的复仇怒火吧！");
        }
        public static void MentorshipConsume(Actor master)
        {
            if (master == null) return;
            PostActor(master, master.getName() + " 炼化了其徒弟，寿命与修为皆有所增。");
        }
        public static void Custom(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            post(text);
        }
        public static void TianyunPrepare() { post("天运子讲道，世界众人准备迎接天运"); }
        public static void TianyunRuinBuilt() { post("天运子在世界上建造了遗迹，已经有人准备去探索了"); }
        public static void TianyunRewardSummary(int n) { post(n + "人被天运子赏赐了"); }
        public static void TianyunBacklashSummary(int n) { post(n + "人被天运子反噬了"); }
        public static void PostActor(Actor who, string text)
        {
            if (string.IsNullOrEmpty(text) || who == null) return;
            if (xn.config.ModConfigHooks.EnableBroadcastDisplay)
            {
                WorldTip.showNowTop(text, pTranslate: false);
            }
            if (xn.config.ModConfigHooks.EnableBroadcastConsoleLog)
            {
                WBConsole.ProcessLog(XNConsoleCleanup.Mark(text), "", LogType.Log, DateTime.Now);
            }
            XNHistoryRegistry.LogBroadcastForActor(who, text);
        }
        public static void PostAtTile(WorldTile tile, string text)
        {
            if (string.IsNullOrEmpty(text) || tile == null) return;
            if (xn.config.ModConfigHooks.EnableBroadcastDisplay)
            {
                WorldTip.showNowTop(text, pTranslate: false);
            }
            if (xn.config.ModConfigHooks.EnableBroadcastConsoleLog)
            {
                WBConsole.ProcessLog(XNConsoleCleanup.Mark(text), "", LogType.Log, DateTime.Now);
            }
            XNHistoryRegistry.LogBroadcastAtTile(tile, text);
        }
        private static void post(string text)
        {
            if (xn.config.ModConfigHooks.EnableBroadcastDisplay)
            {
                WorldTip.showNowTop(text, pTranslate: false);
            }
            if (xn.config.ModConfigHooks.EnableBroadcastConsoleLog)
            {
                WBConsole.ProcessLog(XNConsoleCleanup.Mark(text), "", LogType.Log, DateTime.Now);
            }
            XNHistoryRegistry.LogBroadcast(text);
        }
        private static int parseMiddleNumber(string id)
        {
            if (string.IsNullOrEmpty(id)) return -1;
            int i = id.LastIndexOf('_');           
            if (i <= 0) return -1;
            int j = id.LastIndexOf('_', i - 1);    
            if (j < 0) return -1;
            string mid = id.Substring(j + 1, i - j - 1);
            int num;
            if (int.TryParse(mid, out num)) return num;
            return -1;
        }
        private static readonly Dictionary<string,string> _realmNameMap = new Dictionary<string,string>(24) {
            { "realm_01_qi", "凝气" }, { "realm_02_foundation", "筑基" }, { "realm_03_core", "结丹" },
            { "realm_04_nascent", "元婴" }, { "realm_05_deity", "化神" }, { "realm_06_infantchg", "婴变" },
            { "realm_07_wending", "问鼎" }, { "realm_08_kuinie", "魁涅" }, { "realm_09_jingnie", "境涅" },
            { "realm_10_suinie", "髓涅" }, { "realm_11_kongnie", "空涅" }, { "realm_12_kongling", "空灵" },
            { "realm_13_kongxuan", "空玄" }, { "realm_14_gtianzun", "大天尊" }, { "realm_15_half_tatian", "半踏天" },
            { "realm_16_tatian", "踏天" }
        };
        private static string getTraitDisplayName(string id)
        {
            string name = LocalizedTextManager.getText("trait_" + id);
            if (!string.IsNullOrEmpty(name) && name != ("trait_" + id)) return name;
            string n;
            if (_realmNameMap != null && _realmNameMap.TryGetValue(id, out n)) return n;
            if (id.Contains("infant")) return "婴变";
            if (id.Contains("wending")) return "问鼎";
            if (id.Contains("half") && id.Contains("tatian")) return "半踏天";
            if (id.Contains("tatian")) return "踏天";
            return id;
        }
    }
}