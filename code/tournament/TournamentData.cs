using System.Collections.Generic;
using UnityEngine;
namespace xn.tournament
{
    [System.Serializable]
    public class ParticipantDisplayInfo
    {
        public string BaseName;   
        public string Title;      
        public string Suffix;     
        public ParticipantDisplayInfo() { }
        public ParticipantDisplayInfo(string baseName, string title, string suffix)
        {
            BaseName = baseName ?? "";
            Title = title ?? "";
            Suffix = suffix ?? "";
        }
        public string GetFullDisplayName()
        {
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(Title))
                sb.Append("[").Append(Title).Append("]");
            sb.Append(BaseName);
            if (!string.IsNullOrEmpty(Suffix))
                sb.Append("-").Append(Suffix);
            return sb.ToString();
        }
    }
    public class TournamentData
    {
        public int Edition;                          
        public int StartYear;                        
        public int EndYear;                          
        public TournamentState State;                
        public int CurrentRound;                     
        public List<string> ParticipantIds;          
        public List<MatchData> CurrentMatches;       
        public int CurrentMatchIndex;                
        public string ByeParticipantId;              
        public string ChampionId;                    
        public string RunnerUpId;                    
        public string ThirdPlaceId;                  
        public WorldTile ArenaCenterTile;            
        public Dictionary<string, Vector2> OriginalPositions;      
        public Dictionary<string, ParticipantDisplayInfo> ParticipantInfos; 
        public ParticipantDisplayInfo RunnerUpInfo;                
        public ParticipantDisplayInfo ThirdPlaceInfo;              
        public TournamentData()
        {
            ParticipantIds = new List<string>(16);
            CurrentMatches = new List<MatchData>(8);
            OriginalPositions = new Dictionary<string, Vector2>(16);
            ParticipantInfos = new Dictionary<string, ParticipantDisplayInfo>(16);
        }
    }
    public class MatchData
    {
        public string Fighter1Id;
        public string Fighter2Id;
        public string WinnerId;
        public string LoserId;
        public bool IsFinished;
        public bool FightersOnArena; 
        public float StartTime;      
        public bool IsDeathMatch;    
    }
    public enum TournamentState
    {
        None,           
        Preparing,      
        Fighting,       
        RoundEnd,       
        Finished        
    }
}