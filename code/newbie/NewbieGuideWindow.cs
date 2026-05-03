using System.Collections.Generic;
using UnityEngine;
using xn.access;
namespace xn.newbie
{
    public static class NewbieGuideSystem
    {
        private static List<TutorialPage> _originalPages;
        private static bool _isOurTutorial;
        public static void Start()
        {
            var tutorial = MapBoxAccess.GetTutorial(World.world);
            if (tutorial == null) return;
            if (TutorialAccess.IsActive(tutorial)) return;
            var tab = xn.ui.XNModTab.Tab;
            var pages = TutorialAccess.GetPages(tutorial);
            if (pages == null)
            {
                TutorialAccess.Create(tutorial);
                pages = TutorialAccess.GetPages(tutorial);
                if (pages == null) return;
            }
            _originalPages = new List<TutorialPage>(pages);
            pages.Clear();
            foreach (var step in NewbieGuideData.GetGuideSteps())
            {
                pages.Add(new TutorialPage
                {
                    text = step.Content,
                    wait = 0.3f,
                    object1 = step.HighlightButton 
                });
            }
            _isOurTutorial = true;
            tutorial.startTutorial();
            if (tab != null)
            {
                tab.showTab(null);
            }
        }
        public static void OnTutorialEnd()
        {
            if (!_isOurTutorial) return;
            _isOurTutorial = false;
            var tutorial = MapBoxAccess.GetTutorial(World.world);
            if (tutorial == null || _originalPages == null) return;
            var pages = TutorialAccess.GetPages(tutorial);
            if (pages == null) return;
            pages.Clear();
            pages.AddRange(_originalPages);
            _originalPages = null;
        }
        public static bool IsOurTutorial => _isOurTutorial;
    }
}
