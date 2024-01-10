using HarmonyLib;
using UnityEngine;
using BetterEmote.Menu;

namespace BetterEmote.Patches
{
    internal class MenuPatch
    {
        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPostfix]
        static void QuickMenuStartPostfix(QuickMenuManager __instance)
        {
            Plugin.StaticLogger.LogInfo($"QuickMenuStartPostfix()");
            var settingsContainer = __instance.menuContainer.transform.Find("SettingsPanel");
            Plugin.StaticLogger.LogInfo($"settingsContainer {settingsContainer != null}");
            if (settingsContainer != null)
            {
                var menu = GameObject.Instantiate(Assets.ModSettingsView, settingsContainer);
                Plugin.StaticLogger.LogInfo($"menu {menu != null}");
                menu.InGame = true;
                menu.transform.SetSiblingIndex(menu.transform.GetSiblingIndex() - 2);
            }
            else
            {
                Plugin.StaticLogger.LogWarning($"Cannot patch custom settings menu into quick menu");
            }
        }

        [HarmonyPatch(typeof(MenuManager), "Start")]
        [HarmonyPostfix]
        static void MenuStartPostfix(MenuManager __instance)
        {
            Plugin.StaticLogger.LogInfo($"MenuStartPostfix()");
            var settingsContainer = __instance.transform.parent.Find("MenuContainer/SettingsPanel");
            Plugin.StaticLogger.LogInfo($"settingsContainer {settingsContainer != null}");
            if (settingsContainer != null)
            {
                var menu = GameObject.Instantiate(Assets.ModSettingsView, settingsContainer);
                Plugin.StaticLogger.LogInfo($"menu {menu != null}");
                menu.transform.SetSiblingIndex(menu.transform.GetSiblingIndex() - 2);
            }
            else
            {
                Plugin.StaticLogger.LogWarning($"Cannot patch custom settings menu into main menu");
            }
        }
    }
}
