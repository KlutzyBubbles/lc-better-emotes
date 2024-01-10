using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using BetterEmote.Patches;
using BetterEmote.Utils;

namespace BetterEmote
{
    internal class SelectionWheel : MonoBehaviour
    {
        public RectTransform selectionBlock;

        public bool stopEmote;

        public Text emoteInformation;

        public Text pageInformation;

        private int blocksNumber = 8;

        private int currentBlock = 1;

        public int pageNumber;

        public int selectedEmoteID;

        private float angle;

        private float pageCooldown = 0.1f;

        public GameObject[] Pages;

        public float wheelMovementOffset = 3.3f;

        public bool controller = false;
        public Vector2 controllerValue = Vector2.zero;

        public static string[] emoteNames;
        public static string[] emoteKeybinds;

        public static float controllerDeadzone = 0.25f;

        private Vector2 centerScreen;

        private StickControl joystick;

        private float ignoreRadius = 235;
        private float stopRadius = 470;

        private void OnEnable()
        {
            centerScreen = new Vector2(Screen.width / 2, Screen.height / 2);
            emoteKeybinds = new string[EmoteDefs.getEmoteCount() + 1];
            foreach (string name in Enum.GetNames(typeof(Emote)))
            {
                emoteKeybinds[EmoteDefs.getEmoteNumber(name) - 1] = EmotePatch.keybinds.getByEmote(EmoteDefs.getEmote(name)).GetBindingDisplayString(0, 0);
            }
            Cursor.visible = true;
            selectionBlock = gameObject.transform.Find("SelectedEmote").gameObject.GetComponent<RectTransform>();
            selectionBlock.gameObject.SetActive(false);
            GameObject localGameObject = gameObject.transform.Find("FunctionalContent").gameObject;
            emoteInformation = gameObject.transform.Find("Graphics").gameObject.transform.Find("EmoteInfo").GetComponent<Text>();
            Pages = new GameObject[localGameObject.transform.childCount];
            pageInformation = gameObject.transform.Find("Graphics").gameObject.transform.Find("PageNumber").GetComponent<Text>();
            pageInformation.text = "Page " + Pages.Length.ToString() + "/" + (pageNumber + 1).ToString();
            for (int i = 0; i < localGameObject.transform.childCount; i++)
            {
                Pages[i] = localGameObject.transform.GetChild(i).gameObject;
            }
            if (!GameValues.localPlayerUsingController)
            {
                Mouse.current.WarpCursorPosition(centerScreen);
            }
            string effectivePath = "";
            foreach (InputBinding binding in EmotePatch.keybinds.EmoteWheelController.bindings)
            {
                if (binding.effectivePath != null && binding.effectivePath.Length > 0)
                {
                    effectivePath = binding.effectivePath;
                }
            }
            if (Gamepad.current != null)
            {
                if (effectivePath == "<Gamepad>/leftStick")
                {
                    joystick = Gamepad.current.leftStick;
                }
                else
                {
                    joystick = Gamepad.current.rightStick;
                }
            }
            else
            {
                joystick = null;
            }
            float screen = Screen.width / Screen.height;
            if (screen >= 16 / 9)
            {
                // Wide aspect ratio calcualte off height
                ignoreRadius = (Screen.height * 0.365f) / 2;
                stopRadius = (Screen.height * 0.729f) / 2;
            }
            else
            {
                // Vertical aspect calcualte off width
                ignoreRadius = (Screen.width * 0.183f) / 2;
                stopRadius = (Screen.width * 0.368f) / 2;
            }
        }

        private void Update()
        {
            wheelSelection();
            pageSelection();
            if (selectionBlock.gameObject.activeSelf)
            {
                selectedEmoteID = currentBlock + Mathf.RoundToInt(blocksNumber / 4) + blocksNumber * pageNumber;
            }
            else
            {
                selectedEmoteID = emoteNames.Length + 2;
            }
            displayEmoteInfo();
        }

        private void wheelSelection()
        {
            Vector2 center;
            Vector2 pointer;
            if (joystick != null && GameValues.localPlayerUsingController)
            {
                if (Vector2.Distance(Vector2.zero, joystick.ReadValue()) < wheelMovementOffset / 100)
                {
                    return;
                }
                center = Vector2.zero;
                pointer = joystick.ReadValue();
            }
            else
            {
                if (Vector2.Distance(centerScreen, Mouse.current.position.ReadValue()) < wheelMovementOffset)
                {
                    return;
                }
                center = centerScreen;
                pointer = Mouse.current.position.ReadValue();
            }
            Vector2 diff = pointer - center;
            bool isCenter;
            bool isOuter;
            if (GameValues.localPlayerUsingController)
            {
                isCenter = Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) <= Math.Pow(controllerDeadzone, 2);
                isOuter = false;
            }
            else
            {
                isCenter = Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) < Math.Pow(ignoreRadius, 2);
                isOuter = Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) >= Math.Pow(stopRadius, 2);
            }
            int corner = diff.x > 0 ? (diff.y > 0 ? 1 : 4) : (diff.y > 0 ? 2 : 3);
            float num = 180 * (corner - ((corner > 2) ? 2 : 1));
            angle = Mathf.Atan(diff.y / diff.x) * 57.295776f + num;
            if (angle == 90f)
            {
                angle = 270f;
            }
            else if (angle == 270f)
            {
                angle = 90f;
            }
            float num2 = 360 / blocksNumber;
            currentBlock = Mathf.RoundToInt((angle - num2 * 1.5f) / num2);
            if (isCenter)
            {
                selectionBlock.gameObject.SetActive(false);
            }
            else
            {
                selectionBlock.gameObject.SetActive(true);
                selectionBlock.localRotation = Quaternion.Euler(transform.rotation.z, transform.rotation.y, num2 * currentBlock);
                if (isOuter)
                {
                    if (EmotePatch.stopOnOuter)
                    {
                        selectionBlock.gameObject.SetActive(false);
                        stopEmote = true;
                    }
                    else
                    {
                        stopEmote = false;
                    }
                }
            }
        }

        private void pageSelection()
        {
            pageInformation.text = "Page " + Pages.Length.ToString() + "/" + (pageNumber + 1).ToString();
            if (pageCooldown > 0f)
            {
                pageCooldown -= Time.deltaTime;
            }
            else
            {
                if (Mouse.current.scroll.y.ReadValue() != 0f)
                {
                    foreach (GameObject gameObject in Pages)
                    {
                        gameObject.SetActive(false);
                    }
                    int num = (Mouse.current.scroll.y.ReadValue() > 0f) ? 1 : -1;
                    if (pageNumber + 1 > Pages.Length - 1 && num > 0)
                    {
                        pageNumber = 0;
                    }
                    else
                    {
                        if (pageNumber - 1 < 0 && num < 0)
                        {
                            pageNumber = Pages.Length - 1;
                        }
                        else
                        {
                            pageNumber += num;
                        }
                    }
                    Pages[pageNumber].SetActive(true);
                    pageCooldown = 0.1f;
                }
            }
        }

        private void displayEmoteInfo()
        {
            string text = (selectedEmoteID > emoteKeybinds.Length) ? "" : emoteKeybinds[selectedEmoteID - 1];
            string text2;
            if (selectedEmoteID <= Enum.GetValues(typeof(Emote)).Length)
            {
                Emote emotes = (Emote)selectedEmoteID;
                text2 = emotes.ToString().Replace("_", " ");
            }
            else
            {
                text2 = "EMPTY";
            }
            emoteInformation.text = text2 + "\n[" + text.ToUpper() + "]";
        }
    }
}
