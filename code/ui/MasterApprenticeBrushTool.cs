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
                if (masterId > 0)
                {
                    xn.world.BroadcastSystem.Custom(T("brush_mentorship_already_has_master", "{0} already has a master and cannot be set", ActorName(target)));
                    return false;
                }
                _firstActor = target;
                xn.world.BroadcastSystem.Custom(T("brush_mentorship_first_selected", "First selected: {0}. Click the second unit", ActorName(target)));
                return true;
            }
            if (_firstActor == target)
            {
                xn.world.BroadcastSystem.Custom(T("brush_cannot_select_same_unit", "Cannot select the same unit"));
                return false;
            }
            long targetMasterId;
            xn.access.ActorAccess.GetData(target).get(KEY_MASTER_ID, out targetMasterId, 0L);
            if (targetMasterId > 0)
            {
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom(T("brush_mentorship_already_has_master", "{0} already has a master and cannot be set", ActorName(target)));
                return false;
            }
            long firstMasterId;
            xn.access.ActorAccess.GetData(_firstActor).get(KEY_MASTER_ID, out firstMasterId, 0L);
            if (firstMasterId > 0)
            {
                string firstName = ActorName(_firstActor);
                _firstActor = null; 
                xn.world.BroadcastSystem.Custom(T("brush_mentorship_master_already_has_master", "{0} already has a master and cannot become a master", firstName));
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
                xn.world.BroadcastSystem.Custom(T("brush_mentorship_same_realm", "Both units are in the same realm, so mentorship cannot be set"));
                return false;
            }
            int maxDisciples = GetMaxDisciples(master);
            int currentDisciples = GetDisciplesCount(master);
            if (currentDisciples >= maxDisciples)
            {
                _firstActor = null;
                xn.world.BroadcastSystem.Custom(T("brush_mentorship_disciple_limit", "{0} has reached the disciple limit ({1}) and cannot take another disciple", ActorName(master), maxDisciples));
                return false;
            }
            AddDisciple(master, disciple);
            string masterName = ActorName(master);
            string discipleName = ActorName(disciple);
            xn.world.BroadcastSystem.Custom(T("brush_mentorship_success", "{0} accepted {1} as a disciple", masterName, discipleName));
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
            xn.access.ActorAccess.GetData(master).get(KEY_DISCIPLES_IDS, out idsStr, "");
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
            xn.access.ActorAccess.GetData(master).get(KEY_DISCIPLES_IDS, out idsStr, "");
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
            if (!list.Contains(xn.access.ActorAccess.GetData(disciple).id))
            {
                list.Add(xn.access.ActorAccess.GetData(disciple).id);
                xn.access.ActorAccess.GetData(master).set(KEY_DISCIPLES_IDS, string.Join(",", list));
            }
            xn.access.ActorAccess.GetData(disciple).set(KEY_MASTER_ID, xn.access.ActorAccess.GetData(master).id);
        }
        public static void Reset()
        {
            _firstActor = null;
        }
    }
}
