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
            ResourceAsset existing = AssetManager.resources.get(LingshiResourceId);
            if (existing != null)
            {
                Configure(existing);
                _registered = true;
                return;
            }

            ResourceAsset asset = new ResourceAsset
            {
                id = LingshiResourceId,
                path_icon = "iconResGold",
                type = ResType.Currency
            };
            Configure(asset);
            AssetManager.resources.add(asset);
            _registered = true;
        }

        private static void Configure(ResourceAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            asset.path_gameplay_sprite = "stats/lingshi";
            var sprites = SpriteTextureLoader.getSpriteList("stats/lingshi", false);
            if (sprites != null && sprites.Length > 0)
                asset.gameplay_sprites = sprites;
            else
                Debug.LogWarning("[XN] gameplay_sprites empty for resource 'xn_lingshi', avatar rendering may crash");
            asset.full_sprite_path = "stats/lingshi";
        }
    }
}
