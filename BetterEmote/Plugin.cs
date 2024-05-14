using BepInEx;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using System;
using BetterEmote.Patches;
using BetterEmote.AssetScripts;
using BetterEmote.Utils;
using System.Collections.Generic;
using BetterEmote.Compatibility;
using RuntimeNetcodeRPCValidator;
using GameNetcodeStuff;
using BetterEmote.Netcode;

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
        private static readonly string AssetPath = "Assets/MoreEmotes";
        public static new ManualLogSource Logger;
        public static RuntimeAnimatorController humanoidAnimatorController;
        public static Avatar humanoidAvatar;
        public static GameObject humanoidSkeletonPrefab;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loading...");

            loadAssetBundles();

            Settings.LoadFromConfig(Config);
            Settings.Keybinds = new Keybinds();

            // Patches
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            Patcher.patchCompat(harmony);
            harmony.PatchAll(typeof(EmotePatch));
            harmony.PatchAll(typeof(SignChatPatch));
            harmony.PatchAll(typeof(RoundPatch));
            harmony.PatchAll(typeof(EmoteKeybindPatch));

            // Netcode
            NetcodeValidator validator = new NetcodeValidator(PluginInfo.PLUGIN_GUID);
            validator.PatchAll();
            validator.BindToPreExistingObjectByBehaviour<SyncVRState, PlayerControllerB>();
            validator.BindToPreExistingObjectByBehaviour<SignEmoteText, PlayerControllerB>();
            validator.BindToPreExistingObjectByBehaviour<SyncAnimatorToOthers, PlayerControllerB>();

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");
        }

        public static AnimationClip temp;

        private void loadAssetBundles()
        {
            string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes");
            string animatiorBundlePath = Path.Combine(basePath, "animatorbundle");
            string animationsBundlePath = Path.Combine(basePath, "animationsbundle");
            string customBundlePath = Path.Combine(basePath, "betteremotes");
            try
            {
                loadAnimatorControllers(AssetBundle.LoadFromFile(animatiorBundlePath));
                loadAnimationObjects(AssetBundle.LoadFromFile(animationsBundlePath));
                loadAnimationClips(AssetBundle.LoadFromFile(customBundlePath));
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load AssetBundles. Make sure \"animatorsbundle\", \"animationsbundle\" and \"betteremotes\" are inside the BetterEmotes folder.\nError: " + ex.Message);
            }
        }
        
        private void loadAnimationClips(AssetBundle bundle)
        {
            try
            {
                humanoidAnimatorController = bundle.LoadAsset<RuntimeAnimatorController>("humanoid_animator_controller");
                humanoidAvatar = bundle.LoadAsset<Avatar>("humanoid_avatar");
                humanoidSkeletonPrefab = bundle.LoadAsset<GameObject>("humanoid_skeleton");

                Animator animator = humanoidSkeletonPrefab.GetComponentInChildren<Animator>();
                if (animator == null)
                    animator = humanoidSkeletonPrefab.AddComponent<Animator>();

                if (humanoidAnimatorController == null)
                    Logger.LogError("Failed to load humanoid animator controller from asset bundle: misc");
                if (humanoidAvatar == null)
                    Logger.LogError("Failed to load humanoid avatar from asset bundle: misc");
                if (humanoidSkeletonPrefab == null)
                    Logger.LogError("Failed to load humanoid skeleton prefab from asset bundle: misc");
                temp = bundle.LoadAsset<AnimationClip>(Path.Combine("Assets/AnimationClip", "WalkArmsOut.anim"));
            }
            catch
            {
                Logger.LogError("Failed to load misc Asset Bundle.");
            }
        }

        private void loadAnimatorControllers(AssetBundle bundle)
        {
            EmotePatch.local = new AnimatorOverrideController(bundle.LoadAsset<RuntimeAnimatorController>(Path.Combine(AssetPath, "NEWmetarig.controller")));
            EmotePatch.others = new AnimatorOverrideController(bundle.LoadAsset<RuntimeAnimatorController>(Path.Combine(AssetPath, "NEWmetarigOtherPlayers.controller")));
        }

        private void loadAnimationObjects(AssetBundle bundle)
        {
            CustomAudioAnimationEvent.claps[0] = bundle.LoadAsset<AudioClip>(Path.Combine(AssetPath, "SingleClapEmote1.wav"));
            CustomAudioAnimationEvent.claps[1] = bundle.LoadAsset<AudioClip>(Path.Combine(AssetPath, "SingleClapEmote2.wav"));
            LocalPlayer.LegsPrefab = bundle.LoadAsset<GameObject>(Path.Combine(AssetPath, "Resources/plegs.prefab"));
            LocalPlayer.SignPrefab = bundle.LoadAsset<GameObject>(Path.Combine(AssetPath, "Resources/Sign.prefab"));
            LocalPlayer.SignUIPrefab = bundle.LoadAsset<GameObject>(Path.Combine(AssetPath, "Resources/SignTextUI.prefab"));
            EmoteKeybindPatch.WheelPrefab = bundle.LoadAsset<GameObject>(Path.Combine(AssetPath, "Resources/MoreEmotesMenu.prefab"));
        }

        public static Dictionary<string, long> lastLog = new Dictionary<string, long>();

        public static void Debug(string message)
        {
            if (Settings.Debug)
            {
                Logger.LogDebug($"[DEBUG] {message}");
            }
        }
        public static void Trace(string message)
        {
            if (Settings.Trace)
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (lastLog.ContainsKey(message))
                {
                    long lastTime = lastLog[message];
                    if (currentTime - lastTime > Settings.LogDelay * 1000)
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