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
            foreach (Emote emote in Settings.Emotes.Keys)
            {
                string name = Enum.GetName(typeof(Emote), emote);
                if (EmoteDefs.getEmoteNumber(emote) > EmoteDefs.getEmoteNumber(Emote.Point))
                {
                    builder.NewActionBinding()
                        .WithActionId(name)
                        .WithActionType(InputActionType.Button)
                        .WithKbmPath(Settings.Emotes[emote].keybinds.keyboard)
                        .WithBindingName(name)
                        .WithGamepadPath(Settings.Emotes[emote].keybinds.controller)
                        .Finish();
                }
            }
            builder.NewActionBinding()
                .WithActionId("SignSubmit")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.SignSubmitInput.keyboard)
                .WithGamepadPath(Settings.SignSubmitInput.controller)
                .WithBindingName("Sign Submit")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("SignCancel")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.SignCancelInput.keyboard)
                .WithGamepadPath(Settings.SignCancelInput.controller)
                .WithBindingName("Sign Cancel")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheel")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.EmoteWheelInput.keyboard)
                .WithGamepadPath(Settings.EmoteWheelInput.controller)
                .WithBindingName("Emote Wheel")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheelNextPage")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.EmoteWheelNextInput.keyboard)
                .WithGamepadPath(Settings.EmoteWheelNextInput.controller)
                .WithBindingName("Emote Wheel Next Page")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheelPreviousPage")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(Settings.EmoteWheelPreviousInput.keyboard)
                .WithGamepadPath(Settings.EmoteWheelPreviousInput.controller)
                .WithBindingName("Emote Wheel Previous Page")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("EmoteWheelController")
                .WithActionType(InputActionType.Value)
                .WithGamepadPath(Settings.EmoteWheelMoveInput.controller)
                .WithBindingName("Emote Wheel Selector")
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
