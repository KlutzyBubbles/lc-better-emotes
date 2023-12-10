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
        private void Awake()
        {
            StaticLogger = Logger;
            EmotePatch.animationsBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes/animationsbundle"));
            EmotePatch.animatorBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes/animatorbundle"));
            EmotePatch.local = EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>("Assets/MoreEmotes/NEWmetarig.controller");
            EmotePatch.others = EmotePatch.animatorBundle.LoadAsset<RuntimeAnimatorController>("Assets/MoreEmotes/NEWmetarigOtherPlayers.controller");
            ConfigFile();
            _harmony = new Harmony("BetterEmotes");
            _harmony.PatchAll(typeof(EmotePatch));
            StaticLogger.LogInfo("BetterEmotes loaded");
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002178 File Offset: 0x00000378
        private void ConfigFile()
        {
            config_KeyEmote3 = Config.Bind<string>("MIDDLEFINGER", "EmoteKey", "3", "SUPPORTED KEYS A-Z | 0-9 | F1-F12 ");
            config_toggleEmote3 = Config.Bind<bool>("MIDDLEFINGER", "Enable", true, "TOGGLE MIDDLEFINGER EMOTE KEY");
            EmotePatch.keyBind_Emote3 = config_KeyEmote3.Value;
            EmotePatch.enable3 = config_toggleEmote3.Value;
            config_KeyEmote4 = Config.Bind<string>("THE GRIDDY", "EmoteKey", "6", "SUPPORTED KEYS A-Z | 0-9 | F1-F12 ");
            config_toggleEmote4 = Config.Bind<bool>("THE GRIDDY", "Enable", true, "TOGGLE THE GRIDDY EMOTE KEY");
            EmotePatch.keyBind_Emote4 = config_KeyEmote4.Value;
            EmotePatch.enable4 = config_toggleEmote4.Value;
            config_KeyEmote5 = Config.Bind<string>("SHY", "EmoteKey", "5", "SUPPORTED KEYS A-Z | 0-9 | F1-F12 ");
            config_toggleEmote5 = Config.Bind<bool>("SHY", "Enable", true, "TOGGLE SHY EMOTE KEY");
            EmotePatch.keyBind_Emote5 = config_KeyEmote5.Value;
            EmotePatch.enable5 = config_toggleEmote5.Value;
            config_KeyEmote6 = Config.Bind<string>("CLAP", "EmoteKey", "4", "SUPPORTED KEYS A-Z | 0-9 | F1-F12 ");
            config_toggleEmote6 = Config.Bind<bool>("CLAP", "Enable", true, "TOGGLE CLAP EMOTE KEY");
            EmotePatch.keyBind_Emote6 = config_KeyEmote6.Value;
            EmotePatch.enable6 = config_toggleEmote6.Value;
        }

        public static ManualLogSource StaticLogger;

        private Harmony _harmony;

        private ConfigEntry<string> config_KeyEmote3;

        private ConfigEntry<string> config_KeyEmote4;

        private ConfigEntry<string> config_KeyEmote5;

        private ConfigEntry<string> config_KeyEmote6;

        private ConfigEntry<bool> config_toggleEmote3;

        private ConfigEntry<bool> config_toggleEmote4;

        private ConfigEntry<bool> config_toggleEmote5;

        private ConfigEntry<bool> config_toggleEmote6;
    }
}