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
                if (xn.access.ActorAccess.IsInsideBoat(__instance))
                {
                    Boat insideBoat = xn.access.ActorAccess.GetInsideBoat(__instance);
                    if (insideBoat == null)
                    {
                        xn.access.ActorAccess.SetIsInsideBoat(__instance, false);
                        LogWarning(__instance, "had is_inside_boat=true but inside_boat was null");
                        return false;
                    }
                    Actor boatActor = xn.access.ActorSimpleComponentAccess.GetActor(insideBoat);
                    if (boatActor == null)
                    {
                        xn.access.ActorAccess.SetIsInsideBoat(__instance, false);
                        xn.access.ActorAccess.SetInsideBoat(__instance, null);
                        LogWarning(__instance, "inside_boat.actor was null");
                        return false;
                    }
                    if (boatActor.current_tile == null)
                    {
                        xn.access.ActorAccess.SetIsInsideBoat(__instance, false);
                        xn.access.ActorAccess.SetInsideBoat(__instance, null);
                        LogWarning(__instance, "inside_boat.actor.current_tile was null");
                        return false;
                    }
                }
                if (xn.access.ActorAccess.IsInsideBuilding(__instance) && xn.access.ActorAccess.GetInsideBuilding(__instance) == null)
                {
                    xn.access.ActorAccess.SetIsInsideBuilding(__instance, false);
                    LogWarning(__instance, "had is_inside_building=true but inside_building was null");
                }
                return true;
            }
            catch (System.Exception ex)
            {
                if (xn.config.ModConfigHooks.EnableLog)
                {
                    Debug.LogError($"[XN-Patch] Exception in Pre_u1_checkInside for Actor {xn.access.ActorAccess.GetData(__instance)?.id}: {ex.Message}");
                }
                return false;
            }
        }
        private static void LogWarning(Actor actor, string message)
        {
            if (xn.config.ModConfigHooks.EnableLog && actor != null)
            {
                Debug.LogWarning($"[XN-Patch] Actor {xn.access.ActorAccess.GetData(actor).id} {message}. Cleaning up.");
            }
        }
    }
}
