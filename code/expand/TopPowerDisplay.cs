using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace xn.expand
{
    public static class TopPowerDisplay
    {
        private static Dictionary<InterestingPeopleTab, InterestingPeopleElement> _powerElements = new Dictionary<InterestingPeopleTab, InterestingPeopleElement>();
        private static List<Actor> _unit_top_power = new List<Actor>();
        private static List<long> _unit_top_power_scores = new List<long>();
        public static void Init(Harmony harmony)
        {
            var mAwake = AccessTools.Method(typeof(InterestingPeopleTab), "Awake");
            if (mAwake != null)
                harmony.Patch(mAwake, postfix: new HarmonyMethod(typeof(TopPowerDisplay), nameof(AwakePostfix)));
            var mRender = AccessTools.Method(typeof(InterestingPeopleTab), "renderElements");
            if (mRender != null)
                harmony.Patch(mRender, postfix: new HarmonyMethod(typeof(TopPowerDisplay), nameof(RenderPostfix)));
            var mClear = AccessTools.Method(typeof(InterestingPeopleTab), "clear");
            if (mClear != null)
                harmony.Patch(mClear, postfix: new HarmonyMethod(typeof(TopPowerDisplay), nameof(ClearPostfix)));
            Debug.Log("[XN.TopPowerDisplay] Initialized successfully.");
        }
        private static void AwakePostfix(InterestingPeopleTab __instance)
        {
            if (__instance.strongest == null) return;
            var original = __instance.strongest.gameObject;
            var clone = Object.Instantiate(original, original.transform.parent);
            clone.name = "top_power";
            clone.transform.SetAsFirstSibling();
            var element = clone.GetComponent<InterestingPeopleElement>();
            if (element != null)
            {
                _powerElements[__instance] = element;
                var texts = clone.GetComponentsInChildren<Text>(true);
                foreach (var text in texts)
                {
                    if (text != element._counter)
                    {
                        string title = LocalizedTextManager.getText("top_power");
                        if (string.IsNullOrEmpty(title) || title == "top_power")
                        {
                            title = "战力最强";
                        }
                        text.text = title;
                        var localizedText = text.GetComponent<LocalizedText>();
                        if (localizedText != null)
                        {
                            localizedText.autoField = false;
                            localizedText.key = "top_power";
                        }
                        break;
                    }
                }
                var images = clone.GetComponentsInChildren<Image>(true);
                var chartsSprite = SpriteTextureLoader.getSprite("ui/icon/charts");
                if (chartsSprite == null)
                    chartsSprite = SpriteTextureLoader.getSprite("ui/icons/iconCompareStatistics");
                if (chartsSprite != null)
                {
                    foreach (var img in images)
                    {
                        if (img.gameObject == clone)
                            continue;
                        if (element._grid != null && img.transform.IsChildOf(element._grid))
                            continue;
                        if (element._element != null && img.transform.IsChildOf(element._element.transform))
                            continue;
                        var rectTransform = img.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            var size = rectTransform.sizeDelta;
                            if (size.x <= 64 && size.y <= 64 && size.x > 0 && size.y > 0)
                            {
                                img.sprite = chartsSprite;
                                break;
                            }
                        }
                        string objName = img.gameObject.name.ToLower();
                        string spriteName = img.sprite?.name?.ToLower() ?? "";
                        if (objName.Contains("icon") ||
                            spriteName.Contains("sword") ||
                            spriteName.Contains("damage") ||
                            spriteName.Contains("attack") ||
                            spriteName.Contains("iconDamage"))
                        {
                            img.sprite = chartsSprite;
                            break;
                        }
                    }
                }
            }
            clone.SetActive(false);
        }
        private static void RenderPostfix(InterestingPeopleTab __instance, IEnumerable<Actor> pList, ref IEnumerator __result)
        {
            __result = WrapRenderCoroutine(__instance, pList, __result);
        }
        private static IEnumerator WrapRenderCoroutine(InterestingPeopleTab tab, IEnumerable<Actor> pList, IEnumerator originalCoroutine)
        {
            if (_powerElements.TryGetValue(tab, out var powerElement) && powerElement != null)
            {
                _unit_top_power.Clear();
                _unit_top_power_scores.Clear();
                using (ListPool<Actor> tUnits = new ListPool<Actor>(pList))
                {
                    tUnits.RemoveAll((Actor a) => !a.isAlive() || a.asset.is_boat || !a.asset.can_be_favorited);
                    foreach (var actor in tUnits)
                    {
                        long power = xn.ui.XNPowerRanking.CalcPowerScoreLongInternal(actor);
                        int insertIndex = -1;
                        for (int i = 0; i < _unit_top_power.Count; i++)
                        {
                            if (power > _unit_top_power_scores[i] ||
                                (power == _unit_top_power_scores[i] && actor.getID() < _unit_top_power[i].getID()))
                            {
                                insertIndex = i;
                                break;
                            }
                        }
                        if (insertIndex >= 0)
                        {
                            _unit_top_power.Insert(insertIndex, actor);
                            _unit_top_power_scores.Insert(insertIndex, power);
                            if (_unit_top_power.Count > 3)
                            {
                                _unit_top_power.RemoveAt(_unit_top_power.Count - 1);
                                _unit_top_power_scores.RemoveAt(_unit_top_power_scores.Count - 1);
                            }
                        }
                        else if (_unit_top_power.Count < 3)
                        {
                            _unit_top_power.Add(actor);
                            _unit_top_power_scores.Add(power);
                        }
                    }
                }
                if (_unit_top_power.Count > 0 && _unit_top_power_scores.Count > 0 && _unit_top_power_scores[0] >= 1)
                {
                    powerElement.gameObject.SetActive(true);
                    string displayValue = FormatPowerScore(_unit_top_power_scores[0]);
                    foreach (var actor in _unit_top_power)
                    {
                        if (actor.isAlive())
                        {
                            powerElement.showMember(actor);
                            yield return new WaitForSecondsRealtime(0.025f);
                        }
                    }
                    if (powerElement._counter != null)
                    {
                        powerElement._counter.text = displayValue;
                    }
                }
                else
                {
                    powerElement.gameObject.SetActive(false);
                }
            }
            while (originalCoroutine.MoveNext())
            {
                yield return originalCoroutine.Current;
            }
        }
        private static void ClearPostfix(InterestingPeopleTab __instance)
        {
            if (_powerElements.TryGetValue(__instance, out var element) && element != null)
            {
                element.gameObject.SetActive(false);
            }
            _unit_top_power.Clear();
            _unit_top_power_scores.Clear();
        }
        private static string FormatPowerScore(long score)
        {
            if (score >= 1000000000000) 
                return (score / 1000000000000.0).ToString("F1") + "T";
            if (score >= 1000000000) 
                return (score / 1000000000.0).ToString("F1") + "B";
            if (score >= 1000000) 
                return (score / 1000000.0).ToString("F1") + "M";
            if (score >= 1000) 
                return (score / 1000.0).ToString("F1") + "K";
            return score.ToString();
        }
    }
}