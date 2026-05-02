using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeoModLoader.General;
namespace xn.ui.charts
{
    public static class RankingLegendButton
    {
        private static PowerButton _legendButton;
        private static Text _legendText;
        private static GameObject _legendTextObj;
        private const string LegendMarker = "---\u6218\u529b\u6392\u884c\u699c\u4f20\u5947---";
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        public static void Create(Transform background)
        {
            if (background == null) return;
            var icon = SpriteTextureLoader.getSprite("ui/icon/chartsui");
            _legendButton = PowerButtonCreator.CreateSimpleButton(
                "XN_RankingLegend",
                OnLeftClick,
                icon,
                background,
                new Vector2(-156, 50)
            );
            if (_legendButton != null)
            {
                var tipButton = _legendButton.GetComponent<TipButton>() ?? _legendButton.gameObject.AddComponent<TipButton>();
                tipButton.textOnClick = "ranking_legend_tip";
                tipButton.textOnClickDescription = "ranking_legend_desc";
                var trigger = _legendButton.gameObject.GetComponent<EventTrigger>() ?? _legendButton.gameObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener((data) =>
                {
                    var pointerData = (PointerEventData)data;
                    if (pointerData.button == PointerEventData.InputButton.Right)
                        OnRightClick();
                });
                trigger.triggers.Add(entry);
            }
            EnsureLegendText(background);
            if (RankingLegendStorage.HasLegend())
            {
                string content = RankingLegendStorage.Load();
                ShowLegendText(content);
            }
            else
            {
                HideLegendText();
            }
        }
        private static void EnsureLegendText(Transform bg)
        {
            _legendText = bg.Find("XN_RankingLegend_Text")?.GetComponent<Text>();
            if (_legendText == null)
            {
                _legendTextObj = new GameObject("XN_RankingLegend_Text", typeof(Text), typeof(ContentSizeFitter));
                _legendTextObj.transform.SetParent(bg);
                _legendTextObj.transform.localPosition = new Vector3(230, 30);
                _legendTextObj.transform.localScale = Vector3.one;
                var fitter = _legendTextObj.GetComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                _legendText = _legendTextObj.GetComponent<Text>();
                try { _legendText.font = LocalizedTextManager.current_font; }
                catch
                {
                    var any = bg.GetComponentInChildren<Text>();
                    if (any != null) _legendText.font = any.font;
                }
                _legendText.fontSize = 7;
                _legendText.alignment = TextAnchor.UpperLeft;
                _legendText.color = new Color(1f, 0.95f, 0.8f);
                _legendText.horizontalOverflow = HorizontalWrapMode.Wrap;
                _legendText.verticalOverflow = VerticalWrapMode.Truncate;
                var rectTransform = _legendTextObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(180, 350);
            }
            else
            {
                _legendTextObj = _legendText.gameObject;
            }
        }
        private static void ShowLegendText(string content)
        {
            if (_legendText != null)
            {
                string displayContent = content;
                int idx = content.IndexOf(LegendMarker);
                if (idx >= 0)
                    displayContent = content.Substring(idx + LegendMarker.Length).Trim();
                _legendText.text = displayContent;
                _legendTextObj?.SetActive(true);
            }
        }
        private static void HideLegendText()
        {
            if (_legendTextObj != null)
                _legendTextObj.SetActive(false);
        }
        private static void OnLeftClick()
        {
            if (RankingLegendStorage.HasLegend())
            {
                string content = RankingLegendStorage.Load();
                ShowLegendText(content);
                return;
            }
            if (!RankingLegendGenerator.CanGenerate())
            {
                ShowLegendText(T("ranking_legend_default_key_limit", "The default key can generate only once. Configure a custom API key."));
                return;
            }
            GenerateLegend();
        }
        private static void OnRightClick()
        {
            if (!RankingLegendGenerator.CanRegenerate())
            {
                ShowLegendText(T("ranking_legend_regenerate_requires_custom_key", "Regeneration requires a custom API key."));
                return;
            }
            string previousContent = RankingLegendStorage.Load();
            RankingLegendStorage.Delete();
            GenerateLegend(previousContent);
        }
        private static void GenerateLegend(string previousContent = null)
        {
            var top3 = new Actor[3];
            var scores = new long[3];
            foreach (var a in World.world.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (!a.asset.can_be_favorited) continue;
                long s = XNPowerRanking.CalcPowerScoreLongInternal(a);
                for (int i = 0; i < 3; i++)
                {
                    if (top3[i] == null || s > scores[i])
                    {
                        for (int j = 2; j > i; j--)
                        {
                            top3[j] = top3[j - 1];
                            scores[j] = scores[j - 1];
                        }
                        top3[i] = a;
                        scores[i] = s;
                        break;
                    }
                }
            }
            if (top3[0] == null)
            {
                ShowLegendText(T("ranking_legend_no_rankable_units", "No rankable units found"));
                return;
            }
            ShowLegendText(T("ranking_legend_generating_story", "Generating legend story..."));
            RankingLegendGenerator.GenerateLegend(top3, scores, (result) =>
            {
                ShowLegendText(result);
            }, previousContent);
        }
    }
}
