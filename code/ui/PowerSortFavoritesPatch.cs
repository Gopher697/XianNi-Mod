using System;
using HarmonyLib;
namespace xn.ui
{
    [HarmonyPatch(typeof(WindowFavorites), "setupSortingTabs")]
    internal static class PowerSortFavoritesPatch
    {
        static readonly Comparison<Actor> CMP_POWER = CompareByPower;
        static void Postfix(WindowFavorites __instance)
        {
            var tab = __instance.sorting_tab;
            if (tab == null) return;
            SortButtonAction postShow = delegate { __instance.show(); };
            SortButtonAction setSort  = delegate { __instance.current_sort = CMP_POWER; };
            tab.tryAddButton("ui/icon/charts", "sort_by_power", postShow, setSort);
        }
        static int CompareByPower(Actor a, Actor b)
        {
            long sa = CalcPowerLong(a);
            long sb = CalcPowerLong(b);
            int diff = sb.CompareTo(sa);           
            if (diff != 0) return diff;
            return a.getID().CompareTo(b.getID()); 
        }
        static long CalcPowerLong(Actor u)
        {
            return XNPowerRanking.CalcPowerScoreLongInternal(u);
        }
    }
}