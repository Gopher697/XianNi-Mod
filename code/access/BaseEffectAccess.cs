using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class BaseEffectAccess
    {
        private static readonly FieldInfo SpriteRendererField = AccessTools.Field(typeof(BaseEffect), "sprite_renderer");
        private static readonly FieldInfo SpriteAnimationField = AccessTools.Field(typeof(BaseAnimatedObject), "sprite_animation");
        private static bool _warnedSpriteRenderer;
        private static bool _warnedSpriteAnimation;

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

        public static SpriteAnimation GetSpriteAnimation(BaseAnimatedObject animatedObject)
        {
            if (animatedObject == null) return null;
            if (SpriteAnimationField == null)
            {
                WarnOnce(ref _warnedSpriteAnimation, "[XN] BaseAnimatedObject.sprite_animation field not found; sprite animation lookup failed.");
                return null;
            }
            return SpriteAnimationField.GetValue(animatedObject) as SpriteAnimation;
        }

        public static void SetAnimationLooped(BaseAnimatedObject animatedObject, bool looped)
        {
            var animation = GetSpriteAnimation(animatedObject);
            if (animation != null)
            {
                animation.looped = looped;
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
