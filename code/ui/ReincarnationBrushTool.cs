using UnityEngine;
namespace xn.ui
{
    public static class ReincarnationBrushTool
    {
        private static string _currentPowerId = "xn_reincarnation_brush";
        private static PowerButton _powerButton = null;
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        private static string ActorName(Actor actor)
        {
            return actor?.getName() ?? T("value_unknown", "Unknown");
        }
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
            var selectedButton = xn.access.MapBoxAccess.GetSelectedButton(World.world);
            GodPower selectedPower = xn.access.PowerButtonAccess.GetGodPower(selectedButton);
            if (selectedPower == null || selectedPower.id != _currentPowerId)
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
                xn.world.BroadcastSystem.Custom(T("brush_select_sapient_unit", "Click a sapient unit"));
                return false;
            }
            const string KEY_ENQUEUED = "xn.reinc.enq";
            int enq;
            xn.access.ActorAccess.GetData(target).get(KEY_ENQUEUED, out enq, 0);
            if (enq == 1)
            {
                xn.world.BroadcastSystem.Custom(T("brush_reincarnation_already_in_pool", "{0} has already entered the reincarnation pool", ActorName(target)));
                return false;
            }
            int demonMark; xn.access.ActorAccess.GetData(target).get(xn.world.AmbitionSystem.KEY_AMB_DEMON, out demonMark, 0);
            int dragonMark; xn.access.ActorAccess.GetData(target).get(xn.world.AmbitionSystem.KEY_AMB_DRAGON, out dragonMark, 0);
            if (demonMark == 1 || dragonMark == 1)
            {
                xn.world.BroadcastSystem.Custom(T("brush_reincarnation_tianyunzi_blocked", "{0} is Tian Yunzi and cannot enter the reincarnation pool", ActorName(target)));
                return false;
            }
            xn.world.ReincarnationSystem.ForceAddToPool(target);
            const string KEY_REINC_BRUSH = "xn.reincarnation.brush";
            xn.access.ActorAccess.GetData(target).set(KEY_REINC_BRUSH, 1);
            string targetName = ActorName(target);
            target.die(false, AttackType.Other, true, true);
            xn.access.ActorAccess.GetData(target).set(KEY_REINC_BRUSH, 0);
            xn.world.BroadcastSystem.Custom(T("brush_reincarnation_success", "{0} entered the reincarnation pool to be reborn", targetName));
            return true;
        }
    }
}
