using LethalCompanyInputUtils.Api;
using System;
using UnityEngine.InputSystem;

namespace BetterEmote
{
    internal class Keybinds : LcInputActions
    {
        public InputAction MiddleFinger => Asset["Middle_Finger"];
        public InputAction Clap => Asset["Clap"];
        public InputAction Shy => Asset["Shy"];
        public InputAction Griddy => Asset["Griddy"];
        public InputAction Twerk => Asset["Twerk"];
        public InputAction Salute => Asset["Salute"];
        public InputAction EmoteWheel => Asset["EmoteWheel"];

        public override void CreateInputActions(in InputActionMapBuilder builder)
        {
            base.CreateInputActions(builder);
            foreach (string name in Enum.GetNames(typeof(EmotePatch.Emotes)))
            {
                if ((int)Enum.Parse(typeof(EmotePatch.Emotes), name) > 2)
                {
                    builder.NewActionBinding()
                        .WithActionId(name)
                        .WithActionType(InputActionType.Button)
                        .WithKbmPath(EmotePatch.defaultKeyList[(int)Enum.Parse(typeof(EmotePatch.Emotes), name)])
                        .WithBindingName(name)
                        .Finish();
                }
            }
            builder.NewActionBinding()
                .WithActionId("EmoteWheel")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(EmotePatch.emoteWheelKey)
                .WithBindingName("Emote Wheel")
                .Finish();
        }
    }
}
