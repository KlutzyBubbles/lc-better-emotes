using BetterEmote.Menu.Components;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BetterEmote.Menu
{
    internal class Assets
    {
        internal static ModMenu ModSettingsView { get; private set; }

        internal static GameObject VerticalWrapper { get; private set; }
        internal static GameObject HorizontalWrapper { get; private set; }

        internal static LabelComponentObject LabelPrefab { get; private set; }
        internal static ButtonComponentObject ButtonPrefab { get; private set; }
        internal static SliderComponentObject SliderPrefab { get; private set; }
        internal static ToggleComponentObject TogglePrefab { get; private set; }
        //internal static DropdownComponentObject DropdownPrefab { get; private set; }
        //internal static InputComponentObject InputPrefab { get; private set; }

        internal static void LoadAssets()
        {
            // var bundle = AssetBundle.LoadFromMemory(Properties.Resources.settings_assets);
            Plugin.StaticLogger.LogInfo($"LoadAssets");
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BetterEmotes/settings_assets"));
            GameObject settingsContainer = bundle.LoadAsset<GameObject>("Mod Settings Container");
            Plugin.StaticLogger.LogInfo($"settingsContainer : {settingsContainer != null}");
            ModSettingsView = settingsContainer.AddComponent<ModMenu>();
            Plugin.StaticLogger.LogInfo($"ModSettingsView : {ModSettingsView != null}");
            VerticalWrapper = bundle.LoadAsset<GameObject>("Vertical Wrapper");
            Plugin.StaticLogger.LogInfo($"VerticalWrapper : {VerticalWrapper != null}");
            HorizontalWrapper = bundle.LoadAsset<GameObject>("Horizontal Wrapper");
            Plugin.StaticLogger.LogInfo($"HorizontalWrapper : {HorizontalWrapper != null}");
            LabelPrefab = bundle.LoadAsset<GameObject>("Label").AddComponent<LabelComponentObject>();
            Plugin.StaticLogger.LogInfo($"LabelPrefab : {LabelPrefab != null}");
            ButtonPrefab = bundle.LoadAsset<GameObject>("Button").AddComponent<ButtonComponentObject>();
            Plugin.StaticLogger.LogInfo($"ButtonPrefab : {ButtonPrefab != null}");
            SliderPrefab = bundle.LoadAsset<GameObject>("Slider").AddComponent<SliderComponentObject>();
            Plugin.StaticLogger.LogInfo($"SliderPrefab : {SliderPrefab != null}");
            TogglePrefab = bundle.LoadAsset<GameObject>("Toggle").AddComponent<ToggleComponentObject>();
            Plugin.StaticLogger.LogInfo($"TogglePrefab : {TogglePrefab != null}");
            //DropdownPrefab = bundle.LoadAsset<GameObject>("Dropdown").GetComponent<DropdownComponentObject>();
            //InputPrefab = bundle.LoadAsset<GameObject>("Input").GetComponent<InputComponentObject>();
        }
    }
}
