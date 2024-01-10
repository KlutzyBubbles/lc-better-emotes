using System;
using UnityEngine;

namespace BetterEmote.Menu.Components
{
    public interface IMenuComponent
    {
        public GameObject Construct(GameObject root);
        public Action<IMenuComponent> OnInitialize() { return (self) => { }; }
}
}
