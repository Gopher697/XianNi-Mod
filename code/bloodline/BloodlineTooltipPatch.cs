using HarmonyLib;
namespace xn.bloodline
{
    [HarmonyPatch(typeof(Tooltip), "showTooltip")]
    internal static class Patch_Tooltip_ShowTooltip_Bloodline
    {
        private static long _lastActorId;
        private static int _lastPosition;
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
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
            string position = GetPositionDisplay(actor, currentPosition, isFounder, generation, concentration);
            string generationText = isFounder ? T("bloodline_founder", "Founder") : T("bloodline_generation_value", "Generation {0}", generation);
            string concColor;
            if (concentration >= 80f)
                concColor = "#FF6666"; 
            else if (concentration >= 50f)
                concColor = "#FFCC33"; 
            else if (concentration >= 20f)
                concColor = "#66CC66"; 
            else
                concColor = "#999999"; 
            xn.access.TooltipAccess.AddLineText(__instance, T("bloodline_type", "Bloodline Type"), typeName, "#F3961F", percent: false, localize: false);
            xn.access.TooltipAccess.AddLineText(__instance, T("bloodline_concentration", "Concentration"), $"{concentration:F1}%", concColor, percent: false, localize: false);
            xn.access.TooltipAccess.AddLineText(__instance, T("bloodline_generation", "Generation"), generationText, "#AAAAFF", percent: false, localize: false);
            xn.access.TooltipAccess.AddLineText(__instance, T("bloodline_position", "Position"), position, "#FFD700", percent: false, localize: false);
            if (concentration >= 20f)
            {
                var talents = GetUnlockedTalents(bloodlineType, concentration);
                if (!string.IsNullOrEmpty(talents))
                {
                    xn.access.TooltipAccess.AddLineText(__instance, T("bloodline_talents", "Bloodline Talents"), talents, "#AAFFAA", percent: false, localize: false);
                }
            }
        }
        private static string GetPositionDisplay(Actor actor, int position, bool isFounder, int generation, float concentration)
        {
            if (actor == null) return T("bloodline_position_none", "None");
            if (isFounder) return T("bloodline_founder", "Founder");
            if (BloodlineSystem.IsAtavism(actor))
            {
                switch (generation)
                {
                    case 1: return T("bloodline_founder", "Founder");
                    case 2: return T("bloodline_atavism_founder_02", "Second Generation Founder");
                    case 3: return T("bloodline_atavism_founder_03", "Third Generation Founder");
                    case 4: return T("bloodline_atavism_founder_04", "Fourth Generation Founder");
                    case 5: return T("bloodline_atavism_founder_05", "Fifth Generation Founder");
                    case 6: return T("bloodline_atavism_founder_06", "Sixth Generation Founder");
                    case 7: return T("bloodline_atavism_founder_07", "Seventh Generation Founder");
                    case 8: return T("bloodline_atavism_founder_08", "Eighth Generation Founder");
                    case 9: return T("bloodline_atavism_founder_09", "Ninth Generation Founder");
                    case 10: return T("bloodline_atavism_founder_10", "Tenth Generation Founder");
                    default: return T("bloodline_atavism_founder_n", "Generation {0} Founder", generation);
                }
            }
            if (concentration <= 20f) return T("bloodline_outer_disciples", "Outer Disciples");
            if (position > 0)
            {
                switch (position)
                {
                    case 1: return T("bloodline_chief", "Clan Head");
                    case 2: return T("bloodline_elder_01", "First Elder");
                    case 3: return T("bloodline_elder_02", "Second Elder");
                    case 4: return T("bloodline_elder_03", "Third Elder");
                    case 5: return T("bloodline_elder_04", "Fourth Elder");
                    case 6: return T("bloodline_elder_05", "Fifth Elder");
                    case 7: return T("bloodline_elder_06", "Sixth Elder");
                    case 8: return T("bloodline_elder_07", "Seventh Elder");
                    case 9: return T("bloodline_elder_08", "Eighth Elder");
                    default: return T("bloodline_disciple", "Disciple");
                }
            }
            return T("bloodline_inner_disciples", "Inner Disciples");
        }
        private static void AddTalent(System.Collections.Generic.List<string> talents, string key, string fallback)
        {
            talents.Add(T(key, fallback));
        }
        private static string GetUnlockedTalents(string bloodlineType, float concentration)
        {
            var talents = new System.Collections.Generic.List<string>();
            if (bloodlineType == BloodlineTypes.TAIGU)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_taigu_majesty", "Immemorial Majesty");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_bloodline_suppression", "Bloodline Suppression");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_divine_shock", "Divine Shock");
            }
            else if (bloodlineType == BloodlineTypes.CAOMU)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_nature_affinity", "Nature Affinity");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_parasitic_spores", "Parasitic Spores");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_tree_realm_descent", "Tree Realm Descent");
            }
            else if (bloodlineType == BloodlineTypes.MEIHUO)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_phantom_form", "Phantom Form");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_mind_disorder", "Mind Disorder");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_heart_thrall", "Heart Thrall");
            }
            else if (bloodlineType == BloodlineTypes.HOUYI)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_eagle_eye", "Eagle Eye");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_pierce_clouds", "Pierce the Clouds");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_falling_sun", "Falling Sun");
            }
            else if (bloodlineType == BloodlineTypes.HUANGQUAN)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_yin_body", "Yin Body");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_soul_binding", "Soul Binding");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_underworld_crossing", "Underworld Crossing");
            }
            else if (bloodlineType == BloodlineTypes.ZUZHOU)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_misfortune", "Misfortune");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_weakening_field", "Weakening Field");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_soul_extinguishing_curse", "Soul-Extinguishing Curse");
            }
            else if (bloodlineType == BloodlineTypes.JIHAN)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_frost_body", "Frost Body");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_ice_seal", "Ice Seal");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_shattered_ice", "Shattered Ice");
            }
            else if (bloodlineType == BloodlineTypes.JUMO)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_giant_body", "Giant Body");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_blood_vitalization", "Blood Vitalization");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_teleportation_art", "Teleportation Art");
            }
            else if (bloodlineType == BloodlineTypes.KUANGZHANSHI)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_wrath", "Wrath");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_blood_rage", "Blood Rage");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_unyielding", "Unyielding");
            }
            else if (bloodlineType == BloodlineTypes.NIEPAN)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_spirit_fire", "Spirit Fire");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_embers", "Embers");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_true_fire_burst", "True Fire Burst");
            }
            else if (bloodlineType == BloodlineTypes.JINFA)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_insulation", "Insulation");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_spell_breaking", "Spell Breaking");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_anti_magic_domain", "Anti-Magic Domain");
            }
            else if (bloodlineType == BloodlineTypes.GUTI)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_divine_skin", "Divine Skin");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_divine_strength", "Divine Strength");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_undying_body", "Undying Body");
            }
            else if (bloodlineType == BloodlineTypes.SUIYUE)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_longevity", "Longevity");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_wither_and_flourish", "Wither and Flourish");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_immortality", "Immortality");
            }
            else if (bloodlineType == BloodlineTypes.LEIFA)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_thunder_body", "Thunder Body");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_call_thunder", "Call Thunder");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_thunder_pool", "Thunder Pool");
            }
            else if (bloodlineType == BloodlineTypes.XUANWU)
            {
                if (concentration >= 20f) AddTalent(talents, "bloodline_talent_turtle_breath", "Turtle Breath");
                if (concentration >= 50f) AddTalent(talents, "bloodline_talent_countershock", "Countershock");
                if (concentration >= 80f) AddTalent(talents, "bloodline_talent_absolute_defense", "Absolute Defense");
            }
            else if (bloodlineType == BloodlineTypes.ENAN)
            {
                AddTalent(talents, "bloodline_talent_myriad_poison_domain", "Myriad Poison Domain");
                AddTalent(talents, "bloodline_talent_heavenly_fiend_lone_star_cost", "Heavenly Fiend Lone Star (Cost)");
            }
            else if (bloodlineType == BloodlineTypes.TIANSHA)
            {
                AddTalent(talents, "bloodline_talent_sacrificial_aura", "Sacrificial Aura");
                AddTalent(talents, "bloodline_talent_doomed_allies_cost", "Doomed Allies (Cost)");
            }
            return talents.Count > 0 ? string.Join(T("bloodline_talent_separator", ", "), talents) : "";
        }
    }
}
