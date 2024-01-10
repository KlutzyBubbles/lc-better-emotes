using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEmote.Menu.Components
{
    public class ButtonComponent : IMenuComponent
    {
        public string Text { internal get; set; } = "Button";
        public bool ShowCaret { internal get; set; } = true;
        public bool Enabled { get; set; } = true;
        public Action<ButtonComponent> OnClick { internal get; set; } = (self) => { };

        public GameObject Construct(GameObject root)
        {
            return GameObject.Instantiate(Assets.ButtonPrefab, root.transform).Initialize(this);
        }
    }

    public class ButtonComponentObject : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TextMeshProUGUI label;

        private ButtonComponent component;

        internal GameObject Initialize(ButtonComponent component)
        {
            this.component = component;

            button.onClick.AddListener(() => component.OnClick?.Invoke(component));

            ((IMenuComponent)component).OnInitialize().Invoke(component);

            return gameObject;
        }

        private void FixedUpdate()
        {
            button.interactable = component.Enabled;
            label.text = $"{(component.ShowCaret ? "> " : "")}{component.Text}";
        }
    }
}
