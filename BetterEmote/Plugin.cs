using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using System;
using BetterEmote.Patches;
using BetterEmote.AssetScripts;
using BetterEmote.Utils;
using System.Collections.Generic;
using BetterEmote.Compatibility;
using System.Linq;
using RuntimeNetcodeRPCValidator;
using GameNetcodeStuff;

namespace BetterEmote
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("NicholaScott.BepInEx.RuntimeNetcodeRPCValidator", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Stoneman.LethalProgression", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.malco.lethalcompany.moreshipupgrades", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.daxcess.lcvr", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger;

        private Harmony _harmony;

        private NetcodeValidator netcodeValidator;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("BetterEmotes loading...");
            LoadAssetBundles();
            LoadAssets();
            ConfigFile();
            Settings.keybinds = new Keybinds();
            _harmony = new Harmony("BetterEmotes");
            Patcher.patchCompat(_harmony);
            _harmony.PatchAll(typeof(EmotePatch));
            _harmony.PatchAll(typeof(SignChatPatch));
            _harmony.PatchAll(typeof(EmoteKeybindPatch));
            netcodeValidator = new NetcodeValidator(PluginInfo.PLUGIN_GUID);
            netcodeValidator.PatchAll();
            netcodeValidator.BindToPreExistingObjectByBehaviour<SyncVRState, PlayerControllerB>();
            netcodeValidator.BindToPreExistingObjectByBehaviour<SignEmoteText, PlayerControllerB>();
            netcodeValidator.BindToPreExistingObjectByBehaviour<SyncAnimatorToOthers, PlayerControllerB>();
            Logger.LogInfo("BetterEmotes loaded");
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
            EmotePatch.local = new AnimatorOverrideController(EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>(Path.Combine(path, "NEWmetarig.controller")));
            EmotePatch.others = new AnimatorOverrideController(EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>(Path.Combine(path, "NEWmetarigOtherPlayers.controller")));
            CustomAudioAnimationEvent.claps[0] = EmotePatch.animationsBundle.LoadAsset<AudioClip>(Path.Combine(path, "SingleClapEmote1.wav"));
            CustomAudioAnimationEvent.claps[1] = EmotePatch.animationsBundle.LoadAsset<AudioClip>(Path.Combine(path, "SingleClapEmote2.wav"));
            LocalPlayer.LegsPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/plegs.prefab"));
            LocalPlayer.SignPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/Sign.prefab"));
            LocalPlayer.SignUIPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>(Path.Combine(path, "Resources/SignTextUI.prefab"));
            EmoteKeybindPatch.WheelPrefab = EmotePatch.animationsBundle.LoadAsset<GameObject>("Assets/MoreEmotes/Resources/MoreEmotesMenu.prefab");
        }

        private void ConfigFile()
        {
            Settings.enabledList = new bool[EmoteDefs.getEmoteCount() + 1];
            Settings.defaultInputList = new InputBind[EmoteDefs.getEmoteCount() + 1];
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
                    ConfigEntry<string> controllerConfig = Config.Bind("Emote Controller Bindings", $"{name} Button", "", $"Default controller binding for {name} emote");
                    Settings.defaultInputList[emoteNumber] = new InputBind(
                        Settings.validatePrefixes(["<Keyboard>", "<Mouse>"], "<Keyboard>", keyConfig.Value),
                        Settings.validatePrefix("<Gamepad>", controllerConfig.Value));
                }
                ConfigEntry<bool> enabledConfig = Config.Bind("Enabled Emotes", $"Enable {name}", true, $"Toggle {name} emote key");
                Settings.enabledList[EmoteDefs.getEmoteNumber(name)] = enabledConfig.Value;
            }
            ConfigEntry<string> configSignSubmitKey = Config.Bind("Emote Keys", "Sign Submit Key", Settings.signSubmitInput.keyboard, "Default keybind for the emote wheel");
            Settings.signSubmitInput.keyboard = Settings.validatePrefixes(["<Keyboard>", "<Mouse>"], "<Keyboard>", configSignSubmitKey.Value);
            ConfigEntry<string> configSignSubmitController = Config.Bind("Emote Controller Bindings", "Sign Submit Button", Settings.signSubmitInput.controller, "Default controller binding for the emote wheel");
            Settings.signSubmitInput.controller = Settings.validatePrefix("<Gamepad>", configSignSubmitController.Value);
            ConfigEntry<string> configSignCancelKey = Config.Bind("Emote Keys", "Sign Cancel Key", Settings.signCancelInput.keyboard, "Default keybind for the emote wheel");
            Settings.signCancelInput.keyboard = Settings.validatePrefixes(["<Keyboard>", "<Mouse>"], "<Keyboard>", configSignCancelKey.Value);
            ConfigEntry<string> configSignCancelController = Config.Bind("Emote Controller Bindings", "Sign Cancel Button", Settings.signCancelInput.controller, "Default controller binding for the emote wheel");
            Settings.signCancelInput.controller = Settings.validatePrefix("<Gamepad>", configSignCancelController.Value);

            ConfigEntry<string> configEmoteKey = Config.Bind("Emote Keys", "Emote Wheel Key", Settings.emoteWheelInput.keyboard, "Default keybind for the emote wheel");
            Settings.emoteWheelInput.keyboard = Settings.validatePrefixes(["<Keyboard>", "<Mouse>"], "<Keyboard>", configEmoteKey.Value);
            ConfigEntry<string> configEmoteController = Config.Bind("Emote Controller Bindings", "Emote Wheel Button", Settings.emoteWheelInput.controller, "Default controller binding for the emote wheel");
            Settings.emoteWheelInput.controller = Settings.validatePrefix("<Gamepad>", configEmoteController.Value);

            ConfigEntry<string> configEmoteNextKey = Config.Bind("Emote Keys", "Emote Wheel Next Page Key", Settings.emoteWheelNextInput.keyboard, "Default keybind for the emote wheel next page");
            Settings.emoteWheelNextInput.keyboard = Settings.validatePrefixes(["<Keyboard>", "<Mouse>"], "<Mouse>", configEmoteNextKey.Value);
            ConfigEntry<string> configEmoteNextController = Config.Bind("Emote Controller Bindings", "Emote Wheel Next Page Button", Settings.emoteWheelNextInput.controller, "Default controller binding for the emote wheel next page");
            Settings.emoteWheelNextInput.controller = Settings.validatePrefix("<Gamepad>", configEmoteNextController.Value);
            
            ConfigEntry<string> configEmotePreviousKey = Config.Bind("Emote Keys", "Emote Wheel Previous Page Key", Settings.emoteWheelPreviousInput.keyboard, "Default keybind for the emote wheel previous page");
            Settings.emoteWheelPreviousInput.keyboard = Settings.validatePrefixes(["<Keyboard>", "<Mouse>"], "<Mouse>", configEmotePreviousKey.Value);
            ConfigEntry<string> configEmotePreviousController = Config.Bind("Emote Controller Bindings", "Emote Wheel Previous Page Button", Settings.emoteWheelPreviousInput.controller, "Default controller binding for the emote wheel previous page");
            Settings.emoteWheelPreviousInput.controller = Settings.validatePrefix("<Gamepad>", configEmotePreviousController.Value);

            ConfigEntry<string> configEmoteControllerMove = Config.Bind("Emote Controller Bindings", "Emote Wheel Move", "<Gamepad>/rightStick", "Default controller binding for the emote wheel movement");
            Settings.emoteWheelMoveInput.controller = Settings.validatePrefix("<Gamepad>", configEmoteControllerMove.Value);
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
            ConfigEntry<bool> configSpeedChange = Config.Bind("Debug Settings", "Disable Speed Changed", false, "Whether or not to disable speed changes that might affect other mods");
            Settings.disableSpeedChange = configSpeedChange.Value;
            ConfigEntry<bool> configSelfEmote = Config.Bind("Debug Settings", "Disable Self Emote", false, "Whether or not to disable overriding the player model, can help with conflicting mods");
            Settings.disableModelOverride = configSelfEmote.Value;
        }

        public static Dictionary<string, long> lastLog = new Dictionary<string, long>();

        public static void Debug(string message)
        {
            if (Settings.debug)
            {
                Logger.LogDebug($"[DEBUG] {message}");
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
                        Logger.LogDebug($"[TRACE] {message}");
                        lastLog[message] = currentTime;
                    }
                } else
                {
                    Logger.LogDebug($"[TRACE] {message}");
                    lastLog.Add(message, currentTime);
                }
            }
        }
    }
}