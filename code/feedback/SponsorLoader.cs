using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
namespace xn.feedback
{
    public static class SponsorLoader
    {
        private const string SPONSOR_URL = "https://gitee.com/KangKang0606/version/raw/master/sponsors.json";
        private static bool _loaded;
        private static bool _loading;
        private static List<string> _sponsors = new List<string>();
        private static readonly List<Action<List<string>>> _pendingCallbacks = new();
        [Serializable]
        public class SponsorInfo
        {
            public string name;
            public int amount;
        }
        [Serializable]
        public class SponsorData
        {
            public SponsorInfo[] sponsors;
        }
        public static void LoadOnce()
        {
            if (_loaded || _loading) return;
            _loading = true;
            if (MapBox.instance != null)
            {
                ((MonoBehaviour)MapBox.instance).StartCoroutine(LoadSponsorsCoroutine());
            }
            else
            {
                _loading = false;
            }
        }
        public static void GetSponsors(Action<List<string>> callback)
        {
            if (callback == null) return;
            if (_loaded)
            {
                callback.Invoke(_sponsors);
                return;
            }
            _pendingCallbacks.Add(callback);
            if (!_loading)
            {
                _loading = true;
                if (MapBox.instance != null)
                {
                    ((MonoBehaviour)MapBox.instance).StartCoroutine(LoadSponsorsCoroutine());
                }
                else
                {
                    InvokeAllCallbacks();
                }
            }
        }
        private static IEnumerator LoadSponsorsCoroutine()
        {
            using (var www = UnityWebRequest.Get(SPONSOR_URL))
            {
                www.timeout = 15;
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        ParseSponsorsWithRegex(www.downloadHandler.text);
                    }
                    catch { }
                }
                _loaded = true;
                _loading = false;
                InvokeAllCallbacks();
            }
        }
        private static void ParseSponsorsWithRegex(string json)
        {
            try
            {
                var matches = Regex.Matches(json, @"""name""\s*:\s*""([^""]*)""\s*,\s*""amount""\s*:\s*(\d+)");
                if (matches.Count == 0)
                {
                    matches = Regex.Matches(json, @"""amount""\s*:\s*(\d+)\s*,\s*""name""\s*:\s*""([^""]*)""");
                    if (matches.Count > 0)
                    {
                        var list = new List<(string name, int amount)>();
                        foreach (Match m in matches)
                        {
                            int amount = int.Parse(m.Groups[1].Value);
                            string name = m.Groups[2].Value;
                            list.Add((name, amount));
                        }
                        ProcessSponsorList(list);
                        return;
                    }
                }
                if (matches.Count > 0)
                {
                    var list = new List<(string name, int amount)>();
                    foreach (Match m in matches)
                    {
                        string name = m.Groups[1].Value;
                        int amount = int.Parse(m.Groups[2].Value);
                        list.Add((name, amount));
                    }
                    ProcessSponsorList(list);
                }
            }
            catch { }
        }
        private static void ProcessSponsorList(List<(string name, int amount)> list)
        {
            list.Sort((a, b) => b.amount.CompareTo(a.amount));
            _sponsors.Clear();
            foreach (var (name, amount) in list)
            {
                if (!string.IsNullOrEmpty(name))
                    _sponsors.Add(name);
            }
        }
        private static void InvokeAllCallbacks()
        {
            foreach (var callback in _pendingCallbacks)
            {
                try
                {
                    callback?.Invoke(_sponsors);
                }
                catch { }
            }
            _pendingCallbacks.Clear();
        }
        public static List<string> GetCachedSponsors()
        {
            return _sponsors;
        }
        public static bool IsLoaded => _loaded;
        public static void Reload()
        {
            _loaded = false;
            _loading = false;
            _sponsors.Clear();
            LoadOnce();
        }
    }
}