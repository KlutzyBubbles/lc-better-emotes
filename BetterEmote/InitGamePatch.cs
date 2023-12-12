using BepInEx.Bootstrap;
using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.Mesh;

namespace BetterEmote
{
    internal class InitGamePatch
    {
        [HarmonyPatch(typeof(InitializeGame), "Start")]
        [HarmonyPrefix]
        private static void StartPostfix()
        {
            foreach (KeyValuePair<string, BepInEx.PluginInfo> keyValuePair in Chainloader.PluginInfos)
            {
                BepInPlugin metadata = keyValuePair.Value.Metadata;
                if (metadata.GUID.Equals("com.malco.lethalcompany.moreshipupgrades") || metadata.GUID.Equals("Stoneman.LethalProgression"))
                {
                    EmotePatch.incompatibleStuff = true;
                    break;
                }
            }
        }

    }
}
