using BetterEmote.Patches;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // Plugin.Debug("CustomAnimationObjects.Update()");
            if (_sign == null || _signText == null)
            {
                FindSign();
            }
            else
            {
                _sign.transform.localPosition = _sign.transform.parent.Find("spine").localPosition;
                if (_legs == null && _player.IsOwner && _player.isPlayerControlled)
                {
                    FindLegs();
                }
                else
                {
                    DisableEverything();
                    if (_player.performingEmote)
                    {
                        int emoteNumber = _player.playerBodyAnimator.GetInteger("emoteNumber");
                        if (emoteNumber != -10)
                        {
                            if (emoteNumber == 9)
                            {
                                if (_legs != null)
                                {
                                    _legs.enabled = true;
                                }
                                if (_player.IsOwner)
                                {
                                    EmotePatch.isLocalArmsSeparatedFromCamera = true;
                                }
                                return;
                            }
                            if (emoteNumber != 10)
                            {
                                return;
                            }
                        }
                        _sign.enabled = true;
                        if (!_signText.activeSelf)
                        {
                            _signText.SetActive(true);
                        }
                        if (_player.IsOwner)
                        {
                            EmotePatch.isLocalArmsSeparatedFromCamera = true;
                        }
                    }
                }
            }
        }

        private void DisableEverything()
        {
            // Plugin.Debug("DisableEverything()");
            if (_legs != null)
            {
                _legs.enabled = false;
            }
            _sign.enabled = false;
            if (_signText.activeSelf)
            {
                _signText.SetActive(false);
            }
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                EmotePatch.isLocalArmsSeparatedFromCamera = false;
            }
        }

        private void FindSign()
        {
            Plugin.Debug("FindSign()");
            _sign = _player.transform.Find("ScavengerModel").Find("metarig").Find("Sign").GetComponent<MeshRenderer>();
            _signText = _sign.transform.Find("Text").gameObject;
        }

        private void FindLegs()
        {
            Plugin.Debug("FindLegs()");
            Transform transform = _player.transform;
            Plugin.Debug("transform");
            Transform scavenger = transform.Find("ScavengerModel");
            Plugin.Debug("ScavengerModel");
            Transform legs = scavenger.Find("LEGS");
            Transform legs2 = scavenger.Find("Legs");
            Transform legs3 = scavenger.Find("legs");
            Plugin.Debug("LEGS");
            Plugin.Debug($"{legs == null}");
            Plugin.Debug($"{legs2 == null}");
            Plugin.Debug($"{legs3 == null}");
            _legs = legs.GetComponent<SkinnedMeshRenderer>();
            Plugin.Debug("SkinnedMeshRenderer");
            // _legs = _player.transform.Find("ScavengerModel").Find("LEGS").GetComponent<SkinnedMeshRenderer>();
        }
    }
}
