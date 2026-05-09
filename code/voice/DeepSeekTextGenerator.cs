// This file is kept with its original name for git history continuity.
// The public class is now AITextGenerator; references to DeepSeekTextGenerator
// are provided as a backward-compat alias at the bottom of this file.
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
namespace xn.voice
{
    /// <summary>
    /// Generic AI text generator. Works with any OpenAI-compatible chat completions
    /// endpoint (OpenAI, Azure OpenAI, Ollama, LM Studio, Anthropic-compatible
    /// proxies, SiliconFlow, DeepSeek, etc.).
    ///
    /// Configure in-game under Mod Settings → AI Features:
    ///   API Key  – your provider's API key (required for hosted services)
    ///   API URL  – base URL ending in /v1  OR full chat completions URL
    ///              leave empty to use the default: https://api.openai.com/v1/chat/completions
    ///   AI Model – model identifier (e.g. gpt-4o-mini, llama3.1, deepseek-chat)
    ///              leave empty to use the default: gpt-4o-mini
    /// </summary>
    public static class AITextGenerator
    {
        // ── defaults ──────────────────────────────────────────────────────────
        private const string DefaultEndpoint = "https://api.openai.com/v1/chat/completions";
        private const string DefaultModel    = "gpt-4o-mini";

        // Maximum word count for short broadcast lines
        private const int MaxBroadcastWords = 20;

        // ── provider config ───────────────────────────────────────────────────
        public static (string endpoint, string model) GetProviderConfig()
        {
            string customUrl = xn.config.ModConfigHooks.CustomAIUrl?.Trim() ?? "";
            string model     = xn.config.ModConfigHooks.CustomAIModel?.Trim() ?? "";
            if (string.IsNullOrEmpty(model)) model = DefaultModel;

            string endpoint;
            if (string.IsNullOrEmpty(customUrl))
            {
                endpoint = DefaultEndpoint;
            }
            else
            {
                customUrl = customUrl.TrimEnd('/');
                // Accept base URL (ends with /v1) or full completions URL
                if (customUrl.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                    endpoint = customUrl;
                else if (customUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                    endpoint = customUrl + "/chat/completions";
                else
                    endpoint = customUrl + "/v1/chat/completions";
            }
            return (endpoint, model);
        }

        private static string GetAPIKey()
        {
            return xn.config.ModConfigHooks.CustomAIApiKey?.Trim() ?? "";
        }

        // ── JSON DTOs ─────────────────────────────────────────────────────────
        private class ChatRequest
        {
            public ChatMessage[] messages;
            public string model = DefaultModel;
            public float temperature = 0.7f;
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

        // ── public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Rewrite a raw game-event string into a concise English broadcast line.
        /// Falls back to the raw text if AI is disabled or the call fails.
        /// </summary>
        public static async Task<string> GenerateNaturalText(string rawText, string context = "general")
        {
            if (!xn.config.ModConfigHooks.EnableAITextGen)
                return rawText;

            try
            {
                var (endpoint, model) = GetProviderConfig();
                string apiKey = GetAPIKey();
                string systemPrompt = GetSystemPrompt(context);
                string userPrompt = $"Game event: {rawText}\n\nRequirement: Rewrite as a single concise English voice broadcast line, strictly under {MaxBroadcastWords} words.";
                bool useMaxCompletionTokens = UsesMaxCompletionTokens(model);

                var request = new ChatRequest
                {
                    messages = new[]
                    {
                        new ChatMessage { role = "system", content = systemPrompt },
                        new ChatMessage { role = "user",   content = userPrompt }
                    },
                    model       = model,
                    temperature = 0.9f,
                    max_tokens  = useMaxCompletionTokens ? (int?)null : 80,
                    max_completion_tokens = useMaxCompletionTokens ? 80 : (int?)null
                };

                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) })
                {
                    if (!string.IsNullOrEmpty(apiKey))
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var httpContent = new StringContent(
                        JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(endpoint, httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseText = await response.Content.ReadAsStringAsync();
                        var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(responseText);
                        if (chatResponse?.choices != null && chatResponse.choices.Length > 0)
                        {
                            string generated = FilterThinkingProcess(chatResponse.choices[0].message.content);
                            generated = generated.Trim('"', '\'', '.', ',').Trim();
                            generated = TruncateToWordLimit(generated, MaxBroadcastWords);
                            return generated;
                        }
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Debug.LogWarning($"[XN-Voice] AI API call failed: {response.StatusCode} (Model: {model})");
                        Debug.LogWarning($"[XN-Voice] Error details: {errorBody}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XN-Voice] AI text generation failed: {e.Message}");
                if (e.InnerException != null)
                    Debug.LogWarning($"[XN-Voice] Inner exception: {e.InnerException.Message}");
            }
            return rawText;
        }

        /// <summary>
        /// Generate a short cultivation story for ambient voice broadcasts.
        /// </summary>
        public static async Task<string> GenerateCultivationStory()
        {
            if (!xn.config.ModConfigHooks.EnableAITextGen)
                return "The cultivation world churns — Heaven's will is fickle, fate unpredictable.";

            try
            {
                var (endpoint, model) = GetProviderConfig();
                string apiKey = GetAPIKey();
                bool useMaxCompletionTokens = UsesMaxCompletionTokens(model);

                string systemPrompt =
                    "You are a narrator for a xianxia cultivation world simulation game. " +
                    "Write vivid, immersive short stories set in a world of immortal cultivators. " +
                    "Stories should feel authentic to the genre: breakthroughs, tribulations, rivalries, fate.";

                string userPrompt =
                    "Write a complete short cultivation story (100-200 words). " +
                    "Give the protagonist a name. Include a cultivation element such as a realm " +
                    "breakthrough, a divine treasure, a heavenly tribulation, or a fateful encounter. " +
                    "Each story should have a unique protagonist and plot. Write in English.";

                var request = new ChatRequest
                {
                    messages = new[]
                    {
                        new ChatMessage { role = "system", content = systemPrompt },
                        new ChatMessage { role = "user",   content = userPrompt }
                    },
                    model       = model,
                    temperature = 1.0f,
                    max_tokens  = useMaxCompletionTokens ? (int?)null : 400,
                    max_completion_tokens = useMaxCompletionTokens ? 400 : (int?)null
                };

                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) })
                {
                    if (!string.IsNullOrEmpty(apiKey))
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var httpContent = new StringContent(
                        JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(endpoint, httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseText = await response.Content.ReadAsStringAsync();
                        var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(responseText);
                        if (chatResponse?.choices != null && chatResponse.choices.Length > 0)
                        {
                            string story = FilterThinkingProcess(chatResponse.choices[0].message.content);
                            return story.Trim();
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[XN-Voice] AI story generation failed: {response.StatusCode}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XN-Voice] AI story generation error: {e.Message}");
            }
            return "The cultivation world churns — Heaven's will is fickle, fate unpredictable. The immortal road stretches on without end.";
        }

        /// <summary>Pre-warm the AI cache with common broadcast phrases.</summary>
        public static async Task PreGenerateCommonTexts()
        {
            string[] commonTexts = new[]
            {
                "Main character selected",
                "Power ranking opened",
                "Breakthrough successful",
                "Battle begins",
                "City event"
            };
            foreach (var text in commonTexts)
            {
                await GenerateNaturalText(text);
                await Task.Delay(100);
            }
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private static string GetSystemPrompt(string context)
        {
            return "You are the voice broadcaster for a xianxia cultivation world simulation game. " +
                   "Convert game events into concise, vivid English broadcast lines. " +
                   $"Keep broadcasts under {MaxBroadcastWords} words. Use cultivation-genre tone.";
        }

        public static bool UsesMaxCompletionTokens(string model)
        {
            if (string.IsNullOrEmpty(model)) return false;
            string normalized = model.Trim().ToLowerInvariant();
            return normalized.StartsWith("gpt-5", StringComparison.Ordinal)
                || normalized.StartsWith("o1", StringComparison.Ordinal)
                || normalized.StartsWith("o3", StringComparison.Ordinal)
                || normalized.StartsWith("o4", StringComparison.Ordinal);
        }

        /// <summary>Strip any &lt;think&gt;...&lt;/think&gt; reasoning block from a model response.</summary>
        public static string FilterThinkingProcess(string response)
        {
            if (string.IsNullOrEmpty(response)) return response;
            int thinkEnd = response.IndexOf("</think>", StringComparison.Ordinal);
            return thinkEnd >= 0 ? response.Substring(thinkEnd + 8).Trim() : response;
        }

        private static string TruncateToWordLimit(string text, int maxWords)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string[] words = text.Split(new char[]{' ', '\t', '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= maxWords) return text;
            return string.Join(" ", words, 0, maxWords);
        }
    }

    // ── backward-compat alias ─────────────────────────────────────────────────
    /// <summary>
    /// Deprecated alias. Use AITextGenerator directly.
    /// Kept so that any external code still referencing DeepSeekTextGenerator compiles.
    /// </summary>
    [Obsolete("Use AITextGenerator instead.")]
    public static class DeepSeekTextGenerator
    {
        public static Task<string> GenerateNaturalText(string rawText, string context = "general")
            => AITextGenerator.GenerateNaturalText(rawText, context);
        public static Task<string> GenerateCultivationStory()
            => AITextGenerator.GenerateCultivationStory();
        public static Task PreGenerateCommonTexts()
            => AITextGenerator.PreGenerateCommonTexts();
        public static string FilterThinkingProcess(string response)
            => AITextGenerator.FilterThinkingProcess(response);
        public static (string endpoint, string model) GetProviderConfig()
            => AITextGenerator.GetProviderConfig();
    }
}
