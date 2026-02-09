using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
using xn.feedback;
namespace xn.questions
{
    public static class QuestionsWindow
    {
        private static bool _inited;
        private static GameObject _popupWindow;
        private static Transform _listContent;
        private static Text _loadingText;
        private static readonly Dictionary<int, bool> _expandedState = new();
        private static readonly Dictionary<int, GameObject> _answerPanels = new();
        private static readonly Dictionary<int, Image> _arrowImages = new();
        private static Sprite _arrowRight;
        private static Sprite _arrowDown;
        private static Sprite _qaHeader;
        private static readonly Color COL_BG = new(0.02f, 0.02f, 0.05f, 0.92f);
        private static readonly Color COL_PANEL = new(0.10f, 0.11f, 0.16f, 0.98f);
        private static readonly Color COL_CARD = new(0.14f, 0.15f, 0.22f, 0.95f);
        private static readonly Color COL_CARD_HOVER = new(0.17f, 0.18f, 0.26f, 0.95f);
        private static readonly Color COL_TITLE = new(0.98f, 0.95f, 0.88f);
        private static readonly Color COL_QUESTION = new(0.92f, 0.88f, 0.72f);
        private static readonly Color COL_ANSWER = new(0.75f, 0.78f, 0.85f);
        private static readonly Color COL_TAG = new(0.22f, 0.52f, 0.38f, 0.9f);
        private static readonly Color COL_TAG_TEXT = new(0.7f, 1f, 0.8f);
        private static readonly Color COL_DIVIDER = new(0.45f, 0.52f, 0.68f, 0.35f);
        private static readonly Color COL_ACCENT = new(0.53f, 0.68f, 1f);
        private static readonly Color COL_SUBTLE = new(0.55f, 0.55f, 0.62f);
        private const float PANEL_W = 480f;
        private const float PANEL_H = 420f;
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            QuestionsLoader.LoadOnce();
        }
        public static void Show()
        {
            if (_popupWindow != null) return;
            CreatePopupWindow();
        }
        public static void Toggle()
        {
            if (_popupWindow != null) ClosePopup();
            else Show();
        }
        private static Sprite GetSprite(string path, string fallback = null)
        {
            var s = SpriteTextureLoader.getSprite(path);
            if (s == null && fallback != null)
                s = SpriteTextureLoader.getSprite(fallback);
            return s;
        }
        private static void CreatePopupWindow()
        {
            _expandedState.Clear();
            _answerPanels.Clear();
            _arrowImages.Clear();
            _arrowRight = GetSprite("ui/questionui/arrow_right");
            _arrowDown = GetSprite("ui/questionui/arrow_down");
            _qaHeader = GetSprite("ui/questionui/qa_header");
            var canvas = CanvasMain.instance.canvas_ui;
            _popupWindow = new GameObject("XN_QuestionsPopup");
            _popupWindow.transform.SetParent(canvas.transform, false);
            var popupCanvas = _popupWindow.AddComponent<Canvas>();
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 9999;
            _popupWindow.AddComponent<GraphicRaycaster>();
            var bgObj = CreateUI("Background", _popupWindow.transform);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = COL_BG;
            var bgBtn = bgObj.AddComponent<Button>();
            bgBtn.onClick.AddListener(ClosePopup);
            var panel = CreateUI("MainPanel", _popupWindow.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchoredPosition = new Vector2(-90, 0);
            panelRect.sizeDelta = new Vector2(PANEL_W, PANEL_H);
            var panelBg = panel.AddComponent<Image>();
            panelBg.sprite = SpriteTextureLoader.getSprite("ui/special/window");
            panelBg.type = Image.Type.Sliced;
            panelBg.color = COL_PANEL;
            CreateHeader(panel.transform);
            CreateScrollArea(panel.transform);
            CreateCloseX(panel.transform);
            float spW = 160f;
            var spPos = new Vector2(PANEL_W / 2 - 90 + spW / 2 + 10, 0);
            SponsorPanelBuilder.Create(_popupWindow.transform, spPos, spW, PANEL_H);
            MusicBox.playSoundUI("event:/SFX/UI/WindowWhoosh");
        }
        private static void CreateHeader(Transform parent)
        {
            float y = PANEL_H / 2 - 18f;
            if (_qaHeader != null)
            {
                var iconObj = CreateUI("HeaderIcon", parent);
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(-70f, y);
                iconRect.sizeDelta = new Vector2(22, 22);
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = _qaHeader;
                iconImg.preserveAspect = true;
            }
            MakeText("Title", parent, new Vector2(0, y), new Vector2(PANEL_W - 40, 24),
                "常见问题", 16, TextAnchor.MiddleCenter, COL_TITLE);
            y -= 22f;
            MakeText("Subtitle", parent, new Vector2(0, y), new Vector2(PANEL_W - 40, 16),
                "点击问题查看解答", 10, TextAnchor.MiddleCenter, COL_SUBTLE);
            y -= 16f;
            var div = CreateUI("Divider", parent);
            var divRect = div.GetComponent<RectTransform>();
            divRect.anchoredPosition = new Vector2(0, y);
            divRect.sizeDelta = new Vector2(PANEL_W - 40, 1);
            div.AddComponent<Image>().color = COL_DIVIDER;
        }
        private static void CreateScrollArea(Transform parent)
        {
            float topOffset = 62f;
            float bottomPad = 12f;
            float areaH = PANEL_H - topOffset - bottomPad;
            var scrollObj = CreateUI("Scroll", parent);
            var scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.anchoredPosition = new Vector2(0, -topOffset / 2 + bottomPad / 2);
            scrollRect.sizeDelta = new Vector2(PANEL_W - 24, areaH);
            var viewport = CreateUI("Viewport", scrollObj.transform);
            var vpRect = viewport.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.sizeDelta = Vector2.zero;
            vpRect.anchoredPosition = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            var content = CreateUI("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 6f;
            layout.padding = new RectOffset(4, 4, 6, 6);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _listContent = content.transform;
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.viewport = vpRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.scrollSensitivity = 20f;
            var loadObj = CreateUI("Loading", content.transform);
            loadObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
            _loadingText = loadObj.AddComponent<Text>();
            _loadingText.font = LocalizedTextManager.current_font;
            _loadingText.fontSize = 11;
            _loadingText.alignment = TextAnchor.MiddleCenter;
            _loadingText.color = COL_SUBTLE;
            _loadingText.text = "加载中...";
            LoadQuestions();
        }
        private static void LoadQuestions()
        {
            if (QuestionsLoader.IsLoaded)
                OnQuestionsLoaded(QuestionsLoader.GetCached());
            else
                QuestionsLoader.GetQuestions(OnQuestionsLoaded);
        }
        private static void OnQuestionsLoaded(List<QuestionItem> items)
        {
            if (_listContent == null) return;
            if (_loadingText != null)
            {
                Object.Destroy(_loadingText.gameObject);
                _loadingText = null;
            }
            if (items == null || items.Count == 0)
            {
                var emptyObj = CreateUI("Empty", _listContent);
                emptyObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
                var emptyTxt = emptyObj.AddComponent<Text>();
                emptyTxt.font = LocalizedTextManager.current_font;
                emptyTxt.fontSize = 12;
                emptyTxt.alignment = TextAnchor.MiddleCenter;
                emptyTxt.color = COL_SUBTLE;
                emptyTxt.text = "暂无问题\n请稍后再试";
                return;
            }
            for (int i = 0; i < items.Count; i++)
                CreateQuestionCard(_listContent, i, items[i]);
        }
        private static void CreateQuestionCard(Transform parent, int index, QuestionItem item)
        {
            var card = CreateUI($"Card_{index}", parent);
            var cardRect = card.GetComponent<RectTransform>();
            var cardLayout = card.AddComponent<VerticalLayoutGroup>();
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.spacing = 0f;
            cardLayout.padding = new RectOffset(0, 0, 0, 0);
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            var cardFitter = card.AddComponent<ContentSizeFitter>();
            cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var qRow = CreateUI("QuestionRow", card.transform);
            var qRowRect = qRow.GetComponent<RectTransform>();
            qRowRect.sizeDelta = new Vector2(0, 36);
            var qRowBg = qRow.AddComponent<Image>();
            qRowBg.sprite = SpriteTextureLoader.getSprite("ui/special/window");
            qRowBg.type = Image.Type.Sliced;
            qRowBg.color = COL_CARD;
            var qHlg = qRow.AddComponent<HorizontalLayoutGroup>();
            qHlg.childAlignment = TextAnchor.MiddleLeft;
            qHlg.spacing = 6f;
            qHlg.padding = new RectOffset(8, 8, 4, 4);
            qHlg.childControlWidth = false;
            qHlg.childControlHeight = false;
            qHlg.childForceExpandWidth = false;
            qHlg.childForceExpandHeight = false;
            var arrowObj = CreateUI("Arrow", qRow.transform);
            arrowObj.GetComponent<RectTransform>().sizeDelta = new Vector2(14, 14);
            var arrowImg = arrowObj.AddComponent<Image>();
            arrowImg.sprite = _arrowRight;
            arrowImg.preserveAspect = true;
            arrowImg.color = COL_SUBTLE;
            _arrowImages[index] = arrowImg;
            var numObj = CreateUI("Num", qRow.transform);
            numObj.GetComponent<RectTransform>().sizeDelta = new Vector2(22, 28);
            var numTxt = numObj.AddComponent<Text>();
            numTxt.font = LocalizedTextManager.current_font;
            numTxt.fontSize = 11;
            numTxt.alignment = TextAnchor.MiddleCenter;
            numTxt.color = COL_ACCENT;
            numTxt.text = $"{index + 1}";
            if (!string.IsNullOrEmpty(item.tag))
            {
                var tagObj = CreateUI("Tag", qRow.transform);
                tagObj.GetComponent<RectTransform>().sizeDelta = new Vector2(42, 20);
                var tagBg = tagObj.AddComponent<Image>();
                tagBg.sprite = SpriteTextureLoader.getSprite("ui/special/button");
                tagBg.type = Image.Type.Sliced;
                tagBg.color = COL_TAG;
                var tagTxtObj = CreateUI("TagText", tagObj.transform);
                var tagTxtRect = tagTxtObj.GetComponent<RectTransform>();
                tagTxtRect.anchorMin = Vector2.zero;
                tagTxtRect.anchorMax = Vector2.one;
                tagTxtRect.sizeDelta = Vector2.zero;
                var tagTxt = tagTxtObj.AddComponent<Text>();
                tagTxt.font = LocalizedTextManager.current_font;
                tagTxt.fontSize = 9;
                tagTxt.alignment = TextAnchor.MiddleCenter;
                tagTxt.color = COL_TAG_TEXT;
                tagTxt.text = item.tag;
            }
            float qTextW = PANEL_W - 24 - 8 - 14 - 6 - 22 - 6 - 42 - 6 - 8;
            var qTextObj = CreateUI("QText", qRow.transform);
            qTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(qTextW, 28);
            var qTxt = qTextObj.AddComponent<Text>();
            qTxt.font = LocalizedTextManager.current_font;
            qTxt.fontSize = 12;
            qTxt.alignment = TextAnchor.MiddleLeft;
            qTxt.color = COL_QUESTION;
            qTxt.supportRichText = true;
            qTxt.text = item.q;
            var qBtn = qRow.AddComponent<Button>();
            qBtn.targetGraphic = qRowBg;
            int idx = index;
            qBtn.onClick.AddListener(() => OnToggleAnswer(idx));
            var colors = qBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            qBtn.colors = colors;
            CreateAnswerPanel(card.transform, index, item.a);
            _expandedState[index] = false;
        }
        private static void CreateAnswerPanel(Transform parent, int index, string answer)
        {
            var aPanel = CreateUI($"Answer_{index}", parent);
            var aPanelRect = aPanel.GetComponent<RectTransform>();
            var aBg = aPanel.AddComponent<Image>();
            aBg.sprite = SpriteTextureLoader.getSprite("ui/special/window");
            aBg.type = Image.Type.Sliced;
            aBg.color = new Color(0.08f, 0.10f, 0.15f, 0.95f);
            var aLayout = aPanel.AddComponent<VerticalLayoutGroup>();
            aLayout.childAlignment = TextAnchor.UpperLeft;
            aLayout.spacing = 0f;
            aLayout.padding = new RectOffset(14, 14, 8, 10);
            aLayout.childControlWidth = true;
            aLayout.childControlHeight = true;
            aLayout.childForceExpandWidth = true;
            aLayout.childForceExpandHeight = false;
            var panelFitter = aPanel.AddComponent<ContentSizeFitter>();
            panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var ansRow = CreateUI("AnsRow", aPanel.transform);
            var ansRowHlg = ansRow.AddComponent<HorizontalLayoutGroup>();
            ansRowHlg.childAlignment = TextAnchor.UpperLeft;
            ansRowHlg.spacing = 8f;
            ansRowHlg.padding = new RectOffset(0, 0, 0, 0);
            ansRowHlg.childControlWidth = false;
            ansRowHlg.childControlHeight = true;
            ansRowHlg.childForceExpandWidth = false;
            ansRowHlg.childForceExpandHeight = true;
            var bar = CreateUI("Bar", ansRow.transform);
            bar.GetComponent<RectTransform>().sizeDelta = new Vector2(3, 0);
            var barImg = bar.AddComponent<Image>();
            barImg.color = COL_ACCENT;
            float ansW = PANEL_W - 24 - 14 - 14 - 3 - 8;
            var ansTxtObj = CreateUI("AnsText", ansRow.transform);
            var ansTxtRect = ansTxtObj.GetComponent<RectTransform>();
            ansTxtRect.sizeDelta = new Vector2(ansW, 0);
            var ansTxt = ansTxtObj.AddComponent<Text>();
            ansTxt.font = LocalizedTextManager.current_font;
            ansTxt.fontSize = 11;
            ansTxt.alignment = TextAnchor.UpperLeft;
            ansTxt.color = COL_ANSWER;
            ansTxt.supportRichText = true;
            ansTxt.text = answer;
            ansTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            ansTxt.verticalOverflow = VerticalWrapMode.Overflow;
            var txtFitter = ansTxtObj.AddComponent<ContentSizeFitter>();
            txtFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            aPanel.SetActive(false);
            _answerPanels[index] = aPanel;
        }
        private static void OnToggleAnswer(int index)
        {
            bool expanded = _expandedState.ContainsKey(index) && _expandedState[index];
            expanded = !expanded;
            _expandedState[index] = expanded;
            if (_answerPanels.TryGetValue(index, out var panel))
            {
                panel.SetActive(expanded);
                var cardRect = panel.transform.parent?.GetComponent<RectTransform>();
                if (cardRect != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(cardRect);
                if (_listContent != null)
                {
                    var contentRect = _listContent.GetComponent<RectTransform>();
                    if (contentRect != null)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                }
            }
            if (_arrowImages.TryGetValue(index, out var arrow))
            {
                arrow.sprite = expanded ? _arrowDown : _arrowRight;
                arrow.color = expanded ? COL_ACCENT : COL_SUBTLE;
            }
            MusicBox.playSoundUI("event:/SFX/UI/ButtonClick");
        }
        private static void CreateCloseX(Transform parent)
        {
            var closeX = CreateUI("CloseX", parent);
            var rect = closeX.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(PANEL_W / 2 - 18, PANEL_H / 2 - 18);
            rect.sizeDelta = new Vector2(24, 24);
            var img = closeX.AddComponent<Image>();
            img.sprite = GetSprite("ui/icons/iconClose", "ui/icons/iconCancel");
            img.color = new Color(0.85f, 0.85f, 0.85f);
            var btn = closeX.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(ClosePopup);
        }
        private static void ClosePopup()
        {
            if (_popupWindow != null)
            {
                MusicBox.playSoundUI("event:/SFX/UI/WindowClose");
                Object.Destroy(_popupWindow);
                _popupWindow = null;
                _listContent = null;
                _loadingText = null;
                _expandedState.Clear();
                _answerPanels.Clear();
                _arrowImages.Clear();
            }
        }
        private static GameObject CreateUI(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }
        private static void MakeText(string name, Transform parent, Vector2 pos,
            Vector2 size, string text, int fontSize, TextAnchor align, Color color)
        {
            var obj = CreateUI(name, parent);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var txt = obj.AddComponent<Text>();
            txt.text = text;
            txt.font = LocalizedTextManager.current_font;
            txt.fontSize = fontSize;
            txt.alignment = align;
            txt.color = color;
        }
    }
}