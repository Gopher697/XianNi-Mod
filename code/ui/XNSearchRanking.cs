using System;
using System.Collections.Generic;
using UnityEngine;
namespace xn.ui
{
    public static class XNSearchRanking
    {
        private static bool _inited;
        internal static bool active;
        private static readonly List<Actor> _favoritesBefore = new List<Actor>(256);
        private static readonly List<Actor> _addedFavorites  = new List<Actor>(32);
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            ScrollWindow.addCallbackShow(OnAnyWindowShow);
            ScrollWindow.addCallbackHide(OnAnyWindowHide);
        }
        public static void Open(string keyword)
        {
            if (active) return;
            if (string.IsNullOrEmpty(keyword)) keyword = "";
            BuildAndShow(keyword);
        }
        private static void BuildAndShow(string keyword)
        {
            Init();
            _favoritesBefore.Clear();
            _addedFavorites.Clear();
            foreach (var u in World.world.units) 
            {
                if (u != null && u.isAlive() && u.isFavorite())
                {
                    _favoritesBefore.Add(u);
                }
            }
            for (int i = 0; i < _favoritesBefore.Count; i++)
            {
                var a = _favoritesBefore[i];
                if (a != null && a.isAlive() && a.isFavorite()) a.switchFavorite();
            }
            int found = 0;
            for (var e = World.world.units.GetEnumerator(); e.MoveNext();)
            {
                var a = e.Current;
                if (a == null || !a.isAlive()) continue;
                var name = a.getName();
                if (string.IsNullOrEmpty(name)) continue;
                if (name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (!a.isFavorite())
                    {
                        a.switchFavorite();
                        _addedFavorites.Add(a);
                    }
                    found++;
                    if (found >= 20) break;
                }
            }
            active = true;
            ScrollWindow.showWindow("list_favorite_units");
        }
        private static void OnAnyWindowShow(string screenId)
        {
            if (!active) return;
            if (string.IsNullOrEmpty(screenId)) return;
            if (screenId != "list_favorite_units") RestoreAndExit();
        }
        private static void OnAnyWindowHide(string screenId)
        {
            if (!active) return;
            if (string.IsNullOrEmpty(screenId)) return;
            if (screenId == "list_favorite_units") RestoreAndExit();
        }
        private static void RestoreAndExit()
        {
            for (int i = 0; i < _addedFavorites.Count; i++)
            {
                var a = _addedFavorites[i];
                if (a != null && a.isAlive() && a.isFavorite()) a.switchFavorite();
            }
            _addedFavorites.Clear();
            for (int i = 0; i < _favoritesBefore.Count; i++)
            {
                var a = _favoritesBefore[i];
                if (a != null && a.isAlive() && !a.isFavorite()) a.switchFavorite();
            }
            _favoritesBefore.Clear();
            active = false;
        }
    }
}