using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.ui;
using NeoModLoader.api;
using NeoModLoader.General.UI.Prefabs;
namespace xn.ui
{
    public static class SkinToneBrushToolDisplayPatch
    {
        private static readonly string[] ColorNames = new string[]
        {
            "黑",      
            "白",      
            "灰",      
            "红",      
            "橙",      
            "黄",      
            "绿",      
            "青",      
            "蓝",      
            "紫",      
            "粉",      
            "棕",      
            "棕褐",    
            "金",      
            "银",      
            "藏青"     
        };
        private static System.Type ModConfigListItemType;
        private static System.Reflection.FieldInfo SliderAreaField;
        public static void Init(Harmony harmony)
        {
            ModConfigListItemType = AccessTools.Inner(typeof(ModConfigureWindow), "ModConfigListItem");
            if (ModConfigListItemType == null)
            {
                UnityEngine.Debug.LogWarning("[XN] SkinToneBrushToolDisplayPatch: Could not find ModConfigListItem nested type");
                return;
            }
            SliderAreaField = AccessTools.Field(ModConfigListItemType, "slider_area");
            if (SliderAreaField == null)
            {
                UnityEngine.Debug.LogWarning("[XN] SkinToneBrushToolDisplayPatch: Could not find slider_area field");
                return;
            }
            var method = AccessTools.Method(ModConfigListItemType, "setup_int_slider");
            if (method == null)
            {
                UnityEngine.Debug.LogWarning("[XN] SkinToneBrushToolDisplayPatch: Could not find setup_int_slider method");
                return;
            }
            harmony.Patch(method, postfix: new HarmonyMethod(typeof(SkinToneBrushToolDisplayPatch), nameof(Postfix)));
        }
        public static void Postfix(ModConfigItem pItem, object __instance)
        {
            if (pItem.Id != "xn_config_skin_tone_color")
            {
                return;
            }
            if (SliderAreaField == null) return;
            GameObject sliderArea = SliderAreaField.GetValue(__instance) as GameObject;
            if (sliderArea == null) return;
            Transform info = sliderArea.transform.Find("Info");
            if (info == null) return;
            Transform valueTextObj = info.Find("Value");
            if (valueTextObj == null) return;
            Text valueText = valueTextObj.GetComponent<Text>();
            if (valueText == null) return;
            Transform sliderObj = sliderArea.transform.Find("Slider");
            if (sliderObj == null) return;
            SliderBar sliderBar = sliderObj.GetComponent<SliderBar>();
            if (sliderBar == null) return;
            UpdateColorDisplay(valueText, pItem.IntVal);
            sliderBar.slider.onValueChanged.AddListener(delegate(float val)
            {
                UpdateColorDisplay(valueText, pItem.IntVal);
            });
            var updater = valueTextObj.gameObject.GetComponent<SkinToneColorUpdater>();
            if (updater == null)
            {
                updater = valueTextObj.gameObject.AddComponent<SkinToneColorUpdater>();
                updater.Initialize(pItem, valueText);
            }
        }
        public static string GetColorName(int index)
        {
            if (index >= 0 && index < ColorNames.Length)
            {
                return ColorNames[index];
            }
            return index.ToString();
        }
        private static void UpdateColorDisplay(Text valueText, int intVal)
        {
            if (valueText != null)
            {
                valueText.text = GetColorName(intVal);
            }
        }
    }
    public class SkinToneColorUpdater : MonoBehaviour
    {
        private ModConfigItem configItem;
        private Text valueText;
        private int lastValue = -1;
        public void Initialize(ModConfigItem item, Text text)
        {
            configItem = item;
            valueText = text;
            lastValue = item.IntVal;
            UpdateDisplay();
        }
        private void Update()
        {
            if (configItem != null && valueText != null)
            {
                if (configItem.IntVal != lastValue)
                {
                    lastValue = configItem.IntVal;
                    UpdateDisplay();
                }
            }
        }
        private void UpdateDisplay()
        {
            if (valueText != null && configItem != null)
            {
                valueText.text = SkinToneBrushToolDisplayPatch.GetColorName(configItem.IntVal);
            }
        }
    }
}