using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
namespace xn.ui
{
    public static class XNRankingWindow
    {
        private static bool _inited;
        private static ScrollWindow _window;
        private static Transform _content;
        private static ObjectPoolGenericMono<XNRankingElement> _pool;
        private static Text _titleCounter;
        public const string WINDOW_ID = "xn_power_ranking";
        public const int TOP_COUNT = 50;
        private static readonly Dictionary<long, long> _scoreCache = new Dictionary<long, long>(64);
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _window = WindowCreator.CreateEmptyWindow(WINDOW_ID, "xn_power_ranking_title");
            var bg = _window.transform.Find("Background");
            var scrollView = bg?.Find("Scroll View");
            var viewport = scrollView?.Find("Viewport");
            _content = viewport?.Find("Content");
            if (_content == null)
            {
                Debug.LogError("[XNRankingWindow] Content not found!");
                return;
            }
            SetupContentLayout();
            XNRankingElement.CreatePrefab();
            _pool = new ObjectPoolGenericMono<XNRankingElement>(XNRankingElement.Prefab, _content);
            var titleObj = bg?.Find("Title");
            if (titleObj != null)
            {
                var counterObj = new GameObject("Counter", typeof(Text));
                counterObj.transform.SetParent(titleObj);
                _titleCounter = counterObj.GetComponent<Text>();
                _titleCounter.font = LocalizedTextManager.current_font;
                _titleCounter.fontSize = 12;
                _titleCounter.color = Color.white;
                _titleCounter.alignment = TextAnchor.MiddleRight;
                var rect = counterObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.pivot = new Vector2(1, 0.5f);
                rect.anchoredPosition = new Vector2(-10, 0);
                rect.sizeDelta = new Vector2(60, 20);
            }
            ScrollWindow.addCallbackShow(OnWindowShow);
            xn.ui.charts.RankingLegendButton.Create(bg);
        }
        private static void SetupContentLayout()
        {
            var contentRect = _content.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
            }
            var layoutGroup = _content.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = _content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 2f;
            layoutGroup.padding = new RectOffset(4, 4, 4, 4);
            var sizeFitter = _content.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = _content.gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
        public static void Open()
        {
            if (!_inited) Init();
            ScrollWindow.showWindow(WINDOW_ID);
        }
        private static void OnWindowShow(string windowId)
        {
            if (windowId != WINDOW_ID) return;
            RefreshList();
        }
        private static void RefreshList()
        {
            if (_content == null || _pool == null) return;
            _pool.clear();
            _scoreCache.Clear();
            var topActors = new List<Actor>(TOP_COUNT + 1);
            var topScores = new List<long>(TOP_COUNT + 1);
            foreach (var a in World.world.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (!a.asset.can_be_favorited) continue;
                long s = XNPowerRanking.CalcPowerScoreLongInternal(a);
                int pos = -1;
                for (int t = 0; t < topActors.Count; t++)
                {
                    if (s > topScores[t] || (s == topScores[t] && a.getID() < topActors[t].getID()))
                    {
                        pos = t;
                        break;
                    }
                }
                if (pos == -1)
                {
                    if (topActors.Count < TOP_COUNT)
                    {
                        topActors.Add(a);
                        topScores.Add(s);
                    }
                }
                else
                {
                    topActors.Insert(pos, a);
                    topScores.Insert(pos, s);
                    if (topActors.Count > TOP_COUNT)
                    {
                        topActors.RemoveAt(topActors.Count - 1);
                        topScores.RemoveAt(topScores.Count - 1);
                    }
                }
            }
            for (int i = 0; i < topActors.Count; i++)
            {
                var actor = topActors[i];
                long score = topScores[i];
                _scoreCache[actor.getID()] = score;
                var element = _pool.getNext();
                element.Show(actor, i + 1, score);
            }
            _pool.disableInactive();
            if (_titleCounter != null)
            {
                _titleCounter.text = topActors.Count.ToString();
            }
        }
        internal static bool TryGetScore(Actor a, out long score)
        {
            score = 0;
            if (a == null) return false;
            return _scoreCache.TryGetValue(a.getID(), out score);
        }
    }
}
