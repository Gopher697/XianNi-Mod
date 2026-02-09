using HarmonyLib;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace xn.expand
{
    [HarmonyPatch(typeof(UnitWindow))]
    internal static class UnitWindowCultivationHistoryPatch
    {
        private static bool _initialized = false;
        private static PowerButton _historyButton;
        private static Text _historyText;
        private static GameObject _historyTextObj;
        private static UnitWindow _currentWindow;
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UnitWindow.OnEnable))]
        public static void OnEnable_Postfix(UnitWindow __instance)
        {
            Actor actor = __instance.actor;
            if (actor == null) return;
            _currentWindow = __instance;
            if (!_initialized)
            {
                _initialized = true;
                CreateHistoryButton(__instance);
            }
            EnsureHistoryText(__instance);
            long actorId = actor.getID();
            double createdTime = actor.data.created_time;
            if (CultivationHistoryStorage.HasHistory(actorId, createdTime))
            {
                string history = CultivationHistoryStorage.Load(actorId, createdTime);
                if (!string.IsNullOrEmpty(history))
                {
                    ShowHistoryText(history);
                }
            }
            else
            {
                HideHistoryText();
            }
        }
        private static void CreateHistoryButton(UnitWindow window)
        {
            var backgroundTransform = window.transform.Find("Background");
            if (backgroundTransform == null) return;
            var icon = SpriteTextureLoader.getSprite("ui/icon/historybook")
                ?? SpriteTextureLoader.getSprite("ui/icons/iconBook");
            _historyButton = PowerButtonCreator.CreateSimpleButton(
                "XN_CultivationHistory",
                () => OnHistoryButtonClick(window),
                icon,
                backgroundTransform,
                new Vector2(-156, 50)
            );
            if (_historyButton != null)
            {
                var tipButton = _historyButton.GetComponent<TipButton>() ?? _historyButton.gameObject.AddComponent<TipButton>();
                tipButton.textOnClick = "修仙史";
                tipButton.textOnClickDescription = "左键：查看/生成修仙史\n右键：重新生成\n（每局限15次，使用自定义API不限次数）";
                var trigger = _historyButton.gameObject.GetComponent<EventTrigger>() ?? _historyButton.gameObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener((data) =>
                {
                    var pointerData = (PointerEventData)data;
                    if (pointerData.button == PointerEventData.InputButton.Right)
                    {
                        OnHistoryButtonRightClick();
                    }
                });
                trigger.triggers.Add(entry);
            }
        }
        private static void EnsureHistoryText(UnitWindow window)
        {
            var bg = window.transform.Find("Background");
            if (bg == null) return;
            _historyText = bg.Find("XN_CultivationHistory_Text")?.GetComponent<Text>();
            if (_historyText == null)
            {
                _historyTextObj = new GameObject("XN_CultivationHistory_Text", typeof(Text), typeof(ContentSizeFitter));
                _historyTextObj.transform.SetParent(bg);
                _historyTextObj.transform.localPosition = new Vector3(230, 30);
                _historyTextObj.transform.localScale = Vector3.one;
                var fitter = _historyTextObj.GetComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                _historyText = _historyTextObj.GetComponent<Text>();
                try { _historyText.font = LocalizedTextManager.current_font; }
                catch
                {
                    var any = bg.GetComponentInChildren<Text>();
                    if (any != null) _historyText.font = any.font;
                }
                _historyText.fontSize = 8;
                _historyText.alignment = TextAnchor.UpperLeft;
                _historyText.color = new Color(1f, 0.95f, 0.8f);
                _historyText.horizontalOverflow = HorizontalWrapMode.Wrap;
                _historyText.verticalOverflow = VerticalWrapMode.Truncate;
                var rectTransform = _historyTextObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(180, 350);
            }
            else
            {
                _historyTextObj = _historyText.gameObject;
            }
        }
        private static void ShowHistoryText(string content)
        {
            if (_historyText != null)
            {
                _historyText.text = content;
                _historyTextObj?.SetActive(true);
            }
        }
        private static void HideHistoryText()
        {
            if (_historyTextObj != null)
            {
                _historyTextObj.SetActive(false);
            }
        }
        private static void OnHistoryButtonClick(UnitWindow window)
        {
            Actor actor = window.actor;
            if (actor == null || actor.isRekt())
            {
                ShowHistoryText("角色已失效");
                return;
            }
            long actorId = actor.getID();
            double createdTime = actor.data.created_time;
            string actorName = actor.getName();
            if (CultivationHistoryStorage.HasHistory(actorId, createdTime))
            {
                string existingHistory = CultivationHistoryStorage.Load(actorId, createdTime);
                if (!string.IsNullOrEmpty(existingHistory))
                {
                    ShowHistoryText(existingHistory);
                    return;
                }
            }
            if (!CultivationHistoryGenerator.CanGenerate())
            {
                int remaining = CultivationHistoryGenerator.GetRemainingGenerations();
                string message = remaining > 0 ? "正在生成中，请稍候..." : "本局游戏已达到生成上限（15次）";
                ShowHistoryText(message);
                return;
            }
            ShowHistoryText("正在调用AI大模型生成修仙史，请稍候...");
            CultivationHistoryGenerator.GenerateCultivationHistory(actor, (history) =>
            {
                if (!string.IsNullOrEmpty(history) && !history.StartsWith("生成失败") && !history.StartsWith("未配置") && !history.StartsWith("正在生成") && !history.StartsWith("本局游戏"))
                {
                    CultivationHistoryStorage.Save(actorId, createdTime, actorName, history);
                }
                string displayText = $"【{actorName}的修仙史】\n\n{history}";
                ShowHistoryText(displayText);
            });
        }
        private static void OnHistoryButtonRightClick()
        {
            if (_currentWindow == null) return;
            Actor actor = _currentWindow.actor;
            if (actor == null || actor.isRekt())
            {
                ShowHistoryText("角色已失效");
                return;
            }
            long actorId = actor.getID();
            double createdTime = actor.data.created_time;
            string actorName = actor.getName();
            CultivationHistoryStorage.Delete(actorId, createdTime);
            if (!CultivationHistoryGenerator.CanGenerate())
            {
                int remaining = CultivationHistoryGenerator.GetRemainingGenerations();
                string message = remaining > 0 ? "正在生成中，请稍候..." : "本局游戏已达到生成上限（15次）";
                ShowHistoryText(message);
                return;
            }
            ShowHistoryText("已删除旧文件，正在重新生成修仙史...");
            CultivationHistoryGenerator.GenerateCultivationHistory(actor, (history) =>
            {
                if (!string.IsNullOrEmpty(history) && !history.StartsWith("生成失败") && !history.StartsWith("未配置") && !history.StartsWith("正在生成") && !history.StartsWith("本局游戏"))
                {
                    CultivationHistoryStorage.Save(actorId, createdTime, actorName, history);
                }
                string displayText = $"【{actorName}的修仙史】\n\n{history}";
                ShowHistoryText(displayText);
            });
        }
        static UnitWindowCultivationHistoryPatch()
        {
            MapBox.on_world_loaded += () =>
            {
                CultivationHistoryGenerator.ResetGenerationCount();
                CultivationHistoryStorage.ResetSaveId();
            };
        }
    }
}