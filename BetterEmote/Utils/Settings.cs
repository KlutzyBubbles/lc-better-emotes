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
        public static string[] defaultKeyList = [];
        public static string[] defaultControllerList = [];

        public static string emoteWheelKey = "<Keyboard>/v";
        public static string emoteWheelController = "<Gamepad>/leftShoulder";
        public static string emoteWheelNextKey = "<Mouse>/scroll/up";
        public static string emoteWheelNextController = "<Gamepad>/dpad/right";
        public static string emoteWheelPreviousKey = "<Mouse>/scroll/down";
        public static string emoteWheelPreviousController = "<Gamepad>/dpad/left";
        public static string emoteWheelControllerMove = "<Gamepad>/rightStick";

        public static float griddySpeed = 0.5f;
        public static float prisyadkaSpeed = 0.34f;
        public static float emoteCooldown = 0.5f;
        public static float signTextCooldown = 0.1f;

        public static bool disableSpeedChange = false;
        public static bool disableSelfEmote = false;

        public static float controllerDeadzone = 0.25f;

        public static float logDelay = 1f;

        public static void debugAllSettings()
        {
            Plugin.Debug($"debug: {debug}");
            Plugin.Debug($"trace: {trace}");
            Plugin.Debug($"stopOnOuter: {stopOnOuter}");
            Plugin.Debug($"enabledList: {String.Join(", ", enabledList)}"); 
            Plugin.Debug($"defaultKeyList: {String.Join(", ", defaultKeyList)}");
            Plugin.Debug($"defaultControllerList: {String.Join(", ", defaultControllerList)}");
            Plugin.Debug($"emoteWheelKey: {emoteWheelKey}");
            Plugin.Debug($"emoteWheelController: {emoteWheelController}");
            Plugin.Debug($"emoteWheelNextKey: {emoteWheelNextKey}");
            Plugin.Debug($"emoteWheelNextController: {emoteWheelNextController}");
            Plugin.Debug($"emoteWheelPreviousKey: {emoteWheelPreviousKey}");
            Plugin.Debug($"emoteWheelPreviousController: {emoteWheelPreviousController}");
            Plugin.Debug($"emoteWheelControllerMove: {emoteWheelControllerMove}");
            Plugin.Debug($"griddySpeed: {griddySpeed}");
            Plugin.Debug($"prisyadkaSpeed: {prisyadkaSpeed}");
            Plugin.Debug($"emoteCooldown: {emoteCooldown}");
            Plugin.Debug($"signTextCooldown: {signTextCooldown}");
            Plugin.Debug($"disableSpeedChange: {disableSpeedChange}");
            Plugin.Debug($"disableSelfEmote: {disableSelfEmote}");
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
}
