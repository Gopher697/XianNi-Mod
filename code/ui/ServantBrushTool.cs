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
            var selectedButton = xn.access.MapBoxAccess.GetSelectedButton(World.world);
            GodPower selectedPower = xn.access.PowerButtonAccess.GetGodPower(selectedButton);
            if (selectedPower == null || selectedPower.id != _currentPowerId)
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
                xn.world.BroadcastSystem.Custom(T("brush_select_sapient_unit", "Click a sapient unit"));
                return false;
            }
            if (_firstActor == null)
            {
                long masterId;
                xn.access.ActorAccess.GetData(target).get(KEY_MASTER_ID, out masterId, 0L);
                long slaveId;
                xn.access.ActorAccess.GetData(target).get(KEY_SLAVE_ID, out slaveId, 0L);
                if (masterId > 0)
                {
                    xn.world.BroadcastSystem.Custom(T("brush_servant_already_servant_cannot_master", "{0} is already someone else's servant and cannot be set as master", ActorName(target)));
                    return false;
                }
                if (slaveId > 0)
                {
                    xn.world.BroadcastSystem.Custom(T("brush_servant_already_has_servant", "{0} already has a servant and cannot be set again", ActorName(target)));
                    return false;
                }
                _firstActor = target;
                xn.world.BroadcastSystem.Custom(T("brush_servant_master_selected", "Master selected: {0}. Click the servant", ActorName(target)));
                return true;
            }
            if (_firstActor == target)
            {
                xn.world.BroadcastSystem.Custom(T("brush_cannot_select_same_unit", "Cannot select the same unit"));
                return false;
            }
            long targetMasterId;
            xn.access.ActorAccess.GetData(target).get(KEY_MASTER_ID, out targetMasterId, 0L);
            long targetSlaveId;
            xn.access.ActorAccess.GetData(target).get(KEY_SLAVE_ID, out targetSlaveId, 0L);
            if (targetMasterId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom(T("brush_servant_already_servant", "{0} is already someone else's servant and cannot be set", ActorName(target)));
                return false;
            }
            if (targetSlaveId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom(T("brush_servant_target_has_servant", "{0} already has a servant and cannot be made a servant", ActorName(target)));
                return false;
            }
            long firstSlaveId;
            xn.access.ActorAccess.GetData(_firstActor).get(KEY_SLAVE_ID, out firstSlaveId, 0L);
            if (firstSlaveId > 0)
            {
                string firstName = ActorName(_firstActor);
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom(T("brush_servant_already_has_servant", "{0} already has a servant and cannot be set again", firstName));
                return false;
            }
            int expireYear = Date.getCurrentYear() + 5;
            xn.access.ActorAccess.GetData(target).set(KEY_MASTER_ID, xn.access.ActorAccess.GetData(_firstActor).id);
            xn.access.ActorAccess.GetData(target).set(KEY_EXPIRE_YEAR, expireYear);
            xn.access.ActorAccess.GetData(_firstActor).set(KEY_SLAVE_ID, xn.access.ActorAccess.GetData(target).id);
            target.cancelAllBeh();
            _firstActor.cancelAllBeh();
            string masterName = ActorName(_firstActor);
            string slaveName = ActorName(target);
            xn.world.BroadcastSystem.Custom(T("brush_servant_success", "{0} was marked with {1}'s slave seal (5-year contract)", slaveName, masterName));
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
