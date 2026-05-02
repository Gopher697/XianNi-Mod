using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace xn.ui
{
    public static class UnitWindowStatsIcon
    {
        private static bool _hasDelayedSort = false;
        private static Transform _pendingSortContent = null;
        public static readonly StatsIconData[] Stats = {
            new ("xn_xiuwei",          "stats/xiuwei"),          
            new ("xn_lingli",          "stats/lingli"),          
            new ("xn_yuanli",          "stats/yuanli"),          
            new ("xn_nieli",           "stats/nieli"),           
            new ("xn_gushen_power",    "stats/gushen"),          
            new ("xn_wuxin",            "stats/wuxin"),          
            new ("xn_qiyun",           "stats/qiyun"),           
            new ("xn_xinmo",           "stats/xinmo"),           
            new ("xn_yaoli",           "stats/yaoli"),           
            new ("xn_lingshi",         "stats/lingshi"),         
            new ("xn_lingshi_supreme", "stats/lingshi_supreme")  
        };
        private const string MarkerObjectName = "xn_stats_group_marker";
        private static StatsIconContainer _cachedContainer;
        private static int _lastWindowId = -1;
        public static void Initialize(UnitWindow window)
        {
            if (window == null) return;
            var content = window.gameObject.transform.Find("Background/Scroll View/Viewport/Content/content_more_icons");
            if (content == null) return;
            var statsContainer = content.GetComponent<StatsIconContainer>();
            bool containerExisted = statsContainer != null;
            if (!containerExisted)
            {
                statsContainer = content.gameObject.AddComponent<StatsIconContainer>();
            }
            var marker = content.Find(MarkerObjectName);
            if (marker != null && AreIconsRegistered(statsContainer))
            {
                return;
            }
            if (marker != null)
            {
                CleanupOldRows(content);
                Object.DestroyImmediate(marker.gameObject);
            }
            var scroll = window.gameObject.transform.Find("Background/Scroll View")?.GetComponent<ScrollRect>();
            if (scroll != null) scroll.enabled = true;
            var viewport = window.gameObject.transform.Find("Background/Scroll View/Viewport");
            if (viewport != null)
            {
                var mask = viewport.GetComponent<Mask>(); if (mask != null) mask.enabled = true;
                var img  = viewport.GetComponent<Image>(); if (img  != null) img.enabled  = true;
            }
            Transform protoParent = null;
            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child.name.StartsWith("xn_") || child.name.StartsWith("test_"))
                    continue;
                if (child.Find("i_kills") != null)
                {
                    protoParent = child;
                    break;
                }
            }
            if (protoParent == null)
            {
                protoParent = content.childCount > 4 ? content.GetChild(4) : content.GetChild(content.childCount - 1);
            }
            var protoIcon = protoParent.Find("i_kills");
            if (protoIcon == null) return;
            var newMarker = new GameObject(MarkerObjectName);
            newMarker.transform.SetParent(content, false);
            int total = Stats.Length;
            int created = 0;
            for (int row = 0; created < total; row++)
            {
                var rowGroup = Object.Instantiate(protoParent, content);
                for (int i = rowGroup.childCount - 1; i >= 0; i--)
                {
                    var child = rowGroup.GetChild(i);
                    if (child.name != "i_kills")
                        Object.Destroy(child.gameObject);
                }
                int slots = Mathf.Min(5, total - created);
                for (int i = 0; i < slots; i++)
                {
                    var data = Stats[created + i];
                    var iconTr = Object.Instantiate(protoIcon, rowGroup);
                    var icon   = iconTr.GetComponent<StatsIcon>();
                    var tipBtn = iconTr.GetComponent<TipButton>();
                    icon.name = data.name;
                    string key = "stats_icon_" + data.name;
                    if (tipBtn != null)
                    {
                        tipBtn.textOnClick            = key;
                        tipBtn.textOnClickDescription = key + "_desc";
                    }
                    var sprite = Resources.Load<Sprite>(data.iconPath);
                    if (sprite == null) sprite = Resources.Load<Sprite>("zhanwei");
                    if (sprite != null) icon.getIcon().sprite = sprite;
                    icon.setValue(0f);
                    RegisterIconToContainer(statsContainer, data.name, icon);
                }
                var killProto = rowGroup.Find("i_kills");
                if (killProto != null) Object.DestroyImmediate(killProto.gameObject);
                rowGroup.name = "xn_stats_group_row_" + (row + 1);
                rowGroup.transform.localScale = Vector3.one;
                created += slots;
            }
            _pendingSortContent = content;
            if (!_hasDelayedSort)
            {
                _hasDelayedSort = true;
                DelayedActionsManager.addAction(DelayedMoveRowsToFront, 0.1f, false);
            }
            _cachedContainer = null;
            _lastWindowId = -1;
        }
        private static void DelayedMoveRowsToFront()
        {
            if (_pendingSortContent != null)
            {
                MoveRowsToFront(_pendingSortContent);
                _pendingSortContent = null;
            }
            _hasDelayedSort = false;
        }
        private static void MoveRowsToFront(Transform content)
        {
            if (content == null) return;
            var marker = content.Find(MarkerObjectName);
            int totalRows = Mathf.CeilToInt((float)Stats.Length / 5f);
            var rows = new List<Transform>();
            for (int row = 0; row < totalRows; row++)
            {
                string rowName = "xn_stats_group_row_" + (row + 1);
                var rowGroup = content.Find(rowName);
                if (rowGroup != null)
                {
                    rows.Add(rowGroup);
                }
            }
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                rows[i].SetAsFirstSibling();
            }
            if (marker != null)
            {
                marker.SetAsFirstSibling();
            }
        }
        private static void CleanupOldRows(Transform content)
        {
            int totalRows = Mathf.CeilToInt((float)Stats.Length / 5f);
            for (int row = 0; row < totalRows; row++)
            {
                string rowName = "xn_stats_group_row_" + (row + 1);
                var rowGroup = content.Find(rowName);
                if (rowGroup != null)
                {
                    Object.DestroyImmediate(rowGroup.gameObject);
                }
            }
        }
        private static void RegisterIconToContainer(StatsIconContainer container, string name, StatsIcon icon)
        {
            var statsIcons = xn.access.StatsIconContainerAccess.GetStatsIcons(container);
            if (statsIcons == null) return;
            if (statsIcons.ContainsKey(name))
            {
                statsIcons[name] = icon;
            }
            else
            {
                statsIcons.Add(name, icon);
            }
        }
        private static bool AreIconsRegistered(StatsIconContainer container)
        {
            var statsIcons = xn.access.StatsIconContainerAccess.GetStatsIcons(container);
            if (statsIcons == null) return false;
            int checkCount = Mathf.Min(3, Stats.Length);
            for (int i = 0; i < checkCount; i++)
            {
                var iconName = Stats[i].name;
                if (!statsIcons.TryGetValue(iconName, out var icon))
                    return false;
                if (icon == null || icon.gameObject == null)
                    return false;
            }
            return true;
        }
        private static void EnsureIconsRegistered(StatsIconContainer container, Transform content)
        {
            if (container == null || content == null) return;
            int totalRows = Mathf.CeilToInt((float)Stats.Length / 5f);
            int iconIndex = 0;
            for (int row = 0; row < totalRows && iconIndex < Stats.Length; row++)
            {
                string rowName = "xn_stats_group_row_" + (row + 1);
                var rowGroup = content.Find(rowName);
                if (rowGroup == null) continue;
                for (int i = 0; i < rowGroup.childCount && iconIndex < Stats.Length; i++)
                {
                    var child = rowGroup.GetChild(i);
                    var icon = child.GetComponent<StatsIcon>();
                    if (icon != null && icon.name == Stats[iconIndex].name)
                    {
                        RegisterIconToContainer(container, Stats[iconIndex].name, icon);
                        iconIndex++;
                    }
                }
            }
        }
        public static void OnEnable(UnitWindow window, Actor actor)
        {
            if (window == null || actor == null) return;
            var content = window.gameObject.transform.Find("Background/Scroll View/Viewport/Content/content_more_icons");
            if (content == null) return;
            int windowId = window.GetInstanceID();
            if (_cachedContainer == null || _lastWindowId != windowId)
            {
                _cachedContainer = content.GetComponent<StatsIconContainer>();
                _lastWindowId = windowId;
            }
            var marker = content.Find(MarkerObjectName);
            if (marker == null)
            {
                Initialize(window);
                _cachedContainer = content.GetComponent<StatsIconContainer>();
            }
            else if (_cachedContainer != null && !AreIconsRegistered(_cachedContainer))
            {
                EnsureIconsRegistered(_cachedContainer, content);
                if (!AreIconsRegistered(_cachedContainer))
                {
                    Initialize(window);
                    _cachedContainer = content.GetComponent<StatsIconContainer>();
                }
            }
            if (_cachedContainer == null) return;
            float Xiuwei          = GetLongAsFloat(actor, Keys.Xiuwei);
            float Lingli          = GetInt(actor, Keys.Lingli);
            float Yuanli          = GetInt(actor, Keys.Yuanli);
            float Nieli           = GetInt(actor, Keys.Nieli);
            float GushenPower     = GetInt(actor, Keys.GushenPower);
            float Wuxin           = GetInt(actor, Keys.WuXin);
            float Qiyun           = GetInt(actor, Keys.Qiyun);
            float Xinmo           = GetInt(actor, Keys.Xinmo);
            float Yaoli           = GetInt(actor, Keys.Yaoli);
            float Lingshi         = GetLongAsFloat(actor, Keys.Lingshi);
            float LingshiSupreme  = GetLongAsFloat(actor, Keys.LingshiSupreme);
            _cachedContainer.setIconValue("xn_xiuwei",          Xiuwei);
            _cachedContainer.setIconValue("xn_lingli",          Lingli);
            _cachedContainer.setIconValue("xn_yuanli",          Yuanli);
            _cachedContainer.setIconValue("xn_nieli",           Nieli);
            _cachedContainer.setIconValue("xn_gushen_power",    GushenPower);
            _cachedContainer.setIconValue("xn_wuxin",           Wuxin);
            _cachedContainer.setIconValue("xn_qiyun",           Qiyun);
            _cachedContainer.setIconValue("xn_xinmo",           Xinmo);
            _cachedContainer.setIconValue("xn_yaoli",           Yaoli);
            _cachedContainer.setIconValue("xn_lingshi",         Lingshi);
            _cachedContainer.setIconValue("xn_lingshi_supreme", LingshiSupreme);
        }
        private static int GetInt(Actor actor, string key, int defVal = 0)
        {
            int v; xn.access.ActorAccess.GetData(actor).get(key, out v, defVal);
            return v;
        }
        private static long GetLong(Actor actor, string key, long defVal = 0)
        {
            long v; xn.access.ActorAccess.GetData(actor).get(key, out v, defVal);
            return v;
        }
        private static float GetLongAsFloat(Actor actor, string key, long defVal = 0)
        {
            return (float)GetLong(actor, key, defVal);
        }
        public static void OnDisable(UnitWindow window)
        {
            if (window != null && window.GetInstanceID() == _lastWindowId)
            {
                _cachedContainer = null;
                _lastWindowId = -1;
            }
        }
        private static class Keys
        {
            public const string Xiuwei         = "xn.stat.xiuwei";
            public const string Lingli         = "xn.stat.lingli";
            public const string Yuanli         = "xn.stat.yuanli";
            public const string Nieli          = "xn.stat.nieli";
            public const string GushenPower    = "xn.stat.gushen_power";
            public const string WuXin          = "xn.stat.wuxin";
            public const string Qiyun          = "xn.stat.qiyun";
            public const string Xinmo          = "xn.stat.xinmo";
            public const string Yaoli          = "xn.stat.yaoli";
            public const string Lingshi        = "xn.stat.lingshi";
            public const string LingshiSupreme = "xn.stat.lingshi_supreme";
        }
        public class StatsIconData
        {
            public string name;
            public string iconPath;
            public StatsIconData(string name, string iconPath)
            {
                this.name = name;
                this.iconPath = iconPath;
            }
        }
    }
}
