using HarmonyLib;
using UnityEngine;
namespace xn.ui
{
    internal static class TooltipRegistry
    {
        private static bool _inited = false;
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            var tooltipLib = AssetManager.tooltips;
            if (tooltipLib != null)
            {
                var textTooltipAsset = new TooltipAsset
                {
                    id = "xn_text_info",
                    callback = ShowTextTooltip
                };
                tooltipLib.add(textTooltipAsset);
                var tooltipAsset1 = new TooltipAsset
                {
                    id = "row_previous_life_info",
                    callback = ShowPreviousLifeTooltip
                };
                tooltipLib.add(tooltipAsset1);
                var tooltipAsset2 = new TooltipAsset
                {
                    id = "row_reinc_count_info",
                    callback = ShowReincarnationTooltip
                };
                tooltipLib.add(tooltipAsset2);
                var tooltipAsset3 = new TooltipAsset
                {
                    id = "row_tianjiu_bridge_info",
                    callback = ShowTianjiuBridgeTooltip
                };
                tooltipLib.add(tooltipAsset3);
                var tooltipAsset4 = new TooltipAsset
                {
                    id = "row_bloodline_family_info",
                    callback = ShowBloodlineFamilyTooltip
                };
                tooltipLib.add(tooltipAsset4);
                string[] distributionTooltips =
                {
                    "row_xn_kingdom_cultivators_info",
                    "row_xn_kingdom_ancients_info",
                    "row_xn_kingdom_beasts_info",
                    "row_xn_city_cultivators_info",
                    "row_xn_city_ancients_info",
                    "row_xn_city_beasts_info",
                    "row_xn_clan_cultivators_info",
                    "row_xn_clan_ancients_info",
                    "row_xn_clan_beasts_info",
                    "row_xn_family_cultivators_info",
                    "row_xn_family_ancients_info",
                    "row_xn_family_beasts_info",
                    "row_xn_culture_cultivators_info",
                    "row_xn_culture_ancients_info",
                    "row_xn_culture_beasts_info",
                    "row_xn_language_cultivators_info",
                    "row_xn_language_ancients_info",
                    "row_xn_language_beasts_info",
                    "row_xn_religion_cultivators_info",
                    "row_xn_religion_ancients_info",
                    "row_xn_religion_beasts_info",
                    "row_xn_subspecies_cultivators_info",
                    "row_xn_subspecies_ancients_info",
                    "row_xn_subspecies_beasts_info",
                    "row_xn_alliance_cultivators_info",
                    "row_xn_alliance_ancients_info",
                    "row_xn_alliance_beasts_info",
                    "row_xn_army_cultivators_info",
                    "row_xn_army_ancients_info",
                    "row_xn_army_beasts_info",
                    "row_xn_war_cultivators_info",
                    "row_xn_war_ancients_info",
                    "row_xn_war_beasts_info"
                };
                for (int i = 0; i < distributionTooltips.Length; i++)
                {
                    AddCultivationDistributionTooltip(tooltipLib, distributionTooltips[i]);
                }
            }
        }
        private static void AddCultivationDistributionTooltip(TooltipLibrary tooltipLib, string id)
        {
            var tooltipAsset = new TooltipAsset
            {
                id = id,
                callback = ShowCultivationDistributionTooltip
            };
            tooltipLib.add(tooltipAsset);
        }
        private static void ShowTextTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            string tipName = xn.access.TooltipDataAccess.GetTipName(data);
            string tipDescription = xn.access.TooltipDataAccess.GetTipDescription(data);
            if (!string.IsNullOrEmpty(tipName))
            {
                tooltip.name.text = tipName;
            }
            if (!string.IsNullOrEmpty(tipDescription))
            {
                xn.access.TooltipAccess.SetDescription(tooltip, tipDescription);
            }
        }
        private static void ShowPreviousLifeTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            string tipName = xn.access.TooltipDataAccess.GetTipName(data);
            string tipDescription = xn.access.TooltipDataAccess.GetTipDescription(data);
            if (!string.IsNullOrEmpty(tipName))
            {
                tooltip.name.text = tipName;
            }
            if (!string.IsNullOrEmpty(tipDescription))
            {
                xn.access.TooltipAccess.SetDescription(tooltip, tipDescription);
            }
        }
        private static void ShowReincarnationTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            string tipName = xn.access.TooltipDataAccess.GetTipName(data);
            string tipDescription = xn.access.TooltipDataAccess.GetTipDescription(data);
            if (!string.IsNullOrEmpty(tipName))
            {
                tooltip.name.text = tipName;
            }
            if (!string.IsNullOrEmpty(tipDescription))
            {
                xn.access.TooltipAccess.SetDescription(tooltip, tipDescription);
            }
        }
        private static void ShowTianjiuBridgeTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            string tipName = xn.access.TooltipDataAccess.GetTipName(data);
            string tipDescription = xn.access.TooltipDataAccess.GetTipDescription(data);
            if (!string.IsNullOrEmpty(tipName))
            {
                tooltip.name.text = tipName;
            }
            if (!string.IsNullOrEmpty(tipDescription))
            {
                xn.access.TooltipAccess.SetDescription(tooltip, tipDescription);
            }
        }
        private static void ShowBloodlineFamilyTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            string tipName = xn.access.TooltipDataAccess.GetTipName(data);
            string tipDescription = xn.access.TooltipDataAccess.GetTipDescription(data);
            if (!string.IsNullOrEmpty(tipName))
            {
                tooltip.name.text = tipName;
            }
            if (!string.IsNullOrEmpty(tipDescription))
            {
                xn.access.TooltipAccess.SetDescription(tooltip, tipDescription);
            }
        }
        private static void ShowCultivationDistributionTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            string tipName = xn.access.TooltipDataAccess.GetTipName(data);
            string tipDescription = xn.access.TooltipDataAccess.GetTipDescription(data);
            if (!string.IsNullOrEmpty(tipName))
            {
                tooltip.name.text = tipName;
            }
            if (!string.IsNullOrEmpty(tipDescription))
            {
                xn.access.TooltipAccess.SetDescription(tooltip, tipDescription);
            }
        }
    }
}
