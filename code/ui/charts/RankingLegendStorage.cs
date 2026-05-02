using System.IO;
namespace xn.ui.charts
{
    public static class RankingLegendStorage
    {
        private static string _cultivationFolder;
        private static string _currentSaveId;
        private static string GetCultivationFolder()
        {
            if (string.IsNullOrEmpty(_cultivationFolder))
            {
                var declare = XNMain.Instance?.GetDeclaration();
                if (declare != null && !string.IsNullOrEmpty(declare.FolderPath))
                {
                    _cultivationFolder = Path.Combine(declare.FolderPath, "cultivation");
                    if (!Directory.Exists(_cultivationFolder))
                        Directory.CreateDirectory(_cultivationFolder);
                }
            }
            return _cultivationFolder;
        }
        private static string GetCurrentSaveId()
        {
            if (string.IsNullOrEmpty(_currentSaveId))
            {
                string savePath = SaveManager.currentSavePath;
                if (!string.IsNullOrEmpty(savePath))
                {
                    _currentSaveId = GetStableHash(savePath).ToString("X8");
                }
                else
                {
                    var mapStats = xn.access.MapBoxAccess.GetMapStats(World.world);
                    if (mapStats != null)
                    {
                        string fallback = $"{mapStats.name}_{mapStats.world_time}";
                        _currentSaveId = GetStableHash(fallback).ToString("X8");
                    }
                    else
                    {
                        _currentSaveId = "DEFAULT";
                    }
                }
            }
            return _currentSaveId;
        }
        private static int GetStableHash(string str)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in str)
                    hash = hash * 31 + c;
                return hash;
            }
        }
        private static string GetFilePath()
        {
            string folder = GetCultivationFolder();
            if (string.IsNullOrEmpty(folder)) return null;
            string saveId = GetCurrentSaveId();
            return Path.Combine(folder, $"ranking_legend_{saveId}.txt");
        }
        public static void ResetSaveId() => _currentSaveId = null;
        public static bool Save(string content, int worldYear)
        {
            try
            {
                string filePath = GetFilePath();
                if (string.IsNullOrEmpty(filePath)) return false;
                string saveId = GetCurrentSaveId();
                string fullContent = $"存档ID: {saveId}\n生成时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n世界年份: {worldYear}\n\n---战力排行榜传奇---\n\n{content}";
                File.WriteAllText(filePath, fullContent, System.Text.Encoding.UTF8);
                return true;
            }
            catch { return false; }
        }
        public static int GetSavedWorldYear()
        {
            try
            {
                string content = Load();
                if (string.IsNullOrEmpty(content)) return 0;
                const string prefix = "世界年份: ";
                int idx = content.IndexOf(prefix);
                if (idx < 0) return 0;
                int start = idx + prefix.Length;
                int end = content.IndexOf('\n', start);
                if (end < 0) end = content.Length;
                string yearStr = content.Substring(start, end - start).Trim();
                if (int.TryParse(yearStr, out int year))
                    return year;
                return 0;
            }
            catch { return 0; }
        }
        public static string Load()
        {
            try
            {
                string filePath = GetFilePath();
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    return File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                return null;
            }
            catch { return null; }
        }
        public static bool HasLegend()
        {
            string filePath = GetFilePath();
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }
        public static bool Delete()
        {
            try
            {
                string filePath = GetFilePath();
                if (string.IsNullOrEmpty(filePath)) return false;
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return true;
            }
            catch { return false; }
        }
    }
}
