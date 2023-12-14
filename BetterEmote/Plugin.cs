using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

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
            EmotePatch.local = EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>("Assets/MoreEmotes/NEWmetarig.controller");
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
            ConfigEntry<bool> configMiddlefinger = Config.Bind<bool>("Enabled Emotes", "Enable Middlefinger", true, "TOGGLE MIDDLEFINGER EMOTE KEY");
            EmotePatch.enableMiddlefinger = configMiddlefinger.Value;
            ConfigEntry<bool> configGriddy = Config.Bind<bool>("Enabled Emotes", "Enable Griddy", true, "TOGGLE THE GRIDDY EMOTE KEY");
            EmotePatch.enableGriddy = configGriddy.Value;
            ConfigEntry<bool> configShy = Config.Bind<bool>("Enabled Emotes", "Enable Shy", true, "TOGGLE SHY EMOTE KEY");
            EmotePatch.enableShy = configShy.Value;
            ConfigEntry<bool> configClap = Config.Bind<bool>("Enabled Emotes", "Enable Clap", true, "TOGGLE CLAP EMOTE KEY");
            EmotePatch.enableClap = configClap.Value;
            ConfigEntry<bool> configSalute = Config.Bind<bool>("Enabled Emotes", "Enable Salute", true, "TOGGLE SALUTE EMOTE KEY");
            EmotePatch.enableSalute = configSalute.Value;
            ConfigEntry<bool> configTwerk = Config.Bind<bool>("Enabled Emotes", "Enable Twerk", true, "TOGGLE TWERK EMOTE KEY");
            EmotePatch.enableTwerk = configTwerk.Value;

            ConfigEntry<float> configGriddySpeed = Config.Bind<float>("Emote Settings", "Griddy Speed", 0.5f, "Speed of griddy relative to regular speed");
            EmotePatch.griddySpeed = configGriddySpeed.Value;

            ConfigEntry<float> configEmoteCooldown = Config.Bind<float>("Emote Settings", "Cooldown", 0.5f, "Time (in seconds) to wait before being able to switch emotes");
            EmotePatch.emoteCooldown = configEmoteCooldown.Value;
        }
    }
}