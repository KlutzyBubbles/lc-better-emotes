using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using BetterEmote.Utils;

namespace BetterEmote.AssetScripts
{
    internal class EmoteWheel : MonoBehaviour
    {
        public RectTransform selectionBlock;
        public RectTransform selectionArrow;

        public bool stopEmote;

        public Text emoteInformation;

        public Text pageInformation;

        private int blocksNumber = 8;

        private int currentBlock = 1;

        public int pageNumber;

        public int selectedEmoteID;

        private float angle;

        private float pageCooldown = 0.1f;

        public GameObject[] pages;

        public float wheelMovementOffset = 3.3f;

        public bool controller = false;
        public Vector2 controllerValue = Vector2.zero;

        public static string[] emoteNames;
        public static string[] emoteKeybinds;

        private Vector2 centerScreen;

        private StickControl joystick;

        private float ignoreRadius = 235;
        private float stopRadius = 470;

        private float selectionArrowDelaySpeed = 20f;

        private void Awake()
        {
            Plugin.Debug("EmoteWheel.Awake()");
            findGraphics();
            findPages(gameObject.transform.Find("FunctionalContent"));
            updatePageInfo();
        }

        private void OnEnable()
        {
            Plugin.Debug("EmoteWheel.OnEnable()");
            emoteKeybinds = new string[EmoteDefs.getEmoteCount() + 1];
            foreach (string name in Enum.GetNames(typeof(Emote)))
            {
                emoteKeybinds[EmoteDefs.getEmoteNumber(name) - 1] = Settings.keybinds.getByEmote(EmoteDefs.getEmote(name)).GetBindingDisplayString(0, 0);
            }
            centerScreen = new Vector2(Screen.width / 2, Screen.height / 2);
            Cursor.visible = true;
            if (!GameValues.localPlayerUsingController)
            {
                Mouse.current.WarpCursorPosition(centerScreen);
            }
            string effectivePath = "";
            foreach (InputBinding binding in Settings.keybinds.EmoteWheelController.bindings)
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
                ignoreRadius = Screen.height * 0.365f / 2;
                stopRadius = Screen.height * 0.729f / 2;
            }
            else
            {
                // Vertical aspect calcualte off width
                ignoreRadius = Screen.width * 0.183f / 2;
                stopRadius = Screen.width * 0.368f / 2;
            }
        }

        private void findGraphics()
        {
            Plugin.Debug("EmoteWheel.findGraphics()");
            selectionArrow = gameObject.transform.Find("Graphics").gameObject.transform.Find("SelectionArrow").gameObject.GetComponent<RectTransform>();
            selectionBlock = gameObject.transform.Find("SelectedEmote").gameObject.GetComponent<RectTransform>();
            emoteInformation = gameObject.transform.Find("Graphics").gameObject.transform.Find("EmoteInfo").GetComponent<Text>();
            pageInformation = gameObject.transform.Find("Graphics").gameObject.transform.Find("PageNumber").GetComponent<Text>();
        }

        private void findPages(Transform contentParent)
        {
            pages = new GameObject[contentParent.transform.childCount];
            pageInformation.text = string.Concat(new string[]
            {
                "< Page ",
                (pageNumber + 1).ToString(),
                "/",
                pages.Length.ToString(),
                " >"
            });
            for (int i = 0; i < contentParent.transform.childCount; i++)
            {
                pages[i] = contentParent.transform.GetChild(i).gameObject;
            }
        }

        private void Update()
        {
            Plugin.Trace("EmoteWheel.Update()");
            wheelSelection();
            updateSelectionArrow();
            if (pageCooldown > 0f)
            {
                pageCooldown -= Time.deltaTime;
            }
            if (selectionBlock.gameObject.activeSelf)
            {
                Plugin.Trace("Selection block active self");
                selectedEmoteID = currentBlock + Mathf.RoundToInt(blocksNumber / 4) + blocksNumber * pageNumber;
            }
            else
            {
                Plugin.Trace("Selection block nooo active self");
                selectedEmoteID = emoteNames.Length + 2;
            }
            displayEmoteInfo();
        }

        private void wheelSelection()
        {
            Plugin.Trace("EmoteWheel.wheelSelection()");
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
                isCenter = Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) <= Math.Pow(Settings.controllerDeadzone, 2);
                isOuter = false;
            }
            else
            {
                isCenter = Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) < Math.Pow(ignoreRadius, 2);
                isOuter = Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) >= Math.Pow(stopRadius, 2);
            }
            int corner = diff.x > 0 ? diff.y > 0 ? 1 : 4 : diff.y > 0 ? 2 : 3;
            float num = 180 * (corner - (corner > 2 ? 2 : 1));
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
                    if (Settings.stopOnOuter)
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

        public void alterPage(int byValue)
        {
            Plugin.Trace("EmoteWheel.pageSelection()");
            if (pageCooldown <= 0)
            {
                foreach (GameObject gameObject in pages)
                {
                    gameObject.SetActive(false);
                }
                pageNumber = (pageNumber + byValue + pages.Length) % pages.Length;
                pages[pageNumber].SetActive(true);
                pageCooldown = 0.1f;
                updatePageInfo();
            }
        }

        private void updatePageInfo()
        {
            Plugin.Trace($"EmoteWheel.updatePageInfo({pageNumber}, {pages.Length})");
            pageInformation.text = $"<color=#fe6b02><</color> Page {pageNumber + 1}/{pages.Length} <color=#fe6b02>></color>";
        }

        private void displayEmoteInfo()
        {
            Plugin.Trace($"EmoteWheel.displayEmoteInfo({selectedEmoteID})");
            string text = selectedEmoteID > emoteKeybinds.Length ? "" : emoteKeybinds[selectedEmoteID - 1];
            string text2;
            if (selectedEmoteID <= Enum.GetValues(typeof(Emote)).Length)
            {
                Plugin.Trace($"selectedEmoteID less or equal to emotes length");
                Emote emotes = (Emote)selectedEmoteID;
                text2 = emotes.ToString().Replace("_", " ");
            }
            else
            {
                Plugin.Trace("selectedEmoteID out of range of emotes");
                text2 = "EMPTY";
            }
            emoteInformation.text = $"{text2 ?? ""}\n[{(text ?? "").ToUpper()}]";
        }
        private void updateSelectionArrow()
        {
            Plugin.Trace("EmoteWheel.updateSelectionArrow()");
            float num = 360 / blocksNumber;
            Quaternion b = Quaternion.Euler(0f, 0f, angle - num * 2f);
            selectionArrow.localRotation = Quaternion.Lerp(selectionArrow.localRotation, b, Time.deltaTime * selectionArrowDelaySpeed);
        }
    }
}
