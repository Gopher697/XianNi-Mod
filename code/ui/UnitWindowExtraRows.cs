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
            var a = __instance.actor;
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
            var row = cont.getStatRow(id);
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
        private static void AddTianjiuBridgeRow(StatsRowsContainer cont, Actor a)
        {
            if (!a.hasTrait("realm_14_gtianzun") && !a.hasTrait("realm_15_half_tatian") && !a.hasTrait("realm_16_tatian"))
            {
                return;
            }
            var row = cont.getStatRow("row_tianjiu_bridge");
            if (row == null) return;
            long bridgeL; a.data.get(KEY_TRIAL_BRIDGE, out bridgeL, 0L);
            int bridge = (int)bridgeL;
            string displayText;
            string tooltipText;
            if (a.hasTrait("realm_16_tatian"))
            {
                displayText = "已完成（9/9）";
                tooltipText = "已通过桥数：9/9\n已通过全部9桥，晋升为踏天";
            }
            else if (a.hasTrait("realm_15_half_tatian"))
            {
                displayText = $"{bridge}/9";
                if (bridge < 9)
                {
                    tooltipText = $"已通过桥数：{bridge}/9\n已通过第5桥，晋升为半步踏天\n通过第9桥可晋升为踏天";
                }
                else
                {
                    tooltipText = $"已通过桥数：{bridge}/9\n已通过全部9桥，晋升为踏天";
                }
            }
            else
            {
                displayText = $"{bridge}/9";
                if (bridge < 5)
                {
                    tooltipText = $"已通过桥数：{bridge}/9\n通过第5桥可晋升为半步踏天";
                }
                else
                {
                    tooltipText = $"已通过桥数：{bridge}/9\n已通过第5桥，晋升为半步踏天\n通过第9桥可晋升为踏天";
                }
            }
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_tianjiu_bridge");
            if (string.IsNullOrEmpty(name)) name = "踏天九桥";
            if (row.name_text != null)
                row.name_text.text = name;
            if (row.value != null)
                row.value.text = displayText;
            TooltipDataGetter tooltipData = () =>
            {
                var data = new TooltipData();
                data._tip_name = "踏天九桥进度";
                data._tip_description = tooltipText;
                return data;
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_tianjiu_bridge_info", tooltipData);
        }
        private static void AddBloodlineRow(StatsRowsContainer cont, Actor a)
        {
            if (!xn.bloodline.BloodlineSystem.HasBloodline(a))
                return;
            var row = cont.getStatRow("row_bloodline_family");
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
            if (string.IsNullOrEmpty(name)) name = "血脉家族";
            if (row.name_text != null)
                row.name_text.text = name;
            if (row.value != null)
                row.value.text = displayValue;
            int familyCreatedYear = GetFamilyCreatedYear(a, isFounder);
            string generationText = isFounder ? "始祖" : $"第{generation}代";
            string detailText = $"血脉类型：{typeName}\n血脉浓度：{concentration:F1}%\n血脉辈分：{generationText}\n血脉职位：{position}";
            if (familyCreatedYear > 0)
            {
                detailText += $"\n家族创建年份：{familyCreatedYear}年";
            }
            string talentStatus = GetBloodlineTalentStatus(a, bloodlineType, concentration);
            if (!string.IsNullOrEmpty(talentStatus))
            {
                detailText += "\n\n" + talentStatus;
            }
            TooltipDataGetter tooltipData = () =>
            {
                var data = new TooltipData();
                data._tip_name = "血脉家族";
                data._tip_description = detailText;
                return data;
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_bloodline_family_info", tooltipData);
        }
        private static string GetBloodlineTalentStatus(Actor a, string bloodlineType, float concentration)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("血脉天赋：");
            if (bloodlineType == xn.bloodline.BloodlineTypes.TAIGU)
            {
                sb.AppendLine(concentration >= 20f ? "  [太古威严] 已领悟" : "  [太古威严] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [血脉压制] 已领悟" : "  [血脉压制] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [神震] 已领悟" : "  [神震] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.CAOMU)
            {
                sb.AppendLine(concentration >= 20f ? "  [自然亲和] 已领悟" : "  [自然亲和] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [寄生孢子] 已领悟" : "  [寄生孢子] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [树界降临] 已领悟" : "  [树界降临] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.MEIHUO)
            {
                sb.AppendLine(concentration >= 20f ? "  [幻形] 已领悟" : "  [幻形] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [乱心] 已领悟" : "  [乱心] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [心奴] 已领悟" : "  [心奴] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.HOUYI)
            {
                sb.AppendLine(concentration >= 20f ? "  [鹰眼] 已领悟" : "  [鹰眼] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [穿云] 已领悟" : "  [穿云] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [落日] 已领悟" : "  [落日] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.HUANGQUAN)
            {
                sb.AppendLine(concentration >= 20f ? "  [阴体] 已领悟" : "  [阴体] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [拘魂] 已领悟" : "  [拘魂] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [冥河渡] 已领悟" : "  [冥河渡] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.ZUZHOU)
            {
                sb.AppendLine(concentration >= 20f ? "  [厄运] 已领悟" : "  [厄运] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [虚弱力场] 已领悟" : "  [虚弱力场] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [灭魂咒] 已领悟" : "  [灭魂咒] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.JIHAN)
            {
                sb.AppendLine(concentration >= 20f ? "  [寒躯] 已领悟" : "  [寒躯] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [冰封] 已领悟" : "  [冰封] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [碎冰] 已领悟" : "  [碎冰] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.JUMO)
            {
                sb.AppendLine(concentration >= 20f ? "  [巨体] 已领悟" : "  [巨体] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [活血] 已领悟" : "  [活血] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [传送之术] 已领悟" : "  [传送之术] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.KUANGZHANSHI)
            {
                sb.AppendLine(concentration >= 20f ? "  [怒意] 已领悟" : "  [怒意] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [血怒] 已领悟" : "  [血怒] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [不屈] 已领悟" : "  [不屈] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.NIEPAN)
            {
                sb.AppendLine(concentration >= 20f ? "  [灵火] 已领悟" : "  [灵火] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [余烬] 已领悟" : "  [余烬] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [真火爆裂] 已领悟" : "  [真火爆裂] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.JINFA)
            {
                sb.AppendLine(concentration >= 20f ? "  [绝缘] 已领悟" : "  [绝缘] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [破法] 已领悟" : "  [破法] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [禁魔领域] 已领悟" : "  [禁魔领域] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.GUTI)
            {
                sb.AppendLine(concentration >= 20f ? "  [神皮] 已领悟" : "  [神皮] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [神力] 已领悟" : "  [神力] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [不灭体] 已领悟" : "  [不灭体] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.SUIYUE)
            {
                sb.AppendLine(concentration >= 20f ? "  [长生] 已领悟" : "  [长生] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [枯荣] 已领悟" : "  [枯荣] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [永生] 已领悟" : "  [永生] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.LEIFA)
            {
                sb.AppendLine(concentration >= 20f ? "  [雷体] 已领悟" : "  [雷体] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [引雷] 已领悟" : "  [引雷] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [雷池] 已领悟" : "  [雷池] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.XUANWU)
            {
                sb.AppendLine(concentration >= 20f ? "  [龟息] 已领悟" : "  [龟息] 未领悟 (需20%)");
                sb.AppendLine(concentration >= 50f ? "  [反震] 已领悟" : "  [反震] 未领悟 (需50%)");
                sb.Append(concentration >= 80f ? "  [绝对防御] 已领悟" : "  [绝对防御] 未领悟 (需80%)");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.ENAN)
            {
                sb.AppendLine("  [万毒疆域] 已激活");
                sb.Append("  [天煞孤星] 代价生效中");
            }
            else if (bloodlineType == xn.bloodline.BloodlineTypes.TIANSHA)
            {
                sb.AppendLine("  [献祭光环] 已激活");
                sb.Append("  [克死队友] 代价生效中");
            }
            else
            {
                sb.Append("  (效果开发中...)");
            }
            return sb.ToString();
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
                actor.data.get(xn.bloodline.BloodlineDataKeys.KEY_FAMILY_CREATED_YEAR, out int year, 0);
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
            var row = cont.getStatRow("row_slave_master"); 
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
            long masterId; a.data.get(KEY_MASTER_ID, out masterId, 0L);
            if (masterId > 0)
            {
                var m = World.world?.units?.get(masterId);
                if (m != null && !m.isRekt())
                {
                    metaType = MetaType.Unit;
                    metaId = m.getID();
                    return "主人：" + (m.getName() ?? "未知");
                }
                return "主人：未知";
            }
            long slaveId; a.data.get(KEY_SLAVE_ID, out slaveId, 0L);
            if (slaveId > 0)
            {
                var s = World.world?.units?.get(slaveId);
                if (s != null && !s.isRekt())
                {
                    metaType = MetaType.Unit;
                    metaId = s.getID();
                    return "奴仆：" + (s.getName() ?? "未知");
                }
                return "奴仆：未知";
            }
            return LocalizedTextManager.getText("value_none") ?? "无";
        }
        private static string GetPossessionText(Actor a)
        {
            int v; a.data.get(KEY_POSSESSION, out v, 0);
            var yes = LocalizedTextManager.getText("value_yes"); if (string.IsNullOrEmpty(yes)) yes = "是";
            var no  = LocalizedTextManager.getText("value_no");  if (string.IsNullOrEmpty(no))  no  = "否";
            return v != 0 ? yes : no;
        }
        private static int GetTianyunCount(Actor a) { int c; a.data.get(KEY_TY_COUNT, out c, 0); return c; }
        private static int GetReincCount(Actor a)   { int c; a.data.get(KEY_REINC, out c, 0);   return c; }
        private static void AddReincarnationRow(StatsRowsContainer cont, Actor a)
        {
            var row = cont.getStatRow("row_reinc_count");
            if (row == null) return;
            int reincCount = GetReincCount(a);
            string displayValue = reincCount.ToString();
            string snapshot; a.data.get(KEY_REINC_PREV_INFO, out snapshot, "");
            bool hasPrevInfo = !string.IsNullOrEmpty(snapshot);
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_reinc_count");
            if (string.IsNullOrEmpty(name)) name = "轮回次数";
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
                    string prevName = (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) ? parts[1] : "未知";
                    string realmName = (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) ? GetRealmDisplayName(parts[2]) : "无";
                    string xpStr = (parts.Length > 3 && long.TryParse(parts[3], out long xpVal)) ? FormatNumber(xpVal) : "0";
                    string wuxinStr = (parts.Length > 4 && !string.IsNullOrEmpty(parts[4])) ? parts[4] : "0";
                    string luckStr = (parts.Length > 5 && !string.IsNullOrEmpty(parts[5])) ? parts[5] : "0";
                    string kingdomStr = (parts.Length > 6 && !string.IsNullOrEmpty(parts[6])) ? parts[6] : "无";
                    string speciesStr = (parts.Length > 7 && !string.IsNullOrEmpty(parts[7])) ? parts[7] : "未知";
                    string yearStr = (parts.Length > 8 && !string.IsNullOrEmpty(parts[8]) && parts[8] != "0") ? parts[8] : "未知";
                    string detailText = $"姓名：{prevName}\n境界：{realmName}\n修为：{xpStr}\n悟性：{wuxinStr}\n气运：{luckStr}\n国家：{kingdomStr}\n物种：{speciesStr}\n死亡年份：{yearStr}年";
                    TooltipDataGetter tooltipData = () =>
                    {
                        var data = new TooltipData();
                        data._tip_name = "前世信息";
                        data._tip_description = detailText;
                        return data;
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
            string details = $"前世信息\n\n";
            details += $"姓名：{name}\n";
            details += $"境界：{realm}\n";
            details += $"修为：{xp}\n";
            details += $"悟性：{wuxin}\n";
            details += $"气运：{luck}\n";
            details += $"国家：{kingdom}\n";
            details += $"物种：{species}\n";
            details += $"死亡年份：{year}年";
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
            int taken; a.data.get(KEY_POSSESSION, out taken, 0);
            if (taken == 0) return; 
            var row = cont.getStatRow("row_previous_life");
            if (row == null) return;
            string snapshot; a.data.get(KEY_POS_PREV_INFO, out snapshot, "");
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
            string prevName = (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) ? parts[1] : "未知";
            string realmName = (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) ? GetRealmDisplayName(parts[2]) : "无";
            string xpStr = (parts.Length > 3 && long.TryParse(parts[3], out long xpVal)) ? FormatNumber(xpVal) : "0";
            string wuxinStr = (parts.Length > 4 && !string.IsNullOrEmpty(parts[4])) ? parts[4] : "0";
            string luckStr = (parts.Length > 5 && !string.IsNullOrEmpty(parts[5])) ? parts[5] : "0";
            string kingdomStr = (parts.Length > 6 && !string.IsNullOrEmpty(parts[6])) ? parts[6] : "无";
            string speciesStr = (parts.Length > 7 && !string.IsNullOrEmpty(parts[7])) ? parts[7] : "未知";
            string yearStr = (parts.Length > 8 && !string.IsNullOrEmpty(parts[8]) && parts[8] != "0") ? parts[8] : "未知";
            if (row.icon != null)
                row.icon.gameObject.SetActive(false);
            var name = LocalizedTextManager.getText("row_previous_life");
            if (string.IsNullOrEmpty(name)) name = "前身";
            if (row.name_text != null)
                row.name_text.text = name;
            if (row.value != null)
                row.value.text = prevName; 
            string detailText = $"姓名：{prevName}\n境界：{realmName}\n修为：{xpStr}\n悟性：{wuxinStr}\n气运：{luckStr}\n国家：{kingdomStr}\n物种：{speciesStr}\n死亡年份：{yearStr}年";
            TooltipDataGetter tooltipData = () =>
            {
                var data = new TooltipData();
                data._tip_name = "前身信息";
                data._tip_description = detailText;
                return data;
            };
            row.setMetaForTooltip(MetaType.None, -1L, "row_previous_life_info", tooltipData);
            row.on_click_value = new UnityEngine.Events.UnityAction(() => ShowPreviousLifeDetails(prevName, realmName, xpStr, wuxinStr, luckStr, kingdomStr, speciesStr, yearStr));
            row.gameObject.SetActive(true);
        }
        private static void ShowPreviousLifeDetails(string name, string realm, string xp, string wuxin, string luck, string kingdom, string species, string year)
        {
            string details = $"前身信息\n\n";
            details += $"姓名：{name}\n";
            details += $"境界：{realm}\n";
            details += $"修为：{xp}\n";
            details += $"悟性：{wuxin}\n";
            details += $"气运：{luck}\n";
            details += $"国家：{kingdom}\n";
            details += $"物种：{species}\n";
            details += $"死亡年份：{year}年";
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
            if (string.IsNullOrEmpty(realmId)) return "无";
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
        private static string GetNextBreakRequirement(Actor a)
        {
            bool isAncient = HasAnyTraitInSet(a, ANC_STAR_IDS);
            bool isBeast   = HasAnyTraitInSet(a, BEAST_STAGE_IDS);
            if (isAncient)
            {
                int cur = GetCurrentIndex(a, ANC_STAR_IDS);
                int next = cur + 1;
                if (next < 0 || next >= ANC_THRESHOLDS.Length) return "已至巅峰";
                int power; a.data.get(KEY_ANC_POWER, out power, 0);
                long need = Math.Max(0, (long)ANC_THRESHOLDS[next] - power);
                return need == 0 ? "可突破" : need.ToString();
            }
            if (isBeast)
            {
                int cur = GetCurrentIndex(a, BEAST_STAGE_IDS);
                int next = cur + 1;
                if (next < 0 || next >= ANC_THRESHOLDS.Length) return "已至巅峰";
                int power; a.data.get(KEY_BEAST_PWR, out power, 0);
                long need = Math.Max(0, (long)ANC_THRESHOLDS[next] - power);
                return need == 0 ? "可突破" : need.ToString();
            }
            int r = GetCurrentIndex(a, REALM_IDS);
            int rn = r + 1;
            if (rn < 0 || rn >= REALM_THRESHOLDS.Length) return "已至巅峰";
            long xp; a.data.get(KEY_XP, out xp, 0L);
            long needRealm = Math.Max(0, REALM_THRESHOLDS[rn] - xp);
            return needRealm == 0 ? "可突破" : needRealm.ToString();
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
        [HarmonyPrefix]
        private static bool Prefix(KeyValueField __instance, MetaType pMetaType, long pMetaId, string pTooltipId = null, TooltipDataGetter pData = null)
        {
            if (pMetaType.isNone() && !string.IsNullOrEmpty(pTooltipId))
            {
                TooltipDataGetter safeData = null;
                if (pData == null)
                {
                    safeData = () => new TooltipData
                    {
                        _tip_name = pTooltipId,
                        _tip_description = ""
                    };
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
                                return result ?? new TooltipData { _tip_name = pTooltipId, _tip_description = "" };
                            }
                            return new TooltipData { _tip_name = pTooltipId, _tip_description = "" };
                        }
                        catch
                        {
                            return new TooltipData { _tip_name = pTooltipId, _tip_description = "" };
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
                                Tooltip.show(__instance, pTooltipId, tooltipData);
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