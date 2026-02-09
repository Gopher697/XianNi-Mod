using System;
using HarmonyLib;
using UnityEngine;
namespace xn.world
{
    internal static class KingdomLoadDataPatch
    {
        public static void Init(Harmony h)
        {
            var method = AccessTools.Method(typeof(Kingdom), "loadData", new System.Type[] { typeof(KingdomData) });
            if (method != null)
            {
                h.Patch(method,
                    prefix: new HarmonyMethod(typeof(KingdomLoadDataPatch), nameof(Pre_loadData)),
                    finalizer: new HarmonyMethod(typeof(KingdomLoadDataPatch), nameof(Finalizer_loadData)));
            }
            var getFounderSpeciesMethod = AccessTools.Method(typeof(Kingdom), "getFounderSpecies");
            if (getFounderSpeciesMethod != null)
            {
                h.Patch(getFounderSpeciesMethod,
                    prefix: new HarmonyMethod(typeof(KingdomLoadDataPatch), nameof(Pre_getFounderSpecies)));
            }
            var getActorAssetMethod = AccessTools.Method(typeof(Kingdom), "getActorAsset");
            if (getActorAssetMethod != null)
            {
                h.Patch(getActorAssetMethod,
                    finalizer: new HarmonyMethod(typeof(KingdomLoadDataPatch), nameof(Finalizer_getActorAsset)));
            }
        }
        private static bool Pre_loadData(Kingdom __instance, KingdomData pData)
        {
            try
            {
                if (__instance != null && __instance.data != null)
                {
                    if (string.IsNullOrEmpty(__instance.data.original_actor_asset))
                    {
                        if (__instance.hasKing() && __instance.king != null)
                        {
                            var kingAsset = __instance.king.getActorAsset();
                            if (kingAsset != null)
                            {
                                __instance.data.original_actor_asset = kingAsset.id;
                                if (xn.config.ModConfigHooks.EnableLog)
                                {
                                    Debug.LogWarning($"[XN-Patch] Kingdom {__instance.data.id} had null original_actor_asset, set from king to '{kingAsset.id}'");
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(__instance.data.original_actor_asset))
                        {
                            __instance.data.original_actor_asset = "human";
                            if (xn.config.ModConfigHooks.EnableLog)
                            {
                                Debug.LogWarning($"[XN-Patch] Kingdom {__instance.data.id} had null original_actor_asset, set to 'human'");
                            }
                        }
                    }
                    var actorAsset = __instance.getActorAsset();
                    if (actorAsset == null)
                    {
                        actorAsset = AssetManager.actor_library.get(__instance.data.original_actor_asset);
                        if (actorAsset == null)
                        {
                            actorAsset = AssetManager.actor_library.get("human");
                            if (actorAsset != null)
                            {
                                __instance.data.original_actor_asset = "human";
                            }
                        }
                        if (actorAsset == null && xn.config.ModConfigHooks.EnableLog)
                        {
                            Debug.LogError($"[XN-Patch] Failed to get ActorAsset for Kingdom {__instance.data.id}, original_actor_asset='{__instance.data.original_actor_asset}'");
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (xn.config.ModConfigHooks.EnableLog)
                {
                    Debug.LogError($"[XN-Patch] Exception in Pre_loadData for Kingdom {__instance?.data?.id}: {ex.Message}\n{ex.StackTrace}");
                }
                return true;
            }
        }
        private static Exception Finalizer_loadData(Kingdom __instance, KingdomData pData, Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                try
                {
                    if (xn.config.ModConfigHooks.EnableLog)
                    {
                        Debug.LogWarning($"[XN-Patch] Caught NullReferenceException in Kingdom.loadData for Kingdom {__instance?.data?.id}: {__exception.Message}");
                    }
                    if (__instance != null && __instance.data != null)
                    {
                        if (string.IsNullOrEmpty(__instance.data.original_actor_asset))
                        {
                            __instance.data.original_actor_asset = "human";
                        }
                        var actorAsset = __instance.getActorAsset();
                        if (actorAsset == null)
                        {
                            actorAsset = AssetManager.actor_library.get(__instance.data.original_actor_asset);
                        }
                        if (actorAsset != null && actorAsset.kingdom_id_civilization != null)
                        {
                            __instance.asset = AssetManager.kingdoms.get(actorAsset.kingdom_id_civilization);
                        }
                        if (__instance.asset == null && xn.config.ModConfigHooks.EnableLog)
                        {
                            Debug.LogWarning($"[XN-Patch] Kingdom {__instance.data.id} asset is still null after fix attempt");
                        }
                        return null;
                    }
                }
                catch (Exception fixEx)
                {
                    if (xn.config.ModConfigHooks.EnableLog)
                    {
                        Debug.LogError($"[XN-Patch] Failed to fix Kingdom.loadData for Kingdom {__instance?.data?.id}: {fixEx.Message}");
                    }
                }
            }
            return __exception;
        }
        private static bool Pre_getFounderSpecies(Kingdom __instance, ref ActorAsset __result)
        {
            if (__instance == null || __instance.data == null)
            {
                __result = null;
                return false; 
            }
            if (string.IsNullOrEmpty(__instance.data.original_actor_asset))
            {
                if (__instance.hasKing() && __instance.king != null)
                {
                    var kingAsset = __instance.king.asset;
                    if (kingAsset != null)
                    {
                        __instance.data.original_actor_asset = kingAsset.id;
                        if (xn.config.ModConfigHooks.EnableLog)
                        {
                            Debug.LogWarning($"[XN-Patch] Kingdom {__instance.data.id} getFounderSpecies: fixed null original_actor_asset from king to '{kingAsset.id}'");
                        }
                        __result = kingAsset;
                        return false; 
                    }
                }
                __instance.data.original_actor_asset = "human";
                if (xn.config.ModConfigHooks.EnableLog)
                {
                    Debug.LogWarning($"[XN-Patch] Kingdom {__instance.data.id} getFounderSpecies: fixed null original_actor_asset to 'human'");
                }
                __result = AssetManager.actor_library.get("human");
                return false; 
            }
            return true;
        }
        private static Exception Finalizer_getActorAsset(Kingdom __instance, ref ActorAsset __result, Exception __exception)
        {
            if (__exception != null)
            {
                if (xn.config.ModConfigHooks.EnableLog)
                {
                    Debug.LogWarning($"[XN-Patch] Kingdom {__instance?.data?.id} getActorAsset exception: {__exception.Message}");
                }
                __result = AssetManager.actor_library.get("human");
                if (__instance != null && __instance.data != null)
                {
                    if (string.IsNullOrEmpty(__instance.data.original_actor_asset))
                    {
                        __instance.data.original_actor_asset = "human";
                    }
                }
                return null;
            }
            if (__result == null && __instance != null)
            {
                if (xn.config.ModConfigHooks.EnableLog)
                {
                    Debug.LogWarning($"[XN-Patch] Kingdom {__instance.data?.id} getActorAsset returned null, using 'human' as fallback");
                }
                __result = AssetManager.actor_library.get("human");
                if (__instance.data != null && string.IsNullOrEmpty(__instance.data.original_actor_asset))
                {
                    __instance.data.original_actor_asset = "human";
                }
            }
            return __exception;
        }
    }
}