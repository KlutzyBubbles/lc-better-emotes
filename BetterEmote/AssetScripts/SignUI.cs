using BetterEmote.Patches;
using GameNetcodeStuff;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using BetterEmote.Utils;

namespace BetterEmote.AssetScripts
{
    public class SignUI : MonoBehaviour
    {
        public PlayerControllerB Player;

        private TMP_InputField _inputField;

        private Text _charactersLeftText;

        private TMP_Text _previewText;

        private Button _submitButton;

        private Button _cancelButton;

        public bool IsSignUIOpen;

        private void Awake()
        {
            Plugin.Debug("SignUI.Awake()");
            FindComponents();
            _submitButton.onClick.AddListener(new UnityAction(this.SubmitText));
            _cancelButton.onClick.AddListener(delegate ()
            {
                Close(true);
            });
            _inputField.onValueChanged.AddListener(delegate (string fieldText)
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
            _inputField.Select();
            _inputField.text = string.Empty;
            _previewText.text = "PREVIEW";
            Player.disableLookInput = true;
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
            if (Player.quickMenuManager.isMenuOpen || EmoteKeybindPatch.emoteWheelIsOpened)
            {
                Plugin.Debug("Menu is open or right mouse button is clicked");
                Close(true);
            }
        }

        private void FindComponents()
        {
            Plugin.Debug("SignUI.FindComponents()");
            _inputField = transform.Find("InputField").GetComponent<TMP_InputField>();
            _charactersLeftText = transform.Find("CharsLeft").GetComponent<Text>();
            _submitButton = transform.Find("Submit").GetComponent<Button>();
            _cancelButton = transform.Find("Cancel").GetComponent<Button>();
            _previewText = transform.Find("Sign").transform.Find("Text").GetComponent<TMP_Text>();
        }

        private void UpdateCharactersLeftText()
        {
            Plugin.Debug("SignUI.UpdateCharactersLeftText()");
            _charactersLeftText.text = $"CHARACTERS LEFT: <color=yellow>{this._inputField.characterLimit - this._inputField.text.Length}</color>";
        }

        private void UpdatePreviewText(string text)
        {
            Plugin.Debug($"SignUI.UpdatePreviewText({text})");
            _previewText.text = text;
        }

        public void SubmitText()
        {
            Plugin.Debug("SignUI.SubmitText()");
            if (_inputField.text.Equals(string.Empty))
            {
                Close(true);
            }
            else
            {
                Plugin.Debug($"Submitted {this._inputField.text} to server");
                Player.GetComponent<SignEmoteText>().UpdateSignText(_inputField.text);
                if (Player.timeSinceStartingEmote > Settings.signTextCooldown)
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
