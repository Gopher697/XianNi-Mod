using System;
namespace xn.Traits
{
    public static class RealmTraitGroup
    {
        public const string GroupRealm          = "cultivation_realm";
        public const string GroupAncientRealm   = "ancient_god_realm";
        public const string GroupAttrFive       = "attr_five";
        public const string GroupSpiritRoot     = "spirit_root";
        public const string GroupDaoBase        = "dao_base";
        public const string GroupAncientInherit = "ancient_inherit";
        public const string GroupBeastStage     = "beast_stage";
        public const string GroupDivineArt      = "divine_art";
        public const string GroupCultivation    = "cultivation_path";
        public const string GroupIntent         = "cultivation_intent";
        public const string GroupImmortalArt    = "immortal_art"; 
        public const string GroupTest = "xn_test_group";
        public static void Init()
        {
            AddGroup(GroupSpiritRoot,     "trait_group_spirit_root",       "#2E8B57");
            AddGroup(GroupDaoBase,        "trait_group_dao_base",          "#1E90FF");
            AddGroup(GroupCultivation,    "trait_group_cultivation_path",  "#3b73eaff");
            AddGroup(GroupAncientInherit, "trait_group_ancient_inherit",   "#A0522D");           
            AddGroup(GroupRealm,          "trait_group_cultivation_realm", "#28ee6dff");
            AddGroup(GroupAncientRealm,   "trait_group_ancient_god_realm", "#bfe926ff");                       
            AddGroup(GroupBeastStage,     "trait_group_beast_stage",       "#708090");
            AddGroup(GroupAttrFive,       "trait_group_attr_five",         "#DAA520");                       
            AddGroup(GroupIntent,         "trait_group_cultivation_intent","#20B2AA");
            AddGroup(GroupDivineArt,      "trait_group_divine_art",        "#FF8C00");
            AddGroup(GroupImmortalArt,    "trait_group_immortal_art",      "#e10388ff");
        }
        private static void AddGroup(string id, string nameKey, string colorHex)
        {
            AssetManager.trait_groups.add(new ActorTraitGroupAsset
            {
                id = id,
                name = nameKey,
                color = colorHex
            });
        }
    }
}