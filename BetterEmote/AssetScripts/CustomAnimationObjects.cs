using BetterEmote.Utils;
using GameNetcodeStuff;
using System;
using UnityEngine;

namespace BetterEmote.AssetScripts
{
    public class CustomAnimationObjects : MonoBehaviour
    {
        private PlayerControllerB playerInstance;
        private MeshRenderer sign;
        private GameObject signText;
        private SkinnedMeshRenderer legs;

        private void Start()
        {
            Plugin.Debug("CustomAnimationObjects.Start()");
            playerInstance = GetComponent<PlayerControllerB>();
        }

        private void Update()
        {
            Plugin.Trace("CustomAnimationObjects.Update()");
            if (sign == null || signText == null)
            {
                FindSign();
            }
            else
            {
                try
                {
                    sign.transform.localPosition = sign.transform.parent.Find("spine").localPosition;
                    if (playerInstance == null)
                        return;
                    if (legs == null && playerInstance.IsOwner && playerInstance.isPlayerControlled && !Settings.DisableModelOverride)
                    {
                        FindLegs();
                    }
                    else
                    {
                        DisableEverything();
                        if (playerInstance.performingEmote)
                        {
                            int emoteNumber = playerInstance.playerBodyAnimator?.GetInteger("emoteNumber") ?? 0;
                            if (emoteNumber == EmoteDefs.getEmoteNumber(Emote.Prisyadka))
                            {
                                if (legs != null)
                                {
                                    legs.enabled = true;
                                }
                                if (playerInstance.IsOwner && !Settings.DisableModelOverride)
                                {
                                    LocalPlayer.IsArmsSeparatedFromCamera = true;
                                }
                            }
                            else if (sign != null && (emoteNumber == EmoteDefs.getEmoteNumber(Emote.Sign) || emoteNumber == EmoteDefs.getEmoteNumber(AltEmote.Sign_Text)))
                            {
                                sign.enabled = true;
                                if (signText != null && !signText.activeSelf)
                                {
                                    Plugin.Trace("Sign isnt active self");
                                    signText.SetActive(true);
                                }
                                if (playerInstance.IsOwner && !Settings.DisableModelOverride)
                                {
                                    LocalPlayer.IsArmsSeparatedFromCamera = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Debug($"Custom animation update failed {e.Message}");
                }
            }
        }

        private void DisableEverything()
        {
            Plugin.Trace("DisableEverything()");
            if (legs != null)
            {
                legs.enabled = false;
            }
            sign.enabled = false;
            if (signText.activeSelf)
            {
                signText.SetActive(false);
            }
            if (playerInstance != null && playerInstance.IsOwner && playerInstance.isPlayerControlled && !Settings.DisableModelOverride)
            {
                LocalPlayer.IsArmsSeparatedFromCamera = false;
            }
        }

        private void FindSign()
        {
            Plugin.Debug("FindSign()");
            if (sign == null && playerInstance != null)
            {
                Plugin.Debug("Sign is null and player exists");
                sign = playerInstance.transform.Find("ScavengerModel").Find("metarig").Find("Sign").GetComponent<MeshRenderer>();
            }
            if (signText == null && sign != null)
            {
                Plugin.Debug("Sign text is null and sign exists");
                signText = sign.transform.Find("Text").gameObject;
            }
        }

        private void FindLegs()
        {
            Plugin.Debug("FindLegs()");
            if (legs == null && playerInstance != null)
            {
                Plugin.Debug("Legs are null and player exists");
                try
                {
                    legs = playerInstance.transform.Find("ScavengerModel").Find("LEGS").GetComponent<SkinnedMeshRenderer>();
                }
                catch (Exception)
                {
                    Plugin.Logger.LogWarning("Unable to find custom legs, this should be corrected soon.");
                }
            }
        }
    }
}
