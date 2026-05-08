using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using UnityEngine;
namespace xn.ui.charts
{
    public static class RankingLegendGenerator
    {
        private static bool _isGenerating = false;
        private const string WORLD_KEY_LEGEND_GENERATED = "xn.ranking.legend_generated";
        private const string LegendMarker = "---\u6218\u529b\u6392\u884c\u699c\u4f20\u5947---";
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        private static string GetAPIKey()
        {
            if (IsUsingCustomConfig())
                return xn.config.ModConfigHooks.CustomAIApiKey ?? "";
            return "";
        }
        private static bool IsUsingCustomConfig()
        {
            return !string.IsNullOrEmpty(xn.config.ModConfigHooks.CustomAIApiKey)
                || !string.IsNullOrEmpty(xn.config.ModConfigHooks.CustomAIUrl);
        }
        public static bool HasGenerated()
        {
            var customData = xn.access.MapBoxAccess.GetCustomData(World.world);
            if (customData == null) return false;
            customData.get(WORLD_KEY_LEGEND_GENERATED, out int generated, 0);
            return generated > 0;
        }
        private static void MarkAsGenerated()
        {
            var customData = xn.access.MapBoxAccess.EnsureCustomData(World.world);
            if (customData == null) return;
            customData.set(WORLD_KEY_LEGEND_GENERATED, 1);
        }
        public static bool CanGenerate()
        {
            if (_isGenerating) return false;
            if (IsUsingCustomConfig()) return true;
            return !HasGenerated();
        }
        public static bool CanRegenerate()
        {
            return IsUsingCustomConfig() && !_isGenerating;
        }
        private class ChatRequest
        {
            public ChatMessage[] messages;
            public string model = "deepseek-chat";
            public float temperature = 0.9f;
            public int max_tokens = 8192;
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
        public static async void GenerateLegend(Actor[] top3, long[] scores, Action<string> callback, string previousContent = null)
        {
            if (!CanGenerate() && !CanRegenerate())
            {
                string msg = _isGenerating
                    ? T("ranking_legend_generating_wait", "Generating, please wait...")
                    : T("ranking_legend_default_key_limit", "The default key can generate only once. Configure a custom API key.");
                callback?.Invoke(msg);
                return;
            }
            _isGenerating = true;
            try
            {
                int currentYear = Date.getCurrentYear();
                int previousYear = RankingLegendStorage.GetSavedWorldYear();
                string actorData = CollectTop3Data(top3, scores, currentYear);
                string apiKey = GetAPIKey();
                string legend = await GenerateLegendFromAPI(actorData, apiKey, previousContent, currentYear, previousYear);
                if (!IsUsingCustomConfig())
                    MarkAsGenerated();
                RankingLegendStorage.Save(legend, currentYear);
                callback?.Invoke(legend);
            }
            catch (Exception e)
            {
                callback?.Invoke(T("ai_generation_failed_with_error", "Generation failed: {0}", e.Message));
            }
            finally
            {
                _isGenerating = false;
            }
        }
        private static string CollectTop3Data(Actor[] top3, long[] scores, int currentYear)
        {
            var sb = new StringBuilder();
            sb.AppendLine(T("ranking_legend_data_header", "[Top 3 Power Ranking] (Current world year: Year {0})\n", currentYear));
            string[] titles =
            {
                T("ranking_legend_rank_1", "First Place (Unrivaled Under Heaven)"),
                T("ranking_legend_rank_2", "Second Place (Peerless Powerhouse)"),
                T("ranking_legend_rank_3", "Third Place (World-Shaking Hero)")
            };
            for (int i = 0; i < top3.Length && i < 3; i++)
            {
                var actor = top3[i];
                if (actor == null || !actor.isAlive()) continue;
                string baseName = xn.world.TitleSystem.GetBaseName(actor);
                string title = xn.world.TitleSystem.GetTitle(actor);
                string suffix = xn.world.TitleSystem.GetSuffix(actor);
                sb.AppendLine($"=== {titles[i]} ===");
                sb.AppendLine(T("ranking_legend_data_name", "Name: {0}", baseName));
                if (!string.IsNullOrEmpty(title))
                    sb.AppendLine(T("ranking_legend_data_title", "Title: {0}", title));
                if (!string.IsNullOrEmpty(suffix))
                    sb.AppendLine(T("ranking_legend_data_realm_suffix", "Realm Suffix: {0}", suffix));
                sb.AppendLine(T("ranking_legend_data_power_score", "Power Score: {0}", FormatNumber(scores[i])));
                sb.AppendLine(T("ranking_legend_data_age", "Age: {0}", actor.getAge()));
                sb.AppendLine(T("ranking_legend_data_gender", "Gender: {0}", xn.access.ActorAccess.GetData(actor).sex == ActorSex.Male ? T("ranking_legend_gender_male", "Male") : T("ranking_legend_gender_female", "Female")));
                sb.AppendLine(T("ranking_legend_data_kills", "Kills: {0}", xn.access.ActorAccess.GetData(actor).kills));
                sb.AppendLine(T("ranking_legend_data_level", "Level: {0}", actor.level));
                if (actor.asset != null)
                {
                    string raceName = LocalizedTextManager.getText(actor.asset.id);
                    if (string.IsNullOrEmpty(raceName) || raceName == actor.asset.id)
                        raceName = actor.asset.id;
                    sb.AppendLine(T("ranking_legend_data_race", "Race: {0}", raceName));
                }
                string realm = GetRealm(actor);
                if (!string.IsNullOrEmpty(realm))
                    sb.AppendLine(T("ranking_legend_data_realm", "Realm: {0}", realm));
                var traits = actor.getTraits();
                if (traits != null && traits.Count > 0)
                {
                    var traitNames = new System.Collections.Generic.List<string>();
                    foreach (var t in traits)
                    {
                        if (t != null && !t.id.StartsWith("realm_"))
                        {
                            string name = t.getTranslatedName();
                            if (!string.IsNullOrEmpty(name))
                                traitNames.Add(name);
                        }
                    }
                    if (traitNames.Count > 0)
                        sb.AppendLine(T("ranking_legend_data_traits", "Traits: {0}", string.Join(", ", traitNames)));
                }
                if (xn.bloodline.BloodlineSystem.HasBloodline(actor))
                {
                    string bloodlineType = xn.bloodline.BloodlineSystem.GetBloodlineType(actor);
                    string typeName = xn.bloodline.BloodlineTypes.GetLocaleName(bloodlineType);
                    float concentration = xn.bloodline.BloodlineSystem.GetConcentration(actor);
                    sb.AppendLine(T("ranking_legend_data_bloodline", "Bloodline: {0} (Concentration: {1:F1}%)", typeName, concentration));
                }
                if (actor.kingdom != null && !actor.kingdom.isRekt())
                    sb.AppendLine(T("ranking_legend_data_kingdom", "Kingdom: {0}", actor.kingdom.data.name));
                if (actor.city != null && !actor.city.isRekt())
                    sb.AppendLine(T("ranking_legend_data_city", "City: {0}", actor.city.data.name));
                sb.AppendLine();
            }
            return sb.ToString();
        }
        private static string GetRealm(Actor actor)
        {
            var traits = actor.getTraits();
            if (traits == null) return null;
            foreach (var t in traits)
            {
                if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupAncientRealm)
                    return t.getTranslatedName();
            }
            foreach (var t in traits)
            {
                if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupBeastStage)
                    return t.getTranslatedName();
            }
            foreach (var t in traits)
            {
                if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupRealm)
                    return t.getTranslatedName();
            }
            return null;
        }
        private static string FormatNumber(long num)
        {
            if (num >= 1_000_000_000_000) return T("number_unit_trillion_format", "{0:F1}T", num / 1_000_000_000_000f);
            if (num >= 100_000_000) return T("number_unit_hundred_million_format", "{0:F1}B", num / 100_000_000f);
            if (num >= 10_000) return T("number_unit_ten_thousand_format", "{0:F1}K", num / 10_000f);
            return num.ToString();
        }
        private static async Task<string> GenerateLegendFromAPI(string actorData, string apiKey, string previousContent = null, int currentYear = 0, int previousYear = 0)
        {
            var (endpoint, model) = xn.voice.AITextGenerator.GetProviderConfig();
            string systemPrompt = T("ranking_legend_system_prompt", "You are a cultivation (Renegade Immortal) novelist. Based on the top three Power Ranking data, evaluate these three mighty figures.\nRequirements:\n1. Keep it between 100 and 800 words\n2. Narrate in third person and evaluate each of the three separately\n3. Use grand, sweeping language that fits a cultivation novel\n4. Comment based on their realm, power score, traits, bloodline, and other data\n5. Highlight each person's defining qualities and legendary aspects");
            string userPrompt;
            if (!string.IsNullOrEmpty(previousContent))
            {
                string previousStory = previousContent;
                int idx = previousContent.IndexOf(LegendMarker);
                if (idx >= 0)
                    previousStory = previousContent.Substring(idx + LegendMarker.Length).Trim();
                int yearsPassed = (previousYear > 0 && currentYear > previousYear) ? (currentYear - previousYear) : 0;
                string yearsInfo = yearsPassed > 0 ? T("ranking_legend_years_since_update", "{0} years have passed since the previous ranking update.", yearsPassed) : "";
                userPrompt = T("ranking_legend_regenerate_user_prompt", "Based on the following top three Power Ranking data, rewrite a cultivation legend.\n\n[Previous generated content] (Year {0})\n{1}\n\n[Current ranking data]\n{2}\n\n{3}Use the previous content and the ranking changes as reference. Create something fresh and varied; do not simply copy it.", previousYear, previousStory, actorData, yearsInfo);
            }
            else
            {
                userPrompt = T("ranking_legend_user_prompt", "Based on the following top three Power Ranking data, write a cultivation legend:\n\n{0}", actorData);
            }
            var request = new ChatRequest
            {
                messages = new[]
                {
                    new ChatMessage { role = "system", content = systemPrompt },
                    new ChatMessage { role = "user", content = userPrompt }
                },
                model = model,
                temperature = 0.9f,
                max_tokens = 8192
            };
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) })
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
                        return xn.voice.AITextGenerator.FilterThinkingProcess(chatResponse.choices[0].message.content);
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Debug.LogWarning($"[XN-RankingLegend] API call failed: {response.StatusCode}");
                    throw new Exception(T("ai_api_call_failed", "API call failed: {0}", response.StatusCode));
                }
            }
            throw new Exception(T("ai_api_empty_response", "API returned empty data"));
        }
    }
}
