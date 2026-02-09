using System.Collections.Generic;
using HarmonyLib;
namespace xn.stats
{
    [HarmonyPatch(typeof(ActorEquipment), "initDictionary")]
    internal static class Patch_ActorEquipment_initDictionary
    {
        static void Postfix(ActorEquipment __instance)
        {
            var dict = __instance._dictionary;
            if (dict == null) return;
            EquipmentType treasureType = (EquipmentType)xn.assets.XNTreasureDefs.EQUIP_TYPE_TREASURE_INT;
            if (!dict.ContainsKey(treasureType))
            {
                dict.Add(treasureType, new ActorEquipmentSlot(treasureType));
            }
        }
    }
    [HarmonyPatch(typeof(ActorEquipment), "setItem")]
    internal static class Patch_ActorEquipment_setItem_Prefix
    {
        static void Prefix(ActorEquipment __instance, Item pItem, Actor pActor)
        {
            if (pItem == null) return;
            var asset = pItem.getAsset();
            if (asset == null) return;
            EquipmentType treasureType = (EquipmentType)xn.assets.XNTreasureDefs.EQUIP_TYPE_TREASURE_INT;
            if (asset.equipment_type != treasureType) return;
            var dict = __instance._dictionary;
            if (dict == null) return;
            if (!dict.ContainsKey(treasureType))
            {
                dict.Add(treasureType, new ActorEquipmentSlot(treasureType));
            }
        }
    }
    [HarmonyPatch(typeof(ActorEquipment), "load")]
    internal static class Patch_ActorEquipment_load_Prefix
    {
        static void Prefix(ActorEquipment __instance, List<long> pList, Actor pActor)
        {
            if (pList == null || pList.Count == 0) return;
            if (World.world == null || World.world.items == null) return;
            var dict = __instance._dictionary;
            if (dict == null) return;
            EquipmentType treasureType = (EquipmentType)xn.assets.XNTreasureDefs.EQUIP_TYPE_TREASURE_INT;
            bool hasTreasure = false;
            foreach (long tID in pList)
            {
                Item tItem = World.world.items.get(tID);
                if (tItem != null)
                {
                    EquipmentAsset tAsset = tItem.getAsset();
                    if (tAsset != null && tAsset.equipment_type == treasureType)
                    {
                        hasTreasure = true;
                        break;
                    }
                }
            }
            if (hasTreasure && !dict.ContainsKey(treasureType))
            {
                dict.Add(treasureType, new ActorEquipmentSlot(treasureType));
            }
        }
    }
    [HarmonyPatch(typeof(ActorEquipment), "load")]
    internal static class Patch_ActorEquipment_load_Postfix
    {
        static void Postfix(ActorEquipment __instance, List<long> pList, Actor pActor)
        {
            var dict = __instance._dictionary;
            if (dict == null) return;
            EquipmentType treasureType = (EquipmentType)xn.assets.XNTreasureDefs.EQUIP_TYPE_TREASURE_INT;
            if (!dict.ContainsKey(treasureType))
            {
                dict.Add(treasureType, new ActorEquipmentSlot(treasureType));
            }
        }
    }
    [HarmonyPatch(typeof(ActorEquipment), "getSlot")]
    internal static class Patch_ActorEquipment_getSlot_Prefix
    {
        static bool Prefix(ActorEquipment __instance, EquipmentType pType, ref ActorEquipmentSlot __result)
        {
            EquipmentType treasureType = (EquipmentType)xn.assets.XNTreasureDefs.EQUIP_TYPE_TREASURE_INT;
            if (pType != treasureType) return true;
            var dict = __instance._dictionary;
            if (dict == null)
            {
                __instance.initDictionary();
                dict = __instance._dictionary;
                if (dict == null) return true;
            }
            if (!dict.ContainsKey(treasureType))
            {
                dict.Add(treasureType, new ActorEquipmentSlot(treasureType));
            }
            __result = dict[treasureType];
            return false;
        }
    }
}