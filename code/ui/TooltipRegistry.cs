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
                var tooltipAsset5 = new TooltipAsset
                {
                    id = "row_xn_kingdom_cultivators_info",
                    callback = ShowCultivationDistributionTooltip
                };
                tooltipLib.add(tooltipAsset5);
                var tooltipAsset6 = new TooltipAsset
                {
                    id = "row_xn_kingdom_ancients_info",
                    callback = ShowCultivationDistributionTooltip
                };
                tooltipLib.add(tooltipAsset6);
                var tooltipAsset7 = new TooltipAsset
                {
                    id = "row_xn_kingdom_beasts_info",
                    callback = ShowCultivationDistributionTooltip
                };
                tooltipLib.add(tooltipAsset7);
            }
        }
        private static void ShowPreviousLifeTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            if (!string.IsNullOrEmpty(data._tip_name))
            {
                tooltip.name.text = data._tip_name;
            }
            if (!string.IsNullOrEmpty(data._tip_description))
            {
                tooltip.setDescription(data._tip_description);
            }
        }
        private static void ShowReincarnationTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            if (!string.IsNullOrEmpty(data._tip_name))
            {
                tooltip.name.text = data._tip_name;
            }
            if (!string.IsNullOrEmpty(data._tip_description))
            {
                tooltip.setDescription(data._tip_description);
            }
        }
        private static void ShowTianjiuBridgeTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            if (!string.IsNullOrEmpty(data._tip_name))
            {
                tooltip.name.text = data._tip_name;
            }
            if (!string.IsNullOrEmpty(data._tip_description))
            {
                tooltip.setDescription(data._tip_description);
            }
        }
        private static void ShowBloodlineFamilyTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            if (!string.IsNullOrEmpty(data._tip_name))
            {
                tooltip.name.text = data._tip_name;
            }
            if (!string.IsNullOrEmpty(data._tip_description))
            {
                tooltip.setDescription(data._tip_description);
            }
        }
        private static void ShowCultivationDistributionTooltip(Tooltip tooltip, string type, TooltipData data)
        {
            if (!string.IsNullOrEmpty(data._tip_name))
            {
                tooltip.name.text = data._tip_name;
            }
            if (!string.IsNullOrEmpty(data._tip_description))
            {
                tooltip.setDescription(data._tip_description);
            }
        }
    }
}