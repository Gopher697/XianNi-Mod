using UnityEngine;
using HarmonyLib;
namespace xn.ui
{
    public static class MainCharacterBrushTool
    {
        private static string _currentPowerId = "xn_main_character_brush";
        private static PowerButton _powerButton = null;
        public const string KEY_MAIN_CHARACTER = "xn.main_character"; 
        public const string KEY_MAIN_CHAR_LIVES = "xn.main_char.lives"; 
        public const string KEY_MAIN_CHAR_REMOVED = "xn.main_char.removed"; 
        private const string WORLD_KEY_MAIN_CHAR_ID = "xn.world.main_char_id";
        public static void Init()
        {
            CreateMainCharacterBrushPower();
        }
        private static void CreateMainCharacterBrushPower()
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
                newPower.name = "btn_xn_main_character_brush";
                newPower.path_icon = "ui/icon/mc";
                newPower.show_tool_sizes = false;
                newPower.allow_unit_selection = false;
                newPower.click_power_brush_action = null;
                newPower.click_action = OnClickAction;
                newPower.click_brush_action = null;
            }
            else
            {
                newPower = new GodPower
                {
                    id = _currentPowerId,
                    name = "btn_xn_main_character_brush",
                    path_icon = "ui/icon/mc",
                    show_tool_sizes = false,
                    allow_unit_selection = false,
                    click_action = OnClickAction
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
                    Sprite icon = SpriteTextureLoader.getSprite("ui/icon/mc");
                    _powerButton = NeoModLoader.General.PowerButtonCreator.CreateGodPowerButton(_currentPowerId, icon);
                }
            }
            return _powerButton;
        }
        private static bool OnClickAction(WorldTile pTile, string pPowerID)
        {
            if (pTile == null) return false;
            var selectedButton = World.world.selected_buttons?.selectedButton;
            if (selectedButton == null || selectedButton.godPower == null || selectedButton.godPower.id != _currentPowerId)
            {
                return false;
            }
            Actor target = null;
            pTile.doUnits(delegate(Actor actor)
            {
                if (actor != null && actor.isAlive() && actor.isSapient() && target == null)
                {
                    target = actor;
                }
            });
            if (target == null)
            {
                xn.world.BroadcastSystem.Custom("请点击一个有智慧生物的位置");
                return false;
            }
            int isMainChar;
            target.data.get(KEY_MAIN_CHARACTER, out isMainChar, 0);
            if (isMainChar == 1)
            {
                RemoveMainCharacter(target);
                xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}的主角光环已移除");
                return true;
            }
            else
            {
                SetMainCharacter(target);
                xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}获得了主角光环");
                return true;
            }
        }
        private static void SetMainCharacter(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;
            long currentMainCharId;
            if (World.world.map_stats.custom_data == null)
            {
                World.world.map_stats.custom_data = new SaveCustomData();
            }
            World.world.map_stats.custom_data.get(WORLD_KEY_MAIN_CHAR_ID, out currentMainCharId, 0L);
            if (currentMainCharId > 0 && currentMainCharId != actor.getID())
            {
                var oldMainChar = World.world.units.get(currentMainCharId);
                if (oldMainChar != null && oldMainChar.isAlive())
                {
                    RemoveMainCharacter(oldMainChar);
                }
            }
            actor.data.set(KEY_MAIN_CHARACTER, 1);
            actor.data.set(KEY_MAIN_CHAR_LIVES, 3); 
            actor.data.set(KEY_MAIN_CHAR_REMOVED, 0); 
            if (World.world.map_stats.custom_data == null)
            {
                World.world.map_stats.custom_data = new SaveCustomData();
            }
            World.world.map_stats.custom_data.set(WORLD_KEY_MAIN_CHAR_ID, actor.getID());
            xn.expand.AudioManager.PlayMcSuccess();
            if (!actor.isFavorite())
            {
                actor.switchFavorite();
            }
        }
        private static void RemoveMainCharacter(Actor actor)
        {
            if (actor == null) return;
            actor.data.set(KEY_MAIN_CHARACTER, 0);
            actor.data.set(KEY_MAIN_CHAR_LIVES, 0);
            actor.data.set(KEY_MAIN_CHAR_REMOVED, 1);
            if (World.world.map_stats.custom_data != null)
            {
                long currentMainCharId;
                World.world.map_stats.custom_data.get(WORLD_KEY_MAIN_CHAR_ID, out currentMainCharId, 0L);
                if (currentMainCharId == actor.getID())
                {
                    World.world.map_stats.custom_data.set(WORLD_KEY_MAIN_CHAR_ID, 0L);
                }
            }
            if (actor.isAlive())
            {
                actor.dieAndDestroy(AttackType.Divine);
                if (actor.isAlive())
                {
                    actor.setAlive(false);
                    World.world.units.scheduleDestroyOnPlay(actor);
                }
            }
        }
        public static void Reset()
        {
        }
    }
}