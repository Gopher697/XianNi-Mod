using UnityEngine;
namespace xn.world
{
    internal static class RuinBuildingAssets
    {
        public const string ID = "xn_building_ruins";
        private static bool _inited;
        public static void InitSafe()
        {
            if (_inited) return;
            if (AssetManager.buildings == null) return;
            if (AssetManager.buildings.get(ID) != null) { _inited = true; return; }
            var src = AssetManager.buildings.get("flame_tower"); 
            if (src == null) {
                Debug.LogError("[XN] source building 'flame_tower' not found.");
                return;
            }
            var a = new BuildingAsset();
            a.id = ID;
            a.main_path = "buildings/";          
            a.sprite_path = "buildings/ruins";   
            a.scale_base = new Vector3(0.15f, 0.15f, 0.15f);
            a.fundament = new BuildingFundament(0, 0, 0, 0);
            a.group = "nature";
            a.kingdom = "nature";
            a.building_type = BuildingType.Building_None;
            a.has_kingdom_color = false;
            a.shadow = src.shadow;
            a.random_flip = false;
            a.city_building = false;
            a.can_be_upgraded = false;
            a.can_be_placed_on_liquid = false;
            a.destroy_on_liquid = false;
            a.remove_ruins = true;
            a.has_ruin_state = true;
            a.has_ruins_graphics = true;
            a.has_sprites_main = true;
            a.has_sprites_ruin = true;
            a.ignore_buildings = false;
            a.check_for_close_building = false;
            a.tower = false;
            a.spawn_units = false;
            a.can_be_demolished = true; 
            a.setAtlasID("buildings");
            if (AssetManager.dynamic_sprites_library != null)
                a.atlas_asset = AssetManager.dynamic_sprites_library.get(a.atlas_id);
            AssetManager.buildings.add(a);
            a.loadBuildingSprites();
            Sprite[] arr = SpriteTextureLoader.getSpriteList(a.sprite_path);
            if (a.building_sprites == null || a.building_sprites.animation_data == null || a.building_sprites.animation_data.Count == 0)
                a.sprites_are_initiated = false;
            _inited = true;
        }
        public static Building PlaceAt(WorldTile tile, bool playSfx = true)
        {
            if (tile == null) return null;
            InitSafe();
            var b = World.world.buildings.addBuilding(ID, tile, pCheckForBuild: true, pSfx: playSfx, pType: BuildPlacingType.New);
            if (b != null) xn.world.RuinQuestSystem.OnRuinPlaced(b); 
            return b;
        }
    }
}