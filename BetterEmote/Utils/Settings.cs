using BepInEx.Configuration;
using LCVR;
using System;
using System.Xml.Linq;

namespace BetterEmote.Utils
{
    public class Settings
    {
        private static readonly string KeyLabel = "Emote Keys";
        private static readonly string EnabledLabel = "Enabled Emotes";
        private static readonly string ControllerLabel = "Emote Controller Bindings";
        private static readonly string EmoteSettingsLabel = "Emote Settings";
        private static readonly string DebugSettingsLabel = "Debug Settings";

        public static bool Debug = false;
        public static bool Trace = false;

        public static Keybinds Keybinds;

        public static bool StopOnOuter = false;

        public static bool[] EnabledList = [];

        public static InputBind[] DefaultInputList = [];

        public static InputBind EmoteWheelInput = new InputBind("<Keyboard>/v", "<Gamepad>/leftShoulder");

        public static InputBind EmoteWheelNextInput = new InputBind("<Mouse>/scroll/up", "<Gamepad>/dpad/right");
        public static InputBind EmoteWheelPreviousInput = new InputBind("<Mouse>/scroll/down", "<Gamepad>/dpad/left");

        public static InputBind EmoteWheelMoveInput = new InputBind("", "<Gamepad>/rightStick");

        public static InputBind SignSubmitInput = new InputBind("<Keyboard>/enter", "<Gamepad>/buttonWest");
        public static InputBind SignCancelInput = new InputBind("<Mouse>/rightButton", "<Gamepad>/buttonEast");

        public static float GriddySpeed = 0.5f;
        public static float PrisyadkaSpeed = 0.34f;
        public static float EmoteCooldown = 0.2f;
        public static float SignTextCooldown = 0.1f;

        public static bool DisableSpeedChange = false;
        public static bool DisableModelOverride = false;

        public static float ControllerDeadzone = 0.25f;

        public static float LogDelay = 1f;

        public static void DebugAllSettings()
        {
            Plugin.Debug($"Debug: {Debug}");
            Plugin.Debug($"Trace: {Trace}");
            Plugin.Debug($"StopOnOuter: {StopOnOuter}");
            Plugin.Debug($"EnabledList: {String.Join(", ", EnabledList)}"); 
            Plugin.Debug($"DefaultInputList: {String.Join(", ", DefaultInputList)}");
            Plugin.Debug($"EmoteWheelInput: {EmoteWheelInput}");
            Plugin.Debug($"EmoteWheelNextInput: {EmoteWheelNextInput}");
            Plugin.Debug($"EmoteWheelPreviousInput: {EmoteWheelPreviousInput}");
            Plugin.Debug($"EmoteWheelMoveInput: {EmoteWheelMoveInput}");
            Plugin.Debug($"SignSubmitInput: {SignSubmitInput}");
            Plugin.Debug($"SignCancelInput: {SignCancelInput}");
            Plugin.Debug($"GriddySpeed: {GriddySpeed}");
            Plugin.Debug($"PrisyadkaSpeed: {PrisyadkaSpeed}");
            Plugin.Debug($"EmoteCooldown: {EmoteCooldown}");
            Plugin.Debug($"SignTextCooldown: {SignTextCooldown}");
            Plugin.Debug($"DisableSpeedChange: {DisableSpeedChange}");
            Plugin.Debug($"DisableModelOverride: {DisableModelOverride}");
            Plugin.Debug($"ControllerDeadzone: {ControllerDeadzone}");
            Plugin.Debug($"LogDelay: {LogDelay}");
        }

        public static float ValidateGreaterThanEqualToZero(float value)
        {
            return value < 0 ? 0 : value;
        }

        public static string ValidatePrefix(string prefix, string value)
        {
            return ValidatePrefixes([prefix], prefix, value);
        }

        public static string ValidatePrefixes(string[] prefixes, string defaultPrefix, string value)
        {
            if (value.Equals(""))
            {
                return $"";
            }
            foreach (string prefix in prefixes)
            {
                if (value.ToLower().StartsWith(prefix.ToLower()))
                {
                    return value;
                }
            }
            return $"{defaultPrefix}/{value}";
        }

        public static void LoadFromConfig(ConfigFile config)
        {
            EnabledList = new bool[EmoteDefs.getEmoteCount() + 1];
            DefaultInputList = new InputBind[EmoteDefs.getEmoteCount() + 1];
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
                    DefaultInputList[emoteNumber] = GetFromConfig(config, new InputBind(defaultEmoteKey, ""), name, $"{name} emote");
                }
                EnabledList[EmoteDefs.getEmoteNumber(name)] = config.Bind(EnabledLabel, $"Enable {name}", true, $"Toggle {name} emote key").Value;
            }
            SignSubmitInput = GetFromConfig(config, SignSubmitInput, "Sign Submit", "sign submit");
            SignCancelInput = GetFromConfig(config, SignCancelInput, "Sign Cancel", "sign cancel");
            EmoteWheelInput = GetFromConfig(config, EmoteWheelInput, "Emote Wheel", "emote wheel");
            EmoteWheelNextInput = GetFromConfig(config, EmoteWheelNextInput, "Emote Wheel Next Page", "emote wheel next page");
            EmoteWheelPreviousInput = GetFromConfig(config, EmoteWheelPreviousInput, "Emote Wheel Previous Page", "emote wheel next page");

            EmoteWheelMoveInput.controller = ValidatePrefix("<Gamepad>", config.Bind(ControllerLabel, "Emote Wheel Move", EmoteWheelMoveInput.controller, "Default controller binding for the emote wheel movement").Value);
            ControllerDeadzone = ValidateGreaterThanEqualToZero(config.Bind(ControllerLabel, "Emote Wheel Deadzone", ControllerDeadzone, "Default controller deadzone for emote selection").Value);

            GriddySpeed = ValidateGreaterThanEqualToZero(config.Bind(EmoteSettingsLabel, "Griddy Speed", GriddySpeed, "Speed of griddy relative to regular speed").Value);
            PrisyadkaSpeed = ValidateGreaterThanEqualToZero(config.Bind(EmoteSettingsLabel, "Prisyadka Speed", PrisyadkaSpeed, "Speed of Prisyadka relative to regular speed").Value);

            EmoteCooldown = ValidateGreaterThanEqualToZero(config.Bind(EmoteSettingsLabel, "Cooldown", EmoteCooldown, "Time (in seconds) to wait before being able to switch emotes").Value);

            SignTextCooldown = ValidateGreaterThanEqualToZero(config.Bind(EmoteSettingsLabel, "Sign Text Cooldown", SignTextCooldown, "Time (in seconds) to wait before being able to finish typing (was hard coded into MoreEmotes)").Value);

            StopOnOuter = config.Bind(EmoteSettingsLabel, "Stop on outer", StopOnOuter, "Whether or not to stop emoting when mousing to outside the emote wheel").Value;

            LogDelay = ValidateGreaterThanEqualToZero(config.Bind(DebugSettingsLabel, "Trace Delay", LogDelay, "Time (in seconds) to wait before writing the same trace line, trace messages are very spammy").Value);
            Debug = config.Bind(DebugSettingsLabel, "Debug", Debug, "Whether or not to enable debug log messages, bepinex also needs to be configured to show debug logs").Value;
            Trace = config.Bind(DebugSettingsLabel, "Trace", Trace, "Whether or not to enable trace log messages, bepinex also needs to be configured to show debug logs").Value;
            DisableSpeedChange = config.Bind(DebugSettingsLabel, "Disable Speed Changed", DisableSpeedChange, "Whether or not to disable speed changes that might affect other mods").Value;
            DisableModelOverride = config.Bind(DebugSettingsLabel, "Disable Self Emote", DisableModelOverride, "Whether or not to disable overriding the player model, can help with conflicting mods").Value;
        }

        private static InputBind GetFromConfig(ConfigFile config, InputBind defaultBind, string name, string description)
        {
            return new InputBind(
                ValidatePrefixes(["<Keyboard>", "<Mouse>"], "<Keyboard>", config.Bind(KeyLabel, $"{name} Key", defaultBind.keyboard, $"Default keybind for the {description}").Value),
                ValidatePrefix("<Gamepad>", config.Bind(ControllerLabel, $"{name} Button", defaultBind.controller, $"Default controller binding for the {description}").Value));
        } 
    }

    public struct InputBind
    {
        public string keyboard = "";
        public string controller = "";

        public InputBind(string keyboard, string controller) {
            this.keyboard = keyboard;
            this.controller = controller;
        }

        public override string ToString()
        {
            return $"('{keyboard}', '{controller}')";
        }
    }
}
