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
                var initMethod = AccessTools.Method(typeof(ResourceLibrary), "init");
                if (initMethod != null)
                {
                    harmony.Patch(initMethod, postfix: new HarmonyMethod(typeof(XNResourceRegistry), nameof(PostResourceLibraryInit)));
                    _patchRegistered = true;
                }
                else
                {
                    Debug.LogWarning("[XN] ResourceLibrary.init not found; lingshi resource patch was not registered.");
                }

                var loadSpritesMethod = AccessTools.Method(typeof(ResourceLibrary), "loadSprites");
                if (loadSpritesMethod != null)
                {
                    harmony.Patch(loadSpritesMethod, postfix: new HarmonyMethod(typeof(XNResourceRegistry), nameof(PostResourceLibraryLoadSprites)));
                }
                else
                {
                    Debug.LogWarning("[XN] ResourceLibrary.loadSprites not found; lingshi gameplay sprite refresh was not registered.");
                }
            }

            RegisterIfNeeded();
        }

        private static void PostResourceLibraryInit()
        {
            RegisterIfNeeded();
        }

        private static void PostResourceLibraryLoadSprites()
        {
            RefreshRegisteredAsset();
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

        private static void RefreshRegisteredAsset()
        {
            if (AssetManager.resources == null)
            {
                return;
            }

            ResourceAsset asset = AssetManager.resources.get(LingshiResourceId);
            if (asset != null)
            {
                Configure(asset);
            }
        }

        private static void Configure(ResourceAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            const string spritePath = "stats/lingshi";
            asset.path_gameplay_sprite = spritePath;
            Sprite[] sprites = XNGameplaySpriteFrames.Load(spritePath, "resource 'xn_lingshi'");
            if (sprites != null)
            {
                asset.gameplay_sprites = sprites;
            }
            asset.full_sprite_path = spritePath;
        }
    }
}
