using UnityEngine;
namespace xn.ui
{
    public static class PossessionBrushTool
    {
        private static string _currentPowerId = "xn_possession_brush";
        private static PowerButton _powerButton = null;
        private static Actor _soulActor = null; 
        const string KEY_POS_ACTIVE = "xn.possession.active";
        const string KEY_POS_TARGET = "xn.possession.target_id";
        const string KEY_POS_RESOLVE = "xn.possession.resolve_t";
        const string KEY_POS_TAKEN = "xn.possession.taken";
        const string KEY_POS_BEING_POSSESSED = "xn.possession.being_possessed";
        const string KEY_WUXIN = "xn.stat.wuxin";
        const string KEY_LUCK = "xn.stat.qiyun";
        const string KEY_STOP = "xn.cultivation.stop";
        const string KEY_SEAL_UNTIL_YEAR = "xn.seal_until_year";
        const string KEY_XP = "xn.stat.xiuwei";
        const string KEY_REINC = "xn.reincarnation.count";
        const string KEY_POS_PREV_INFO = "xn.possession.prev_info";
        static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        private static readonly long[] REALM_THRESHOLDS = new long[]
        {
            100000,      
            1500000,    
            4000000,    
            9600000,    
            30000000,   
            80000000,   
            150000000,  
            250000000,  
            400000000,  
            600000000,  
            700000000,  
            800000000,  
            900000000,  
            980000000,  
            1200000000,
            1500000000 
        };
        public static void Init()
        {
            CreatePossessionBrushPower();
        }
        private static void CreatePossessionBrushPower()
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
                newPower.name = "btn_xn_possession_brush";
                newPower.path_icon = "ui/icon/takeaway";
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
                    name = "btn_xn_possession_brush",
                    path_icon = "ui/icon/takeaway",
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
                    Sprite icon = SpriteTextureLoader.getSprite("ui/icon/takeaway");
                    _powerButton = NeoModLoader.General.PowerButtonCreator.CreateGodPowerButton(_currentPowerId, icon);
                }
            }
            return _powerButton;
        }
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key, null);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
        private static string ActorName(Actor actor)
        {
            return actor?.getName() ?? T("value_unknown", "Unknown");
        }
        private static bool OnClickAction(WorldTile pTile, string pPowerID)
        {
            if (pTile == null) return false;
            var selectedButton = xn.access.MapBoxAccess.GetSelectedButton(World.world);
            GodPower selectedPower = xn.access.PowerButtonAccess.GetGodPower(selectedButton);
            if (selectedPower == null || selectedPower.id != _currentPowerId)
            {
                _soulActor = null; 
                return false;
            }
            if (_soulActor != null && (_soulActor.isRekt() || !_soulActor.isAlive()))
            {
                _soulActor = null;
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
            if (_soulActor == null)
            {
                int beingPossessed;
                xn.access.ActorAccess.GetData(target).get(KEY_POS_BEING_POSSESSED, out beingPossessed, 0);
                if (beingPossessed != 0)
                {
                    xn.world.BroadcastSystem.Custom(string.Format(T("brush_possession_being_possessed_soul", "{0} is being possessed and cannot be used as the soul"), ActorName(target)));
                    return false;
                }
                _soulActor = target;
                xn.world.BroadcastSystem.Custom(string.Format(T("brush_possession_soul_selected", "Soul selected: {0}. Click the target"), ActorName(target)));
                return true;
            }
            if (_soulActor == target)
            {
                xn.world.BroadcastSystem.Custom(T("brush_cannot_select_same_unit", "Cannot select the same unit"));
                return false;
            }
            int taken;
            xn.access.ActorAccess.GetData(target).get(KEY_POS_TAKEN, out taken, 0);
            if (taken != 0)
            {
                _soulActor = null;
                xn.world.BroadcastSystem.Custom(string.Format(T("brush_possession_already_taken_target", "{0} has already been possessed and cannot be used as the target"), ActorName(target)));
                return false;
            }
            int targetBeingPossessed;
            xn.access.ActorAccess.GetData(target).get(KEY_POS_BEING_POSSESSED, out targetBeingPossessed, 0);
            if (targetBeingPossessed != 0)
            {
                _soulActor = null;
                xn.world.BroadcastSystem.Custom(string.Format(T("brush_possession_being_possessed_target", "{0} is being possessed and cannot be used as the target"), ActorName(target)));
                return false;
            }
            int soulActive;
            xn.access.ActorAccess.GetData(_soulActor).get(KEY_POS_ACTIVE, out soulActive, 0);
            if (soulActive != 0)
            {
                string soulName = ActorName(_soulActor);
                _soulActor = null;
                xn.world.BroadcastSystem.Custom(string.Format(T("brush_possession_soul_active", "{0} is already possessing someone and cannot be used again"), soulName));
                return false;
            }
            ExecutePossession(_soulActor, target);
            _soulActor = null;
            return true;
        }
        private static void ExecutePossession(Actor soul, Actor target)
        {
            xn.access.ActorAccess.GetData(target).set(KEY_POS_BEING_POSSESSED, 1);
            FreezeDuringFX(soul, target);
            DuoSheFX.PlayOnce(target);
            ResolvePossession(soul, target);
        }
        private static void FreezeDuringFX(Actor a, Actor t)
        {
            float d = DuoSheFX.GetDuration();
            if (a.isAlive()) a.makeStunned(d);
            if (t != null && t.isAlive()) t.makeStunned(d);
        }
        private static void ResolvePossession(Actor soul, Actor target)
        {
            xn.access.ActorAccess.GetData(soul).set(KEY_POS_ACTIVE, 0);
            long tid = 0L;
            xn.access.ActorAccess.GetData(soul).get(KEY_POS_TARGET, out tid, 0L);
            xn.access.ActorAccess.GetData(soul).set(KEY_POS_TARGET, 0L);
            xn.access.ActorAccess.GetData(soul).set(KEY_POS_RESOLVE, 0f);
            if (target != null && target.isAlive())
            {
                xn.access.ActorAccess.GetData(target).set(KEY_POS_BEING_POSSESSED, 0);
            }
            if (target == null || !target.isAlive())
            {
                xn.world.ReincarnationSystem.OnEligibleDeath(soul);
                soul.dieAndDestroy(AttackType.Other);
                return;
            }
            int soulRealmIdx = GetRealmIndex(soul);
            int targetRealmIdx = GetRealmIndex(target);
            if (targetRealmIdx >= soulRealmIdx)
            {
                xn.world.BroadcastSystem.PossessionFail(soul, target);
                xn.world.ReincarnationSystem.OnEligibleDeath(soul);
                soul.dieAndDestroy(AttackType.Other);
                return;
            }
            int sw; xn.access.ActorAccess.GetData(soul).get(KEY_WUXIN, out sw, 0);
            int sl; xn.access.ActorAccess.GetData(soul).get(KEY_LUCK, out sl, 0);
            int tw; xn.access.ActorAccess.GetData(target).get(KEY_WUXIN, out tw, 0);
            int tl; xn.access.ActorAccess.GetData(target).get(KEY_LUCK, out tl, 0);
            int realmIdx = GetRealmIndex(soul);
            float floor = 0.10f; 
            if (realmIdx >= 3)
            {
                float stepped = 0.10f + 0.10f * (realmIdx - 3);
                floor = Mathf.Min(0.70f, stepped);
            }
            float prob = Mathf.Max(floor, (sw + sl - (tw + tl)) * 0.10f);
            if (Randy.randomChance(prob))
            {
                SavePreviousLifeSnapshot(soul, target);
                ApplyPossessionSuccess(soul, target);
                int rc; xn.access.ActorAccess.GetData(soul).get(KEY_REINC, out rc, 0);
                xn.access.ActorAccess.GetData(target).set(KEY_REINC, rc);
                xn.access.ActorAccess.GetData(target).set(KEY_POS_TAKEN, 1);
                xn.access.ActorAccess.GetData(target).set(KEY_STOP, 1);
                xn.access.ActorAccess.GetData(target).set(KEY_SEAL_UNTIL_YEAR, Date.getCurrentYear() + 10);
                xn.world.BroadcastSystem.PossessionSuccess(soul, target);
                soul.dieAndDestroy(AttackType.Other);
            }
            else
            {
                xn.world.BroadcastSystem.PossessionFail(soul, target);
                xn.world.ReincarnationSystem.OnEligibleDeath(soul);
                soul.dieAndDestroy(AttackType.Other);
            }
        }
        private static void SavePreviousLifeSnapshot(Actor soul, Actor target)
        {
            if (soul == null || target == null) return;
            long soulId = soul.getID();
            string soulName = soul.getName();
            int realmIdx = GetRealmIndex(soul);
            string realmName = (realmIdx >= 0 && realmIdx < REALM_IDS.Length) ? REALM_IDS[realmIdx] : "";
            long xp; xn.access.ActorAccess.GetData(soul).get(KEY_XP, out xp, 0L);
            int wuxin; xn.access.ActorAccess.GetData(soul).get(KEY_WUXIN, out wuxin, 0);
            int luck; xn.access.ActorAccess.GetData(soul).get(KEY_LUCK, out luck, 0);
            string kingdomName = soul.hasKingdom() ? soul.kingdom.name : "";
            string speciesId = "";
            if (soul.asset != null && !string.IsNullOrEmpty(soul.asset.id))
            {
                speciesId = soul.asset.id;
            }
            else if (xn.access.ActorAccess.GetData(soul) != null && !string.IsNullOrEmpty(xn.access.ActorAccess.GetData(soul).asset_id))
            {
                speciesId = xn.access.ActorAccess.GetData(soul).asset_id;
            }
            int year = Date.getCurrentYear();
            string snapshot = $"{soulId}|{soulName}|{realmName}|{xp}|{wuxin}|{luck}|{kingdomName}|{speciesId}|{year}";
            xn.access.ActorAccess.GetData(target).set(KEY_POS_PREV_INFO, snapshot);
        }
        private static void ApplyPossessionSuccess(Actor src, Actor dst)
        {
            bool dstFavorite = xn.access.ActorAccess.GetData(dst).favorite;
            TransferBloodlineData(src, dst);
            string srcBase = ExtractBaseNameOnly(src.getName());
            dst.setName(srcBase);
            if (src.hasKingdom()) xn.access.ActorAccess.SetKingdom(dst, src.kingdom);
            long xp; xn.access.ActorAccess.GetData(src).get(KEY_XP, out xp, 0L);
            xn.access.ActorAccess.GetData(dst).set(KEY_XP, xp);
            if (!HasAnyCultivationRealm(dst))
            {
                var qi = AssetManager.traits.get("realm_01_qi") as ActorTrait;
                if (qi != null) dst.addTrait(qi);
                xn.access.ActorAccess.GetData(dst).set(KEY_XP, 0L);
            }
            var removeBuf = new System.Collections.Generic.List<ActorTrait>(16);
            var tsDst = dst.getTraits();
            if (tsDst != null)
            {
                foreach (var t in tsDst) if (t != null) removeBuf.Add(t);
                for (int i = 0; i < removeBuf.Count; i++) dst.removeTrait(removeBuf[i]);
            }
            var tsSrc = src.getTraits();
            if (tsSrc != null)
            {
                foreach (var t in tsSrc) if (t != null && !dst.hasTrait(t)) dst.addTrait(t);
            }
            int currentRealmIdx = GetRealmIndex(dst);
            if (currentRealmIdx >= 1)
            {
                int newRealmIdx = currentRealmIdx - 1;
                if (newRealmIdx >= 0 && newRealmIdx < REALM_IDS.Length)
                {
                    string currentRealmId = REALM_IDS[currentRealmIdx];
                    var currentRealmTrait = AssetManager.traits.get(currentRealmId) as ActorTrait;
                    if (currentRealmTrait != null && dst.hasTrait(currentRealmTrait))
                    {
                        dst.removeTrait(currentRealmTrait);
                    }
                    string newRealmId = REALM_IDS[newRealmIdx];
                    var newRealmTrait = AssetManager.traits.get(newRealmId) as ActorTrait;
                    if (newRealmTrait != null)
                    {
                        dst.addTrait(newRealmTrait);
                    }
                    if (newRealmIdx < REALM_THRESHOLDS.Length)
                    {
                        long newXP = REALM_THRESHOLDS[newRealmIdx];
                        xn.access.ActorAccess.GetData(dst).set(KEY_XP, newXP);
                    }
                }
            }
            if (dstFavorite)
            {
                xn.access.ActorAccess.GetData(dst).favorite = true;
            }
        }
        private static int GetRealmIndex(Actor a)
        {
            int idx = -1;
            var ts = a.getTraits();
            if (ts == null) return -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
                foreach (var t in ts) if (t != null && t.id == REALM_IDS[i]) { if (i > idx) idx = i; }
            return idx;
        }
        private static bool HasAnyCultivationRealm(Actor a)
        {
            var list = a.getTraits();
            if (list == null) return false;
            foreach (var t in list) if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupRealm) return true;
            return false;
        }
        private static string ExtractBaseNameOnly(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            string rest = name.Trim();
            while (rest.Length > 0 && rest[0] == '[')
            {
                int end = rest.IndexOf(']');
                if (end > 0)
                {
                    rest = (end + 1 < rest.Length) ? rest.Substring(end + 1).Trim() : "";
                }
                else
                {
                    break;
                }
            }
            int dash = rest.LastIndexOf('-');
            if (dash >= 0) rest = rest.Substring(0, dash).Trim();
            return string.IsNullOrEmpty(rest) ? name.Trim() : rest;
        }
        private static void TransferBloodlineData(Actor src, Actor dst)
        {
            if (src == null || dst == null) return;
            xn.access.ActorAccess.GetData(src).get("xn.bloodline.type", out string bloodlineType, "");
            if (string.IsNullOrEmpty(bloodlineType))
            {
                return;
            }
            string[] stringKeys = new string[]
            {
                "xn.bloodline.type",
                "xn.bloodline.founder_name",
                "xn.bloodline.mutation_type"
            };
            foreach (var key in stringKeys)
            {
                xn.access.ActorAccess.GetData(src).get(key, out string value, "");
                if (!string.IsNullOrEmpty(value))
                {
                    xn.access.ActorAccess.GetData(dst).set(key, value);
                }
            }
            xn.access.ActorAccess.GetData(src).get("xn.bloodline.concentration", out float concentration, 0f);
            if (concentration > 0f)
            {
                xn.access.ActorAccess.GetData(dst).set("xn.bloodline.concentration", concentration);
            }
            string[] intKeys = new string[]
            {
                "xn.bloodline.generation",
                "xn.bloodline.awakened",
                "xn.bloodline.awakened_year",
                "xn.bloodline.is_founder",
                "xn.bloodline.is_atavism",
                "xn.bloodline.position",
                "xn.bloodline.last_election_year",
                "xn.bloodline.family_created_year"
            };
            foreach (var key in intKeys)
            {
                xn.access.ActorAccess.GetData(src).get(key, out int value, 0);
                xn.access.ActorAccess.GetData(dst).set(key, value);
            }
            string[] longKeys = new string[]
            {
                "xn.bloodline.founder_id"
            };
            foreach (var key in longKeys)
            {
                xn.access.ActorAccess.GetData(src).get(key, out long value, -1L);
                xn.access.ActorAccess.GetData(dst).set(key, value);
            }
            if (src.hasClan())
            {
                xn.access.ActorAccess.GetData(dst).set("xn.bloodline.clan_id", src.clan.getID());
                if (src.clan.isAlive())
                {
                    dst.setClan(src.clan);
                }
            }
            else if (dst.hasClan())
            {
                xn.access.ActorAccess.GetData(dst).set("xn.bloodline.clan_id", dst.clan.getID());
            }
        }
        public static void Reset()
        {
            _soulActor = null;
        }
    }
}
