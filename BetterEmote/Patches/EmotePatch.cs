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
        public static float prisyadkaSpeed = 0.34f;
        public static float emoteCooldown = 0.5f;

        public static RuntimeAnimatorController local;

        public static RuntimeAnimatorController others;

        private static int currentEmoteID;

        private static float movSpeed;

        public static bool incompatibleStuff;

        public static bool emoteWheelIsOpened;

        public static bool[] playersPerformingEmotes = new bool[40];

        private static EmoteWheel selectionWheel;

        //public static GameObject ButtonPrefab;
        //public static GameObject SettingsPrefab;
        public static GameObject LegsPrefab;
        public static GameObject SignPrefab;
        public static GameObject SignUIPrefab;
        public static GameObject WheelPrefab;
        private static GameObject localPlayerLevelBadge;
        private static GameObject localPlayerBetaBadge;

        public static SignUI customSignInputField;

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

        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        private static void AwakePost(RoundManager __instance)
        {
            Plugin.Debug("AwakePost()");
            GameObject gameObject = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
            selectionWheel = UnityEngine.Object.Instantiate(WheelPrefab, gameObject.transform).AddComponent<EmoteWheel>();
            EmoteWheel.emoteNames = new string[EmoteDefs.getEmoteCount() + 1];
            foreach (string name in Enum.GetNames(typeof(Emote)))
            {
                EmoteWheel.emoteNames[EmoteDefs.getEmoteNumber(name) - 1] = name;
            }
            customSignInputField = UnityEngine.Object.Instantiate(SignUIPrefab, gameObject.transform).AddComponent<SignUI>();
            isPlayerFirstFrame = true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("StartPostfix()");
            GameObject gameObject = __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject;
            CustomAudioAnimationEvent customAudioAnimationEvent = gameObject.AddComponent<CustomAudioAnimationEvent>();
            customAudioAnimationEvent.player = __instance;
            // __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject.AddComponent<CustomAudioAnimationEvent>().player = __instance;
            movSpeed = __instance.movementSpeed;
            __instance.gameObject.AddComponent<CustomAnimationObjects>();
            SpawnSign(__instance);
            /*
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
            */
            keybinds.MiddleFinger.performed += onEmoteKeyMiddleFinger;
            keybinds.Griddy.performed += onEmoteKeyGriddy;
            keybinds.Shy.performed += onEmoteKeyShy;
            keybinds.Clap.performed += onEmoteKeyClap;
            keybinds.Salute.performed += onEmoteKeySalute;
            keybinds.Prisyadka.performed -= onEmoteKeyPrisyadka;
            keybinds.Sign.performed -= onEmoteKeySign;
            keybinds.Twerk.performed += onEmoteKeyTwerk;
            keybinds.EmoteWheel.started += onEmoteKeyWheelStarted;
            keybinds.EmoteWheel.canceled += onEmoteKeyWheelCanceled;
            keybinds.MiddleFinger.Enable();
            keybinds.Griddy.Enable();
            keybinds.Shy.Enable();
            keybinds.Clap.Enable();
            keybinds.Salute.Enable();
            keybinds.Prisyadka.Enable();
            keybinds.Sign.Enable();
            keybinds.Twerk.Enable();
            keybinds.EmoteWheel.Enable();
        }

        [HarmonyPatch(typeof(PlayerControllerB), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisablePostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("OnDisablePostfix()");
            if (__instance == GameValues.localPlayerController)
            {
                keybinds.MiddleFinger.performed -= onEmoteKeyMiddleFinger;
                keybinds.Griddy.performed -= onEmoteKeyGriddy;
                keybinds.Shy.performed -= onEmoteKeyShy;
                keybinds.Clap.performed -= onEmoteKeyClap;
                keybinds.Salute.performed -= onEmoteKeySalute;
                keybinds.Prisyadka.performed -= onEmoteKeyPrisyadka;
                keybinds.Sign.performed -= onEmoteKeySign;
                keybinds.Twerk.performed -= onEmoteKeyTwerk;
                keybinds.EmoteWheel.started -= onEmoteKeyWheelStarted;
                keybinds.EmoteWheel.canceled -= onEmoteKeyWheelCanceled;
                keybinds.MiddleFinger.Disable();
                keybinds.Griddy.Disable();
                keybinds.Shy.Disable();
                keybinds.Clap.Disable();
                keybinds.Salute.Disable();
                keybinds.Prisyadka.Disable();
                keybinds.Sign.Disable();
                keybinds.Twerk.Disable();
                keybinds.EmoteWheel.Disable();
            }
        }

        public static void onEmoteKeyWheelStarted(InputAction.CallbackContext context)
        {
            Plugin.Debug("onEmoteKeyWheelStarted()");
            if (!emoteWheelIsOpened && !GameValues.localPlayerController.isPlayerDead && !GameValues.localPlayerController.inTerminalMenu && !GameValues.localPlayerController.quickMenuManager.isMenuOpen)
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
                CheckEmoteInput(context, enabledList[selectionWheel.selectedEmoteID], selectionWheel.selectedEmoteID, GameValues.localPlayerController);
            }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            emoteWheelIsOpened = false;
            selectionWheel.gameObject.SetActive(emoteWheelIsOpened);
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

        public static void onEmoteKeyPrisyadka(InputAction.CallbackContext context)
        {
            Plugin.Debug("PlayerControllerB.onEmoteKeyPrisyadka()");
            onEmoteKeyPerformed(context, Emote.Prisyadka);
        }

        public static void onEmoteKeySign(InputAction.CallbackContext context)
        {
            Plugin.Debug("PlayerControllerB.onEmoteKeySign()");
            onEmoteKeyPerformed(context, Emote.Sign);
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
        [HarmonyPrefix]
        private static void UpdatePrefix(PlayerControllerB __instance)
        {
            // Plugin.Debug("PlayerControllerB.UpdatePrefix()");
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
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(PlayerControllerB __instance)
        {
            // Plugin.Debug("PlayerControllerB.UpdatePostfix()");
            if (!__instance.isPlayerControlled || !__instance.IsOwner)
            {
                __instance.playerBodyAnimator.runtimeAnimatorController = others;
                turnControllerIntoAnOverrideController(__instance.playerBodyAnimator.runtimeAnimatorController);
            }
            else
            {
                if (__instance.playerBodyAnimator != local)
                {
                    if (isPlayerFirstFrame)
                    {
                        SpawnLegs(__instance);
                    }
                    __instance.playerBodyAnimator.runtimeAnimatorController = local;
                    if (isPlayerFirstFrame)
                    {
                        OnFirstLocalPlayerFrameWithNewAnimator(__instance);
                    }
                    if (isPlayerSpawning)
                    {
                        __instance.SpawnPlayerAnimation();
                        isPlayerSpawning = false;
                    }
                }
                currentEmoteID = __instance.playerBodyAnimator.GetInteger("emoteNumber");
                if (!incompatibleStuff)
                {
                    __instance.movementSpeed = movSpeed;
                    if (__instance.CheckConditionsForEmote() && __instance.performingEmote)
                    {
                        if (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Griddy))
                        {
                            __instance.movementSpeed = movSpeed * griddySpeed;
                        }
                        else if (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Prisyadka))
                        {
                            __instance.movementSpeed = movSpeed * prisyadkaSpeed;
                        }
                    }
                }
                __instance.localArmsRotationTarget = isLocalArmsSeparatedFromCamera ? freeArmsTarget : lockedArmsTarget;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SpawnPlayerAnimation")]
        [HarmonyPrefix]
        private static void OnLocalPlayerSpawn(PlayerControllerB __instance)
        {
            Plugin.Debug("PlayerControllerB.OnLocalPlayerSpawn()");
            if (__instance.IsOwner && __instance.isPlayerControlled)
            {
                isPlayerSpawning = true;
            }
        }

        private static void CheckEmoteInput(InputAction.CallbackContext context, bool enabled, int emoteID, PlayerControllerB player)
        {
            Plugin.Debug($"CheckEmoteInput({enabled}, {emoteID})");
            if (enabled)
            {
                player.PerformEmote(context, emoteID);
            }
        }

        private static void OnFirstLocalPlayerFrameWithNewAnimator(PlayerControllerB player)
        {
            Plugin.Debug("OnFirstLocalPlayerFrameWithNewAnimator()");
            isPlayerFirstFrame = false;
            turnControllerIntoAnOverrideController(player.playerBodyAnimator.runtimeAnimatorController);
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
            Plugin.Debug("SpawnSign()");
            GameObject gameObject = UnityEngine.Object.Instantiate(SignPrefab, player.transform.Find("ScavengerModel").transform.Find("metarig").transform);
            gameObject.transform.SetSiblingIndex(6);
            gameObject.name = "Sign";
            gameObject.transform.localPosition = new Vector3(0.029f, -0.45f, 1.3217f);
            gameObject.transform.localRotation = Quaternion.Euler(65.556f, 180f, 180f);
        }

        private static void SpawnLegs(PlayerControllerB player)
        {
            Plugin.Debug("SpawnLegs()");
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
            Plugin.Debug("ResetIKWeights()");
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

        private static bool CheckIfTooManyEmotesIsPlaying(PlayerControllerB player)
        {
            Plugin.Debug("CheckIfTooManyEmotesIsPlaying()");
            Animator playerBodyAnimator = player.playerBodyAnimator;
            return playerBodyAnimator.GetCurrentAnimatorStateInfo(1).IsName("Dance1") && player.performingEmote && GetAnimatorEmoteClipName(playerBodyAnimator) != "Dance1";
        }

        private static string GetAnimatorEmoteClipName(Animator animator)
        {
            Plugin.Debug("GetAnimatorEmoteClipName()");
            return animator.GetCurrentAnimatorClipInfo(1)[0].clip.name;
        }

        private static void UpdateLegsMaterial(PlayerControllerB player)
        {
            Plugin.Debug("UpdateLegsMaterial()");
            legsMesh.GetComponent<SkinnedMeshRenderer>().material = player.playerBodyAnimator.transform.parent.transform.Find("LOD1").gameObject.GetComponent<SkinnedMeshRenderer>().material;
        }

        private static void TogglePlayerBadges(bool enabled)
        {
            Plugin.Debug("TogglePlayerBadges()");
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

        private static void turnControllerIntoAnOverrideController(RuntimeAnimatorController controller)
        {
            // Plugin.Debug("turnControllerIntoAnOverrideController()");
            if (controller is not AnimatorOverrideController)
            {
                controller = new AnimatorOverrideController(controller);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "PerformEmote")]
        [HarmonyPrefix]
        private static bool PerformEmotePrefix(ref InputAction.CallbackContext context, int emoteID, PlayerControllerB __instance)
        {
            Plugin.Debug($"PerformEmotePrefix({emoteID})");
            int localEmoteID = emoteID;
            if (CheckIfTooManyEmotesIsPlaying(__instance) && localEmoteID > EmoteDefs.getEmoteNumber(Emote.Point))
            {
                return false;
            }
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || (__instance.IsServer && !__instance.isHostPlayerObject)) && !__instance.isTestingPlayer)
            {
                return false;
            }
            if (customSignInputField.IsSignUIOpen && localEmoteID == EmoteDefs.getEmoteNumber(DoubleEmote.Double_Sign))
            {
                return false;
            }
            if (localEmoteID > 0 && localEmoteID <= EmoteDefs.getEmoteNumber(Emote.Dance) && !emoteWheelIsOpened && !context.performed)
            {
                return false;
            }
            foreach (string name in Enum.GetNames(typeof(DoubleEmote)))
            {
                int num = EmoteDefs.getEmoteNumber(EmoteDefs.getDoubleEmote(name));
                bool invCheck = false;
                if (localEmoteID == num && currentEmoteID == localEmoteID && __instance.performingEmote && (!__instance.isHoldingObject || !invCheck))
                {
                    localEmoteID += 1000;
                }
            }
            if ((localEmoteID != currentEmoteID && localEmoteID <= EmoteDefs.getEmoteNumber(Emote.Dance)) || !__instance.performingEmote)
            {
                ResetIKWeights(__instance);
            }
            if (__instance.CheckConditionsForEmote())
            {
                if (__instance.timeSinceStartingEmote >= emoteCooldown)
                {
                    currentEmoteID = emoteID;
                    Action action = delegate ()
                    {
                        __instance.timeSinceStartingEmote = 0f;
                        __instance.playerBodyAnimator.SetInteger("emoteNumber", localEmoteID);
                        __instance.performingEmote = true;
                        __instance.StartPerformingEmoteServerRpc();
                        syncAnimator.UpdateEmoteIDForOthers(emoteID);
                        TogglePlayerBadges(false);
                    };
                    if (localEmoteID == EmoteDefs.getEmoteNumber(Emote.Prisyadka))
                    {
                        action = (Action)Delegate.Combine(action, new Action(delegate ()
                        {
                            UpdateLegsMaterial(__instance);
                        }));
                    } else if (localEmoteID == EmoteDefs.getEmoteNumber(Emote.Sign))
                    {
                        action = (Action)Delegate.Combine(action, new Action(delegate ()
                        {
                            customSignInputField.gameObject.SetActive(true);
                        }));
                    }
                    action();
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "CheckConditionsForEmote")]
        [HarmonyPrefix]
        private static bool prefixCheckConditions(ref bool __result, PlayerControllerB __instance)
        {
            // Plugin.Debug("prefixCheckConditions()");
            if ((currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Griddy) && griddySpeed != 0) || (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Prisyadka) && prisyadkaSpeed != 0))
            {
                __result = !__instance.inSpecialInteractAnimation && !__instance.isPlayerDead && !__instance.isJumping && __instance.moveInputVector.x == 0f && !__instance.isSprinting && !__instance.isCrouching && !__instance.isClimbingLadder && !__instance.isGrabbingObjectAnimation && !__instance.inTerminalMenu && !__instance.isTypingChat;
                return false;
            } else if (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Sign) || currentEmoteID == EmoteDefs.getEmoteNumber(DoubleEmote.Double_Sign))
            {
                __result = !__instance.inSpecialInteractAnimation && !!__instance.isPlayerDead && !__instance.isJumping && !__instance.isWalking && !__instance.isSprinting && !__instance.isCrouching && !__instance.isClimbingLadder && !__instance.isGrabbingObjectAnimation && !__instance.inTerminalMenu;
                return false;
            }
            return true;
        }
    }
}
