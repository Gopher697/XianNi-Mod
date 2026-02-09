using UnityEngine;
using System.Collections.Generic;
namespace xn.ui
{
    public static class MasterApprenticeBrushTool
    {
        private static string _currentPowerId = "xn_master_apprentice_brush";
        private static PowerButton _powerButton = null;
        private static Actor _firstActor = null; 
        const string KEY_MASTER_ID = "xn_men_master_id";           
        const string KEY_DISCIPLES_IDS = "xn_men_disciples_ids";   
        static readonly string[] REALM_IDS = {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        public static void Init()
        {
            CreateMasterApprenticeBrushPower();
        }
        private static void CreateMasterApprenticeBrushPower()
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
                newPower.name = "btn_xn_master_apprentice_brush";
                newPower.path_icon = "ui/icon/master";
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
                    name = "btn_xn_master_apprentice_brush",
                    path_icon = "ui/icon/master",
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
                    Sprite icon = SpriteTextureLoader.getSprite("ui/icon/master");
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
                if (masterId > 0)
                {
                    xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}已经有师傅，不能设置");
                    return false;
                }
                _firstActor = target;
                xn.world.BroadcastSystem.Custom($"已选择第一个：{target.getName() ?? "未知"}，请点击第二个");
                return true;
            }
            if (_firstActor == target)
            {
                xn.world.BroadcastSystem.Custom("不能选择同一个单位");
                return false;
            }
            long targetMasterId;
            target.data.get(KEY_MASTER_ID, out targetMasterId, 0L);
            if (targetMasterId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom($"{target.getName() ?? "未知"}已经有师傅，不能设置");
                return false;
            }
            long firstMasterId;
            _firstActor.data.get(KEY_MASTER_ID, out firstMasterId, 0L);
            if (firstMasterId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom($"{_firstActor.getName() ?? "未知"}已经有师傅，不能设置为师傅");
                return false;
            }
            int firstRealm = GetRealmIndex(_firstActor);
            int secondRealm = GetRealmIndex(target);
            Actor master = null;
            Actor disciple = null;
            if (firstRealm > secondRealm)
            {
                master = _firstActor;
                disciple = target;
            }
            else if (secondRealm > firstRealm)
            {
                master = target;
                disciple = _firstActor;
            }
            else
            {
                _firstActor = null;
                xn.world.BroadcastSystem.Custom("两个单位境界相同，无法设置师徒关系");
                return false;
            }
            int maxDisciples = GetMaxDisciples(master);
            int currentDisciples = GetDisciplesCount(master);
            if (currentDisciples >= maxDisciples)
            {
                _firstActor = null;
                xn.world.BroadcastSystem.Custom($"{master.getName() ?? "未知"}已达到收徒上限（{maxDisciples}名），无法再收徒");
                return false;
            }
            AddDisciple(master, disciple);
            string masterName = master.getName() ?? "未知";
            string discipleName = disciple.getName() ?? "未知";
            xn.world.BroadcastSystem.Custom($"{masterName}收{discipleName}为徒");
            xn.voice.AIVoiceBroadcast.OnMentorshipSuccess(disciple, master);
            _firstActor = null;
            return true;
        }
        private static int GetRealmIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            foreach (var tr in a.traits)
            {
                if (tr == null || string.IsNullOrEmpty(tr.id)) continue;
                for (int i = 0; i < REALM_IDS.Length; i++)
                    if (tr.id == REALM_IDS[i]) { if (i > idx) idx = i; }
            }
            return idx;
        }
        private static int GetMaxDisciples(Actor master)
        {
            int realm = GetRealmIndex(master);
            if (realm < 1) return 0; 
            return realm;
        }
        private static int GetDisciplesCount(Actor master)
        {
            string idsStr;
            master.data.get(KEY_DISCIPLES_IDS, out idsStr, "");
            if (string.IsNullOrEmpty(idsStr)) return 0;
            string[] parts = idsStr.Split(',');
            int count = 0;
            foreach (var part in parts)
            {
                if (long.TryParse(part.Trim(), out long id) && id > 0)
                {
                    var d = World.world.units.get(id);
                    if (d != null && !d.isRekt()) count++;
                }
            }
            return count;
        }
        private static List<long> GetDisciplesList(Actor master)
        {
            List<long> list = new List<long>();
            string idsStr;
            master.data.get(KEY_DISCIPLES_IDS, out idsStr, "");
            if (string.IsNullOrEmpty(idsStr)) return list;
            string[] parts = idsStr.Split(',');
            foreach (var part in parts)
            {
                if (long.TryParse(part.Trim(), out long id) && id > 0)
                {
                    var d = World.world.units.get(id);
                    if (d != null && !d.isRekt()) list.Add(id);
                }
            }
            return list;
        }
        private static void AddDisciple(Actor master, Actor disciple)
        {
            List<long> list = GetDisciplesList(master);
            if (!list.Contains(disciple.data.id))
            {
                list.Add(disciple.data.id);
                master.data.set(KEY_DISCIPLES_IDS, string.Join(",", list));
            }
            disciple.data.set(KEY_MASTER_ID, master.data.id);
        }
        public static void Reset()
        {
            _firstActor = null;
        }
    }
}