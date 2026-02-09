using System;
using System.Collections.Generic;
using xn.world;
namespace xn.Traits
{
    public static class RealmTraits
    {
        private const string DAO_DAMAGED = "dao_07_damaged";
        public static void Init()
        {
            Add(RealmTraitGroup.GroupRealm, "realm_01_qi",          Rarity.R2_Epic);
            Add(RealmTraitGroup.GroupRealm, "realm_02_foundation",  Rarity.R2_Epic);
            Add(RealmTraitGroup.GroupRealm, "realm_03_core",        Rarity.R2_Epic);
            Add(RealmTraitGroup.GroupRealm, "realm_04_nascent",     Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_05_deity",       Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_06_infantchg",   Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_07_wending",     Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_08_kuinie",      Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_09_jingnie",     Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_10_suinie",      Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_11_kongnie",     Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_12_kongling",    Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_13_kongxuan",    Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_14_gtianzun",    Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_15_half_tatian", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupRealm, "realm_16_tatian",      Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_01_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_02_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_03_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_04_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_05_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_06_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_07_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_08_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_09_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAncientRealm, "ancient_10_star", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAttrFive, "attr_01_metal", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAttrFive, "attr_02_wood",  Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAttrFive, "attr_03_water", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAttrFive, "attr_04_fire",  Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupAttrFive, "attr_05_earth", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupSpiritRoot, "root_01_mortal",  Rarity.R0_Normal);
            Add(RealmTraitGroup.GroupSpiritRoot, "root_02_low",     Rarity.R0_Normal);
            Add(RealmTraitGroup.GroupSpiritRoot, "root_03_mid",     Rarity.R1_Rare);
            Add(RealmTraitGroup.GroupSpiritRoot, "root_04_high",    Rarity.R1_Rare);
            Add(RealmTraitGroup.GroupSpiritRoot, "root_05_supreme", Rarity.R2_Epic);
            Add(RealmTraitGroup.GroupSpiritRoot, "root_06_tiandi",  Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupSpiritRoot, "root_07_broken",  Rarity.R0_Normal);
            Add(RealmTraitGroup.GroupDaoBase, "dao_01_mortal",  Rarity.R0_Normal);
            Add(RealmTraitGroup.GroupDaoBase, "dao_02_low",     Rarity.R0_Normal);
            Add(RealmTraitGroup.GroupDaoBase, "dao_03_mid",     Rarity.R1_Rare);
            Add(RealmTraitGroup.GroupDaoBase, "dao_04_high",    Rarity.R1_Rare);
            Add(RealmTraitGroup.GroupDaoBase, "dao_05_supreme", Rarity.R2_Epic);
            Add(RealmTraitGroup.GroupDaoBase, "dao_06_tiandi",  Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDaoBase, "dao_07_damaged", Rarity.R0_Normal); 
            Add(RealmTraitGroup.GroupAncientInherit, "inherit_01_poor",         Rarity.R0_Normal);
            Add(RealmTraitGroup.GroupAncientInherit, "inherit_02_normal",       Rarity.R1_Rare);
            Add(RealmTraitGroup.GroupAncientInherit, "inherit_03_supreme",      Rarity.R2_Epic);
            Add(RealmTraitGroup.GroupAncientInherit, "inherit_04_tusi",         Rarity.R2_Epic);
            Add(RealmTraitGroup.GroupAncientInherit, "inherit_05_ancientblood", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_01_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_02_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_03_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_04_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_05_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_06_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_07_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_08_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_09_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupBeastStage, "beast_10_stage", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_01_baonuzhibian",      Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_02_weiya",             Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_03_sanmeizhenhuo",     Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_04_wanjianguizong",    Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_05_xuankongpo",        Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_06_zhenkongquan",       Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_07_jiuyinbaiguzhao",    Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_08_duqidan",            Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupDivineArt, "divine_09_jianzhan",            Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupCultivation, "path_01_demonic",  Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupCultivation, "path_02_immortal", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupCultivation, "path_03_beast",    Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupCultivation, "path_04_ancient",  Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_01_extreme",       Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_02_angel",         Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_03_qianhuan",      Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_04_killing",       Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_05_reverse",       Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_06_life_death",    Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_07_reincarnation", Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_08_chaos",         Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupIntent, "intent_09_madness",       Rarity.R3_Legendary);
            Add(RealmTraitGroup.GroupImmortalArt, "art_01_missile",   Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_02_ascension", Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_03_slash",     Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_04_quake",     Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_05_waves",     Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_06_convert",   Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_07_palm",      Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_08_breaker",   Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_09_shield",    Rarity.R3_Legendary); 
            Add(RealmTraitGroup.GroupImmortalArt, "art_10_link",      Rarity.R3_Legendary); 
            GameProgress.saveData();
        }
        private static void Add(string groupId, string id, Rarity rarity)
        {
            string iconPath = $"trair/{id}";
            if (SpriteTextureLoader.getSprite(iconPath) == null)
                iconPath = "zhanwei";
            var t = new ActorTrait
            {
                id = id,
                path_icon = iconPath,
                group_id = groupId,
                rarity = rarity,
                rate_birth = 0,
                rate_inherit = 0,
                needs_to_be_explored = false,
                has_localized_id = true,
                has_description_1 = true,
                show_for_unlockables_ui = true,
                show_in_knowledge_window = true
            };
            var hook = new WorldActionTrait(EnforceExclusiveOnAddOrLoad);
            t.action_on_augmentation_add  = (WorldActionTrait)Delegate.Combine(t.action_on_augmentation_add,  hook);
            t.action_on_augmentation_load = (WorldActionTrait)Delegate.Combine(t.action_on_augmentation_load, hook);
            var br = new WorldActionTrait(xn.world.BroadcastSystem.OnTraitAdded);
            t.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(t.action_on_augmentation_add, br);
            var attackAction = xn.world.XNAttackActions.GetActionFor(id);
            if (attackAction != null)
            {
                t.action_attack_target = attackAction;
            }
            xn.world.CultivationStatsConfigTable.ApplyRealmBaseStats(t);
            if (groupId == RealmTraitGroup.GroupRealm)
            {
                if (id == "realm_12_kongling" || id == "realm_13_kongxuan" || 
                    id == "realm_14_gtianzun" || id == "realm_15_half_tatian" || id == "realm_16_tatian")
                {
                    t.base_stats.addTag("immunity_fire");
                }
            }
            else if (groupId == RealmTraitGroup.GroupAncientRealm)
            {
                if (id == "ancient_08_star" || id == "ancient_09_star" || id == "ancient_10_star")
                {
                    t.base_stats.addTag("immunity_fire");
                }
            }
            else if (groupId == RealmTraitGroup.GroupBeastStage)
            {
                if (id == "beast_08_stage" || id == "beast_09_stage" || id == "beast_10_stage")
                {
                    t.base_stats.addTag("immunity_fire");
                }
            }
            AssetManager.traits.add(t);
            t.unlock(false);
        }
        private static bool EnforceExclusiveOnAddOrLoad(NanoObject target, BaseAugmentationAsset traitAsset)
        {
            Actor actor = target as Actor;
            ActorTrait newTrait = traitAsset as ActorTrait;
            if (actor == null || newTrait == null)
                return false;
            string gid = newTrait.group_id;
            bool exclusiveGroup =
                gid == RealmTraitGroup.GroupRealm          || 
                gid == RealmTraitGroup.GroupAncientRealm   || 
                gid == RealmTraitGroup.GroupBeastStage     || 
                gid == RealmTraitGroup.GroupSpiritRoot     || 
                gid == RealmTraitGroup.GroupAncientInherit || 
                gid == RealmTraitGroup.GroupCultivation    || 
                gid == RealmTraitGroup.GroupIntent         || 
                gid == RealmTraitGroup.GroupDaoBase;          
            if (!exclusiveGroup)
                return false;
            bool newIsDaoDamaged = (gid == RealmTraitGroup.GroupDaoBase && newTrait.id == DAO_DAMAGED);
            bool hasImmortal = false;  
            bool hasDemonic = false;   
            bool hasAncient = false;   
            bool hasBeast = false;     
            var existingTraits = actor.getTraits();
            if (existingTraits != null)
            {
                foreach (var ex in existingTraits)
                {
                    if (ex == null) continue;
                    if (ex.group_id == RealmTraitGroup.GroupCultivation)
                    {
                        if (ex.id == "path_02_immortal") hasImmortal = true;
                        else if (ex.id == "path_01_demonic") hasDemonic = true;
                        else if (ex.id == "path_04_ancient") hasAncient = true;
                        else if (ex.id == "path_03_beast") hasBeast = true;
                    }
                }
            }
            if ((hasImmortal || hasDemonic) &&
                (gid == RealmTraitGroup.GroupAncientInherit || gid == RealmTraitGroup.GroupAncientRealm ||
                 gid == RealmTraitGroup.GroupBeastStage || (gid == RealmTraitGroup.GroupCultivation && newTrait.id == "path_03_beast")))
            {
                actor.removeTrait(newTrait); 
                return false;
            }
            if (hasAncient &&
                (gid == RealmTraitGroup.GroupSpiritRoot ||
                 gid == RealmTraitGroup.GroupRealm ||
                 gid == RealmTraitGroup.GroupBeastStage ||
                 (gid == RealmTraitGroup.GroupCultivation && newTrait.id == "path_03_beast") ||
                 (gid == RealmTraitGroup.GroupDaoBase && !newIsDaoDamaged)))
            {
                actor.removeTrait(newTrait); 
                return false;
            }
            if (hasBeast &&
                (gid == RealmTraitGroup.GroupSpiritRoot ||
                 gid == RealmTraitGroup.GroupRealm ||
                 gid == RealmTraitGroup.GroupAncientInherit ||
                 gid == RealmTraitGroup.GroupAncientRealm ||
                 (gid == RealmTraitGroup.GroupCultivation && (newTrait.id == "path_01_demonic" || newTrait.id == "path_02_immortal" || newTrait.id == "path_04_ancient")) ||
                 (gid == RealmTraitGroup.GroupDaoBase && !newIsDaoDamaged)))
            {
                actor.removeTrait(newTrait); 
                return false;
            }
            var toRemove = new List<ActorTrait>(8);
            foreach (var ex in actor.getTraits())
            {
                if (ex == null || ex == newTrait) continue;
                if (ex.group_id != gid) continue;
                if (gid == RealmTraitGroup.GroupDaoBase)
                {
                    if (ex.id == DAO_DAMAGED) continue; 
                    if (newIsDaoDamaged) continue;      
                }
                toRemove.Add(ex);
            }
            if (toRemove.Count > 0)
                actor.removeTraits(toRemove);
            if (gid == RealmTraitGroup.GroupCultivation &&
                (newTrait.id == "path_01_demonic" || newTrait.id == "path_02_immortal"))
            {
                var rm = new List<ActorTrait>(4);
                if (existingTraits != null)
                {
                    foreach (var ex in existingTraits)
                    {
                        if (ex == null || ex == newTrait) continue;
                        if (ex.group_id == RealmTraitGroup.GroupAncientInherit ||
                            ex.group_id == RealmTraitGroup.GroupAncientRealm ||
                            ex.group_id == RealmTraitGroup.GroupBeastStage)
                            rm.Add(ex);
                    }
                }
                if (rm.Count > 0) actor.removeTraits(rm);
            }
            if (gid == RealmTraitGroup.GroupCultivation && newTrait.id == "path_04_ancient")
            {
                var rm = new List<ActorTrait>(4);
                if (existingTraits != null)
                {
                    foreach (var ex in existingTraits)
                    {
                        if (ex == null || ex == newTrait) continue;
                        if (ex.group_id == RealmTraitGroup.GroupSpiritRoot ||
                            ex.group_id == RealmTraitGroup.GroupRealm ||
                            ex.group_id == RealmTraitGroup.GroupBeastStage ||
                            (ex.group_id == RealmTraitGroup.GroupDaoBase && ex.id != DAO_DAMAGED))
                            rm.Add(ex);
                    }
                }
                if (rm.Count > 0) actor.removeTraits(rm);
            }
            if (gid == RealmTraitGroup.GroupCultivation && newTrait.id == "path_03_beast")
            {
                var rm = new List<ActorTrait>(4);
                if (existingTraits != null)
                {
                    foreach (var ex in existingTraits)
                    {
                        if (ex == null || ex == newTrait) continue;
                        if (ex.group_id == RealmTraitGroup.GroupSpiritRoot ||
                            ex.group_id == RealmTraitGroup.GroupRealm ||
                            ex.group_id == RealmTraitGroup.GroupAncientInherit ||
                            ex.group_id == RealmTraitGroup.GroupAncientRealm ||
                            (ex.group_id == RealmTraitGroup.GroupDaoBase && ex.id != DAO_DAMAGED))
                            rm.Add(ex);
                    }
                }
                if (rm.Count > 0) actor.removeTraits(rm);
            }
            return false; 
        }
    }
}