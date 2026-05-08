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
            string text = $"{masterName} placed a Slave Seal on {slaveName}";
            PlayWithOptimization(text, "slave");
        }
        public static void OnMentorshipSuccess(Actor disciple, Actor master)
        {
            if (disciple == null || master == null) return;
            string discipleName = disciple.getName();
            string masterName = master.getName();
            string text = $"{discipleName} has taken {masterName} as master";
            PlayWithOptimization(text, "mentorship");
        }
        public static void OnRankingClicked()
        {
            string text = "Power Ranking";
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
                if (xn.config.ModConfigHooks.EnableAITextGen)
                {
                    story = await AITextGenerator.GenerateCultivationStory();
                }
                else
                {
                    story = "The cultivation world churns — Heaven's will is fickle, fate unpredictable.";
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
                if (xn.config.ModConfigHooks.EnableAITextGen)
                {
                    finalText = await AITextGenerator.GenerateNaturalText(rawText, context);
                }
                AIVoiceManager.Play(finalText);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[XN-Voice] Broadcast failed: {e.Message}");
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