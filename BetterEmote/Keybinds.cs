using LethalCompanyInputUtils.Api;
using System;
using UnityEngine;
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
        public InputAction EmoteWheelController => Asset["EmoteWheelController"];

        public override void CreateInputActions(in InputActionMapBuilder builder)
        {
            base.CreateInputActions(builder);
            foreach (string name in Enum.GetNames(typeof(Emote)))
            {
                if (EmoteDefs.getEmoteNumber(name) > 2)
                {
                    builder.NewActionBinding()
                        .WithActionId(name)
                        .WithActionType(InputActionType.Button)
                        .WithKbmPath(EmotePatch.defaultKeyList[EmoteDefs.getEmoteNumber(name)])
                        .WithBindingName(name)
                        .WithGamepadPath(EmotePatch.defaultKeyList[EmoteDefs.getEmoteNumber(name)])
                        .Finish();
                }
            }
            builder.NewActionBinding()
                .WithActionId("EmoteWheel")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(EmotePatch.emoteWheelKey)
                .WithGamepadPath(EmotePatch.emoteWheelController)
                .WithBindingName("Emote Wheel")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheelController")
                .WithActionType(InputActionType.Value)
                .WithKbmPath("")
                .WithGamepadPath(EmotePatch.emoteWheelControllerMove)
                .WithBindingName("Emote Wheel CONTROLLER ONLY")
                .Finish();
        }

        public InputAction getByEmote(Emote emote)
        {
            PlayerInput component = GameObject.Find("PlayerSettingsObject").GetComponent<PlayerInput>();
            switch (emote)
            {
                case Emote.Middle_Finger:
                    return this.MiddleFinger;
                case Emote.Clap:
                    return this.Clap;
                case Emote.Shy:
                    return this.Clap;
                case Emote.Griddy:
                    return this.Clap;
                case Emote.Twerk:
                    return this.Clap;
                case Emote.Salute:
                    return this.Clap;
                case Emote.Dance:
                    return component.currentActionMap.FindAction("Emote1", false);
                case Emote.Point:
                    return component.currentActionMap.FindAction("Emote2", false);
                default:
                    throw new Exception("Attempted to get input of unknown emote");
            }
        }
    }
}
