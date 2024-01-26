using HarmonyLib;

namespace BetterEmote.Patches
{
    internal class SignChatPatch
    {
        [HarmonyPatch(typeof(HUDManager), "EnableChat_performed")]
        [HarmonyPrefix]
        private static bool OpenChatPrefix()
        {
            return !EmotePatch.customSignInputField.IsSignUIOpen;
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static bool SubmitChatPrefix()
        {
            return !EmotePatch.customSignInputField.IsSignUIOpen;
        }
    }
}
