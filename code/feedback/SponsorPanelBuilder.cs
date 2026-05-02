using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
namespace xn.feedback
{
    public static class SponsorPanelBuilder
    {
        private static readonly Color COL_PANEL = new(0.12f, 0.13f, 0.18f, 0.98f);
        private static readonly Color COL_DIVIDER = new(0.45f, 0.52f, 0.68f, 0.35f);
        private static readonly Color COL_SUBTLE = new(0.55f, 0.55f, 0.62f);
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
        public static void Create(Transform parent, Vector2 position, float width, float height)
        {
            var panel = MakeGO("SponsorPanel", parent);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchoredPosition = position;
            panelRect.sizeDelta = new Vector2(width, height);
            var bg = panel.AddComponent<Image>();
            bg.sprite = SpriteTextureLoader.getSprite("ui/special/window");
            bg.type = Image.Type.Sliced;
            bg.color = COL_PANEL;
            float y = height / 2 - 15f;
            var trophy = SpriteTextureLoader.getSprite("ui/icon/trophy");
            if (trophy != null)
            {
                var iconObj = MakeGO("TrophyIcon", panel.transform);
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(-32f, y);
                iconRect.sizeDelta = new Vector2(18, 18);
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = trophy;
                iconImg.preserveAspect = true;
            }
            MakeText("Title", panel.transform, new Vector2(0, y),
                new Vector2(width - 20, 22), T("sponsor_title", "Sponsors"), 13,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f));
            y -= 25f;
            var div = MakeGO("Divider", panel.transform);
            div.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y);
            div.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 30, 1);
            div.AddComponent<Image>().color = COL_DIVIDER;
            y -= 10f;
            float listH = height - 70f;
            var scrollObj = MakeGO("SponsorScroll", panel.transform);
            var sRect = scrollObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0, y - listH / 2);
            sRect.sizeDelta = new Vector2(width - 12, listH);
            var viewport = MakeGO("Viewport", scrollObj.transform);
            var vpRect = viewport.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.sizeDelta = Vector2.zero;
            vpRect.anchoredPosition = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            viewport.AddComponent<Image>().color = Color.clear;
            var content = MakeGO("Content", viewport.transform);
            var cRect = content.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 1);
            cRect.anchorMax = new Vector2(1, 1);
            cRect.pivot = new Vector2(0.5f, 1);
            cRect.anchoredPosition = Vector2.zero;
            cRect.sizeDelta = Vector2.zero;
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 3f;
            layout.padding = new RectOffset(2, 2, 4, 4);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.viewport = vpRect;
            scroll.content = cRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 15f;
            var loadObj = MakeGO("Loading", content.transform);
            loadObj.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 20, 30);
            var loadingText = loadObj.AddComponent<Text>();
            loadingText.font = LocalizedTextManager.current_font;
            loadingText.fontSize = 10;
            loadingText.alignment = TextAnchor.MiddleCenter;
            loadingText.color = COL_SUBTLE;
            loadingText.text = T("sponsor_loading", "Loading...");
            var listContent = content.transform;
            if (SponsorLoader.IsLoaded)
                PopulateList(listContent, loadingText, SponsorLoader.GetCachedSponsors());
            else
                SponsorLoader.GetSponsors(s => PopulateList(listContent, loadingText, s));
        }
        private static void PopulateList(Transform listContent, Text loadingText, List<string> sponsors)
        {
            if (listContent == null) return;
            if (loadingText != null)
                Object.Destroy(loadingText.gameObject);
            if (sponsors == null || sponsors.Count == 0)
            {
                var emptyObj = MakeGO("Empty", listContent);
                emptyObj.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 40);
                var t = emptyObj.AddComponent<Text>();
                t.font = LocalizedTextManager.current_font;
                t.fontSize = 10;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = COL_SUBTLE;
                t.text = T("sponsor_empty", "No sponsors yet\nThank you for your support!");
                return;
            }
            for (int i = 0; i < sponsors.Count; i++)
                CreateRow(listContent, i, sponsors[i]);
        }
        private static void CreateRow(Transform parent, int rank, string name)
        {
            var row = MakeGO($"Sponsor_{rank}", parent);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 24);
            var rowBg = row.AddComponent<Image>();
            rowBg.sprite = SpriteTextureLoader.getSprite("ui/special/window");
            rowBg.type = Image.Type.Sliced;
            if (rank == 0)      rowBg.color = new Color(0.25f, 0.22f, 0.10f, 0.85f);
            else if (rank == 1) rowBg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);
            else if (rank == 2) rowBg.color = new Color(0.22f, 0.16f, 0.10f, 0.85f);
            else                rowBg.color = new Color(0.14f, 0.15f, 0.20f, 0.7f);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 4f;
            hlg.padding = new RectOffset(4, 4, 2, 2);
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            if (rank < 3)
            {
                string iconPath = rank == 0 ? "ui/icon/crown"
                    : (rank == 1 ? "ui/icon/medal_silver" : "ui/icon/medal_bronze");
                var iconObj = MakeGO("RankIcon", row.transform);
                iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(18, 18);
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = SpriteTextureLoader.getSprite(iconPath);
                iconImg.preserveAspect = true;
            }
            else
            {
                var numObj = MakeGO("RankNum", row.transform);
                numObj.GetComponent<RectTransform>().sizeDelta = new Vector2(18, 18);
                var numTxt = numObj.AddComponent<Text>();
                numTxt.font = LocalizedTextManager.current_font;
                numTxt.fontSize = 9;
                numTxt.alignment = TextAnchor.MiddleCenter;
                numTxt.color = COL_SUBTLE;
                numTxt.text = (rank + 1).ToString();
            }
            var nameObj = MakeGO("Name", row.transform);
            nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(108, 20);
            var nameTxt = nameObj.AddComponent<Text>();
            nameTxt.font = LocalizedTextManager.current_font;
            nameTxt.fontSize = 11;
            nameTxt.alignment = TextAnchor.MiddleLeft;
            if (rank == 0)      nameTxt.color = new Color(1f, 0.84f, 0.28f);
            else if (rank == 1) nameTxt.color = new Color(0.82f, 0.82f, 0.88f);
            else if (rank == 2) nameTxt.color = new Color(0.88f, 0.65f, 0.38f);
            else                nameTxt.color = new Color(0.72f, 0.68f, 0.55f);
            nameTxt.text = name;
        }
        private static GameObject MakeGO(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }
        private static void MakeText(string name, Transform parent, Vector2 pos,
            Vector2 size, string text, int fontSize, TextAnchor align, Color color)
        {
            var obj = MakeGO(name, parent);
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
