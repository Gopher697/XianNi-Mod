using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace xn.world
{
    internal static class TitleSystem
    {
        private static bool _inited;
        private const string KEY_BASE_NAME = "xn.title.base_name";   
        private const string KEY_TITLE = "xn.title.current";          
        private const string KEY_SUFFIX = "xn.title.suffix";          
        private static readonly Dictionary<string, string[]> _titleCache = new Dictionary<string, string[]>();
        private static string _titleFolderPath;
        private static readonly string[] REALM_TRAIT_IDS = new[]
        {
            "realm_01_qi",
            "realm_02_foundation",
            "realm_03_core",
            "realm_04_nascent",
            "realm_05_deity",
            "realm_06_infantchg",
            "realm_07_wending",
            "realm_08_kuinie",
            "realm_09_jingnie",
            "realm_10_suinie",
            "realm_11_kongnie",
            "realm_12_kongling",
            "realm_13_kongxuan",
            "realm_14_gtianzun",
            "realm_15_half_tatian",
            "realm_16_tatian"
        };
        private static readonly string[] BEAST_STAGE_IDS = new[]
        {
            "beast_01_stage",
            "beast_02_stage",
            "beast_03_stage",
            "beast_04_stage",
            "beast_05_stage",
            "beast_06_stage",
            "beast_07_stage",
            "beast_08_stage",
            "beast_09_stage",
            "beast_10_stage"
        };
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        private static string TraitName(string traitId, string fallback)
        {
            return string.IsNullOrEmpty(traitId) ? fallback : T("trait_" + traitId, fallback);
        }
        public static void ClearTitleData(Actor actor)
        {
            if (actor == null) return;
            xn.access.ActorAccess.GetData(actor).set(KEY_BASE_NAME, "");
            xn.access.ActorAccess.GetData(actor).set(KEY_TITLE, "");
            xn.access.ActorAccess.GetData(actor).set(KEY_SUFFIX, "");
        }
        public static string GetBaseName(Actor actor)
        {
            if (actor == null) return "";
            xn.access.ActorAccess.GetData(actor).get(KEY_BASE_NAME, out string baseName, "");
            if (!string.IsNullOrEmpty(baseName)) return baseName;
            string name = actor.getName() ?? "";
            ExtractTitleAndBaseName(actor, name, out _, out string parsed);
            return string.IsNullOrEmpty(parsed) ? name : parsed;
        }
        public static string GetTitle(Actor actor)
        {
            if (actor == null) return "";
            xn.access.ActorAccess.GetData(actor).get(KEY_TITLE, out string title, "");
            return title ?? "";
        }
        public static string GetSuffix(Actor actor)
        {
            if (actor == null) return "";
            xn.access.ActorAccess.GetData(actor).get(KEY_SUFFIX, out string suffix, "");
            return suffix ?? "";
        }
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            InitTitleFolderPath();
            AttachAllHooks();
            Debug.Log("[XN] TitleSystem: title hooks attached to realm_/ancient_/beast_ traits");
        }
        private static void InitTitleFolderPath()
        {
            string[] possiblePaths = new string[]
            {
                Path.Combine(Application.streamingAssetsPath, "Mods", "xianni", "Title"),
                Path.Combine(Application.dataPath, "Mods", "xianni", "Title"),
                Path.Combine(Directory.GetCurrentDirectory(), "Mods", "xianni", "Title")
            };
            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    _titleFolderPath = path;
                    Debug.Log("[XN] TitleSystem: title folder path: " + _titleFolderPath);
                    return;
                }
            }
            _titleFolderPath = possiblePaths[0];
            Debug.LogWarning("[XN] TitleSystem: title folder not found, using default path: " + _titleFolderPath);
        }
        private static string GetSubFolder(string fileKey)
        {
            if (fileKey.StartsWith("realm_")) return "Realm";
            if (fileKey.StartsWith("ancient_")) return "Ancient";
            if (fileKey.StartsWith("beast_")) return "Beast";
            return "";
        }
        private static string[] LoadTitlesFromFile(string fileKey)
        {
            if (_titleCache.TryGetValue(fileKey, out string[] cached))
                return cached;
            if (string.IsNullOrEmpty(_titleFolderPath))
            {
                _titleCache[fileKey] = null;
                return null;
            }
            string subFolder = GetSubFolder(fileKey);
            string filePath = string.IsNullOrEmpty(subFolder)
                ? Path.Combine(_titleFolderPath, fileKey + ".txt")
                : Path.Combine(_titleFolderPath, subFolder, fileKey + ".txt");
            if (!File.Exists(filePath))
            {
                _titleCache[fileKey] = null;
                return null;
            }
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                var titles = new List<string>();
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        titles.Add(trimmed);
                }
                string[] result = titles.Count > 0 ? titles.ToArray() : null;
                _titleCache[fileKey] = result;
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError("[XN] TitleSystem: failed to read title file " + filePath + ": " + ex.Message);
                _titleCache[fileKey] = null;
                return null;
            }
        }
        private static void AttachAllHooks()
        {
            var list = AssetManager.traits.list;
            if (list == null || list.Count == 0)
                return;
            foreach (var t in list)
            {
                if (t == null || string.IsNullOrEmpty(t.id))
                    continue;
                string id = t.id;
                if (id.StartsWith("realm_"))
                {
                    var hook = new WorldActionTrait(RealmHook);
                    t.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(
                        t.action_on_augmentation_add,
                        hook
                    );
                    continue;
                }
                if (id.StartsWith("ancient_"))
                {
                    var hook = new WorldActionTrait(AncientHook);
                    t.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(
                        t.action_on_augmentation_add,
                        hook
                    );
                    continue;
                }
                if (id.StartsWith("beast_"))
                {
                    var hook = new WorldActionTrait(BeastHook);
                    t.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(
                        t.action_on_augmentation_add,
                        hook
                    );
                    continue;
                }
            }
        }
        private static bool RealmHook(NanoObject target, BaseAugmentationAsset traitAsset)
        {
            try
            {
                var actor = target as Actor;
                var trait = traitAsset as ActorTrait;
                if (actor != null && trait != null)
                {
                    if (actor.asset != null && actor.asset.id == "dashou")
                        return false;
                    OnRealmTraitAdded(actor, trait.id);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[XN] TitleSystem RealmHook exception: " + ex);
            }
            return false;
        }
        private static bool AncientHook(NanoObject target, BaseAugmentationAsset traitAsset)
        {
            try
            {
                var actor = target as Actor;
                var trait = traitAsset as ActorTrait;
                if (actor != null && trait != null)
                {
                    if (actor.asset != null && actor.asset.id == "dashou")
                        return false;
                    OnAncientRealmTraitAdded(actor, trait.id);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[XN] TitleSystem AncientHook exception: " + ex);
            }
            return false;
        }
        private static bool BeastHook(NanoObject target, BaseAugmentationAsset traitAsset)
        {
            try
            {
                var actor = target as Actor;
                var trait = traitAsset as ActorTrait;
                if (actor != null && trait != null)
                {
                    if (actor.asset != null && actor.asset.id == "dashou")
                        return false;
                    OnBeastStageTraitAdded(actor, trait.id);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[XN] TitleSystem BeastHook exception: " + ex);
            }
            return false;
        }
        private static readonly Dictionary<string, string> _realmSuffixMap =
            new Dictionary<string, string>
        {
            { "realm_01", "Qi Condensation" },
            { "realm_02", "Foundation Establishment" },
            { "realm_03", "Core Formation" },
            { "realm_04", "Nascent Soul" },
            { "realm_05", "Soul Formation" },
            { "realm_06", "Soul Transformation" },
            { "realm_07", "Ascendant" },
            { "realm_08", "Nirvana Scryer" },
            { "realm_09", "Nirvana Cleanser" },
            { "realm_10", "Nirvana Shatterer" },
            { "realm_11", "Void Nirvana" },
            { "realm_12", "Void Spirit" },
            { "realm_13", "Void Arcanum" },
            { "realm_14", "Grand Empyrean" },
            { "realm_15", "Half-Step Heaven Trampling" },
            { "realm_16", "Heaven Trampling" }
        };
        private static readonly Dictionary<string, string> _realmTraitIdMap =
            new Dictionary<string, string>
        {
            { "realm_01", "realm_01_qi" },
            { "realm_02", "realm_02_foundation" },
            { "realm_03", "realm_03_core" },
            { "realm_04", "realm_04_nascent" },
            { "realm_05", "realm_05_deity" },
            { "realm_06", "realm_06_infantchg" },
            { "realm_07", "realm_07_wending" },
            { "realm_08", "realm_08_kuinie" },
            { "realm_09", "realm_09_jingnie" },
            { "realm_10", "realm_10_suinie" },
            { "realm_11", "realm_11_kongnie" },
            { "realm_12", "realm_12_kongling" },
            { "realm_13", "realm_13_kongxuan" },
            { "realm_14", "realm_14_gtianzun" },
            { "realm_15", "realm_15_half_tatian" },
            { "realm_16", "realm_16_tatian" }
        };
        private static readonly Dictionary<string, string> _beastSuffixMap =
            new Dictionary<string, string>
        {
            { "beast_01_stage", "1st Tier Beast" },
            { "beast_02_stage", "2nd Tier Beast" },
            { "beast_03_stage", "3rd Tier Beast" },
            { "beast_04_stage", "4th Tier Beast" },
            { "beast_05_stage", "5th Tier Beast" },
            { "beast_06_stage", "6th Tier Beast" },
            { "beast_07_stage", "7th Tier Beast" },
            { "beast_08_stage", "8th Tier Beast" },
            { "beast_09_stage", "9th Tier Beast" },
            { "beast_10_stage", "10th Tier Beast" }
        };
        private static string GetAncientSuffix(int starLevel)
        {
            switch (starLevel)
            {
                case 1: return TraitName("ancient_01_star", "Ancient God: 1-Star");
                case 2: return TraitName("ancient_02_star", "Ancient God: 2-Star");
                case 3: return TraitName("ancient_03_star", "Ancient God: 3-Star");
                case 4: return TraitName("ancient_04_star", "Ancient God: 4-Star");
                case 5: return TraitName("ancient_05_star", "Ancient God: 5-Star");
                case 6: return TraitName("ancient_06_star", "Ancient God: 6-Star");
                case 7: return TraitName("ancient_07_star", "Ancient God: 7-Star");
                case 8: return TraitName("ancient_08_star", "Ancient God: 8-Star");
                case 9: return TraitName("ancient_09_star", "Ancient God: 9-Star");
                case 10: return TraitName("ancient_10_star", "Ancient God: 10-Star");
            }
            return null;
        }
        private static string NormalizeRealmKey(string traitId)
        {
            if (string.IsNullOrEmpty(traitId) || !traitId.StartsWith("realm_"))
                return null;
            int secondUnderscore = traitId.IndexOf('_', "realm_".Length);
            if (secondUnderscore <= 0)
                return traitId; 
            return traitId.Substring(0, secondUnderscore);
        }
        public static void OnRealmTraitAdded(Actor actor, string traitId)
        {
            if (!xn.config.ModConfigHooks.EnableTitleGeneration) return;
            if (actor == null || string.IsNullOrEmpty(traitId))
                return;
            string key = NormalizeRealmKey(traitId);
            if (string.IsNullOrEmpty(key))
                return;
            if (!_realmSuffixMap.TryGetValue(key, out string suffix))
                return;
            if (_realmTraitIdMap.TryGetValue(key, out string traitNameKey))
                suffix = TraitName(traitNameKey, suffix);
            string currentName = actor.getName();
            if (string.IsNullOrEmpty(currentName))
                return;
            ExtractTitleAndBaseName(actor, currentName, out string titlePart, out string baseName);
            string title = null;
            if (!string.IsNullOrEmpty(titlePart) && titlePart.Length > 2)
                title = titlePart.Substring(1, titlePart.Length - 2);
            string[] titles = LoadTitlesFromFile(key);
            if (titles != null && titles.Length > 0)
            {
                int index = UnityEngine.Random.Range(0, titles.Length);
                if (index >= 0 && index < titles.Length)
                {
                    string picked = titles[index];
                    if (!string.IsNullOrEmpty(picked))
                    {
                        title = picked;
                    }
                }
            }
            if (string.IsNullOrEmpty(baseName))
                baseName = currentName.Trim();
            SetActorName(actor, title, baseName, suffix);
        }
        public static void OnAncientRealmTraitAdded(Actor actor, string traitId)
        {
            if (!xn.config.ModConfigHooks.EnableTitleGeneration) return;
            if (actor == null || string.IsNullOrEmpty(traitId))
                return;
            int star = GetAncientStarLevel(traitId);
            if (star <= 0)
                return;
            if (star >= 3)
            {
                ApplyAncientGodTitle(actor, star);
            }
            string suffix = GetAncientSuffix(star);
            if (string.IsNullOrEmpty(suffix))
                return;
            string currentName = actor.getName();
            if (string.IsNullOrEmpty(currentName))
                return;
            ExtractTitleAndBaseName(actor, currentName, out string titlePart, out string baseName);
            if (string.IsNullOrEmpty(baseName))
                baseName = currentName.Trim();
            string title = null;
            xn.access.ActorAccess.GetData(actor).get(KEY_TITLE, out string storedTitle, "");
            if (!string.IsNullOrEmpty(storedTitle))
            {
                title = storedTitle;
            }
            else if (!string.IsNullOrEmpty(titlePart) && titlePart.Length > 2)
            {
                title = titlePart.Substring(1, titlePart.Length - 2);
            }
            SetActorName(actor, title, baseName, suffix);
        }
        private static int GetAncientStarLevel(string traitId)
        {
            if (string.IsNullOrEmpty(traitId) || !traitId.StartsWith("ancient_"))
                return 0;
            int secondUnderscore = traitId.IndexOf('_', "ancient_".Length);
            if (secondUnderscore <= "ancient_".Length)
                return 0;
            string numStr = traitId.Substring("ancient_".Length, secondUnderscore - "ancient_".Length);
            if (int.TryParse(numStr, out int level))
                return level;
            return 0;
        }
        public static void ApplyAncientGodTitle(Actor actor, int starLevel)
        {
            if (actor == null)
                return;
            string fileKey = "ancient_" + starLevel.ToString("D2");
            string[] titles = LoadTitlesFromFile(fileKey);
            if (titles == null || titles.Length == 0)
                return;
            string currentName = actor.getName();
            if (string.IsNullOrEmpty(currentName))
                return;
            ExtractTitleAndBaseName(actor, currentName, out string titlePart, out string baseName);
            int index = UnityEngine.Random.Range(0, titles.Length);
            if (index < 0 || index >= titles.Length)
                return;
            string picked = titles[index];
            if (string.IsNullOrEmpty(picked))
                return;
            if (string.IsNullOrEmpty(baseName))
                baseName = currentName.Trim();
            xn.access.ActorAccess.GetData(actor).set(KEY_BASE_NAME, baseName);
            xn.access.ActorAccess.GetData(actor).set(KEY_TITLE, picked);
        }
        private static int GetBeastStage(string traitId)
        {
            if (string.IsNullOrEmpty(traitId) || !traitId.StartsWith("beast_"))
                return 0;
            int secondUnderscore = traitId.IndexOf('_', "beast_".Length);
            if (secondUnderscore <= "beast_".Length)
                return 0;
            string numStr = traitId.Substring("beast_".Length, secondUnderscore - "beast_".Length);
            if (int.TryParse(numStr, out int stage))
                return stage;
            return 0;
        }
        public static void OnBeastStageTraitAdded(Actor actor, string traitId)
        {
            if (!xn.config.ModConfigHooks.EnableTitleGeneration) return;
            if (actor == null || string.IsNullOrEmpty(traitId))
                return;
            if (!_beastSuffixMap.TryGetValue(traitId, out string suffix))
                return;
            suffix = TraitName(traitId, suffix);
            string currentName = actor.getName();
            if (string.IsNullOrEmpty(currentName))
                return;
            ExtractTitleAndBaseName(actor, currentName, out string titlePart, out string baseName);
            if (string.IsNullOrEmpty(baseName))
                baseName = currentName.Trim();
            string title = null;
            if (!string.IsNullOrEmpty(titlePart) && titlePart.Length > 2)
                title = titlePart.Substring(1, titlePart.Length - 2);
            int stage = GetBeastStage(traitId);
            if (stage >= 3)
            {
                string fileKey = "beast_" + stage.ToString("D2");
                string[] titles = LoadTitlesFromFile(fileKey);
                if (titles != null && titles.Length > 0)
                {
                    int index = UnityEngine.Random.Range(0, titles.Length);
                    if (index >= 0 && index < titles.Length)
                    {
                        string picked = titles[index];
                        if (!string.IsNullOrEmpty(picked))
                        {
                            title = picked;
                        }
                    }
                }
            }
            SetActorName(actor, title, baseName, suffix);
        }
        public static void ReconcileActorName(Actor actor)
        {
            if (!xn.config.ModConfigHooks.EnableTitleGeneration) return;
            if (actor == null)
                return;
            string suffix = GetCurrentCultivationSuffix(actor);
            if (string.IsNullOrEmpty(suffix))
                return;
            string currentName = actor.getName();
            if (string.IsNullOrEmpty(currentName))
                return;
            ExtractTitleAndBaseName(actor, currentName, out string titlePart, out string baseName);
            if (string.IsNullOrEmpty(baseName))
                baseName = currentName.Trim();
            string title = null;
            xn.access.ActorAccess.GetData(actor).get(KEY_TITLE, out string storedTitle, "");
            if (!string.IsNullOrEmpty(storedTitle))
                title = storedTitle;
            else if (!string.IsNullOrEmpty(titlePart) && titlePart.Length > 2)
                title = titlePart.Substring(1, titlePart.Length - 2);
            string expectedName = BuildExpectedName(title, baseName, suffix);
            if (currentName.Trim() == expectedName)
            {
                xn.access.ActorAccess.GetData(actor).set(KEY_SUFFIX, suffix);
                return;
            }
            SetActorName(actor, title, baseName, suffix);
        }
        private static string GetCurrentCultivationSuffix(Actor actor)
        {
            int ancientStar = GetHighestAncientStar(actor);
            if (ancientStar > 0)
                return GetAncientSuffix(ancientStar);
            int beastStage = GetHighestBeastStage(actor);
            if (beastStage > 0)
            {
                string beastId = "beast_" + beastStage.ToString("D2") + "_stage";
                if (_beastSuffixMap.TryGetValue(beastId, out string beastFallback))
                    return TraitName(beastId, beastFallback);
            }
            string realmId = GetHighestRealmTrait(actor);
            if (!string.IsNullOrEmpty(realmId))
            {
                string key = NormalizeRealmKey(realmId);
                if (!string.IsNullOrEmpty(key) && _realmSuffixMap.TryGetValue(key, out string realmFallback))
                    return TraitName(realmId, realmFallback);
            }
            return null;
        }
        private static int GetHighestAncientStar(Actor actor)
        {
            if (actor == null) return 0;
            for (int i = 10; i >= 1; i--)
            {
                string id = "ancient_" + i.ToString("D2") + "_star";
                if (actor.hasTrait(id))
                    return i;
            }
            return 0;
        }
        private static int GetHighestBeastStage(Actor actor)
        {
            if (actor == null) return 0;
            for (int i = BEAST_STAGE_IDS.Length - 1; i >= 0; i--)
            {
                if (actor.hasTrait(BEAST_STAGE_IDS[i]))
                    return i + 1;
            }
            return 0;
        }
        private static string GetHighestRealmTrait(Actor actor)
        {
            if (actor == null) return null;
            for (int i = REALM_TRAIT_IDS.Length - 1; i >= 0; i--)
            {
                if (actor.hasTrait(REALM_TRAIT_IDS[i]))
                    return REALM_TRAIT_IDS[i];
            }
            return null;
        }
        private static void ExtractTitleAndBaseName(Actor actor, string name, out string titlePart, out string baseName)
        {
            titlePart = null;
            baseName = "";
            if (actor == null || string.IsNullOrEmpty(name))
                return;
            string storedBase = null;
            string storedTitle = null;
            string storedSuffix = null;
            xn.access.ActorAccess.GetData(actor).get(KEY_BASE_NAME, out storedBase, null);
            xn.access.ActorAccess.GetData(actor).get(KEY_TITLE, out storedTitle, null);
            xn.access.ActorAccess.GetData(actor).get(KEY_SUFFIX, out storedSuffix, null);
            if (!string.IsNullOrEmpty(storedBase))
            {
                string expectedName = BuildExpectedName(storedTitle, storedBase, storedSuffix);
                if (name.Trim() != expectedName)
                {
                    storedBase = null;
                    storedTitle = null;
                }
                else
                {
                    baseName = storedBase;
                    titlePart = string.IsNullOrEmpty(storedTitle) ? null : "[" + storedTitle + "]";
                    return;
                }
            }
            string rest = name.Trim();
            int lastBracket = rest.LastIndexOf(']');
            if (lastBracket >= 0 && lastBracket + 1 < rest.Length)
            {
                rest = rest.Substring(lastBracket + 1).Trim();
            }
            else
            {
                while (true)
                {
                    int startBracket = rest.IndexOf('[');
                    if (startBracket < 0) break;
                    int endBracket = rest.IndexOf(']', startBracket);
                    if (endBracket <= startBracket) break;
                    rest = rest.Substring(0, startBracket) + rest.Substring(endBracket + 1);
                    rest = rest.Trim();
                }
            }
            int dash = rest.IndexOf('-');
            baseName = dash >= 0 ? rest.Substring(0, dash).Trim() : rest.Trim();
            if (string.IsNullOrEmpty(baseName))
                baseName = name.Trim();
            if (!string.IsNullOrEmpty(baseName))
            {
                xn.access.ActorAccess.GetData(actor).set(KEY_BASE_NAME, baseName);
            }
        }
        private static string BuildExpectedName(string title, string baseName, string suffix)
        {
            string titlePart = string.IsNullOrEmpty(title) ? "" : "[" + title + "]";
            string suffixPart = string.IsNullOrEmpty(suffix) ? "" : "-" + suffix;
            return titlePart + baseName + suffixPart;
        }
        private static void SetActorName(Actor actor, string title, string baseName, string suffix)
        {
            if (actor == null || string.IsNullOrEmpty(baseName))
                return;
            xn.access.ActorAccess.GetData(actor).set(KEY_BASE_NAME, baseName);
            xn.access.ActorAccess.GetData(actor).set(KEY_TITLE, title ?? "");
            xn.access.ActorAccess.GetData(actor).set(KEY_SUFFIX, suffix ?? "");
            string titlePart = string.IsNullOrEmpty(title) ? "" : "[" + title + "]";
            string suffixPart = string.IsNullOrEmpty(suffix) ? "" : "-" + suffix;
            string finalName = titlePart + baseName + suffixPart;
            actor.setName(finalName);
        }
    }
}
