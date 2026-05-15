using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace xn.world
{
    public static class CityAuraSystem
    {
        public static bool Visible { get; private set; } = false;

        private const int OverlayUpdateIntervalFrames = 10;
        private const int OverlaySuppressBelowAura = 100;

        private static bool _inited;
        private static Harmony _h;
        private static Canvas _canvas;
        private static readonly List<Text> _labels = new List<Text>();
        private static int _lastOverlayFrame = -1;

        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _h = new Harmony("xn.worldbox.cityaura");
        }

        public static void Toggle()
        {
            Visible = !Visible;
            if (Visible)
            {
                EnsureOverlay();
                SetOverlayVisible(true);
                UpdateOverlay(force: true);
            }
            else
            {
                SetOverlayVisible(false);
            }

            WorldTip.showNowTop(Visible ? "tip_lingqi_on" : "tip_lingqi_off", pTranslate: true);
        }

        private static void EnsureOverlay()
        {
            if (_canvas != null)
            {
                return;
            }

            GameObject root = new GameObject("XN_AuraChunkOverlay_Canvas");
            Object.DontDestroyOnLoad(root);
            _canvas = root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 32000;
            root.AddComponent<AuraChunkOverlayDriver>();
        }

        private static void SetOverlayVisible(bool visible)
        {
            if (_canvas != null)
            {
                _canvas.gameObject.SetActive(visible);
            }

            if (!visible)
            {
                for (int i = 0; i < _labels.Count; i++)
                {
                    if (_labels[i] != null)
                    {
                        _labels[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private static void UpdateOverlay(bool force)
        {
            if (!Visible)
            {
                return;
            }

            EnsureOverlay();
            if (!force && _lastOverlayFrame >= 0 && Time.frameCount - _lastOverlayFrame < OverlayUpdateIntervalFrames)
            {
                return;
            }
            _lastOverlayFrame = Time.frameCount;

            Camera camera = xn.access.MapBoxAccess.GetCamera(World.world);
            if (camera == null)
            {
                HideLabelsFrom(0);
                return;
            }

            int assigned = 0;
            int gridW = AuraChunkSystem.GridWidth;
            int gridH = AuraChunkSystem.GridHeight;
            for (int cy = 0; cy < gridH; cy++)
            {
                for (int cx = 0; cx < gridW; cx++)
                {
                    int aura = AuraChunkSystem.GetChunkAura(cx, cy);
                    if (aura < OverlaySuppressBelowAura)
                    {
                        continue;
                    }

                    WorldTile tile = AuraChunkSystem.GetChunkCenterTile(cx, cy);
                    if (tile == null)
                    {
                        continue;
                    }

                    Vector3 worldPos = tile.posV3;
                    Vector3 viewport = camera.WorldToViewportPoint(worldPos);
                    if (viewport.z < 0f || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
                    {
                        continue;
                    }

                    Text label = GetLabel(assigned);
                    label.text = aura.ToString();
                    label.rectTransform.position = camera.WorldToScreenPoint(worldPos);
                    label.gameObject.SetActive(true);
                    assigned++;
                }
            }

            HideLabelsFrom(assigned);
        }

        private static Text GetLabel(int index)
        {
            while (_labels.Count <= index)
            {
                _labels.Add(CreateLabel(_labels.Count));
            }

            return _labels[index];
        }

        private static Text CreateLabel(int index)
        {
            GameObject go = new GameObject("XN_AuraChunk_Label_" + index);
            go.transform.SetParent(_canvas.transform, false);
            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 13;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.5f, 1f, 0.75f, 0.9f);
            text.raycastTarget = false;

            RectTransform rect = text.rectTransform;
            rect.sizeDelta = new Vector2(90f, 22f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private static void HideLabelsFrom(int start)
        {
            for (int i = start; i < _labels.Count; i++)
            {
                if (_labels[i] != null)
                {
                    _labels[i].gameObject.SetActive(false);
                }
            }
        }

        private sealed class AuraChunkOverlayDriver : MonoBehaviour
        {
            private void Update()
            {
                UpdateOverlay(force: false);
            }
        }
    }
}
