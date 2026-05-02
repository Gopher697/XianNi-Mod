using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace xn.bloodline
{
    public static class BloodlineElectionSystem
    {
        private static bool _inited;
        private static int _lastCheckYear = -1;
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
        }
        public static void CheckElections()
        {
            if (World.world == null) return;
            int currentYear = Date.getCurrentYear();
            if (currentYear == _lastCheckYear) return;
            _lastCheckYear = currentYear;
            var list = World.world.units.getSimpleList();
            for (int i = 0; i < list.Count; i++)
            {
                var actor = list[i];
                if (actor == null || !actor.isAlive()) continue;
                if (!BloodlineSystem.IsFounder(actor)) continue;
                CheckAndRunElection(actor, currentYear);
            }
        }
        public static void CheckAndRunElection(Actor founder, int currentYear)
        {
            if (founder == null || !founder.isAlive()) return;
            if (!BloodlineSystem.IsFounder(founder)) return;
            xn.access.ActorAccess.GetData(founder).get(BloodlineDataKeys.KEY_FAMILY_CREATED_YEAR, out int createdYear, 0);
            if (createdYear == 0)
            {
                xn.access.ActorAccess.GetData(founder).set(BloodlineDataKeys.KEY_FAMILY_CREATED_YEAR, currentYear);
                createdYear = currentYear;
            }
            if (currentYear - createdYear < BloodlineDataKeys.FAMILY_CREATION_COOLDOWN_YEARS)
            {
                return; 
            }
            xn.access.ActorAccess.GetData(founder).get(BloodlineDataKeys.KEY_LAST_ELECTION_YEAR, out int lastElectionYear, 0);
            if (lastElectionYear > 0 && currentYear - lastElectionYear < BloodlineDataKeys.ELECTION_COOLDOWN_YEARS)
            {
                return; 
            }
            RunElection(founder, currentYear);
        }
        public static void RunElection(Actor founder, int currentYear)
        {
            if (founder == null) return;
            long founderId = founder.getID();
            var allMembers = BloodlineSystem.GetBloodlineDescendants(founderId);
            if (allMembers == null || allMembers.Count == 0) return;
            var candidates = new List<Actor>();
            foreach (var member in allMembers)
            {
                if (member == null || member.isRekt()) continue;
                if (BloodlineSystem.IsFounder(member)) continue;
                if (BloodlineSystem.IsAtavism(member)) continue; 
                float conc = BloodlineSystem.GetConcentration(member);
                if (conc <= 20f) continue;
                candidates.Add(member);
            }
            if (candidates.Count == 0) return;
            candidates.Sort((a, b) =>
            {
                int realmA = GetElectionScore(a);
                int realmB = GetElectionScore(b);
                if (realmA != realmB)
                    return realmB.CompareTo(realmA);
                float concA = BloodlineSystem.GetConcentration(a);
                float concB = BloodlineSystem.GetConcentration(b);
                return concB.CompareTo(concA);
            });
            foreach (var member in allMembers)
            {
                if (member == null || member.isRekt()) continue;
                if (BloodlineSystem.IsFounder(member)) continue;
                xn.access.ActorAccess.GetData(member).set(BloodlineDataKeys.KEY_POSITION, 0);
            }
            string electionResults = "";
            for (int i = 0; i < candidates.Count && i < 9; i++)
            {
                var candidate = candidates[i];
                int position = i + 1; 
                xn.access.ActorAccess.GetData(candidate).set(BloodlineDataKeys.KEY_POSITION, position);
                string positionName = GetPositionName(position);
                if (i == 0)
                {
                    electionResults = T("broadcast_bloodline_election_winner", "{0} was elected {1}", candidate.getName(), positionName);
                }
            }
            xn.access.ActorAccess.GetData(founder).set(BloodlineDataKeys.KEY_LAST_ELECTION_YEAR, currentYear);
            string bloodlineType = BloodlineSystem.GetBloodlineType(founder);
            string typeName = BloodlineTypes.GetLocaleName(bloodlineType);
            if (!string.IsNullOrEmpty(electionResults))
            {
                xn.world.BroadcastSystem.Custom(T("broadcast_bloodline_election_complete", "{0} family completed succession; {1}", typeName, electionResults));
            }
        }
        private static int GetElectionScore(Actor actor)
        {
            int realmIdx = BloodlineSystem.GetRealmIndex(actor);
            if (realmIdx >= 0)
                return realmIdx + 100;
            int ancStar = BloodlineSystem.GetAncientStar(actor);
            if (ancStar > 0)
                return ancStar + 50;
            int beastStage = BloodlineSystem.GetBeastStage(actor);
            if (beastStage > 0)
                return beastStage;
            return 0;
        }
        public static string GetPositionName(int position)
        {
            switch (position)
            {
                case 1: return T("bloodline_position_chief", "Chief");
                case 2: return T("bloodline_position_elder_1", "First Elder");
                case 3: return T("bloodline_position_elder_2", "Second Elder");
                case 4: return T("bloodline_position_elder_3", "Third Elder");
                case 5: return T("bloodline_position_elder_4", "Fourth Elder");
                case 6: return T("bloodline_position_elder_5", "Fifth Elder");
                case 7: return T("bloodline_position_elder_6", "Sixth Elder");
                case 8: return T("bloodline_position_elder_7", "Seventh Elder");
                case 9: return T("bloodline_position_elder_8", "Eighth Elder");
                default: return T("bloodline_position_disciple", "Disciple");
            }
        }
        public static int GetPosition(Actor actor)
        {
            if (actor == null) return 0;
            if (BloodlineSystem.IsFounder(actor)) return -1; 
            xn.access.ActorAccess.GetData(actor).get(BloodlineDataKeys.KEY_POSITION, out int position, 0);
            return position;
        }
        public static string GetPositionNameForActor(Actor actor)
        {
            if (actor == null) return T("common_none", "None");
            if (BloodlineSystem.IsFounder(actor)) return T("bloodline_role_progenitor", "Progenitor");
            if (BloodlineSystem.IsAtavism(actor))
            {
                int generation = BloodlineSystem.GetGeneration(actor);
                return GetAtavismTitle(generation);
            }
            float conc = BloodlineSystem.GetConcentration(actor);
            if (conc <= 20f)
            {
                return T("bloodline_position_outer_disciple", "Outer Disciple");
            }
            int position = GetPosition(actor);
            if (position > 0)
            {
                return GetPositionName(position);
            }
            return T("bloodline_position_inner_disciple", "Inner Disciple");
        }
        private static string GetAtavismTitle(int generation)
        {
            switch (generation)
            {
                case 1: return T("bloodline_atavism_title_1", "Progenitor");
                case 2: return T("bloodline_atavism_title_2", "Second-generation Progenitor");
                case 3: return T("bloodline_atavism_title_3", "Third-generation Progenitor");
                case 4: return T("bloodline_atavism_title_4", "Fourth-generation Progenitor");
                case 5: return T("bloodline_atavism_title_5", "Fifth-generation Progenitor");
                case 6: return T("bloodline_atavism_title_6", "Sixth-generation Progenitor");
                case 7: return T("bloodline_atavism_title_7", "Seventh-generation Progenitor");
                case 8: return T("bloodline_atavism_title_8", "Eighth-generation Progenitor");
                case 9: return T("bloodline_atavism_title_9", "Ninth-generation Progenitor");
                case 10: return T("bloodline_atavism_title_10", "Tenth-generation Progenitor");
                default: return T("bloodline_atavism_title_n", "Generation {0} Progenitor", generation);
            }
        }
    }
}
