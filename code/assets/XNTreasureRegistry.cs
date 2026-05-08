using System;
using System.Collections.Generic;
using HarmonyLib;
namespace xn.assets
{
    public static class XNTreasureDefs
    {
        public const int EQUIP_TYPE_TREASURE_INT = 99;
        public const string SUBTYPE_TREASURE = "treasure";
        public const string GROUP_TREASURE_BASIC = "xn_treasure_basic";
        public const string GROUP_TREASURE_VOID = "xn_treasure_void";
    }
    internal static class XNTreasureRegistry
    {
        private static bool _doneGroups;
        private static bool _doneItems;
        public static void RegisterGroupsIfNeeded()
        {
            if (_doneGroups) return;
            _doneGroups = true;
            if (AssetManager.item_groups.get(XNTreasureDefs.GROUP_TREASURE_BASIC) == null)
                AssetManager.item_groups.add(new ItemGroupAsset { id = XNTreasureDefs.GROUP_TREASURE_BASIC, name = "xn_treasure_group_basic", color = "#BAFFC2" });
            if (AssetManager.item_groups.get(XNTreasureDefs.GROUP_TREASURE_VOID) == null)
                AssetManager.item_groups.add(new ItemGroupAsset { id = XNTreasureDefs.GROUP_TREASURE_VOID,  name = "xn_treasure_group_void",  color = "#FFDDBA" });
        }
        public static void RegisterItemsIfNeeded()
        {
            if (_doneItems) return;
            _doneItems = true;
            EquipmentType treasureType = (EquipmentType)XNTreasureDefs.EQUIP_TYPE_TREASURE_INT;
            add("hunfan",          T("treasure_name_hunfan", "Soul Banner"),                         XNTreasureDefs.GROUP_TREASURE_BASIC, treasureType);
            add("qingtong_jian",   T("treasure_name_qingtong_jian", "Bronze Sword"),                 XNTreasureDefs.GROUP_TREASURE_BASIC, treasureType);
            add("zijin_hulu",      T("treasure_name_zijin_hulu", "Purple-Gold Gourd"),                  XNTreasureDefs.GROUP_TREASURE_BASIC, treasureType);
            add("pangu_fu",        T("treasure_name_pangu_fu", "Pangu Axe"),                    XNTreasureDefs.GROUP_TREASURE_BASIC, treasureType);
            add("qingguang_dun",   T("treasure_name_qingguang_dun", "Azureglow Shield"),                 XNTreasureDefs.GROUP_TREASURE_BASIC, treasureType);
            add("tianni_zhu",      T("treasure_name_tianni_zhu", "Heaven-Defying Bead"),                    XNTreasureDefs.GROUP_TREASURE_VOID,  treasureType);
            add("xianyu_baota",    T("treasure_name_xianyu_baota", "Immortal Jade Pagoda"),                XNTreasureDefs.GROUP_TREASURE_VOID,  treasureType);
            add("mieshen_mao",     T("treasure_name_mieshen_mao", "Godslayer Spear"),                   XNTreasureDefs.GROUP_TREASURE_VOID,  treasureType);
            add("liguang_gong",    T("treasure_name_liguang_gong", "Li Guang's Bow"),                  XNTreasureDefs.GROUP_TREASURE_VOID,  treasureType);
            add("xiuxing_zhixin",  T("treasure_name_xiuxing_zhixin", "Cultivation Starheart"),              XNTreasureDefs.GROUP_TREASURE_VOID,  treasureType);
            static void add(string id, string display, string group, EquipmentType et)
            {
                string finalId = "xn_treasure_" + id;
                if (AssetManager.items.get(finalId) != null) return; 
                var a = new EquipmentAsset();
                a.id = finalId;
                a.group_id = group;
                a.equipment_type = et;
                a.show_in_meta_editor = true;
                a.mod_can_be_given = true;
                a.path_icon = "sutras/" + id;      
                a.path_gameplay_sprite = "sutras/" + id;      
                a.name_class = "item_class_treasure";
                AssetManager.items.add(a);
                    a.unlock(true);
            }
        }
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
    }
    [HarmonyPatch(typeof(ItemGroupLibrary), "init")]
    internal static class Patch_ItemGroupLibrary_init
    {
        static void Postfix() { XNTreasureRegistry.RegisterGroupsIfNeeded(); }
    }
    [HarmonyPatch(typeof(ItemLibrary), "init")]
    internal static class Patch_ItemLibrary_init
    {
        static void Postfix() { XNTreasureRegistry.RegisterItemsIfNeeded(); }
    }
}
