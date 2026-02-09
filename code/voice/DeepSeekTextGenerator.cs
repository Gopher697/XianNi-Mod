using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
namespace xn.voice
{
    public static class DeepSeekTextGenerator
    {
        private const string DefaultModel = "deepseek-ai/DeepSeek-V3";
        private static bool IsUsingCustomConfig()
        {
            return !string.IsNullOrEmpty(xn.config.ModConfigHooks.CustomAIApiKey)
                || !string.IsNullOrEmpty(xn.config.ModConfigHooks.CustomAIUrl);
        }
        public static (string endpoint, string model) GetProviderConfig()
        {
            if (IsUsingCustomConfig())
            {
                string customUrl = xn.config.ModConfigHooks.CustomAIUrl;
                string endpoint;
                if (string.IsNullOrEmpty(customUrl))
                {
                    endpoint = "https://api.siliconflow.cn/v1/chat/completions";
                }
                else
                {
                    customUrl = customUrl.TrimEnd('/');
                    endpoint = customUrl.EndsWith("/chat/completions")
                        ? customUrl
                        : customUrl + "/chat/completions";
                }
                string model = string.IsNullOrEmpty(xn.config.ModConfigHooks.CustomAIModel)
                    ? DefaultModel
                    : xn.config.ModConfigHooks.CustomAIModel;
                return (endpoint, model);
            }
            return (xn.config.ModConfigHooks.DefaultProxyUrl, DefaultModel);
        }
        private static string GetAPIKey()
        {
            if (IsUsingCustomConfig())
            {
                return xn.config.ModConfigHooks.CustomAIApiKey ?? "";
            }
            return "";
        }
        private class ChatRequest
        {
            public ChatMessage[] messages;
            public string model = "deepseek-chat";
            public float temperature = 0.7f;
            public int max_tokens = 100;
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
        public static async Task<string> GenerateNaturalText(string rawText, string context = "general")
        {
            if (!xn.config.ModConfigHooks.EnableDeepSeekTextGen)
            {
                return rawText;
            }
            try
            {
                var (endpoint, model) = GetProviderConfig();
                string apiKey = GetAPIKey();
                string systemPrompt = GetSystemPrompt(context);
                string userPrompt = $"游戏事件：{rawText}\n\n要求：生成一句简洁的语音播报（严格限制在20个汉字以内，不要超过）";
                var request = new ChatRequest
                {
                    messages = new[]
                    {
                        new ChatMessage { role = "system", content = systemPrompt },
                        new ChatMessage { role = "user", content = userPrompt }
                    },
                    model = model,
                    temperature = 0.9f,
                    max_tokens = IsUsingCustomConfig() ? 8192 : 50
                };
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
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
                            string generatedText = FilterThinkingProcess(chatResponse.choices[0].message.content);
                            generatedText = generatedText
                                .Trim('"', '\'', '"', '"', '。', '！', '？', '，')
                                .Trim();
                            int charCount = 0;
                            StringBuilder sb = new StringBuilder();
                            foreach (char c in generatedText)
                            {
                                if (char.IsLetterOrDigit(c) || c >= 0x4E00 && c <= 0x9FA5)
                                {
                                    charCount++;
                                    if (charCount > 20) break;
                                }
                                sb.Append(c);
                            }
                            generatedText = sb.ToString().Trim();
                            if (generatedText.Length > 20)
                            {
                                generatedText = generatedText.Substring(0, 20);
                            }
                            return generatedText;
                        }
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Debug.LogWarning($"[XN-Voice] AI API调用失败: {response.StatusCode} (Model: {model})");
                        Debug.LogWarning($"[XN-Voice] 错误详情: {errorBody}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XN-Voice] AI生成文本失败: {e.Message}");
                if (e.InnerException != null)
                {
                    Debug.LogWarning($"[XN-Voice] 内部异常: {e.InnerException.Message}");
                }
            }
            return rawText;
        }
        private static string GetSystemPrompt(string context)
        {
            return "你是仙逆修仙模组玩法播报员，将游戏事件转为简洁自然的播报，严格20字以内";
        }
        public static string FilterThinkingProcess(string response)
        {
            if (string.IsNullOrEmpty(response))
                return response;
            int thinkEndIndex = response.IndexOf("</think>");
            if (thinkEndIndex >= 0)
            {
                return response.Substring(thinkEndIndex + 8).Trim();
            }
            return response;
        }
        public static async Task<string> GenerateCultivationStory()
        {
            if (!xn.config.ModConfigHooks.EnableDeepSeekTextGen)
            {
                return "修仙界风云变幻，天道无常，世事难料。";
            }
            try
            {
                var (endpoint, model) = GetProviderConfig();
                string apiKey = GetAPIKey();
                string systemPrompt = @"你是一个专业的修仙小说作者。请创作一个完整的修仙小故事，要求：
1. 故事要完整，有开头、发展、高潮、结尾
2. 包含修仙元素：境界突破、法宝、神通、历练等
3. 主角要有名字，经历要生动有趣
4. 字数控制在150-250字之间
5. 语言流畅，适合语音播报
6. 每次创作不同的主角和故事情节";
                string userPrompt = "请创作一个修仙小故事";
                var request = new ChatRequest
                {
                    messages = new[]
                    {
                        new ChatMessage { role = "system", content = systemPrompt },
                        new ChatMessage { role = "user", content = userPrompt }
                    },
                    model = model,
                    temperature = 1.0f,
                    max_tokens = IsUsingCustomConfig() ? 8192 : 800
                };
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) })
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
                            string story = FilterThinkingProcess(chatResponse.choices[0].message.content);
                            return story;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[XN-Voice] AI生成故事失败: {response.StatusCode}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XN-Voice] AI生成故事异常: {e.Message}");
            }
            return "修仙界风云变幻，天道无常，世事难料。仙路漫漫，求索不止。";
        }
        public static async Task PreGenerateCommonTexts()
        {
            string[] commonTexts = new[]
            {
                "主角已选中",
                "战力排行榜已开启",
                "突破成功",
                "战斗开始",
                "城市事件"
            };
            foreach (var text in commonTexts)
            {
                await GenerateNaturalText(text);
                await Task.Delay(100);
            }
        }
    }
}