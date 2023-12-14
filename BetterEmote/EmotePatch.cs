using GameNetcodeStuff;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterEmote
{
    internal class EmotePatch
    {
        public static Keybinds keybinds = new Keybinds();

        public static AssetBundle animationsBundle;

        public static AssetBundle animatorBundle;

        public static bool stopOnOuter = false;

        public static bool[] enabledList;

        public static float griddySpeed = 0.5f;
        public static float emoteCooldown = 0.5f;

        private static InputAction.CallbackContext context;

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
            keybinds.MiddleFinger.performed += delegate
            {
                CheckEmoteInput(enabledList[(int)Emotes.Middle_Finger], (int)Emotes.Middle_Finger, __instance);
            };
            keybinds.Griddy.performed += delegate
            {
                CheckEmoteInput(enabledList[(int)Emotes.Griddy], (int)Emotes.Griddy, __instance);
            };
            keybinds.Shy.performed += delegate
            {
                CheckEmoteInput(enabledList[(int)Emotes.Shy], (int)Emotes.Shy, __instance);
            };
            keybinds.Clap.performed += delegate
            {
                CheckEmoteInput(enabledList[(int)Emotes.Clap], (int)Emotes.Clap, __instance);
            };
            keybinds.Salute.performed += delegate
            {
                CheckEmoteInput(enabledList[(int)Emotes.Salute], (int)Emotes.Salute, __instance);
            };
            keybinds.Twerk.performed += delegate
            {
                CheckEmoteInput(enabledList[(int)Emotes.Twerk], (int)Emotes.Twerk, __instance);
            };
            keybinds.EmoteWheel.started += delegate
            {
                if (!emoteWheelIsOpened && !__instance.isPlayerDead && !__instance.inTerminalMenu && !__instance.quickMenuManager.isMenuOpen)
                {
                    emoteWheelIsOpened = true;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;
                    wheel.SetActive(emoteWheelIsOpened);
                    __instance.quickMenuManager.isMenuOpen = true;
                    __instance.disableLookInput = true;
                }
            };
            keybinds.EmoteWheel.canceled += delegate
            {
                __instance.quickMenuManager.isMenuOpen = false;
                __instance.disableLookInput = false;
                if (selectionWheel.selectedEmoteID >= enabledList.Length)
                {
                    if (selectionWheel.stopEmote)
                    {
                        __instance.performingEmote = false;
                        __instance.StopPerformingEmoteServerRpc();
                        __instance.timeSinceStartingEmote = 0f;
                    }
                }
                else
                {
                    CheckEmoteInput(enabledList[selectionWheel.selectedEmoteID], selectionWheel.selectedEmoteID, __instance);
                }
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                emoteWheelIsOpened = false;
                wheel.SetActive(emoteWheelIsOpened);
            };
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
                bool? conditionsOpt = Traverse.Create(__instance).Method("CheckConditionsForEmote").GetValue() as bool?;
                bool conditions = conditionsOpt ?? false;
                currentEmoteID = __instance.playerBodyAnimator.GetInteger("emoteNumber");
                if (!incompatibleStuff)
                {
                    __instance.movementSpeed = (conditions && currentEmoteID == (int)Emotes.Griddy && __instance.performingEmote) ? (movSpeed * (griddySpeed)) : movSpeed;
                }
            }
        }

        private static void CheckEmoteInput(bool enabled, int emoteID, PlayerControllerB player)
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
            bool? conditionsOpt = Traverse.Create(__instance).Method("CheckConditionsForEmote").GetValue() as bool?;
            bool conditions = conditionsOpt ?? false;
            if (conditions)
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
            bool? isJumpingOpt = Traverse.Create(__instance).Field("isJumping").GetValue() as bool?;
            bool isJumping = isJumpingOpt ?? false;
            bool result;
            if (currentEmoteID == (int)Emotes.Griddy)
            {
                __result = (!__instance.inSpecialInteractAnimation && !__instance.isPlayerDead && !isJumping && __instance.moveInputVector.x == 0f && !__instance.isSprinting && !__instance.isCrouching && !__instance.isClimbingLadder && !__instance.isGrabbingObjectAnimation && !__instance.inTerminalMenu && !__instance.isTypingChat);
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
