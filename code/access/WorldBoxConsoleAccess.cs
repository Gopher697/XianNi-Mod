using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class WorldBoxConsoleAccess
    {
        private static readonly FieldInfo TextsField = AccessTools.Field(typeof(WorldBoxConsole.Console), "_texts");
        private static readonly FieldInfo LineNumField = AccessTools.Field(typeof(WorldBoxConsole.Console), "_line_num");

        public static Queue<string> GetTexts()
        {
            if (TextsField == null)
            {
                Debug.LogWarning("[XN] WorldBoxConsole.Console._texts field not found.");
                return null;
            }
            return TextsField.GetValue(null) as Queue<string>;
        }

        public static void SetTexts(Queue<string> texts)
        {
            if (TextsField == null)
            {
                Debug.LogWarning("[XN] WorldBoxConsole.Console._texts field not found; console text queue was not updated.");
                return;
            }
            TextsField.SetValue(null, texts);
        }

        public static void SetLineNum(int value)
        {
            if (LineNumField == null)
            {
                Debug.LogWarning("[XN] WorldBoxConsole.Console._line_num field not found; line number was not reset.");
                return;
            }
            LineNumField.SetValue(null, value);
        }
    }
}
