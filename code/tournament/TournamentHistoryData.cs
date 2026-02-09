using System;
using System.Collections.Generic;
namespace xn.tournament
{
    [Serializable]
    public class TournamentHistoryData
    {
        public int Edition;                          
        public int Year;                             
        public int EndYear;                          
        public string ChampionId;                    
        public ParticipantDisplayInfo ChampionInfo;  
        public string RunnerUpId;                    
        public ParticipantDisplayInfo RunnerUpInfo;  
        public string ThirdPlaceId;                  
        public ParticipantDisplayInfo ThirdPlaceInfo;
        public List<ParticipantDisplayInfo> ParticipantInfos; 
        public int TotalRounds;                      
        public string Summary;                       
        public double Timestamp;                     
        public string ChampionName;
        public string RunnerUpName;
        public string ThirdPlaceName;
        public List<string> ParticipantNames;
        public TournamentHistoryData()
        {
            ParticipantInfos = new List<ParticipantDisplayInfo>();
            ParticipantNames = new List<string>();
        }
    }
}