using HarmonyLib;
using UnityEngine;
using UnityEngine.Profiling;
using System;
using System.Collections;
namespace xn.world
{
    internal static class GarbageCollectorSystem
    {
        private const float COOLDOWN_SECONDS = 50f;
        private const float MEMORY_CHECK_INTERVAL = 5f;
        private const long BYTES_PER_MB = 1024L * 1024L;
        private static float _lastGCTime = -COOLDOWN_SECONDS; 
        private static float _lastCheckTime;
        private static bool _isRunning;
        private static AsyncOperation _unloadOp;
        public static void Init(Harmony h)
        {
            h.Patch(
                AccessTools.Method(typeof(MapBox), nameof(MapBox.Update)),
                postfix: new HarmonyMethod(typeof(GarbageCollectorSystem), nameof(OnUpdate)));
        }
        public static void RunGC(string reason = "manual", bool unloadAssets = true)
        {
            if (_isRunning) return;
            long beforeMB = GetUsedMB();
            _isRunning = true;
            try
            {
                if (unloadAssets)
                {
                    Resources.UnloadUnusedAssets();
                }
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
                GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: false);
                ResetTimers();
                XNConsoleCleanup.ClearBroadcastLogs();
                long afterMB = GetUsedMB();
                Debug.Log($"[XN-GC] {reason}: {beforeMB}MB -> {afterMB}MB (freed {beforeMB - afterMB}MB)");
            }
            finally
            {
                _isRunning = false;
            }
        }
        public static void RunLightGC(string reason = "auto")
        {
            if (_isRunning) return;
            long beforeMB = GetUsedMB();
            _isRunning = true;
            try
            {
                GC.Collect(1, GCCollectionMode.Optimized, blocking: false, compacting: false);
                ResetTimers();
                long afterMB = GetUsedMB();
                if (beforeMB != afterMB)
                {
                    Debug.Log($"[XN-GC] {reason}: {beforeMB}MB -> {afterMB}MB");
                }
            }
            finally
            {
                _isRunning = false;
            }
        }
        public static IEnumerator RunGCAsync(string reason = "async")
        {
            if (_isRunning) yield break;
            long beforeMB = GetUsedMB();
            _isRunning = true;
            _unloadOp = Resources.UnloadUnusedAssets();
            yield return _unloadOp;
            _unloadOp = null;
            GC.Collect(0, GCCollectionMode.Optimized, blocking: false);
            yield return null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false);
            yield return null;
            GC.WaitForPendingFinalizers();
            GC.Collect(0, GCCollectionMode.Optimized, blocking: false);
            ResetTimers();
            XNConsoleCleanup.ClearBroadcastLogs();
            _isRunning = false;
            long afterMB = GetUsedMB();
            Debug.Log($"[XN-GC] {reason}: {beforeMB}MB -> {afterMB}MB (freed {beforeMB - afterMB}MB)");
        }
        private static void OnUpdate(MapBox __instance)
        {
            if (!xn.config.ModConfigHooks.EnableAutoGC) return;
            if (!Config.game_loaded || _isRunning) return;
            float now = Time.unscaledTime;
            if (now - _lastGCTime < COOLDOWN_SECONDS) return;
            if (now - _lastCheckTime < MEMORY_CHECK_INTERVAL) return;
            _lastCheckTime = now;
            long usedBytes = Profiler.GetMonoUsedSizeLong();
            long thresholdBytes = xn.config.ModConfigHooks.AutoGCThresholdMB * BYTES_PER_MB;
            if (usedBytes >= thresholdBytes)
            {
                long usedMB = usedBytes / BYTES_PER_MB;
                long thresholdMB = thresholdBytes / BYTES_PER_MB;
                Debug.Log($"[XN-GC] auto trigger: {usedMB}MB >= {thresholdMB}MB threshold");
                RunLightGC("auto");
            }
        }
        private static void ResetTimers()
        {
            float now = Time.unscaledTime;
            _lastGCTime = now;
            _lastCheckTime = now;
        }
        private static long GetUsedMB() => Profiler.GetMonoUsedSizeLong() / BYTES_PER_MB;
    }
}