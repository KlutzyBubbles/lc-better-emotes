using BetterEmote.AssetScripts;
using BetterEmote.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace BetterEmote.Patches
{
    internal class EmotePatch
    {
        public static Keybinds keybinds;

        public static AssetBundle animationsBundle;

        public static AssetBundle animatorBundle;

        public static bool stopOnOuter = false;

        public static bool[] enabledList;
        public static string[] defaultKeyList;
        public static string[] defaultControllerList;

        public static string emoteWheelKey = "<Keyboard>/v";
        public static string emoteWheelController = "<Gamepad>/leftShoulder";
        public static string emoteWheelControllerMove = "<Gamepad>/rightStick";

        public static float griddySpeed = 0.5f;
        public static float emoteCooldown = 0.5f;

        public static RuntimeAnimatorController local;

        public static RuntimeAnimatorController others;

        private static int currentEmoteID;

        private static float movSpeed;

        public static bool incompatibleStuff;

        public static bool emoteWheelIsOpened;

        public static bool[] playersPerformingEmotes = new bool[40];

        public static GameObject wheel;

        private static EmoteWheel selectionWheel;

        //public static GameObject ButtonPrefab;
        //public static GameObject SettingsPrefab;
        public static GameObject LegsPrefab;
        public static GameObject SignPrefab;
        public static GameObject SignUIPrefab;
        public static GameObject WheelPrefab;
        private static GameObject localPlayerLevelBadge;
        private static GameObject localPlayerBetaBadge;

        private static SignUI customSignInputField;

        private static SyncAnimatorToOthers syncAnimator;

        private static bool isPlayerFirstFrame;
        private static bool isFirstTimeOnMenu;
        private static bool isPlayerSpawning;

        private static Emote[] doubleEmotesIDS = {
            Emote.Middle_Finger,
            Emote.Clap
        };

        public static bool isLocalArmsSeparatedFromCamera;

        private static Transform freeArmsTarget;
        private static Transform lockedArmsTarget;
        private static Transform legsMesh;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            GameObject gameObject = __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject;
            CustomAudioAnimationEvent customAudioAnimationEvent = gameObject.AddComponent<CustomAudioAnimationEvent>();
            customAudioAnimationEvent.player = __instance;
            movSpeed = __instance.movementSpeed;
            if (UnityEngine.Object.FindObjectsOfType(typeof(EmoteWheel)).Length == 0)
            {
                GameObject original = animationsBundle.LoadAsset<GameObject>("Assets/MoreEmotes/Resources/MoreEmotesMenu.prefab");
                GameObject gameObject2 = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
                if (wheel != null)
                {
                    UnityEngine.Object.Destroy(wheel.gameObject);
                }
                wheel = UnityEngine.Object.Instantiate(original, gameObject2.transform);
                selectionWheel = wheel.AddComponent<EmoteWheel>();
                EmoteWheel.emoteNames = new string[EmoteDefs.getEmoteCount() + 1];
                foreach (string name in Enum.GetNames(typeof(Emote)))
                {
                    EmoteWheel.emoteNames[EmoteDefs.getEmoteNumber(name) - 1] = name;
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
            keybinds.MiddleFinger.Enable();
            keybinds.Griddy.Enable();
            keybinds.Shy.Enable();
            keybinds.Clap.Enable();
            keybinds.Salute.Enable();
            keybinds.Twerk.Enable();
            keybinds.EmoteWheel.Enable();
        }

        [HarmonyPatch(typeof(PlayerControllerB), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisablePostfix(PlayerControllerB __instance)
        {
            if (__instance == GameValues.localPlayerController)
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
            if (!emoteWheelIsOpened && !GameValues.localPlayerController.isPlayerDead && !GameValues.localPlayerController.inTerminalMenu && !GameValues.localPlayerController.quickMenuManager.isMenuOpen)
            {
                emoteWheelIsOpened = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                wheel.SetActive(emoteWheelIsOpened);
                GameValues.localPlayerController.quickMenuManager.isMenuOpen = true;
                GameValues.localPlayerController.disableLookInput = true;
            }
        }

        public static void onEmoteKeyWheelCanceled(InputAction.CallbackContext context)
        {
            GameValues.localPlayerController.quickMenuManager.isMenuOpen = false;
            GameValues.localPlayerController.disableLookInput = false;
            if (selectionWheel.selectedEmoteID >= enabledList.Length)
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
                CheckEmoteInput(context, enabledList[selectionWheel.selectedEmoteID], selectionWheel.selectedEmoteID, Utils.localPlayerController);
            }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            emoteWheelIsOpened = false;
            wheel.SetActive(emoteWheelIsOpened);
        }

        public static void onEmoteKeyMiddleFinger(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emote.Middle_Finger);
        }

        public static void onEmoteKeyGriddy(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emote.Griddy);
        }

        public static void onEmoteKeyShy(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emote.Shy);
        }

        public static void onEmoteKeyClap(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emote.Clap);
        }

        public static void onEmoteKeySalute(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emote.Salute);
        }

        public static void onEmoteKeyTwerk(InputAction.CallbackContext context)
        {
            onEmoteKeyPerformed(context, Emote.Twerk);
        }

        public static void onEmoteKeyPerformed(InputAction.CallbackContext context, Emote emote)
        {
            CheckEmoteInput(context, enabledList[(int)emote], (int)emote, GameValues.localPlayerController);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePrefix(PlayerControllerB __instance)
        {
            bool performingEmote = __instance.performingEmote;
            checked
            {
                if (performingEmote)
                {
                    playersPerformingEmotes[(int)((IntPtr)__instance.playerClientId)] = true;
                }
                bool flag2 = !__instance.performingEmote && playersPerformingEmotes[(int)((IntPtr)__instance.playerClientId)];
                if (!__instance.performingEmote && playersPerformingEmotes[(int)((IntPtr)__instance.playerClientId)])
                {
                    playersPerformingEmotes[(int)((IntPtr)__instance.playerClientId)] = false;
                    ResetIKWeights(__instance);
                }
            }
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
                        __instance.movementSpeed = __instance.CheckConditionsForEmote() && currentEmoteID == (int)Emote.Griddy && __instance.performingEmote ? movSpeed * griddySpeed : movSpeed;
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

        private static void OnFirstLocalPlayerFrameWithNewAnimator(PlayerControllerB player)
        {
            isPlayerFirstFrame = false;
            syncAnimator = player.GetComponent<SyncAnimatorToOthers>();
            customSignInputField.Player = player;
            freeArmsTarget = UnityEngine.Object.Instantiate(player.localArmsRotationTarget, player.localArmsRotationTarget.parent.parent);
            lockedArmsTarget = player.localArmsRotationTarget;
            Transform transform = player.transform.Find("ScavengerModel").Find("metarig").Find("spine").Find("spine.001").Find("spine.002").Find("spine.003");
            localPlayerLevelBadge = transform.Find("LevelSticker").gameObject;
            localPlayerBetaBadge = transform.Find("BetaBadge").gameObject;
            player.SpawnPlayerAnimation();
        }

        private static void SpawnSign(PlayerControllerB player)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(SignPrefab, player.transform.Find("ScavengerModel").transform.Find("metarig").transform);
            gameObject.transform.SetSiblingIndex(6);
            gameObject.name = "Sign";
            gameObject.transform.localPosition = new Vector3(0.029f, -0.45f, 1.3217f);
            gameObject.transform.localRotation = Quaternion.Euler(65.556f, 180f, 180f);
        }

        private static void SpawnLegs(PlayerControllerB player)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(LegsPrefab, player.playerBodyAnimator.transform.parent.transform);
            legsMesh = gameObject.transform.Find("Mesh");
            legsMesh.transform.parent = player.playerBodyAnimator.transform.parent;
            legsMesh.name = "LEGS";
            GameObject gameObject2 = gameObject.transform.Find("Armature").gameObject;
            gameObject2.transform.parent = player.playerBodyAnimator.transform;
            gameObject2.name = "FistPersonLegs";
            gameObject2.transform.position = new Vector3(0f, 0.197f, 0f);
            gameObject2.transform.localScale = new Vector3(13.99568f, 13.99568f, 13.99568f);
            UnityEngine.Object.Destroy(gameObject);
        }

        private static void ResetIKWeights(PlayerControllerB player)
        {
            Transform transform = player.playerBodyAnimator.transform.Find("Rig 1");
            ChainIKConstraint component = transform.Find("RightArm").GetComponent<ChainIKConstraint>();
            ChainIKConstraint component2 = transform.Find("LeftArm").GetComponent<ChainIKConstraint>();
            TwoBoneIKConstraint component3 = transform.Find("RightLeg").GetComponent<TwoBoneIKConstraint>();
            TwoBoneIKConstraint component4 = transform.Find("LeftLeg").GetComponent<TwoBoneIKConstraint>();
            Transform transform2 = player.playerBodyAnimator.transform.Find("ScavengerModelArmsOnly").Find("metarig").Find("spine.003").Find("RigArms");
            ChainIKConstraint component5 = transform2.Find("RightArm").GetComponent<ChainIKConstraint>();
            ChainIKConstraint component6 = transform2.Find("LeftArm").GetComponent<ChainIKConstraint>();
            component.weight = 1f;
            component2.weight = 1f;
            component.weight = 1f;
            component4.weight = 1f;
            component5.weight = 1f;
            component6.weight = 1f;
        }

        private static void UpdateLegsMaterial(PlayerControllerB player)
        {
            legsMesh.GetComponent<SkinnedMeshRenderer>().material = player.playerBodyAnimator.transform.parent.transform.Find("LOD1").gameObject.GetComponent<SkinnedMeshRenderer>().material;
        }

        private static void TogglePlayerBadges(PlayerControllerB player, bool enabled)
        {
            if (localPlayerBetaBadge != null)
            {
                localPlayerBetaBadge.GetComponent<MeshRenderer>().enabled = enabled;
            }
            if (localPlayerLevelBadge != null)
            {
                localPlayerLevelBadge.GetComponent<MeshRenderer>().enabled = enabled;
            }
            else
            {
                Plugin.StaticLogger.LogError("Couldn't find the level badge");
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "PerformEmote")]
        [HarmonyPrefix]
        private static void PerformEmotePrefix(ref InputAction.CallbackContext context, int emoteID, PlayerControllerB __instance)
        {
            currentEmoteID = emoteID;
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || __instance.IsServer && !__instance.isHostPlayerObject) && !__instance.isTestingPlayer)
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
            if (currentEmoteID == (int)Emote.Griddy && griddySpeed != 0)
            {
                __result = !__instance.inSpecialInteractAnimation && !__instance.isPlayerDead && !__instance.isJumping && __instance.moveInputVector.x == 0f && !__instance.isSprinting && !__instance.isCrouching && !__instance.isClimbingLadder && !__instance.isGrabbingObjectAnimation && !__instance.inTerminalMenu && !__instance.isTypingChat;
                return false;
            }
            return true;
        }
    }
}
