using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using FMOD;
using FMODUnity;
using UnityEngine;
namespace xn.expand
{
    public static class AudioManager
    {
        private static FMOD.System _fmodSystem;
        private static ChannelGroup _channelGroup;
        private static string _audioRoot;
        private static bool _initialized;
        private static List<string> _yuluFiles = new List<string>();
        private static int _lastYearCheck;
        public static void Init()
        {
            var declare = XNMain.Instance?.GetDeclaration();
            if (declare != null && !string.IsNullOrEmpty(declare.FolderPath))
            {
                _audioRoot = Path.Combine(declare.FolderPath, "GameResources", "audio");
                LoadYuluFiles();
            }
            InitFMOD();
            RegisterEvents();
        }
        private static void InitFMOD()
        {
            if (_initialized) return;
            var result = RuntimeManager.StudioSystem.getCoreSystem(out _fmodSystem);
            if (result != RESULT.OK) return;
            result = _fmodSystem.getMasterChannelGroup(out _channelGroup);
            if (result != RESULT.OK) return;
            _initialized = true;
        }
        private static void LoadYuluFiles()
        {
            string yuluPath = Path.Combine(_audioRoot, "yulu");
            if (!Directory.Exists(yuluPath)) return;
            var files = Directory.GetFiles(yuluPath, "*.mp3");
            _yuluFiles.AddRange(files);
        }
        private static void RegisterEvents()
        {
            MapBox.on_world_loaded += OnWorldLoaded;
            var h = new Harmony("xn.expand.audiomanager");
            h.PatchAll(typeof(Patch_ConfigSwitch));
            h.PatchAll(typeof(Patch_MapBoxUpdate));
        }
        private static void OnWorldLoaded()
        {
            Play("wizard/welcome.mp3");
        }
        public static void Play(string relativePath, bool ignoreSwitch = false)
        {
            if (!ignoreSwitch && !xn.config.ModConfigHooks.EnableMcSelectSfx) return;
            if (!_initialized || string.IsNullOrEmpty(_audioRoot)) return;
            string fullPath = Path.Combine(_audioRoot, relativePath);
            if (!File.Exists(fullPath)) return;
            Sound sound;
            var result = _fmodSystem.createSound(fullPath, MODE.DEFAULT, out sound);
            if (result != RESULT.OK) return;
            Channel channel;
            result = _fmodSystem.playSound(sound, _channelGroup, false, out channel);
            if (result != RESULT.OK)
            {
                sound.release();
                return;
            }
            float vol = PlayerConfig.getIntValue("volume_sound_effects") / 100f
                      * PlayerConfig.getIntValue("volume_master_sound") / 100f;
            channel.setVolume(vol);
        }
        public static void PlayMcSuccess()
        {
            Play("wizard/mcsuccess.mp3");
        }
        public static void PlayPaihangbang()
        {
            Play("wizard/paihangbang.mp3");
        }
        [HarmonyPatch(typeof(xn.config.ModConfigHooks), nameof(xn.config.ModConfigHooks.OnMcSelectSfxSwitchChanged))]
        private static class Patch_ConfigSwitch
        {
            private static void Postfix(bool v)
            {
                if (v)
                    Play("wizard/open.mp3", ignoreSwitch: true);
                else
                    Play("wizard/close.mp3", ignoreSwitch: true);
            }
        }
        [HarmonyPatch(typeof(MapBox), "Update")]
        private static class Patch_MapBoxUpdate
        {
            private static void Postfix()
            {
                if (!xn.config.ModConfigHooks.EnableMcSelectSfx) return;
                if (!Config.game_loaded || _yuluFiles.Count == 0) return;
                int currentYear = Date.getCurrentYear();
                if (_lastYearCheck == 0)
                {
                    _lastYearCheck = currentYear;
                    return;
                }
                if (currentYear >= _lastYearCheck + 100)
                {
                    _lastYearCheck = currentYear;
                    var file = _yuluFiles.GetRandom();
                    var relativePath = file.Replace(_audioRoot + Path.DirectorySeparatorChar, "").Replace('\\', '/');
                    Play(relativePath);
                }
            }
        }
    }
}