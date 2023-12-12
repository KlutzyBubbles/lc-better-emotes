using System;
using BepInEx.Bootstrap;
using BepInEx;
using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace BetterEmote
{
    internal class EmotePatch
    {
        public static AssetBundle animationsBundle;

        public static AssetBundle animatorBundle;

        public static bool enable3;
        public static bool enable4;
        public static bool enable5;
        public static bool enable6;

        public static string keyBind_Emote3;
        public static string keyBind_Emote4;
        public static string keyBind_Emote5;
        public static string keyBind_Emote6;

        private static InputAction.CallbackContext context;

        public static RuntimeAnimatorController local;

        public static RuntimeAnimatorController others;

        private static int currentEmoteID;

        private static float movSpeed;

        public static bool incompatibleStuff;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            GameObject gameObject = __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject;
            CustomAudioAnimationEvent customAudioAnimationEvent = gameObject.AddComponent<CustomAudioAnimationEvent>();
            customAudioAnimationEvent.player = __instance;
            movSpeed = __instance.movementSpeed;
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
                    __instance.movementSpeed = (conditions && currentEmoteID == 4 && __instance.performingEmote) ? (movSpeed / 2f) : movSpeed;
                }
                CheckEmoteInput(keyBind_Emote3, enable3, 3, __instance);
                CheckEmoteInput(keyBind_Emote4, enable4, 4, __instance);
                CheckEmoteInput(keyBind_Emote5, enable5, 5, __instance);
                CheckEmoteInput(keyBind_Emote6, enable6, 6, __instance);
            }
        }

        private static void CheckEmoteInput(string keyBind, bool enabled, int emoteID, PlayerControllerB player)
        {
            if (Keyboard.current[keyBind].IsPressed(0f) && enabled && (!player.performingEmote || currentEmoteID != emoteID))
            {
                player.PerformEmote(context, emoteID);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "PerformEmote")]
        [HarmonyPrefix]
        private static void PerformEmotePrefix(ref InputAction.CallbackContext context, int emoteID, PlayerControllerB __instance)
        {
            currentEmoteID = emoteID;
            if (emoteID < 3 && !context.performed)
            {
                return;
            }
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || (__instance.IsServer && !__instance.isHostPlayerObject)) && !__instance.isTestingPlayer)
            {
                return;
            }
            bool? conditionsOpt = Traverse.Create(__instance).Method("CheckConditionsForEmote").GetValue() as bool?;
            bool conditions = conditionsOpt ?? false;
            if (conditions)
            {
                if (__instance.timeSinceStartingEmote >= 0.5f)
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
            if (currentEmoteID == 4)
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
