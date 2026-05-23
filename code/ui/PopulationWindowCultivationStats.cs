using HarmonyLib;
using System;
using System.Collections.Generic;

namespace xn.ui
{
    internal static class PopulationWindowCultivationStats
    {
        internal static void Init(Harmony h)
        {
            Patch(h, typeof(ClanWindow), nameof(Post_ClanWindow_showStatsRows));
            Patch(h, typeof(FamilyWindow), nameof(Post_FamilyWindow_showStatsRows));
            Patch(h, typeof(CultureWindow), nameof(Post_CultureWindow_showStatsRows));
            Patch(h, typeof(LanguageWindow), nameof(Post_LanguageWindow_showStatsRows));
            Patch(h, typeof(ReligionWindow), nameof(Post_ReligionWindow_showStatsRows));
            Patch(h, typeof(SubspeciesWindow), nameof(Post_SubspeciesWindow_showStatsRows));
            Patch(h, typeof(AllianceWindow), nameof(Post_AllianceWindow_showStatsRows));
            Patch(h, typeof(ArmyWindow), nameof(Post_ArmyWindow_showStatsRows));
            Patch(h, typeof(WarWindow), nameof(Post_WarWindow_showStatsRows));
        }

        public static void Post_ClanWindow_showStatsRows(ClanWindow __instance)
        {
            var clan = SelectedMetas.selected_clan;
            if (clan == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, ActorsWhere(actor => actor.clan == clan), CultivationInfoRows.ClanRows);
        }

        public static void Post_FamilyWindow_showStatsRows(FamilyWindow __instance)
        {
            var family = SelectedMetas.selected_family;
            if (family == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, ActorsWhere(actor => actor.family == family), CultivationInfoRows.FamilyRows);
        }

        public static void Post_CultureWindow_showStatsRows(CultureWindow __instance)
        {
            var culture = SelectedMetas.selected_culture;
            if (culture == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, ActorsWhere(actor => actor.culture == culture), CultivationInfoRows.CultureRows);
        }

        public static void Post_LanguageWindow_showStatsRows(LanguageWindow __instance)
        {
            var language = SelectedMetas.selected_language;
            if (language == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, ActorsWhere(actor => actor.language == language), CultivationInfoRows.LanguageRows);
        }

        public static void Post_ReligionWindow_showStatsRows(ReligionWindow __instance)
        {
            var religion = SelectedMetas.selected_religion;
            if (religion == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, ActorsWhere(actor => actor.religion == religion), CultivationInfoRows.ReligionRows);
        }

        public static void Post_SubspeciesWindow_showStatsRows(SubspeciesWindow __instance)
        {
            var subspecies = SelectedMetas.selected_subspecies;
            if (subspecies == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, ActorsWhere(actor => actor.subspecies == subspecies), CultivationInfoRows.SubspeciesRows);
        }

        public static void Post_AllianceWindow_showStatsRows(AllianceWindow __instance)
        {
            var alliance = SelectedMetas.selected_alliance;
            if (alliance == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, alliance.getUnits(), CultivationInfoRows.AllianceRows);
        }

        public static void Post_ArmyWindow_showStatsRows(ArmyWindow __instance)
        {
            var army = SelectedMetas.selected_army;
            if (army == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, ActorsWhere(actor => actor.army == army), CultivationInfoRows.ArmyRows);
        }

        public static void Post_WarWindow_showStatsRows(WarWindow __instance)
        {
            var war = SelectedMetas.selected_war;
            if (war == null || __instance == null) return;
            CultivationInfoRows.Show(__instance, war.getUnits(), CultivationInfoRows.WarRows);
        }

        private static void Patch(Harmony h, Type windowType, string postfixName)
        {
            var method = AccessTools.Method(windowType, "showStatsRows");
            var postfix = AccessTools.Method(typeof(PopulationWindowCultivationStats), postfixName);
            if (method == null || postfix == null) return;
            h.Patch(method, postfix: new HarmonyMethod(postfix));
        }

        private static IEnumerable<Actor> ActorsWhere(Func<Actor, bool> predicate)
        {
            if (World.world == null || World.world.units == null) yield break;
            var actors = World.world.units.getSimpleList();
            if (actors == null) yield break;
            for (int i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                if (actor != null && predicate(actor)) yield return actor;
            }
        }
    }
}
