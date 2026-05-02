using UnityEngine;
namespace xn.fx
{
    public static class XNStatusFX
    {
        private static bool s_registered;
        private static Material s_material;
        public static void Register()
        {
            if (s_registered) return;
            s_material = LibraryMaterials.instance.dict["mat_world_object_lit"];
            RegisterLoopStatus();
            RegisterOnceEffects();
            s_registered = true;
        }
        private static void RegisterLoopStatus()
        {
            AddLoopStatus("xn_tiandao", "tiandao", "ui/icons/xn_tiandao", 99999f, 1f, -5f);
            AddLoopStatus("xn_yijing_loop", "yijing/tongyong", "ui/icons/xn_yijing", 99999f, 1f, -7f);
            AddLoopStatus("xn_condense_root", "genanim/condense", "ui/icons/xn_condense", 99999f, 1f, -6f);
            AddLoopStatus("xn_territory", "territory", "ui/icons/xn_territory", 99999f, 1f, 0f);
            AddLoopStatus("xn_weiya", "shentong/weiya", "ui/icons/xn_weiya", 99999f, 1f, -6f);
            AddLoopStatus("xn_baonu", "shentong/baonuzhibian", "ui/icons/xn_baonu", 99999f, 1f, -6f);
            AddLoopStatus("xn_xs_ascension", "xianshu/ascension", "ui/icons/xn_ascension", 99999f, 1f, -5f);
            AddLoopStatus("xn_xs_shield", "xianshu/shield", "ui/icons/xn_shield", 99999f, 1f, -5f);
        }
        private static void RegisterOnceEffects()
        {
            var lib = AssetManager.effects_library;
            AddOnceEffect(lib, "fx_duoshe", "yijing/duoshe");
            AddOnceEffect(lib, "fx_benyuan_open", "benyuan/open");
            AddOnceEffect(lib, "fx_yijing_jijing", "yijing/jijing");
            AddOnceEffect(lib, "fx_sanmei", "shentong/sanmeizhenhuo");
            AddOnceEffect(lib, "fx_wanjian", "shentong/wanjianguizong");
            AddOnceEffect(lib, "fx_xuankongpo", "shentong/xuankongpo");
            AddOnceEffect(lib, "fx_zhenkongquan", "shentong/zhenkongquan");
            AddOnceEffect(lib, "fx_jiuyin", "shentong/jiuyinbaiguzhao");
            AddOnceEffect(lib, "fx_duqi", "shentong/duqidan");
            AddOnceEffect(lib, "fx_jianzhan", "shentong/jianzhan");
            AddOnceEffect(lib, "fx_xs_slash", "xianshu/slash");
            AddOnceEffect(lib, "fx_xs_quake", "xianshu/quake");
            AddOnceEffect(lib, "fx_xs_waves", "xianshu/waves");
            AddOnceEffect(lib, "fx_xs_convert", "xianshu/convert");
            AddOnceEffect(lib, "fx_xs_missile", "xianshu/missile");
            AddOnceEffect(lib, "fx_xs_palm", "xianshu/palm");
            AddOnceEffect(lib, "fx_xs_breaker", "xianshu/breaker");
            AddOnceEffect(lib, "fx_xs_link", "xianshu/link");
        }
        private static void AddLoopStatus(string id, string texturePath, string iconPath, float duration, float scale, float offsetY = 0f)
        {
            if (AssetManager.status.has(id)) return;
            var spriteList = SpriteTextureLoader.getSpriteList(texturePath, false);
            if (spriteList == null || spriteList.Length == 0)
            {
                Debug.LogWarning($"[XIANNI] AddLoopStatus: sprite_list is empty for '{id}', path='{texturePath}', skipping registration");
                return;
            }
            var status = new StatusAsset
            {
                id = id,
                texture = texturePath,
                sprite_list = spriteList,
                path_icon = iconPath,
                duration = duration,
                animated = true,
                need_visual_render = true,
                is_animated_in_pause = true,
                can_be_flipped = true,
                loop = true,
                scale = scale,
                render_priority = 5,
                material = s_material,
                allow_timer_reset = true,
                offset_y = offsetY  
            };
            AssetManager.status.add(status);
        }
        private static void AddOnceEffect(EffectsLibrary lib, string id, string spritePath)
        {
            if (lib.has(id)) return;
            lib.add(new EffectAsset
            {
                id = id,
                use_basic_prefab = true,
                sorting_layer_id = "EffectsTop",
                sprite_path = spritePath,
                time_between_frames = 1f / 12f,
                limit = 128
            });
        }
        public static void StartTiandao(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_tiandao");
        }
        public static void StopTiandao(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_tiandao");
        }
        public static void StartYijingLoop(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_yijing_loop");
        }
        public static void StopYijingLoop(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_yijing_loop");
        }
        public static void StartCondenseRoot(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_condense_root");
        }
        public static void StopCondenseRoot(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_condense_root");
        }
        public static void StartTerritory(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_territory");
        }
        public static void StopTerritory(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_territory");
        }
        public static void StartWeiya(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_weiya");
        }
        public static void StopWeiya(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_weiya");
        }
        public static void StartBaonu(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_baonu");
        }
        public static void StopBaonu(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_baonu");
        }
        public static void StartXsAscension(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_xs_ascension");
        }
        public static void StopXsAscension(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_xs_ascension");
        }
        public static void StartXsShield(Actor a)
        {
            if (a == null || !a.isAlive()) return;
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            a.addStatusEffect("xn_xs_shield");
        }
        public static void StopXsShield(Actor a)
        {
            if (a == null) return;
            a.finishStatusEffect("xn_xs_shield");
        }
        private const float FX_Y_OFFSET = -3.5f;
        public static void PlayDuoshe(Actor caster)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (caster == null || !caster.isAlive()) return;
            Register();
            SpawnOnceAt("fx_duoshe", caster.current_position, xn.access.ActorAccess.GetActorScale(caster), FX_Y_OFFSET);
        }
        public static void PlayBenyuanOpen(Actor caster)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (caster == null || !caster.isAlive()) return;
            Register();
            SpawnOnceAt("fx_benyuan_open", caster.current_position, xn.access.ActorAccess.GetActorScale(caster), FX_Y_OFFSET);
        }
        public static void PlayJijing(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_yijing_jijing", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlaySanmei(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_sanmei", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayWanjian(WorldTile center, float scale = 1f)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (center == null) return;
            Register();
            SpawnOnceAt("fx_wanjian", center.pos, scale, FX_Y_OFFSET);
        }
        public static void PlayXuankongpo(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_xuankongpo", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayZhenkongquan(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_zhenkongquan", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayJiuyin(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_jiuyin", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayDuqi(WorldTile center, float scale = 1f)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (center == null) return;
            Register();
            SpawnOnceAt("fx_duqi", center.pos, scale, FX_Y_OFFSET);
        }
        public static void PlayJianzhan(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_jianzhan", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayXsSlash(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_xs_slash", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayXsQuake(WorldTile center, float scale = 1f)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (center == null) return;
            Register();
            SpawnOnceAt("fx_xs_quake", center.pos, scale, FX_Y_OFFSET);
        }
        public static void PlayXsWaves(Actor caster)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (caster == null || !caster.isAlive()) return;
            Register();
            SpawnOnceAt("fx_xs_waves", caster.current_position, xn.access.ActorAccess.GetActorScale(caster), FX_Y_OFFSET);
        }
        public static void PlayXsConvert(WorldTile center, float scale = 1f)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (center == null) return;
            Register();
            SpawnOnceAt("fx_xs_convert", center.pos, scale, FX_Y_OFFSET);
        }
        public static void PlayXsMissile(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_xs_missile", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayXsPalm(WorldTile center, float scale = 1f)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (center == null) return;
            Register();
            SpawnOnceAt("fx_xs_palm", center.pos, scale, FX_Y_OFFSET);
        }
        public static void PlayXsBreaker(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_xs_breaker", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        public static void PlayXsLink(Actor target)
        {
            if (!xn.config.ModConfigHooks.EnableAnimation) return;
            if (target == null || !target.isAlive()) return;
            Register();
            SpawnOnceAt("fx_xs_link", target.current_position, xn.access.ActorAccess.GetActorScale(target), FX_Y_OFFSET);
        }
        private static void SpawnOnceAt(string fxId, Vector2 pos, float scale, float yOffset)
        {
            pos.y += yOffset;
            var effect = EffectsLibrary.spawnAt(fxId, pos, scale);
            if (effect != null && effect.sprite_animation != null)
            {
                effect.sprite_animation.looped = false;
            }
        }
    }
}