using BetterEmote.Utils;
using HarmonyLib;
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
            if (LCVR.Plugin.Flags.HasFlag(LCVR.Flags.VR))
            {
                Plugin.Debug($"Found VR mode on LCVR, disabiling self emotes");
                Settings.disableModelOverride = true;
            }
        }
    }
}
