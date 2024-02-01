using BetterEmote.AssetScripts;
using BetterEmote.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static SignUI customSignInputField;

        private static SyncAnimatorToOthers syncAnimator;
        public static SyncVRState syncVR;

        private static bool isPlayerFirstFrame;
        private static bool isPlayerSpawning;

        public static bool isLocalArmsSeparatedFromCamera;

        public static Dictionary<ulong, bool> vrPlayers = new Dictionary<ulong, bool>();

        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        private static void AwakePost()
        {
            Plugin.Debug("AwakePost()");
            Settings.debugAllSettings();
            if (!Settings.disableModelOverride)
            {
                GameObject gameObject = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
                customSignInputField = UnityEngine.Object.Instantiate(LocalPlayer.SignUIPrefab, gameObject.transform).AddComponent<SignUI>();
            }
            isPlayerFirstFrame = true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            if (__instance == null)
                return;
            Plugin.Debug("EmotePatch.StartPostfix()");
            GameObject gameObject = __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject;
            CustomAudioAnimationEvent customAudioAnimationEvent = gameObject.AddComponent<CustomAudioAnimationEvent>();
            customAudioAnimationEvent.player = __instance;
            movSpeed = __instance.movementSpeed;
            __instance.gameObject.AddComponent<CustomAnimationObjects>();
            LocalPlayer.SpawnSign(__instance);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void ConnectClientToPlayerObjectPostfix(PlayerControllerB __instance)
        {
            if (__instance == null)
                return;
            Plugin.Debug("EmotePatch.ConnectClientToPlayerObjectPostfix()");
            if (syncVR != null)
            {
                syncVR.RequestVRStateFromOthers();
                syncVR.UpdateVRStateForOthers(Settings.disableModelOverride);
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Low)]
        private static void UpdatePrefix(PlayerControllerB __instance)
        {
            if (__instance == null)
                return;
            Plugin.Trace("PlayerControllerB.UpdatePrefix()");
            checked
            {
                if (__instance.performingEmote)
                {
                    playersPerformingEmotes[__instance.playerClientId] = true;
                }
                else if (playersPerformingEmotes[__instance.playerClientId])
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
            if (__instance == null)
                return;
            Plugin.Trace("PlayerControllerB.UpdatePostfix()");
            if (!__instance.isPlayerControlled || !__instance.IsOwner)
            {
                if (syncVR != null)
                {
                    if (vrPlayers.ContainsKey(__instance.playerClientId) && !vrPlayers[__instance.playerClientId])
                    {
                        __instance.playerBodyAnimator.runtimeAnimatorController = others;
                    }
                }
            }
            else
            {
                if (__instance.playerBodyAnimator != local)
                {
                    if (isPlayerFirstFrame && !Settings.disableModelOverride)
                    {
                        LocalPlayer.SpawnLegs(__instance);
                    }
                    if (!Settings.disableModelOverride)
                    {
                        __instance.playerBodyAnimator.runtimeAnimatorController = local;
                    }
                    if (isPlayerFirstFrame)
                    {
                        Plugin.Debug("isPlayerFirstFrame");
                        syncVR = __instance.GetComponent<SyncVRState>();
                        syncAnimator = __instance.GetComponent<SyncAnimatorToOthers>();
                        isPlayerFirstFrame = false;
                        if (!Settings.disableModelOverride)
                        {
                            LocalPlayer.OnFirstLocalPlayerFrameWithNewAnimator(__instance);
                        }
                        if (syncVR != null)
                        {
                            syncVR.RequestVRStateFromOthers();
                            syncVR.UpdateVRStateForOthers(Settings.disableModelOverride);
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
                if (!Settings.disableModelOverride)
                {
                    __instance.localArmsRotationTarget = isLocalArmsSeparatedFromCamera ? LocalPlayer.freeArmsTarget : LocalPlayer.lockedArmsTarget;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SpawnPlayerAnimation")]
        [HarmonyPrefix]
        private static void OnLocalPlayerSpawn(PlayerControllerB __instance)
        {
            if (__instance == null)
                return;
            Plugin.Debug("PlayerControllerB.OnLocalPlayerSpawn()");
            if (__instance.IsOwner && __instance.isPlayerControlled)
            {
                isPlayerSpawning = true;
            }
        }

        private static void ResetIKWeights(PlayerControllerB player)
        {
            Plugin.Debug("ResetIKWeights()");
            Transform transform = player?.playerBodyAnimator?.transform?.Find("Rig 1");
            if (transform != null)
            {
                try
                {
                    string[] armNames = ["RightArm", "LeftArm"];
                    string[] legNames = ["RightLeg", "LeftLeg"];
                    armNames.ToList().ForEach(name => transform.Find(name).GetComponent<ChainIKConstraint>().weight = 1f);
                    legNames.ToList().ForEach(name => transform.Find(name).GetComponent<TwoBoneIKConstraint>().weight = 1f);
                    Transform transform2 = player.playerBodyAnimator.transform.Find("ScavengerModelArmsOnly").Find("metarig").Find("spine.003").Find("RigArms");
                    armNames.ToList().ForEach(name => transform2.Find(name).GetComponent<ChainIKConstraint>().weight = 1f);
                }
                catch (NullReferenceException)
                {
                    Plugin.Logger.LogWarning("Unable to reset IK weights, if this is spammed please report it");
                }
            }
            else
            {
                Plugin.Debug("ResetIKWeights transform is null");
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "PerformEmote")]
        [HarmonyPrefix]
        private static bool PerformEmotePrefix(ref InputAction.CallbackContext context, int emoteID, PlayerControllerB __instance)
        {
            if (__instance == null)
                return true;
            Plugin.Debug($"PerformEmotePrefix({emoteID})");
            int localEmoteID = emoteID;
            if (LocalPlayer.CheckIfTooManyEmotesIsPlaying(__instance) && localEmoteID > EmoteDefs.getEmoteNumber(Emote.Point))
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
                syncVR.UpdateVRStateForOthers(Settings.disableModelOverride);
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
                        LocalPlayer.TogglePlayerBadges(false);
                    };
                    if (localEmoteID == EmoteDefs.getEmoteNumber(Emote.Prisyadka) && !Settings.disableModelOverride)
                    {
                        Plugin.Debug($"Adding UpdateLegsMaterial for Prisyadka");
                        action = (Action)Delegate.Combine(action, new Action(delegate ()
                        {
                            LocalPlayer.UpdateLegsMaterial(__instance);
                        }));
                    } else if (localEmoteID == EmoteDefs.getEmoteNumber(Emote.Sign) && !Settings.disableModelOverride)
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
            if (__instance == null)
                return;
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
            if (__instance == null)
                return;
            Plugin.Debug("StopPerformingEmoteServerPrefix()");
            if (__instance.IsOwner && __instance.isPlayerControlled)
            {
                __instance.playerBodyAnimator.SetInteger("emoteNumber", 0);
                syncAnimator.UpdateEmoteIDForOthers(0);
                currentEmoteID = 0;
            }
            LocalPlayer.TogglePlayerBadges(true);
        }
    }
}
