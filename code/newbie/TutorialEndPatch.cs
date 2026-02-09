using HarmonyLib;
namespace xn.newbie
{
    [HarmonyPatch(typeof(Tutorial), nameof(Tutorial.endTutorial))]
    public static class TutorialEndPatch
    {
        public static void Postfix()
        {
            NewbieGuideSystem.OnTutorialEnd();
        }
    }
}