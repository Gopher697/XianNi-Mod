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
            var customData = xn.access.MapBoxAccess.GetCustomData(World.world);
            if (customData == null) return 0;
            customData.get(WORLD_KEY_HISTORY_COUNT, out int count, 0);
            return count;
        }
        private static void IncrementGenerationCount()
        {
            var customData = xn.access.MapBoxAccess.EnsureCustomData(World.world);
            if (customData == null) return;
            int current = GetGenerationCount();
            customData.set(WORLD_KEY_HISTORY_COUNT, current + 1);
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
            _isGenerating = false;
        }
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key, null);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
        private static string F(string key, string fallback, params object[] args)
        {
            return string.Format(T(key, fallback), args);
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
                string message = _isGenerating
                    ? T("cultivation_history_generating", "Generating, please wait...")
                    : F("cultivation_history_limit_reached_format", "This world has reached the generation limit ({0} times)", MAX_GENERATIONS);
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
                callback?.Invoke(F("ai_generation_failed_with_error", "Generation failed: {0}", e.Message));
            }
            finally
            {
                _isGenerating = false;
            }
        }
        internal static string CollectActorData(Actor actor)
        {
            if (actor == null || actor.isRekt())
                return T("cultivation_history_unknown_actor", "Unknown actor");
            var sb = new StringBuilder();
            string baseName = xn.world.TitleSystem.GetBaseName(actor);
            string title = xn.world.TitleSystem.GetTitle(actor);
            string suffix = xn.world.TitleSystem.GetSuffix(actor);
            sb.AppendLine(F("cultivation_history_data_name", "Name: {0}", baseName));
            if (!string.IsNullOrEmpty(title))
                sb.AppendLine(F("cultivation_history_data_title", "Title: {0}", title));
            if (!string.IsNullOrEmpty(suffix))
                sb.AppendLine(F("cultivation_history_data_realm_suffix", "Realm Suffix: {0}", suffix));
            sb.AppendLine(F("cultivation_history_data_age", "Age: {0}", actor.getAge()));
            sb.AppendLine(F("cultivation_history_data_gender", "Gender: {0}", xn.access.ActorAccess.GetData(actor).sex == ActorSex.Male ? T("cultivation_history_gender_male", "Male") : T("cultivation_history_gender_female", "Female")));
            var actorAsset = actor.asset;
            if (actorAsset != null && !string.IsNullOrEmpty(actorAsset.id))
            {
                string raceName = GetActorAssetDisplayName(actorAsset);
                sb.AppendLine(F("cultivation_history_data_race", "Race: {0}", raceName));
            }
            string realmName = GetActorRealm(actor);
            sb.AppendLine(F("cultivation_history_data_realm", "Realm: {0}", realmName));
            xn.access.ActorAccess.GetData(actor).get("xn.stat.xiuwei", out long xiuwei, 0L);
            sb.AppendLine(F("cultivation_history_data_cultivation", "Cultivation: {0}", FormatNumber(xiuwei)));
            var stats = xn.access.BaseSimObjectAccess.GetStats(actor);
            if (stats != null)
            {
                sb.AppendLine(F("cultivation_history_data_health", "Health: {0}", Mathf.RoundToInt(stats["health"])));
                sb.AppendLine(F("cultivation_history_data_attack", "Attack: {0}", Mathf.RoundToInt(stats["damage"])));
                sb.AppendLine(F("cultivation_history_data_defense", "Defense: {0}", Mathf.RoundToInt(stats["armor"])));
                sb.AppendLine(F("cultivation_history_data_speed", "Speed: {0}", Mathf.RoundToInt(stats["speed"])));
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
                    sb.AppendLine(F("cultivation_history_data_traits", "Traits: {0}", string.Join(T("cultivation_history_trait_separator", ", "), cultivationTraits)));
            }
            var kingdom = actor.kingdom;
            if (kingdom != null && !kingdom.isRekt())
                sb.AppendLine(F("cultivation_history_data_kingdom", "Kingdom: {0}", kingdom.data.name));
            var city = actor.city;
            if (city != null && !city.isRekt())
                sb.AppendLine(F("cultivation_history_data_city", "City: {0}", city.data.name));
            var citizenJob = actor.citizen_job;
            if (citizenJob != null && !string.IsNullOrEmpty(citizenJob.id))
            {
                string jobName = TryGetLocalized(citizenJob.id);
                if (string.IsNullOrEmpty(jobName))
                    jobName = FormatId(citizenJob.id);
                sb.AppendLine(F("cultivation_history_data_job", "Occupation: {0}", jobName));
            }
            AppendMentorshipData(actor, sb);
            sb.AppendLine(F("cultivation_history_data_kills", "Kills: {0}", xn.access.ActorAccess.GetData(actor).kills));
            sb.AppendLine(F("cultivation_history_data_births", "Births: {0}", xn.access.ActorAccess.GetData(actor).births));
            xn.access.ActorAccess.GetData(actor).get("xn.possession.taken", out int possession, 0);
            if (possession > 0)
                sb.AppendLine(T("cultivation_history_data_possessed", "Experience: Was once possessed"));
            xn.access.ActorAccess.GetData(actor).get("xn.reincarnation.count", out int reincarnation, 0);
            if (reincarnation > 0)
                sb.AppendLine(F("cultivation_history_data_reincarnation", "Reincarnations: {0}", reincarnation));
            xn.access.ActorAccess.GetData(actor).get("xn.tianyun.count", out int tianyun, 0);
            if (tianyun > 0)
                sb.AppendLine(F("cultivation_history_data_tianyun", "Heavenly Fate Encounters: {0}", tianyun));
            if (xn.bloodline.BloodlineSystem.HasBloodline(actor))
            {
                string bloodlineType = xn.bloodline.BloodlineSystem.GetBloodlineType(actor);
                string typeName = xn.bloodline.BloodlineTypes.GetLocaleName(bloodlineType);
                float concentration = xn.bloodline.BloodlineSystem.GetConcentration(actor);
                sb.AppendLine(F("cultivation_history_data_bloodline", "Bloodline: {0} (Concentration: {1:F1}%)", typeName, concentration));
            }
            if (actor.hasTrait("realm_14_gtianzun") || actor.hasTrait("realm_15_half_tatian") || actor.hasTrait("realm_16_tatian"))
            {
                xn.access.ActorAccess.GetData(actor).get("xn.trial.bridge", out long bridgeL, 0L);
                sb.AppendLine(F("cultivation_history_data_heaven_trampling_bridges", "Heaven Trampling Bridges: {0}/9", (int)bridgeL));
            }
            string ancientRealm = GetAncientRealm(actor);
            if (!string.IsNullOrEmpty(ancientRealm))
            {
                sb.AppendLine(F("cultivation_history_data_ancient_god_realm", "Ancient God Realm: {0}", ancientRealm));
                xn.access.ActorAccess.GetData(actor).get("xn.stat.gushen_power", out int gushenPower, 0);
                if (gushenPower > 0)
                    sb.AppendLine(F("cultivation_history_data_ancient_god_power", "Ancient God Power: {0}", gushenPower));
            }
            string beastRealm = GetBeastRealm(actor);
            if (!string.IsNullOrEmpty(beastRealm))
            {
                sb.AppendLine(F("cultivation_history_data_beast_realm", "Beast Realm: {0}", beastRealm));
                xn.access.ActorAccess.GetData(actor).get("xn.stat.yaoli", out int yaoli, 0);
                if (yaoli > 0)
                    sb.AppendLine(F("cultivation_history_data_beast_power", "Beast Power: {0}", yaoli));
            }
            xn.access.ActorAccess.GetData(actor).get("xn.stat.wuxin", out int wuxin, 0);
            if (wuxin > 0)
                sb.AppendLine(F("cultivation_history_data_comprehension", "Comprehension: {0}", wuxin));
            xn.access.ActorAccess.GetData(actor).get("xn.stat.qiyun", out int qiyun, 0);
            if (qiyun != 0)
                sb.AppendLine(F("cultivation_history_data_luck", "Luck/Fate: {0}", qiyun));
            xn.access.ActorAccess.GetData(actor).get("xn.stat.xinmo", out int xinmo, 0);
            if (xinmo > 0)
                sb.AppendLine(F("cultivation_history_data_inner_demon", "Inner Demon: {0}", xinmo));
            return sb.ToString();
        }
        private static void AppendMentorshipData(Actor actor, StringBuilder sb)
        {
            const string KEY_MASTER_ID = "xn_men_master_id";
            const string KEY_DISCIPLES_IDS = "xn_men_disciples_ids";

            xn.access.ActorAccess.GetData(actor).get(KEY_MASTER_ID, out long masterId, 0L);
            if (masterId > 0 && World.world != null && World.world.units != null)
            {
                Actor master = World.world.units.get(masterId);
                if (master != null && !master.isRekt())
                    sb.AppendLine(F("cultivation_history_data_master", "Master: {0}", GetActorStoryName(master)));
            }

            xn.access.ActorAccess.GetData(actor).get(KEY_DISCIPLES_IDS, out string idsStr, "");
            if (string.IsNullOrEmpty(idsStr) || World.world == null || World.world.units == null)
                return;

            var names = new System.Collections.Generic.List<string>();
            string[] parts = idsStr.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!long.TryParse(parts[i], out long discipleId) || discipleId <= 0)
                    continue;

                Actor disciple = World.world.units.get(discipleId);
                if (disciple != null && !disciple.isRekt())
                    names.Add(GetActorStoryName(disciple));
            }

            if (names.Count > 0)
                sb.AppendLine(F("cultivation_history_data_disciples", "Disciple(s): {0}", string.Join(T("cultivation_history_trait_separator", ", "), names)));
        }
        private static string GetActorStoryName(Actor actor)
        {
            if (actor == null) return T("value_unknown", "Unknown");
            string baseName = xn.world.TitleSystem.GetBaseName(actor);
            if (!string.IsNullOrEmpty(baseName)) return baseName;
            string name = actor.getName();
            return string.IsNullOrEmpty(name) ? T("value_unknown", "Unknown") : name;
        }
        private static string GetActorAssetDisplayName(ActorAsset actorAsset)
        {
            string name = TryGetLocalized(actorAsset.name_locale);
            if (!string.IsNullOrEmpty(name))
                return name;

            name = TryGetLocalized(actorAsset.id);
            if (!string.IsNullOrEmpty(name))
                return name;

            return actorAsset.id;
        }
        private static string TryGetLocalized(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            string fallback = KnownFallback(key);
            if (!string.IsNullOrEmpty(fallback))
                return fallback;

            string text = LocalizedTextManager.getText(key, null);
            return string.IsNullOrEmpty(text) || text == key ? null : text;
        }
        private static string KnownFallback(string key)
        {
            switch (key)
            {
                case "Human":
                    return "Human";
                case "gatherer_herbs":
                    return "Herb Gatherer";
                default:
                    return null;
            }
        }
        private static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id)) return T("value_unknown", "Unknown");
            string[] parts = id.Replace('-', '_').Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0) continue;
                parts[i] = char.ToUpperInvariant(parts[i][0]) + (parts[i].Length > 1 ? parts[i].Substring(1) : "");
            }
            return string.Join(" ", parts);
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
            return T("cultivation_history_no_realm", "No realm");
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
            var (endpoint, model) = xn.voice.AITextGenerator.GetProviderConfig();
            string systemPrompt = T("cultivation_history_system_prompt", "You are a professional cultivation (Renegade Immortal) novelist. Based on the provided character data, write a short cultivation-history story.\nRequirements:\n1. Keep it between 50 and 550 words\n2. The story should have a beginning, development, turn, and conclusion, including cultivation, breakthroughs, trials, and wandering experience\n3. Use the character's realm, traits, relationships, experiences, and other data to weave a plausible plot\n4. Treat the Immortal/Devil Cultivator path as a moment of self-revelation shaped by the character's nature, Inner Demon, Comprehension, traits, and relationships; never describe it as random fate or an equal default chance\n5. Use vivid, flavorful language that fits a cultivation novel\n6. Make the story complete and leave no unresolved suspense\n7. Do not repeat the raw character data; weave it naturally into the story");
            string userPrompt = F("cultivation_history_user_prompt", "Based on the following character data, write a cultivation-history story:\n\n{0}", actorData);
            int tokenLimit = IsUsingCustomConfig() ? 8192 : 1500;
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
                        return xn.voice.AITextGenerator.FilterThinkingProcess(chatResponse.choices[0].message.content);
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Debug.LogWarning(F("cultivation_history_api_call_failed_log", "[XN-History] API call failed: {0} (Model: {1})", response.StatusCode, model));
                    Debug.LogWarning(F("cultivation_history_api_error_detail_log", "[XN-History] Error details: {0}", errorBody));
                    throw new Exception(F("ai_api_call_failed", "API call failed: {0}", response.StatusCode));
                }
            }
            throw new Exception(T("ai_api_empty_response", "API returned empty data"));
        }
    }
}
