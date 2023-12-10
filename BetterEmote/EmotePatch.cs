using System;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterEmote
{
    internal class EmotePatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            GameObject gameObject = __instance.gameObject.transform.Find("ScavengerModel").transform.Find("metarig").gameObject;
            CustomAudioAnimationEvent customAudioAnimationEvent = gameObject.AddComponent<CustomAudioAnimationEvent>();
            customAudioAnimationEvent.player = __instance;
            if (_clap1 == null)
            {
                _clap1 = animationsBundle.LoadAsset<AudioClip>("Assets/MoreEmotes/SingleClapEmote1.wav");
            }
            if (_clap2 == null)
            {
                _clap2 = animationsBundle.LoadAsset<AudioClip>("Assets/MoreEmotes/SingleClapEmote2.wav");
            }
            customAudioAnimationEvent.clap1 = _clap1;
            customAudioAnimationEvent.clap2 = _clap2;
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
                if (Keyboard.current[keyBind_Emote3].IsPressed(0f) && !keyFlag_Emote3 && enable3)
                {
                    keyFlag_Emote3 = true;
                    __instance.PerformEmote(context, 3);
                }
                else
                {
                    if (!Keyboard.current[keyBind_Emote3].IsPressed(0f))
                    {
                        keyFlag_Emote3 = false;
                    }
                }
                if (Keyboard.current[keyBind_Emote4].IsPressed(0f) && !keyFlag_Emote4 && enable4)
                {
                    keyFlag_Emote4 = true;
                    __instance.PerformEmote(context, 4);
                }
                else
                {
                    if (!Keyboard.current[keyBind_Emote4].IsPressed(0f))
                    {
                        keyFlag_Emote4 = false;
                    }
                }
                if (Keyboard.current[keyBind_Emote5].IsPressed(0f) && !keyFlag_Emote5 && enable5)
                {
                    keyFlag_Emote5 = true;
                    __instance.PerformEmote(context, 5);
                }
                else
                {
                    if (!Keyboard.current[keyBind_Emote5].IsPressed(0f))
                    {
                        keyFlag_Emote5 = false;
                    }
                }
                if (Keyboard.current[keyBind_Emote6].IsPressed(0f) && !keyFlag_Emote6 && enable6)
                {
                    keyFlag_Emote6 = true;
                    __instance.PerformEmote(context, 6);
                }
                else
                {
                    if (!Keyboard.current[keyBind_Emote6].IsPressed(0f))
                    {
                        keyFlag_Emote6 = false;
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(PlayerControllerB), "PerformEmote")]
        [HarmonyPrefix]
        private static void PerformEmotePrefix(ref InputAction.CallbackContext context, int emoteID, PlayerControllerB __instance)
        {
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

        public static AssetBundle animationsBundle;

        public static AssetBundle animatorBundle;

        private static bool keyFlag_Emote3;

        public static bool enable3;

        private static bool keyFlag_Emote4;

        public static bool enable4;

        private static bool keyFlag_Emote5;

        public static bool enable5;

        private static bool keyFlag_Emote6;

        public static bool enable6;

        public static string keyBind_Emote3;

        public static string keyBind_Emote4;

        public static string keyBind_Emote5;

        public static string keyBind_Emote6;

        private static InputAction.CallbackContext context;

        public static RuntimeAnimatorController local;

        public static RuntimeAnimatorController others;

        private static AudioClip _clap1;

        private static AudioClip _clap2;
    }
}
