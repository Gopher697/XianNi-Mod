using HarmonyLib;
using NeoModLoader.api;
using UnityEngine;
using ai;
using System.Reflection;
namespace xn
{
    internal class XNMain : BasicMod<XNMain>
    {
        public static readonly string ModId = "xn.worldbox.mod.cultivation";
        protected override void OnModLoad()
        {
            race.NvpuRace.Init();
            race.DashouRace.Init();
            cultivation.StatsCustomRegistry.Init();                      
            Traits.RealmTraitGroup.Init();
            Traits.RealmTraits.Init();
            expand.FanjieKingdomTrait.Init();
            world.CityAuraSystem.Init();
            world.XiuzhenguoSystem.Init();
            ui.XNModTab.Init();
            expand.AudioManager.Init();
            voice.AIVoiceManager.Init();
            var h = new Harmony(ModId);
            voice.AIVoiceBroadcast.Init(h);
            expand.TopPowerDisplay.Init(h);
            var mCityRows = AccessTools.Method(typeof(CityWindow), "showStatsRows");
            if (mCityRows != null)
                h.Patch(mCityRows,
                    postfix: new HarmonyMethod(typeof(CityWindowRowsPatch), nameof(CityWindowRowsPatch.Post_showStatsRows)));
            var mKingdomRows = AccessTools.Method(typeof(KingdomWindow), "showStatsRows");
            if (mKingdomRows != null)
                h.Patch(mKingdomRows,
                    postfix: new HarmonyMethod(typeof(ui.KingdomWindowCultivationStats), nameof(ui.KingdomWindowCultivationStats.Post_showStatsRows)));
            h.PatchAll(Assembly.GetExecutingAssembly()); 
            config.ModConfigHooks.InitializeFromConfig(this.GetConfig());
            cultivation.DamageSystemOverride.Init(h);
            cultivation.DeathSystemOverride.Init(h);
            xn.ui.SkinToneBrushToolDisplayPatch.Init(h);
            race.DashouSystem.Init(h);
            cultivation.IntentSystem.Init(h); 
            xn.world.XNAttackActions.InitStateSystem(h); 
            xn.world.BenyuanSystem.Init(h);
            cultivation.StatusEffectRealmLimitPatch.Init(h); 
            xn.world.RuinQuestSystem.Init(h);
            xn.world.RuinDestroyJob.Init(h);
            xn.expand.BuildingProtectionSystem.Init(h);
            xn.world.TianyunSystem.Init(h); 
            cultivation.ai.PossessionJob.Init(h);
            xn.world.MentorshipJob.Init(h);
            xn.world.ReincarnationSystem.Init(h);
            cultivation.ai.MainCharacterSystem.Init(h);
            xn.expand.RealmAttackSkillSystem.Init(h);
            xn.expand.TatianProjectileSkill.Init(h);
            xn.world.GarbageCollectorSystem.Init(h);
            xn.world.ActorNullReferencePatch.Init(h);
            xn.world.KingdomLoadDataPatch.Init(h);
            xn.world.ActorLoadFromSavePatch.Init(h);
            cultivation.ai.BreakthroughJob.Init(); 
            cultivation.ai.CondenseRootJob.Init(); 
            cultivation.ai.DemonicHuntJob.Init(); 
            xn.assets.XNTreasureRegistry.RegisterGroupsIfNeeded();
            xn.assets.XNTreasureRegistry.RegisterItemsIfNeeded();
            world.ShentongFX.Register();
            DuoSheFX.Register();
            YijingFX.Register();
            cultivation.BlackHoleSkill.Register();
            world.HeavenTrialFX.Register();
            xn.world.BroadcastSystem.Init(h);
            xn.world.XNHistoryRegistry.Init(); 
            xn.ui.RealmTitleDisplay.Init();
            world.TitleSystem.Init();
            ui.TooltipRegistry.Init();
            UnityEngine.Debug.Log("[XN] Harmony patched (assembly)");
            var mUWRows = AccessTools.Method(typeof(UnitWindow), "showStatsRows");
            if (mUWRows != null)
            {
                var harmonyMethod = new HarmonyMethod(typeof(UnitWindowPatch), nameof(UnitWindowPatch.ShowStatsRowsPostfix));
                harmonyMethod.priority = Priority.Last; 
                h.Patch(mUWRows, postfix: harmonyMethod);
            }
            xn.version.OnlineVersionChecker.CheckOnLoad();
        }
    }
    internal static class CityWindowRowsPatch
    {
        private static PropertyInfo _pStatsContainer;
        private static MethodInfo _mGetRow;
        public static void Post_showStatsRows(CityWindow __instance)
        {
            var city = SelectedMetas.selected_city;
            if (city == null || __instance == null) return;
            int aura; city.data.get(xn.world.CityAuraSystem.KeyAura, out aura, 0);
            if (_pStatsContainer == null)
                _pStatsContainer = AccessTools.Property(typeof(StatsWindow), "stats_rows_container");
            var container = _pStatsContainer?.GetValue(__instance, null) as StatsRowsContainer;
            if (container == null) return;
            if (_mGetRow == null)
                _mGetRow = AccessTools.Method(typeof(StatsRowsContainer), "getStatRow", new[] { typeof(string) });
            var row = _mGetRow?.Invoke(container, new object[] { "xn_city_aura" }) as KeyValueField;
            if (row == null) return;
            string title = LocalizedTextManager.getText("row_xn_city_aura");
            if (string.IsNullOrEmpty(title) || title == "row_xn_city_aura") title = "Aura";
            row.name_text.text = title;
            row.value.text = aura.ToString();
            var sp = SpriteTextureLoader.getSprite("ui/icon/lingqi")
                  ?? SpriteTextureLoader.getSprite("zhanwei");
            if (sp != null && row.icon != null) row.icon.sprite = sp;
            row.gameObject.SetActive(true);
        }
    }
    internal static class UnitWindowPatch
    {
        public static void ShowStatsRowsPostfix(UnitWindow __instance)
        {
            var actor = __instance != null ? xn.access.UnitWindowAccess.GetActor(__instance) : null;
            if (actor == null) return;
            ui.UnitWindowStatsIcon.Initialize(__instance);
            ui.UnitWindowStatsIcon.OnEnable(__instance, actor);
        }
    }
    [HarmonyPatch(typeof(UnitWindow), "OnDisable")]
    internal static class UnitWindowDisablePatch
    {
        [HarmonyPostfix]
        public static void Postfix(UnitWindow __instance)
        {
            ui.UnitWindowStatsIcon.OnDisable(__instance);
        }
    }
}