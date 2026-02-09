using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
namespace xn.tournament
{
    public static class TournamentHistoryWindow
    {
        private static bool _inited;
        private static ScrollWindow _window;
        private static Text _contentText;
        public const string WINDOW_ID = "xn_tournament_history";
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            try
            {
                _window = WindowCreator.CreateEmptyWindow(WINDOW_ID, "对战历史");
                var winRT = _window.transform as RectTransform;
                if (winRT != null)
                {
                    winRT.sizeDelta = new Vector2(700, 600);
                }
                _window.transform_scrollRect.gameObject.SetActive(true);
                var contentTransform = _window.transform_content;
                _contentText = contentTransform.gameObject.AddComponent<Text>();
                _contentText.font = LocalizedTextManager.current_font;
                _contentText.supportRichText = true;
                _contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
                _contentText.verticalOverflow = VerticalWrapMode.Overflow;
                _contentText.alignment = TextAnchor.UpperLeft;
                _contentText.color = new Color(1f, 0.95f, 0.8f);
                _contentText.resizeTextForBestFit = true;
                _contentText.resizeTextMinSize = 8;
                _contentText.resizeTextMaxSize = 14;
                _contentText.lineSpacing = 1.2f;
                var textRT = _contentText.rectTransform;
                textRT.anchorMin = new Vector2(0, 1);
                textRT.anchorMax = new Vector2(1, 1);
                textRT.pivot = new Vector2(0, 1);
                textRT.anchoredPosition = new Vector2(20, -10);
                textRT.sizeDelta = new Vector2(-40, 0);
                var sizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                ScrollWindow.addCallbackShow(OnWindowShow);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[TournamentHistoryWindow] 初始化失败: " + e.Message);
            }
        }
        public static void Open()
        {
            if (!_inited) Init();
            ScrollWindow.showWindow(WINDOW_ID);
        }
        public static void Toggle()
        {
            if (_window != null && _window.gameObject.activeSelf)
            {
                _window.clickHide();
            }
            else
            {
                Open();
            }
        }
        private static void OnWindowShow(string windowId)
        {
            if (windowId != WINDOW_ID) return;
            RefreshDisplay();
        }
        private static void RefreshDisplay()
        {
            if (_contentText == null) return;
            var histories = TournamentHistoryStorage.GetAllHistories();
            if (histories.Count == 0)
            {
                _contentText.text = "暂无比武大会历史记录";
                return;
            }
            var sb = new System.Text.StringBuilder();
            for (int i = histories.Count - 1; i >= 0; i--)
            {
                var history = histories[i];
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine($"<b><size=16>第{history.Edition}届比武大会（{history.Year}年-{history.EndYear}年）</size></b>");
                sb.AppendLine($"参赛人数：{history.ParticipantNames.Count}人");
                sb.AppendLine($"总轮次：{history.TotalRounds}轮");
                sb.AppendLine($"冠军：<color=#FFD700><b>{history.ChampionName}</b></color>");
                if (!string.IsNullOrEmpty(history.RunnerUpName))
                {
                    sb.AppendLine($"亚军：<color=#C0C0C0><b>{history.RunnerUpName}</b></color>");
                }
                if (!string.IsNullOrEmpty(history.ThirdPlaceName))
                {
                    sb.AppendLine($"季军：<color=#CD7F32><b>{history.ThirdPlaceName}</b></color>");
                }
                sb.AppendLine();
                if (!string.IsNullOrEmpty(history.Summary))
                {
                    sb.AppendLine(history.Summary);
                }
                sb.AppendLine();
            }
            _contentText.text = sb.ToString();
        }
    }
}