using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class BaseEffectAccess
    {
        private static readonly FieldInfo SpriteRendererField = AccessTools.Field(typeof(BaseEffect), "sprite_renderer");
        private static bool _warnedSpriteRenderer;

        public static SpriteRenderer GetSpriteRenderer(BaseEffect effect)
        {
            if (effect == null) return null;
            if (SpriteRendererField == null)
            {
                WarnOnce(ref _warnedSpriteRenderer, "[XN] BaseEffect.sprite_renderer field not found; effect sprite renderer lookup failed.");
                return null;
            }
            return SpriteRendererField.GetValue(effect) as SpriteRenderer;
        }

        public static void SetFlipX(BaseEffect effect, bool value)
        {
            var renderer = GetSpriteRenderer(effect);
            if (renderer != null)
            {
                renderer.flipX = value;
            }
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
