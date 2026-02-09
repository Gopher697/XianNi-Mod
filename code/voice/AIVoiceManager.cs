using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
namespace xn.voice
{
    public static class AIVoiceManager
    {
        private static string _audioRoot;
        private static bool _initialized = false;
        private static ITTSProvider _ttsProvider;   
        private static Queue<VoiceRequest> _requestQueue = new Queue<VoiceRequest>();
        private static bool _isProcessing = false;
        private class VoiceRequest
        {
            public string Text;
            public bool IgnoreSwitch;
        }
        public static void Init()
        {
            if (_initialized)
            {
                return;
            }
            try
            {
                var declare = XNMain.Instance?.GetDeclaration();
                if (declare != null && !string.IsNullOrEmpty(declare.FolderPath))
                {
                    _audioRoot = Path.Combine(declare.FolderPath, "GameResources", "audio", "ai_voice");
                    if (!Directory.Exists(_audioRoot))
                    {
                        Directory.CreateDirectory(_audioRoot);
                    }
                    string cacheDir = Path.Combine(_audioRoot, "cache");
                    if (!Directory.Exists(cacheDir))
                    {
                        Directory.CreateDirectory(cacheDir);
                    }
                    VoiceCache.Init(_audioRoot);
                    VoiceCache.ClearCache();
                    _ttsProvider = new OnlineEdgeTTSProvider();
                    _initialized = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[XN-Voice] 初始化异常: {e.Message}");
            }
        }
        public static void Play(string text, bool ignoreSwitch = false)
        {
            if (!_initialized || string.IsNullOrEmpty(text))
            {
                return;
            }
            if (!ignoreSwitch && !xn.config.ModConfigHooks.EnableAIVoice)
            {
                return;
            }
            _requestQueue.Enqueue(new VoiceRequest
            {
                Text = text,
                IgnoreSwitch = ignoreSwitch
            });
            if (!_isProcessing)
            {
                _ = ProcessQueueAsync();
            }
        }
        private static async Task ProcessQueueAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            try
            {
                while (_requestQueue.Count > 0)
                {
                    var request = _requestQueue.Dequeue();
                    await GenerateAndPlayAsync(request.Text, request.IgnoreSwitch);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[XN-Voice] 处理队列异常: {e.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }
        private static async Task GenerateAndPlayAsync(string text, bool ignoreSwitch)
        {
            try
            {
                string cachedFile = VoiceCache.GetCachedFile(text, "");
                if (cachedFile != null && File.Exists(cachedFile))
                {
                    PlayAudioFile(cachedFile, ignoreSwitch);
                    return;
                }
                string outputPath = VoiceCache.GenerateCacheFilePath(text, "");
                if (string.IsNullOrEmpty(outputPath))
                {
                    return;
                }
                bool success = false;
                if (_ttsProvider != null)
                {
                    success = await _ttsProvider.GenerateSpeech(text, "", outputPath);
                }
                if (success && File.Exists(outputPath))
                {
                    VoiceCache.AddToCache(text, "", outputPath);
                    PlayAudioFile(outputPath, ignoreSwitch);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[XN-Voice] 生成并播放异常: {e.Message}");
            }
        }
        private static void PlayAudioFile(string fullPath, bool ignoreSwitch)
        {
            try
            {
                if (!File.Exists(fullPath))
                {
                    return;
                }
                string audioBaseDir = Path.Combine(
                    XNMain.Instance.GetDeclaration().FolderPath,
                    "GameResources",
                    "audio"
                );
                string relativePath = fullPath.Replace(audioBaseDir, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                relativePath = relativePath.Replace('\\', '/');
                xn.expand.AudioManager.Play(relativePath, ignoreSwitch: true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[XN-Voice] 播放音频异常: {e.Message}");
            }
        }
    }
}