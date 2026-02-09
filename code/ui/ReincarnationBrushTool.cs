using UnityEngine;
namespace xn.ui
{
    public static class ReincarnationBrushTool
    {
        private static string _currentPowerId = "xn_reincarnation_brush";
        private static PowerButton _powerButton = null;
        public static void Init()
        {
            CreateReincarnationBrushPower();
        }
        private static void CreateReincarnationBrushPower()
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
                newPower.name = "btn_xn_reincarnation_brush";
                newPower.path_icon = "ui/icon/transmigration";
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
                    name = "btn_xn_reincarnation_brush",
                    path_icon = "ui/icon/transmigration",
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
                    Sprite icon = SpriteTextureLoader.getSprite("ui/icon/transmigration");
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
            const string KEY_ENQUEUED = "xn.reinc.enq";
            int enq;
            target.data.get(KEY_ENQUEUED, out enq, 0);
            if (enq == 1)
            {
                xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}已经进入轮回池");
                return false;
            }
            int demonMark; target.data.get(xn.world.AmbitionSystem.KEY_AMB_DEMON, out demonMark, 0);
            int dragonMark; target.data.get(xn.world.AmbitionSystem.KEY_AMB_DRAGON, out dragonMark, 0);
            if (demonMark == 1 || dragonMark == 1)
            {
                xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}是天运子，不能进入轮回池");
                return false;
            }
            xn.world.ReincarnationSystem.ForceAddToPool(target);
            const string KEY_REINC_BRUSH = "xn.reincarnation.brush";
            target.data.set(KEY_REINC_BRUSH, 1);
            string targetName = target.getName() ?? "未知";
            target.die(false, AttackType.Other, true, true);
            target.data.set(KEY_REINC_BRUSH, 0);
            xn.world.BroadcastSystem.Custom($"{targetName}已进入轮回池转世");
            return true;
        }
    }
}