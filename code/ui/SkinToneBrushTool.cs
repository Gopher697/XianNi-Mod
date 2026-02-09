using System.Collections.Generic;
using UnityEngine;
namespace xn.ui
{
    public static class SkinToneBrushTool
    {
        private static string _currentPowerId = "xn_skin_tone_brush";
        private static PowerButton _powerButton = null;
        private static readonly Dictionary<int, string> ColorIdToPhenotypeId = new Dictionary<int, string>
        {
            { 0, "skin_black" },      
            { 1, "white_gray" },      
            { 2, "mid_gray" },        
            { 3, "skin_red" },        
            { 4, "bright_orange" },   
            { 5, "skin_yellow" },     
            { 6, "skin_green" },      
            { 7, "bright_teal" },     
            { 8, "skin_blue" },       
            { 9, "skin_purple" },     
            { 10, "skin_pink" },      
            { 11, "wood" },           
            { 12, "desert" },         
            { 13, "bright_yellow" },  
            { 14, "white_gray" },     
            { 15, "dark_blue" }       
        };
        public static void Init()
        {
            CreateSkinToneBrushPower();
        }
        private static void CreateSkinToneBrushPower()
        {
            if (AssetManager.powers.get(_currentPowerId) != null)
            {
                return;
            }
            GodPower template = AssetManager.powers.get("inspect");
            GodPower newPower;
            if (template != null)
            {
                newPower = AssetManager.powers.clone(_currentPowerId, "inspect");
                newPower.name = "btn_xn_skin_tone_brush";
                newPower.path_icon = "ui/icon/skintone";
                newPower.show_tool_sizes = true;
                newPower.allow_unit_selection = false;
                newPower.click_power_brush_action = OnBrushAction;
                newPower.click_action = null;
                newPower.click_brush_action = null;
            }
            else
            {
                newPower = new GodPower
                {
                    id = _currentPowerId,
                    name = "btn_xn_skin_tone_brush",
                    path_icon = "ui/icon/skintone",
                    show_tool_sizes = true,
                    allow_unit_selection = false,
                    click_power_brush_action = OnBrushAction
                };
                AssetManager.powers.add(newPower);
            }
        }
        public static PowerButton GetPowerButton()
        {
            if (_powerButton == null)
            {
                GodPower power = AssetManager.powers.get(_currentPowerId);
                if (power != null)
                {
                    Sprite icon = SpriteTextureLoader.getSprite("GameResources/ui/icon/skintone")
                                  ?? SpriteTextureLoader.getSprite("ui/icon/skintone");
                    _powerButton = NeoModLoader.General.PowerButtonCreator.CreateGodPowerButton(_currentPowerId, icon);
                }
            }
            return _powerButton;
        }
        private static bool OnBrushAction(WorldTile pTile, GodPower pPower)
        {
            int colorIndex = xn.config.ModConfigHooks.SkinToneColorIndex;
            if (!ColorIdToPhenotypeId.TryGetValue(colorIndex, out string phenotypeId))
            {
                UnityEngine.Debug.LogWarning($"[XN] SkinToneBrush: Invalid color index: {colorIndex}");
                return false;
            }
            PhenotypeAsset phenotype = AssetManager.phenotype_library.get(phenotypeId);
            if (phenotype == null)
            {
                UnityEngine.Debug.LogWarning($"[XN] SkinToneBrush: Phenotype not found: {phenotypeId} for color index {colorIndex}");
                return false;
            }
            int phenotypeIndex = phenotype.phenotype_index;
            if (phenotypeIndex == 0)
            {
                UnityEngine.Debug.LogWarning($"[XN] SkinToneBrush: Invalid phenotype_index (0) for phenotype: {phenotypeId}");
                return false;
            }
            BrushData brush = Config.current_brush_data;
            if (brush == null)
            {
                return false;
            }
            World.world.loopWithBrush(pTile, brush, delegate(WorldTile tile, GodPower power)
            {
                tile.doUnits(delegate(Actor actor)
                {
                    if (actor == null || actor.isRekt())
                    {
                        return;
                    }
                    ActorAsset asset = actor.getActorAsset();
                    if (asset == null)
                    {
                        return;
                    }
                    if (!asset.use_phenotypes)
                    {
                        if (asset.has_override_sprite)
                        {
                            string[] unsupportedIds = { "dragon", "zombie_dragon", "worm" };
                            if (System.Array.IndexOf(unsupportedIds, asset.id) >= 0)
                            {
                                return;
                            }
                            SpriteAnimation spriteAnim = actor.getSpriteAnimation();
                            if (spriteAnim != null && spriteAnim.frames != null && spriteAnim.frames.Length > 0)
                            {
                                spriteAnim.phenotype = phenotype;
                                spriteAnim.updateFrame();
                                actor.data.phenotype_index = phenotypeIndex;
                                actor.data.phenotype_shade = Actor.getRandomPhenotypeShade();
                                actor.clearGraphicsFully();
                                actor.setStatsDirty();
                            }
                        }
                        else
                        {
                            return;
                        }
                        if (asset.has_avatar_prefab && actor.avatar != null)
                        {
                            ActorAvatarData avatarData = actor.avatar.GetComponent<ActorAvatarData>();
                            if (avatarData != null)
                            {
                                avatarData.setData(actor);
                            }
                        }
                        return;
                    }
                    if (actor.hasSubspecies())
                    {
                        Subspecies subspecies = actor.subspecies;
                        if (subspecies != null)
                        {
                            if (!subspecies._phenotypes_set_indexes.Contains(phenotypeIndex))
                            {
                                subspecies.cachePhenotype(phenotype);
                            }
                        }
                    }
                    else
                    {
                    }
                    actor.data.phenotype_index = phenotypeIndex;
                    actor.data.phenotype_shade = Actor.getRandomPhenotypeShade();
                    actor.clearGraphicsFully();
                    actor.setStatsDirty();
                    if (asset.has_avatar_prefab && actor.avatar != null)
                    {
                        ActorAvatarData avatarData = actor.avatar.GetComponent<ActorAvatarData>();
                        if (avatarData != null)
                        {
                            avatarData.setData(actor);
                        }
                    }
                });
                return true;
            }, pPower);
            return true;
        }
    }
}