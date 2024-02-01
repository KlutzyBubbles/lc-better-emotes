using BetterEmote.Utils;
using HarmonyLib;
using static BetterEmote.Compatibility.Patcher;

namespace BetterEmote.Compatibility
{
    [CompatPatch(dependency: "com.malco.lethalcompany.moreshipupgrades")]
    [HarmonyPatch]
    internal class MoreShipUpgrades
    {
        [HarmonyPatch(typeof(InitializeGame), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix()
        {
            Plugin.Debug("MoreShipUpgrades.StartPostfix()");
            Settings.DisableSpeedChange = true;
        }
    }
}