namespace BetterEmote.Utils
{
    public class Settings
    {
        public static bool debug = true;
        public static bool trace = false;

        public static Keybinds keybinds;

        public static bool stopOnOuter = false;

        public static bool[] enabledList;
        public static string[] defaultKeyList;
        public static string[] defaultControllerList;

        public static string emoteWheelKey = "<Keyboard>/v";
        public static string emoteWheelController = "<Gamepad>/leftShoulder";
        public static string emoteWheelControllerMove = "<Gamepad>/rightStick";

        public static float griddySpeed = 0.5f;
        public static float prisyadkaSpeed = 0.34f;
        public static float emoteCooldown = 0.5f;
        public static float signTextCooldown = 0.1f;

        public static bool incompatibleStuff;

        public static float controllerDeadzone = 0.25f;

        public static float logDelay = 1f;

        public static float validateGreaterThanEqualToZero(float value)
        {
            return value < 0 ? 0 : value;
        }

        public static string validatePrefix(string prefix, string value)
        {
            return value.Equals("") ? "" : (value.ToLower().StartsWith(prefix.ToLower()) ? value : $"{prefix}/{value}");
        }
    }
}
