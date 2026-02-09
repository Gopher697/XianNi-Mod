using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace xn.world
{
    public static class CityAuraSystem
    {
        public const string KeyAura = "xn.city.aura";
        public static bool Visible { get; private set; } = false;
        private static bool _inited;
        private static Harmony _h;
        private static int _lastAuraRefreshYear = -1;
        public static void Init()
        {
            if (_inited) return; _inited = true;
            _h = new Harmony("xn.worldbox.cityaura");
            var mCityInit = AccessTools.Method(typeof(City), "init");
            if (mCityInit != null)
                _h.Patch(mCityInit, postfix: new HarmonyMethod(typeof(CityAuraSystem), nameof(Post_City_Init)));
            var mSetText = AccessTools.Method(typeof(NameplateText), "setText",
                new[] { typeof(string), typeof(Vector3), typeof(int) });
            if (mSetText != null)
                _h.Patch(mSetText, prefix: new HarmonyMethod(typeof(CityAuraSystem), nameof(Pre_Nameplate_SetText)));
            var mUpdate = AccessTools.Method(typeof(MapBox), "Update");
            if (mUpdate != null)
                _h.Patch(mUpdate, postfix: new HarmonyMethod(typeof(CityAuraSystem), nameof(Post_MapBox_Update)));
        }
        public static void Toggle()
        {
            Visible = !Visible;
            WorldTip.showNowTop(Visible ? "tip_lingqi_on" : "tip_lingqi_off", pTranslate: true);
        }
        public static int GetAura(City c)
        {
            if (c == null) return 0;
            int aura; c.data.get(KeyAura, out aura, 0);
            return aura;
        }
        public static void Post_City_Init(City __instance)
        {
            if (__instance == null) return;
            if (xn.expand.FanjieKingdomTrait.CityHasFanjieTrait(__instance))
            {
                __instance.data.set(KeyAura, 0);
                return;
            }
            int aura; __instance.data.get(KeyAura, out aura, -1);
            if (aura < 0)
            {
                int max = xn.config.ModConfigHooks.MaxCityAura;
                if (max <= 0)
                {
                    max = 10000;
                }
                if (max > 100000000)
                {
                    max = 100000000;
                }
                if (xn.config.ModConfigHooks.EnableXiuzhenguoAuraLimit)
                {
                    int maxAura = 40000; 
                    if (__instance.kingdom != null && !__instance.kingdom.isRekt())
                    {
                        maxAura = xn.world.XiuzhenguoSystem.GetMaxAura(__instance.kingdom);
                    }
                    if (maxAura < int.MaxValue && maxAura < max)
                    {
                        max = maxAura;
                    }
                }
                int rnd = UnityEngine.Random.Range(0, max + 1);
                __instance.data.set(KeyAura, rnd);
            }
        }
        public static void Pre_Nameplate_SetText(NameplateText __instance, ref string pNewText, Vector3 pPos, ref int pAdditionalWidth)
        {
            if (!Visible || __instance == null) return;
            var meta = __instance.nano_object;
            if (meta == null) return;
            var city = meta as City;
            if (city != null)
            {
                int aura; city.data.get(KeyAura, out aura, 0);
                pNewText = pNewText + " · 灵气 " + aura;
                pAdditionalWidth += 80;
                return;
            }
            var kingdom = meta as Kingdom;
            if (kingdom != null)
            {
                int sum = SumAuraFromKingdom(kingdom);
                if (sum > 0)
                {
                    pNewText = pNewText + " · 总灵气 " + sum;
                    pAdditionalWidth += 100;
                }
                return;
            }
        }
        public static int SumAuraFromKingdom(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.cities == null) return 0;
            int sum = 0;
            for (int i = 0; i < kingdom.cities.Count; i++)
            {
                var c = kingdom.cities[i];
                if (c == null || c.isRekt()) continue;
                int aura;
                c.data.get(KeyAura, out aura, 0);
                sum += aura;
            }
            return sum;
        }
        public static void Post_MapBox_Update(MapBox __instance)
        {
            if (__instance == null) return;
            int years = xn.config.ModConfigHooks.CityAuraRefreshYears;
            if (years <= 0) return; 
            int curYear = Date.getCurrentYear();
            if (curYear <= 0) return;
            if (_lastAuraRefreshYear > 0 && curYear - _lastAuraRefreshYear < years) return;
            _lastAuraRefreshYear = curYear;
            RefreshAllCityAura(__instance);
        }
        public static void RefreshAllCityAura(MapBox map)
        {
            if (map == null) map = MapBox.instance;
            if (map == null || World.world == null || World.world.cities == null) return;
            int max = xn.config.ModConfigHooks.MaxCityAura;
            if (max <= 0) max = 10000;
            if (max > 100000000) max = 100000000;
            foreach (var c in World.world.cities)
            {
                if (c == null || c.isRekt() || c.data == null) continue;
                if (xn.expand.FanjieKingdomTrait.CityHasFanjieTrait(c))
                {
                    c.data.set(KeyAura, 0);
                    continue;
                }
                int cityMax = max;
                if (xn.config.ModConfigHooks.EnableXiuzhenguoAuraLimit)
                {
                    int maxAura = 40000; 
                    if (c.kingdom != null && !c.kingdom.isRekt())
                    {
                        maxAura = xn.world.XiuzhenguoSystem.GetMaxAura(c.kingdom);
                    }
                    if (maxAura < int.MaxValue && maxAura < cityMax)
                    {
                        cityMax = maxAura;
                    }
                }
                int rnd = UnityEngine.Random.Range(0, cityMax + 1);
                c.data.set(KeyAura, rnd);
            }
        }
    }
}