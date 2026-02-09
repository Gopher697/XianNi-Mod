using UnityEngine;
using HarmonyLib;
namespace xn.ui
{
    public static class AuraAddBrushTool
    {
        private static readonly string _currentPowerId = "xn_aura_add_brush";
        private static PowerButton _powerButton = null;
        private const string KEY_CITY_AURA = "xn.city.aura";
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
        private static bool OnLeftClickAction(WorldTile pTile, GodPower pPower)
        {
            return ModifyAura(pTile, !_isDecreaseMode);
        }
        public static void ToggleMode()
        {
            _isDecreaseMode = !_isDecreaseMode;
            string modeText = _isDecreaseMode ? "减少模式" : "增加模式";
            xn.world.BroadcastSystem.Custom($"灵气笔刷切换为: {modeText}");
        }
        private static bool ModifyAura(WorldTile pTile, bool isAdd)
        {
            if (pTile == null) return false;
            City city = pTile.zone?.city;
            if (city == null || city.data == null)
            {
                return false;
            }
            int maxAura = xn.config.ModConfigHooks.MaxCityAura;
            if (maxAura <= 0) maxAura = 10000;
            int currentAura;
            city.data.get(KEY_CITY_AURA, out currentAura, 0);
            string cityName = city.data.name ?? "未知城市";
            if (isAdd)
            {
                if (currentAura >= maxAura)
                {
                    xn.world.BroadcastSystem.Custom($"城市 {cityName} 灵气已达上限 {maxAura}");
                    return false;
                }
                int remaining = maxAura - currentAura;
                int addAmount = Random.Range(1, remaining + 1);
                int newAura = currentAura + addAmount;
                if (newAura > maxAura) newAura = maxAura;
                city.data.set(KEY_CITY_AURA, newAura);
                xn.world.BroadcastSystem.Custom($"城市 {cityName} 灵气 +{addAmount} (当前: {newAura}/{maxAura})");
            }
            else
            {
                if (currentAura <= 0)
                {
                    xn.world.BroadcastSystem.Custom($"城市 {cityName} 灵气已为0，无法减少");
                    return false;
                }
                int reduceAmount = Random.Range(1, currentAura + 1);
                int newAura = currentAura - reduceAmount;
                if (newAura < 0) newAura = 0;
                city.data.set(KEY_CITY_AURA, newAura);
                xn.world.BroadcastSystem.Custom($"城市 {cityName} 灵气 -{reduceAmount} (当前: {newAura}/{maxAura})");
            }
            return true;
        }
        public static bool IsOurPowerSelected()
        {
            if (World.world == null || World.world.selected_buttons == null) return false;
            var selectedButton = World.world.selected_buttons.selectedButton;
            if (selectedButton == null || selectedButton.godPower == null) return false;
            return selectedButton.godPower.id == _currentPowerId;
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