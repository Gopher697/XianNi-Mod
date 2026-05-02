using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
namespace xn.feedback
{
    public static class FeedbackWindow
    {
        private static bool _inited;
        private static GameObject _popupWindow;
        private static int _selectedRating = 5;
        private static InputField _contentInput;
        private static Text _ratingText;
        private static Button[] _starButtons;
        private static Image[] _starImages;
        private static Button _submitButton;
        private static Text _statusText;
        private static Sprite _starFilledSprite;
        private static Sprite _starEmptySprite;
        private const string FEEDBACK_KEY = "XN_HasFeedback";
        private const string FEEDBACK_VERSION_KEY = "XN_FeedbackVersion";
        private static string CurrentVersion => xn.version.OnlineVersionChecker.CurrentVersion;
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
            SponsorLoader.LoadOnce();
        }
        public static bool HasFeedback()
        {
            return PlayerPrefs.GetInt(FEEDBACK_KEY, 0) == 1;
        }
        private static void MarkAsFeedback()
        {
            PlayerPrefs.SetInt(FEEDBACK_KEY, 1);
            PlayerPrefs.SetString(FEEDBACK_VERSION_KEY, CurrentVersion);
            PlayerPrefs.Save();
        }
        public static void Show()
        {
            if (_popupWindow != null) return;
            CreatePopupWindow();
        }
        public static void Toggle()
        {
            if (_popupWindow != null)
                ClosePopup();
            else
                Show();
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
            _selectedRating = 5;
            _starFilledSprite = GetSprite("ui/icon/star_filled", "ui/Icons/iconFavoriteStar");
            _starEmptySprite = GetSprite("ui/icon/star_empty", "ui/Icons/iconFavoriteStar");
            var canvas = CanvasMain.instance.canvas_ui;
            _popupWindow = new GameObject("XN_FeedbackPopup");
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
            bgImage.color = new Color(0.02f, 0.02f, 0.05f, 0.92f);
            var bgBtn = bgObj.AddComponent<Button>();
            bgBtn.onClick.AddListener(ClosePopup);
            var container = CreateUIElement("Container", _popupWindow.transform);
            var containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(780, 380);
            CreateLeftPanel(container.transform);
            CreateCenterPanel(container.transform);
            CreateRightPanel(container.transform);
            MusicBox.playSoundUI("event:/SFX/UI/WindowWhoosh");
        }
        private static void CreateLeftPanel(Transform parent)
        {
            float panelWidth = 160f;
            float panelHeight = 380f;
            var panel = CreateUIElement("LeftPanel", parent);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchoredPosition = new Vector2(-310, 0);
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
            var bg = panel.AddComponent<Image>();
            bg.sprite = SpriteTextureLoader.getSprite("ui/special/window");
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);
            float y = panelHeight / 2 - 15f;
            CreateText("Title", panel.transform, new Vector2(0, y), new Vector2(140, 22),
                T("feedback_support_author", "Support the Author"), 13, TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.55f));
            y -= 25f;
            CreateGradientDivider(panel.transform, y, 130);
            y -= 15f;
            var qrObj = CreateUIElement("QRImage", panel.transform);
            var qrRect = qrObj.GetComponent<RectTransform>();
            qrRect.anchoredPosition = new Vector2(0, y - 55f);
            qrRect.sizeDelta = new Vector2(110, 110);
            var qrImg = qrObj.AddComponent<Image>();
            qrImg.sprite = SpriteTextureLoader.getSprite("ui/sponsor/kangwechat");
            qrImg.preserveAspect = true;
            if (qrImg.sprite == null)
            {
                qrImg.color = new Color(0.2f, 0.22f, 0.28f);
                CreateText("QRPlaceholder", qrObj.transform, Vector2.zero, new Vector2(100, 40),
                    T("feedback_sponsor_code", "Sponsor Code"), 11, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.55f));
            }
            y -= 125f;
            CreateText("WechatLabel", panel.transform, new Vector2(0, y), new Vector2(140, 18),
                T("feedback_wechat_sponsor", "Scan with WeChat to sponsor"), 10, TextAnchor.MiddleCenter, new Color(0.65f, 0.65f, 0.72f));
            y -= 25f;
            CreateGradientDivider(panel.transform, y, 130);
            y -= 15f;
            CreateText("Thanks", panel.transform, new Vector2(0, y - 30f), new Vector2(130, 70),
                T("feedback_support_thanks", "Your support\nkeeps updates\nmoving forward"), 10, TextAnchor.UpperCenter, new Color(0.55f, 0.55f, 0.62f));
        }
        private static void CreateCenterPanel(Transform parent)
        {
            float panelWidth = 420f;
            float panelHeight = 380f;
            var panel = CreateUIElement("CenterPanel", parent);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchoredPosition = new Vector2(0, 0);
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
            var bg = panel.AddComponent<Image>();
            bg.sprite = SpriteTextureLoader.getSprite("ui/special/window");
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);
            float y = panelHeight / 2 - 15f;
            CreateText("CenterTitle_Text", panel.transform, new Vector2(0, y), new Vector2(380, 24),
                T("feedback_about_title", "About Xian Ni Mod"), 16, TextAnchor.MiddleCenter, new Color(0.98f, 0.95f, 0.88f));
            var commentSprite = GetSprite("ui/icon/comment", "ui/icons/iconAbout");
            if (commentSprite != null)
            {
                var iconObj = CreateUIElement("CenterTitle_Icon", panel.transform);
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(-72f, y);
                iconRect.sizeDelta = new Vector2(20, 20);
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = commentSprite;
                iconImg.preserveAspect = true;
                iconImg.color = Color.white;
            }
            y -= 25f;
            CreateText("Version", panel.transform, new Vector2(0, y), new Vector2(380, 16),
                T("feedback_current_version", "Current Version: v{0}", CurrentVersion), 10, TextAnchor.MiddleCenter, new Color(0.55f, 0.55f, 0.62f));
            y -= 20f;
            CreateGradientDivider(panel.transform, y, 360);
            y -= 15f;
            CreateText("RatingLabel", panel.transform, new Vector2(0, y), new Vector2(380, 18),
                T("feedback_rating_label", "Please rate the mod"), 12, TextAnchor.MiddleCenter, new Color(0.88f, 0.88f, 0.9f));
            y -= 30f;
            CreateStarRating(panel.transform, new Vector2(0, y));
            y -= 28f;
            var ratingObj = CreateUIElement("RatingText", panel.transform);
            var ratingRect = ratingObj.GetComponent<RectTransform>();
            ratingRect.anchoredPosition = new Vector2(0, y);
            ratingRect.sizeDelta = new Vector2(380, 18);
            _ratingText = ratingObj.AddComponent<Text>();
            _ratingText.font = LocalizedTextManager.current_font;
            _ratingText.fontSize = 11;
            _ratingText.alignment = TextAnchor.MiddleCenter;
            _ratingText.supportRichText = true;
            UpdateRatingText();
            y -= 20f;
            CreateGradientDivider(panel.transform, y, 360);
            y -= 12f;
            CreateText("ContentLabel", panel.transform, new Vector2(-130, y), new Vector2(100, 16),
                T("feedback_message_label", "Leave a message"), 11, TextAnchor.MiddleLeft, new Color(0.82f, 0.82f, 0.85f));
            CreateText("Optional", panel.transform, new Vector2(20, y), new Vector2(80, 16),
                T("feedback_suggestion_label", "Feedback"), 9, TextAnchor.MiddleLeft, new Color(0.45f, 0.45f, 0.52f));
            y -= 35f;
            CreateInputField(panel.transform, new Vector2(0, y));
            y -= 45f;
            var statusObj = CreateUIElement("Status", panel.transform);
            var statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(0, y);
            statusRect.sizeDelta = new Vector2(380, 16);
            _statusText = statusObj.AddComponent<Text>();
            _statusText.font = LocalizedTextManager.current_font;
            _statusText.fontSize = 9;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color = new Color(0.5f, 0.5f, 0.58f);
            _statusText.text = HasFeedback()
                ? T("feedback_status_already_rated", "You have already rated this. Submitting again will update it.")
                : T("feedback_status_default", "Your feedback helps us keep improving");
            y -= 22f;
            CreateButtons(panel.transform, y);
            y -= 38f;
            CreateText("Warning", panel.transform, new Vector2(0, y), new Vector2(380, 14),
                T("feedback_rate_limit_warning", "Note: one submission per day. Please do not submit repeatedly."), 9, TextAnchor.MiddleCenter, new Color(0.9f, 0.4f, 0.4f));
            CreateCloseX(panel.transform, panelWidth, panelHeight);
        }
        private static void CreateRightPanel(Transform parent)
        {
            SponsorPanelBuilder.Create(parent, new Vector2(310, 0), 160f, 380f);
        }
        private static void CreateStarRating(Transform parent, Vector2 pos)
        {
            var container = CreateUIElement("Stars", parent);
            var containerRect = container.GetComponent<RectTransform>();
            containerRect.anchoredPosition = pos;
            containerRect.sizeDelta = new Vector2(300, 40);
            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 14f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            _starButtons = new Button[5];
            _starImages = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                int rating = i + 1;
                var star = CreateUIElement($"Star{rating}", container.transform);
                var starRect = star.GetComponent<RectTransform>();
                starRect.sizeDelta = new Vector2(32, 32);
                var starImg = star.AddComponent<Image>();
                starImg.sprite = rating <= _selectedRating ? _starFilledSprite : _starEmptySprite;
                starImg.preserveAspect = true;
                starImg.color = Color.white;
                _starImages[i] = starImg;
                var starBtn = star.AddComponent<Button>();
                starBtn.targetGraphic = starImg;
                int r = rating;
                starBtn.onClick.AddListener(() => OnStarClick(r));
                _starButtons[i] = starBtn;
            }
        }
        private static void OnStarClick(int rating)
        {
            _selectedRating = rating;
            UpdateStarDisplay();
            UpdateRatingText();
            MusicBox.playSoundUI("event:/SFX/UI/ButtonClick");
            if (MapBox.instance != null && rating >= 1 && rating <= 5)
            {
                var starTransform = _starImages[rating - 1]?.transform;
                if (starTransform != null)
                    ((MonoBehaviour)MapBox.instance).StartCoroutine(StarBounceAnim(starTransform));
            }
        }
        private static IEnumerator StarBounceAnim(Transform target)
        {
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 original = Vector3.one;
            Vector3 peak = new Vector3(1.2f, 1.2f, 1f);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                if (target != null)
                    target.localScale = Vector3.Lerp(original, peak, t);
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                if (target != null)
                    target.localScale = Vector3.Lerp(peak, original, t);
                yield return null;
            }
            if (target != null)
                target.localScale = original;
        }
        private static void UpdateStarDisplay()
        {
            for (int i = 0; i < 5; i++)
            {
                _starImages[i].sprite = (i + 1) <= _selectedRating ? _starFilledSprite : _starEmptySprite;
                _starImages[i].color = Color.white;
            }
        }
        private static void UpdateRatingText()
        {
            string[] texts =
            {
                T("feedback_rating_1", "Very Poor"),
                T("feedback_rating_2", "Poor"),
                T("feedback_rating_3", "Average"),
                T("feedback_rating_4", "Good"),
                T("feedback_rating_5", "Excellent!")
            };
            string[] colors = { "#E85555", "#F09050", "#E8C050", "#88C855", "#50D880" };
            string rating = T("feedback_rating_format", "{0} Star - {1}", _selectedRating, texts[_selectedRating - 1]);
            _ratingText.text = $"<color={colors[_selectedRating - 1]}>{rating}</color>";
        }
        private static void CreateInputField(Transform parent, Vector2 pos)
        {
            var inputObj = CreateUIElement("InputField", parent);
            var inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchoredPosition = pos;
            inputRect.sizeDelta = new Vector2(370, 55);
            var inputBg = inputObj.AddComponent<Image>();
            inputBg.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
            inputBg.type = Image.Type.Sliced;
            inputBg.color = new Color(0.06f, 0.07f, 0.1f, 1f);
            var textArea = CreateUIElement("TextArea", inputObj.transform);
            var textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.sizeDelta = new Vector2(-12, -8);
            textAreaRect.anchoredPosition = Vector2.zero;
            var textObj = CreateUIElement("Text", textArea.transform);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var text = textObj.AddComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 11;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.supportRichText = false;
            var placeholderObj = CreateUIElement("Placeholder", textArea.transform);
            var placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;
            var placeholder = placeholderObj.AddComponent<Text>();
            placeholder.font = LocalizedTextManager.current_font;
            placeholder.fontSize = 11;
            placeholder.color = new Color(0.4f, 0.4f, 0.45f);
            placeholder.alignment = TextAnchor.UpperLeft;
            placeholder.text = T("feedback_input_placeholder", "Enter what you want to say to the author...");
            placeholder.fontStyle = FontStyle.Italic;
            _contentInput = inputObj.AddComponent<InputField>();
            _contentInput.textComponent = text;
            _contentInput.placeholder = placeholder;
            _contentInput.lineType = InputField.LineType.MultiLineNewline;
            _contentInput.characterLimit = 500;
        }
        private static void CreateButtons(Transform parent, float y)
        {
            var submitObj = CreateUIElement("Submit", parent);
            var submitRect = submitObj.GetComponent<RectTransform>();
            submitRect.anchoredPosition = new Vector2(-75, y);
            submitRect.sizeDelta = new Vector2(120, 30);
            var submitBg = submitObj.AddComponent<Image>();
            submitBg.sprite = SpriteTextureLoader.getSprite("ui/special/button");
            submitBg.type = Image.Type.Sliced;
            submitBg.color = new Color(0.22f, 0.52f, 0.38f, 1f);
            _submitButton = submitObj.AddComponent<Button>();
            _submitButton.targetGraphic = submitBg;
            _submitButton.onClick.AddListener(OnSubmit);
            var submitColors = _submitButton.colors;
            submitColors.normalColor = Color.white;
            submitColors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            submitColors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            _submitButton.colors = submitColors;
            var submitText = CreateUIElement("Text", submitObj.transform);
            var submitTextRect = submitText.GetComponent<RectTransform>();
            submitTextRect.anchorMin = Vector2.zero;
            submitTextRect.anchorMax = Vector2.one;
            submitTextRect.sizeDelta = Vector2.zero;
            var submitTxt = submitText.AddComponent<Text>();
            submitTxt.text = T("feedback_submit_button", "Submit Review");
            submitTxt.font = LocalizedTextManager.current_font;
            submitTxt.fontSize = 13;
            submitTxt.alignment = TextAnchor.MiddleCenter;
            submitTxt.color = Color.white;
            var cancelObj = CreateUIElement("Cancel", parent);
            var cancelRect = cancelObj.GetComponent<RectTransform>();
            cancelRect.anchoredPosition = new Vector2(75, y);
            cancelRect.sizeDelta = new Vector2(120, 30);
            var cancelBg = cancelObj.AddComponent<Image>();
            cancelBg.sprite = SpriteTextureLoader.getSprite("ui/special/button");
            cancelBg.type = Image.Type.Sliced;
            cancelBg.color = new Color(0.35f, 0.32f, 0.38f, 1f);
            var cancelBtn = cancelObj.AddComponent<Button>();
            cancelBtn.targetGraphic = cancelBg;
            cancelBtn.onClick.AddListener(ClosePopup);
            var cancelColors = cancelBtn.colors;
            cancelColors.normalColor = Color.white;
            cancelColors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            cancelColors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            cancelBtn.colors = cancelColors;
            var cancelText = CreateUIElement("Text", cancelObj.transform);
            var cancelTextRect = cancelText.GetComponent<RectTransform>();
            cancelTextRect.anchorMin = Vector2.zero;
            cancelTextRect.anchorMax = Vector2.one;
            cancelTextRect.sizeDelta = Vector2.zero;
            var cancelTxt = cancelText.AddComponent<Text>();
            cancelTxt.text = T("feedback_close_button", "Close");
            cancelTxt.font = LocalizedTextManager.current_font;
            cancelTxt.fontSize = 13;
            cancelTxt.alignment = TextAnchor.MiddleCenter;
            cancelTxt.color = Color.white;
        }
        private static void OnSubmit()
        {
            string content = _contentInput != null ? _contentInput.text : "";
            _statusText.text = T("feedback_submitting", "Submitting...");
            _statusText.color = new Color(0.9f, 0.85f, 0.3f);
            _submitButton.interactable = false;
            if (MapBox.instance != null)
            {
                ((MonoBehaviour)MapBox.instance).StartCoroutine(
                    FeedbackSender.SendFeedback(_selectedRating, content, CurrentVersion, OnSubmitResult));
            }
        }
        private static void OnSubmitResult(bool success, string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.color = success ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.9f, 0.4f, 0.4f);
            }
            if (_submitButton != null)
                _submitButton.interactable = true;
            if (success)
            {
                MarkAsFeedback();
                MusicBox.playSoundUI("event:/SFX/UI/ButtonClick");
                if (MapBox.instance != null)
                    ((MonoBehaviour)MapBox.instance).StartCoroutine(DelayedClose());
            }
        }
        private static IEnumerator DelayedClose()
        {
            yield return new WaitForSeconds(1.5f);
            ClosePopup();
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
        private static void CreateGradientDivider(Transform parent, float y, float width)
        {
            var div = CreateUIElement("GradientDivider", parent);
            var rect = div.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, y);
            rect.sizeDelta = new Vector2(width, 1);
            var img = div.AddComponent<Image>();
            img.color = new Color(0.45f, 0.52f, 0.68f, 0.35f);
        }
        private static void CreateCloseX(Transform parent, float w, float h)
        {
            var closeX = CreateUIElement("CloseX", parent);
            var rect = closeX.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(w / 2 - 18, h / 2 - 18);
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
                _contentInput = null;
                _ratingText = null;
                _starButtons = null;
                _starImages = null;
                _submitButton = null;
                _statusText = null;
            }
        }
    }
}
