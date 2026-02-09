using System.Collections.Generic;
using UnityEngine;
namespace xn.newbie
{
    public static class NewbieGuideSystem
    {
        private static List<TutorialPage> _originalPages;
        private static bool _isOurTutorial;
        public static void Start()
        {
            var tutorial = World.world?.tutorial;
            if (tutorial == null) return;
            if (tutorial.isActive()) return;
            var tab = xn.ui.XNModTab.Tab;
            if (tutorial.pages == null)
            {
                tutorial.create();
            }
            _originalPages = new List<TutorialPage>(tutorial.pages);
            tutorial.pages.Clear();
            foreach (var step in NewbieGuideData.GetGuideSteps())
            {
                tutorial.pages.Add(new TutorialPage
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
            var tutorial = World.world?.tutorial;
            if (tutorial == null || _originalPages == null) return;
            tutorial.pages.Clear();
            tutorial.pages.AddRange(_originalPages);
            _originalPages = null;
        }
        public static bool IsOurTutorial => _isOurTutorial;
    }
}