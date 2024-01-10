using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEmote.Menu.Components
{
    public class ToggleComponent : IInputComponent<bool>
    {
        public string Text { get; set; } = "Toggle";
        public int FontSize { get; set; } = 15;
        public bool Enabled { get; set; } = true;

        internal bool _toggled;
        public bool Value
        {
            get => _toggled;
            set
            {
                if (componentObject != null)
                {
                    componentObject.SetToggled(value);
                }
                else
                {
                    _toggled = value;
                }
            }
        }

        private ToggleComponentObject componentObject;

        public GameObject Construct(GameObject root)
        {
            componentObject = GameObject.Instantiate(Assets.TogglePrefab, root.transform);
            return componentObject.Initialize(this);
        }
    }

    public class ToggleComponentObject : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private GameObject toggleImage;

        private ToggleComponent component;

        internal GameObject Initialize(ToggleComponent component)
        {
            this.component = component;

            button.onClick.AddListener(() => SetToggled(!component.Value));

            ((IMenuComponent)component).OnInitialize().Invoke(component);

            return gameObject;
        }

        private void FixedUpdate()
        {
            button.interactable = component.Enabled;
            label.text = component.Text;
            label.fontSize = component.FontSize;
            toggleImage.SetActive(component.Value);
        }

        internal void SetToggled(bool toggled)
        {
            component._toggled = toggled;
            ((IInputComponent<bool>)component).OnValueChanged().Invoke(component, toggled);
        }
    }
}
