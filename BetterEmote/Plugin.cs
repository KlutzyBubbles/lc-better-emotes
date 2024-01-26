using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using System;
using GameNetcodeStuff;
using RuntimeNetcodeRPCValidator;
using BetterEmote.Patches;
using BetterEmote.AssetScripts;
using BetterEmote.Utils;
using System.Diagnostics;
using System.Collections.Generic;

namespace BetterEmote
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("NicholaScott.BepInEx.RuntimeNetcodeRPCValidator", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource StaticLogger;

        private Harmony _harmony;

        private NetcodeValidator netcodeValidator;

        private static bool debug = true;
        private static bool trace = false;

        private void Awake()
        {
            StaticLogger = Logger;
            StaticLogger.LogInfo("BetterEmotes loading...");
            LoadAssetBundles();
            LoadAssets();
            ConfigFile();
            EmotePatch.keybinds = new Keybinds();
            _harmony = new Harmony("BetterEmotes");
            _harmony.PatchAll(typeof(InitGamePatch));
            _harmony.PatchAll(typeof(EmotePatch));
            _harmony.PatchAll(typeof(SignChatPatch));
            netcodeValidator = new NetcodeValidator(PluginInfo.PLUGIN_GUID);
            netcodeValidator.PatchAll();
            netcodeValidator.BindToPreExistingObjectByBehaviour<SignEmoteText, PlayerControllerB>();
            netcodeValidator.BindToPreExistingObjectByBehaviour<SyncAnimatorToOthers, PlayerControllerB>();
            StaticLogger.LogInfo("BetterEmotes loaded");
        }

        private void LoadAssetBundles()
        {
            string animationsBundlePatch = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes/animationsbundle");
            string animatiorBundlePatch = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes/animatorbundle");
            try
            {
                EmotePatch.animationsBundle = AssetBundle.LoadFromFile(animationsBundlePatch);
                EmotePatch.animatorBundle = AssetBundle.LoadFromFile(animatiorBundlePatch);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load AssetBundles. Make sure \"animatorsbundle\" and \"animationsbundle\" are inside the MoreEmotes folder.\nError: " + ex.Message);
            }
        }

        private void LoadAssets()
        {
            string path = "Assets/MoreEmotes";
            EmotePatch.local = EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>(Path.Combine(path, "NEWmetarig.controller"));
            EmotePatch.others = EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>(Path.Combine(path, "NEWmetarigOtherPlayers.controller"));
            CustomAudioAnimationEvent.claps[0] = EmotePatch.animationsBundle.LoadAsset<AudioClip>(Path.Combine(path, "SingleClapEmote1.wav"));
            CustomAudioAnimationEvent.claps[1] = EmotePatch.animationsBundle.LoadAsset<AudioClip>(Path.Combine(path, "SingleClapEmote2.wav"));
            //EmotePatch.SettingsPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/MoreEmotesPanel.prefab"));
            //EmotePatch.ButtonPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/MoreEmotesButton.prefab"));
            EmotePatch.LegsPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/plegs.prefab"));
            EmotePatch.SignPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/Sign.prefab"));
            EmotePatch.SignUIPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/SignTextUI.prefab"));
            EmotePatch.WheelPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>("Assets/MoreEmotes/Resources/MoreEmotesMenu.prefab");
        }

        private void ConfigFile()
        {
            EmotePatch.enabledList = new bool[EmoteDefs.getEmoteCount() + 1];
            EmotePatch.defaultKeyList = new string[EmoteDefs.getEmoteCount() + 1];
            EmotePatch.defaultControllerList = new string[EmoteDefs.getEmoteCount() + 1];
            foreach (string name in Enum.GetNames(typeof(Emote)))
            {
                if (EmoteDefs.getEmoteNumber(name) > 2)
                {
                    string defaultEmoteKey = "";
                    int emoteNumber = EmoteDefs.getEmoteNumber(name);
                    if (emoteNumber <= 10)
                    {
                        defaultEmoteKey = $"<Keyboard>/{emoteNumber % 10}";
                    }
                    ConfigEntry<string> keyConfig = Config.Bind("Emote Keys", $"{name} Key", defaultEmoteKey, $"Default keybind for {name} emote");
                    EmotePatch.defaultKeyList[emoteNumber] = keyConfig.Value.Equals("") ? "" : (keyConfig.Value.ToLower().StartsWith("<keyboard>") ? keyConfig.Value : $"<Keyboard>/{keyConfig.Value}");
                    ConfigEntry<string> controllerConfig = Config.Bind("Emote Controller Bindings", $"{name} Button", "", $"Default controller binding for {name} emote");
                    EmotePatch.defaultControllerList[emoteNumber] = controllerConfig.Value.Equals("") ? "" : (controllerConfig.Value.ToLower().StartsWith("<gamepad>") ? controllerConfig.Value : $"<Gamepad>/{controllerConfig.Value}");
                }
                ConfigEntry<bool> enabledConfig = Config.Bind("Enabled Emotes", $"Enable {name}", true, $"Toggle {name} emote key");
                EmotePatch.enabledList[EmoteDefs.getEmoteNumber(name)] = enabledConfig.Value;
            }
            ConfigEntry<string> configEmoteKey = Config.Bind("Emote Keys", "Emote Wheel Key", "<Keyboard>/v", "Default keybind for the emote wheel");
            EmotePatch.emoteWheelKey = configEmoteKey.Value.Equals("") ? "" : (configEmoteKey.Value.ToLower().StartsWith("<keyboard>") ? configEmoteKey.Value : $"<Keyboard>/{configEmoteKey.Value}");
            ConfigEntry<string> configEmoteController = Config.Bind("Emote Controller Bindings", "Emote Wheel Button", "<Gamepad>/leftShoulder", "Default controller binding for the emote wheel");
            EmotePatch.emoteWheelController = configEmoteController.Value.Equals("") ? "" : (configEmoteController.Value.ToLower().StartsWith("<gamepad>") ? configEmoteController.Value : $"<Gamepad>/{configEmoteController.Value}");
            ConfigEntry<string> configEmoteControllerMove = Config.Bind("Emote Controller Bindings", "Emote Wheel Move", "<Gamepad>/rightStick", "Default controller binding for the emote wheel movement");
            EmotePatch.emoteWheelControllerMove = configEmoteControllerMove.Value.Equals("") ? "" : (configEmoteControllerMove.Value.ToLower().StartsWith("<gamepad>") ? configEmoteControllerMove.Value : $"<Gamepad>/{configEmoteControllerMove.Value}");
            ConfigEntry<float> configEmoteControllerDeadzone = Config.Bind("Emote Controller Bindings", "Emote Wheel Deadzone", 0.25f, "Default controller deadzone for emote selection");
            EmoteWheel.controllerDeadzone = configEmoteControllerDeadzone.Value < 0 ? 0 : configEmoteControllerDeadzone.Value;

            ConfigEntry<float> configGriddySpeed = Config.Bind("Emote Settings", "Griddy Speed", 0.5f, "Speed of griddy relative to regular speed");
            EmotePatch.griddySpeed = configGriddySpeed.Value < 0 ? 0 : configGriddySpeed.Value;

            ConfigEntry<float> configEmoteCooldown = Config.Bind("Emote Settings", "Cooldown", 0.5f, "Time (in seconds) to wait before being able to switch emotes");
            EmotePatch.emoteCooldown = configEmoteCooldown.Value < 0 ? 0 : configEmoteCooldown.Value;

            ConfigEntry<bool> configEmoteStop = Config.Bind("Emote Settings", "Stop on outer", false, "Whether or not to stop emoting when mousing to outside the emote wheel");
            EmotePatch.stopOnOuter = configEmoteStop.Value;
        }

        public static Dictionary<string, long> lastLog = new Dictionary<string, long>();
        public static float logDelay = 1f;

        public static void Debug(string message)
        {
            if (debug)
            {
                StaticLogger.LogDebug($"[DEBUG] {message}");
            }
        }
        public static void Trace(string message)
        {
            if (trace)
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (lastLog.ContainsKey(message))
                {
                    long lastTime = lastLog[message];
                    if (currentTime - lastTime > logDelay * 1000)
                    {
                        StaticLogger.LogDebug($"[TRACE] {message}");
                        lastLog[message] = currentTime;
                    }
                } else
                {
                    StaticLogger.LogDebug($"[TRACE] {message}");
                    lastLog.Add(message, currentTime);
                }
            }
        }
    }
}