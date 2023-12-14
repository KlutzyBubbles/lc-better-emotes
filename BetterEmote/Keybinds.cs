using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace BetterEmote
{
    internal class Keybinds : LcInputActions
    {
        [InputAction("<Keyboard>/3", Name = "Middle Finger")]
        public InputAction MiddleFinger { get; set; }

        [InputAction("<Keyboard>/6", Name = "Griddy")]
        public InputAction Griddy { get; set; }

        [InputAction("<Keyboard>/5", Name = "Shy")]
        public InputAction Shy { get; set; }

        [InputAction("<Keyboard>/4", Name = "Clap")]
        public InputAction Clap { get; set; }

        [InputAction("<Keyboard>/7", Name = "Salute")]
        public InputAction Salute { get; set; }

        [InputAction("<Keyboard>/8", Name = "Twerk")]
        public InputAction Twerk { get; set; }
    }
}
