using BetterEmote.Patches;
using BetterEmote.Utils;
using GameNetcodeStuff;
using System;
using UnityEngine;

namespace BetterEmote.AssetScripts
{
    public class CustomAnimationObjects : MonoBehaviour
    {
        private PlayerControllerB _player;
        private MeshRenderer _sign;
        private GameObject _signText;
        private SkinnedMeshRenderer _legs;

        private void Start()
        {
            Plugin.Debug("CustomAnimationObjects.Start()");
            _player = GetComponent<PlayerControllerB>();
        }

        private void Update()
        {
            Plugin.Trace("CustomAnimationObjects.Update()");
            if (_sign == null || _signText == null)
            {
                FindSign();
            }
            else
            {
                _sign.transform.localPosition = _sign.transform.parent.Find("spine").localPosition;
                if (_legs == null && _player.IsOwner && _player.isPlayerControlled && !Settings.disableModelOverride)
                {
                    FindLegs();
                }
                else
                {
                    DisableEverything();
                    if (_player.performingEmote)
                    {
                        int emoteNumber = _player.playerBodyAnimator.GetInteger("emoteNumber");
                        if (emoteNumber == EmoteDefs.getEmoteNumber(Emote.Prisyadka))
                        {
                            if (_legs != null)
                            {
                                _legs.enabled = true;
                            }
                            if (_player.IsOwner && !Settings.disableModelOverride)
                            {
                                EmotePatch.isLocalArmsSeparatedFromCamera = true;
                            }
                        }
                        else if (emoteNumber == EmoteDefs.getEmoteNumber(Emote.Sign) || emoteNumber == EmoteDefs.getEmoteNumber(AltEmote.Sign_Text))
                        {
                            _sign.enabled = true;
                            if (!_signText.activeSelf)
                            {
                                Plugin.Trace("Sign isnt active self");
                                _signText.SetActive(true);
                            }
                            if (_player.IsOwner && !Settings.disableModelOverride)
                            {
                                EmotePatch.isLocalArmsSeparatedFromCamera = true;
                            }
                        }
                    }
                }
            }
        }

        private void DisableEverything()
        {
            Plugin.Trace("DisableEverything()");
            if (_legs != null)
            {
                _legs.enabled = false;
            }
            _sign.enabled = false;
            if (_signText.activeSelf)
            {
                _signText.SetActive(false);
            }
            if (_player.IsOwner && _player.isPlayerControlled && !Settings.disableModelOverride)
            {
                EmotePatch.isLocalArmsSeparatedFromCamera = false;
            }
        }

        private void FindSign()
        {
            Plugin.Debug("FindSign()");
            if (_sign == null && _player != null)
            {
                Plugin.Debug("Sign is null and player exists");
                _sign = _player.transform.Find("ScavengerModel").Find("metarig").Find("Sign").GetComponent<MeshRenderer>();
            }
            if (_signText == null && _sign != null)
            {
                Plugin.Debug("Sign text is null and sign exists");
                _signText = _sign.transform.Find("Text").gameObject;
            }
        }

        private void FindLegs()
        {
            Plugin.Debug("FindLegs()");
            if (_legs == null && _player != null)
            {
                Plugin.Debug("Legs are null and player exists");
                try
                {
                    _legs = _player.transform.Find("ScavengerModel").Find("LEGS").GetComponent<SkinnedMeshRenderer>();
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogWarning("Unable to find custom legs, this should be corrected soon.");
                }
            }
        }
    }
}
