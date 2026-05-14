using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
namespace xn.voice
{
    public static class AIVoiceBroadcast
    {
        private static float _lastRandomBroadcastYear = 0;
        private const float RANDOM_BROADCAST_INTERVAL = 200f; 
        private const string FALLBACK_CULTIVATION_STORY = "Across the cultivation world, Dao hearts stir, old vows deepen, and each cultivator's nature slowly reveals the road ahead.";
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
                    Actor actor = PickRandomCultivatingSapientActor();
                    story = actor != null
                        ? await AITextGenerator.GenerateCultivationStory(actor)
                        : FALLBACK_CULTIVATION_STORY;
                }
                else
                {
                    story = FALLBACK_CULTIVATION_STORY;
                }
                AIVoiceManager.Play(story);
            }
        }
        private static Actor PickRandomCultivatingSapientActor()
        {
            if (MapBox.instance == null || MapBox.instance.units == null)
                return null;

            var list = MapBox.instance.units.getSimpleList();
            if (list == null || list.Count == 0)
                return null;

            var candidates = new List<Actor>();
            for (int i = 0; i < list.Count; i++)
            {
                Actor actor = list[i];
                if (actor == null || !actor.isAlive() || !actor.isSapient())
                    continue;

                xn.access.ActorAccess.GetData(actor).get("xn.stat.xiuwei", out long xiuwei, 0L);
                if (xiuwei > 0)
                    candidates.Add(actor);
            }

            if (candidates.Count == 0)
                return null;

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
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
