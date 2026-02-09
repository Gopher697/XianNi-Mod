using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
namespace xn.questions
{
    [Serializable]
    public class QuestionItem
    {
        public string q;
        public string a;
        public string tag;
    }
    [Serializable]
    public class QuestionsData
    {
        public QuestionItem[] questions;
    }
    public static class QuestionsLoader
    {
        private const string QUESTIONS_URL = "https://gitee.com/KangKang0606/version/raw/master/questions.json";
        private static bool _loaded;
        private static bool _loading;
        private static List<QuestionItem> _questions = new List<QuestionItem>();
        private static readonly List<Action<List<QuestionItem>>> _pendingCallbacks = new();
        public static bool IsLoaded => _loaded;
        public static void LoadOnce()
        {
            if (_loaded || _loading) return;
            _loading = true;
            if (MapBox.instance != null)
                ((MonoBehaviour)MapBox.instance).StartCoroutine(LoadCoroutine());
            else
                _loading = false;
        }
        public static void GetQuestions(Action<List<QuestionItem>> callback)
        {
            if (callback == null) return;
            if (_loaded)
            {
                callback.Invoke(_questions);
                return;
            }
            _pendingCallbacks.Add(callback);
            if (!_loading)
            {
                _loading = true;
                if (MapBox.instance != null)
                    ((MonoBehaviour)MapBox.instance).StartCoroutine(LoadCoroutine());
                else
                    InvokeAllCallbacks();
            }
        }
        public static List<QuestionItem> GetCached() => _questions;
        private static IEnumerator LoadCoroutine()
        {
            using (var www = UnityWebRequest.Get(QUESTIONS_URL))
            {
                www.timeout = 15;
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        ParseJson(www.downloadHandler.text);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[XIANNI] QuestionsLoader parse error: {e.Message}");
                    }
                }
                _loaded = true;
                _loading = false;
                InvokeAllCallbacks();
            }
        }
        private static void ParseJson(string json)
        {
            try
            {
                var data = JsonUtility.FromJson<QuestionsData>(json);
                if (data?.questions != null && data.questions.Length > 0)
                {
                    _questions.Clear();
                    foreach (var item in data.questions)
                    {
                        item.q = DecodeRichText(item.q);
                        item.a = DecodeRichText(item.a);
                        _questions.Add(item);
                    }
                    return;
                }
            }
            catch { }
            ParseWithRegex(json);
        }
        private static string DecodeRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">");
            text = text.Replace("&amp;", "&");
            text = text.Replace("&quot;", "\"");
            text = text.Replace("\\n", "\n");
            return text;
        }
        private static void ParseWithRegex(string json)
        {
            var pattern = @"""q""\s*:\s*""([^""]*)""\s*,\s*""a""\s*:\s*""([^""]*)""\s*,\s*""tag""\s*:\s*""([^""]*)""";
            var matches = Regex.Matches(json, pattern);
            if (matches.Count > 0)
            {
                _questions.Clear();
                foreach (Match m in matches)
                {
                    _questions.Add(new QuestionItem
                    {
                        q = DecodeRichText(m.Groups[1].Value),
                        a = DecodeRichText(m.Groups[2].Value),
                        tag = m.Groups[3].Value
                    });
                }
            }
        }
        private static void InvokeAllCallbacks()
        {
            foreach (var cb in _pendingCallbacks)
            {
                try { cb?.Invoke(_questions); }
                catch { }
            }
            _pendingCallbacks.Clear();
        }
        public static void Reload()
        {
            _loaded = false;
            _loading = false;
            _questions.Clear();
            LoadOnce();
        }
    }
}