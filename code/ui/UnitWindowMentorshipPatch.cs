using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace xn.ui
{
    [HarmonyPatch(typeof(UnitWindow), "OnEnable")]
    internal static class Patch_UnitWindow_OnEnable_Mentorship
    {
        private static bool s_initialized = false;
        [HarmonyPrefix]
        private static void Prefix(UnitWindow __instance)
        {
            if (!(__instance.actor?.isAlive() ?? false)) return;
            if (!s_initialized)
            {
                s_initialized = true;
                var original_genealogy = __instance.transform.GetComponentInChildren<UnitGenealogyElement>(true);
                var content_mentorship_obj = Object.Instantiate(original_genealogy, __instance.transform.Find("Background/Scroll View/Viewport/Content")).gameObject;
                content_mentorship_obj.name = "content_mentorship";
                var old_component = content_mentorship_obj.GetComponent<UnitGenealogyElement>();
                var saved_prefab_avatar = old_component.prefab_avatar;
                var saved_transform_grandparents = old_component.transform_grandparents;
                var saved_transform_parents = old_component.transform_parents;
                var saved_transform_siblings = old_component.transform_siblings;
                var saved_transform_children = old_component.transform_children;
                var saved_prefab_unfolder = old_component._prefab_unfolder;
                var saved_sex_icon = old_component._sex_icon;
                Object.DestroyImmediate(old_component);
                var content_mentorship = content_mentorship_obj.AddComponent<UnitMentorshipElement>();
                content_mentorship.prefab_avatar = saved_prefab_avatar;
                content_mentorship.transform_shizu = saved_transform_grandparents;
                content_mentorship.transform_shifu = saved_transform_parents;
                content_mentorship.transform_tongmen = saved_transform_siblings;
                content_mentorship.transform_disciples = saved_transform_children;
                content_mentorship._prefab_unfolder = saved_prefab_unfolder;
                content_mentorship._sex_icon = saved_sex_icon;
                content_mentorship._default_mentorship_icon = SpriteTextureLoader.getSprite("ui/icon/shitu") 
                                                           ?? SpriteTextureLoader.getSprite("ui/icons/shitu");
                var vertical_layout_group = content_mentorship_obj.GetComponent<VerticalLayoutGroup>();
                if (vertical_layout_group != null)
                {
                    vertical_layout_group.childControlHeight = true;
                    vertical_layout_group.childControlWidth = false;
                    vertical_layout_group.childForceExpandHeight = true;
                    vertical_layout_group.childForceExpandWidth = false;
                    vertical_layout_group.spacing = 6;
                    vertical_layout_group.childAlignment = TextAnchor.UpperCenter;
                }
                var transformGrandparents = content_mentorship_obj.transform.Find("Grandparents") ?? content_mentorship_obj.transform.Find("bg_grandparents");
                var transformParents = content_mentorship_obj.transform.Find("Parents") ?? content_mentorship_obj.transform.Find("bg_parents");
                var transformSiblings = content_mentorship_obj.transform.Find("Siblings") ?? content_mentorship_obj.transform.Find("bg_siblings");
                var transformChildren = content_mentorship_obj.transform.Find("Children") ?? content_mentorship_obj.transform.Find("bg_children");
                SetLocalizedTextForTransform(transformGrandparents, "mentorship_grandparents");
                SetLocalizedTextForTransform(transformParents, "mentorship_parents");
                SetLocalizedTextForTransform(transformSiblings, "mentorship_siblings");
                SetLocalizedTextForTransform(transformChildren, "mentorship_children");
                Transform tabsContainer = __instance.transform.Find("Background/Tabs");
                Transform genealogyTabTransform = null;
                int indexToInsert = -1;
                for (int i = 0; i < tabsContainer.childCount; i++)
                {
                    var t = tabsContainer.GetChild(i);
                    if (t.name.ToLower().Contains("genealogy"))
                    {
                        genealogyTabTransform = t;
                        indexToInsert = i;
                        break;
                    }
                }
                if (indexToInsert < 0)
                    indexToInsert = tabsContainer.childCount;
                var mentorship_entry = Object.Instantiate(__instance.transform.Find("Background/Tabs/Genealogy").GetComponent<WindowMetaTab>(), tabsContainer);
                mentorship_entry.name = "MentorshipTab";
                mentorship_entry.tab_action = new WindowMetaTabEvent();
                mentorship_entry.tab_action.AddListener(new UnityEngine.Events.UnityAction<WindowMetaTab>(tab =>
                {
                    __instance.showTab(tab);
                }));
                var shituIcon = SpriteTextureLoader.getSprite("ui/icon/shitu") 
                             ?? SpriteTextureLoader.getSprite("ui/icons/shitu");
                if (shituIcon != null)
                {
                    var powerButton = mentorship_entry.GetComponentInChildren<PowerButton>();
                    if (powerButton != null && powerButton.icon != null)
                    {
                        powerButton.icon.sprite = shituIcon;
                    }
                    else
                    {
                        var images = mentorship_entry.GetComponentsInChildren<Image>(true);
                        foreach (var img in images)
                        {
                            if (img.gameObject.name.ToLower().Contains("icon") || 
                                img.transform.parent != null && img.transform.parent.GetComponent<Button>() != null)
                            {
                                img.sprite = shituIcon;
                                break;
                            }
                        }
                    }
                }
                var tipButton = mentorship_entry.GetComponentInChildren<TipButton>();
                if (tipButton != null)
                {
                    tipButton.textOnClick = "tab_mentorship";
                    tipButton.textOnClickDescription = "tab_mentorship_description";
                }
                mentorship_entry.transform.SetSiblingIndex(indexToInsert);
                for (int i = 0; i < indexToInsert; i++)
                {
                    var t = tabsContainer.GetChild(i);
                    if (t.name.ToLower().Contains("space"))
                    {
                        Object.DestroyImmediate(t.gameObject);
                        break;
                    }
                }
                mentorship_entry.container = __instance.tabs;
                mentorship_entry.tab_elements.RemoveAll(t => t.name.ToLower().StartsWith("content_"));
                if (!__instance.tabs._tabs.Contains(mentorship_entry))
                    {
                    __instance.tabs._tabs.Add(mentorship_entry);
                }
                __instance.tabs.addTabContent(mentorship_entry, content_mentorship_obj.transform);
                __instance.tabs.refillTabsWithContent();
            }
        }
        private static void SetLocalizedTextForTransform(Transform parent, string key)
        {
            if (parent == null) return;
            var localizedText = parent.GetComponentInChildren<LocalizedText>(true);
            if (localizedText != null)
            {
                localizedText.key = key;
                return;
            }
            var textComponent = parent.GetComponentInChildren<Text>(true);
            if (textComponent != null)
            {
                localizedText = textComponent.GetComponent<LocalizedText>();
                if (localizedText == null)
                {
                    localizedText = textComponent.gameObject.AddComponent<LocalizedText>();
                }
                localizedText.key = key;
            }
        }
    }
}