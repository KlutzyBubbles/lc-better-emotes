using BetterEmote.AssetScripts;
using BetterEmote.Utils;
using HarmonyLib;
using UnityEngine;

namespace BetterEmote.Patches
{
    internal class RoundPatch
    {
        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        private static void AwakePost()
        {
            Plugin.Debug("AwakePost()");
            Settings.DebugAllSettings();
            if (!Settings.DisableModelOverride)
            {
                GameObject gameObject = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
                LocalPlayer.CustomSignInputField = UnityEngine.Object.Instantiate(LocalPlayer.SignUIPrefab, gameObject.transform).AddComponent<SignUI>();
            }
            LocalPlayer.IsPlayerFirstFrame = true;
        }
    }
}
