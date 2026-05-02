using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace xn.version
{
    internal static class OnlineVersionChecker
    {
        private static readonly string[] VERSION_URLS = {
            "https://gitee.com/KangKang0606/version/raw/master/xianni.json",
            "https://raw.githubusercontent.com/kangkang0606/version/main/xianni.json"
        };
        private const string CURRENT_VERSION = "0.3.97";
        public static string CurrentVersion => CURRENT_VERSION;
        private static bool _hasChecked;
        private static GameObject _popupWindow;
        private static string _downloadUrl;
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
        public static void CheckOnLoad()
        {
            if (_hasChecked) return;
            _hasChecked = true;
            if (MapBox.instance != null)
            {
                ((MonoBehaviour)MapBox.instance).StartCoroutine(DelayedCheck());
            }
        }
        private static IEnumerator DelayedCheck()
        {
            while (!Config.game_loaded || World.world == null)
            {
                yield return null;
            }
            yield return new WaitForSeconds(5f);
            yield return CheckVersionCoroutine();
        }
        private static IEnumerator CheckVersionCoroutine()
        {
            foreach (var url in VERSION_URLS)
            {
                using (var www = UnityWebRequest.Get(url))
                {
                    www.timeout = 8;
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            var data = JsonUtility.FromJson<VersionData>(www.downloadHandler.text);
                            if (!string.IsNullOrEmpty(data.version) && data.version != CURRENT_VERSION)
                            {
                                if (IsNewerVersion(data.version, CURRENT_VERSION))
                                {
                                    ShowUpdatePopup(data);
                                }
                            }
                            yield break;
                        }
                        catch { }
                    }
                }
            }
        }
        private static bool IsNewerVersion(string remote, string local)
        {
            try
            {
                var remoteParts = remote.Split('.');
                var localParts = local.Split('.');
                for (int i = 0; i < Math.Max(remoteParts.Length, localParts.Length); i++)
                {
                    int r = i < remoteParts.Length ? int.Parse(remoteParts[i]) : 0;
                    int l = i < localParts.Length ? int.Parse(localParts[i]) : 0;
                    if (r > l) return true;
                    if (r < l) return false;
                }
            }
            catch { }
            return false;
        }
        private static void ShowUpdatePopup(VersionData data)
        {
            CreatePopupWindow(data);
        }
        private static void CreatePopupWindow(VersionData data)
        {
            if (_popupWindow != null) return;
            _downloadUrl = data.download_url;
            var canvas = CanvasMain.instance.canvas_ui;
            _popupWindow = new GameObject("XN_UpdatePopup");
            _popupWindow.transform.SetParent(canvas.transform, false);
            var popupCanvas = _popupWindow.AddComponent<Canvas>();
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 9999;
            _popupWindow.AddComponent<GraphicRaycaster>();
            var bgObj = CreateUIElement("Background", _popupWindow.transform);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);
            var bgBtn = bgObj.AddComponent<Button>();
            bgBtn.onClick.AddListener(ClosePopup);
            bool hasNotice = !string.IsNullOrEmpty(data.notice);
            bool hasUrl = !string.IsNullOrEmpty(data.download_url);
            bool hasChangelog = !string.IsNullOrEmpty(data.changelog);
            float panelHeight = 200f;
            if (hasChangelog) panelHeight += 80f;
            if (hasNotice) panelHeight += 60f;
            if (hasUrl) panelHeight += 50f;
            var panelObj = CreateUIElement("Panel", _popupWindow.transform);
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(450, panelHeight);
            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            float currentY = panelHeight / 2 - 35f;
            CreateText("Title", panelObj.transform, new Vector2(0, currentY), new Vector2(420, 35),
                T("version_update_title", "New Xianni Mod Version Available!"), 24, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.2f));
            currentY -= 50f;
            CreateText("Info", panelObj.transform, new Vector2(0, currentY), new Vector2(420, 45),
                string.Format(T("version_update_info", "Current: {0}  Latest: {1}"), CURRENT_VERSION, data.version), 16, TextAnchor.MiddleCenter, Color.white);
            currentY -= 45f;
            if (hasChangelog)
            {
                var changelogBg = CreateUIElement("ChangelogBg", panelObj.transform);
                var changelogBgRect = changelogBg.GetComponent<RectTransform>();
                changelogBgRect.anchoredPosition = new Vector2(0, currentY - 35f);
                changelogBgRect.sizeDelta = new Vector2(420, 70);
                var changelogBgImg = changelogBg.AddComponent<Image>();
                changelogBgImg.color = new Color(0.08f, 0.08f, 0.1f, 1f);
                var changelogTxt = CreateUIElement("Text", changelogBg.transform);
                var changelogTxtRect = changelogTxt.GetComponent<RectTransform>();
                changelogTxtRect.anchorMin = Vector2.zero;
                changelogTxtRect.anchorMax = Vector2.one;
                changelogTxtRect.sizeDelta = new Vector2(-10, -6);
                changelogTxtRect.anchoredPosition = Vector2.zero;
                var txt = changelogTxt.AddComponent<Text>();
                txt.text = data.changelog;
                txt.font = LocalizedTextManager.current_font;
                txt.fontSize = 14;
                txt.alignment = TextAnchor.UpperCenter;
                txt.color = new Color(0.75f, 0.75f, 0.75f);
                currentY -= 80f;
            }
            if (hasNotice)
            {
                var noticeObj = CreateUIElement("Notice", panelObj.transform);
                var noticeRect = noticeObj.GetComponent<RectTransform>();
                noticeRect.anchoredPosition = new Vector2(0, currentY - 20f);
                noticeRect.sizeDelta = new Vector2(420, 45);
                var noticeBg = noticeObj.AddComponent<Image>();
                noticeBg.color = new Color(0.2f, 0.15f, 0.08f, 1f);
                var noticeTxt = CreateUIElement("Text", noticeObj.transform);
                var noticeTxtRect = noticeTxt.GetComponent<RectTransform>();
                noticeTxtRect.anchorMin = Vector2.zero;
                noticeTxtRect.anchorMax = Vector2.one;
                noticeTxtRect.sizeDelta = Vector2.zero;
                var txt = noticeTxt.AddComponent<Text>();
                txt.text = data.notice;
                txt.font = LocalizedTextManager.current_font;
                txt.fontSize = 13;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = new Color(1f, 0.9f, 0.6f);
                currentY -= 55f;
            }
            if (hasUrl)
            {
                var urlObj = CreateUIElement("UrlButton", panelObj.transform);
                var urlRect = urlObj.GetComponent<RectTransform>();
                urlRect.anchoredPosition = new Vector2(0, currentY - 15f);
                urlRect.sizeDelta = new Vector2(200, 36);
                var urlImage = urlObj.AddComponent<Image>();
                urlImage.color = new Color(0.2f, 0.4f, 0.6f, 1f);
                var urlBtn = urlObj.AddComponent<Button>();
                urlBtn.onClick.AddListener(OpenDownloadUrl);
                var urlTxt = CreateUIElement("Text", urlObj.transform);
                var urlTxtRect = urlTxt.GetComponent<RectTransform>();
                urlTxtRect.anchorMin = Vector2.zero;
                urlTxtRect.anchorMax = Vector2.one;
                urlTxtRect.sizeDelta = Vector2.zero;
                var txt = urlTxt.AddComponent<Text>();
                txt.text = T("version_download_button", "Download Latest Version");
                txt.font = LocalizedTextManager.current_font;
                txt.fontSize = 16;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
                currentY -= 45f;
            }
            var closeObj = CreateUIElement("CloseButton", panelObj.transform);
            var closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchoredPosition = new Vector2(0, -panelHeight / 2 + 30);
            closeRect.sizeDelta = new Vector2(100, 32);
            var closeImage = closeObj.AddComponent<Image>();
            closeImage.color = new Color(0.25f, 0.25f, 0.3f);
            var closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(ClosePopup);
            var closeTxt = CreateUIElement("Text", closeObj.transform);
            var closeTxtRect = closeTxt.GetComponent<RectTransform>();
            closeTxtRect.anchorMin = Vector2.zero;
            closeTxtRect.anchorMax = Vector2.one;
            closeTxtRect.sizeDelta = Vector2.zero;
            var closeTxtComp = closeTxt.AddComponent<Text>();
            closeTxtComp.text = T("common_close", "Close");
            closeTxtComp.font = LocalizedTextManager.current_font;
            closeTxtComp.fontSize = 15;
            closeTxtComp.alignment = TextAnchor.MiddleCenter;
            closeTxtComp.color = Color.white;
            MusicBox.playSoundUI("event:/SFX/UI/WindowWhoosh");
        }
        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }
        private static void CreateText(string name, Transform parent, Vector2 pos, Vector2 size,
            string text, int fontSize, TextAnchor alignment, Color color)
        {
            var obj = CreateUIElement(name, parent);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var txt = obj.AddComponent<Text>();
            txt.text = text;
            txt.font = LocalizedTextManager.current_font;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
        }
        private static void OpenDownloadUrl()
        {
            if (!string.IsNullOrEmpty(_downloadUrl))
            {
                Application.OpenURL(_downloadUrl);
            }
        }
        private static void ClosePopup()
        {
            if (_popupWindow != null)
            {
                MusicBox.playSoundUI("event:/SFX/UI/WindowClose");
                UnityEngine.Object.Destroy(_popupWindow);
                _popupWindow = null;
            }
        }
        [Serializable]
        private class VersionData
        {
            public string version;
            public string changelog;
            public string notice;
            public string download_url;
        }
    }
}
