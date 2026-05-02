using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace xn.access
{
    internal static class UnitWindowAccess
    {
        private static readonly FieldInfo PrefabUnfolderField = AccessTools.Field(typeof(UnitGenealogyElement), "_prefab_unfolder");
        private static readonly FieldInfo SexIconField = AccessTools.Field(typeof(UnitGenealogyElement), "_sex_icon");
        private static readonly FieldInfo TabsField = AccessTools.Field(typeof(WindowMetaTabButtonsContainer), "_tabs");
        private static readonly PropertyInfo ActorProperty = AccessTools.Property(typeof(UnitWindow), "actor");
        private static bool _warnedActor;

        public static UnfoldButton GetPrefabUnfolder(UnitGenealogyElement element)
        {
            if (element == null || PrefabUnfolderField == null) return null;
            return PrefabUnfolderField.GetValue(element) as UnfoldButton;
        }

        public static Image GetSexIcon(UnitGenealogyElement element)
        {
            if (element == null || SexIconField == null) return null;
            return SexIconField.GetValue(element) as Image;
        }

        public static List<WindowMetaTab> GetTabs(WindowMetaTabButtonsContainer container)
        {
            if (container == null) return null;
            if (TabsField == null)
            {
                Debug.LogWarning("[XN] WindowMetaTabButtonsContainer._tabs field not found.");
                return null;
            }
            return TabsField.GetValue(container) as List<WindowMetaTab>;
        }

        public static Actor GetActor(UnitWindow window)
        {
            if (window == null) return null;
            if (ActorProperty == null)
            {
                WarnOnce(ref _warnedActor, "[XN] UnitWindow.actor property not found; unit window actor lookup failed.");
                return null;
            }
            return ActorProperty.GetValue(window, null) as Actor;
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
