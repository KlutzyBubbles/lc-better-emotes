using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEmote.Menu.Components
{
    public class SliderComponent : IInputComponent<float>
    {
        public string Text { get; set; } = "Slider";
        public bool ShowValue { get; set; } = true;
        public bool WholeNumbers { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public float MinValue { get; set; } = 0f;
        public float MaxValue { get; set; } = 100f;

        internal float _currentValue = 50f;
        public float Value
        {
            get => _currentValue;
            set
            {
                if (componentObject != null)
                {
                    componentObject.SetValue(value);
                }
                else
                {
                    _currentValue = value;
                }
            }
        }

        private SliderComponentObject componentObject;

        public GameObject Construct(GameObject root)
        {
            componentObject = GameObject.Instantiate(Assets.SliderPrefab, root.transform);
            return componentObject.Initialize(this);
        }
    }

    public class SliderComponentObject : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private Slider slider;

        private SliderComponent component;

        internal GameObject Initialize(SliderComponent component)
        {
            this.component = component;

            slider.wholeNumbers = component.WholeNumbers;
            slider.interactable = component.Enabled;
            var value = component._currentValue;
            slider.minValue = component.MinValue;
            slider.maxValue = component.MaxValue;
            slider.value = component._currentValue;

            slider.onValueChanged.AddListener(SetValue);

            ((IMenuComponent)component).OnInitialize().Invoke(component);

            return gameObject;
        }

        private void FixedUpdate()
        {
            slider.wholeNumbers = component.WholeNumbers;
            slider.interactable = component.Enabled;
            slider.minValue = component.MinValue;
            slider.maxValue = component.MaxValue;
            slider.value = component._currentValue;
            label.text = $"{component.Text} {(component.ShowValue ? slider.value : "")}";
        }

        internal void SetValue(float value)
        {
            slider.value = value;
            component._currentValue = value;
            ((IInputComponent<float>)component).OnValueChanged().Invoke(component, value);
        }
    }
}
