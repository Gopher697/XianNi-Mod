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
                        string text = $"Protagonist {actorName} selected";
                        if (xn.config.ModConfigHooks.EnableAITextGen)
                        {
                            _ = PlayWithAIOptimization(text);
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
            private static async System.Threading.Tasks.Task PlayWithAIOptimization(string rawText)
            {
                try
                {
                    string optimizedText = await xn.voice.AITextGenerator.GenerateNaturalText(rawText, "protagonist");
                    xn.voice.AIVoiceManager.Play(optimizedText);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"[XN-Voice] AI optimization failed, using raw text: {e.Message}");
                    xn.voice.AIVoiceManager.Play(rawText);
                }
            }
        }
    }
}
