using LethalCompanyInputUtils.Api;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterEmote.Utils
{
    public class Keybinds : LcInputActions
    {
        public InputAction MiddleFinger => Asset["Middle_Finger"];
        public InputAction Clap => Asset["Clap"];
        public InputAction Shy => Asset["Shy"];
        public InputAction Griddy => Asset["Griddy"];
        public InputAction Twerk => Asset["Twerk"];
        public InputAction Salute => Asset["Salute"];
        public InputAction Prisyadka => Asset["Prisyadka"];
        public InputAction Sign => Asset["Sign"];

        public InputAction SignSubmit => Asset["SignSubmit"];
        public InputAction SignCancel => Asset["SignCancel"];

        public InputAction EmoteWheel => Asset["EmoteWheel"];
        public InputAction EmoteWheelNextPage => Asset["EmoteWheelNextPage"];
        public InputAction EmoteWheelPreviousPage => Asset["EmoteWheelPreviousPage"];
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
                        .WithKbmPath(Settings.defaultInputList[EmoteDefs.getEmoteNumber(name)].keyboard)
                        .WithBindingName(name)
                        .WithGamepadPath(Settings.defaultInputList[EmoteDefs.getEmoteNumber(name)].controller)
                        .Finish();
                }
            }
            builder.NewActionBinding()
                .WithActionId("SignSubmit")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.signSubmitInput.keyboard)
                .WithGamepadPath(Settings.signSubmitInput.controller)
                .WithBindingName("Sign Submit")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("SignCancel")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.signCancelInput.keyboard)
                .WithGamepadPath(Settings.signCancelInput.controller)
                .WithBindingName("Sign Cancel")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheel")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.emoteWheelInput.keyboard)
                .WithGamepadPath(Settings.emoteWheelInput.controller)
                .WithBindingName("Emote Wheel")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheelNextPage")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.emoteWheelNextInput.keyboard)
                .WithGamepadPath(Settings.emoteWheelNextInput.controller)
                .WithBindingName("Emote Wheel Next Page")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheelPreviousPage")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.emoteWheelPreviousInput.keyboard)
                .WithGamepadPath(Settings.emoteWheelPreviousInput.controller)
                .WithBindingName("Emote Wheel Previous Page")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheelController")
                .WithActionType(InputActionType.Value)
                .WithKbmPath("") // TODO Remove when InputUtils is undated
                .WithGamepadPath(Settings.emoteWheelMoveInput.controller)
                .WithBindingName("Emote Wheel CONTROLLER ONLY")
                .Finish();
        }

        public static InputBind getDisplayStrings(InputAction action)
        {
            BetterEmote.Plugin.Debug($"getDisplayStrings()");
            return new InputBind(action.GetBindingDisplayString(0, 0) ?? "", (action.GetBindingDisplayString(1, 0) ?? "").Replace("Left Stick", "LS").Replace("Right Stick", "RS"));
        }

        public static string formatInputBind(InputBind bind)
        {
            BetterEmote.Plugin.Trace($"formatInputBind()");
            if ((bind.keyboard == null || bind.keyboard == "") && (bind.controller == null || bind.controller == ""))
            {
                return "";
            } else if (bind.keyboard == null || bind.keyboard == "")
            {
                return $"[{bind.controller}]";
            } else if (bind.controller == null || bind.controller == "")
            {
                return $"[{bind.keyboard}]";
            }
            return $"[{bind.keyboard}/{bind.controller}]";
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
