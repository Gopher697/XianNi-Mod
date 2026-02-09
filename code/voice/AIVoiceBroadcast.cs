using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
namespace xn.voice
{
    public static class AIVoiceBroadcast
    {
        private static float _lastRandomBroadcastYear = 0;
        private const float RANDOM_BROADCAST_INTERVAL = 200f; 
        public static void Init(Harmony harmony)
        {
            harmony.PatchAll(typeof(Hook_MapBox_updateSimulation));
        }
        public static void OnSlaveSealSuccess(Actor slave, Actor master)
        {
            if (slave == null || master == null) return;
            string slaveName = slave.getName();
            string masterName = master.getName();
            string text = $"{slaveName}被{masterName}种下奴印";
            PlayWithOptimization(text, "slave");
        }
        public static void OnMentorshipSuccess(Actor disciple, Actor master)
        {
            if (disciple == null || master == null) return;
            string discipleName = disciple.getName();
            string masterName = master.getName();
            string text = $"{discipleName}拜{masterName}为师";
            PlayWithOptimization(text, "mentorship");
        }
        public static void OnRankingClicked()
        {
            string text = "战力排行榜";
            PlayWithOptimization(text, "ranking");
        }
        public static void OnButtonClicked(string buttonName)
        {
            PlayWithOptimization(buttonName, "button");
        }
        private static async void CheckRandomBroadcast()
        {
            if (!xn.config.ModConfigHooks.EnableAIVoice)
            {
                return;
            }
            int currentYear = Date.getCurrentYear();
            if (currentYear - _lastRandomBroadcastYear >= RANDOM_BROADCAST_INTERVAL)
            {
                _lastRandomBroadcastYear = currentYear;
                string story;
                if (xn.config.ModConfigHooks.EnableDeepSeekTextGen)
                {
                    story = await DeepSeekTextGenerator.GenerateCultivationStory();
                }
                else
                {
                    story = "修仙界风云变幻，天道无常，世事难料";
                }
                AIVoiceManager.Play(story);
            }
        }
        private static async void PlayWithOptimization(string rawText, string context)
        {
            if (!xn.config.ModConfigHooks.EnableAIVoice)
            {
                return;
            }
            try
            {
                string finalText = rawText;
                if (xn.config.ModConfigHooks.EnableDeepSeekTextGen)
                {
                    finalText = await DeepSeekTextGenerator.GenerateNaturalText(rawText, context);
                }
                AIVoiceManager.Play(finalText);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[XN-Voice] 播报失败: {e.Message}");
                AIVoiceManager.Play(rawText);
            }
        }
        [HarmonyPatch(typeof(MapBox), "updateSimulation")]
        private static class Hook_MapBox_updateSimulation
        {
            private static void Postfix()
            {
                if (MapBox.instance == null) return;
                CheckRandomBroadcast();
            }
        }
    }
}