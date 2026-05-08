using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
namespace xn.voice
{
    public static class VoiceCache
    {
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();
        private static string _cacheIndexFile;
        private static string _cacheDir;
        private static bool _initialized = false;
        public static void Init(string audioRoot)
        {
            if (_initialized) return;
            _cacheDir = Path.Combine(audioRoot, "cache");
            _cacheIndexFile = Path.Combine(_cacheDir, "index.json");
            if (!Directory.Exists(_cacheDir))
            {
                Directory.CreateDirectory(_cacheDir);
            }
            LoadCache();
            _initialized = true;
        }
        private static void LoadCache()
        {
            if (File.Exists(_cacheIndexFile))
            {
                try
                {
                    string json = File.ReadAllText(_cacheIndexFile);
                    _cache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                        ?? new Dictionary<string, string>();
                    var keysToRemove = new List<string>();
                    foreach (var kvp in _cache)
                    {
                        if (!File.Exists(kvp.Value))
                        {
                            keysToRemove.Add(kvp.Key);
                        }
                    }
                    foreach (var key in keysToRemove)
                    {
                        _cache.Remove(key);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[XN-Voice] Failed to load cache index: {e.Message}");
                    _cache = new Dictionary<string, string>();
                }
            }
        }
        private static void SaveCache()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_cache, Formatting.Indented);
                File.WriteAllText(_cacheIndexFile, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[XN-Voice] Failed to save cache index: {e.Message}");
            }
        }
        private static string GetCacheKey(string text, string voiceId)
        {
            return $"{ComputeHash(text)}_{voiceId}";
        }
        private static string ComputeHash(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16).ToLower();
            }
        }
        public static string GetCachedFile(string text, string voiceId)
        {
            if (!_initialized || string.IsNullOrEmpty(text)) return null;
            string key = GetCacheKey(text, voiceId);
            if (_cache.TryGetValue(key, out string filePath))
            {
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    _cache.Remove(key);
                    SaveCache();
                }
            }
            return null;
        }
        public static void AddToCache(string text, string voiceId, string filePath)
        {
            if (!_initialized || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(filePath))
                return;
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[XN-Voice] Attempted to cache non-existent file: {filePath}");
                return;
            }
            string key = GetCacheKey(text, voiceId);
            _cache[key] = filePath;
            SaveCache();
        }
        public static string GenerateCacheFilePath(string text, string voiceId)
        {
            if (!_initialized) return null;
            string hash = ComputeHash(text);
            string fileName = $"{hash}_{voiceId}.mp3";
            return Path.Combine(_cacheDir, fileName);
        }
        public static void ClearCache()
        {
            if (!_initialized) return;
            try
            {
                foreach (var filePath in _cache.Values)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                _cache.Clear();
                SaveCache();
            }
            catch (Exception e)
            {
                Debug.LogError($"[XN-Voice] Failed to clear cache: {e.Message}");
            }
        }
        public static string GetCacheStats()
        {
            if (!_initialized) return "Cache not initialized";
            long totalSize = 0;
            int fileCount = 0;
            foreach (var filePath in _cache.Values)
            {
                if (File.Exists(filePath))
                {
                    FileInfo fi = new FileInfo(filePath);
                    totalSize += fi.Length;
                    fileCount++;
                }
            }
            double sizeMB = totalSize / (1024.0 * 1024.0);
            return $"Cache files: {fileCount}, total size: {sizeMB:F2} MB";
        }
    }
}