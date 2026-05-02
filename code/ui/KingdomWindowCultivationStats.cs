using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using xn.Traits;
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
        private static readonly string[] REALM_IDS = {
            "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
            "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
            "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
            "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian"
        };
        private static readonly string[] REALM_NAMES = {
            "Qi Condensation", "Foundation Establishment", "Core Formation", "Nascent Soul", "Soul Formation", "Soul Transformation", "Ascendant", "Nirvana Scryer",
            "Nirvana Cleanser", "Nirvana Shatterer", "Void Nirvana", "Void Spirit", "Void Arcanum", "Grand Empyrean", "Half-Step Heaven Trampling", "Heaven Trampling"
        };
        private static readonly string[] ANCIENT_IDS = {
            "ancient_01_star", "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star",
            "ancient_06_star", "ancient_07_star", "ancient_08_star", "ancient_09_star", "ancient_10_star"
        };
        private static readonly string[] ANCIENT_NAMES = {
            "1 Star Ancient God", "2 Star Ancient God", "3 Star Ancient God", "4 Star Ancient God", "5 Star Ancient God",
            "6 Star Ancient God", "7 Star Ancient God", "8 Star Ancient God", "9 Star Ancient God", "10 Star Ancient God"
        };
        private static readonly string[] BEAST_IDS = {
            "beast_01_stage", "beast_02_stage", "beast_03_stage", "beast_04_stage", "beast_05_stage",
            "beast_06_stage", "beast_07_stage", "beast_08_stage", "beast_09_stage", "beast_10_stage"
        };
        private static readonly string[] BEAST_NAMES = {
            "1st Tier Beast", "2nd Tier Beast", "3rd Tier Beast", "4th Tier Beast", "5th Tier Beast",
            "6th Tier Beast", "7th Tier Beast", "8th Tier Beast", "9th Tier Beast", "10th Tier Beast"
        };
        public static void Post_showStatsRows(KingdomWindow __instance)
        {
            var kingdom = SelectedMetas.selected_kingdom;
            if (kingdom == null || __instance == null || kingdom.isRekt()) return;
            var container = __instance.stats_rows_container;
            if (container == null) return;
            int[] realmCounts = new int[REALM_IDS.Length];
            int[] ancientCounts = new int[ANCIENT_IDS.Length];
            int[] beastCounts = new int[BEAST_IDS.Length];
            int totalCultivators = 0;  
            int totalAncients = 0;     
            int totalBeasts = 0;       
            if (kingdom.units != null)
            {
                foreach (var unit in kingdom.units)
                {
                    if (unit == null || !unit.isAlive()) continue;
                    for (int i = REALM_IDS.Length - 1; i >= 0; i--)
                    {
                        if (unit.hasTrait(REALM_IDS[i]))
                        {
                            realmCounts[i]++;
                            totalCultivators++;
                            break;
                        }
                    }
                    for (int i = ANCIENT_IDS.Length - 1; i >= 0; i--)
                    {
                        if (unit.hasTrait(ANCIENT_IDS[i]))
                        {
                            ancientCounts[i]++;
                            totalAncients++;
                            break;
                        }
                    }
                    for (int i = BEAST_IDS.Length - 1; i >= 0; i--)
                    {
                        if (unit.hasTrait(BEAST_IDS[i]))
                        {
                            beastCounts[i]++;
                            totalBeasts++;
                            break;
                        }
                    }
                }
            }
            ShowLevelRow(container, kingdom);
            ShowAuraRow(container, kingdom);
            ShowCultivatorRow(container, "xn_kingdom_cultivators", "Cultivators", totalCultivators, realmCounts, REALM_IDS, REALM_NAMES);
            ShowCultivatorRow(container, "xn_kingdom_ancients", "Ancient Gods", totalAncients, ancientCounts, ANCIENT_IDS, ANCIENT_NAMES);
            ShowCultivatorRow(container, "xn_kingdom_beasts", "Beasts", totalBeasts, beastCounts, BEAST_IDS, BEAST_NAMES);
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
            int auraSum = CityAuraSystem.SumAuraFromKingdom(kingdom);
            string title = T("row_xn_kingdom_aura_sum", "Total Kingdom Aura");
            row.name_text.text = title;
            row.value.text = auraSum.ToString();
            row.icon.gameObject.SetActive(false);
            row.setMetaForTooltip(MetaType.None, -1L, "row_xn_kingdom_aura_sum_info");
            row.gameObject.SetActive(true);
        }
        private static void ShowCultivatorRow(StatsRowsContainer container, string rowId, string typeName, int total, int[] counts, string[] ids, string[] names)
        {
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(container, rowId);
            if (row == null) return;
            string title = T("row_" + rowId, typeName);
            row.name_text.text = title;
            row.value.text = total.ToString();
            row.icon.gameObject.SetActive(false);
            TooltipDataGetter tooltipData = () =>
            {
                var sb = new StringBuilder();
                bool hasAny = false;
                for (int i = counts.Length - 1; i >= 0; i--)
                {
                    if (counts[i] > 0)
                    {
                        string name = T("trait_" + ids[i], names[i]);
                        sb.AppendLine(T("kingdom_stats_count_line", "{0}: {1}", name, counts[i]));
                        hasAny = true;
                    }
                }
                if (!hasAny) sb.AppendLine(T("kingdom_stats_empty", "None"));
                return xn.access.TooltipDataAccess.Create(
                    T("kingdom_stats_distribution_title", "{0} Distribution", title),
                    sb.ToString().TrimEnd());
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_" + rowId + "_info", tooltipData);
            row.gameObject.SetActive(true);
        }
    }
}
