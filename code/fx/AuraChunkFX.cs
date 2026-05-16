using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.world;

namespace xn.fx
{
    public static class AuraChunkFX
    {
        internal const string OverlaySortingLayer = "MapOverlay";
        internal const int OverlaySortingOrder = 200;

        private static bool _initialized;
        private static Sprite _auraSprite;
        private static GameObject _root;
        private static int _lastUpdateFrame = -1;
        private static Harmony _harmony;
        private static readonly List<AuraQuad> _quads = new List<AuraQuad>();

        public static void Init()
        {
            if (_initialized)
                return;

            _initialized = true;
            GetOrCreateAuraSprite();
            EnsureRoot();
            PatchRebuildHooks();
            Rebuild();
        }

        public static void Rebuild()
        {
            EnsureRoot();

            ClearQuads();
            if (_root != null && CityAuraSystem.Visible == false)
                _root.SetActive(false);

            if (World.world == null || AuraChunkSystem.GridWidth <= 0 || AuraChunkSystem.GridHeight <= 0)
                return;

            float tileWorldSize = MeasureTileWorldSize();
            float quadScale = AuraChunkSystem.CHUNK_SIZE * tileWorldSize;

            for (int cy = 0; cy < AuraChunkSystem.GridHeight; cy++)
            {
                for (int cx = 0; cx < AuraChunkSystem.GridWidth; cx++)
                {
                    WorldTile centerTile = AuraChunkSystem.GetChunkCenterTile(cx, cy);
                    if (centerTile == null)
                        continue;

                    GameObject child = new GameObject($"XN_AuraChunkFX_{cx}_{cy}");
                    child.transform.SetParent(_root.transform, false);
                    child.transform.position = centerTile.posV3;
                    child.transform.localScale = new Vector3(quadScale, quadScale, 1f);

                    AuraQuad quad = new AuraQuad
                    {
                        cx = cx,
                        cy = cy,
                        go = child
                    };

                    SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                    renderer.sprite = GetOrCreateAuraSprite();
                    renderer.color = new Color(1f, 0.75f, 0.2f, 0f);
                    renderer.sortingLayerName = OverlaySortingLayer;
                    renderer.sortingOrder = OverlaySortingOrder;
                    renderer.enabled = false;
                    quad.spriteRenderer = renderer;

                    _quads.Add(quad);
                }
            }
        }

        public static void SetVisible(bool visible)
        {
            if (_root != null)
                _root.SetActive(visible);
        }

        private static Sprite GetOrCreateAuraSprite()
        {
            if (_auraSprite != null)
                return _auraSprite;

            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            const float noiseScale = 4f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x / (float)size * noiseScale, y / (float)size * noiseScale);
                    tex.SetPixel(x, y, new Color(1f, 0.85f + n * 0.15f, 0.3f + n * 0.2f, 1f));
                }
            }

            tex.Apply();
            _auraSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _auraSprite;
        }

        private static void EnsureRoot()
        {
            if (_root != null)
                return;

            _root = new GameObject("XN_AuraChunkFX_Root");
            _root.AddComponent<AuraChunkFXDriver>();
            _root.SetActive(false);
        }

        private static void PatchRebuildHooks()
        {
            if (_harmony != null)
                return;

            _harmony = new Harmony("xn.fx.aura_chunk_fx");
            _harmony.Patch(
                AccessTools.Method(typeof(MapBox), "generateNewMap"),
                postfix: new HarmonyMethod(typeof(AuraChunkFX), nameof(PostMapGenerated)));
            _harmony.Patch(
                AccessTools.Method(typeof(SaveManager), "loadWorld", new Type[] { typeof(string), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(AuraChunkFX), nameof(PostWorldLoaded)));
        }

        private static void PostMapGenerated()
        {
            Rebuild();
        }

        private static void PostWorldLoaded()
        {
            Rebuild();
        }

        private static void ClearQuads()
        {
            for (int i = 0; i < _quads.Count; i++)
            {
                if (_quads[i].go != null)
                    UnityEngine.Object.Destroy(_quads[i].go);
            }

            _quads.Clear();
        }

        private static float MeasureTileWorldSize()
        {
            try
            {
                WorldTile a = World.world.GetTile(0, 0);
                WorldTile b = World.world.GetTile(1, 0);
                if (a == null || b == null)
                    return 1f;

                Vector2 delta = b.pos - a.pos;
                float size = Mathf.Abs(delta.x) > 0.001f ? Mathf.Abs(delta.x) : delta.magnitude;
                return size > 0.001f ? size : 1f;
            }
            catch
            {
                return 1f;
            }
        }

        private static void UpdateVisuals()
        {
            if (_root == null)
                return;

            if (_quads.Count == 0 && AuraChunkSystem.GridWidth > 0 && AuraChunkSystem.GridHeight > 0)
                Rebuild();

            if (_quads.Count == 0)
                return;

            if (Time.frameCount == _lastUpdateFrame || Time.frameCount % 30 != 0)
                return;

            _lastUpdateFrame = Time.frameCount;

            for (int i = 0; i < _quads.Count; i++)
            {
                AuraQuad quad = _quads[i];
                if (quad.go == null)
                    continue;

                if (!quad.go.activeSelf)
                    quad.go.SetActive(true);

                int aura = AuraChunkSystem.GetChunkAura(quad.cx, quad.cy);
                int ceiling = AuraChunkSystem.GetChunkCeiling(quad.cx, quad.cy);
                float fill = ceiling > 0 ? Mathf.Clamp01((float)aura / ceiling) : 0f;
                float alpha = Mathf.Pow(fill, 0.5f) * 0.15f;
                ApplySpriteAlpha(quad, alpha);
            }
        }

        private static void ApplySpriteAlpha(AuraQuad quad, float alpha)
        {
            if (quad.spriteRenderer == null)
                return;

            if (alpha < 0.002f)
            {
                quad.spriteRenderer.enabled = false;
                return;
            }

            quad.spriteRenderer.enabled = true;
            quad.spriteRenderer.color = new Color(1f, 0.75f, 0.2f, alpha);
        }

        private sealed class AuraQuad
        {
            public int cx;
            public int cy;
            public GameObject go;
            public SpriteRenderer spriteRenderer;
        }

        private sealed class AuraChunkFXDriver : MonoBehaviour
        {
            private void Update()
            {
                AuraChunkFX.UpdateVisuals();
            }
        }
    }
}
