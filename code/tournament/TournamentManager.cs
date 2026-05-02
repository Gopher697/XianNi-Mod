using System.Collections.Generic;
using UnityEngine;
namespace xn.tournament
{
    using xn.world;
    public static class TournamentManager
    {
        private static bool _inited;
        private static int _editionCounter;
        private static TournamentData _current;
        private static float _tickTimer;
        private const float TICK_INTERVAL = 0.5f;
        private static Dictionary<string, Actor> _actorCache = new Dictionary<string, Actor>();
        private static Dictionary<string, float> _deathTriggers = new Dictionary<string, float>(); 
        public static bool IsRunning => _current != null && _current.State != TournamentState.None && _current.State != TournamentState.Finished;
        public static bool IsFinalRound => _current != null && _current.ParticipantIds != null && _current.ParticipantIds.Count == 2;
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _editionCounter = 0;
        }
        internal static void SetEditionCounter(int edition)
        {
            _editionCounter = edition;
        }
        public static void Update()
        {
            if (_current == null || _current.State == TournamentState.None || _current.State == TournamentState.Finished) return;
            _tickTimer += Time.deltaTime;
            if (_tickTimer < TICK_INTERVAL) return;
            _tickTimer = 0f;
            switch (_current.State)
            {
                case TournamentState.Preparing:
                    OnPreparing();
                    break;
                case TournamentState.Fighting:
                    OnFighting();
                    break;
                case TournamentState.RoundEnd:
                    OnRoundEnd();
                    break;
            }
        }
        public static bool StartTournament()
        {
            if (IsRunning)
            {
                xn.world.BroadcastSystem.Custom(T("broadcast_tournament_already_running", "Tournament is already underway; a new match cannot begin"));
                return false;
            }
            int participantCount = Random.Range(10, 21);
            var topActors = GetTopPowerActors(participantCount);
            if (topActors == null || topActors.Count < 2)
            {
                xn.world.BroadcastSystem.Custom(T("broadcast_tournament_not_enough_participants", "Not enough participants to hold the tournament"));
                return false;
            }
            var arenaTile = TournamentArena.FindOceanTileForArena();
            if (arenaTile == null)
            {
                xn.world.BroadcastSystem.Custom(T("broadcast_tournament_no_arena_tile", "No suitable ocean site was found for the arena"));
                return false;
            }
            _editionCounter++;
            _current = new TournamentData
            {
                Edition = _editionCounter,
                StartYear = Date.getYear(World.world.getCurWorldTime()),
                State = TournamentState.Preparing,
                CurrentRound = 0,
                CurrentMatchIndex = 0,
                ArenaCenterTile = arenaTile
            };
            _actorCache.Clear();
            foreach (var a in topActors)
            {
                if (a == null || !a.isAlive()) continue;
                string id = a.getID().ToString();
                _current.ParticipantIds.Add(id);
                _current.OriginalPositions[id] = new Vector2(a.current_tile.x, a.current_tile.y);
                _current.ParticipantInfos[id] = GetParticipantDisplayInfo(a); 
                _actorCache[id] = a; 
            }
            xn.world.BroadcastSystem.Custom(T("broadcast_tournament_starting", "Tournament #{0} is about to begin! {1} fighters have entered", _editionCounter, _current.ParticipantIds.Count));
            TournamentArena.BuildArena(arenaTile);
            return true;
        }
        private static void OnPreparing()
        {
            if (_current == null) return;
            if (!TournamentArena.IsArenaReady(_current.ArenaCenterTile)) return;
            StartNextRound();
        }
        private static void OnFighting()
        {
            if (_current == null || _current.CurrentMatches == null) return;
            if (_current.CurrentMatchIndex >= _current.CurrentMatches.Count)
            {
                _current.State = TournamentState.RoundEnd;
                return;
            }
            var match = _current.CurrentMatches[_current.CurrentMatchIndex];
            if (match == null)
            {
                _current.CurrentMatchIndex++;
                return;
            }
            if (!match.FightersOnArena)
            {
                TeleportFightersToArena(match);
                match.FightersOnArena = true;
                TournamentMatch.StartMatch(match);
                return;
            }
            if (!match.IsFinished)
            {
                TournamentMatch.UpdateMatch(match);
            }
            if (match.IsFinished)
            {
                HandleMatchEnd(match);
                _current.CurrentMatchIndex++;
                if (_current.CurrentMatchIndex < _current.CurrentMatches.Count)
                {
                    var nextMatch = _current.CurrentMatches[_current.CurrentMatchIndex];
                    var f1 = GetActorById(nextMatch.Fighter1Id);
                    var f2 = GetActorById(nextMatch.Fighter2Id);
                    if (f1 != null && f2 != null)
                    {
                        xn.world.BroadcastSystem.Custom(T("broadcast_tournament_next_match", "Next match: {0} VS {1}", f1.getName(), f2.getName()));
                    }
                }
            }
        }
        private static void HandleMatchEnd(MatchData match)
        {
            if (match == null) return;
            if (!string.IsNullOrEmpty(match.LoserId))
            {
                TeleportBackToOriginal(match.LoserId);
            }
            if (!string.IsNullOrEmpty(match.WinnerId))
            {
                TeleportBackToOriginal(match.WinnerId);
            }
        }
        private static void OnRoundEnd()
        {
            if (_current == null) return;
            var winners = new List<string>(8);
            foreach (var match in _current.CurrentMatches)
            {
                if (match != null && !string.IsNullOrEmpty(match.WinnerId))
                    winners.Add(match.WinnerId);
            }
            if (!string.IsNullOrEmpty(_current.ByeParticipantId))
            {
                winners.Add(_current.ByeParticipantId);
                _current.ByeParticipantId = null;
            }
            int participantCount = _current.ParticipantIds.Count;
            if (participantCount == 2)
            {
                foreach (var match in _current.CurrentMatches)
                {
                    if (match != null && !string.IsNullOrEmpty(match.LoserId))
                    {
                        _current.RunnerUpId = match.LoserId;
                        if (_current.ParticipantInfos.TryGetValue(match.LoserId, out var info))
                        {
                            _current.RunnerUpInfo = info;
                        }
                        break;
                    }
                }
            }
            else if (participantCount >= 3 && participantCount <= 4)
            {
                foreach (var match in _current.CurrentMatches)
                {
                    if (match != null && !string.IsNullOrEmpty(match.LoserId))
                    {
                        if (string.IsNullOrEmpty(_current.ThirdPlaceId))
                        {
                            _current.ThirdPlaceId = match.LoserId;
                            if (_current.ParticipantInfos.TryGetValue(match.LoserId, out var info))
                            {
                                _current.ThirdPlaceInfo = info;
                            }
                            break;
                        }
                    }
                }
            }
            _current.ParticipantIds.Clear();
            _current.ParticipantIds.AddRange(winners);
            if (_current.ParticipantIds.Count <= 1)
            {
                FinishTournament();
                return;
            }
            StartNextRound();
        }
        private static void StartNextRound()
        {
            if (_current == null) return;
            _current.CurrentRound++;
            _current.CurrentMatches.Clear();
            _current.CurrentMatchIndex = 0;
            _current.ByeParticipantId = null;
            var participants = new List<string>(_current.ParticipantIds);
            ShuffleList(participants);
            if (participants.Count % 2 == 1)
            {
                int byeIndex = Random.Range(0, participants.Count);
                _current.ByeParticipantId = participants[byeIndex];
                participants.RemoveAt(byeIndex);
                var byeActor = GetActorById(_current.ByeParticipantId);
                if (byeActor != null)
                {
                    xn.world.BroadcastSystem.PostActor(byeActor, T("broadcast_tournament_bye", "Tournament #{0} Round {1}: {2} gets a bye", _current.Edition, _current.CurrentRound, byeActor.getName()));
                }
            }
            for (int i = 0; i < participants.Count; i += 2)
            {
                var match = new MatchData
                {
                    Fighter1Id = participants[i],
                    Fighter2Id = participants[i + 1],
                    IsFinished = false,
                    FightersOnArena = false
                };
                _current.CurrentMatches.Add(match);
            }
            _current.State = TournamentState.Fighting;
            xn.world.BroadcastSystem.Custom(T("broadcast_tournament_round_start", "Tournament #{0} Round {1} begins! {2} duels are set", _current.Edition, _current.CurrentRound, _current.CurrentMatches.Count));
            if (_current.CurrentMatches.Count > 0)
            {
                var firstMatch = _current.CurrentMatches[0];
                var f1 = GetActorById(firstMatch.Fighter1Id);
                var f2 = GetActorById(firstMatch.Fighter2Id);
                if (f1 != null && f2 != null)
                {
                    xn.world.BroadcastSystem.Custom(T("broadcast_tournament_first_match", "Opening match: {0} VS {1}", f1.getName(), f2.getName()));
                }
            }
        }
        private static void FinishTournament()
        {
            if (_current == null) return;
            _current.State = TournamentState.Finished;
            _current.EndYear = Date.getYear(World.world.getCurWorldTime()); 
            foreach (var kvp in _current.OriginalPositions)
            {
                var actor = GetActorById(kvp.Key);
                if (actor != null && actor.isAlive())
                {
                    actor.finishStatusEffect("tantrum");
                    actor.finishAngryStatus(); 
                }
            }
            _actorCache.Clear();
            ParticipantDisplayInfo championInfo = null;
            if (_current.ParticipantIds.Count > 0)
            {
                _current.ChampionId = _current.ParticipantIds[0];
                var champion = GetActorById(_current.ChampionId);
                if (champion != null)
                {
                    championInfo = GetParticipantDisplayInfo(champion);
                    string championName = championInfo.GetFullDisplayName();
                    xn.world.BroadcastSystem.PostActor(champion, T("broadcast_tournament_champion", "Tournament #{0} Champion: {1}!", _current.Edition, championName));
                }
            }
            else
            {
                xn.world.BroadcastSystem.Custom(T("broadcast_tournament_no_winner", "Tournament #{0} ended with no victor", _current.Edition));
            }
            SaveTournamentHistory(championInfo);
            foreach (var kvp in _current.OriginalPositions)
            {
                TeleportBackToOriginal(kvp.Key);
            }
            TournamentArena.CleanupArena(_current.ArenaCenterTile);
            _current = null;
        }
        private static void SaveTournamentHistory(ParticipantDisplayInfo championInfo)
        {
            if (_current == null) return;
            var historyData = new TournamentHistoryData
            {
                Edition = _current.Edition,
                Year = _current.StartYear,
                EndYear = _current.EndYear,
                ChampionId = _current.ChampionId,
                ChampionInfo = championInfo,
                ChampionName = championInfo?.GetFullDisplayName() ?? T("value_none", "None"), 
                TotalRounds = _current.CurrentRound,
                Timestamp = World.world.getCurWorldTime()
            };
            if (!string.IsNullOrEmpty(_current.RunnerUpId))
            {
                historyData.RunnerUpId = _current.RunnerUpId;
                historyData.RunnerUpInfo = _current.RunnerUpInfo;
                historyData.RunnerUpName = _current.RunnerUpInfo?.GetFullDisplayName(); 
            }
            if (!string.IsNullOrEmpty(_current.ThirdPlaceId))
            {
                historyData.ThirdPlaceId = _current.ThirdPlaceId;
                historyData.ThirdPlaceInfo = _current.ThirdPlaceInfo;
                historyData.ThirdPlaceName = _current.ThirdPlaceInfo?.GetFullDisplayName(); 
            }
            foreach (var kvp in _current.ParticipantInfos)
            {
                historyData.ParticipantInfos.Add(kvp.Value);
                historyData.ParticipantNames.Add(kvp.Value.GetFullDisplayName()); 
            }
            TournamentHistoryGenerator.GenerateTournamentSummary(historyData, (summary) =>
            {
                historyData.Summary = summary;
                TournamentHistoryStorage.AddHistory(historyData);
            });
        }
        private static void TeleportFightersToArena(MatchData match)
        {
            if (match == null || _current == null) return;
            var arenaTiles = TournamentArena.GetArenaTiles(_current.ArenaCenterTile);
            if (arenaTiles == null || arenaTiles.Count < 2) return;
            var f1 = GetActorById(match.Fighter1Id);
            var f2 = GetActorById(match.Fighter2Id);
            if (f1 != null && f1.isAlive())
            {
                f1.cancelAllBeh();
                xn.access.ActorAccess.SpawnOn(f1, arenaTiles[0]);
            }
            if (f2 != null && f2.isAlive())
            {
                f2.cancelAllBeh();
                xn.access.ActorAccess.SpawnOn(f2, arenaTiles[arenaTiles.Count / 2]); 
            }
        }
        private static void TeleportBackToOriginal(string actorId)
        {
            if (_current == null || string.IsNullOrEmpty(actorId)) return;
            var actor = GetActorById(actorId);
            if (actor == null || !actor.isAlive()) return;
            if (_current.OriginalPositions.TryGetValue(actorId, out Vector2 pos))
            {
                var tile = World.world.GetTile((int)pos.x, (int)pos.y);
                if (tile != null)
                {
                    actor.cancelAllBeh();
                    actor.clearAttackTarget();
                    xn.access.ActorAccess.SpawnOn(actor, tile);
                }
            }
        }
        private static List<Actor> GetTopPowerActors(int count)
        {
            var result = new List<Actor>(count + 1);
            var scores = new List<long>(count + 1);
            foreach (var a in World.world.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (!a.asset.can_be_favorited) continue;
                if (a.asset.id == "xn_dashou") continue;
                if (IsTianyunzi(a)) continue;
                if (IsInSpecialTask(a)) continue;
                long s = xn.ui.XNPowerRanking.CalcPowerScoreLongInternal(a);
                int pos = -1;
                for (int t = 0; t < result.Count; t++)
                {
                    if (s > scores[t] || (s == scores[t] && a.getID() < result[t].getID()))
                    {
                        pos = t;
                        break;
                    }
                }
                if (pos == -1)
                {
                    if (result.Count < count)
                    {
                        result.Add(a);
                        scores.Add(s);
                    }
                }
                else
                {
                    result.Insert(pos, a);
                    scores.Insert(pos, s);
                    if (result.Count > count)
                    {
                        result.RemoveAt(result.Count - 1);
                        scores.RemoveAt(scores.Count - 1);
                    }
                }
            }
            return result;
        }
        private static bool IsTianyunzi(Actor a)
        {
            if (a == null) return false;
            int flag; xn.access.ActorAccess.GetData(a).get("xn_is_tianyunzi", out flag, 0);
            return flag == 1;
        }
        private static bool IsInSpecialTask(Actor a)
        {
            if (a == null) return false;
            int trialActive; xn.access.ActorAccess.GetData(a).get("xn.trial.active", out trialActive, 0);
            if (trialActive == 1) return true;
            int breakStop; xn.access.ActorAccess.GetData(a).get("xn.cultivation.stop", out breakStop, 0);
            if (breakStop == 1) return true;
            int condenseReady; xn.access.ActorAccess.GetData(a).get("xn.root.condense_ready", out condenseReady, 0);
            if (condenseReady == 1) return true;
            var ai = xn.access.ActorAccess.GetAI(a);
            if (ai == null) return false;
            var task = ai.task;
            if (task == null) return false;
            string taskId = task.id;
            if (string.IsNullOrEmpty(taskId)) return false;
            switch (taskId)
            {
                case "task_xn_breakthrough_stay":      
                case "task_xn_condense_root_stay":     
                case "task_xn_intent_comprehend_stay": 
                case "task_xn_demonic_hunt":           
                case "task_xn_tianyunzi_hunt":         
                    return true;
                default:
                    return false;
            }
        }
        internal static Actor GetActorById(string idStr)
        {
            if (string.IsNullOrEmpty(idStr)) return null;
            if (_actorCache.TryGetValue(idStr, out Actor cached))
            {
                if (cached != null && cached.isAlive()) return cached;
                _actorCache.Remove(idStr); 
            }
            if (!long.TryParse(idStr, out long id)) return null;
            foreach (var a in World.world.units)
            {
                if (a != null && a.getID() == id)
                {
                    _actorCache[idStr] = a; 
                    return a;
                }
            }
            return null;
        }
        public static bool IsParticipant(Actor a)
        {
            if (_current == null || a == null) return false;
            string id = a.getID().ToString();
            return _current.ParticipantIds.Contains(id);
        }
        internal static WorldTile GetArenaCenterTile()
        {
            return _current?.ArenaCenterTile;
        }
        internal static TournamentData GetCurrentData()
        {
            return _current;
        }
        internal static MatchData GetCurrentMatch(Actor a)
        {
            if (_current == null || a == null) return null;
            if (_current.CurrentMatches == null || _current.CurrentMatchIndex >= _current.CurrentMatches.Count) return null;
            string actorId = a.getID().ToString();
            var match = _current.CurrentMatches[_current.CurrentMatchIndex];
            if (match != null && (match.Fighter1Id == actorId || match.Fighter2Id == actorId))
            {
                return match;
            }
            return null;
        }
        internal static void RecordDeathTrigger(Actor a)
        {
            if (a == null) return;
            string id = a.getID().ToString();
            if (!_deathTriggers.ContainsKey(id))
            {
                _deathTriggers[id] = Time.time;
            }
        }
        internal static string CheckDeathTriggerLoser(MatchData match)
        {
            if (match == null) return null;
            bool f1Triggered = _deathTriggers.ContainsKey(match.Fighter1Id);
            bool f2Triggered = _deathTriggers.ContainsKey(match.Fighter2Id);
            if (f1Triggered && f2Triggered)
            {
                return _deathTriggers[match.Fighter1Id] <= _deathTriggers[match.Fighter2Id]
                    ? match.Fighter1Id : match.Fighter2Id;
            }
            else if (f1Triggered)
            {
                return match.Fighter1Id;
            }
            else if (f2Triggered)
            {
                return match.Fighter2Id;
            }
            return null;
        }
        internal static void ClearDeathTriggers(MatchData match)
        {
            if (match == null) return;
            _deathTriggers.Remove(match.Fighter1Id);
            _deathTriggers.Remove(match.Fighter2Id);
        }
        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
        private static ParticipantDisplayInfo GetParticipantDisplayInfo(Actor actor)
        {
            if (actor == null) return new ParticipantDisplayInfo(T("value_unknown", "Unknown"), "", "");
            string baseName = TitleSystem.GetBaseName(actor);
            string title = TitleSystem.GetTitle(actor);
            string suffix = TitleSystem.GetSuffix(actor);
            if (string.IsNullOrEmpty(baseName))
                baseName = actor.getName() ?? T("value_unknown", "Unknown");
            return new ParticipantDisplayInfo(baseName, title, suffix);
        }
    }
}
