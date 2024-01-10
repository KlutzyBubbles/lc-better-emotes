using System;
using UnityEngine;

namespace BetterEmote.Menu.Components
{
    public interface IInputComponent<T> : IMenuComponent
    {
        public Action<IInputComponent<T>, T> OnValueChanged() { return (self, value) => { }; }
    }
}
