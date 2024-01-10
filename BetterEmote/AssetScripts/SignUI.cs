using BetterEmote.Patches;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

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
            Player.isTypingChat = true;
            IsSignUIOpen = true;
            _inputField.Select();
            _inputField.text = string.Empty;
            _previewText.text = "PREVIEW";
            Player.disableLookInput = true;
        }

        private void Update()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            if (!Player.performingEmote)
            {
                Close(true);
            }
            if (Keyboard.current[Key.Enter].wasPressedThisFrame && !Keyboard.current[Key.LeftShift].isPressed)
            {
                SubmitText();
            }
            if (Player.quickMenuManager.isMenuOpen || EmotePatch.emoteWheelIsOpened || Mouse.current["rightButton"].IsPressed(0f))
            {
                Close(true);
            }
            if (Gamepad.all.Count != 0)
            {
                if (Gamepad.current.buttonWest.isPressed || Gamepad.current.startButton.isPressed)
                {
                    SubmitText();
                }
                if (Gamepad.current.buttonEast.isPressed || Gamepad.current.selectButton.isPressed)
                {
                    Close(true);
                }
            }
        }

        private void FindComponents()
        {
            _inputField = transform.Find("InputField").GetComponent<TMP_InputField>();
            _charactersLeftText = transform.Find("CharsLeft").GetComponent<Text>();
            _submitButton = transform.Find("Submit").GetComponent<Button>();
            _cancelButton = transform.Find("Cancel").GetComponent<Button>();
            _previewText = transform.Find("Sign").transform.Find("Text").GetComponent<TMP_Text>();
        }

        private void UpdateCharactersLeftText()
        {
            _charactersLeftText.text = $"CHARACTERS LEFT: <color=yellow>{this._inputField.characterLimit - this._inputField.text.Length}</color>";
        }

        private void UpdatePreviewText(string text)
        {
            _previewText.text = text;
        }

        private void SubmitText()
        {
            if (_inputField.text.Equals(string.Empty))
            {
                Close(true);
            }
            else
            {
                // D.L("Submitted " + this._inputField.text + " to server");
                Player.GetComponent<SignEmoteText>().UpdateSignText(_inputField.text);
                if (Player.timeSinceStartingEmote > 0.5f)
                {
                    InputAction.CallbackContext context = default(InputAction.CallbackContext);
                    Player.PerformEmote(context, -10);
                }
                Close(false);
            }
        }

        private void Close(bool cancelAction)
        {
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
