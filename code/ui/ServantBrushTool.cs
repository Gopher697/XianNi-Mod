using UnityEngine;
namespace xn.ui
{
    public static class ServantBrushTool
    {
        private static string _currentPowerId = "xn_servant_brush";
        private static PowerButton _powerButton = null;
        private static Actor _firstActor = null; 
        const string KEY_MASTER_ID = "xn_slave_master_id";  
        const string KEY_SLAVE_ID = "xn_slave_id";          
        const string KEY_EXPIRE_YEAR = "xn_slave_expire_year"; 
        public static void Init()
        {
            CreateServantBrushPower();
        }
        private static void CreateServantBrushPower()
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
                newPower.name = "btn_xn_servant_brush";
                newPower.path_icon = "ui/icon/servant";
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
                    name = "btn_xn_servant_brush",
                    path_icon = "ui/icon/servant",
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
                    Sprite icon = SpriteTextureLoader.getSprite("ui/icon/servant");
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
                _firstActor = null; 
                return false;
            }
            if (_firstActor != null && (_firstActor.isRekt() || !_firstActor.isAlive()))
            {
                _firstActor = null;
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
            if (_firstActor == null)
            {
                long masterId;
                target.data.get(KEY_MASTER_ID, out masterId, 0L);
                long slaveId;
                target.data.get(KEY_SLAVE_ID, out slaveId, 0L);
                if (masterId > 0)
                {
                    xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}已经是别人的奴仆，不能设置为主");
                    return false;
                }
                if (slaveId > 0)
                {
                    xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}已经有奴仆，不能重复设置");
                    return false;
                }
                _firstActor = target;
                xn.world.BroadcastSystem.Custom($"已选择主人：{target.getName() ?? "未知"}，请点击奴仆");
                return true;
            }
            if (_firstActor == target)
            {
                xn.world.BroadcastSystem.Custom("不能选择同一个单位");
                return false;
            }
            long targetMasterId;
            target.data.get(KEY_MASTER_ID, out targetMasterId, 0L);
            long targetSlaveId;
            target.data.get(KEY_SLAVE_ID, out targetSlaveId, 0L);
            if (targetMasterId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}已经是别人的奴仆，不能设置");
                return false;
            }
            if (targetSlaveId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}已经有奴仆，不能设置为奴");
                return false;
            }
            long firstSlaveId;
            _firstActor.data.get(KEY_SLAVE_ID, out firstSlaveId, 0L);
            if (firstSlaveId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom($"{_firstActor.getName() ?? "未知"}已经有奴仆，不能重复设置");
                return false;
            }
            int expireYear = Date.getCurrentYear() + 5;
            target.data.set(KEY_MASTER_ID, _firstActor.data.id);
            target.data.set(KEY_EXPIRE_YEAR, expireYear);
            _firstActor.data.set(KEY_SLAVE_ID, target.data.id);
            target.cancelAllBeh();
            _firstActor.cancelAllBeh();
            string masterName = _firstActor.getName() ?? "未知";
            string slaveName = target.getName() ?? "未知";
            xn.world.BroadcastSystem.Custom($"{slaveName}被{masterName}种下了奴印（5年契约）");
            xn.voice.AIVoiceBroadcast.OnSlaveSealSuccess(target, _firstActor);
            _firstActor = null;
            return true;
        }
        public static void Reset()
        {
            _firstActor = null;
        }
    }
}