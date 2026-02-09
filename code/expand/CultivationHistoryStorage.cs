using System.IO;
using System.Linq;
namespace xn.expand
{
    public static class CultivationHistoryStorage
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
                    {
                        Directory.CreateDirectory(_cultivationFolder);
                    }
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
                    var mapStats = World.world?.map_stats;
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
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }
        private static string GetFilePath(long actorId, double createdTime)
        {
            string folder = GetCultivationFolder();
            if (string.IsNullOrEmpty(folder)) return null;
            string saveId = GetCurrentSaveId();
            long timeId = (long)createdTime;
            return Path.Combine(folder, $"{saveId}_{actorId}_{timeId}.txt");
        }
        public static void ResetSaveId()
        {
            _currentSaveId = null;
        }
        public static bool Save(long actorId, double createdTime, string actorName, string history)
        {
            try
            {
                string filePath = GetFilePath(actorId, createdTime);
                if (string.IsNullOrEmpty(filePath)) return false;
                string saveId = GetCurrentSaveId();
                long timeId = (long)createdTime;
                string content = $"存档ID: {saveId}\n角色ID: {actorId}\n创建时间: {timeId}\n角色名: {actorName}\n生成时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n---修仙史---\n\n{history}";
                File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string Load(long actorId, double createdTime)
        {
            try
            {
                string filePath = GetFilePath(actorId, createdTime);
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    return File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                }
                string folder = GetCultivationFolder();
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                {
                    long timeId = (long)createdTime;
                    string pattern = $"*_{actorId}_{timeId}.txt";
                    string[] matchingFiles = Directory.GetFiles(folder, pattern);
                    if (matchingFiles.Length > 0)
                    {
                        var newestFile = matchingFiles
                            .Select(f => new FileInfo(f))
                            .OrderByDescending(fi => fi.LastWriteTime)
                            .FirstOrDefault();
                        if (newestFile != null)
                        {
                            return File.ReadAllText(newestFile.FullName, System.Text.Encoding.UTF8);
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public static bool HasHistory(long actorId, double createdTime)
        {
            string filePath = GetFilePath(actorId, createdTime);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                return true;
            string folder = GetCultivationFolder();
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                long timeId = (long)createdTime;
                string pattern = $"*_{actorId}_{timeId}.txt";
                string[] matchingFiles = Directory.GetFiles(folder, pattern);
                return matchingFiles.Length > 0;
            }
            return false;
        }
        public static bool Delete(long actorId, double createdTime)
        {
            try
            {
                string filePath = GetFilePath(actorId, createdTime);
                if (string.IsNullOrEmpty(filePath)) return false;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void ClearAllHistory()
        {
            try
            {
                string folder = GetCultivationFolder();
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                    return;
                string[] files = Directory.GetFiles(folder, "*.txt");
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}