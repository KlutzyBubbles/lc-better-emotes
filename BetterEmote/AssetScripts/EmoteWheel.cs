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
        private static readonly int TotalBlocks = 8;

        private static readonly float SelectionArrowDelaySpeed = 20f;
        private static readonly float PageCooldown = 0.1f;

        public static readonly float WheelMovementOffset = 3.3f;

        private float ignoreRadius = 235;
        private float stopRadius = 470;

        public RectTransform selectionBlock;
        public RectTransform selectionArrow;

        public Text emoteInformation;
        public Text pageInformation;

        public GameObject[] pages = [];

        public bool stopEmote = false;
        public bool controller = false;

        private int currentBlock = 1;
        public int pageNumber = 1;
        public int selectedEmoteID = 0;

        private float angle;
        private float pageCurrentCooldown = PageCooldown;

        private Vector2 centerScreen;

        private StickControl joystick;

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
            try
            {
                selectionArrow = gameObject.transform?.Find("Graphics")?.gameObject?.transform?.Find("SelectionArrow")?.gameObject?.GetComponent<RectTransform>();
                selectionBlock = gameObject.transform?.Find("SelectedEmote")?.gameObject?.GetComponent<RectTransform>();
                emoteInformation = gameObject.transform?.Find("Graphics")?.gameObject?.transform?.Find("EmoteInfo")?.GetComponent<Text>();
                pageInformation = gameObject.transform?.Find("Graphics")?.gameObject?.transform?.Find("PageNumber")?.GetComponent<Text>();
            }
            catch (Exception e)
            {
                Plugin.Debug($"Unable to find graphics {e.Message}");
            }
        }

        private void findPages(Transform contentParent)
        {
            pages = new GameObject[contentParent.transform.childCount];
            updatePageInfo();
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
            if (pageCurrentCooldown > 0f)
            {
                pageCurrentCooldown -= Time.deltaTime;
            }
            if (selectionBlock != null && selectionBlock.gameObject.activeSelf)
            {
                Plugin.Trace("Selection block active self");
                selectedEmoteID = currentBlock + Mathf.RoundToInt(TotalBlocks / 4) + TotalBlocks * pageNumber;
            }
            else
            {
                Plugin.Trace("Selection block nooo active self");
                selectedEmoteID = EmoteDefs.getEmoteCount() + 2;
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
                if (Vector2.Distance(Vector2.zero, joystick.ReadValue()) < WheelMovementOffset / 100)
                {
                    return;
                }
                center = Vector2.zero;
                pointer = joystick.ReadValue();
            }
            else
            {
                if (Vector2.Distance(centerScreen, Mouse.current.position.ReadValue()) < WheelMovementOffset)
                {
                    return;
                }
                center = centerScreen;
                pointer = Mouse.current.position.ReadValue();
            }
            Vector2 diff = pointer - center;
            double distance = Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2);
            bool isCenter;
            bool isOuter;
            if (GameValues.localPlayerUsingController)
            {
                isCenter = distance <= Math.Pow(Settings.controllerDeadzone, 2);
                isOuter = false;
            }
            else
            {
                isCenter = distance < Math.Pow(ignoreRadius, 2);
                isOuter = distance >= Math.Pow(stopRadius, 2);
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
            float num2 = 360 / TotalBlocks;
            currentBlock = Mathf.RoundToInt((angle - num2 * 1.5f) / num2);
            if (isCenter)
            {
                selectionBlock?.gameObject?.SetActive(false);
            }
            else
            {
                if (selectionBlock != null)
                {
                    selectionBlock.gameObject?.SetActive(true);
                    selectionBlock.localRotation = Quaternion.Euler(transform.rotation.z, transform.rotation.y, num2 * currentBlock);
                }
                if (isOuter)
                {
                    if (Settings.stopOnOuter)
                    {
                        selectionBlock?.gameObject?.SetActive(false);
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
            if (pageCurrentCooldown <= 0)
            {
                foreach (GameObject gameObject in pages)
                {
                    gameObject?.SetActive(false);
                }
                pageNumber = (pageNumber + byValue + pages.Length) % pages.Length;
                pages[pageNumber]?.SetActive(true);
                pageCurrentCooldown = PageCooldown;
                updatePageInfo();
            }
        }

        private void updatePageInfo()
        {
            Plugin.Trace($"EmoteWheel.updatePageInfo({pageNumber}, {pages.Length})");
            if (pageInformation != null)
            {
                pageInformation.text = $"<color=#fe6b02><</color> Page {pageNumber + 1}/{pages.Length} <color=#fe6b02>></color>";
            }
        }

        private void displayEmoteInfo()
        {
            Plugin.Trace($"EmoteWheel.displayEmoteInfo({selectedEmoteID})");
            InputBind bind = selectedEmoteID > EmoteDefs.getEmoteCount() ? new InputBind("", "") : Keybinds.getDisplayStrings(Settings.keybinds.getByEmote((Emote)selectedEmoteID));
            string emoteName = "Empty";
            if (selectedEmoteID <= EmoteDefs.getEmoteCount())
            {
                Plugin.Trace($"selectedEmoteID less or equal to emotes length");
                Emote emotes = (Emote)selectedEmoteID;
                emoteName = emotes.ToString().Replace("_", " ");
            }
            if (emoteInformation != null)
            {
                emoteInformation.text = $"{emoteName}\n{Keybinds.formatInputBind(bind)}";
            }
        }
        private void updateSelectionArrow()
        {
            Plugin.Trace("EmoteWheel.updateSelectionArrow()");
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle - (360 / TotalBlocks) * 2f);
            if (selectionArrow != null)
            {
                selectionArrow.localRotation = Quaternion.Lerp(selectionArrow.localRotation, targetRotation, Time.deltaTime * SelectionArrowDelaySpeed);
            }
        }
    }
}
