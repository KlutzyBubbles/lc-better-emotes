using HarmonyLib;
using System;

namespace BetterEmote.Utils
{
    public class Settings
    {
        public static bool debug = true;
        public static bool trace = false;

        public static Keybinds keybinds;

        public static bool stopOnOuter = false;

        public static bool[] enabledList = [];

        public static InputBind[] defaultInputList = [];

        public static InputBind emoteWheelInput = new InputBind("<Keyboard>/v", "<Gamepad>/leftShoulder");

        public static InputBind emoteWheelNextInput = new InputBind("<Mouse>/scroll/up", "<Gamepad>/dpad/right");
        public static InputBind emoteWheelPreviousInput = new InputBind("<Mouse>/scroll/down", "<Gamepad>/dpad/left");

        public static InputBind emoteWheelMoveInput = new InputBind("", "<Gamepad>/rightStick");

        public static InputBind signSubmitInput = new InputBind("<Keyboard>/enter", "<Gamepad>/buttonWest");
        public static InputBind signCancelInput = new InputBind("<Mouse>/rightButton", "<Gamepad>/buttonEast");

        public static float griddySpeed = 0.5f;
        public static float prisyadkaSpeed = 0.34f;
        public static float emoteCooldown = 0.5f;
        public static float signTextCooldown = 0.1f;

        public static bool disableSpeedChange = false;
        public static bool disableModelOverride = false;

        public static float controllerDeadzone = 0.25f;

        public static float logDelay = 1f;

        public static void debugAllSettings()
        {
            Plugin.Debug($"debug: {debug}");
            Plugin.Debug($"trace: {trace}");
            Plugin.Debug($"stopOnOuter: {stopOnOuter}");
            Plugin.Debug($"enabledList: {String.Join(", ", enabledList)}"); 
            Plugin.Debug($"defaultInputList: {String.Join(", ", defaultInputList)}");
            Plugin.Debug($"emoteWheelInput: {emoteWheelInput}");
            Plugin.Debug($"emoteWheelNextInput: {emoteWheelNextInput}");
            Plugin.Debug($"emoteWheelPreviousInput: {emoteWheelPreviousInput}");
            Plugin.Debug($"emoteWheelMoveInput: {emoteWheelMoveInput}");
            Plugin.Debug($"signSubmitInput: {signSubmitInput}");
            Plugin.Debug($"signCancelInput: {signCancelInput}");
            Plugin.Debug($"griddySpeed: {griddySpeed}");
            Plugin.Debug($"prisyadkaSpeed: {prisyadkaSpeed}");
            Plugin.Debug($"emoteCooldown: {emoteCooldown}");
            Plugin.Debug($"signTextCooldown: {signTextCooldown}");
            Plugin.Debug($"disableSpeedChange: {disableSpeedChange}");
            Plugin.Debug($"disableModelOverride: {disableModelOverride}");
            Plugin.Debug($"controllerDeadzone: {controllerDeadzone}");
            Plugin.Debug($"logDelay: {logDelay}");
        }

        public static float validateGreaterThanEqualToZero(float value)
        {
            return value < 0 ? 0 : value;
        }

        public static string validatePrefix(string prefix, string value)
        {
            return validatePrefixes([prefix], prefix, value);
        }

        public static string validatePrefixes(string[] prefixes, string defaultPrefix, string value)
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
