using System;
using TMPro;
using UnityEngine;

namespace BetterEmote.Menu.Components
{
    public class LabelComponent : IMenuComponent
    {
        public string Text { internal get; set; } = "Label Text";
        public float FontSize { internal get; set; } = 16;
        public TextAlignmentOptions Alignment { internal get; set; } = TextAlignmentOptions.MidlineLeft;

        public GameObject Construct(GameObject root)
        {
            return GameObject.Instantiate(Assets.LabelPrefab, root.transform).Initialize(this);
        }
    }

    public class LabelComponentObject : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label;

        private LabelComponent component;

        internal GameObject Initialize(LabelComponent component)
        {
            this.component = component;

            ((IMenuComponent)component).OnInitialize().Invoke(component);

            return gameObject;
        }

        private void FixedUpdate()
        {
            label.text = component.Text;
            label.fontSize = component.FontSize;
            label.alignment = component.Alignment;
        }
    }
}
