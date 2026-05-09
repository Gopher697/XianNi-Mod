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
        private static readonly string[] REALM_IDS = new[]
        {
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
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        private static bool ShouldBroadcastRealmId(string realmId)
        {
            return ShouldBroadcastRealmIndex(Array.IndexOf(REALM_IDS, realmId));
        }
        private static bool ShouldBroadcastRealmIndex(int realmIndex)
        {
            if (realmIndex < 0) return false;
            int gate = xn.config.ModConfigHooks.BroadcastRealmGate;
            if (gate <= 0) return true;
            if (gate > 13) gate = 13;
            int minRealmIndex = gate + 2;
            return realmIndex >= minRealmIndex;
        }
        private static int MapAncientBeastStageToRealmIndex(int stage)
        {
            switch (stage)
            {
                case 1: return 2;
                case 2: return 4;
                case 3: return 6;
                case 4: return 7;
                case 5: return 8;
                case 6: return 9;
                case 7: return 10;
                case 8: return 11;
                case 9: return 13;
                case 10: return 14;
                default: return -1;
            }
        }

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
            PostActor(a, T("broadcast_heaven_start", "{0} is undergoing the trial of Heaven", a.getName()));
        }
        public static void HeavenSuccess(Actor a)
        {
            if (a == null) return;
            PostActor(a, T("broadcast_heaven_success", "{0} succeeded in the trial of Heaven", a.getName()));
        }
        public static void HeavenSuccessRealm(Actor a, string realmId)
        {
            if (a == null) return;
            if (!ShouldBroadcastRealmId(realmId)) return;
            string realm = getTraitDisplayName(realmId);
            PostActor(a, T("broadcast_heaven_success_realm", "{0} succeeded in the trial of Heaven and advanced to {1}", a.getName(), realm));
        }
        public static void HeavenFail(Actor a)
        {
            if (a == null) return;
            PostActor(a, T("broadcast_heaven_fail", "{0} challenged the trial of Heaven and got humbled", a.getName()));
        }
        public static void RealmUp(Actor a, string realmId)
        {
            if (a == null) return;
            if (!ShouldBroadcastRealmId(realmId)) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            string realm = getTraitDisplayName(realmId);
            PostActor(a, T("broadcast_realm_up", "{0} broke through to {1}", a.getName(), realm));
        }
        public static void RealmFailDemote(Actor a, string realmId)
        {
            if (a == null) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            string realm = getTraitDisplayName(realmId);
            PostActor(a, T("broadcast_realm_fail_demote", "{0} botched their breakthrough and fell back to {1}", a.getName(), realm));
        }
        public static void AncientUp(Actor a, int star)
        {
            if (a == null) return;
            if (star < 3) return;
            if (!ShouldBroadcastRealmIndex(MapAncientBeastStageToRealmIndex(star))) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            PostActor(a, T("broadcast_ancient_up", "{0}'s Ancient God body broke through to {1}-Star", a.getName(), star));
        }
        public static void BeastUp(Actor a, int stage)
        {
            if (a == null) return;
            if (stage < 3) return;
            if (!ShouldBroadcastRealmIndex(MapAncientBeastStageToRealmIndex(stage))) return;
            if (UnityEngine.Random.value >= 0.5f) return; 
            PostActor(a, T("broadcast_beast_up", "{0}'s Beast cultivation broke through to Tier {1}", a.getName(), stage));
        }
        public static void IntentGain(Actor a, string intentId)
        {
            if (a == null) return;
            string n = getTraitDisplayName(intentId);
            if (string.IsNullOrEmpty(n)) n = intentId;
            PostActor(a, T("broadcast_intent_gain", "{0} comprehended {1} Intent", a.getName(), n));
        }
        public static void IntentComprehendFail(Actor a)
        {
            if (a == null) return;
            PostActor(a, T("broadcast_intent_fail", "{0} tried to comprehend Intent and came up empty", a.getName()));
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
            PostActor(a, T("broadcast_ruin_explore", "{0} explored the ruins and obtained {1}", name, disp));
        }
        public static void PossessionSuccess(Actor src, Actor dst)
        {
            if (src == null || dst == null) return;
            PostActor(dst, T("broadcast_possession_success", "{0} successfully possessed the body", src.getName()));
        }
        public static void PossessionFail(Actor src, Actor dst)
        {
            if (src == null || dst == null) return;
            PostActor(src, T("broadcast_possession_fail", "{0} failed to possess {1}; their Nascent Soul perished for it", src.getName(), dst.getName()));
        }
        public static void MentorshipTake(Actor master, Actor appr)
        {
            if (master == null || appr == null) return;
            PostActor(master, T("broadcast_mentorship_take", "{0} took [{1}] as a disciple", master.getName(), appr.getName()));
        }
        public static void MentorshipTrans(Actor master, Actor appr, long gain)
        {
            if (master == null || appr == null) return;
            PostActor(master, T("broadcast_mentorship_trans", "{0} transferred cultivation to {1} (+{2} cultivation)", master.getName(), appr.getName(), gain));
        }
        public static void MentorshipVow(Actor master)
        {
            if (master == null) return;
            PostActor(master, T("broadcast_mentorship_vow", "{0}: You dare harm my disciple! Await my vengeful wrath!", master.getName()));
        }
        public static void MentorshipConsume(Actor master)
        {
            if (master == null) return;
            PostActor(master, T("broadcast_mentorship_consume", "{0} refined their disciple, gaining lifespan and cultivation", master.getName()));
        }
        public static void Custom(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            post(text);
        }
        public static void TianyunPrepare() { post(T("broadcast_tianyun_prepare", "Tian Yunzi is preaching, the world prepares to receive Heavenly Fate")); }
        public static void TianyunRuinBuilt() { post(T("broadcast_tianyun_ruin_built", "Tian Yunzi built ruins in the world, people are preparing to explore")); }
        public static void TianyunRewardSummary(int n) { post(T("broadcast_tianyun_reward_summary", "{0} people were rewarded by Tian Yunzi", n)); }
        public static void TianyunBacklashSummary(int n) { post(T("broadcast_tianyun_backlash_summary", "{0} people were bitten by Tian Yunzi's backlash", n)); }
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
            { "realm_01_qi", "Qi Condensation" }, { "realm_02_foundation", "Foundation Establishment" }, { "realm_03_core", "Core Formation" },
            { "realm_04_nascent", "Nascent Soul" }, { "realm_05_deity", "Soul Formation" }, { "realm_06_infantchg", "Soul Transformation" },
            { "realm_07_wending", "Ascendant" }, { "realm_08_kuinie", "Nirvana Scryer" }, { "realm_09_jingnie", "Nirvana Cleanser" },
            { "realm_10_suinie", "Nirvana Shatterer" }, { "realm_11_kongnie", "Void Nirvana" }, { "realm_12_kongling", "Void Spirit" },
            { "realm_13_kongxuan", "Void Arcanum" }, { "realm_14_gtianzun", "Grand Empyrean" }, { "realm_15_half_tatian", "Half-Step Heaven Trampling" },
            { "realm_16_tatian", "Heaven Trampling" }
        };
        private static string getTraitDisplayName(string id)
        {
            string name = LocalizedTextManager.getText("trait_" + id);
            if (!string.IsNullOrEmpty(name) && name != ("trait_" + id)) return name;
            string n;
            if (_realmNameMap != null && _realmNameMap.TryGetValue(id, out n)) return n;
            if (id.Contains("infant")) return "Soul Transformation";
            if (id.Contains("wending")) return "Ascendant";
            if (id.Contains("half") && id.Contains("tatian")) return "Half-Step Heaven Trampling";
            if (id.Contains("tatian")) return "Heaven Trampling";
            return id;
        }
    }
}
