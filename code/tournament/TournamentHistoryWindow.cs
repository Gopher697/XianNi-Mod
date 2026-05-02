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
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            try
            {
                _window = WindowCreator.CreateEmptyWindow(WINDOW_ID, T("tournament_history_window_title", "Battle History"));
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
                Debug.LogError("[TournamentHistoryWindow] Init failed: " + e.Message);
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
                _contentText.text = T("tournament_history_empty", "No Grand Martial Arts Tournament history available");
                return;
            }
            var sb = new System.Text.StringBuilder();
            for (int i = histories.Count - 1; i >= 0; i--)
            {
                var history = histories[i];
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine(T("tournament_history_header_format", "<b><size=16>Grand Martial Arts Tournament {0} ({1}–{2})</size></b>", history.Edition, history.Year, history.EndYear));
                sb.AppendLine(T("tournament_history_participants_format", "Number of participants: {0}", history.ParticipantNames.Count));
                sb.AppendLine(T("tournament_history_rounds_format", "Total rounds: {0}", history.TotalRounds));
                sb.AppendLine(T("tournament_history_champion_format", "Champion: <color=#FFD700><b>{0}</b></color>", history.ChampionName));
                if (!string.IsNullOrEmpty(history.RunnerUpName))
                {
                    sb.AppendLine(T("tournament_history_runner_up_format", "Runner-up: <color=#C0C0C0><b>{0}</b></color>", history.RunnerUpName));
                }
                if (!string.IsNullOrEmpty(history.ThirdPlaceName))
                {
                    sb.AppendLine(T("tournament_history_third_place_format", "Third Place: <color=#CD7F32><b>{0}</b></color>", history.ThirdPlaceName));
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
