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
            if (World.world?.map_stats?.custom_data == null) return false;
            World.world.map_stats.custom_data.get(WORLD_KEY_LEGEND_GENERATED, out int generated, 0);
            return generated > 0;
        }
        private static void MarkAsGenerated()
        {
            if (World.world?.map_stats == null) return;
            if (World.world.map_stats.custom_data == null)
                World.world.map_stats.custom_data = new SaveCustomData();
            World.world.map_stats.custom_data.set(WORLD_KEY_LEGEND_GENERATED, 1);
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
                string msg = _isGenerating ? "正在生成中，请稍候..." : "默认密匙只能生成一次，请配置自定义API密匙";
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
                callback?.Invoke($"生成失败：{e.Message}");
            }
            finally
            {
                _isGenerating = false;
            }
        }
        private static string CollectTop3Data(Actor[] top3, long[] scores, int currentYear)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"【战力排行榜前三名】（当前世界年份：第{currentYear}年）\n");
            string[] titles = { "第一名（天下第一）", "第二名（绝世强者）", "第三名（盖世豪杰）" };
            for (int i = 0; i < top3.Length && i < 3; i++)
            {
                var actor = top3[i];
                if (actor == null || !actor.isAlive()) continue;
                string baseName = xn.world.TitleSystem.GetBaseName(actor);
                string title = xn.world.TitleSystem.GetTitle(actor);
                string suffix = xn.world.TitleSystem.GetSuffix(actor);
                sb.AppendLine($"=== {titles[i]} ===");
                sb.AppendLine($"名字：{baseName}");
                if (!string.IsNullOrEmpty(title))
                    sb.AppendLine($"称号：{title}");
                if (!string.IsNullOrEmpty(suffix))
                    sb.AppendLine($"境界后缀：{suffix}");
                sb.AppendLine($"战力分：{FormatNumber(scores[i])}");
                sb.AppendLine($"年龄：{actor.getAge()}岁");
                sb.AppendLine($"性别：{(actor.data.sex == ActorSex.Male ? "男" : "女")}");
                sb.AppendLine($"击杀：{actor.data.kills}");
                sb.AppendLine($"等级：{actor.level}");
                if (actor.asset != null)
                {
                    string raceName = LocalizedTextManager.getText(actor.asset.id);
                    if (string.IsNullOrEmpty(raceName) || raceName == actor.asset.id)
                        raceName = actor.asset.id;
                    sb.AppendLine($"种族：{raceName}");
                }
                string realm = GetRealm(actor);
                if (!string.IsNullOrEmpty(realm))
                    sb.AppendLine($"境界：{realm}");
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
                        sb.AppendLine($"特质：{string.Join("、", traitNames)}");
                }
                if (xn.bloodline.BloodlineSystem.HasBloodline(actor))
                {
                    string bloodlineType = xn.bloodline.BloodlineSystem.GetBloodlineType(actor);
                    string typeName = xn.bloodline.BloodlineTypes.GetLocaleName(bloodlineType);
                    float concentration = xn.bloodline.BloodlineSystem.GetConcentration(actor);
                    sb.AppendLine($"血脉：{typeName}（浓度：{concentration:F1}%）");
                }
                if (actor.kingdom != null && !actor.kingdom.isRekt())
                    sb.AppendLine($"国家：{actor.kingdom.data.name}");
                if (actor.city != null && !actor.city.isRekt())
                    sb.AppendLine($"城市：{actor.city.data.name}");
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
            if (num >= 1_000_000_000_000) return $"{num / 1_000_000_000_000f:F1}兆";
            if (num >= 100_000_000) return $"{num / 100_000_000f:F1}亿";
            if (num >= 10_000) return $"{num / 10_000f:F1}万";
            return num.ToString();
        }
        private static async Task<string> GenerateLegendFromAPI(string actorData, string apiKey, string previousContent = null, int currentYear = 0, int previousYear = 0)
        {
            var (endpoint, model) = xn.voice.DeepSeekTextGenerator.GetProviderConfig();
            string systemPrompt = @"你是一位修仙(仙逆)小说作家。根据仙逆战力排行榜前三名数据，对这三位强者进行评价。
要求：
1. 字数控制在100-800字
2. 以第三人称叙述，分别评价这三位强者
3. 语言大气磅礴，符合修仙小说风格
4. 根据他们的境界、战力分、特质、血脉等数据进行点评
5. 突出每个人的特点和传奇之处";
            string userPrompt;
            if (!string.IsNullOrEmpty(previousContent))
            {
                string previousStory = previousContent;
                int idx = previousContent.IndexOf("---战力排行榜传奇---");
                if (idx >= 0)
                    previousStory = previousContent.Substring(idx + "---战力排行榜传奇---".Length).Trim();
                int yearsPassed = (previousYear > 0 && currentYear > previousYear) ? (currentYear - previousYear) : 0;
                string yearsInfo = yearsPassed > 0 ? $"距离上次排行榜更新已过去{yearsPassed}年。" : "";
                userPrompt = $"请根据以下战力排行榜前三名数据，重新创作一篇修仙传奇故事。\n\n【上一次生成的内容】（第{previousYear}年）\n{previousStory}\n\n【当前排行榜数据】\n{actorData}\n\n{yearsInfo}请参考上一次的内容和排行榜更新情况，要有所创新和变化，不要简单复制。";
            }
            else
            {
                userPrompt = $"请根据以下战力排行榜前三名数据，创作一篇修仙传奇故事：\n\n{actorData}";
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
                        return xn.voice.DeepSeekTextGenerator.FilterThinkingProcess(chatResponse.choices[0].message.content);
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Debug.LogWarning($"[XN-RankingLegend] API调用失败: {response.StatusCode}");
                    throw new Exception($"API调用失败: {response.StatusCode}");
                }
            }
            throw new Exception("API返回数据为空");
        }
    }
}