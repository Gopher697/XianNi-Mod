using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
namespace xn.voice
{
    public class OnlineEdgeTTSProvider : ITTSProvider
    {
        private const string GOOGLE_TTS_URL = "https://translate.google.com/translate_tts";
        public string GetProviderName()
        {
            return "Google TTS";
        }
        public async Task<bool> GenerateSpeech(string text, string voiceId, string outputPath)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            try
            {
                string directory = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    if (text.Length > 200)
                    {
                        text = text.Substring(0, 200);
                    }
                    string encodedText = Uri.EscapeDataString(text);
                    string url = $"{GOOGLE_TTS_URL}?ie=UTF-8&q={encodedText}&tl=zh-CN&client=tw-ob";
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    client.DefaultRequestHeaders.Add("Referer", "https://translate.google.com/");
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] audioData = await response.Content.ReadAsByteArrayAsync();
                        if (audioData != null && audioData.Length > 100)
                        {
                            File.WriteAllBytes(outputPath, audioData);
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[XN-Voice] TTS生成异常: {e.Message}");
            }
            return false;
        }
    }
}