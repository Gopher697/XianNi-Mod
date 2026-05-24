using UnityEngine;

namespace xn.assets
{
    internal static class XNGameplaySpriteFrames
    {
        public static Sprite[] Load(string spritePath, string warningContext)
        {
            Sprite[] frames = SpriteTextureLoader.getSpriteList(spritePath, false);
            if (frames != null && frames.Length > 0)
            {
                return frames;
            }

            Sprite single = SpriteTextureLoader.getSprite(spritePath);
            if (single != null)
            {
                return new[] { single };
            }

            Debug.LogWarning($"[XN] gameplay_sprites empty for {warningContext}, avatar rendering may crash");
            return null;
        }
    }
}
