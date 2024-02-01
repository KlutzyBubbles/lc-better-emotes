using GameNetcodeStuff;
using System;
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
            if (EmotePatch.customSignInputField != null)
            {
                EmotePatch.customSignInputField.Player = player;
            }
            try
            {
                freeArmsTarget = UnityEngine.Object.Instantiate(player.localArmsRotationTarget, player.localArmsRotationTarget.parent.parent);
                lockedArmsTarget = player.localArmsRotationTarget;
                Transform transform = player.transform.Find("ScavengerModel").Find("metarig").Find("spine").Find("spine.001").Find("spine.002").Find("spine.003");
                LevelBadge = transform.Find("LevelSticker").gameObject;
                BetaBadge = transform.Find("BetaBadge").gameObject;
            }
            catch (Exception e)
            {
                Plugin.Debug($"Unable to init first frame objects: {e.Message}");
            }
        }

        public static void SpawnSign(PlayerControllerB player)
        {
            Plugin.Debug("SpawnSign()");
            try
            {
                GameObject signObject = UnityEngine.Object.Instantiate(SignPrefab, player.transform.Find("ScavengerModel").transform.Find("metarig").transform);
                if (signObject != null)
                {
                    signObject.name = "Sign";
                    signObject.transform.SetSiblingIndex(6);
                    signObject.transform.localPosition = new Vector3(0.029f, -0.45f, 1.3217f);
                    signObject.transform.localRotation = Quaternion.Euler(65.556f, 180f, 180f);
                }
            }
            catch (Exception e)
            {
                Plugin.Debug($"Unable to spawn sign: {e.Message}");
            }
        }

        public static void SpawnLegs(PlayerControllerB player)
        {
            Plugin.Debug("SpawnLegs()");
            try
            {
                GameObject legsObject = UnityEngine.Object.Instantiate(LegsPrefab, player.playerBodyAnimator.transform.parent.transform);
                legsMesh = legsObject.transform.Find("Mesh");
                legsMesh.name = "LEGS";
                legsMesh.transform.parent = player.playerBodyAnimator?.transform?.parent;
                GameObject legsArmature = legsObject.transform?.Find("Armature")?.gameObject;
                if (legsArmature != null)
                {
                    legsArmature.name = "FistPersonLegs";
                    legsArmature.transform.parent = player.playerBodyAnimator?.transform;
                    legsArmature.transform.position = new Vector3(0f, 0.197f, 0f);
                    legsArmature.transform.localScale = new Vector3(13.99568f, 13.99568f, 13.99568f);
                }
                UnityEngine.Object.Destroy(legsObject);
            }
            catch (Exception e)
            {
                Plugin.Debug($"Unable to spawn legs: {e.Message}");
            }
        }
        public static void UpdateLegsMaterial(PlayerControllerB player)
        {
            Plugin.Debug("UpdateLegsMaterial()");
            if (legsMesh == null)
                return;
            SkinnedMeshRenderer renderer = legsMesh.GetComponent<SkinnedMeshRenderer>();
            if (renderer == null)
                return;
            Material newMaterial = player.playerBodyAnimator?.transform?.parent?.transform?.Find("LOD1")?.gameObject?.GetComponent<SkinnedMeshRenderer>()?.material;
            if (newMaterial == null)
                return;
            renderer.material = newMaterial;
        }

        public static bool CheckIfTooManyEmotesIsPlaying(PlayerControllerB player)
        {
            Plugin.Debug("CheckIfTooManyEmotesIsPlaying()");
            Animator playerBodyAnimator = player.playerBodyAnimator;
            if (playerBodyAnimator != null)
            {
                try
                {
                    return playerBodyAnimator.GetCurrentAnimatorStateInfo(1).IsName("Dance1") && player.performingEmote && GetAnimatorEmoteClipName(playerBodyAnimator) != "Dance1";
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        private static string GetAnimatorEmoteClipName(Animator animator)
        {
            Plugin.Debug("GetAnimatorEmoteClipName()");
            try
            {
                return animator.GetCurrentAnimatorClipInfo(1)[0].clip.name;
            }
            catch (Exception)
            {
                return "";
            }
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
