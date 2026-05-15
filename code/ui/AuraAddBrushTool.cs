using UnityEngine;
using HarmonyLib;
namespace xn.ui
{
    public static class AuraAddBrushTool
    {
        private static readonly string _currentPowerId = "xn_aura_add_brush";
        private static PowerButton _powerButton = null;
        private static bool _isDecreaseMode = false;
        public static bool IsDecreaseMode => _isDecreaseMode;
        private static AuraAddBrushInputListener _inputListener = null;
        public static void Init()
        {
            CreateAuraAddBrushPower();
            EnsureInputListener();
        }
        private static void EnsureInputListener()
        {
            if (_inputListener == null)
            {
                var go = new GameObject("XN_AuraAddBrush_InputListener");
                GameObject.DontDestroyOnLoad(go);
                _inputListener = go.AddComponent<AuraAddBrushInputListener>();
            }
        }
        private static void CreateAuraAddBrushPower()
        {
            if (AssetManager.powers.get(_currentPowerId) != null)
            {
                return;
            }
            GodPower template = AssetManager.powers.get("inspect");
            GodPower newPower;
            if (template != null)
            {
                newPower = AssetManager.powers.clone(_currentPowerId, "inspect");
                newPower.name = "btn_xn_aura_add_brush";
                newPower.path_icon = "ui/icon/lingqiadd";
                newPower.show_tool_sizes = false;
                newPower.allow_unit_selection = false;
                newPower.click_power_brush_action = OnLeftClickAction;
                newPower.click_action = null;
                newPower.click_brush_action = null;
            }
            else
            {
                newPower = new GodPower
                {
                    id = _currentPowerId,
                    name = "btn_xn_aura_add_brush",
                    path_icon = "ui/icon/lingqiadd",
                    show_tool_sizes = false,
                    allow_unit_selection = false,
                    click_power_brush_action = OnLeftClickAction
                };
                AssetManager.powers.add(newPower);
            }
        }
        public static PowerButton GetPowerButton()
        {
            if (_powerButton == null)
            {
                GodPower power = AssetManager.powers.get(_currentPowerId);
                if (power != null)
                {
                    Sprite icon = SpriteTextureLoader.getSprite("GameResources/ui/icon/lingqiadd")
                                  ?? SpriteTextureLoader.getSprite("ui/icon/lingqiadd");
                    _powerButton = NeoModLoader.General.PowerButtonCreator.CreateGodPowerButton(_currentPowerId, icon);
                }
            }
            return _powerButton;
        }
        private static string T(string key, string fallback)
        {
            string text = LocalizedTextManager.getText(key, null);
            return string.IsNullOrEmpty(text) || text == key ? fallback : text;
        }
        private static bool OnLeftClickAction(WorldTile pTile, GodPower pPower)
        {
            return ModifyAura(pTile, !_isDecreaseMode);
        }
        public static void ToggleMode()
        {
            _isDecreaseMode = !_isDecreaseMode;
            string modeText = _isDecreaseMode ? T("brush_aura_mode_decrease", "Decrease Mode") : T("brush_aura_mode_increase", "Increase Mode");
            xn.world.BroadcastSystem.Custom(string.Format(T("brush_aura_mode_switched", "Aura brush switched to: {0}"), modeText));
        }
        private static bool ModifyAura(WorldTile pTile, bool isAdd)
        {
            if (pTile == null) return false;
            var chunk = xn.world.AuraChunkSystem.TileToChunk(pTile.x, pTile.y);
            int maxAura = xn.world.AuraChunkSystem.GetChunkLimit(chunk.cx, chunk.cy);
            int currentAura = xn.world.AuraChunkSystem.GetChunkAura(chunk.cx, chunk.cy);
            string chunkName = chunk.cx + "," + chunk.cy;
            if (isAdd)
            {
                if (currentAura >= maxAura)
                {
                    xn.world.BroadcastSystem.Custom(string.Format(T("brush_aura_city_at_max", "Chunk {0} Aura has reached the limit {1}"), chunkName, maxAura));
                    return false;
                }
                int remaining = maxAura - currentAura;
                int addAmount = Random.Range(1, remaining + 1);
                xn.world.AuraChunkSystem.AddChunkAura(chunk.cx, chunk.cy, addAmount);
                int newAura = xn.world.AuraChunkSystem.GetChunkAura(chunk.cx, chunk.cy);
                xn.world.BroadcastSystem.Custom(string.Format(T("brush_aura_city_added", "Chunk {0} Aura +{1} (Current: {2}/{3})"), chunkName, newAura - currentAura, newAura, maxAura));
            }
            else
            {
                if (currentAura <= 0)
                {
                    xn.world.BroadcastSystem.Custom(string.Format(T("brush_aura_city_zero", "Chunk {0} Aura is already 0 and cannot be reduced"), chunkName));
                    return false;
                }
                int reduceAmount = Random.Range(1, currentAura + 1);
                xn.world.AuraChunkSystem.DeductChunkAura(chunk.cx, chunk.cy, reduceAmount);
                int newAura = xn.world.AuraChunkSystem.GetChunkAura(chunk.cx, chunk.cy);
                xn.world.BroadcastSystem.Custom(string.Format(T("brush_aura_city_reduced", "Chunk {0} Aura -{1} (Current: {2}/{3})"), chunkName, currentAura - newAura, newAura, maxAura));
            }
            return true;
        }
        public static bool IsOurPowerSelected()
        {
            var selectedButton = xn.access.MapBoxAccess.GetSelectedButton(World.world);
            GodPower selectedPower = xn.access.PowerButtonAccess.GetGodPower(selectedButton);
            if (selectedPower == null) return false;
            return selectedPower.id == _currentPowerId;
        }
    }
    public class AuraAddBrushInputListener : MonoBehaviour
    {
        private void Update()
        {
            if (!AuraAddBrushTool.IsOurPowerSelected()) return;
            if (Input.GetMouseButtonDown(1))
            {
                AuraAddBrushTool.ToggleMode();
            }
        }
    }
}
