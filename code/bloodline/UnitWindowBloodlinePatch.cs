using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
namespace xn.bloodline
{
    [HarmonyPatch(typeof(UnitWindow), "OnEnable")]
    internal static class Patch_UnitWindow_OnEnable_Bloodline
    {
        private static bool s_initialized = false;
        [HarmonyPrefix]
        private static void Prefix(UnitWindow __instance)
        {
            if (!(xn.access.UnitWindowAccess.GetActor(__instance)?.isAlive() ?? false)) return;
            if (!s_initialized)
            {
                s_initialized = true;
                CreateBloodlineTab(__instance);
            }
        }
        private static void CreateBloodlineTab(UnitWindow window)
        {
            var original_genealogy = window.transform.GetComponentInChildren<UnitGenealogyElement>(true);
            if (original_genealogy == null)
            {
                Debug.LogError("[BloodlinePatch] UnitGenealogyElement not found!");
                return;
            }
            var content_bloodline_obj = Object.Instantiate(original_genealogy, window.transform.Find("Background/Scroll View/Viewport/Content")).gameObject;
            content_bloodline_obj.name = "content_bloodline";
            var old_component = content_bloodline_obj.GetComponent<UnitGenealogyElement>();
            var saved_prefab_avatar = old_component.prefab_avatar;
            var saved_transform_grandparents = old_component.transform_grandparents;
            var saved_transform_parents = old_component.transform_parents;
            var saved_transform_siblings = old_component.transform_siblings;
            var saved_transform_children = old_component.transform_children;
            var saved_prefab_unfolder = xn.access.UnitWindowAccess.GetPrefabUnfolder(old_component);
            var saved_sex_icon = xn.access.UnitWindowAccess.GetSexIcon(old_component);
            Object.DestroyImmediate(old_component);
            var content_bloodline = content_bloodline_obj.AddComponent<UnitBloodlineElement>();
            content_bloodline.prefab_avatar = saved_prefab_avatar;
            content_bloodline.transform_founder = saved_transform_grandparents;
            content_bloodline.transform_elders = saved_transform_parents;
            content_bloodline.transform_enforcers = saved_transform_siblings;
            content_bloodline.transform_members = saved_transform_children;
            content_bloodline._prefab_unfolder = saved_prefab_unfolder;
            content_bloodline._sex_icon = saved_sex_icon;
            content_bloodline._default_bloodline_icon = SpriteTextureLoader.getSprite("ui/icon/bloodline");
            var vertical_layout_group = content_bloodline_obj.GetComponent<VerticalLayoutGroup>();
            if (vertical_layout_group != null)
            {
                vertical_layout_group.childControlHeight = true;
                vertical_layout_group.childControlWidth = false;
                vertical_layout_group.childForceExpandHeight = true;
                vertical_layout_group.childForceExpandWidth = false;
                vertical_layout_group.spacing = 6;
                vertical_layout_group.childAlignment = TextAnchor.UpperCenter;
            }
            var transformGrandparents = content_bloodline_obj.transform.Find("Grandparents") ?? content_bloodline_obj.transform.Find("bg_grandparents");
            var transformParents = content_bloodline_obj.transform.Find("Parents") ?? content_bloodline_obj.transform.Find("bg_parents");
            var transformSiblings = content_bloodline_obj.transform.Find("Siblings") ?? content_bloodline_obj.transform.Find("bg_siblings");
            var transformChildren = content_bloodline_obj.transform.Find("Children") ?? content_bloodline_obj.transform.Find("bg_children");
            SetLocalizedTextForTransform(transformGrandparents, "bloodline_founder");
            SetLocalizedTextForTransform(transformParents, "bloodline_elders");
            SetLocalizedTextForTransform(transformSiblings, "bloodline_enforcers");
            SetLocalizedTextForTransform(transformChildren, "bloodline_members");
            Transform tabsContainer = window.transform.Find("Background/Tabs");
            int indexToInsert = -1;
            for (int i = 0; i < tabsContainer.childCount; i++)
            {
                var t = tabsContainer.GetChild(i);
                if (t.name.ToLower().Contains("genealogy"))
                {
                    indexToInsert = i + 1; 
                    break;
                }
            }
            if (indexToInsert < 0)
                indexToInsert = tabsContainer.childCount;
            var tabButtonsContainer = xn.access.UnitWindowAccess.GetTabButtonsContainer(window);
            if (tabButtonsContainer == null)
            {
                Debug.LogWarning("[BloodlinePatch] Unit window tab container not found.");
                return;
            }
            var bloodline_entry = Object.Instantiate(window.transform.Find("Background/Tabs/Genealogy").GetComponent<WindowMetaTab>(), tabsContainer);
            bloodline_entry.name = "BloodlineTab";
            bloodline_entry.tab_action = new WindowMetaTabEvent();
            bloodline_entry.tab_action.AddListener(new UnityEngine.Events.UnityAction<WindowMetaTab>(tab =>
            {
                window.showTab(tab);
            }));
            var bloodIcon = SpriteTextureLoader.getSprite("ui/icon/bloodline");
            if (bloodIcon != null)
            {
                var powerButton = bloodline_entry.GetComponentInChildren<PowerButton>();
                if (powerButton != null && powerButton.icon != null)
                {
                    powerButton.icon.sprite = bloodIcon;
                }
                else
                {
                    var images = bloodline_entry.GetComponentsInChildren<Image>(true);
                    foreach (var img in images)
                    {
                        if (img.gameObject.name.ToLower().Contains("icon") ||
                            img.transform.parent != null && img.transform.parent.GetComponent<Button>() != null)
                        {
                            img.sprite = bloodIcon;
                            break;
                        }
                    }
                }
            }
            var tipButton = bloodline_entry.GetComponentInChildren<TipButton>();
            if (tipButton != null)
            {
                tipButton.textOnClick = "tab_bloodline";
                tipButton.textOnClickDescription = "tab_bloodline_description";
            }
            bloodline_entry.transform.SetSiblingIndex(indexToInsert);
            xn.access.UnitWindowAccess.SetContainer(bloodline_entry, tabButtonsContainer);
            bloodline_entry.tab_elements.RemoveAll(t => t.name.ToLower().StartsWith("content_"));
            var tabs = xn.access.UnitWindowAccess.GetTabs(tabButtonsContainer);
            if (tabs != null && !tabs.Contains(bloodline_entry))
            {
                tabs.Add(bloodline_entry);
            }
            xn.access.UnitWindowAccess.AddTabContent(tabButtonsContainer, bloodline_entry, content_bloodline_obj.transform);
            xn.access.UnitWindowAccess.RefillTabsWithContent(tabButtonsContainer);
            Debug.Log("[BloodlinePatch] Bloodline tab created successfully!");
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
