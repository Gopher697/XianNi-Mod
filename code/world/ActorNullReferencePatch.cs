using HarmonyLib;
using UnityEngine;
namespace xn.world
{
    internal static class ActorNullReferencePatch
    {
        public static void Init(Harmony h)
        {
            var method = AccessTools.Method(typeof(Actor), "u1_checkInside");
            if (method != null)
            {
                h.Patch(method,
                    prefix: new HarmonyMethod(typeof(ActorNullReferencePatch), nameof(Pre_u1_checkInside)));
            }
        }
        private static bool Pre_u1_checkInside(Actor __instance, float pElapsed)
        {
            try
            {
                if (__instance == null)
                {
                    return false; 
                }
                if (__instance.is_inside_boat)
                {
                    if (__instance.inside_boat == null)
                    {
                        __instance.is_inside_boat = false;
                        LogWarning(__instance, "had is_inside_boat=true but inside_boat was null");
                        return false;
                    }
                    if (__instance.inside_boat.actor == null)
                    {
                        __instance.is_inside_boat = false;
                        __instance.inside_boat = null;
                        LogWarning(__instance, "inside_boat.actor was null");
                        return false;
                    }
                    if (__instance.inside_boat.actor.current_tile == null)
                    {
                        __instance.is_inside_boat = false;
                        __instance.inside_boat = null;
                        LogWarning(__instance, "inside_boat.actor.current_tile was null");
                        return false;
                    }
                }
                if (__instance.is_inside_building && __instance.inside_building == null)
                {
                    __instance.is_inside_building = false;
                    LogWarning(__instance, "had is_inside_building=true but inside_building was null");
                }
                return true;
            }
            catch (System.Exception ex)
            {
                if (xn.config.ModConfigHooks.EnableLog)
                {
                    Debug.LogError($"[XN-Patch] Exception in Pre_u1_checkInside for Actor {__instance?.data?.id}: {ex.Message}");
                }
                return false;
            }
        }
        private static void LogWarning(Actor actor, string message)
        {
            if (xn.config.ModConfigHooks.EnableLog && actor != null)
            {
                Debug.LogWarning($"[XN-Patch] Actor {actor.data.id} {message}. Cleaning up.");
            }
        }
    }
}