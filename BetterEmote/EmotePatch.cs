using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterEmote
{
    internal class EmotePatch
    {
        public static Keybinds keybinds = new Keybinds();

        public static AssetBundle animationsBundle;

        public static AssetBundle animatorBundle;

        public static bool enableMiddlefinger = true;
        public static bool enableGriddy = true;
        public static bool enableShy = true;
        public static bool enableClap = true;
        public static bool enableSalute = true;
        public static bool enableTwerk = true;

        public static float griddySpeed = 0.5f;
        public static float emoteCooldown = 0.5f;

        private static int middlefingerID = 3;
        private static int griddyID = 4;
        private static int shyID = 5;
        private static int clapID = 6;
        private static int saluteID = 8;
        private static int twerkID = 7;

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
            keybinds.MiddleFinger.performed += delegate
            {
                CheckEmoteInput(enableMiddlefinger, middlefingerID, __instance);
            };
            keybinds.Griddy.performed += delegate
            {
                CheckEmoteInput(enableGriddy, griddyID, __instance);
            };
            keybinds.Shy.performed += delegate
            {
                CheckEmoteInput(enableShy, shyID, __instance);
            };
            keybinds.Clap.performed += delegate
            {
                CheckEmoteInput(enableClap, clapID, __instance);
            };
            keybinds.Salute.performed += delegate
            {
                CheckEmoteInput(enableSalute, saluteID, __instance);
            };
            keybinds.Twerk.performed += delegate
            {
                CheckEmoteInput(enableTwerk, twerkID, __instance);
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
                    __instance.movementSpeed = (conditions && currentEmoteID == griddyID && __instance.performingEmote) ? (movSpeed * (griddySpeed)) : movSpeed;
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
            if (currentEmoteID == griddyID)
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
