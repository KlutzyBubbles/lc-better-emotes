using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

        public string selectedEmoteName;

        public float wheelMovementOffset = 3.3f;

        public static string[] emoteNames;
        public static string[] emoteKeybinds;

        private Vector2 center;

        private void OnEnable()
        {
            center = new Vector2(Screen.width / 2, Screen.height / 2);
            PlayerInput component = GameObject.Find("PlayerSettingsObject").GetComponent<PlayerInput>();
            emoteKeybinds = new string[Enum.GetNames(typeof(EmotePatch.Emotes)).Length + 1];
            emoteKeybinds[0] = component.currentActionMap.FindAction("Emote1", false).GetBindingDisplayString(0, 0);
            emoteKeybinds[1] = component.currentActionMap.FindAction("Emote2", false).GetBindingDisplayString(0, 0);
            emoteKeybinds[(int)EmotePatch.Emotes.Middle_Finger - 1] = EmotePatch.keybinds.MiddleFinger.GetBindingDisplayString();
            emoteKeybinds[(int)EmotePatch.Emotes.Clap - 1] = EmotePatch.keybinds.Clap.GetBindingDisplayString();
            emoteKeybinds[(int)EmotePatch.Emotes.Shy - 1] = EmotePatch.keybinds.Shy.GetBindingDisplayString();
            emoteKeybinds[(int)EmotePatch.Emotes.Griddy - 1] = EmotePatch.keybinds.Griddy.GetBindingDisplayString();
            emoteKeybinds[(int)EmotePatch.Emotes.Salute - 1] = EmotePatch.keybinds.Salute.GetBindingDisplayString();
            emoteKeybinds[(int)EmotePatch.Emotes.Twerk - 1] = EmotePatch.keybinds.Twerk.GetBindingDisplayString();
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
            Mouse.current.WarpCursorPosition(center);
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
            if (Vector2.Distance(center, Mouse.current.position.ReadValue()) >= wheelMovementOffset)
            {
                float xDiff = Math.Abs(Mouse.current.position.x.ReadValue() - center.x);
                float yDiff = Math.Abs(Mouse.current.position.y.ReadValue() - center.y);
                float ignoreRadius = 235;
                float stopRadius = 470;
                bool isCenter = Math.Pow(Mouse.current.position.x.ReadValue() - center.x, 2) + Math.Pow(Mouse.current.position.y.ReadValue() - center.y, 2) < Math.Pow(ignoreRadius, 2);
                bool isOuter = Math.Pow(Mouse.current.position.x.ReadValue() - center.x, 2) + Math.Pow(Mouse.current.position.y.ReadValue() - center.y, 2) >= Math.Pow(stopRadius, 2);
                bool flag2 = Mouse.current.position.x.ReadValue() > center.x;
                bool flag3 = Mouse.current.position.y.ReadValue() > center.y;
                int corner = flag2 ? (flag3 ? 1 : 4) : (flag3 ? 2 : 3);
                float f = (Mouse.current.position.y.ReadValue() - center.y) / (Mouse.current.position.x.ReadValue() - center.x);
                float num = 180 * (corner - ((corner > 2) ? 2 : 1));
                angle = Mathf.Atan(f) * 57.295776f + num;
                if (angle == 90f)
                {
                    angle = 270f;
                }
                else
                {
                    if (angle == 270f)
                    {
                        angle = 90f;
                    }
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
            if (selectedEmoteID <= Enum.GetValues(typeof(EmotePatch.Emotes)).Length)
            {
                EmotePatch.Emotes emotes = (EmotePatch.Emotes)selectedEmoteID;
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
