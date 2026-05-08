using System;
using System.Collections.Generic;
namespace xn.world
{
    internal static class XNConsoleCleanup
    {
        private const string CONSOLE_MARK = "\u2063XN";
        private static readonly string[] KNOWN_PHRASES = new string[] {
            "is undergoing the trial of Heaven",
            "succeeded in the trial of Heaven",
            "broke through to ",
            "botched their breakthrough",
            "'s Ancient God body broke through",
            "'s Beast cultivation broke through",
            "comprehended ",
            "tried to comprehend Intent",
            "explored the ruins and obtained",
            "successfully possessed the body",
            "failed to possess ",
            "took [",
            "transferred cultivation to",
            "Await my vengeful wrath",
            "refined their disciple",
            "Tian Yunzi is preaching",
            "built ruins in the world",
            "people were rewarded by Tian Yunzi",
            "people were bitten by Tian Yunzi",
            "Tian Yunzi reward",
            "Tian Yunzi backlash"
        };
        public static string Mark(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text + CONSOLE_MARK;
        }
        public static void ClearBroadcastLogs()
        {
            var q = xn.access.WorldBoxConsoleAccess.GetTexts();
            if (q == null) return;
            var kept = new Queue<string>(q.Count);
            while (q.Count > 0)
            {
                var s = q.Dequeue();
                if (ShouldKeep(s)) kept.Enqueue(s);
            }
            xn.access.WorldBoxConsoleAccess.SetTexts(kept);
            xn.access.WorldBoxConsoleAccess.SetLineNum(0);
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
