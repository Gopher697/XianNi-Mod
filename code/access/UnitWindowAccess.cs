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
        private static readonly FieldInfo WindowMetaTabContainerField = AccessTools.Field(typeof(WindowMetaTab), "container");
        private static readonly MethodInfo TabbedWindowTabsGetter = AccessTools.PropertyGetter(typeof(TabbedWindow), "tabs");
        private static readonly MethodInfo AddTabContentMethod = AccessTools.Method(typeof(WindowMetaTabButtonsContainer), "addTabContent", new[] { typeof(WindowMetaTab), typeof(Transform) });
        private static readonly MethodInfo RefillTabsWithContentMethod = AccessTools.Method(typeof(WindowMetaTabButtonsContainer), "refillTabsWithContent");
        private static readonly PropertyInfo ActorProperty = AccessTools.Property(typeof(UnitWindow), "actor");
        private static bool _warnedWindowMetaTabContainer;
        private static bool _warnedTabbedWindowTabs;
        private static bool _warnedAddTabContent;
        private static bool _warnedRefillTabsWithContent;
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

        public static void SetContainer(WindowMetaTab tab, WindowMetaTabButtonsContainer container)
        {
            if (tab == null || container == null) return;
            if (WindowMetaTabContainerField == null)
            {
                WarnOnce(ref _warnedWindowMetaTabContainer, "[XN] WindowMetaTab.container field not found; tab container was not changed.");
                return;
            }
            WindowMetaTabContainerField.SetValue(tab, container);
        }

        public static WindowMetaTabButtonsContainer GetTabButtonsContainer(TabbedWindow window)
        {
            if (window == null) return null;
            if (TabbedWindowTabsGetter == null)
            {
                WarnOnce(ref _warnedTabbedWindowTabs, "[XN] TabbedWindow.tabs getter not found; tab container lookup failed.");
                return null;
            }
            return TabbedWindowTabsGetter.Invoke(window, null) as WindowMetaTabButtonsContainer;
        }

        public static Transform AddTabContent(WindowMetaTabButtonsContainer container, WindowMetaTab tab, Transform content)
        {
            if (container == null || tab == null || content == null) return null;
            if (AddTabContentMethod == null)
            {
                WarnOnce(ref _warnedAddTabContent, "[XN] WindowMetaTabButtonsContainer.addTabContent method not found; tab content was not added.");
                return null;
            }
            return AddTabContentMethod.Invoke(container, new object[] { tab, content }) as Transform;
        }

        public static void RefillTabsWithContent(WindowMetaTabButtonsContainer container)
        {
            if (container == null) return;
            if (RefillTabsWithContentMethod == null)
            {
                WarnOnce(ref _warnedRefillTabsWithContent, "[XN] WindowMetaTabButtonsContainer.refillTabsWithContent method not found; tabs were not refilled.");
                return;
            }
            RefillTabsWithContentMethod.Invoke(container, null);
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
