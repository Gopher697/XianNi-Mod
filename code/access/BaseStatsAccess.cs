using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class BaseStatsAccess
    {
        private static readonly FieldInfo TagsField = AccessTools.Field(typeof(BaseStats), "_tags");

        public static bool RemoveTag(BaseStats stats, string tag)
        {
            if (stats == null || string.IsNullOrEmpty(tag)) return false;
            if (TagsField == null)
            {
                Debug.LogWarning("[XN] BaseStats._tags field not found; tag was not removed.");
                return false;
            }
            var tags = TagsField.GetValue(stats) as ICollection<string>;
            return tags != null && tags.Remove(tag);
        }
    }
}
