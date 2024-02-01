using BetterEmote.AssetScripts;
using HarmonyLib;

namespace BetterEmote.Patches
{
    internal class SignChatPatch
    {
        [HarmonyPatch(typeof(HUDManager), "EnableChat_performed")]
        [HarmonyPrefix]
        private static bool OpenChatPrefix()
        {
            return LocalPlayer.CustomSignInputField == null || !LocalPlayer.CustomSignInputField.IsSignUIOpen;
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static bool SubmitChatPrefix()
        {
            return LocalPlayer.CustomSignInputField == null || !LocalPlayer.CustomSignInputField.IsSignUIOpen;
        }
    }
}
