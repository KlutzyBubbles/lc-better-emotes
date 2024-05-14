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
        public static bool EmoteWheelIsOpened = false;

        private static EmoteWheel SelectionWheel;

        public static GameObject WheelPrefab;

        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        private static void AwakePost(RoundManager __instance)
        {
            Plugin.Debug("EmoteKeybindPatch.AwakePost()");
            GameObject gameObject = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
            if (WheelPrefab != null)
            {
                SelectionWheel = UnityEngine.Object.Instantiate(WheelPrefab, gameObject.transform).AddComponent<EmoteWheel>();
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("EmoteKeybindPatch.StartPostfix()");
            if (Settings.Keybinds != null && !Settings.DisableModelOverride)
            {
                Settings.Keybinds.MiddleFinger.performed += onEmoteKeyMiddleFinger;
                Settings.Keybinds.Griddy.performed += onEmoteKeyGriddy;
                Settings.Keybinds.Shy.performed += onEmoteKeyShy;
                Settings.Keybinds.Clap.performed += onEmoteKeyClap;
                Settings.Keybinds.Salute.performed += onEmoteKeySalute;
                Settings.Keybinds.Prisyadka.performed += onEmoteKeyPrisyadka;
                Settings.Keybinds.Sign.performed += onEmoteKeySign;
                Settings.Keybinds.Twerk.performed += onEmoteKeyTwerk;
                Settings.Keybinds.SignSubmit.performed += onSignKeySubmit;
                Settings.Keybinds.SignCancel.performed += onSignKeyCancel;
                Settings.Keybinds.EmoteWheel.started += onEmoteKeyWheelStarted;
                Settings.Keybinds.EmoteWheel.canceled += onEmoteKeyWheelCanceled;
                Settings.Keybinds.EmoteWheelNextPage.performed += onEmoteKeyWheelNext;
                Settings.Keybinds.EmoteWheelPreviousPage.performed += onEmoteKeyWheelPrevious;
                Settings.Keybinds.MiddleFinger.Enable();
                Settings.Keybinds.Griddy.Enable();
                Settings.Keybinds.Shy.Enable();
                Settings.Keybinds.Clap.Enable();
                Settings.Keybinds.Salute.Enable();
                Settings.Keybinds.Prisyadka.Enable();
                Settings.Keybinds.Sign.Enable();
                Settings.Keybinds.Twerk.Enable();
                Settings.Keybinds.SignSubmit.Enable();
                Settings.Keybinds.SignCancel.Enable();
                Settings.Keybinds.EmoteWheel.Enable();
                Settings.Keybinds.EmoteWheelNextPage.Enable();
                Settings.Keybinds.EmoteWheelPreviousPage.Enable();
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisablePostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("EmoteKeybindPatch.OnDisablePostfix()");
            if (Settings.Keybinds != null && __instance == GameValues.localPlayerController)
            {
                Settings.Keybinds.MiddleFinger.performed -= onEmoteKeyMiddleFinger;
                Settings.Keybinds.Griddy.performed -= onEmoteKeyGriddy;
                Settings.Keybinds.Shy.performed -= onEmoteKeyShy;
                Settings.Keybinds.Clap.performed -= onEmoteKeyClap;
                Settings.Keybinds.Salute.performed -= onEmoteKeySalute;
                Settings.Keybinds.Prisyadka.performed -= onEmoteKeyPrisyadka;
                Settings.Keybinds.Sign.performed -= onEmoteKeySign;
                Settings.Keybinds.Twerk.performed -= onEmoteKeyTwerk;
                Settings.Keybinds.SignSubmit.performed -= onSignKeySubmit;
                Settings.Keybinds.SignCancel.performed -= onSignKeyCancel;
                Settings.Keybinds.EmoteWheel.started -= onEmoteKeyWheelStarted;
                Settings.Keybinds.EmoteWheel.canceled -= onEmoteKeyWheelCanceled;
                Settings.Keybinds.EmoteWheelNextPage.performed -= onEmoteKeyWheelNext;
                Settings.Keybinds.EmoteWheelPreviousPage.performed -= onEmoteKeyWheelPrevious;
                Settings.Keybinds.MiddleFinger.Disable();
                Settings.Keybinds.Griddy.Disable();
                Settings.Keybinds.Shy.Disable();
                Settings.Keybinds.Clap.Disable();
                Settings.Keybinds.Salute.Disable();
                Settings.Keybinds.Prisyadka.Disable();
                Settings.Keybinds.Sign.Disable();
                Settings.Keybinds.Twerk.Disable();
                Settings.Keybinds.SignSubmit.Disable();
                Settings.Keybinds.SignCancel.Disable();
                Settings.Keybinds.EmoteWheel.Disable();
                Settings.Keybinds.EmoteWheelNextPage.Disable();
                Settings.Keybinds.EmoteWheelPreviousPage.Disable();
            }
        }

        public static void onSignKeySubmit(InputAction.CallbackContext context)
        {
            Plugin.Debug("onSignKeySubmit()");
            if (Keyboard.current[Key.RightShift].isPressed || Keyboard.current[Key.LeftShift].isPressed)
            {
                Plugin.Debug("They have one of the shifts pressed");
                return;
            }
            if (!Settings.DisableModelOverride && LocalPlayer.CustomSignInputField != null && LocalPlayer.CustomSignInputField.IsSignUIOpen)
            {
                LocalPlayer.CustomSignInputField.SubmitText();
            }
        }

        public static void onSignKeyCancel(InputAction.CallbackContext context)
        {
            Plugin.Debug("onSignKeyCancel()");
            if (!Settings.DisableModelOverride && LocalPlayer.CustomSignInputField != null && LocalPlayer.CustomSignInputField.IsSignUIOpen)
            {
                LocalPlayer.CustomSignInputField.Close(true);
            }
        }

        public static void onEmoteKeyWheelStarted(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelStarted()");
            if (!EmoteWheelIsOpened
                && GameValues.localPlayerController != null
                && !GameValues.localPlayerController.isPlayerDead
                && !GameValues.localPlayerController.inTerminalMenu
                && !GameValues.localPlayerController.isTypingChat
                && !GameValues.localPlayerController.quickMenuManager.isMenuOpen
                && (LocalPlayer.CustomSignInputField == null || !LocalPlayer.CustomSignInputField.IsSignUIOpen))
            {
                EmoteWheelIsOpened = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                SelectionWheel?.gameObject?.SetActive(EmoteWheelIsOpened);
                GameValues.localPlayerController.quickMenuManager.isMenuOpen = true;
                GameValues.localPlayerController.disableLookInput = true;
            }
        }

        public static void onEmoteKeyWheelCanceled(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelCanceled()");
            if (EmoteWheelIsOpened && GameValues.localPlayerController != null && SelectionWheel != null)
            {
                GameValues.localPlayerController.quickMenuManager.isMenuOpen = false;
                GameValues.localPlayerController.disableLookInput = false;
                if (SelectionWheel.selectedEmoteID >= Settings.EnabledList.Length)
                {
                    if (SelectionWheel.stopEmote)
                    {
                        GameValues.localPlayerController.performingEmote = false;
                        GameValues.localPlayerController.StopPerformingEmoteServerRpc();
                        GameValues.localPlayerController.timeSinceStartingEmote = 0f;
                    }
                }
                else
                {
                    CheckEmoteInput(context, Settings.EnabledList[SelectionWheel.selectedEmoteID], SelectionWheel.selectedEmoteID, GameValues.localPlayerController);
                }
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                EmoteWheelIsOpened = false;
                SelectionWheel.gameObject.SetActive(EmoteWheelIsOpened);
            }
        }

        public static void onEmoteKeyWheelNext(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelNext()");
            if (EmoteWheelIsOpened)
            {
                SelectionWheel?.alterPage(1);
            }
        }

        public static void onEmoteKeyWheelPrevious(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelPrevious()");
            if (EmoteWheelIsOpened)
            {
                SelectionWheel?.alterPage(-1);
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
            CheckEmoteInput(context, Settings.EnabledList[(int)emote], (int)emote, GameValues.localPlayerController);
        }

        private static void CheckEmoteInput(InputAction.CallbackContext context, bool enabled, int emoteID, PlayerControllerB player)
        {
            Plugin.Debug($"CheckEmoteInput({enabled}, {emoteID})");
            if (enabled && player != null)
            {
                EmoteControllerPlayer.emoteControllerLocal.TryPerformingEmoteLocal(Plugin.temp);
                // player.PerformEmote(context, emoteID);
            }
        }
    }
}
