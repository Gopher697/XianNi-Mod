using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.access;
using xn.world;

namespace xn.fx
{
    public static class LingshiVeinSparkle
    {
        private const string BehaviourId = "xn_lingshi_vein_sparkle";
        private const int MaxSparklesPerTick = 3;
        private static bool _patched;
        private static bool _registered;
        private static Sprite _sparkleSprite;
        private static readonly List<Building> _scratchVeins = new List<Building>();

        public static void Init(Harmony harmony)
        {
            if (!_patched)
            {
                harmony.Patch(
                    AccessTools.Method(typeof(WorldBehaviourLibrary), "init"),
                    postfix: new HarmonyMethod(typeof(LingshiVeinSparkle), nameof(PostWorldBehaviourLibraryInit)));
                _patched = true;
            }

            RegisterIfNeeded();
        }

        private static void PostWorldBehaviourLibraryInit(WorldBehaviourLibrary __instance)
        {
            RegisterIfNeeded(__instance);
        }

        private static void RegisterIfNeeded(WorldBehaviourLibrary library = null)
        {
            if (_registered)
                return;

            WorldBehaviourLibrary lib = library ?? AssetManager.world_behaviours;
            if (lib == null)
                return;

            if (lib.get(BehaviourId) != null)
            {
                _registered = true;
                return;
            }

            WorldBehaviourAsset asset = new WorldBehaviourAsset
            {
                id = BehaviourId,
                interval = 6f,
                interval_random = 3f,
                enabled = true,
                stop_when_world_on_pause = true,
                action = SparkleVeins
            };

            lib.add(asset);
            asset.manager = new WorldBehaviour(asset);
            _registered = true;
        }

        private static Sprite GetOrCreateSprite()
        {
            if (_sparkleSprite != null)
                return _sparkleSprite;

            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2(7.5f, 7.5f);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / 7.5f;
                    float falloff = Mathf.Clamp01(1f - distance);
                    float core = falloff * falloff * falloff;
                    float alpha = falloff * falloff;
                    Color color = alpha <= 0f
                        ? Color.clear
                        : new Color(0.25f + core * 0.75f, 0.85f + core * 0.15f, 1f, alpha);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            _sparkleSprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
            _sparkleSprite.name = "XN_BlueVeinSparkle";
            return _sparkleSprite;
        }

        private static void SparkleVeins()
        {
            if (World.world == null || World.world.buildings == null)
                return;

            var list = World.world.buildings.getSimpleList();
            if (list == null || list.Count == 0)
                return;

            _scratchVeins.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Building building = list[i];
                if (building == null || !building.isAlive() || building.current_tile == null)
                    continue;

                BuildingAsset asset = BuildingAccess.GetAsset(building);
                if (asset == null || asset.id != LingshiVeinAssets.ID)
                    continue;

                _scratchVeins.Add(building);
            }

            int count = Mathf.Min(MaxSparklesPerTick, _scratchVeins.Count);
            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(i, _scratchVeins.Count);
                Building selected = _scratchVeins[index];
                _scratchVeins[index] = _scratchVeins[i];
                _scratchVeins[i] = selected;
                SpawnSparkle(selected);
            }

            _scratchVeins.Clear();
        }

        private static void SpawnSparkle(Building vein)
        {
            if (vein == null || vein.current_tile == null)
                return;

            Sprite sprite = GetOrCreateSprite();
            if (sprite == null)
                return;

            GameObject sparkle = new GameObject("XN_VeinSparkle");
            sparkle.transform.position = vein.current_tile.posV3;

            SpriteRenderer renderer = sparkle.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = AuraChunkFX.OverlaySortingLayer;
            renderer.sortingOrder = AuraChunkFX.OverlaySortingOrder + 1;
            renderer.color = Color.white;

            VeinSparkleParticle particle = sparkle.AddComponent<VeinSparkleParticle>();
            particle.Init(renderer);
        }

        private sealed class VeinSparkleParticle : MonoBehaviour
        {
            private const float Duration = 0.8f;
            private SpriteRenderer _renderer;
            private float _age;

            public void Init(SpriteRenderer renderer)
            {
                _renderer = renderer;
                transform.localScale = Vector3.one * 0.4f;
            }

            private void Update()
            {
                _age += Time.deltaTime;
                float t = Mathf.Clamp01(_age / Duration);
                transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 0.7f, t);

                if (_renderer != null)
                {
                    Color color = _renderer.color;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    _renderer.color = color;
                }

                if (_age >= Duration)
                    UnityEngine.Object.Destroy(gameObject);
            }
        }
    }
}
