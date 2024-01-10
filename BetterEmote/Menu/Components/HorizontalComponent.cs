using UnityEngine.UI;
using UnityEngine;

namespace BetterEmote.Menu.Components
{
    public class HorizontalComponent : IMenuComponent
    {
        public IMenuComponent[] Children { internal get; set; } = [];
        public int Spacing { internal get; set; } = 10;
        public TextAnchor ChildAlignment { internal get; set; } = TextAnchor.MiddleLeft;

        public GameObject Construct(GameObject root)
        {
            var layoutGroup = GameObject.Instantiate(Assets.HorizontalWrapper, root.transform).GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = Spacing;
            layoutGroup.childAlignment = ChildAlignment;
            foreach (var child in Children)
            {
                child.Construct(layoutGroup.gameObject);
            }
            return layoutGroup.gameObject;
        }
    }
}
