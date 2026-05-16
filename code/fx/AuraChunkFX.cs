using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.access;
using xn.world;

namespace xn.fx
{
    public static class AuraChunkFX
    {
        private const string TexturePath = "effects/aura/xn_aura_amber";
        private static readonly string[] ShaderCandidates =
        {
            "Legacy Shaders/Particles/Alpha Blended",
            "Particles/Standard Unlit",
            "Sprites/Default"
        };

        private static bool _initialized;
        private static bool _useFrameFallback;
        private static Shader _shader;
        private static Material _material;
        private static Sprite[] _frames;
        private static GameObject _root;
        private static Mesh _mesh;
        private static float _scrollOffset;
        private static int _lastUpdateFrame = -1;
        private static float _nextFrameTime;
        private static int _frameIndex;
        private static Harmony _harmony;
        private static readonly List<AuraQuad> _quads = new List<AuraQuad>();

        public static void Init()
        {
            if (_initialized)
                return;

            _initialized = true;
            ProbeShader();
            LoadTextureFrames();

            if (_useFrameFallback && (_frames == null || _frames.Length == 0))
            {
                Debug.LogWarning("[XN] AuraChunkFX: amber texture frames not found, overlay disabled.");
                return;
            }

            if (!_useFrameFallback)
                PrepareMaterial();

            EnsureRoot();
            PatchRebuildHooks();
            Rebuild();
        }

        public static void Rebuild()
        {
            if (_root == null)
                return;

            ClearQuads();
            EnsureMesh();

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
                        go = child,
                        mpb = new MaterialPropertyBlock()
                    };

                    if (_useFrameFallback)
                    {
                        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                        renderer.sprite = _frames != null && _frames.Length > 0 ? _frames[0] : null;
                        renderer.color = new Color(1f, 0.75f, 0.2f, 0f);
                        quad.spriteRenderer = renderer;
                    }
                    else
                    {
                        MeshFilter filter = child.AddComponent<MeshFilter>();
                        filter.sharedMesh = _mesh;
                        MeshRenderer renderer = child.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = _material;
                        renderer.enabled = false;
                        quad.meshRenderer = renderer;
                    }

                    _quads.Add(quad);
                }
            }
        }

        private static void ProbeShader()
        {
            foreach (string candidate in ShaderCandidates)
            {
                Shader shader = Shader.Find(candidate);
                if (shader == null)
                    continue;

                _shader = shader;
                _useFrameFallback = false;
                return;
            }

            _useFrameFallback = true;
        }

        private static void LoadTextureFrames()
        {
            _frames = SpriteTextureLoader.getSpriteList(TexturePath);
        }

        private static void PrepareMaterial()
        {
            _material = new Material(_shader);
            Texture texture = null;

            if (_frames != null && _frames.Length > 0 && _frames[0] != null)
                texture = _frames[0].texture;

            if (texture == null)
            {
                Texture2D placeholder = new Texture2D(1, 1);
                placeholder.SetPixel(0, 0, new Color(1f, 0.75f, 0.2f, 0.18f));
                placeholder.Apply();
                texture = placeholder;
                Debug.Log("[XN] AuraChunkFX: amber texture not found, using placeholder");
            }

            _material.SetTexture("_MainTex", texture);
        }

        private static void EnsureRoot()
        {
            if (_root != null)
                return;

            _root = new GameObject("XN_AuraChunkFX_Root");
            UnityEngine.Object.DontDestroyOnLoad(_root);
            _root.AddComponent<AuraChunkFXDriver>();
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

        private static void EnsureMesh()
        {
            if (_mesh != null)
                return;

            _mesh = new Mesh();
            _mesh.name = "XN_AuraChunkFX_Quad";
            _mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            _mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            _mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            _mesh.RecalculateBounds();
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

            if (!_useFrameFallback && _material != null)
            {
                _scrollOffset += Time.deltaTime * 0.02f;
                _material.SetTextureOffset("_MainTex", new Vector2(_scrollOffset, _scrollOffset * 0.7f));
            }

            if (_useFrameFallback)
                UpdateFallbackFrame();

            if (Time.frameCount == _lastUpdateFrame || Time.frameCount % 30 != 0)
                return;

            _lastUpdateFrame = Time.frameCount;

            Camera camera = MapBoxAccess.GetCamera(World.world);
            bool hiddenByZoom = camera != null && camera.orthographicSize > 40f;
            for (int i = 0; i < _quads.Count; i++)
            {
                AuraQuad quad = _quads[i];
                if (quad.go == null)
                    continue;

                if (hiddenByZoom)
                {
                    quad.go.SetActive(false);
                    continue;
                }

                if (!quad.go.activeSelf)
                    quad.go.SetActive(true);

                int aura = AuraChunkSystem.GetChunkAura(quad.cx, quad.cy);
                float alpha = Mathf.Clamp01(aura / 120000f) * 0.12f;
                if (_useFrameFallback)
                    ApplySpriteAlpha(quad, alpha);
                else
                    ApplyMeshAlpha(quad, alpha);
            }
        }

        private static void UpdateFallbackFrame()
        {
            if (_frames == null || _frames.Length == 0 || Time.time < _nextFrameTime)
                return;

            _nextFrameTime = Time.time + 0.15f;
            _frameIndex++;
            Sprite frame = _frames[_frameIndex % _frames.Length];
            for (int i = 0; i < _quads.Count; i++)
            {
                if (_quads[i].spriteRenderer != null)
                    _quads[i].spriteRenderer.sprite = frame;
            }
        }

        private static void ApplyMeshAlpha(AuraQuad quad, float alpha)
        {
            if (quad.meshRenderer == null)
                return;

            if (alpha < 0.005f)
            {
                quad.meshRenderer.enabled = false;
                return;
            }

            quad.meshRenderer.enabled = true;
            Color color = new Color(1f, 0.75f, 0.2f, alpha);
            quad.mpb.SetColor("_Color", color);
            quad.mpb.SetColor("_TintColor", color);
            quad.meshRenderer.SetPropertyBlock(quad.mpb);
        }

        private static void ApplySpriteAlpha(AuraQuad quad, float alpha)
        {
            if (quad.spriteRenderer == null)
                return;

            if (alpha < 0.005f)
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
            public MeshRenderer meshRenderer;
            public SpriteRenderer spriteRenderer;
            public MaterialPropertyBlock mpb;
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
