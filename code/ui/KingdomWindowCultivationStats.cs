using xn.world;
namespace xn.ui
{
    internal static class KingdomWindowCultivationStats
    {
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        public static void Post_showStatsRows(KingdomWindow __instance)
        {
            var kingdom = SelectedMetas.selected_kingdom;
            if (kingdom == null || __instance == null || kingdom.isRekt()) return;
            var container = xn.access.StatsRowsContainerAccess.GetStatsRowsContainer(__instance);
            if (container == null) return;
            ShowLevelRow(container, kingdom);
            ShowAuraRow(container, kingdom);
            CultivationInfoRows.Show(container, kingdom.units, CultivationInfoRows.KingdomRows);
        }
        private static void ShowLevelRow(StatsRowsContainer container, Kingdom kingdom)
        {
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(container, "xn_kingdom_cultivation_level");
            if (row == null) return;
            int level = XiuzhenguoSystem.GetLevel(kingdom);
            var cfg = XiuzhenguoSystem.GetConfig(level);
            string title = T("row_xn_kingdom_cultivation_level", "Cultivation Level");
            row.name_text.text = title;
            row.value.text = T(cfg.localeKey, cfg.name);
            row.icon.gameObject.SetActive(false);
            row.setMetaForTooltip(MetaType.None, -1L, "row_xn_kingdom_cultivation_level_info");
            row.gameObject.SetActive(true);
        }
        private static void ShowAuraRow(StatsRowsContainer container, Kingdom kingdom)
        {
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(container, "xn_kingdom_aura_sum");
            if (row == null) return;
            int auraSum = AuraChunkSystem.SumAuraForKingdom(kingdom);
            string title = T("row_xn_kingdom_aura_sum", "Total Kingdom Aura");
            row.name_text.text = title;
            row.value.text = auraSum.ToString();
            row.icon.gameObject.SetActive(false);
            row.setMetaForTooltip(MetaType.None, -1L, "row_xn_kingdom_aura_sum_info");
            row.gameObject.SetActive(true);
        }
    }
}
