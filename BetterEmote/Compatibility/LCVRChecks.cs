using BetterEmote.Utils;
using HarmonyLib;
using System;
using static BetterEmote.Compatibility.Patcher;

namespace BetterEmote.Compatibility
{
    [CompatPatch(dependency: "io.daxcess.lcvr")]
    [HarmonyPatch]
    internal class LCVRChecks
    {
        [HarmonyPatch(typeof(InitializeGame), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix()
        {
            Plugin.Debug("LCVRChecks.StartPostfix()");
            try
            {
                if (LCVR.Plugin.Flags.HasFlag(LCVR.Flags.VR))
                {
                    Plugin.Debug($"Found VR mode on LCVR, disabiling self emotes");
                    Settings.DisableModelOverride = true;
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"Unable to hook into LCVR to see if the player is VR {e.Message}");
            }
        }
    }
}
