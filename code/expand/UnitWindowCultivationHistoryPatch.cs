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
        private const string PrefixGenerationFailed = "\u751f\u6210\u5931\u8d25";
        private const string PrefixNotConfigured = "\u672a\u914d\u7f6e";
        private const string PrefixGenerating = "\u6b63\u5728\u751f\u6210";
        private const string PrefixGameLimit = "\u672c\u5c40\u6e38\u620f";
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UnitWindow.OnEnable))]
        public static void OnEnable_Postfix(UnitWindow __instance)
        {
            Actor actor = xn.access.UnitWindowAccess.GetActor(__instance);
            if (actor == null) return;
            _currentWindow = __instance;
            if (!_initialized)
            {
                _initialized = true;
                CreateHistoryButton(__instance);
            }
            EnsureHistoryText(__instance);
            long actorId = actor.getID();
            double createdTime = xn.access.ActorAccess.GetData(actor).created_time;
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
                tipButton.textOnClick = "cultivation_history_tip";
                tipButton.textOnClickDescription = "cultivation_history_tip_desc";
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
            Actor actor = xn.access.UnitWindowAccess.GetActor(window);
            if (actor == null || actor.isRekt())
            {
                ShowHistoryText(T("cultivation_history_actor_invalid", "Actor is no longer valid"));
                return;
            }
            long actorId = actor.getID();
            double createdTime = xn.access.ActorAccess.GetData(actor).created_time;
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
                string message = remaining > 0 ? T("cultivation_history_generating", "Generating, please wait...") : T("cultivation_history_limit_reached", "This world has reached the generation limit (15 times)");
                ShowHistoryText(message);
                return;
            }
            ShowHistoryText(T("cultivation_history_calling_ai", "Calling the AI model to generate cultivation history, please wait..."));
            CultivationHistoryGenerator.GenerateCultivationHistory(actor, (history) =>
            {
                if (!IsGeneratorStatus(history))
                {
                    CultivationHistoryStorage.Save(actorId, createdTime, actorName, history);
                }
                string displayText = T("cultivation_history_title_format", "[Cultivation History of {0}]\n\n{1}", actorName, history);
                ShowHistoryText(displayText);
            });
        }
        private static void OnHistoryButtonRightClick()
        {
            if (_currentWindow == null) return;
            Actor actor = xn.access.UnitWindowAccess.GetActor(_currentWindow);
            if (actor == null || actor.isRekt())
            {
                ShowHistoryText(T("cultivation_history_actor_invalid", "Actor is no longer valid"));
                return;
            }
            long actorId = actor.getID();
            double createdTime = xn.access.ActorAccess.GetData(actor).created_time;
            string actorName = actor.getName();
            CultivationHistoryStorage.Delete(actorId, createdTime);
            if (!CultivationHistoryGenerator.CanGenerate())
            {
                int remaining = CultivationHistoryGenerator.GetRemainingGenerations();
                string message = remaining > 0 ? T("cultivation_history_generating", "Generating, please wait...") : T("cultivation_history_limit_reached", "This world has reached the generation limit (15 times)");
                ShowHistoryText(message);
                return;
            }
            ShowHistoryText(T("cultivation_history_regenerating", "Deleted the old file. Regenerating cultivation history..."));
            CultivationHistoryGenerator.GenerateCultivationHistory(actor, (history) =>
            {
                if (!IsGeneratorStatus(history))
                {
                    CultivationHistoryStorage.Save(actorId, createdTime, actorName, history);
                }
                string displayText = T("cultivation_history_title_format", "[Cultivation History of {0}]\n\n{1}", actorName, history);
                ShowHistoryText(displayText);
            });
        }
        private static bool IsGeneratorStatus(string history)
        {
            return string.IsNullOrEmpty(history)
                || history.StartsWith(PrefixGenerationFailed)
                || history.StartsWith(PrefixNotConfigured)
                || history.StartsWith(PrefixGenerating)
                || history.StartsWith(PrefixGameLimit);
        }
        static UnitWindowCultivationHistoryPatch()
        {
            xn.access.MapBoxAccess.AddWorldLoadedHandler(() =>
            {
                CultivationHistoryGenerator.ResetGenerationCount();
                CultivationHistoryStorage.ResetSaveId();
            });
        }
    }
}
