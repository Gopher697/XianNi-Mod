using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace xn.ui
{
    [HarmonyPatch(typeof(UnitWindow), "showStatsRows")]
    internal static class Patch_UnitWindow_showStatsRows_ExtraRows
    {
        private const string KEY_POSSESSION = "xn.possession.taken"; 
        private const string KEY_TY_COUNT   = "xn.tianyun.count";    
        private const string KEY_REINC      = "xn.reincarnation.count"; 
        private const string KEY_POS_PREV_INFO = "xn.possession.prev_info"; 
        private const string KEY_REINC_PREV_INFO = "xn.reincarnation.prev_info"; 
        private const string KEY_TRIAL_BRIDGE = "xn.trial.bridge"; 
        private const string KEY_XP         = "xn.stat.xiuwei";        
        private const string KEY_ANC_POWER  = "xn.stat.gushen_power";  
        private const string KEY_BEAST_PWR  = "xn.stat.yaoli";         
        private static readonly string[] REALM_IDS = new[]
        {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        private static readonly long[] REALM_THRESHOLDS = new long[]
        {
            100000, 1500000, 4000000, 9600000, 30000000, 80000000,
            150000000, 250000000, 400000000, 600000000,
            700000000, 800000000, 900000000, 980000000,
            1200000000, 1500000000
        };
        private static readonly int[] ANC_THRESHOLDS = new int[]
        {
            5000, 30000, 50000, 100000, 200000, 500000,
            1000000, 1500000, 3000000, 5000000
        };
        private static readonly string[] ANC_STAR_IDS = new[]
        {
            "ancient_01_star","ancient_02_star","ancient_03_star","ancient_04_star","ancient_05_star",
            "ancient_06_star","ancient_07_star","ancient_08_star","ancient_09_star","ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = new[]
        {
            "beast_01_stage","beast_02_stage","beast_03_stage","beast_04_stage","beast_05_stage",
            "beast_06_stage","beast_07_stage","beast_08_stage","beast_09_stage","beast_10_stage"
        };
        [HarmonyPostfix]
        private static void Postfix(UnitWindow __instance)
        {
            var cont = __instance.GetComponentInChildren<StatsRowsContainer>(true);
            var a = xn.access.UnitWindowAccess.GetActor(__instance);
            if (cont == null || a == null || !a.isAlive()) return;
            AddRow(cont, "row_possession_taken", GetPossessionText(a), "row_possession_taken_info");
            AddPreviousLifeRow(cont, a);
            AddRow(cont, "row_tianyun_count", GetTianyunCount(a).ToString(), "row_tianyun_count_info");
            AddReincarnationRow(cont, a);
            AddSlaveRow(cont, a);
            AddTianjiuBridgeRow(cont, a);
            AddRow(cont, "row_next_break_req", GetNextBreakRequirement(a), "row_next_break_req_info");
            AddBloodlineRow(cont, a);
        }
        private static void AddRow(StatsRowsContainer cont, string id, string value, string tooltipId)
        {
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(cont, id);
            if (row == null) return;
            if (row.icon != null)
                row.icon.gameObject.SetActive(false); 
            var name = LocalizedTextManager.getText(id);
            if (row.name_text != null)
                row.name_text.text = string.IsNullOrEmpty(name) ? id : name;
            if (row.value != null)
                row.value.text = value;
            row.setMetaForTooltip(MetaType.None, -1L, tooltipId);
        }
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key, null);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
        private static string F(string key, string fallback, params object[] args)
        {
            return string.Format(T(key, fallback), args);
        }
        private static string Unknown()
        {
            return T("value_unknown", "Unknown");
        }
        private static string None()
        {
            return T("value_none", "None");
        }
        private static string YearText(string year)
        {
            return F("unit_extra_year_value", "{0}", year);
        }
        private static void AddTianjiuBridgeRow(StatsRowsContainer cont, Actor a)
        {
            if (!a.hasTrait("realm_14_gtianzun") && !a.hasTrait("realm_15_half_tatian") && !a.hasTrait("realm_16_tatian"))
            {
                return;
            }
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(cont, "row_tianjiu_bridge");
            if (row == null) return;
            long bridgeL; xn.access.ActorAccess.GetData(a).get(KEY_TRIAL_BRIDGE, out bridgeL, 0L);
            int bridge = (int)bridgeL;
            string displayText;
            string tooltipText;
            if (a.hasTrait("realm_16_tatian"))
            {
                displayText = T("unit_extra_bridge_completed_value", "Completed (9/9)");
                tooltipText = T("unit_extra_bridge_tooltip_completed", "Passed bridges: 9/9\nPassed all 9 bridges and advanced to Heaven Trampling");
            }
            else if (a.hasTrait("realm_15_half_tatian"))
            {
                displayText = $"{bridge}/9";
                if (bridge < 9)
                {
                    tooltipText = F("unit_extra_bridge_tooltip_half_progress", "Passed bridges: {0}/9\nPassed the 5th bridge and advanced to Half-Step Heaven Trampling\nPassing the 9th bridge advances to Heaven Trampling", bridge);
                }
                else
                {
                    tooltipText = F("unit_extra_bridge_tooltip_completed_progress", "Passed bridges: {0}/9\nPassed all 9 bridges and advanced to Heaven Trampling", bridge);
                }
            }
            else
            {
                displayText = $"{bridge}/9";
                if (bridge < 5)
                {
                    tooltipText = F("unit_extra_bridge_tooltip_pre_half", "Passed bridges: {0}/9\nPassing the 5th bridge advances to Half-Step Heaven Trampling", bridge);
                }
                else
                {
                    tooltipText = F("unit_extra_bridge_tooltip_half_progress", "Passed bridges: {0}/9\nPassed the 5th bridge and advanced to Half-Step Heaven Trampling\nPassing the 9th bridge advances to Heaven Trampling", bridge);
                }
            }
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_tianjiu_bridge");
            if (string.IsNullOrEmpty(name)) name = T("row_tianjiu_bridge", "9 Bridges Progress");
            if (row.name_text != null)
                row.name_text.text = name;
            if (row.value != null)
                row.value.text = displayText;
            TooltipDataGetter tooltipData = () =>
            {
                return xn.access.TooltipDataAccess.Create(T("unit_extra_bridge_tip_name", "9 Bridges Progress"), tooltipText);
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_tianjiu_bridge_info", tooltipData);
        }
        private static void AddBloodlineRow(StatsRowsContainer cont, Actor a)
        {
            if (!xn.bloodline.BloodlineSystem.HasBloodline(a))
                return;
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(cont, "row_bloodline_family");
            if (row == null) return;
            bool isFounder = xn.bloodline.BloodlineSystem.IsFounder(a);
            string bloodlineType = xn.bloodline.BloodlineSystem.GetBloodlineType(a);
            string typeName = xn.bloodline.BloodlineTypes.GetLocaleName(bloodlineType);
            float concentration = xn.bloodline.BloodlineSystem.GetConcentration(a);
            int generation = xn.bloodline.BloodlineSystem.GetGeneration(a);
            string position = GetBloodlinePosition(a, isFounder);
            string displayValue = typeName;
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_bloodline_family");
            if (string.IsNullOrEmpty(name)) name = T("row_bloodline_family", "Bloodline Family");
            if (row.name_text != null)
                row.name_text.text = name;
            if (row.value != null)
                row.value.text = displayValue;
            int familyCreatedYear = GetFamilyCreatedYear(a, isFounder);
            string generationText = isFounder ? T("bloodline_founder", "Founder") : F("bloodline_generation_value", "Generation {0}", generation);
            string detailText = F("unit_extra_bloodline_detail_format", "Bloodline Type: {0}\nConcentration: {1:F1}%\nGeneration: {2}\nPosition: {3}", typeName, concentration, generationText, position);
            if (familyCreatedYear > 0)
            {
                detailText += "\n" + F("unit_extra_bloodline_family_created_year", "Family founded in year: {0}", familyCreatedYear);
            }
            string talentStatus = GetBloodlineTalentStatus(a, bloodlineType, concentration);
            if (!string.IsNullOrEmpty(talentStatus))
            {
                detailText += "\n\n" + talentStatus;
            }
            TooltipDataGetter tooltipData = () =>
            {
                return xn.access.TooltipDataAccess.Create(T("row_bloodline_family", "Bloodline Family"), detailText);
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_bloodline_family_info", tooltipData);
        }
        private static string GetBloodlineTalentStatus(Actor a, string bloodlineType, float concentration)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(T("unit_extra_bloodline_talents_header", "Bloodline Talents:"));
            if (bloodlineType == xn.bloodline.BloodlineTypes.TAIGU)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_taigu_majesty", "Immemorial Majesty", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_bloodline_suppression", "Bloodline Suppression", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_divine_shock", "Divine Shock", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.CAOMU)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_nature_affinity", "Nature Affinity", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_parasitic_spores", "Parasitic Spores", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_tree_realm_descent", "Tree Realm Descent", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.MEIHUO)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_phantom_form", "Phantom Form", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_mind_disorder", "Mind Disorder", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_heart_thrall", "Heart Thrall", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.HOUYI)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_eagle_eye", "Eagle Eye", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_pierce_clouds", "Pierce Clouds", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_falling_sun", "Falling Sun", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.HUANGQUAN)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_yin_body", "Yin Body", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_soul_binding", "Soul Binding", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_underworld_crossing", "Underworld Crossing", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.ZUZHOU)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_misfortune", "Misfortune", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_weakening_field", "Weakening Field", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_soul_extinguishing_curse", "Soul-Extinguishing Curse", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.JIHAN)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_frost_body", "Frost Body", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_ice_seal", "Ice Seal", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_shattered_ice", "Shattered Ice", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.JUMO)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_giant_body", "Giant Body", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_blood_vitalization", "Blood Vitalization", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_teleportation_art", "Teleportation Art", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.KUANGZHANSHI)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_wrath", "Wrath", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_blood_rage", "Blood Rage", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_unyielding", "Unyielding", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.NIEPAN)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_spirit_fire", "Spirit Fire", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_embers", "Embers", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_true_fire_burst", "True Fire Burst", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.JINFA)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_insulation", "Insulation", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_spell_breaking", "Spell Breaking", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_anti_magic_domain", "Anti-Magic Domain", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.GUTI)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_divine_skin", "Divine Skin", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_divine_strength", "Divine Strength", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_undying_body", "Undying Body", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.SUIYUE)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_longevity", "Longevity", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_wither_and_flourish", "Wither and Flourish", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_immortality", "Immortality", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.LEIFA)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_thunder_body", "Thunder Body", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_call_thunder", "Call Thunder", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_thunder_pool", "Thunder Pool", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.XUANWU)
            {
                AppendTalentLine(sb, concentration, 20f, "bloodline_talent_turtle_breath", "Turtle Breath", true);
                AppendTalentLine(sb, concentration, 50f, "bloodline_talent_countershock", "Countershock", true);
                AppendTalentLine(sb, concentration, 80f, "bloodline_talent_absolute_defense", "Absolute Defense", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.ENAN)
            {
                AppendActiveTalentLine(sb, "bloodline_talent_myriad_poison_domain", "Myriad Poison Domain", true);
                AppendCostTalentLine(sb, "bloodline_talent_heavenly_fiend_lone_star_cost", "Heavenly Fiend Lone Star Cost", false);
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.TIANSHA)
            {
                AppendActiveTalentLine(sb, "bloodline_talent_sacrificial_aura", "Sacrificial Aura", true);
                AppendCostTalentLine(sb, "bloodline_talent_doomed_allies_cost", "Doomed Allies Cost", false);
            }
            else
            {
                sb.Append(T("unit_extra_effects_in_development", "  (Effects in development...)"));
            }
            return sb.ToString();
        }
        private static void AppendTalentLine(System.Text.StringBuilder sb, float concentration, float required, string talentKey, string talentFallback, bool appendLine)
        {
            string talent = T(talentKey, talentFallback);
            string text = concentration >= required
                ? F("unit_extra_talent_understood", "  [{0}] Understood", talent)
                : F("unit_extra_talent_not_understood", "  [{0}] Not understood (Requires {1}%)", talent, required.ToString("0"));
            if (appendLine) sb.AppendLine(text); else sb.Append(text);
        }
        private static void AppendActiveTalentLine(System.Text.StringBuilder sb, string talentKey, string talentFallback, bool appendLine)
        {
            string text = F("unit_extra_talent_active", "  [{0}] Active", T(talentKey, talentFallback));
            if (appendLine) sb.AppendLine(text); else sb.Append(text);
        }
        private static void AppendCostTalentLine(System.Text.StringBuilder sb, string talentKey, string talentFallback, bool appendLine)
        {
            string text = F("unit_extra_talent_cost_active", "  [{0}] Cost active", T(talentKey, talentFallback));
            if (appendLine) sb.AppendLine(text); else sb.Append(text);
        }
        private static string GetBloodlinePosition(Actor actor, bool isFounder)
        {
            return xn.bloodline.BloodlineElectionSystem.GetPositionNameForActor(actor);
        }
        private static int GetFamilyCreatedYear(Actor actor, bool isFounder)
        {
            if (actor == null) return 0;
            if (isFounder)
            {
                xn.access.ActorAccess.GetData(actor).get(xn.bloodline.BloodlineDataKeys.KEY_FAMILY_CREATED_YEAR, out int year, 0);
                return year;
            }
            long founderId = xn.bloodline.BloodlineSystem.GetFounderId(actor);
            if (founderId <= 0) return 0;
            var founder = World.world?.units?.get(founderId);
            if (founder == null || !founder.isAlive()) return 0;
            founder.data.get(xn.bloodline.BloodlineDataKeys.KEY_FAMILY_CREATED_YEAR, out int createdYear, 0);
            return createdYear;
        }
        private static void AddSlaveRow(StatsRowsContainer cont, Actor a)
        {
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(cont, "row_slave_master"); 
            if (row == null) return;
            MetaType metaType;
            long metaId;
            string text = GetSlaveTextAndMeta(a, out metaType, out metaId);
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_slave_master");
            if (row.name_text != null)
                row.name_text.text = string.IsNullOrEmpty(name) ? "row_slave_master" : name;
            if (row.value != null)
                row.value.text = text;
            if (metaType != MetaType.None && metaId > 0)
                row.setMetaForTooltip(metaType, metaId, "row_slave_master_info");
            else
                row.setMetaForTooltip(MetaType.None, -1L, "row_slave_master_info");
        }
        private static string GetSlaveTextAndMeta(Actor a, out MetaType metaType, out long metaId)
        {
            const string KEY_MASTER_ID = "xn_slave_master_id"; 
            const string KEY_SLAVE_ID = "xn_slave_id";        
            metaType = MetaType.None;
            metaId = -1;
            long masterId; xn.access.ActorAccess.GetData(a).get(KEY_MASTER_ID, out masterId, 0L);
            if (masterId > 0)
            {
                var m = World.world?.units?.get(masterId);
                if (m != null && !m.isRekt())
                {
                    metaType = MetaType.Unit;
                    metaId = m.getID();
                    return F("unit_extra_master_value", "Master: {0}", m.getName() ?? Unknown());
                }
                return F("unit_extra_master_value", "Master: {0}", Unknown());
            }
            long slaveId; xn.access.ActorAccess.GetData(a).get(KEY_SLAVE_ID, out slaveId, 0L);
            if (slaveId > 0)
            {
                var s = World.world?.units?.get(slaveId);
                if (s != null && !s.isRekt())
                {
                    metaType = MetaType.Unit;
                    metaId = s.getID();
                    return F("unit_extra_slave_value", "Slave: {0}", s.getName() ?? Unknown());
                }
                return F("unit_extra_slave_value", "Slave: {0}", Unknown());
            }
            return None();
        }
        private static string GetPossessionText(Actor a)
        {
            int v; xn.access.ActorAccess.GetData(a).get(KEY_POSSESSION, out v, 0);
            var yes = T("value_yes", "Yes");
            var no  = T("value_no", "No");
            return v != 0 ? yes : no;
        }
        private static int GetTianyunCount(Actor a) { int c; xn.access.ActorAccess.GetData(a).get(KEY_TY_COUNT, out c, 0); return c; }
        private static int GetReincCount(Actor a)   { int c; xn.access.ActorAccess.GetData(a).get(KEY_REINC, out c, 0);   return c; }
        private static void AddReincarnationRow(StatsRowsContainer cont, Actor a)
        {
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(cont, "row_reinc_count");
            if (row == null) return;
            int reincCount = GetReincCount(a);
            string displayValue = reincCount.ToString();
            string snapshot; xn.access.ActorAccess.GetData(a).get(KEY_REINC_PREV_INFO, out snapshot, "");
            bool hasPrevInfo = !string.IsNullOrEmpty(snapshot);
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_reinc_count");
            if (string.IsNullOrEmpty(name)) name = T("row_reinc_count", "Reincarnations");
            if (row.name_text != null)
                row.name_text.text = name;
            if (row.value != null)
                row.value.text = displayValue;
            if (hasPrevInfo)
            {
                string[] parts = snapshot.Split('|');
                if (parts.Length >= 8)
                {
                    if (parts.Length < 9)
                    {
                        string[] newParts = new string[9];
                        for (int i = 0; i < parts.Length; i++)
                        {
                            newParts[i] = parts[i];
                        }
                        for (int i = parts.Length; i < 9; i++)
                        {
                            newParts[i] = "";
                        }
                        parts = newParts;
                    }
                    string prevName = (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) ? parts[1] : Unknown();
                    string realmName = (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) ? GetRealmDisplayName(parts[2]) : None();
                    string xpStr = (parts.Length > 3 && long.TryParse(parts[3], out long xpVal)) ? FormatNumber(xpVal) : "0";
                    string wuxinStr = (parts.Length > 4 && !string.IsNullOrEmpty(parts[4])) ? parts[4] : "0";
                    string luckStr = (parts.Length > 5 && !string.IsNullOrEmpty(parts[5])) ? parts[5] : "0";
                    string kingdomStr = (parts.Length > 6 && !string.IsNullOrEmpty(parts[6])) ? parts[6] : None();
                    string speciesStr = (parts.Length > 7 && !string.IsNullOrEmpty(parts[7])) ? parts[7] : Unknown();
                    string yearStr = YearText((parts.Length > 8 && !string.IsNullOrEmpty(parts[8]) && parts[8] != "0") ? parts[8] : Unknown());
                    string detailText = BuildLifeTooltip(prevName, realmName, xpStr, wuxinStr, luckStr, kingdomStr, speciesStr, yearStr);
                    TooltipDataGetter tooltipData = () =>
                    {
                        return xn.access.TooltipDataAccess.Create(T("unit_extra_past_life_title", "Past Life Info"), detailText);
                    };
                    row.setMetaForTooltip(MetaType.None, -1L, "row_reinc_count_info", tooltipData);
                    row.on_click_value = new UnityEngine.Events.UnityAction(() => ShowReincarnationDetails(prevName, realmName, xpStr, wuxinStr, luckStr, kingdomStr, speciesStr, yearStr));
                }
                else
                {
                    row.setMetaForTooltip(MetaType.None, -1L, "row_reinc_count_info");
                }
            }
            else
            {
                row.setMetaForTooltip(MetaType.None, -1L, "row_reinc_count_info");
            }
        }
        private static void ShowReincarnationDetails(string name, string realm, string xp, string wuxin, string luck, string kingdom, string species, string year)
        {
            string details = BuildLifeDetail(T("unit_extra_past_life_title", "Past Life Info"), name, realm, xp, wuxin, luck, kingdom, species, year);
            try
            {
                var infoWindowType = System.Type.GetType("NeoModLoader.ui.InformationWindow, NeoModLoader");
                if (infoWindowType != null)
                {
                    var showMethod = infoWindowType.GetMethod("ShowWindow", new[] { typeof(string), typeof(System.Action) });
                    if (showMethod != null)
                    {
                        showMethod.Invoke(null, new object[] { details, null });
                        return;
                    }
                }
            }
            catch { }
            WorldTip.instance?.show(details, pTranslate: false, pTime: 5f);
        }
        private static void AddPreviousLifeRow(StatsRowsContainer cont, Actor a)
        {
            int taken; xn.access.ActorAccess.GetData(a).get(KEY_POSSESSION, out taken, 0);
            if (taken == 0) return; 
            var row = xn.access.StatsRowsContainerAccess.GetStatRow(cont, "row_previous_life");
            if (row == null) return;
            string snapshot; xn.access.ActorAccess.GetData(a).get(KEY_POS_PREV_INFO, out snapshot, "");
            if (string.IsNullOrEmpty(snapshot))
            {
                row.gameObject.SetActive(false);
                return;
            }
            string[] parts = snapshot.Split('|');
            if (parts.Length < 8)
            {
                row.gameObject.SetActive(false);
                return;
            }
            if (parts.Length < 9)
            {
                string[] newParts = new string[9];
                for (int i = 0; i < parts.Length; i++)
                {
                    newParts[i] = parts[i];
                }
                for (int i = parts.Length; i < 9; i++)
                {
                    newParts[i] = "";
                }
                parts = newParts;
            }
            string prevName = (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) ? parts[1] : Unknown();
            string realmName = (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) ? GetRealmDisplayName(parts[2]) : None();
            string xpStr = (parts.Length > 3 && long.TryParse(parts[3], out long xpVal)) ? FormatNumber(xpVal) : "0";
            string wuxinStr = (parts.Length > 4 && !string.IsNullOrEmpty(parts[4])) ? parts[4] : "0";
            string luckStr = (parts.Length > 5 && !string.IsNullOrEmpty(parts[5])) ? parts[5] : "0";
            string kingdomStr = (parts.Length > 6 && !string.IsNullOrEmpty(parts[6])) ? parts[6] : None();
            string speciesStr = (parts.Length > 7 && !string.IsNullOrEmpty(parts[7])) ? parts[7] : Unknown();
            string yearStr = YearText((parts.Length > 8 && !string.IsNullOrEmpty(parts[8]) && parts[8] != "0") ? parts[8] : Unknown());
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_previous_life");
            if (string.IsNullOrEmpty(name)) name = T("row_previous_life", "Past Life Info");
            if (row.name_text != null)
                row.name_text.text = name;
            if (row.value != null)
                row.value.text = prevName; 
            string detailText = BuildLifeTooltip(prevName, realmName, xpStr, wuxinStr, luckStr, kingdomStr, speciesStr, yearStr);
            TooltipDataGetter tooltipData = () =>
            {
                return xn.access.TooltipDataAccess.Create(T("unit_extra_previous_body_title", "Previous Body Info"), detailText);
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_previous_life_info", tooltipData);
            row.on_click_value = new UnityEngine.Events.UnityAction(() => ShowPreviousLifeDetails(prevName, realmName, xpStr, wuxinStr, luckStr, kingdomStr, speciesStr, yearStr));
            row.gameObject.SetActive(true);
        }
        private static void ShowPreviousLifeDetails(string name, string realm, string xp, string wuxin, string luck, string kingdom, string species, string year)
        {
            string details = BuildLifeDetail(T("unit_extra_previous_body_title", "Previous Body Info"), name, realm, xp, wuxin, luck, kingdom, species, year);
            try
            {
                var infoWindowType = System.Type.GetType("NeoModLoader.ui.InformationWindow, NeoModLoader");
                if (infoWindowType != null)
                {
                    var showMethod = infoWindowType.GetMethod("ShowWindow", new[] { typeof(string), typeof(System.Action) });
                    if (showMethod != null)
                    {
                        showMethod.Invoke(null, new object[] { details, null });
                        return;
                    }
                }
            }
            catch { }
            WorldTip.instance?.show(details, pTranslate: false, pTime: 5f);
        }
        private static string GetRealmDisplayName(string realmId)
        {
            if (string.IsNullOrEmpty(realmId)) return None();
            var trait = AssetManager.traits?.get(realmId) as ActorTrait;
            if (trait != null)
            {
                string localized = trait.getTranslatedName();
                if (!string.IsNullOrEmpty(localized) && localized != realmId)
                    return localized;
            }
            return realmId.Replace("realm_", "").Replace("_", " ");
        }
        private static string FormatNumber(long num)
        {
            if (num >= 1000000000) return (num / 1000000000.0).ToString("F2") + "B";
            if (num >= 1000000) return (num / 1000000.0).ToString("F2") + "M";
            if (num >= 1000) return (num / 1000.0).ToString("F2") + "K";
            return num.ToString();
        }
        private static string BuildLifeTooltip(string name, string realm, string xp, string wuxin, string luck, string kingdom, string species, string year)
        {
            return F("unit_extra_life_tooltip_format", "Name: {0}\nRealm: {1}\nCultivation: {2}\nComprehension: {3}\nLuck/Fate: {4}\nKingdom: {5}\nSpecies: {6}\nDeath Year: {7}", name, realm, xp, wuxin, luck, kingdom, species, year);
        }
        private static string BuildLifeDetail(string title, string name, string realm, string xp, string wuxin, string luck, string kingdom, string species, string year)
        {
            return F("unit_extra_life_detail_format", "{0}\n\nName: {1}\nRealm: {2}\nCultivation: {3}\nComprehension: {4}\nLuck/Fate: {5}\nKingdom: {6}\nSpecies: {7}\nDeath Year: {8}", title, name, realm, xp, wuxin, luck, kingdom, species, year);
        }
        private static string GetNextBreakRequirement(Actor a)
        {
            bool isAncient = HasAnyTraitInSet(a, ANC_STAR_IDS);
            bool isBeast   = HasAnyTraitInSet(a, BEAST_STAGE_IDS);
            if (isAncient)
            {
                int cur = GetCurrentIndex(a, ANC_STAR_IDS);
                int next = cur + 1;
                if (next < 0 || next >= ANC_THRESHOLDS.Length) return T("unit_extra_break_peak", "Peak");
                int power; xn.access.ActorAccess.GetData(a).get(KEY_ANC_POWER, out power, 0);
                long need = Math.Max(0, (long)ANC_THRESHOLDS[next] - power);
                return need == 0 ? T("unit_extra_break_ready", "Ready") : need.ToString();
            }
            if (isBeast)
            {
                int cur = GetCurrentIndex(a, BEAST_STAGE_IDS);
                int next = cur + 1;
                if (next < 0 || next >= ANC_THRESHOLDS.Length) return T("unit_extra_break_peak", "Peak");
                int power; xn.access.ActorAccess.GetData(a).get(KEY_BEAST_PWR, out power, 0);
                long need = Math.Max(0, (long)ANC_THRESHOLDS[next] - power);
                return need == 0 ? T("unit_extra_break_ready", "Ready") : need.ToString();
            }
            int r = GetCurrentIndex(a, REALM_IDS);
            int rn = r + 1;
            if (rn < 0 || rn >= REALM_THRESHOLDS.Length) return T("unit_extra_break_peak", "Peak");
            long xp; xn.access.ActorAccess.GetData(a).get(KEY_XP, out xp, 0L);
            long needRealm = Math.Max(0, REALM_THRESHOLDS[rn] - xp);
            return needRealm == 0 ? T("unit_extra_break_ready", "Ready") : needRealm.ToString();
        }
        private static bool HasAnyTraitInSet(Actor a, string[] ids)
        {
            var ts = a.getTraits(); if (ts == null) return false;
            var set = _tmpCacheSet;
            set.Clear(); for (int i = 0; i < ids.Length; i++) set.Add(ids[i]);
            foreach (var t in ts)
            {
                if (t != null && set.Contains(t.id)) return true;
            }
            return false;
        }
        private static int GetCurrentIndex(Actor a, string[] orderedIds)
        {
            var ts = a.getTraits(); if (ts == null) return -1;
            int idx = -1;
            foreach (var t in ts)
            {
                if (t == null) continue;
                for (int k = 0; k < orderedIds.Length; k++)
                    if (t.id == orderedIds[k] && k > idx) idx = k;
            }
            return idx;
        }
        private static readonly HashSet<string> _tmpCacheSet = new HashSet<string>(32);
    }
    [HarmonyPatch(typeof(KeyValueField), "setMetaForTooltip")]
    internal static class Patch_KeyValueField_setMetaForTooltip_FixNullRef
    {
        private const string GenericTooltipAssetId = "xn_text_info";

        [HarmonyPrefix]
        private static bool Prefix(KeyValueField __instance, MetaType pMetaType, long pMetaId, string pTooltipId = null, TooltipDataGetter pData = null)
        {
            if (pMetaType.isNone() && !string.IsNullOrEmpty(pTooltipId))
            {
                TooltipDataGetter safeData = null;
                if (pData == null)
                {
                    safeData = () => CreateFallbackTooltipData(pTooltipId);
                }
                else
                {
                    var originalData = pData;
                    safeData = () =>
                    {
                        try
                        {
                            if (originalData != null)
                            {
                                var result = originalData();
                                return result ?? CreateFallbackTooltipData(pTooltipId);
                            }
                            return CreateFallbackTooltipData(pTooltipId);
                        }
                        catch
                        {
                            return CreateFallbackTooltipData(pTooltipId);
                        }
                    };
                }
                __instance.on_hover_value = null;
                __instance.on_hover_value_out = new UnityEngine.Events.UnityAction(Tooltip.hideTooltip);
                __instance.on_click_value = null;
                __instance.on_hover_value = new UnityEngine.Events.UnityAction(() =>
                {
                    try
                    {
                        if (safeData != null && __instance != null && !((UnityEngine.Object)__instance == null))
                        {
                            var tooltipData = safeData();
                            if (tooltipData != null)
                            {
                                Tooltip.show(__instance, GetTooltipAssetId(pTooltipId), tooltipData);
                            }
                        }
                    }
                    catch
                    {
                    }
                });
                return false;
            }
            return true;
        }

        private static string GetTooltipAssetId(string tooltipId)
        {
            if (AssetManager.tooltips?.get(tooltipId) != null)
            {
                return tooltipId;
            }
            if (AssetManager.tooltips?.get(GenericTooltipAssetId) != null)
            {
                return GenericTooltipAssetId;
            }
            return tooltipId;
        }

        private static TooltipData CreateFallbackTooltipData(string tooltipId)
        {
            string nameKey = tooltipId;
            if (nameKey.EndsWith("_info", StringComparison.Ordinal))
            {
                nameKey = nameKey.Substring(0, nameKey.Length - "_info".Length);
            }
            return xn.access.TooltipDataAccess.Create(Localize(nameKey, nameKey), Localize(tooltipId, ""));
        }

        private static string Localize(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key, null);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
    }
    [HarmonyPatch(typeof(Tooltip), "showTooltip")]
    internal static class Patch_Tooltip_showTooltip_FixNullRef
    {
        [HarmonyPrefix]
        private static bool Prefix(Tooltip __instance, object pObject, string pType)
        {
            if (__instance.data == null)
            {
                return false; 
            }
            if (__instance.asset == null)
            {
                __instance.asset = AssetManager.tooltips?.get(pType);
                if (__instance.asset == null)
                {
                    return false; 
                }
            }
            return true; 
        }
    }
}
