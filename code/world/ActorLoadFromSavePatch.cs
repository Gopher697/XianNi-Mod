using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace xn.world
{
    internal static class ActorLoadFromSavePatch
    {
        public static void Init(Harmony h)
        {
            var method = AccessTools.Method(typeof(Actor), "loadFromSave");
            if (method != null)
            {
                h.Patch(method,
                    prefix: new HarmonyMethod(typeof(ActorLoadFromSavePatch), nameof(Pre_loadFromSave)));
            }
        }
        private static bool Pre_loadFromSave(Actor __instance)
        {
            try
            {
                if (__instance == null)
                {
                    return false; 
                }
                __instance.setStatsDirty();
                TraitTools.loadTraits(__instance, xn.access.ActorAccess.GetData(__instance).saved_traits);
                if (__instance.traits != null && __instance.traits.Count > 0)
                {
                    var traitsSnapshot = __instance.traits.ToList();
                    foreach (ActorTrait trait in traitsSnapshot)
                    {
                        trait.action_on_augmentation_load?.Invoke(__instance, trait);
                    }
                }
                if (__instance.isSapient() && __instance.is_profession_nothing)
                {
                    xn.access.ActorAccess.GetData(__instance).profession = UnitProfession.Unit;
                }
                xn.access.ActorAccess.SetProfession(__instance, xn.access.ActorAccess.GetData(__instance).profession, cancelBeh: false);
                City city = World.world.cities.get(xn.access.ActorAccess.GetData(__instance).cityID);
                Kingdom kingdom = World.world.kingdoms.get(xn.access.ActorAccess.GetData(__instance).civ_kingdom_id);
                if (city != null && !city.isNeutral())
                {
                    xn.access.ActorAccess.SetCity(__instance, city);
                }
                if (kingdom != null)
                {
                    xn.access.ActorAccess.SetKingdom(__instance, kingdom);
                }
                if (__instance.hasEquipment())
                {
                    foreach (ActorEquipmentSlot item2 in __instance.equipment)
                    {
                        if (item2.isEmpty())
                        {
                            continue;
                        }
                        Item item = item2.getItem();
                        int num = 0;
                        while (num < item.data.modifiers.Count)
                        {
                            if (AssetManager.items_modifiers.get(item.data.modifiers[num]) == null)
                            {
                                item.data.modifiers.RemoveAt(num);
                            }
                            else
                            {
                                num++;
                            }
                        }
                    }
                }
                if (xn.access.ActorAccess.GetData(__instance).inventory.isEmpty())
                {
                    xn.access.ActorAccess.GetData(__instance).inventory.empty();
                }
                foreach (Actor parent in __instance.getParents())
                {
                    parent.increaseChildren();
                }
                __instance.asset.action_on_load?.Invoke(__instance);
                return false;
            }
            catch (System.Exception ex)
            {
                if (xn.config.ModConfigHooks.EnableLog)
                {
                    Debug.LogError($"[XN-Patch] Exception in Pre_loadFromSave for Actor {__instance?.data?.id}: {ex.Message}\n{ex.StackTrace}");
                }
                return true;
            }
        }
    }
}