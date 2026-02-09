using HarmonyLib;
using UnityEngine;
namespace xn
{
    [HarmonyPatch(typeof(Actor), "calculateMainSprite")]
    internal static class TianyunziSpritePatch
    {
        private static Sprite s_tianyunzi;
        private const string KEY_FLAG_TYZ    = "xn_is_tianyunzi";      
        private const string KEY_BASE_SCALEX = "xn_tyz_base_scale_x";  
        private const string KEY_BASE_SCALEY = "xn_tyz_base_scale_y";
        private const string KEY_EXTRA_PCT   = "xn_tyz_scale_pct";     
        private static readonly string[] PATHS_TYZ = {
            "GameResources/acots/skin/tianyunzi",
            "acots/skin/tianyunzi",
            "actors/skin/tianyunzi"
        };
        private static Sprite LoadTyz()
        {
            foreach (var p in PATHS_TYZ)
            {
                var sp = SpriteTextureLoader.getSprite(p);
                if (sp != null) return sp;
            }
            return null;
        }
        static void Postfix(Actor __instance, ref Sprite __result)
        {
            int isTYZ = 0;
            __instance.data.get(KEY_FLAG_TYZ, out isTYZ, 0);
            if (isTYZ != 1) return;
            if (s_tianyunzi == null)
            {
                s_tianyunzi = LoadTyz();
                if (s_tianyunzi == null) return;
            }
            float baseX = 0f, baseY = 0f;
            __instance.data.get(KEY_BASE_SCALEX, out baseX, 0f);
            __instance.data.get(KEY_BASE_SCALEY, out baseY, 0f);
            if (baseX <= 0f || baseY <= 0f)
            {
                var sc0 = __instance.current_scale;
                baseX = sc0.x; baseY = sc0.y;
                __instance.data.set(KEY_BASE_SCALEX, baseX);
                __instance.data.set(KEY_BASE_SCALEY, baseY);
            }
            var oldSprite = __result != null ? __result : s_tianyunzi;
            float oldH = oldSprite.bounds.size.y;
            float newH = s_tianyunzi.bounds.size.y;
            if (newH > 0f && oldH > 0f)
            {
                float ratio = oldH / newH;
                float extra = 1f;
                __instance.data.get(KEY_EXTRA_PCT, out extra, 1f);
                if (extra < 0.05f) extra = 0.05f;   
                if (extra > 2f)    extra = 2f;      
                __instance.current_scale = new Vector2(baseX * ratio * extra, baseY * ratio * extra);
            }
            __result = s_tianyunzi;
        }
    }
}