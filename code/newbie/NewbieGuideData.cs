using System.Collections.Generic;
using UnityEngine;
namespace xn.newbie
{
    public class NewbieGuideStep
    {
        public string Content;
        public GameObject HighlightButton; 
    }
    public static class NewbieGuideData
    {
        public static List<NewbieGuideStep> GetGuideSteps()
        {
            var tab = xn.ui.XNModTab.Tab;
            var settings = xn.ui.XNModTab.BtnModSettings?.gameObject;
            return new List<NewbieGuideStep>
            {
                new NewbieGuideStep { Content = "xn_guide_welcome" },
                new NewbieGuideStep { Content = "xn_guide_ad" },
                new NewbieGuideStep { Content = "xn_guide_aura", HighlightButton = xn.ui.XNModTab.BtnAura?.gameObject },
                new NewbieGuideStep { Content = "xn_guide_xiuzhenguo", HighlightButton = xn.ui.XNModTab.BtnXiuzhenguo?.gameObject },
                new NewbieGuideStep { Content = "xn_guide_ruins", HighlightButton = xn.ui.XNModTab.BtnRuins?.gameObject },
                new NewbieGuideStep { Content = "xn_guide_ranking", HighlightButton = xn.ui.XNModTab.BtnRanking?.gameObject },
                new NewbieGuideStep { Content = "xn_guide_search", HighlightButton = xn.ui.XNModTab.BtnSearch?.gameObject },
                new NewbieGuideStep { Content = "xn_guide_bloodline", HighlightButton = xn.ui.XNModTab.BtnBloodline?.gameObject },
                new NewbieGuideStep { Content = "xn_guide_tournament", HighlightButton = xn.ui.XNModTab.BtnTournament?.gameObject },
                new NewbieGuideStep { Content = "xn_guide_brushes" },
                new NewbieGuideStep { Content = "xn_guide_cfg_log", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_fav", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_tianyun", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_anim", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_title", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_gc", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_ambition", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_search", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_boss", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_suppress", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_balance", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_army", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_building", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_auralimit", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_broadcast", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_skin", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_sfx", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_ai", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_cfg_dashou", HighlightButton = settings },
                new NewbieGuideStep { Content = "xn_guide_end" }
            };
        }
    }
}
