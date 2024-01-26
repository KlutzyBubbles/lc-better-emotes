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

        private void Awake()
        {
            StaticLogger = Logger;
            StaticLogger.LogInfo("BetterEmotes loading...");
            LoadAssetBundles();
            LoadAssets();
            ConfigFile();
            Settings.keybinds = new Keybinds();
            _harmony = new Harmony("BetterEmotes");
            _harmony.PatchAll(typeof(InitGamePatch));
            _harmony.PatchAll(typeof(EmotePatch));
            _harmony.PatchAll(typeof(SignChatPatch));
            _harmony.PatchAll(typeof(EmoteKeybindPatch));
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
            EmotePatch.LegsPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/plegs.prefab"));
            EmotePatch.SignPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/Sign.prefab"));
            EmotePatch.SignUIPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/SignTextUI.prefab"));
            EmoteKeybindPatch.WheelPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>("Assets/MoreEmotes/Resources/MoreEmotesMenu.prefab");
        }

        private void ConfigFile()
        {
            Settings.enabledList = new bool[EmoteDefs.getEmoteCount() + 1];
            Settings.defaultKeyList = new string[EmoteDefs.getEmoteCount() + 1];
            Settings.defaultControllerList = new string[EmoteDefs.getEmoteCount() + 1];
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
                    Settings.defaultKeyList[emoteNumber] = Settings.validatePrefix("<Keyboard>", keyConfig.Value);
                    ConfigEntry<string> controllerConfig = Config.Bind("Emote Controller Bindings", $"{name} Button", "", $"Default controller binding for {name} emote");
                    Settings.defaultControllerList[emoteNumber] = Settings.validatePrefix("<Gamepad>", controllerConfig.Value);
                }
                ConfigEntry<bool> enabledConfig = Config.Bind("Enabled Emotes", $"Enable {name}", true, $"Toggle {name} emote key");
                Settings.enabledList[EmoteDefs.getEmoteNumber(name)] = enabledConfig.Value;
            }
            ConfigEntry<string> configEmoteKey = Config.Bind("Emote Keys", "Emote Wheel Key", "<Keyboard>/v", "Default keybind for the emote wheel");
            Settings.emoteWheelKey = Settings.validatePrefix("<Keyboard>", configEmoteKey.Value);
            ConfigEntry<string> configEmoteController = Config.Bind("Emote Controller Bindings", "Emote Wheel Button", "<Gamepad>/leftShoulder", "Default controller binding for the emote wheel");
            Settings.emoteWheelController = Settings.validatePrefix("<Gamepad>", configEmoteController.Value);
            ConfigEntry<string> configEmoteControllerMove = Config.Bind("Emote Controller Bindings", "Emote Wheel Move", "<Gamepad>/rightStick", "Default controller binding for the emote wheel movement");
            Settings.emoteWheelControllerMove = Settings.validatePrefix("<Gamepad>", configEmoteControllerMove.Value);
            ConfigEntry<float> configEmoteControllerDeadzone = Config.Bind("Emote Controller Bindings", "Emote Wheel Deadzone", 0.25f, "Default controller deadzone for emote selection");
            Settings.controllerDeadzone = Settings.validateGreaterThanEqualToZero(configEmoteControllerDeadzone.Value);

            ConfigEntry<float> configGriddySpeed = Config.Bind("Emote Settings", "Griddy Speed", 0.5f, "Speed of griddy relative to regular speed");
            Settings.griddySpeed = Settings.validateGreaterThanEqualToZero(configGriddySpeed.Value);
            ConfigEntry<float> configPrisyadkaSpeed = Config.Bind("Emote Settings", "Prisyadka Speed", 0.5f, "Speed of Prisyadka relative to regular speed");
            Settings.prisyadkaSpeed = Settings.validateGreaterThanEqualToZero(configPrisyadkaSpeed.Value);

            ConfigEntry<float> configEmoteCooldown = Config.Bind("Emote Settings", "Cooldown", 0.5f, "Time (in seconds) to wait before being able to switch emotes");
            Settings.emoteCooldown = Settings.validateGreaterThanEqualToZero(configEmoteCooldown.Value);

            ConfigEntry<float> configSignEmoteCooldown = Config.Bind("Emote Settings", "Sign Text Cooldown", 0.5f, "Time (in seconds) to wait before being able to finish typing (was hard coded into MoreEmotes)");
            Settings.signTextCooldown = Settings.validateGreaterThanEqualToZero(configSignEmoteCooldown.Value);

            ConfigEntry<bool> configEmoteStop = Config.Bind("Emote Settings", "Stop on outer", false, "Whether or not to stop emoting when mousing to outside the emote wheel");
            Settings.stopOnOuter = configEmoteStop.Value;

            ConfigEntry<float> configTraceDelay = Config.Bind("Debug Settings", "Trace Delay", 0.5f, "Time (in seconds) to wait before writing the same trace line, trace messages are very spammy");
            Settings.logDelay = Settings.validateGreaterThanEqualToZero(configTraceDelay.Value);
            ConfigEntry<bool> configDebug = Config.Bind("Debug Settings", "Debug", false, "Whether or not to enable debug log messages, bepinex also needs to be configured to show debug logs");
            Settings.debug = configDebug.Value;
            ConfigEntry<bool> configTrace = Config.Bind("Debug Settings", "Trace", false, "Whether or not to enable trace log messages, bepinex also needs to be configured to show debug logs");
            Settings.trace = configTrace.Value;
            ConfigEntry<bool> configIncompat = Config.Bind("Debug Settings", "Incompatible Things", false, "Whether or not to tell the mod there are incompatible mods, this disables things like speed changes");
            Settings.incompatibleStuff = configIncompat.Value;
        }

        public static Dictionary<string, long> lastLog = new Dictionary<string, long>();

        public static void Debug(string message)
        {
            if (Settings.debug)
            {
                StaticLogger.LogDebug($"[DEBUG] {message}");
            }
        }
        public static void Trace(string message)
        {
            if (Settings.trace)
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (lastLog.ContainsKey(message))
                {
                    long lastTime = lastLog[message];
                    if (currentTime - lastTime > Settings.logDelay * 1000)
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