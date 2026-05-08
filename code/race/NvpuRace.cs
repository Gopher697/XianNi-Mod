using UnityEngine;
namespace xn.race
{
    internal static class NvpuRace
    {
        private static bool _inited = false;
        public const string ACTOR_ID = "nvpu";
        public const string TEXTURE_PATH = "acots/nvpu/main";  
        public const string TEXTURE_BASE_PATH = "acots/nvpu/";  
        public const string POWER_ID = "power_spawn_nvpu";
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
                Debug.LogError("[XN] NvpuRace: Cannot clone from 'human' asset.");
                return;
            }
            asset.has_advanced_textures = false;
            asset.has_baby_form = false;
            asset.use_phenotypes = false; 
            asset.can_have_subspecies = false; 
            asset.unit_other = true;
            asset.texture_id = ACTOR_ID;
            asset.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" };
            asset.animation_walk_speed = 10f;
            asset.animation_idle = asset.animation_walk;
            asset.animation_idle_speed = 10f;
            asset.animation_swim = asset.animation_walk;
            asset.animation_swim_speed = 8f;
            asset.name_locale = "actor_nvpu";
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
                Debug.LogError($"[XN] NvpuRace: Failed to load textures from path: {TEXTURE_PATH}");
            }
            else
            {
                Debug.Log($"[XN] NvpuRace: Successfully loaded {testSprites.Length} sprites from {TEXTURE_PATH}");
                foreach (var animName in asset.animation_walk)
                {
                    bool found = false;
                    foreach (var sprite in testSprites)
                    {
                        if (sprite.name == animName)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Debug.LogWarning($"[XN] NvpuRace: Animation frame '{animName}' not found in loaded sprites. Available sprites: {string.Join(", ", System.Array.ConvertAll(testSprites, s => s.name))}");
                    }
                }
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
                var iconSprite = SpriteTextureLoader.getSprite("ui/icons/nvpu");
                if (iconSprite != null)
                {
                    asset.icon = "nvpu";
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
                Debug.LogError("[XN] NvpuRace: Cannot find '$template_spawn_actor$' template.");
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
            power.name = "Maidservant";
            power.click_action = PowerLibraryAccess.SpawnUnit;
            lib.add(power);
        }
    }
}
