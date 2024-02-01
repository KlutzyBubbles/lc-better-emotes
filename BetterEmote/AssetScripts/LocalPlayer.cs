using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Animations.Rigging;
using UnityEngine;
using BetterEmote.Utils;
using BetterEmote.Patches;

namespace BetterEmote.AssetScripts
{
    internal class LocalPlayer
    {

        public static GameObject LegsPrefab;
        public static GameObject SignPrefab;
        public static GameObject SignUIPrefab;
        private static GameObject LevelBadge;
        private static GameObject BetaBadge;

        public static Transform freeArmsTarget;
        public static Transform lockedArmsTarget;
        private static Transform legsMesh;

        public static void OnFirstLocalPlayerFrameWithNewAnimator(PlayerControllerB player)
        {
            Plugin.Debug("OnFirstLocalPlayerFrameWithNewAnimator()");
            EmotePatch.customSignInputField.Player = player;
            freeArmsTarget = UnityEngine.Object.Instantiate(player.localArmsRotationTarget, player.localArmsRotationTarget.parent.parent);
            lockedArmsTarget = player.localArmsRotationTarget;
            Transform transform = player.transform.Find("ScavengerModel").Find("metarig").Find("spine").Find("spine.001").Find("spine.002").Find("spine.003");
            LevelBadge = transform.Find("LevelSticker").gameObject;
            BetaBadge = transform.Find("BetaBadge").gameObject;
        }

        public static void SpawnSign(PlayerControllerB player)
        {
            Plugin.Debug("SpawnSign()");
            GameObject gameObject = UnityEngine.Object.Instantiate(SignPrefab, player.transform.Find("ScavengerModel").transform.Find("metarig").transform);
            gameObject.transform.SetSiblingIndex(6);
            gameObject.name = "Sign";
            gameObject.transform.localPosition = new Vector3(0.029f, -0.45f, 1.3217f);
            gameObject.transform.localRotation = Quaternion.Euler(65.556f, 180f, 180f);
        }

        public static void SpawnLegs(PlayerControllerB player)
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
        public static void UpdateLegsMaterial(PlayerControllerB player)
        {
            Plugin.Debug("UpdateLegsMaterial()");
            legsMesh.GetComponent<SkinnedMeshRenderer>().material = player.playerBodyAnimator.transform.parent.transform.Find("LOD1").gameObject.GetComponent<SkinnedMeshRenderer>().material;
        }

        public static bool CheckIfTooManyEmotesIsPlaying(PlayerControllerB player)
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

        public static void TogglePlayerBadges(bool enabled)
        {
            Plugin.Debug($"TogglePlayerBadges({enabled})");
            if (BetaBadge != null)
            {
                BetaBadge.GetComponent<MeshRenderer>().enabled = enabled;
            }
            if (LevelBadge != null)
            {
                LevelBadge.GetComponent<MeshRenderer>().enabled = enabled;
            }
            else
            {
                if (Settings.disableModelOverride)
                {
                    Plugin.Debug("Couldn't find the level badge (its fine for the settings)");
                }
                else
                {
                    Plugin.Logger.LogError("Couldn't find the level badge");
                }
            }
        }
    }
}
