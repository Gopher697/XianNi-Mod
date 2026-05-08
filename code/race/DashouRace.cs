using UnityEngine;
namespace xn.race
{
    internal static class DashouRace
    {
        private static bool _inited = false;
        public const string ACTOR_ID = "dashou";
        public const string TEXTURE_PATH = "acots/xianren/main";  
        public const string TEXTURE_BASE_PATH = "acots/xianren/";  
        public const string POWER_ID = "power_spawn_dashou";
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            RegisterActorAsset();
            RegisterGodPower();
        }
        private static void RegisterActorAsset()
        {
            var lib = AssetManager.actor_library;
            if (lib.get(ACTOR_ID) != null) return; 
            var asset = lib.clone(ACTOR_ID, "human");
            if (asset == null)
            {
                Debug.LogError("[XN] DashouRace: Cannot clone from 'human' asset.");
                return;
            }
            asset.has_advanced_textures = false;
            asset.has_baby_form = false;
            asset.use_phenotypes = false; 
            asset.can_have_subspecies = false; 
            asset.unit_other = true;
            asset.damaged_by_ocean = false; 
            if (asset.base_stats != null)
            {
                asset.base_stats.addTag("fast_swimming"); 
            }
            asset.texture_id = ACTOR_ID;
            asset.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" };
            asset.animation_walk_speed = 10f;
            asset.animation_swim = new string[] { "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7", "swim_8", "swim_9" };
            asset.animation_swim_speed = 8f;
            asset.animation_idle = asset.animation_walk;
            asset.animation_idle_speed = 10f;
            asset.name_locale = "actor_dashou";
            if (asset.check_flip == null)
            {
                asset.check_flip = (BaseSimObject _, WorldTile __) => true;
            }
            asset.texture_asset = new ActorTextureSubAsset(TEXTURE_BASE_PATH, asset.has_advanced_textures);
            asset.texture_asset.prevent_unconscious_rotation = asset.prevent_unconscious_rotation;
            asset.texture_asset.render_heads_for_children = asset.render_heads_for_babies;
            ActorTextureSubAssetAccess.SetTextureBasePath(asset.texture_asset, TEXTURE_BASE_PATH);
            asset.texture_asset.texture_path_base = TEXTURE_BASE_PATH;
            asset.texture_asset.texture_path_main = TEXTURE_PATH;
            if (asset.shadow)
            {
                asset.texture_asset.shadow = true;
                asset.texture_asset.shadow_texture = asset.shadow_texture;
                asset.texture_asset.shadow_texture_egg = asset.shadow_texture_egg;
                asset.texture_asset.shadow_texture_baby = asset.shadow_texture_baby;
            }
            asset.texture_asset.preloadSprites(
                pCivTextures: false,  
                pHasBabyForm: false,  
                pAnimationAsset: asset  
            );
            var testSprites = SpriteTextureLoader.getSpriteList(TEXTURE_PATH);
            if (testSprites == null || testSprites.Length == 0)
            {
                Debug.LogError($"[XN] DashouRace: Failed to load textures from path: {TEXTURE_PATH}");
            }
            else
            {
                Debug.Log($"[XN] DashouRace: Successfully loaded {testSprites.Length} sprites from {TEXTURE_PATH}");
            }
            if (asset.shadow && asset.texture_asset.shadow)
            {
                ActorTextureSubAssetAccess.LoadShadow(asset.texture_asset);
            }
            var textureSprites = SpriteTextureLoader.getSpriteList(TEXTURE_PATH);
            if (textureSprites != null && textureSprites.Length > 0)
            {
                ActorAssetAccess.SetCachedSprite(asset, textureSprites[0]);
                asset.icon = "iconHuman"; 
            }
            else
            {
                var iconSprite = SpriteTextureLoader.getSprite("ui/icon/dashou");
                if (iconSprite != null)
                {
                    asset.icon = "dashou";
                    ActorAssetAccess.SetCachedSprite(asset, iconSprite);
                }
                else
                {
                    asset.icon = "iconHuman";
                }
            }
        }
        private static void RegisterGodPower()
        {
            var lib = AssetManager.powers;
            if (lib.get(POWER_ID) != null) return; 
            var template = lib.get("$template_spawn_actor$");
            if (template == null)
            {
                Debug.LogError("[XN] DashouRace: Cannot find '$template_spawn_actor$' template.");
                return;
            }
            var power = new GodPower();
            power.id = POWER_ID;
            power.type = PowerActionType.PowerSpawnActor;
            power.rank = template.rank;
            power.unselect_when_window = template.unselect_when_window;
            power.show_spawn_effect = template.show_spawn_effect;
            power.actor_spawn_height = template.actor_spawn_height;
            power.multiple_spawn_tip = template.multiple_spawn_tip;
            power.show_unit_stats_overview = template.show_unit_stats_overview;
            power.set_used_camera_drag_on_long_move = template.set_used_camera_drag_on_long_move;
            power.actor_asset_id = ACTOR_ID;
            power.name = "God's Enforcers";
            power.path_icon = "ui/icon/dashou"; 
            power.click_action = PowerLibraryAccess.SpawnUnit;
            lib.add(power);
        }
    }
}
