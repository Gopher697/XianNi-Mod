using HarmonyLib;
namespace xn.ui
{
    public static class MainCharacterSelectSfx
    {
        public static void Init()
        {
            var h = new Harmony("xn.ui.mc.selectsfx");
            h.PatchAll(typeof(Patch_MakeMainSelected));
        }
        [HarmonyPatch(typeof(SelectedUnit), nameof(SelectedUnit.makeMainSelected))]
        private static class Patch_MakeMainSelected
        {
            private static bool Prefix(Actor pActor)
            {
                if (pActor == null) return true;
                if (!xn.config.ModConfigHooks.EnableMcSelectSfx)
                    return true;
                int mc;
                xn.access.ActorAccess.GetData(pActor).get(MainCharacterBrushTool.KEY_MAIN_CHARACTER, out mc, 0);
                if (xn.access.SelectedUnitAccess.GetUnitMain() != pActor)
                {
                    if (mc == 1)
                    {
                        string actorName = pActor.getName();
                        string text = $"主角{actorName}已选中";
                        if (xn.config.ModConfigHooks.EnableDeepSeekTextGen)
                        {
                            _ = PlayWithDeepSeekOptimization(text);
                        }
                        else
                        {
                            xn.voice.AIVoiceManager.Play(text);
                        }
                    }
                    else
                    {
                        pActor.makeSpawnSound(pFromUI: true);
                    }
                }
                xn.access.SelectedUnitAccess.SetUnitMain(pActor);
                return false; 
            }
            private static async System.Threading.Tasks.Task PlayWithDeepSeekOptimization(string rawText)
            {
                try
                {
                    string optimizedText = await xn.voice.DeepSeekTextGenerator.GenerateNaturalText(rawText, "主角");
                    xn.voice.AIVoiceManager.Play(optimizedText);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"[XN-Voice] DeepSeek优化失败，使用原文: {e.Message}");
                    xn.voice.AIVoiceManager.Play(rawText);
                }
            }
        }
    }
}
