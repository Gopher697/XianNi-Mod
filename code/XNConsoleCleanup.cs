using System;
using System.Collections.Generic;
namespace xn.world
{
    internal static class XNConsoleCleanup
    {
        private const string CONSOLE_MARK = "\u2063XN";
        private static readonly string[] KNOWN_PHRASES = new string[] {
            "正在经历天道的考验",
            "在天道考验中成功",
            "突破了 ",
            "突破失败跌落至 ",
            "的古神之躯突破到 ",
            "的妖兽道行突破到 ",
            "领悟了 ",
            "尝试领悟意境失败了",
            "探索遗迹获得了 ",
            "夺舍成功",
            "夺舍 ",
            "收了 [",
            "传功于 ",
            "等待我的复仇怒火吧",
            "炼化了其徒弟",
            "天运子讲道",
            "建造了遗迹",
            "人被天运子赏赐了",
            "人被天运子反噬了",
            "天运赏赐",
            "天运反噬"
        };
        public static string Mark(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text + CONSOLE_MARK;
        }
        public static void ClearBroadcastLogs()
        {
            var q = WorldBoxConsole.Console._texts;
            if (q == null) return;
            var kept = new Queue<string>(q.Count);
            while (q.Count > 0)
            {
                var s = q.Dequeue();
                if (ShouldKeep(s)) kept.Enqueue(s);
            }
            WorldBoxConsole.Console._texts = kept;
            WorldBoxConsole.Console._line_num = 0;
        }
        private static bool ShouldKeep(string s)
        {
            if (string.IsNullOrEmpty(s)) return true;
            if (s.IndexOf(CONSOLE_MARK, StringComparison.Ordinal) >= 0) return false;
            for (int i = 0; i < KNOWN_PHRASES.Length; i++)
                if (s.IndexOf(KNOWN_PHRASES[i], StringComparison.Ordinal) >= 0)
                    return false;
            return true;
        }
    }
}