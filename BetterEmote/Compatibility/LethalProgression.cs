using BetterEmote.Utils;
using HarmonyLib;
using static BetterEmote.Compatibility.Patcher;

namespace BetterEmote.Compatibility
{
    [CompatPatch(dependency: "Stoneman.LethalProgression")]
    [HarmonyPatch]
    internal class LethalProgression
    {
        [HarmonyPatch(typeof(InitializeGame), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix()
        {
            Plugin.Debug("LethalProgression.StartPostfix()");
            Settings.disableSpeedChange = true;
        }
    }
}