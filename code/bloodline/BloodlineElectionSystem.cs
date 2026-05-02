using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace xn.bloodline
{
    public static class BloodlineElectionSystem
    {
        private static bool _inited;
        private static int _lastCheckYear = -1;
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
                    electionResults = $"{candidate.getName()} 当选为{positionName}";
                }
            }
            xn.access.ActorAccess.GetData(founder).set(BloodlineDataKeys.KEY_LAST_ELECTION_YEAR, currentYear);
            string bloodlineType = BloodlineSystem.GetBloodlineType(founder);
            string typeName = BloodlineTypes.GetLocaleName(bloodlineType);
            if (!string.IsNullOrEmpty(electionResults))
            {
                xn.world.BroadcastSystem.Custom($"{typeName}家族完成换届选举，{electionResults}");
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
                case 1: return "族长";
                case 2: return "大长老";
                case 3: return "二长老";
                case 4: return "三长老";
                case 5: return "四长老";
                case 6: return "五长老";
                case 7: return "六长老";
                case 8: return "七长老";
                case 9: return "八长老";
                default: return "弟子";
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
            if (actor == null) return "无";
            if (BloodlineSystem.IsFounder(actor)) return "始祖";
            if (BloodlineSystem.IsAtavism(actor))
            {
                int generation = BloodlineSystem.GetGeneration(actor);
                return GetAtavismTitle(generation);
            }
            float conc = BloodlineSystem.GetConcentration(actor);
            if (conc <= 20f)
            {
                return "外门弟子";
            }
            int position = GetPosition(actor);
            if (position > 0)
            {
                return GetPositionName(position);
            }
            return "内门弟子";
        }
        private static string GetAtavismTitle(int generation)
        {
            switch (generation)
            {
                case 1: return "始祖"; 
                case 2: return "二代始祖";
                case 3: return "三代始祖";
                case 4: return "四代始祖";
                case 5: return "五代始祖";
                case 6: return "六代始祖";
                case 7: return "七代始祖";
                case 8: return "八代始祖";
                case 9: return "九代始祖";
                case 10: return "十代始祖";
                default: return $"第{generation}代始祖";
            }
        }
    }
}