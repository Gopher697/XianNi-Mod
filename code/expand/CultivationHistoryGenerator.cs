using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using UnityEngine;
namespace xn.expand
{
    public static class CultivationHistoryGenerator
    {
        private static string GetAPIKey()
        {
            if (IsUsingCustomConfig())
            {
                return xn.config.ModConfigHooks.CustomAIApiKey ?? "";
            }
            return "";
        }
        private const int MAX_GENERATIONS = 15;
        private const string WORLD_KEY_HISTORY_COUNT = "xn.history.gen_count";
        private static bool _isGenerating = false;
        private static int GetGenerationCount()
        {
            if (World.world?.map_stats?.custom_data == null)
                return 0;
            World.world.map_stats.custom_data.get(WORLD_KEY_HISTORY_COUNT, out int count, 0);
            return count;
        }
        private static void IncrementGenerationCount()
        {
            if (World.world?.map_stats == null) return;
            if (World.world.map_stats.custom_data == null)
                World.world.map_stats.custom_data = new SaveCustomData();
            int current = GetGenerationCount();
            World.world.map_stats.custom_data.set(WORLD_KEY_HISTORY_COUNT, current + 1);
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
            public int max_tokens = 1500;
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
            _isGenerating = false;
        }
        public static int GetRemainingGenerations()
        {
            if (IsUsingCustomConfig())
                return int.MaxValue;
            return Math.Max(0, MAX_GENERATIONS - GetGenerationCount());
        }
        public static bool CanGenerate()
        {
            if (_isGenerating)
                return false;
            if (IsUsingCustomConfig())
                return true;
            return GetGenerationCount() < MAX_GENERATIONS;
        }
        public static async void GenerateCultivationHistory(Actor actor, Action<string> callback)
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
                string actorData = CollectActorData(actor);
                string apiKey = GetAPIKey();
                string history = await GenerateHistoryFromAPI(actorData, apiKey);
                if (!IsUsingCustomConfig())
                    IncrementGenerationCount();
                callback?.Invoke(history);
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
        private static string CollectActorData(Actor actor)
        {
            if (actor == null || actor.isRekt())
                return "未知角色";
            var sb = new StringBuilder();
            string baseName = xn.world.TitleSystem.GetBaseName(actor);
            string title = xn.world.TitleSystem.GetTitle(actor);
            string suffix = xn.world.TitleSystem.GetSuffix(actor);
            sb.AppendLine($"名字：{baseName}");
            if (!string.IsNullOrEmpty(title))
                sb.AppendLine($"称号：{title}");
            if (!string.IsNullOrEmpty(suffix))
                sb.AppendLine($"境界后缀：{suffix}");
            sb.AppendLine($"年龄：{actor.getAge()}岁");
            sb.AppendLine($"性别：{(actor.data.sex == ActorSex.Male ? "男" : "女")}");
            var actorAsset = actor.asset;
            if (actorAsset != null && !string.IsNullOrEmpty(actorAsset.id))
            {
                string raceName = LocalizedTextManager.getText(actorAsset.id);
                if (string.IsNullOrEmpty(raceName) || raceName == actorAsset.id)
                    raceName = actorAsset.id;
                sb.AppendLine($"种族：{raceName}");
            }
            string realmName = GetActorRealm(actor);
            sb.AppendLine($"境界：{realmName}");
            actor.data.get("xn.stat.xiuwei", out long xiuwei, 0L);
            sb.AppendLine($"修为：{FormatNumber(xiuwei)}");
            var stats = actor.stats;
            if (stats != null)
            {
                sb.AppendLine($"生命：{Mathf.RoundToInt(stats["health"])}");
                sb.AppendLine($"攻击：{Mathf.RoundToInt(stats["damage"])}");
                sb.AppendLine($"防御：{Mathf.RoundToInt(stats["armor"])}");
                sb.AppendLine($"速度：{Mathf.RoundToInt(stats["speed"])}");
            }
            var traits = actor.getTraits();
            if (traits != null && traits.Count > 0)
            {
                var cultivationTraits = new System.Collections.Generic.List<string>();
                foreach (var trait in traits)
                {
                    if (trait != null && !trait.id.StartsWith("realm_"))
                    {
                        string traitName = trait.getTranslatedName();
                        if (!string.IsNullOrEmpty(traitName))
                            cultivationTraits.Add(traitName);
                    }
                }
                if (cultivationTraits.Count > 0)
                    sb.AppendLine($"特质：{string.Join("、", cultivationTraits)}");
            }
            var kingdom = actor.kingdom;
            if (kingdom != null && !kingdom.isRekt())
                sb.AppendLine($"国家：{kingdom.data.name}");
            var city = actor.city;
            if (city != null && !city.isRekt())
                sb.AppendLine($"城市：{city.data.name}");
            var citizenJob = actor.citizen_job;
            if (citizenJob != null && !string.IsNullOrEmpty(citizenJob.id))
            {
                string jobName = LocalizedTextManager.getText(citizenJob.id);
                if (string.IsNullOrEmpty(jobName) || jobName == citizenJob.id)
                    jobName = citizenJob.id;
                sb.AppendLine($"职业：{jobName}");
            }
            sb.AppendLine($"击杀：{actor.data.kills}");
            sb.AppendLine($"生育次数：{actor.data.births}");
            actor.data.get("xn.possession.taken", out int possession, 0);
            if (possession > 0)
                sb.AppendLine("经历：曾被夺舍");
            actor.data.get("xn.reincarnation.count", out int reincarnation, 0);
            if (reincarnation > 0)
                sb.AppendLine($"轮回：{reincarnation}次");
            actor.data.get("xn.tianyun.count", out int tianyun, 0);
            if (tianyun > 0)
                sb.AppendLine($"天运：{tianyun}次");
            if (xn.bloodline.BloodlineSystem.HasBloodline(actor))
            {
                string bloodlineType = xn.bloodline.BloodlineSystem.GetBloodlineType(actor);
                string typeName = xn.bloodline.BloodlineTypes.GetLocaleName(bloodlineType);
                float concentration = xn.bloodline.BloodlineSystem.GetConcentration(actor);
                sb.AppendLine($"血脉：{typeName}（浓度：{concentration:F1}%）");
            }
            if (actor.hasTrait("realm_14_gtianzun") || actor.hasTrait("realm_15_half_tatian") || actor.hasTrait("realm_16_tatian"))
            {
                actor.data.get("xn.trial.bridge", out long bridgeL, 0L);
                sb.AppendLine($"踏天九桥：{(int)bridgeL}/9");
            }
            string ancientRealm = GetAncientRealm(actor);
            if (!string.IsNullOrEmpty(ancientRealm))
            {
                sb.AppendLine($"古神境界：{ancientRealm}");
                actor.data.get("xn.stat.gushen_power", out int gushenPower, 0);
                if (gushenPower > 0)
                    sb.AppendLine($"古神之力：{gushenPower}");
            }
            string beastRealm = GetBeastRealm(actor);
            if (!string.IsNullOrEmpty(beastRealm))
            {
                sb.AppendLine($"妖修境界：{beastRealm}");
                actor.data.get("xn.stat.yaoli", out int yaoli, 0);
                if (yaoli > 0)
                    sb.AppendLine($"妖力：{yaoli}");
            }
            actor.data.get("xn.stat.wuxin", out int wuxin, 0);
            if (wuxin > 0)
                sb.AppendLine($"悟性：{wuxin}");
            actor.data.get("xn.stat.qiyun", out int qiyun, 0);
            if (qiyun != 0)
                sb.AppendLine($"气运：{qiyun}");
            actor.data.get("xn.stat.xinmo", out int xinmo, 0);
            if (xinmo > 0)
                sb.AppendLine($"心魔：{xinmo}");
            return sb.ToString();
        }
        private static string GetAncientRealm(Actor actor)
        {
            string[] ancientIds = new[]
            {
                "ancient_01_star", "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star",
                "ancient_06_star", "ancient_07_star", "ancient_08_star", "ancient_09_star", "ancient_10_star"
            };
            foreach (var id in ancientIds)
            {
                if (actor.hasTrait(id))
                {
                    var trait = AssetManager.traits.get(id);
                    if (trait != null)
                        return trait.getTranslatedName();
                }
            }
            return null;
        }
        private static string GetBeastRealm(Actor actor)
        {
            string[] beastIds = new[]
            {
                "beast_01_stage", "beast_02_stage", "beast_03_stage", "beast_04_stage", "beast_05_stage",
                "beast_06_stage", "beast_07_stage", "beast_08_stage", "beast_09_stage", "beast_10_stage"
            };
            foreach (var id in beastIds)
            {
                if (actor.hasTrait(id))
                {
                    var trait = AssetManager.traits.get(id);
                    if (trait != null)
                        return trait.getTranslatedName();
                }
            }
            return null;
        }
        private static string GetActorRealm(Actor actor)
        {
            string[] realmIds = new[]
            {
                "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
                "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
                "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
                "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian"
            };
            var traits = actor.getTraits();
            if (traits != null)
            {
                foreach (var realmId in realmIds)
                {
                    if (actor.hasTrait(realmId))
                    {
                        var trait = AssetManager.traits.get(realmId);
                        if (trait != null)
                            return trait.getTranslatedName();
                    }
                }
            }
            return "无境界";
        }
        private static string FormatNumber(long num)
        {
            if (num >= 1000000000) return (num / 1000000000.0).ToString("F1") + "B";
            if (num >= 1000000) return (num / 1000000.0).ToString("F1") + "M";
            if (num >= 1000) return (num / 1000.0).ToString("F1") + "K";
            return num.ToString();
        }
        private static async Task<string> GenerateHistoryFromAPI(string actorData, string apiKey)
        {
            var (endpoint, model) = xn.voice.DeepSeekTextGenerator.GetProviderConfig();
            string systemPrompt = @"你是一位专业的修仙(仙逆)小说作家。根据提供的角色数据，创作一段简短的修仙历程故事。
要求：
1. 字数控制在50-550字之间
2. 故事要有起承转合，包含修炼、突破、历练等元素
3. 根据角色的境界、特质、经历等数据，编织出合理的故事情节
4. 语言要生动有趣，符合修仙小说风格
5. 故事要完整，不要留下悬念
6. 不要重复角色数据，而是将数据融入故事中";
            string userPrompt = $"请根据以下角色数据，创作一段修仙历程故事：\n\n{actorData}";
            var request = new ChatRequest
            {
                messages = new[]
                {
                    new ChatMessage { role = "system", content = systemPrompt },
                    new ChatMessage { role = "user", content = userPrompt }
                },
                model = model,
                temperature = 0.8f,
                max_tokens = IsUsingCustomConfig() ? 8192 : 1500
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
                        return xn.voice.DeepSeekTextGenerator.FilterThinkingProcess(chatResponse.choices[0].message.content);
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Debug.LogWarning($"[XN-History] API调用失败: {response.StatusCode} (Model: {model})");
                    Debug.LogWarning($"[XN-History] 错误详情: {errorBody}");
                    throw new Exception($"API调用失败: {response.StatusCode}");
                }
            }
            throw new Exception("API返回数据为空");
        }
    }
}