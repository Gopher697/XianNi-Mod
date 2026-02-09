using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using xn.Traits;
using xn.world;
namespace xn.ui
{
    internal static class KingdomWindowCultivationStats
    {
        private static readonly string[] REALM_IDS = {
            "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
            "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
            "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
            "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian"
        };
        private static readonly string[] REALM_NAMES = {
            "凝气", "筑基", "结丹", "元婴", "化神", "婴变", "问鼎", "窥涅",
            "净涅", "碎涅", "空涅", "空灵", "空玄", "天尊", "半步踏天", "踏天"
        };
        private static readonly string[] ANCIENT_IDS = {
            "ancient_01_star", "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star",
            "ancient_06_star", "ancient_07_star", "ancient_08_star", "ancient_09_star", "ancient_10_star"
        };
        private static readonly string[] ANCIENT_NAMES = {
            "一星", "二星", "三星", "四星", "五星", "六星", "七星", "八星", "九星", "十星"
        };
        private static readonly string[] BEAST_IDS = {
            "beast_01_stage", "beast_02_stage", "beast_03_stage", "beast_04_stage", "beast_05_stage",
            "beast_06_stage", "beast_07_stage", "beast_08_stage", "beast_09_stage", "beast_10_stage"
        };
        private static readonly string[] BEAST_NAMES = {
            "一阶", "二阶", "三阶", "四阶", "五阶", "六阶", "七阶", "八阶", "九阶", "十阶"
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
            ShowCultivatorRow(container, "xn_kingdom_cultivators", "修士", totalCultivators, realmCounts, REALM_NAMES);
            ShowCultivatorRow(container, "xn_kingdom_ancients", "古神", totalAncients, ancientCounts, ANCIENT_NAMES);
            ShowCultivatorRow(container, "xn_kingdom_beasts", "妖兽", totalBeasts, beastCounts, BEAST_NAMES);
        }
        private static void ShowLevelRow(StatsRowsContainer container, Kingdom kingdom)
        {
            var row = container.getStatRow("xn_kingdom_cultivation_level");
            if (row == null) return;
            int level = XiuzhenguoSystem.GetLevel(kingdom);
            var cfg = XiuzhenguoSystem.GetConfig(level);
            string title = LocalizedTextManager.getText("row_xn_kingdom_cultivation_level");
            if (string.IsNullOrEmpty(title) || title == "row_xn_kingdom_cultivation_level") title = "修真国等级";
            row.name_text.text = title;
            row.value.text = cfg.name;
            row.icon.gameObject.SetActive(false);
            row.setMetaForTooltip(MetaType.None, -1L, "row_xn_kingdom_cultivation_level_info");
            row.gameObject.SetActive(true);
        }
        private static void ShowAuraRow(StatsRowsContainer container, Kingdom kingdom)
        {
            var row = container.getStatRow("xn_kingdom_aura_sum");
            if (row == null) return;
            int auraSum = CityAuraSystem.SumAuraFromKingdom(kingdom);
            string title = LocalizedTextManager.getText("row_xn_kingdom_aura_sum");
            if (string.IsNullOrEmpty(title) || title == "row_xn_kingdom_aura_sum") title = "国家灵气强度总和";
            row.name_text.text = title;
            row.value.text = auraSum.ToString();
            row.icon.gameObject.SetActive(false);
            row.setMetaForTooltip(MetaType.None, -1L, "row_xn_kingdom_aura_sum_info");
            row.gameObject.SetActive(true);
        }
        private static void ShowCultivatorRow(StatsRowsContainer container, string rowId, string typeName, int total, int[] counts, string[] names)
        {
            var row = container.getStatRow(rowId);
            if (row == null) return;
            string title = LocalizedTextManager.getText("row_" + rowId);
            if (string.IsNullOrEmpty(title) || title == "row_" + rowId) title = typeName;
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
                        sb.AppendLine($"{names[i]}：{counts[i]}人");
                        hasAny = true;
                    }
                }
                if (!hasAny) sb.AppendLine("暂无");
                return new TooltipData
                {
                    _tip_name = $"{typeName}境界分布",
                    _tip_description = sb.ToString().TrimEnd()
                };
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_" + rowId + "_info", tooltipData);
            row.gameObject.SetActive(true);
        }
    }
}