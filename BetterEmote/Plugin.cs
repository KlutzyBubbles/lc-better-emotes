using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using System;

namespace BetterEmote
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource StaticLogger;

        private Harmony _harmony;

        private void Awake()
        {
            StaticLogger = Logger;
            StaticLogger.LogInfo("BetterEmotes loading...");
            EmotePatch.animationsBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes/animationsbundle"));
            EmotePatch.animatorBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes/animatorbundle"));
            EmotePatch.local = new AnimatorOverrideController(EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>("Assets/MoreEmotes/NEWmetarig.controller"));
            EmotePatch.others = EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>("Assets/MoreEmotes/NEWmetarigOtherPlayers.controller");
            CustomAudioAnimationEvent.claps[0] = EmotePatch.animationsBundle.LoadAsset<AudioClip>("Assets/MoreEmotes/SingleClapEmote1.wav");
            CustomAudioAnimationEvent.claps[1] = EmotePatch.animationsBundle.LoadAsset<AudioClip>("Assets/MoreEmotes/SingleClapEmote2.wav");
            ConfigFile();
            _harmony = new Harmony("BetterEmotes");
            _harmony.PatchAll(typeof(InitGamePatch));
            _harmony.PatchAll(typeof(EmotePatch));
            StaticLogger.LogInfo("BetterEmotes loaded");
        }

        private void ConfigFile()
        {
            EmotePatch.enabledList = new bool[Enum.GetNames(typeof(EmotePatch.Emotes)).Length + 1];
            foreach (string name in Enum.GetNames(typeof(EmotePatch.Emotes)))
            {
                ConfigEntry<bool> config = Config.Bind("Enabled Emotes", $"Enable {name}", true, $"Toggle {name} emote key");
                EmotePatch.enabledList[(int)Enum.Parse(typeof(EmotePatch.Emotes), name)] = config.Value;
            }

            ConfigEntry<float> configGriddySpeed = Config.Bind("Emote Settings", "Griddy Speed", 0.5f, "Speed of griddy relative to regular speed");
            EmotePatch.griddySpeed = configGriddySpeed.Value;

            ConfigEntry<float> configEmoteCooldown = Config.Bind("Emote Settings", "Cooldown", 0.5f, "Time (in seconds) to wait before being able to switch emotes");
            EmotePatch.emoteCooldown = configEmoteCooldown.Value;

            ConfigEntry<bool> configEmoteStop = Config.Bind("Emote Settings", "Stop on outer", false, "Whether or not to stop emoting when mousing to outside the emote wheel");
            EmotePatch.stopOnOuter = configEmoteStop.Value;
        }
    }
}