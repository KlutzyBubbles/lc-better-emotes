using BetterEmote.Menu.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace BetterEmote.Menu
{
    public class ModMenu : MonoBehaviour
    {
        private static List<ModSettingsConfig> menuCategories = new List<ModSettingsConfig>();

        [SerializeField]
        internal Transform modListScrollView, modSettingsScrollView;

        internal bool InGame { get; set; }

        public class ModSettingsConfig
        {
            public string Name;
            public string Description;
            public IMenuComponent[] MenuComponents = [];

            public Action<GameObject, ReadOnlyCollection<IMenuComponent>> OnMenuOpen, OnMenuClose;

            internal ButtonComponent ShowSettingsButton;
            internal GameObject Viewport;
        }

        IEnumerator Start()
        {
            ModSettingsConfig active = menuCategories.OrderBy(m => m.Name).First();
            BuildMod(active);
            foreach (var mod in menuCategories.OrderBy(m => m.Name).Skip(1))
            {
                BuildMod(mod);
            }
            ShowModSettings(active, menuCategories);

            yield return new WaitUntil(() => modSettingsScrollView.gameObject.activeInHierarchy);
            yield return LayoutFix();
        }

        private IEnumerator LayoutFix()
        {
            modSettingsScrollView.gameObject.SetActive(false);
            yield return null;
            modSettingsScrollView.gameObject.SetActive(true);

            yield return null;

            modListScrollView.gameObject.SetActive(false);
            yield return null;
            modListScrollView.gameObject.SetActive(true);
        }

        private void BuildMod(ModSettingsConfig mod)
        {
            // Create menu button for mod
            mod.ShowSettingsButton = new ButtonComponent
            {
                Text = mod.Name,
                OnClick = (self) => ShowModSettings(mod, menuCategories)
            };
            mod.ShowSettingsButton.Construct(modListScrollView.gameObject);

            // Create mod settings menu contents
            mod.Viewport = new VerticalComponent
            {
                ChildAlignment = TextAnchor.UpperLeft,
                Children = [
                    new LabelComponent { Text = "Description", FontSize = 16 },
                    new LabelComponent { Text = mod.Description, FontSize = 10 },
                    .. mod.MenuComponents
                ]
            }.Construct(modSettingsScrollView.gameObject);
        }

        private static void ShowModSettings(ModSettingsConfig activeMod, List<ModSettingsConfig> availableMods)
        {
            foreach (var mod in availableMods)
            {
                bool wasClosed = mod.Viewport.activeSelf && mod != activeMod;
                bool wasOpened = !mod.Viewport.activeSelf && mod == activeMod;

                if (wasClosed)
                    mod.OnMenuClose?.Invoke(mod.Viewport, new ReadOnlyCollection<IMenuComponent>(mod.MenuComponents));

                mod.Viewport.SetActive(mod == activeMod);
                mod.ShowSettingsButton.ShowCaret = mod == activeMod;

                if (wasOpened)
                    mod.OnMenuOpen?.Invoke(mod.Viewport, new ReadOnlyCollection<IMenuComponent>(mod.MenuComponents));
            }
        }

        public static void RegisterMod(ModSettingsConfig config)
        {
            menuCategories.Add(config);
        }
    }
}
