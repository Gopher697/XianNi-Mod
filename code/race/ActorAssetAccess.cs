using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.race
{
    internal static class ActorAssetAccess
    {
        private static readonly FieldInfo CachedSpriteField = AccessTools.Field(typeof(ActorAsset), "_cached_sprite");

        public static void SetCachedSprite(ActorAsset asset, Sprite sprite)
        {
            if (asset == null) return;

            if (CachedSpriteField == null)
            {
                Debug.LogWarning("[XN] ActorAsset._cached_sprite field not found; cached sprite was not set.");
                return;
            }

            CachedSpriteField.SetValue(asset, sprite);
        }
    }
}
