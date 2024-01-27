using BetterEmote.AssetScripts;
using BetterEmote.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterEmote.Patches
{
    internal class EmoteKeybindPatch
    {
        public static bool emoteWheelIsOpened;

        private static EmoteWheel selectionWheel;

        public static GameObject WheelPrefab;

        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        private static void AwakePost(RoundManager __instance)
        {
            Plugin.Debug("EmoteKeybindPatch.AwakePost()");
            GameObject gameObject = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
            selectionWheel = UnityEngine.Object.Instantiate(WheelPrefab, gameObject.transform).AddComponent<EmoteWheel>();
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("EmoteKeybindPatch.StartPostfix()");
            if (!Settings.disableSelfEmote)
            {
                Settings.keybinds.MiddleFinger.performed += onEmoteKeyMiddleFinger;
                Settings.keybinds.Griddy.performed += onEmoteKeyGriddy;
                Settings.keybinds.Shy.performed += onEmoteKeyShy;
                Settings.keybinds.Clap.performed += onEmoteKeyClap;
                Settings.keybinds.Salute.performed += onEmoteKeySalute;
                Settings.keybinds.Prisyadka.performed += onEmoteKeyPrisyadka;
                Settings.keybinds.Sign.performed += onEmoteKeySign;
                Settings.keybinds.Twerk.performed += onEmoteKeyTwerk;
                Settings.keybinds.EmoteWheel.started += onEmoteKeyWheelStarted;
                Settings.keybinds.EmoteWheel.canceled += onEmoteKeyWheelCanceled;
                Settings.keybinds.EmoteWheelNextPage.performed += onEmoteKeyWheelNext;
                Settings.keybinds.EmoteWheelPreviousPage.performed += onEmoteKeyWheelPrevious;
                Settings.keybinds.MiddleFinger.Enable();
                Settings.keybinds.Griddy.Enable();
                Settings.keybinds.Shy.Enable();
                Settings.keybinds.Clap.Enable();
                Settings.keybinds.Salute.Enable();
                Settings.keybinds.Prisyadka.Enable();
                Settings.keybinds.Sign.Enable();
                Settings.keybinds.Twerk.Enable();
                Settings.keybinds.EmoteWheel.Enable();
                Settings.keybinds.EmoteWheelNextPage.Enable();
                Settings.keybinds.EmoteWheelPreviousPage.Enable();
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisablePostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("EmoteKeybindPatch.OnDisablePostfix()");
            if (__instance == GameValues.localPlayerController)
            {
                Settings.keybinds.MiddleFinger.performed -= onEmoteKeyMiddleFinger;
                Settings.keybinds.Griddy.performed -= onEmoteKeyGriddy;
                Settings.keybinds.Shy.performed -= onEmoteKeyShy;
                Settings.keybinds.Clap.performed -= onEmoteKeyClap;
                Settings.keybinds.Salute.performed -= onEmoteKeySalute;
                Settings.keybinds.Prisyadka.performed -= onEmoteKeyPrisyadka;
                Settings.keybinds.Sign.performed -= onEmoteKeySign;
                Settings.keybinds.Twerk.performed -= onEmoteKeyTwerk;
                Settings.keybinds.EmoteWheel.started -= onEmoteKeyWheelStarted;
                Settings.keybinds.EmoteWheel.canceled -= onEmoteKeyWheelCanceled;
                Settings.keybinds.EmoteWheelNextPage.performed -= onEmoteKeyWheelNext;
                Settings.keybinds.EmoteWheelPreviousPage.performed -= onEmoteKeyWheelPrevious;
                Settings.keybinds.MiddleFinger.Disable();
                Settings.keybinds.Griddy.Disable();
                Settings.keybinds.Shy.Disable();
                Settings.keybinds.Clap.Disable();
                Settings.keybinds.Salute.Disable();
                Settings.keybinds.Prisyadka.Disable();
                Settings.keybinds.Sign.Disable();
                Settings.keybinds.Twerk.Disable();
                Settings.keybinds.EmoteWheel.Disable();
                Settings.keybinds.EmoteWheelNextPage.Disable();
                Settings.keybinds.EmoteWheelPreviousPage.Disable();
            }
        }


        public static void onEmoteKeyWheelStarted(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelStarted()");
            if (!emoteWheelIsOpened
                && !GameValues.localPlayerController.isPlayerDead
                && !GameValues.localPlayerController.inTerminalMenu
                && !GameValues.localPlayerController.isTypingChat
                && !GameValues.localPlayerController.quickMenuManager.isMenuOpen
                && !EmotePatch.customSignInputField.IsSignUIOpen)
            {
                emoteWheelIsOpened = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                selectionWheel.gameObject.SetActive(emoteWheelIsOpened);
                GameValues.localPlayerController.quickMenuManager.isMenuOpen = true;
                GameValues.localPlayerController.disableLookInput = true;
            }
        }

        public static void onEmoteKeyWheelCanceled(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelCanceled()");
            if (emoteWheelIsOpened)
            {
                GameValues.localPlayerController.quickMenuManager.isMenuOpen = false;
                GameValues.localPlayerController.disableLookInput = false;
                if (selectionWheel.selectedEmoteID >= Settings.enabledList.Length)
                {
                    if (selectionWheel.stopEmote)
                    {
                        GameValues.localPlayerController.performingEmote = false;
                        GameValues.localPlayerController.StopPerformingEmoteServerRpc();
                        GameValues.localPlayerController.timeSinceStartingEmote = 0f;
                    }
                }
                else
                {
                    CheckEmoteInput(context, Settings.enabledList[selectionWheel.selectedEmoteID], selectionWheel.selectedEmoteID, GameValues.localPlayerController);
                }
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                emoteWheelIsOpened = false;
                selectionWheel.gameObject.SetActive(emoteWheelIsOpened);
            }
        }

        public static void onEmoteKeyWheelNext(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelNext()");
            if (emoteWheelIsOpened)
            {
                selectionWheel.alterPage(1);
            }
        }

        public static void onEmoteKeyWheelPrevious(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelPrevious()");
            if (emoteWheelIsOpened)
            {
                selectionWheel.alterPage(-1);
            }
        }

        public static void onEmoteKeyMiddleFinger(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyMiddleFinger()");
            onEmoteKeyPerformed(context, Emote.Middle_Finger);
        }

        public static void onEmoteKeyGriddy(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyGriddy()");
            onEmoteKeyPerformed(context, Emote.Griddy);
        }

        public static void onEmoteKeyShy(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyShy()");
            onEmoteKeyPerformed(context, Emote.Shy);
        }

        public static void onEmoteKeyClap(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyClap()");
            onEmoteKeyPerformed(context, Emote.Clap);
        }

        public static void onEmoteKeySalute(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeySalute()");
            onEmoteKeyPerformed(context, Emote.Salute);
        }

        public static void onEmoteKeyPrisyadka(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyPrisyadka()");
            onEmoteKeyPerformed(context, Emote.Prisyadka);
        }

        public static void onEmoteKeySign(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeySign()");
            onEmoteKeyPerformed(context, Emote.Sign);
        }

        public static void onEmoteKeyTwerk(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyTwerk()");
            onEmoteKeyPerformed(context, Emote.Twerk);
        }

        public static void onEmoteKeyPerformed(InputAction.CallbackContext context, Emote emote)
        {
            CheckEmoteInput(context, Settings.enabledList[(int)emote], (int)emote, GameValues.localPlayerController);
        }

        private static void CheckEmoteInput(InputAction.CallbackContext context, bool enabled, int emoteID, PlayerControllerB player)
        {
            Plugin.Debug($"CheckEmoteInput({enabled}, {emoteID})");
            if (enabled)
            {
                player.PerformEmote(context, emoteID);
            }
        }
    }
}
