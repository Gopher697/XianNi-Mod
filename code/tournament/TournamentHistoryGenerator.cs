using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
namespace xn.tournament
{
    public static class TournamentHistoryGenerator
    {
        private static string GetAPIKey()
        {
            if (IsUsingCustomConfig())
                return xn.config.ModConfigHooks.CustomAIApiKey ?? "";
            return "";
        }
        private static int _generationCount = 0;
        private const int MAX_GENERATIONS = 10;
        private static bool _isGenerating = false;
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        private static bool IsUsingCustomConfig()
        {
            return !string.IsNullOrEmpty(xn.config.ModConfigHooks.CustomAIApiKey)
                || !string.IsNullOrEmpty(xn.config.ModConfigHooks.CustomAIUrl);
        }
        private class ChatRequest
        {
            public ChatMessage[] messages;
            public string model = "deepseek-chat";
            public float temperature = 0.8f;
            [JsonProperty("max_tokens", NullValueHandling = NullValueHandling.Ignore)]
            public int? max_tokens;
            [JsonProperty("max_completion_tokens", NullValueHandling = NullValueHandling.Ignore)]
            public int? max_completion_tokens;
        }
        private class ChatMessage
        {
            public string role;
            public string content;
        }
        private class ChatResponse
        {
            public ChatChoice[] choices;
        }
        private class ChatChoice
        {
            public ChatMessage message;
        }
        public static void ResetGenerationCount()
        {
            _generationCount = 0;
            _isGenerating = false;
        }
        public static int GetRemainingGenerations()
        {
            if (IsUsingCustomConfig())
                return int.MaxValue;
            return Math.Max(0, MAX_GENERATIONS - _generationCount);
        }
        public static bool CanGenerate()
        {
            if (_isGenerating)
                return false;
            if (IsUsingCustomConfig())
                return true;
            return _generationCount < MAX_GENERATIONS;
        }
        public static async void GenerateTournamentSummary(TournamentHistoryData data, Action<string> callback)
        {
            if (!CanGenerate())
            {
                string message = _isGenerating
                    ? T("tournament_summary_generating", "Generating, please wait...")
                    : T("tournament_summary_limit_reached", "This world has reached the generation limit ({0} times)", MAX_GENERATIONS);
                callback?.Invoke(message);
                return;
            }
            _isGenerating = true;
            try
            {
                string tournamentData = CollectTournamentData(data);
                string apiKey = GetAPIKey();
                string summary = await GenerateSummaryFromAPI(tournamentData, apiKey);
                if (!IsUsingCustomConfig())
                    _generationCount++;
                callback?.Invoke(summary);
            }
            catch (Exception e)
            {
                callback?.Invoke(T("tournament_summary_fallback_champion", "Tournament #{0} Champion: {1}", data.Edition, data.ChampionName));
            }
            finally
            {
                _isGenerating = false;
            }
        }
        private static string CollectTournamentData(TournamentHistoryData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine(T("tournament_summary_data_title", "Tournament #{0}", data.Edition));
            sb.AppendLine(T("tournament_summary_data_participants", "Participants: {0}", data.ParticipantInfos.Count));
            sb.AppendLine(T("tournament_summary_data_rounds", "Total Rounds: {0}", data.TotalRounds));
            sb.AppendLine();
            sb.AppendLine(T("tournament_summary_section_participants", "[Participants]"));
            foreach (var info in data.ParticipantInfos)
            {
                sb.Append(T("tournament_summary_participant_name_inline", "- Name: {0}", info.BaseName));
                if (!string.IsNullOrEmpty(info.Title))
                    sb.Append(T("tournament_summary_participant_title_inline", ", Title: {0}", info.Title));
                if (!string.IsNullOrEmpty(info.Suffix))
                    sb.Append(T("tournament_summary_participant_realm_inline", ", Realm: {0}", info.Suffix));
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine(T("tournament_summary_section_champion", "[Champion]"));
            if (data.ChampionInfo != null)
            {
                sb.AppendLine(T("tournament_summary_data_name", "Name: {0}", data.ChampionInfo.BaseName));
                if (!string.IsNullOrEmpty(data.ChampionInfo.Title))
                    sb.AppendLine(T("tournament_summary_data_title_label", "Title: {0}", data.ChampionInfo.Title));
                if (!string.IsNullOrEmpty(data.ChampionInfo.Suffix))
                    sb.AppendLine(T("tournament_summary_data_realm", "Realm: {0}", data.ChampionInfo.Suffix));
            }
            else if (!string.IsNullOrEmpty(data.ChampionName))
            {
                sb.AppendLine(T("tournament_summary_data_name", "Name: {0}", data.ChampionName));
            }
            if (data.RunnerUpInfo != null)
            {
                sb.AppendLine();
                sb.AppendLine(T("tournament_summary_section_runner_up", "[Runner-up]"));
                sb.AppendLine(T("tournament_summary_data_name", "Name: {0}", data.RunnerUpInfo.BaseName));
                if (!string.IsNullOrEmpty(data.RunnerUpInfo.Title))
                    sb.AppendLine(T("tournament_summary_data_title_label", "Title: {0}", data.RunnerUpInfo.Title));
                if (!string.IsNullOrEmpty(data.RunnerUpInfo.Suffix))
                    sb.AppendLine(T("tournament_summary_data_realm", "Realm: {0}", data.RunnerUpInfo.Suffix));
            }
            else if (!string.IsNullOrEmpty(data.RunnerUpName))
            {
                sb.AppendLine();
                sb.AppendLine(T("tournament_summary_section_runner_up", "[Runner-up]"));
                sb.AppendLine(T("tournament_summary_data_name", "Name: {0}", data.RunnerUpName));
            }
            if (data.ThirdPlaceInfo != null)
            {
                sb.AppendLine();
                sb.AppendLine(T("tournament_summary_section_third_place", "[Third Place]"));
                sb.AppendLine(T("tournament_summary_data_name", "Name: {0}", data.ThirdPlaceInfo.BaseName));
                if (!string.IsNullOrEmpty(data.ThirdPlaceInfo.Title))
                    sb.AppendLine(T("tournament_summary_data_title_label", "Title: {0}", data.ThirdPlaceInfo.Title));
                if (!string.IsNullOrEmpty(data.ThirdPlaceInfo.Suffix))
                    sb.AppendLine(T("tournament_summary_data_realm", "Realm: {0}", data.ThirdPlaceInfo.Suffix));
            }
            else if (!string.IsNullOrEmpty(data.ThirdPlaceName))
            {
                sb.AppendLine();
                sb.AppendLine(T("tournament_summary_section_third_place", "[Third Place]"));
                sb.AppendLine(T("tournament_summary_data_name", "Name: {0}", data.ThirdPlaceName));
            }
            return sb.ToString();
        }
        private static async Task<string> GenerateSummaryFromAPI(string tournamentData, string apiKey)
        {
            var (endpoint, model) = xn.voice.AITextGenerator.GetProviderConfig();
            string systemPrompt = T("tournament_summary_system_prompt", "You are a professional xianxia novelist. Based on the tournament data provided, write a short match summary.\nRequirements:\n1. Keep it between 50 and 200 words\n2. Describe how exciting the matches were and how the champion performed\n3. If runner-up and third-place information is available, mention their performances too\n4. Use vivid, flavorful language that fits a cultivation story\n5. Make the summary complete and emphasize the champion's strength");
            string userPrompt = T("tournament_summary_user_prompt", "Based on the following tournament data, write a match summary:\n\n{0}", tournamentData);
            int tokenLimit = IsUsingCustomConfig() ? 8192 : 800;
            bool useMaxCompletionTokens = xn.voice.AITextGenerator.UsesMaxCompletionTokens(model);
            var request = new ChatRequest
            {
                messages = new[]
                {
                    new ChatMessage { role = "system", content = systemPrompt },
                    new ChatMessage { role = "user", content = userPrompt }
                },
                model = model,
                temperature = 0.8f,
                max_tokens = useMaxCompletionTokens ? (int?)null : tokenLimit,
                max_completion_tokens = useMaxCompletionTokens ? tokenLimit : (int?)null
            };
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                }
                else
                {
                    // No API key configured — request will be sent without Authorization header
                }
                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await client.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(responseText);
                    if (chatResponse?.choices != null && chatResponse.choices.Length > 0)
                    {
                        string messageContent = chatResponse.choices[0].message.content ?? T("ai_generation_failed", "Generation failed");
                        return xn.voice.AITextGenerator.FilterThinkingProcess(messageContent);
                    }
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    UnityEngine.Debug.LogWarning($"[XN-Tournament] API call failed: {response.StatusCode} (Model: {model})");
                    UnityEngine.Debug.LogWarning($"[XN-Tournament] Error details: {errorBody}");
                }
                throw new Exception(T("ai_api_request_failed", "API request failed: {0}", response.StatusCode));
            }
        }
    }
}
