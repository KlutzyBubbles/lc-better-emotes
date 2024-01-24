using BetterEmote.Patches;
using LethalCompanyInputUtils.Api;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterEmote.Utils
{
    internal class Keybinds : LcInputActions
    {
        public InputAction MiddleFinger => Asset["Middle_Finger"];
        public InputAction Clap => Asset["Clap"];
        public InputAction Shy => Asset["Shy"];
        public InputAction Griddy => Asset["Griddy"];
        public InputAction Twerk => Asset["Twerk"];
        public InputAction Salute => Asset["Salute"];
        public InputAction Prisyadka => Asset["Prisyadka"];
        public InputAction Sign => Asset["Sign"];
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
                        .WithGamepadPath(EmotePatch.defaultControllerList[EmoteDefs.getEmoteNumber(name)])
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
                    return MiddleFinger;
                case Emote.Clap:
                    return Clap;
                case Emote.Shy:
                    return Shy;
                case Emote.Griddy:
                    return Griddy;
                case Emote.Twerk:
                    return Twerk;
                case Emote.Salute:
                    return Salute;
                case Emote.Prisyadka:
                    return Prisyadka;
                case Emote.Sign:
                    return Sign;
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
