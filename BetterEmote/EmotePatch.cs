using GameNetcodeStuff;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterEmote
{
    internal class EmotePatch
    {
        public static Keybinds keybinds;

        public static AssetBundle animationsBundle;

        public static AssetBundle animatorBundle;

        public static bool stopOnOuter = false;

        public static bool[] enabledList;
        public static string[] defaultKeyList;

        public static string emoteWheelKey = "<Keyboard>/v";

        public static float griddySpeed = 0.5f;
        public static float emoteCooldown = 0.5f;

        public static RuntimeAnimatorController local;

        public static RuntimeAnimatorController others;

        private static int currentEmoteID;

        private static float movSpeed;

        public static bool incompatibleStuff;

        public static bool emoteWheelIsOpened;

        public static GameObject wheel;

        private static SelectionWheel selectionWheel;

        public enum Emotes:int
        {
            Dance = 1,
            Point = 2,
            Middle_Finger = 3,
            Clap = 4,
            Shy = 5,
            Griddy = 6,
            Twerk = 7,
            Salute = 8
        }

        private static PlayerControllerB localPlayerController
        {
            get
            {
                StartOfRound instance = StartOfRound.Instance;
                return (instance != null) ? instance.localPlayerController : null;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            GameObject gameObject = __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject;
            CustomAudioAnimationEvent customAudioAnimationEvent = gameObject.AddComponent<CustomAudioAnimationEvent>();
            customAudioAnimationEvent.player = __instance;
            movSpeed = __instance.movementSpeed;
            if (UnityEngine.Object.FindObjectsOfType(typeof(SelectionWheel)).Length == 0)
            {
                GameObject original = animationsBundle.LoadAsset<GameObject>("Assets/MoreEmotes/Resources/MoreEmotesMenu.prefab");
                GameObject gameObject2 = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
                if (wheel != null)
                {
                    UnityEngine.Object.Destroy(wheel.gameObject);
                }
                wheel = UnityEngine.Object.Instantiate(original, gameObject2.transform);
                selectionWheel = wheel.AddComponent<SelectionWheel>();
                SelectionWheel.emoteNames = new string[Enum.GetNames(typeof(Emotes)).Length + 1];
                foreach (string name in Enum.GetNames(typeof(Emotes)))
                {
                    SelectionWheel.emoteNames[(int)Enum.Parse(typeof(Emotes), name) - 1] = name;
                }
            }
            keybinds.MiddleFinger.performed += onEmoteKeyMiddleFinger;
            keybinds.Griddy.performed += onEmoteKeyGriddy;
            keybinds.Shy.performed += onEmoteKeyShy;
            keybinds.Clap.performed += onEmoteKeyClap;
            keybinds.Salute.performed += onEmoteKeySalute;
            keybinds.Twerk.performed += onEmoteKeyTwerk;
            keybinds.EmoteWheel.started += onEmoteKeyWheelStarted;
            keybinds.EmoteWheel.canceled += onEmoteKeyWheelCanceled;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisablePostfix(PlayerControllerB __instance)
        {
            if (__instance == localPlayerController)
            {
                keybinds.MiddleFinger.performed -= onEmoteKeyMiddleFinger;
                keybinds.Griddy.performed -= onEmoteKeyGriddy;
                keybinds.Shy.performed -= onEmoteKeyShy;
                keybinds.Clap.performed -= onEmoteKeyClap;
                keybinds.Salute.performed -= onEmoteKeySalute;
                keybinds.Twerk.performed -= onEmoteKeyTwerk;
                keybinds.EmoteWheel.started -= onEmoteKeyWheelStarted;
                keybinds.EmoteWheel.canceled -= onEmoteKeyWheelCanceled;
                keybinds.MiddleFinger.Disable();
                keybinds.Griddy.Disable();
                keybinds.Shy.Disable();
                keybinds.Clap.Disable();
                keybinds.Salute.Disable();
                keybinds.Twerk.Disable();
                keybinds.EmoteWheel.Disable();
            }
        }

        public static void onEmoteKeyWheelStarted(InputAction.CallbackContext context)
        {
            if (!emoteWheelIsOpened && !localPlayerController.isPlayerDead && !localPlayerController.inTerminalMenu && !localPlayerController.quickMenuManager.isMenuOpen)
            {
                emoteWheelIsOpened = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                wheel.SetActive(emoteWheelIsOpened);
                localPlayerController.quickMenuManager.isMenuOpen = true;
                localPlayerController.disableLookInput = true;
            }
        }

        public static void onEmoteKeyWheelCanceled(InputAction.CallbackContext context)
        {
            localPlayerController.quickMenuManager.isMenuOpen = false;
            localPlayerController.disableLookInput = false;
            if (selectionWheel.selectedEmoteID >= enabledList.Length)
            {
                if (selectionWheel.stopEmote)
                {
                    localPlayerController.performingEmote = false;
                    localPlayerController.StopPerformingEmoteServerRpc();
                    localPlayerController.timeSinceStartingEmote = 0f;
                }
            }
            else
            {
                CheckEmoteInput(context, enabledList[selectionWheel.selectedEmoteID], selectionWheel.selectedEmoteID, localPlayerController);
            }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            emoteWheelIsOpened = false;
            wheel.SetActive(emoteWheelIsOpened);
        }

        public static void onEmoteKeyMiddleFinger(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emotes.Middle_Finger);
        }

        public static void onEmoteKeyGriddy(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emotes.Griddy);
        }

        public static void onEmoteKeyShy(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emotes.Shy);
        }

        public static void onEmoteKeyClap(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emotes.Clap);
        }

        public static void onEmoteKeySalute(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emotes.Salute);
        }

        public static void onEmoteKeyTwerk(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emotes.Twerk);
        }

        public static void onEmoteKeyPerformed(InputAction.CallbackContext context, Emotes emote)
        {
            CheckEmoteInput(context, enabledList[(int)emote], (int)emote, localPlayerController);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePrefix(PlayerControllerB __instance)
        {
            if (!__instance.isPlayerControlled || !__instance.IsOwner)
            {
                __instance.playerBodyAnimator.runtimeAnimatorController = others;
            }
            else
            {
                if (__instance.playerBodyAnimator != local)
                {
                    __instance.playerBodyAnimator.runtimeAnimatorController = local;
                }
                currentEmoteID = __instance.playerBodyAnimator.GetInteger("emoteNumber");
                if (!incompatibleStuff)
                {
                    if (__instance.movementSpeed != 0 && griddySpeed != 0)
                    {
                        __instance.movementSpeed = (__instance.CheckConditionsForEmote() && currentEmoteID == (int)Emotes.Griddy && __instance.performingEmote) ? (movSpeed * (griddySpeed)) : movSpeed;
                    }
                }
            }
        }

        private static void CheckEmoteInput(InputAction.CallbackContext context, bool enabled, int emoteID, PlayerControllerB player)
        {
            if (enabled)
            {
                player.PerformEmote(context, emoteID);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "PerformEmote")]
        [HarmonyPrefix]
        private static void PerformEmotePrefix(ref InputAction.CallbackContext context, int emoteID, PlayerControllerB __instance)
        {
            currentEmoteID = emoteID;
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || (__instance.IsServer && !__instance.isHostPlayerObject)) && !__instance.isTestingPlayer)
            {
                return;
            }
            if (__instance.CheckConditionsForEmote())
            {
                if (__instance.timeSinceStartingEmote >= emoteCooldown)
                {
                    __instance.timeSinceStartingEmote = 0f;
                    __instance.performingEmote = true;
                    __instance.playerBodyAnimator.SetInteger("emoteNumber", emoteID);
                    __instance.StartPerformingEmoteServerRpc();
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "CheckConditionsForEmote")]
        [HarmonyPrefix]
        private static bool prefixCheckConditions(ref bool __result, PlayerControllerB __instance)
        {
            bool result;
            if (currentEmoteID == (int)Emotes.Griddy && griddySpeed != 0)
            {
                __result = (!__instance.inSpecialInteractAnimation && !__instance.isPlayerDead && !__instance.isJumping && __instance.moveInputVector.x == 0f && !__instance.isSprinting && !__instance.isCrouching && !__instance.isClimbingLadder && !__instance.isGrabbingObjectAnimation && !__instance.inTerminalMenu && !__instance.isTypingChat);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}
