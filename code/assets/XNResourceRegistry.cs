using HarmonyLib;
using UnityEngine;

namespace xn.assets
{
    internal static class XNResourceRegistry
    {
        public const string LingshiResourceId = "xn_lingshi";

        private static bool _patchRegistered;
        private static bool _registered;

        public static void Init(Harmony harmony)
        {
            if (!_patchRegistered && harmony != null)
            {
                var method = AccessTools.Method(typeof(ResourceLibrary), "init");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(XNResourceRegistry), nameof(PostResourceLibraryInit)));
                    _patchRegistered = true;
                }
                else
                {
                    Debug.LogWarning("[XN] ResourceLibrary.init not found; lingshi resource patch was not registered.");
                }
            }

            RegisterIfNeeded();
        }

        private static void PostResourceLibraryInit()
        {
            RegisterIfNeeded();
        }

        public static void RegisterIfNeeded()
        {
            if (_registered)
            {
                return;
            }
            if (AssetManager.resources == null)
            {
                return;
            }
            if (AssetManager.resources.get(LingshiResourceId) != null)
            {
                _registered = true;
                return;
            }

            AssetManager.resources.add(new ResourceAsset
            {
                id = LingshiResourceId,
                path_icon = "iconResGold",
                path_gameplay_sprite = "gold",
                full_sprite_path = "items/resources/gold",
                type = ResType.Currency
            });
            _registered = true;
        }
    }
}
