using UnityEngine;
using db; 
namespace xn.world
{
    public static class XNHistoryRegistry
    {
        public const string GROUP_ID = "cultivation";                 
        public const string LOG_ID   = "cultivation_broadcast";       
        public const string ICON_PATH = "ui/icon/history";            
        static bool _inited;
        static HistoryGroupAsset _groupAsset;
        static WorldLogAsset _logAsset;
        public static void Init()
        {
            if (_inited) return;
            _groupAsset = new HistoryGroupAsset
            {
                id = GROUP_ID,
                icon_path = ICON_PATH
            };
            AssetManager.history_groups.add(_groupAsset);
            _logAsset = new WorldLogAsset
            {
                id = LOG_ID,
                group = GROUP_ID,
                path_icon = ICON_PATH,
                color = Toolbox.color_log_neutral,
                text_replacer = delegate(WorldLogMessage msg, ref string text)
                {
                    text = msg.special1;
                }
            };
            AssetManager.world_log_library.add(_logAsset);
            _inited = true;
        }
        public static void LogBroadcast(string text)
        {
            if (!_inited || _logAsset == null || string.IsNullOrEmpty(text))
                return;
            var msg = new WorldLogMessage(_logAsset, text);
            DBInserter.insertLog(msg);
            if (xn.config.ModConfigHooks.EnableBroadcastDisplay && HistoryHud.instance != null)
            {
                HistoryHud.instance.newHistory(msg);
            }
        }
        public static void LogBroadcastForActor(Actor a, string text)
        {
            if (!_inited || _logAsset == null || string.IsNullOrEmpty(text) || a == null)
                return;
            var msg = new WorldLogMessage(_logAsset, text)
            {
                unit     = a,                       
                location = a.current_position       
            };
            DBInserter.insertLog(msg);
            if (xn.config.ModConfigHooks.EnableBroadcastDisplay && HistoryHud.instance != null)
            {
                HistoryHud.instance.newHistory(msg);
            }
        }
        public static void LogBroadcastAtTile(WorldTile tile, string text)
        {
            if (!_inited || _logAsset == null || string.IsNullOrEmpty(text) || tile == null)
                return;
            var msg = new WorldLogMessage(_logAsset, text)
            {
                location = tile.posV3               
            };
            DBInserter.insertLog(msg);
            if (xn.config.ModConfigHooks.EnableBroadcastDisplay && HistoryHud.instance != null)
            {
                HistoryHud.instance.newHistory(msg);
            }
        }
    }
}