using HarmonyLib;
namespace xn.bloodline
{
    [HarmonyPatch(typeof(Tooltip), "showTooltip")]
    internal static class Patch_Tooltip_ShowTooltip_Bloodline
    {
        private static long _lastActorId;
        private static int _lastPosition;
        [HarmonyPostfix]
        private static void Postfix(Tooltip __instance, object pObject, string pType)
        {
            if (pType != "actor" && pType != "actor_king" && pType != "actor_leader")
                return;
            Actor actor = __instance.data?.actor;
            if (actor == null || actor.isRekt())
                return;
            int currentPosition = BloodlineElectionSystem.GetPosition(actor);
            long actorId = actor.getID();
            if (_lastActorId == actorId && _lastPosition == currentPosition)
                return;
            _lastActorId = actorId;
            _lastPosition = currentPosition;
            if (!BloodlineSystem.HasBloodline(actor))
                return;
            bool isFounder = BloodlineSystem.IsFounder(actor);
            string bloodlineType = BloodlineSystem.GetBloodlineType(actor);
            string typeName = BloodlineTypes.GetLocaleName(bloodlineType);
            float concentration = BloodlineSystem.GetConcentration(actor);
            int generation = BloodlineSystem.GetGeneration(actor);
            string position = BloodlineElectionSystem.GetPositionNameForActor(actor);
            string generationText = isFounder ? "始祖" : $"第{generation}代";
            string concColor;
            if (concentration >= 80f)
                concColor = "#FF6666"; 
            else if (concentration >= 50f)
                concColor = "#FFCC33"; 
            else if (concentration >= 20f)
                concColor = "#66CC66"; 
            else
                concColor = "#999999"; 
            __instance.addLineText("血脉", typeName, "#F3961F", pPercent: false, pLocalize: false);
            __instance.addLineText("血脉浓度", $"{concentration:F1}%", concColor, pPercent: false, pLocalize: false);
            __instance.addLineText("血脉辈分", generationText, "#AAAAFF", pPercent: false, pLocalize: false);
            __instance.addLineText("血脉职位", position, "#FFD700", pPercent: false, pLocalize: false);
            if (concentration >= 20f)
            {
                var talents = GetUnlockedTalents(bloodlineType, concentration);
                if (!string.IsNullOrEmpty(talents))
                {
                    __instance.addLineText("血脉天赋", talents, "#AAFFAA", pPercent: false, pLocalize: false);
                }
            }
        }
        private static string GetUnlockedTalents(string bloodlineType, float concentration)
        {
            var talents = new System.Collections.Generic.List<string>();
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                if (concentration >= 20f) talents.Add("太古威严");
                if (concentration >= 50f) talents.Add("血脉压制");
                if (concentration >= 80f) talents.Add("神震");
            }
            else if (bloodlineType == BloodlineTypes.CAOMU)
            {
                if (concentration >= 20f) talents.Add("自然亲和");
                if (concentration >= 50f) talents.Add("寄生孢子");
                if (concentration >= 80f) talents.Add("树界降临");
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO)
            {
                if (concentration >= 20f) talents.Add("幻形");
                if (concentration >= 50f) talents.Add("乱心");
                if (concentration >= 80f) talents.Add("心奴");
            }
            else if (bloodlineType == BloodlineTypes.HOUYI)
            {
                if (concentration >= 20f) talents.Add("鹰眼");
                if (concentration >= 50f) talents.Add("穿云");
                if (concentration >= 80f) talents.Add("落日");
            }
            else if (bloodlineType == BloodlineTypes.HUANGQUAN)
            {
                if (concentration >= 20f) talents.Add("阴体");
                if (concentration >= 50f) talents.Add("拘魂");
                if (concentration >= 80f) talents.Add("冥河渡");
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU)
            {
                if (concentration >= 20f) talents.Add("厄运");
                if (concentration >= 50f) talents.Add("虚弱力场");
                if (concentration >= 80f) talents.Add("灭魂咒");
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                if (concentration >= 20f) talents.Add("寒躯");
                if (concentration >= 50f) talents.Add("冰封");
                if (concentration >= 80f) talents.Add("碎冰");
            }
            else if (bloodlineType == BloodlineTypes.JUMO)
            {
                if (concentration >= 20f) talents.Add("巨体");
                if (concentration >= 50f) talents.Add("活血");
                if (concentration >= 80f) talents.Add("传送之术");
            }
            else if (bloodlineType == BloodlineTypes.KUANGZHANSHI)
            {
                if (concentration >= 20f) talents.Add("怒意");
                if (concentration >= 50f) talents.Add("血怒");
                if (concentration >= 80f) talents.Add("不屈");
            }
            else if (bloodlineType == BloodlineTypes.NIEPAN)
            {
                if (concentration >= 20f) talents.Add("灵火");
                if (concentration >= 50f) talents.Add("余烬");
                if (concentration >= 80f) talents.Add("真火爆裂");
            }
            else if (bloodlineType == BloodlineTypes.JINFA)
            {
                if (concentration >= 20f) talents.Add("绝缘");
                if (concentration >= 50f) talents.Add("破法");
                if (concentration >= 80f) talents.Add("禁魔领域");
            }
            else if (bloodlineType == BloodlineTypes.GUTI)
            {
                if (concentration >= 20f) talents.Add("神皮");
                if (concentration >= 50f) talents.Add("神力");
                if (concentration >= 80f) talents.Add("不灭体");
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                if (concentration >= 20f) talents.Add("长生");
                if (concentration >= 50f) talents.Add("枯荣");
                if (concentration >= 80f) talents.Add("永生");
            }
            else if (bloodlineType == BloodlineTypes.LEIFA)
            {
                if (concentration >= 20f) talents.Add("雷体");
                if (concentration >= 50f) talents.Add("引雷");
                if (concentration >= 80f) talents.Add("雷池");
            }
            else if (bloodlineType == BloodlineTypes.XUANWU)
            {
                if (concentration >= 20f) talents.Add("龟息");
                if (concentration >= 50f) talents.Add("反震");
                if (concentration >= 80f) talents.Add("绝对防御");
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                talents.Add("万毒疆域");
                talents.Add("天煞孤星(代价)");
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                talents.Add("献祭光环");
                talents.Add("克死队友(代价)");
            }
            return talents.Count > 0 ? string.Join("、", talents) : "";
        }
    }
}