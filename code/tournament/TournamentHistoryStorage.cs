using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Newtonsoft.Json;
namespace xn.tournament
{
    public static class TournamentHistoryStorage
    {
        private static List<TournamentHistoryData> _histories = new List<TournamentHistoryData>();
        private const string HISTORY_FILE_NAME = "xn_tournament_history.json";
        public static void AddHistory(TournamentHistoryData data)
        {
            if (data == null) return;
            _histories.Add(data);
        }
        public static List<TournamentHistoryData> GetAllHistories()
        {
            return new List<TournamentHistoryData>(_histories);
        }
        public static void Clear()
        {
            _histories.Clear();
        }
        public static int GetCount()
        {
            return _histories.Count;
        }
        public static void SaveToPath(string savePath)
        {
            if (string.IsNullOrEmpty(savePath)) return;
            if (_histories.Count == 0) return;
            try
            {
                string filePath = savePath + HISTORY_FILE_NAME;
                string json = JsonConvert.SerializeObject(_histories);
                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[XN-Tournament] 保存历史记录失败: {e.Message}");
            }
        }
        public static void LoadFromPath(string savePath)
        {
            if (string.IsNullOrEmpty(savePath)) return;
            _histories.Clear();
            try
            {
                string filePath = savePath + HISTORY_FILE_NAME;
                if (!File.Exists(filePath))
                {
                    TournamentManager.SetEditionCounter(0);
                    return;
                }
                string json = File.ReadAllText(filePath);
                var loaded = JsonConvert.DeserializeObject<List<TournamentHistoryData>>(json);
                if (loaded != null)
                {
                    _histories = loaded;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[XN-Tournament] 加载历史记录失败: {e.Message}");
            }
            int maxEdition = 0;
            foreach (var history in _histories)
            {
                if (history.Edition > maxEdition)
                {
                    maxEdition = history.Edition;
                }
            }
            TournamentManager.SetEditionCounter(maxEdition);
        }
    }
    [HarmonyPatch(typeof(SaveManager), "saveMapData")]
    internal static class TournamentSaveHook
    {
        [HarmonyPostfix]
        private static void Postfix(string pFolder)
        {
            string path = SaveManager.folderPath(pFolder);
            TournamentHistoryStorage.SaveToPath(path);
        }
    }
    [HarmonyPatch(typeof(SaveManager), "loadWorld", typeof(string), typeof(bool))]
    internal static class TournamentLoadHook
    {
        [HarmonyPostfix]
        private static void Postfix(string pPath)
        {
            string path = SaveManager.folderPath(pPath);
            TournamentHistoryStorage.LoadFromPath(path);
        }
    }
    [HarmonyPatch(typeof(MapBox), "generateNewMap")]
    internal static class TournamentNewWorldHook
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            TournamentHistoryStorage.Clear();
        }
    }
}