using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using NeoModLoader.api;
using NeoModLoader.ui;
using xn.bloodline;
using xn.tournament;
using xn.newbie;
namespace xn.ui
{
    public static class XNModTab
    {
        private static bool _inited;
        private static PowersTab _tab;
        public static PowerButton BtnAura;
        public static PowerButton BtnXiuzhenguo;
        public static PowerButton BtnRuins;
        public static PowerButton BtnRanking;
        public static PowerButton BtnSearch;
        public static PowerButton BtnBloodline;
        public static PowerButton BtnTournament;
        public static PowerButton BtnModSettings;
        public static PowersTab Tab => _tab;
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }

        public static void Init()
        {
            if (_inited) return; _inited = true;
            var tabIcon = SpriteTextureLoader.getSprite("ui/icon/icontab")
                        ?? SpriteTextureLoader.getSprite("ui/icons/iconTab");
            _tab = TabManager.CreateTab("xn_tab_root", "xn_tab_root", "xn_tab_root_desc", tabIcon);
            _tab.SetLayout(new List<string> { "tools" });
            var btnIcon = SpriteTextureLoader.getSprite("ui/icon/lingqi")
                      ?? SpriteTextureLoader.getSprite("ui/icons/iconBook");
            var btn = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_aura_toggle", "Aura Display"), OnToggleAura, btnIcon);
            _tab.AddPowerButton("tools", btn);
            BtnAura = btn;
            var tip = btn.GetComponent<TipButton>() ?? btn.gameObject.AddComponent<TipButton>();
            tip.textOnClick            = "btn_xn_aura_toggle";
            tip.textOnClickDescription = "btn_xn_aura_toggle_desc";
            AuraAddBrushTool.Init();
            var auraAddIcon = SpriteTextureLoader.getSprite("GameResources/ui/icon/lingqiadd")
                           ?? SpriteTextureLoader.getSprite("ui/icon/lingqiadd");
            var btnAuraAdd = AuraAddBrushTool.GetPowerButton();
            if (btnAuraAdd != null)
            {
                _tab.AddPowerButton("tools", btnAuraAdd);
                var tipAuraAdd = btnAuraAdd.GetComponent<TipButton>() ?? btnAuraAdd.gameObject.AddComponent<TipButton>();
                tipAuraAdd.textOnClick = "btn_xn_aura_add_brush";
                tipAuraAdd.textOnClickDescription = "btn_xn_aura_add_brush_desc";
            }
            var xzgIcon = SpriteTextureLoader.getSprite("ui/icon/xiuzhenguo")
                       ?? SpriteTextureLoader.getSprite("ui/icons/iconKingdoms");
            var btnXzg = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_xiuzhenguo_toggle", "Kingdom Label"), OnToggleXiuzhenguo, xzgIcon);
            _tab.AddPowerButton("tools", btnXzg);
            BtnXiuzhenguo = btnXzg;
            var tipXzg = btnXzg.GetComponent<TipButton>() ?? btnXzg.gameObject.AddComponent<TipButton>();
            tipXzg.textOnClick = "btn_xn_xiuzhenguo_toggle";
            tipXzg.textOnClickDescription = "btn_xn_xiuzhenguo_toggle_desc";
            var icon = SpriteTextureLoader.getSprite("ui/icon/ruins")
               ?? SpriteTextureLoader.getSprite("ui/icons/iconTemple");
            var btnRuin = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_place_ruins", "Place Ruins"), OnPlaceRuinsOnce, icon);
            _tab.AddPowerButton("tools", btnRuin);
            BtnRuins = btnRuin;
            var tipRuin = btnRuin.GetComponent<TipButton>() ?? btnRuin.gameObject.AddComponent<TipButton>();
            tipRuin.textOnClick = "btn_xn_place_ruins";
            tipRuin.textOnClickDescription = "btn_xn_place_ruins_desc";
            var chartsIcon = SpriteTextureLoader.getSprite("ui/icon/charts")
                ?? SpriteTextureLoader.getSprite("ui/icons/iconCompareStatistics"); 
            var btnRank = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_power_rank", "Power Ranking"), OnOpenRanking, chartsIcon);
            _tab.AddPowerButton("tools", btnRank);
            BtnRanking = btnRank;
            var tipRank = btnRank.GetComponent<TipButton>() ?? btnRank.gameObject.AddComponent<TipButton>();
            tipRank.textOnClick = "btn_xn_power_rank";
            tipRank.textOnClickDescription = "btn_xn_power_rank_desc";
            var gcIcon = SpriteTextureLoader.getSprite("ui/icon/gc")
                ?? SpriteTextureLoader.getSprite("ui/icons/iconTrash")
                ?? SpriteTextureLoader.getSprite("ui/icons/iconBomb");
            var btnGC = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_manual_gc", "Free Memory (GC)"), OnManualGC, gcIcon);
            _tab.AddPowerButton("tools", btnGC);
            var tipGC = btnGC.GetComponent<TipButton>() ?? btnGC.gameObject.AddComponent<TipButton>();
            tipGC.textOnClick = "btn_xn_manual_gc";
            tipGC.textOnClickDescription = "btn_xn_manual_gc_desc";
            var yxIcon = SpriteTextureLoader.getSprite("ui/icon/yexin")
                ?? SpriteTextureLoader.getSprite("ui/icons/iconBook");
            var btnAmb = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_add_ambition", "Add Ambition"), OnAddAmbition, yxIcon);
            _tab.AddPowerButton("tools", btnAmb);
            var tipAmb = btnAmb.GetComponent<TipButton>() ?? btnAmb.gameObject.AddComponent<TipButton>();
            tipAmb.textOnClick = "btn_xn_add_ambition";
            tipAmb.textOnClickDescription = "btn_xn_add_ambition_desc";
            var searchIcon = SpriteTextureLoader.getSprite("ui/icon/search");
            var btnSearch = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_search_units", "Unit Search"), OnSearchUnits, searchIcon);
            _tab.AddPowerButton("tools", btnSearch);
            BtnSearch = btnSearch;
            var tipSearch = btnSearch.GetComponent<TipButton>() ?? btnSearch.gameObject.AddComponent<TipButton>();
            tipSearch.textOnClick = "btn_xn_search_units";
            tipSearch.textOnClickDescription = "btn_xn_search_units_desc";
            var modsysIcon = SpriteTextureLoader.getSprite("ui/icon/modsystem");
            var btnModSettings = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_open_modsettings", "Ergenverse Settings"), OnOpenModSettings, modsysIcon);
            _tab.AddPowerButton("tools", btnModSettings);
            BtnModSettings = btnModSettings;
            var tipMod = btnModSettings.GetComponent<TipButton>() ?? btnModSettings.gameObject.AddComponent<TipButton>();
            tipMod.textOnClick = "btn_xn_open_modsettings";
            tipMod.textOnClickDescription = "btn_xn_open_modsettings_desc";
            var nvpuIcon = SpriteTextureLoader.getSprite("GameResources/ui/icon/nvpu")
                        ?? SpriteTextureLoader.getSprite("ui/icon/nvpu");
            var btnNvpu = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_place_nvpu", "Place Maid"), OnPlaceNvpu, nvpuIcon);
            _tab.AddPowerButton("tools", btnNvpu);
            var tipNvpu = btnNvpu.GetComponent<TipButton>() ?? btnNvpu.gameObject.AddComponent<TipButton>();
            tipNvpu.textOnClick = "btn_xn_place_nvpu";
            tipNvpu.textOnClickDescription = "btn_xn_place_nvpu_desc";
            var dashouIcon = SpriteTextureLoader.getSprite("ui/icon/dashou")
                        ?? SpriteTextureLoader.getSprite("ui/icons/iconHuman");
            var btnDashou = NeoModLoader.General.PowerButtonCreator.CreateGodPowerButton(xn.race.DashouRace.POWER_ID, dashouIcon);
            _tab.AddPowerButton("tools", btnDashou);
            var tipDashou = btnDashou.GetComponent<TipButton>() ?? btnDashou.gameObject.AddComponent<TipButton>();
            tipDashou.textOnClick = "btn_xn_place_dashou";
            tipDashou.textOnClickDescription = "btn_xn_place_dashou_desc";
            var kongzhiIcon = SpriteTextureLoader.getSprite("ui/icon/kongzhi")
                        ?? SpriteTextureLoader.getSprite("ui/icons/iconBook");
            var btnControl = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_control_dashou", "Control Thugs"), OnControlDashou, kongzhiIcon);
            _tab.AddPowerButton("tools", btnControl);
            var tipControl = btnControl.GetComponent<TipButton>() ?? btnControl.gameObject.AddComponent<TipButton>();
            tipControl.textOnClick = "btn_xn_control_dashou";
            tipControl.textOnClickDescription = "btn_xn_control_dashou_desc";
            SkinToneBrushTool.Init();
            var skintoneIcon = SpriteTextureLoader.getSprite("GameResources/ui/icon/skintone")
                            ?? SpriteTextureLoader.getSprite("ui/icon/skintone");
            var btnSkinTone = SkinToneBrushTool.GetPowerButton();
            if (btnSkinTone != null)
            {
                _tab.AddPowerButton("tools", btnSkinTone);
                var tipSkinTone = btnSkinTone.GetComponent<TipButton>() ?? btnSkinTone.gameObject.AddComponent<TipButton>();
                tipSkinTone.textOnClick = "btn_xn_skin_tone_brush";
                tipSkinTone.textOnClickDescription = "btn_xn_skin_tone_brush_desc";
            }
            BloodlineSystem.Init();
            BloodlineWindow.Init();
            var bloodlineIcon = SpriteTextureLoader.getSprite("ui/icon/bloodline")
                             ?? SpriteTextureLoader.getSprite("zhanwei");
            var btnBloodline = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_bloodline", "Bloodline"), OnOpenBloodline, bloodlineIcon);
            _tab.AddPowerButton("tools", btnBloodline);
            BtnBloodline = btnBloodline;
            var tipBloodline = btnBloodline.GetComponent<TipButton>() ?? btnBloodline.gameObject.AddComponent<TipButton>();
            tipBloodline.textOnClick = "btn_xn_bloodline";
            tipBloodline.textOnClickDescription = "btn_xn_bloodline_desc";
            ServantBrushTool.Init();
            var servantIcon = SpriteTextureLoader.getSprite("ui/icon/servant");
            var btnServant = ServantBrushTool.GetPowerButton();
            if (btnServant != null)
            {
                _tab.AddPowerButton("tools", btnServant);
                var tipServant = btnServant.GetComponent<TipButton>() ?? btnServant.gameObject.AddComponent<TipButton>();
                tipServant.textOnClick = "btn_xn_servant_brush";
                tipServant.textOnClickDescription = "btn_xn_servant_brush_desc";
            }
            MasterApprenticeBrushTool.Init();
            var masterIcon = SpriteTextureLoader.getSprite("ui/icon/master");
            var btnMaster = MasterApprenticeBrushTool.GetPowerButton();
            if (btnMaster != null)
            {
                _tab.AddPowerButton("tools", btnMaster);
                var tipMaster = btnMaster.GetComponent<TipButton>() ?? btnMaster.gameObject.AddComponent<TipButton>();
                tipMaster.textOnClick = "btn_xn_master_apprentice_brush";
                tipMaster.textOnClickDescription = "btn_xn_master_apprentice_brush_desc";
            }
            PossessionBrushTool.Init();
            var takeawayIcon = SpriteTextureLoader.getSprite("ui/icon/takeaway");
            var btnPossession = PossessionBrushTool.GetPowerButton();
            if (btnPossession != null)
            {
                _tab.AddPowerButton("tools", btnPossession);
                var tipPossession = btnPossession.GetComponent<TipButton>() ?? btnPossession.gameObject.AddComponent<TipButton>();
                tipPossession.textOnClick = "btn_xn_possession_brush";
                tipPossession.textOnClickDescription = "btn_xn_possession_brush_desc";
            }
            ReincarnationBrushTool.Init();
            var transmigrationIcon = SpriteTextureLoader.getSprite("ui/icon/transmigration");
            var btnReincarnation = ReincarnationBrushTool.GetPowerButton();
            if (btnReincarnation != null)
            {
                _tab.AddPowerButton("tools", btnReincarnation);
                var tipReincarnation = btnReincarnation.GetComponent<TipButton>() ?? btnReincarnation.gameObject.AddComponent<TipButton>();
                tipReincarnation.textOnClick = "btn_xn_reincarnation_brush";
                tipReincarnation.textOnClickDescription = "btn_xn_reincarnation_brush_desc";
            }
            TournamentManager.Init();
            var tournamentIcon = SpriteTextureLoader.getSprite("ui/icon/tournament")
                              ?? SpriteTextureLoader.getSprite("ui/icons/iconWar");
            var btnTournament = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_tournament", "Martial Tournament"), OnStartTournament, tournamentIcon);
            _tab.AddPowerButton("tools", btnTournament);
            BtnTournament = btnTournament;
            var tipTournament = btnTournament.GetComponent<TipButton>() ?? btnTournament.gameObject.AddComponent<TipButton>();
            tipTournament.textOnClick = "btn_xn_tournament";
            tipTournament.textOnClickDescription = "btn_xn_tournament_desc";
            MainCharacterBrushTool.Init();
            MainCharacterSelectSfx.Init();
            var mcIcon = SpriteTextureLoader.getSprite("ui/icon/mc");
            var btnMC = MainCharacterBrushTool.GetPowerButton();
            if (btnMC != null)
            {
                _tab.AddPowerButton("tools", btnMC);
                var tipMC = btnMC.GetComponent<TipButton>() ?? btnMC.gameObject.AddComponent<TipButton>();
                tipMC.textOnClick = "btn_xn_main_character_brush";
                tipMC.textOnClickDescription = "btn_xn_main_character_brush_desc";
            }
            TournamentHistoryWindow.Init();
            var historyIcon = SpriteTextureLoader.getSprite("ui/icon/duizhanlishi")
                           ?? SpriteTextureLoader.getSprite("ui/icons/iconWar");
            var btnHistory = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_tournament_history", "Tourney History"), OnOpenTournamentHistory, historyIcon);
            _tab.AddPowerButton("tools", btnHistory);
            var tipHistory = btnHistory.GetComponent<TipButton>() ?? btnHistory.gameObject.AddComponent<TipButton>();
            tipHistory.textOnClick = "btn_xn_tournament_history";
            tipHistory.textOnClickDescription = "btn_xn_tournament_history_desc";
            var guideIcon = SpriteTextureLoader.getSprite("ui/icon/youwan")
                         ?? SpriteTextureLoader.getSprite("ui/icons/iconBook");
            var btnGuide = NeoModLoader.General.PowerButtonCreator.CreateSimpleButton(T("btn_xn_newbie_guide", "Newbie Guide"), OnOpenNewbieGuide, guideIcon);
            _tab.AddPowerButton("tools", btnGuide);
            var tipGuide = btnGuide.GetComponent<TipButton>() ?? btnGuide.gameObject.AddComponent<TipButton>();
            tipGuide.textOnClick = "btn_xn_newbie_guide";
            tipGuide.textOnClickDescription = "btn_xn_newbie_guide_desc";
            _tab.UpdateLayout();
            XNPowerRanking.Init();
        }
        private static void OnOpenRanking()
        {
            xn.voice.AIVoiceBroadcast.OnRankingClicked();
            XNPowerRanking.Open();
        }
        private static void OnToggleAura()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_aura_toggle", "Aura Display"));
            if (xn.world.XiuzhenguoSystem.Visible)
            {
                xn.world.XiuzhenguoSystem.Toggle();
            }
            xn.world.CityAuraSystem.Toggle();
        }
        private static void OnToggleXiuzhenguo()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_xiuzhenguo_toggle", "Kingdom Label"));
            if (xn.world.CityAuraSystem.Visible)
            {
                xn.world.CityAuraSystem.Toggle();
            }
            xn.world.XiuzhenguoSystem.Toggle();
        }
        private static void OnPlaceRuinsOnce()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_place_ruins", "Place Ruins"));
            xn.ui.RuinPlacementTool.BeginOneShot();
        }
        private static void OnManualGC()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_manual_gc", "Free Memory (GC)"));
            xn.voice.VoiceCache.ClearCache();
            xn.world.GarbageCollectorSystem.RunGC("manual_button");
        }
        private static void OnAddAmbition()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_add_ambition", "Add Ambition"));
            int add = xn.config.ModConfigHooks.AmbitionAddValue;
            if (add == 0)
            {
                xn.world.AmbitionSystem.ClearAll();
                int cur0 = xn.world.AmbitionSystem.GetValue();
                xn.world.BroadcastSystem.Custom(string.Format(T("broadcast_xn_ambition_current", "Current Tian Yunzi ambition: {0}"), cur0));
                return;
            }
            if (add < 0) return; 
            xn.world.AmbitionSystem.Add(add);
            int cur = xn.world.AmbitionSystem.GetValue();
            xn.world.BroadcastSystem.Custom(string.Format(T("broadcast_xn_ambition_current", "Current Tian Yunzi ambition: {0}"), cur));
        }
        private static void OnSearchUnits()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_search_units", "Unit Search"));
            string kw = xn.config.ModConfigHooks.UnitSearchKeyword;
            if (string.IsNullOrEmpty(kw)) kw = "";
            xn.ui.XNSearchRanking.Open(kw);
        }
        private static void OnOpenModSettings()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_open_modsettings", "Ergenverse Settings"));
            const string MY_UID = "XIAN_NI_MOD";
            IMod self = null;
            foreach (var m in NeoModLoader.WorldBoxMod.LoadedMods)
            {
                var declare = m?.GetDeclaration();
                if (declare != null && declare.UID == MY_UID) { self = m; break; }
            }
            if (self == null) return;
            var go = self.GetGameObject();
            if (go == null) return;
            var configurable = go.GetComponent<IConfigurable>();
            if (configurable == null) return;
            var cfg = configurable.GetConfig();
            if (cfg == null) return;
            ModConfigureWindow.ShowWindow(cfg);
        }
        private static void OnPlaceNvpu()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_place_nvpu", "Place Maid"));
            NvpuPlacementTool.BeginOneShot();
        }
        private static void OnControlDashou()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_control_dashou", "Control Thugs"));
            xn.race.DashouSystem.ApplyBehaviorToAll();
        }
        private static void OnOpenBloodline()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_bloodline", "Bloodline"));
            BloodlineWindow.Toggle();
        }
        private static void OnStartTournament()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_tournament", "Martial Tournament"));
            if (TournamentManager.IsRunning)
            {
                xn.world.BroadcastSystem.Custom(T("broadcast_xn_tournament_running", "Tournament is already running. Please wait."));
                return;
            }
            TournamentManager.StartTournament();
        }
        private static void OnOpenTournamentHistory()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_tournament_history", "Tourney History"));
            TournamentHistoryWindow.Toggle();
        }
        private static void OnOpenNewbieGuide()
        {
            xn.voice.AIVoiceBroadcast.OnButtonClicked(T("btn_xn_newbie_guide", "Newbie Guide"));
            NewbieGuideSystem.Start();
        }
    }
}
