using UnityEngine;
using UnityEngine.UI;

namespace xn.ui
{
    internal sealed class ScrollableTextPanel
    {
        private const float PaddingX = 6f;
        private const float PaddingY = 6f;

        private readonly RectTransform _rootRect;
        private readonly RectTransform _contentRect;
        private readonly ScrollRect _scrollRect;

        public GameObject Root { get; }
        public Text Text { get; }

        private ScrollableTextPanel(GameObject root, RectTransform rootRect, RectTransform contentRect, ScrollRect scrollRect, Text text)
        {
            Root = root;
            _rootRect = rootRect;
            _contentRect = contentRect;
            _scrollRect = scrollRect;
            Text = text;
        }

        public static ScrollableTextPanel Create(Transform parent, string name, Vector3 localPosition, Vector2 size, int fontSize, Color color)
        {
            if (parent == null) return null;

            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;
            root.transform.localScale = Vector3.one;

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = size;

            var rootImage = root.GetComponent<Image>();
            rootImage.color = Color.clear;
            rootImage.raycastTarget = true;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(root.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.anchoredPosition = Vector2.zero;
            viewportRect.sizeDelta = Vector2.zero;

            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;

            var content = new GameObject("Content", typeof(RectTransform), typeof(Text));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.anchoredPosition = new Vector2(PaddingX, -PaddingY);
            contentRect.sizeDelta = new Vector2(-PaddingX * 2f, 0);

            var text = content.GetComponent<Text>();
            text.font = ResolveFont(parent);
            text.supportRichText = true;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1.1f;

            var scrollRect = root.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;

            root.SetActive(false);
            return new ScrollableTextPanel(root, rootRect, contentRect, scrollRect, text);
        }

        public void SetText(string value)
        {
            if (Text == null || Root == null) return;
            Text.text = value ?? string.Empty;
            Root.SetActive(true);
            ResizeContent();
            if (_scrollRect != null)
            {
                _scrollRect.StopMovement();
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void SetActive(bool active)
        {
            if (Root != null) Root.SetActive(active);
        }

        private void ResizeContent()
        {
            if (_contentRect == null || Text == null) return;

            Canvas.ForceUpdateCanvases();
            float width = Mathf.Max(10f, _contentRect.rect.width);
            var settings = Text.GetGenerationSettings(new Vector2(width, 0f));
            float preferredHeight = Text.cachedTextGeneratorForLayout.GetPreferredHeight(Text.text, settings) / Text.pixelsPerUnit;
            float minHeight = _rootRect != null ? Mathf.Max(0f, _rootRect.rect.height - PaddingY * 2f) : 0f;
            _contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(minHeight, preferredHeight + PaddingY * 2f));
            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
            Canvas.ForceUpdateCanvases();
        }

        private static Font ResolveFont(Transform parent)
        {
            try
            {
                if (LocalizedTextManager.current_font != null) return LocalizedTextManager.current_font;
            }
            catch
            {
            }

            var text = parent.GetComponentInChildren<Text>(true);
            return text != null ? text.font : null;
        }
    }
}
