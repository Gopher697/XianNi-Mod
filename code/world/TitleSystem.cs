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
        public static void ClearTitleData(Actor actor)
        {
            if (actor == null) return;
            actor.data.set(KEY_BASE_NAME, "");
            actor.data.set(KEY_TITLE, "");
            actor.data.set(KEY_SUFFIX, "");
        }
        public static string GetBaseName(Actor actor)
        {
            if (actor == null) return "";
            actor.data.get(KEY_BASE_NAME, out string baseName, "");
            if (!string.IsNullOrEmpty(baseName)) return baseName;
            string name = actor.getName() ?? "";
            ExtractTitleAndBaseName(actor, name, out _, out string parsed);
            return string.IsNullOrEmpty(parsed) ? name : parsed;
        }
        public static string GetTitle(Actor actor)
        {
            if (actor == null) return "";
            actor.data.get(KEY_TITLE, out string title, "");
            return title ?? "";
        }
        public static string GetSuffix(Actor actor)
        {
            if (actor == null) return "";
            actor.data.get(KEY_SUFFIX, out string suffix, "");
            return suffix ?? "";
        }
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            InitTitleFolderPath();
            AttachAllHooks();
            Debug.Log("[XN] TitleSystem: 称号逻辑已挂接到 realm_/ancient_/beast_ 特质");
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
                    Debug.Log("[XN] TitleSystem: 称号文件夹路径: " + _titleFolderPath);
                    return;
                }
            }
            _titleFolderPath = possiblePaths[0];
            Debug.LogWarning("[XN] TitleSystem: 未找到称号文件夹，使用默认路径: " + _titleFolderPath);
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
                Debug.LogError("[XN] TitleSystem: 读取称号文件失败 " + filePath + ": " + ex.Message);
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
                Debug.LogError("[XN] TitleSystem RealmHook 异常: " + ex);
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
                Debug.LogError("[XN] TitleSystem AncientHook 异常: " + ex);
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
                Debug.LogError("[XN] TitleSystem BeastHook 异常: " + ex);
            }
            return false;
        }
        private static readonly Dictionary<string, string> _realmSuffixMap =
            new Dictionary<string, string>
        {
            { "realm_01", "凝气" },
            { "realm_02", "筑基" },
            { "realm_03", "结丹" },
            { "realm_04", "元婴" },
            { "realm_05", "化神" },
            { "realm_06", "婴变" },
            { "realm_07", "问鼎" },
            { "realm_08", "窥涅" },
            { "realm_09", "净涅" },
            { "realm_10", "碎涅" },
            { "realm_11", "空涅" },
            { "realm_12", "空灵" },
            { "realm_13", "空玄" },
            { "realm_14", "天尊" },
            { "realm_15", "半踏天" },
            { "realm_16", "踏天" }
        };
        private static readonly Dictionary<string, string> _beastSuffixMap =
            new Dictionary<string, string>
        {
            { "beast_01_stage", "一阶妖兽" },
            { "beast_02_stage", "二阶妖兽" },
            { "beast_03_stage", "三阶妖兽" },
            { "beast_04_stage", "四阶妖兽" },
            { "beast_05_stage", "五阶妖兽" },
            { "beast_06_stage", "六阶妖兽" },
            { "beast_07_stage", "七阶妖兽" },
            { "beast_08_stage", "八阶妖兽" },
            { "beast_09_stage", "九阶妖兽" },
            { "beast_10_stage", "十阶妖兽" }
        };
        private static string GetAncientSuffix(int starLevel)
        {
            switch (starLevel)
            {
                case 1: return "一星古神";
                case 2: return "二星古神";
                case 3: return "三星古神";
                case 4: return "四星古神";
                case 5: return "五星古神";
                case 6: return "六星古神";
                case 7: return "七星古神";
                case 8: return "八星古神";
                case 9: return "九星古神";
                case 10: return "十星古神";
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
            if (actor == null || string.IsNullOrEmpty(traitId))
                return;
            string key = NormalizeRealmKey(traitId);
            if (string.IsNullOrEmpty(key))
                return;
            if (!_realmSuffixMap.TryGetValue(key, out string suffix))
                return;
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
            actor.data.get(KEY_TITLE, out string storedTitle, "");
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
            actor.data.set(KEY_BASE_NAME, baseName);
            actor.data.set(KEY_TITLE, picked);
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
            if (actor == null || string.IsNullOrEmpty(traitId))
                return;
            if (!_beastSuffixMap.TryGetValue(traitId, out string suffix))
                return;
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
        private static void ExtractTitleAndBaseName(Actor actor, string name, out string titlePart, out string baseName)
        {
            titlePart = null;
            baseName = "";
            if (actor == null || string.IsNullOrEmpty(name))
                return;
            string storedBase = null;
            string storedTitle = null;
            string storedSuffix = null;
            actor.data.get(KEY_BASE_NAME, out storedBase, null);
            actor.data.get(KEY_TITLE, out storedTitle, null);
            actor.data.get(KEY_SUFFIX, out storedSuffix, null);
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
                actor.data.set(KEY_BASE_NAME, baseName);
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
            actor.data.set(KEY_BASE_NAME, baseName);
            actor.data.set(KEY_TITLE, title ?? "");
            actor.data.set(KEY_SUFFIX, suffix ?? "");
            string titlePart = string.IsNullOrEmpty(title) ? "" : "[" + title + "]";
            string suffixPart = string.IsNullOrEmpty(suffix) ? "" : "-" + suffix;
            string finalName = titlePart + baseName + suffixPart;
            actor.setName(finalName);
        }
    }
}