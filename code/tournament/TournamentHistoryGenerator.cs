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
            public int max_tokens = 800;
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
                string message = _isGenerating ? "正在生成中，请稍候..." : $"本局游戏已达到生成上限（{MAX_GENERATIONS}次）";
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
                callback?.Invoke($"第{data.Edition}届冠军：{data.ChampionName}");
            }
            finally
            {
                _isGenerating = false;
            }
        }
        private static string CollectTournamentData(TournamentHistoryData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"第{data.Edition}届比武大会");
            sb.AppendLine($"参赛人数：{data.ParticipantInfos.Count}人");
            sb.AppendLine($"总轮次：{data.TotalRounds}轮");
            sb.AppendLine();
            sb.AppendLine("【参赛者】");
            foreach (var info in data.ParticipantInfos)
            {
                sb.Append($"- 名字：{info.BaseName}");
                if (!string.IsNullOrEmpty(info.Title))
                    sb.Append($"，称号：{info.Title}");
                if (!string.IsNullOrEmpty(info.Suffix))
                    sb.Append($"，境界：{info.Suffix}");
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("【冠军】");
            if (data.ChampionInfo != null)
            {
                sb.AppendLine($"名字：{data.ChampionInfo.BaseName}");
                if (!string.IsNullOrEmpty(data.ChampionInfo.Title))
                    sb.AppendLine($"称号：{data.ChampionInfo.Title}");
                if (!string.IsNullOrEmpty(data.ChampionInfo.Suffix))
                    sb.AppendLine($"境界：{data.ChampionInfo.Suffix}");
            }
            else if (!string.IsNullOrEmpty(data.ChampionName))
            {
                sb.AppendLine($"名字：{data.ChampionName}");
            }
            if (data.RunnerUpInfo != null)
            {
                sb.AppendLine();
                sb.AppendLine("【亚军】");
                sb.AppendLine($"名字：{data.RunnerUpInfo.BaseName}");
                if (!string.IsNullOrEmpty(data.RunnerUpInfo.Title))
                    sb.AppendLine($"称号：{data.RunnerUpInfo.Title}");
                if (!string.IsNullOrEmpty(data.RunnerUpInfo.Suffix))
                    sb.AppendLine($"境界：{data.RunnerUpInfo.Suffix}");
            }
            else if (!string.IsNullOrEmpty(data.RunnerUpName))
            {
                sb.AppendLine();
                sb.AppendLine($"【亚军】");
                sb.AppendLine($"名字：{data.RunnerUpName}");
            }
            if (data.ThirdPlaceInfo != null)
            {
                sb.AppendLine();
                sb.AppendLine("【季军】");
                sb.AppendLine($"名字：{data.ThirdPlaceInfo.BaseName}");
                if (!string.IsNullOrEmpty(data.ThirdPlaceInfo.Title))
                    sb.AppendLine($"称号：{data.ThirdPlaceInfo.Title}");
                if (!string.IsNullOrEmpty(data.ThirdPlaceInfo.Suffix))
                    sb.AppendLine($"境界：{data.ThirdPlaceInfo.Suffix}");
            }
            else if (!string.IsNullOrEmpty(data.ThirdPlaceName))
            {
                sb.AppendLine();
                sb.AppendLine($"【季军】");
                sb.AppendLine($"名字：{data.ThirdPlaceName}");
            }
            return sb.ToString();
        }
        private static async Task<string> GenerateSummaryFromAPI(string tournamentData, string apiKey)
        {
            var (endpoint, model) = xn.voice.DeepSeekTextGenerator.GetProviderConfig();
            string systemPrompt = @"你是一位专业的修仙小说作家。根据提供的比武大会数据，创作一段简短的比赛总结。
要求：
1. 字数控制在50-200字之间
2. 描述比赛的精彩程度、冠军的表现等
3. 如果有亚军和季军信息，也要提及他们的表现
4. 语言要生动有趣，符合修仙风格
5. 总结要完整，突出冠军的实力";
            string userPrompt = $"请根据以下比武大会数据，创作一段比赛总结：\n\n{tournamentData}";
            var request = new ChatRequest
            {
                messages = new[]
                {
                    new ChatMessage { role = "system", content = systemPrompt },
                    new ChatMessage { role = "user", content = userPrompt }
                },
                model = model,
                temperature = 0.8f,
                max_tokens = IsUsingCustomConfig() ? 8192 : 800
            };
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                }
                else
                {
                    client.DefaultRequestHeaders.Add("X-Mod-Secret", xn.config.ModConfigHooks.ProxySecret);
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
                        string messageContent = chatResponse.choices[0].message.content ?? "生成失败";
                        return xn.voice.DeepSeekTextGenerator.FilterThinkingProcess(messageContent);
                    }
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    UnityEngine.Debug.LogWarning($"[XN-Tournament] API调用失败: {response.StatusCode} (Model: {model})");
                    UnityEngine.Debug.LogWarning($"[XN-Tournament] 错误详情: {errorBody}");
                }
                throw new Exception($"API请求失败: {response.StatusCode}");
            }
        }
    }
}