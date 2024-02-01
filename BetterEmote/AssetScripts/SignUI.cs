using BetterEmote.Patches;
using GameNetcodeStuff;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using BetterEmote.Utils;
using System;
using BetterEmote.Netcode;

namespace BetterEmote.AssetScripts
{
    public class SignUI : MonoBehaviour
    {
        public PlayerControllerB Player;

        private TMP_InputField inputField;

        private TMP_Text previewText;

        private Text charactersLeftText;
        private Text submitText;
        private Text cancelText;

        private Button submitButton;
        private Button cancelButton;

        public bool IsSignUIOpen;

        private void Awake()
        {
            Plugin.Debug("SignUI.Awake()");
            FindComponents();
            submitButton?.onClick?.AddListener(new UnityAction(this.SubmitText));
            cancelButton?.onClick?.AddListener(delegate ()
            {
                Close(true);
            });
            inputField?.onValueChanged?.AddListener(delegate (string fieldText)
            {
                UpdatePreviewText(fieldText);
                UpdateCharactersLeftText();
            });
        }

        private void OnEnable()
        {
            Plugin.Debug("SignUI.OnEnable()");
            Player.isTypingChat = true;
            IsSignUIOpen = true;
            if (inputField != null)
            {
                inputField.Select();
                inputField.text = string.Empty;
            }
            if (previewText != null)
            {
                previewText.text = "PREVIEW";
            }
            updateKeybindText();
            Player.disableLookInput = true;
        }

        public void updateKeybindText()
        {
            Plugin.Debug("SignUI.updateKeybindText()");
            InputBind submit = Keybinds.getDisplayStrings(Settings.Keybinds.SignSubmit);
            InputBind cancel = Keybinds.getDisplayStrings(Settings.Keybinds.SignCancel);
            if (submitText != null)
            {
                submitText.text = $"<color=orange>{Keybinds.formatInputBind(submit)}</color> Submit";
            }
            if (cancelText != null)
            {
                cancelText.text = $"<color=orange>{Keybinds.formatInputBind(cancel)}</color> Cancel";
            }
        }

        private void Update()
        {
            Plugin.Trace("SignUI.Update()");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            if (!Player.performingEmote)
            {
                Plugin.Debug("SignUI Player isnt performing emote");
                Close(true);
            }
            if (Player.quickMenuManager.isMenuOpen || EmoteKeybindPatch.EmoteWheelIsOpened)
            {
                Plugin.Debug("Menu is open or right mouse button is clicked");
                Close(true);
            }
        }

        private void FindComponents()
        {
            Plugin.Debug("SignUI.FindComponents()");
            try
            {
                inputField = transform.Find("InputField")?.GetComponent<TMP_InputField>();
                charactersLeftText = transform.Find("CharsLeft")?.GetComponent<Text>();
                submitButton = transform.Find("Submit")?.GetComponent<Button>();
                cancelButton = transform.Find("Cancel")?.GetComponent<Button>();
                previewText = transform.Find("Sign")?.transform?.Find("Text")?.GetComponent<TMP_Text>();
                submitText = transform.Find("Submit")?.transform?.Find("Text")?.GetComponent<Text>();
                cancelText = transform.Find("Cancel")?.transform?.Find("Text")?.GetComponent<Text>();
            }
            catch (Exception e)
            {
                Plugin.Debug($"Unable to find components for sign UI {e}");
            }
        }

        private void UpdateCharactersLeftText()
        {
            Plugin.Debug("SignUI.UpdateCharactersLeftText()");
            if (charactersLeftText != null)
            {
                charactersLeftText.text = $"CHARACTERS LEFT: <color=yellow>{(inputField?.characterLimit ?? 0) - (inputField?.text?.Length ?? 0)}</color>";
            }
        }

        private void UpdatePreviewText(string text)
        {
            Plugin.Debug($"SignUI.UpdatePreviewText({text})");
            if (previewText != null)
            {
                previewText.text = text;
            }
        }

        public void SubmitText()
        {
            Plugin.Debug("SignUI.SubmitText()");
            if (inputField?.text?.Equals(string.Empty) ?? true)
            {
                Close(true);
            }
            else
            {
                if (inputField != null)
                {
                    Plugin.Debug($"Submitted {inputField.text} to server");
                    Player.GetComponent<SignEmoteText>().UpdateSignText(inputField.text);
                }
                if (Player.timeSinceStartingEmote > Settings.SignTextCooldown)
                {
                    Plugin.Debug($"Time elapsed, time to perform");
                    InputAction.CallbackContext context = default(InputAction.CallbackContext);
                    Player.PerformEmote(context, EmoteDefs.getEmoteNumber(AltEmote.Sign_Text));
                }
                Close(false);
            }
        }

        public void Close(bool cancelAction)
        {
            Plugin.Debug($"SignUI.Close({cancelAction})");
            Player.isTypingChat = false;
            IsSignUIOpen = false;
            if (cancelAction)
            {
                Player.performingEmote = false;
                Player.StopPerformingEmoteServerRpc();
            }
            if (!Player.quickMenuManager.isMenuOpen)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            Player.disableLookInput = false;
            gameObject.SetActive(false);
        }
    }
}
