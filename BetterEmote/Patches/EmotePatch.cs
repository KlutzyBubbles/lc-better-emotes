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
        public static AssetBundle animationsBundle;

        public static AssetBundle animatorBundle;

        public static RuntimeAnimatorController local;

        public static RuntimeAnimatorController others;

        private static int currentEmoteID;

        private static float movSpeed;

        public static bool[] playersPerformingEmotes = new bool[40];

        public static GameObject LegsPrefab;
        public static GameObject SignPrefab;
        public static GameObject SignUIPrefab;
        private static GameObject localPlayerLevelBadge;
        private static GameObject localPlayerBetaBadge;

        public static SignUI customSignInputField;

        private static SyncAnimatorToOthers syncAnimator;
        public static SyncVRState syncVR;

        private static bool isPlayerFirstFrame;
        private static bool isPlayerSpawning;

        public static bool isLocalArmsSeparatedFromCamera;

        private static Transform freeArmsTarget;
        private static Transform lockedArmsTarget;
        private static Transform legsMesh;

        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        private static void AwakePost(RoundManager __instance)
        {
            Plugin.Debug("AwakePost()");
            Settings.debugAllSettings();
            EmoteWheel.emoteNames = new string[EmoteDefs.getEmoteCount() + 1];
            foreach (string name in Enum.GetNames(typeof(Emote)))
            {
                EmoteWheel.emoteNames[EmoteDefs.getEmoteNumber(name) - 1] = name;
            }
            if (!Settings.disableSelfEmote)
            {
                GameObject gameObject = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
                customSignInputField = UnityEngine.Object.Instantiate(SignUIPrefab, gameObject.transform).AddComponent<SignUI>();
            }
            isPlayerFirstFrame = true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("EmotePatch.StartPostfix()");
            GameObject gameObject = __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject;
            CustomAudioAnimationEvent customAudioAnimationEvent = gameObject.AddComponent<CustomAudioAnimationEvent>();
            customAudioAnimationEvent.player = __instance;
            movSpeed = __instance.movementSpeed;
            __instance.gameObject.AddComponent<CustomAnimationObjects>();
            SpawnSign(__instance);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void ConnectClientToPlayerObjectPostfix(PlayerControllerB __instance)
        {
            Plugin.Debug("EmotePatch.ConnectClientToPlayerObjectPostfix()");
            if (syncVR != null)
            {
                syncVR.RequestVRStateFromOthers();
                syncVR.UpdateVRStateForOthers(Settings.disableSelfEmote);
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        private static void UpdatePrefix(PlayerControllerB __instance)
        {
            Plugin.Trace("PlayerControllerB.UpdatePrefix()");
            checked
            {
                if (__instance.performingEmote)
                {
                    playersPerformingEmotes[__instance.playerClientId] = true;
                } else if (playersPerformingEmotes[__instance.playerClientId])
                {
                    playersPerformingEmotes[__instance.playerClientId] = false;
                    ResetIKWeights(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(PlayerControllerB __instance)
        {
            Plugin.Trace("PlayerControllerB.UpdatePostfix()");
            if (!__instance.isPlayerControlled || !__instance.IsOwner)
            {
                if (syncVR == null || !syncVR.vrPlayers.Contains(__instance.playerClientId))
                {
                    Plugin.Trace("SyncVRState doesnt contian");
                    __instance.playerBodyAnimator.runtimeAnimatorController = others;
                    turnControllerIntoAnOverrideController(__instance.playerBodyAnimator.runtimeAnimatorController);
                }
            }
            else
            {
                if (__instance.playerBodyAnimator != local)
                {
                    if (isPlayerFirstFrame && !Settings.disableSelfEmote)
                    {
                        SpawnLegs(__instance);
                    }
                    if (!Settings.disableSelfEmote)
                    {
                        __instance.playerBodyAnimator.runtimeAnimatorController = local;
                    }
                    if (isPlayerFirstFrame)
                    {
                        Plugin.Debug("isPlayerFirstFrame");
                        syncVR = __instance.GetComponent<SyncVRState>();
                        syncAnimator = __instance.GetComponent<SyncAnimatorToOthers>();
                        isPlayerFirstFrame = false;
                        if (!Settings.disableSelfEmote)
                        {
                            OnFirstLocalPlayerFrameWithNewAnimator(__instance);
                        }
                        if (syncVR != null)
                        {
                            syncVR.RequestVRStateFromOthers();
                            syncVR.UpdateVRStateForOthers(Settings.disableSelfEmote);
                        }
                        Plugin.Debug("SpawnPlayerAnimation");
                        __instance.SpawnPlayerAnimation();
                    }
                    if (isPlayerSpawning)
                    {
                        __instance.SpawnPlayerAnimation();
                        isPlayerSpawning = false;
                    }
                }
                if (!Settings.disableSpeedChange)
                {
                    __instance.movementSpeed = movSpeed;
                    if (__instance.CheckConditionsForEmote() && __instance.performingEmote)
                    {
                        if (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Griddy))
                        {
                            __instance.movementSpeed = movSpeed * Settings.griddySpeed;
                        }
                        else if (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Prisyadka))
                        {
                            __instance.movementSpeed = movSpeed * Settings.prisyadkaSpeed;
                        }
                    }
                }
                if (!Settings.disableSelfEmote)
                {
                    __instance.localArmsRotationTarget = isLocalArmsSeparatedFromCamera ? freeArmsTarget : lockedArmsTarget;
                }
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

        private static void OnFirstLocalPlayerFrameWithNewAnimator(PlayerControllerB player)
        {
            Plugin.Debug("OnFirstLocalPlayerFrameWithNewAnimator()");
            turnControllerIntoAnOverrideController(player.playerBodyAnimator.runtimeAnimatorController);
            customSignInputField.Player = player;
            freeArmsTarget = UnityEngine.Object.Instantiate(player.localArmsRotationTarget, player.localArmsRotationTarget.parent.parent);
            lockedArmsTarget = player.localArmsRotationTarget;
            Transform transform = player.transform.Find("ScavengerModel").Find("metarig").Find("spine").Find("spine.001").Find("spine.002").Find("spine.003");
            localPlayerLevelBadge = transform.Find("LevelSticker").gameObject;
            localPlayerBetaBadge = transform.Find("BetaBadge").gameObject;
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
            component3.weight = 1f;
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
            Plugin.Debug($"TogglePlayerBadges({enabled})");
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
                if (Settings.disableSelfEmote)
                {
                    Plugin.Debug("Couldn't find the level badge (its fine for the settings)");
                }
                else
                {
                    Plugin.StaticLogger.LogError("Couldn't find the level badge");
                }
            }
        }

        private static void turnControllerIntoAnOverrideController(RuntimeAnimatorController controller)
        {
            Plugin.Trace("turnControllerIntoAnOverrideController()");
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
                Plugin.Debug($"Is custom emote with too many emotes currently playing");
                return false;
            }
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || (__instance.IsServer && !__instance.isHostPlayerObject)) && !__instance.isTestingPlayer)
            {
                Plugin.Debug($"Is player controllered or owner check failed");
                return false;
            }
            if (syncVR != null)
            {
                Plugin.Debug($"syncVR not null, updating");
                syncVR.RequestVRStateFromOthers();
                syncVR.UpdateVRStateForOthers(Settings.disableSelfEmote);
            }
            if (customSignInputField != null && customSignInputField.IsSignUIOpen && localEmoteID != EmoteDefs.getEmoteNumber(AltEmote.Sign_Text))
            {
                Plugin.Debug($"Sign UI is open, is this a sign?");
                return false;
            }
            if (localEmoteID > 0 && localEmoteID <= EmoteDefs.getEmoteNumber(Emote.Point) && !EmoteKeybindPatch.emoteWheelIsOpened && !context.performed)
            {
                Plugin.Debug($"Normal emote with no emote wheel or context performed");
                return false;
            }
            foreach (string name in Enum.GetNames(typeof(DoubleEmote)))
            {
                Plugin.Debug($"Checking double emote {name}");
                int num = EmoteDefs.getEmoteNumber(EmoteDefs.getEmote(name));
                bool invCheck = false;
                if (currentEmoteID == localEmoteID && localEmoteID >= EmoteDefs.getEmoteNumber(Emote.Point) && __instance.performingEmote && (!__instance.isHoldingObject || !invCheck))
                {
                    Plugin.Debug($"Damn, emote ids match with all other checks");
                    if (localEmoteID == num)
                    {
                        Plugin.Debug($"Adding offset");
                        localEmoteID += 1000;
                    } else if (localEmoteID > 1000)
                    {
                        Plugin.Debug($"Removing offset");
                        localEmoteID -= 1000;
                    }
                }
            }
            Plugin.Debug($"localEmoteID after: {localEmoteID}");
            if ((localEmoteID != currentEmoteID && localEmoteID <= EmoteDefs.getEmoteNumber(Emote.Point)) || !__instance.performingEmote)
            {
                Plugin.Debug($"Ressetting IKWeights because its not a custom emote");
                ResetIKWeights(__instance);
            }
            Plugin.Trace($"__instance.inSpecialInteractAnimation: {__instance.inSpecialInteractAnimation}");
            Plugin.Trace($"__instance.isPlayerDead: {__instance.isPlayerDead}");
            Plugin.Trace($"__instance.isJumping: {__instance.isJumping}");
            Plugin.Trace($"__instance.moveInputVector.x: {__instance.moveInputVector.x}");
            Plugin.Trace($"__instance.isWalking: {__instance.isWalking}");
            Plugin.Trace($"__instance.isSprinting: {__instance.isSprinting}");
            Plugin.Trace($"__instance.isCrouching: {__instance.isCrouching}");
            Plugin.Trace($"__instance.isClimbingLadder: {__instance.isClimbingLadder}");
            Plugin.Trace($"__instance.isGrabbingObjectAnimation: {__instance.isGrabbingObjectAnimation}");
            Plugin.Trace($"__instance.inTerminalMenu: {__instance.inTerminalMenu}");
            Plugin.Trace($"__instance.isTypingChat: {__instance.isTypingChat}");
            int previousEmote = currentEmoteID;
            currentEmoteID = localEmoteID;
            if (__instance.CheckConditionsForEmote())
            {
                Plugin.Debug($"Check conditions passed");
                if (__instance.timeSinceStartingEmote >= Settings.emoteCooldown)
                {
                    Plugin.Debug($"Time elapsed since last emote cooldown");
                    Action action = delegate ()
                    {
                        Plugin.Debug($"Starting emote {localEmoteID}");
                        __instance.timeSinceStartingEmote = 0f;
                        __instance.playerBodyAnimator.SetInteger("emoteNumber", localEmoteID);
                        __instance.performingEmote = true;
                        __instance.StartPerformingEmoteServerRpc();
                        syncAnimator.UpdateEmoteIDForOthers(localEmoteID);
                        TogglePlayerBadges(false);
                    };
                    if (localEmoteID == EmoteDefs.getEmoteNumber(Emote.Prisyadka) && !Settings.disableSelfEmote)
                    {
                        Plugin.Debug($"Adding UpdateLegsMaterial for Prisyadka");
                        action = (Action)Delegate.Combine(action, new Action(delegate ()
                        {
                            UpdateLegsMaterial(__instance);
                        }));
                    } else if (localEmoteID == EmoteDefs.getEmoteNumber(Emote.Sign) && !Settings.disableSelfEmote)
                    {
                        Plugin.Debug($"Adding customSignInputField setActive for Sign");
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
                    currentEmoteID = previousEmote;
                    Plugin.Debug($"Emote cooldown still in effect, try again soon");
                    return false;
                }
            }
            else
            {
                currentEmoteID = previousEmote;
                Plugin.Debug($"Check confitions failed");
                return false;
            }
        }

        
        [HarmonyPatch(typeof(PlayerControllerB), "CheckConditionsForEmote")]
        [HarmonyPostfix]
        private static void postfixCheckConditions(ref bool __result, PlayerControllerB __instance)
        {
            Plugin.Trace($"prefixCheckConditions({currentEmoteID})");

            if (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Griddy) || currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Prisyadka))
            {
                __result = !__instance.inSpecialInteractAnimation && !__instance.isPlayerDead && !__instance.isJumping && __instance.moveInputVector.x == 0f && !__instance.isSprinting && !__instance.isCrouching && !__instance.isClimbingLadder && !__instance.isGrabbingObjectAnimation && !__instance.inTerminalMenu && !__instance.isTypingChat;
            }
            else
            {
                if (currentEmoteID == EmoteDefs.getEmoteNumber(Emote.Sign) || currentEmoteID == EmoteDefs.getEmoteNumber(AltEmote.Sign_Text))
                {
                    __result = !__instance.inSpecialInteractAnimation && !__instance.isPlayerDead && !__instance.isJumping && !__instance.isWalking && !__instance.isCrouching && !__instance.isClimbingLadder && !__instance.isGrabbingObjectAnimation && !__instance.inTerminalMenu;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "StopPerformingEmoteServerRpc")]
        [HarmonyPostfix]
        private static void StopPerformingEmoteServerPrefix(PlayerControllerB __instance)
        {
            Plugin.Debug("prefixCheckConditions()");
            if (__instance.IsOwner && __instance.isPlayerControlled)
            {
                __instance.playerBodyAnimator.SetInteger("emoteNumber", 0);
            }
            TogglePlayerBadges(true);
            syncAnimator.UpdateEmoteIDForOthers(0);
            currentEmoteID = 0;
        }
    }
}
